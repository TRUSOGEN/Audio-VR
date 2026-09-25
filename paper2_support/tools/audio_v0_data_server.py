"""Serve the local Audio V0 log viewer and its Unity technical-demo logs.

The server exposes only files under a locally supplied AudioV0 data root.
It has no upload endpoint and accepts no participant data from the browser.
"""

from __future__ import annotations

import argparse
import json
import re
import sys
import threading
import uuid
from collections import OrderedDict
from http import HTTPStatus
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
from urllib.parse import parse_qs, urlparse

from audio_v0_review import build_review, cleaned_rows, csv_bytes, make_figure


TOOL_ROOT = Path(__file__).resolve().parent
DATA_ROOT = Path.home() / "AudioV0_Data" / "technical_demo"
COLLECTION_ROOT = DATA_ROOT.parent
CATEGORIES = ("technical_demo", "pilot", "participant_run")
FIGURE_LOCK = threading.Lock()
SNAPSHOT_LOCK = threading.Lock()
SNAPSHOTS: OrderedDict[str, dict[str, object]] = OrderedDict()


class AudioV0RequestHandler(SimpleHTTPRequestHandler):
    """Serve the static viewer plus small read-only health and log endpoints."""

    def __init__(self, *args: object, **kwargs: object) -> None:
        super().__init__(*args, directory=str(TOOL_ROOT), **kwargs)

    def do_GET(self) -> None:  # noqa: N802
        """Route read-only JSON endpoints before falling back to static files."""
        parsed = urlparse(self.path)
        path = parsed.path
        if path == "/api/health":
            self._send_json({"ok": True, "data_root_exists": DATA_ROOT.is_dir(),
                             "data_root": str(DATA_ROOT), "viewer_version": "research_review_20260923",
                             "collection_root": str(COLLECTION_ROOT)})
            return
        if path == "/api/index":
            self._send_json({"sessions": self._session_index()})
            return
        if path.startswith("/api/review"):
            self._review_response(path, parse_qs(parsed.query))
            return
        if path == "/api/sessions":
            self._send_json({"logs": self._read_logs()})
            return
        if path == "/":
            self.path = "/audio_v0_log_viewer.html"
        super().do_GET()

    def _session_index(self) -> list[dict[str, object]]:
        """以元数据和末尾日志建立会话索引，不将不同证据类别混合。"""
        sessions = []
        for category in CATEGORIES:
            for path in (COLLECTION_ROOT / category).glob("session_*/event_log.jsonl"):
                manifest, issues = {}, []
                manifest_path = path.parent / "session_manifest.json"
                if manifest_path.exists():
                    try:
                        manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
                        if not isinstance(manifest, dict):
                            raise ValueError("manifest is not an object")
                    except (OSError, ValueError) as error:
                        manifest = {}
                        issues.append(str(error))
                try:
                    with path.open("rb") as stream:
                        stream.seek(max(0, path.stat().st_size - 65536))
                        tail = stream.read().decode("utf-8", errors="replace")
                    sessions.append({"session_id": path.parent.name, "category": category,
                                     "participant_id": manifest.get("participant_id", "unknown"),
                                     "created_utc": manifest.get("created_utc", ""),
                                     "four_blocks_complete": bool(re.search(r'"event_type"\s*:\s*"experiment_sequence_complete"', tail)),
                                     "closed": bool(re.search(r'"event_type"\s*:\s*"session_end"', tail)),
                                     "bytes": path.stat().st_size, "metadata_issues": issues})
                except OSError as error:
                    sessions.append({"session_id": path.parent.name, "category": category, "read_error": str(error)})
        return sorted(sessions, key=lambda item: str(item["session_id"]), reverse=True)

    def _review_response(self, path: str, query: dict[str, list[str]]) -> None:
        """只读审阅固定集合内的单个日志；CSV 和图件共享筛选及聚合模型。"""
        category = query.get("category", ["technical_demo"])[0]
        session = query.get("session", [""])[0]
        if category not in CATEGORIES or not re.fullmatch(r"session_[A-Za-z0-9_-]+", session):
            self.send_error(HTTPStatus.BAD_REQUEST, "Invalid session or category")
            return
        log_path = COLLECTION_ROOT / category / session / "event_log.jsonl"
        if not log_path.is_file() or not log_path.resolve().is_relative_to(COLLECTION_ROOT.resolve()):
            self.send_error(HTTPStatus.NOT_FOUND, "Session log not found")
            return
        filters = {name: query.get(name, [""])[0] for name in ("condition", "block", "route")}
        try:
            snapshot = query.get("snapshot", [""])[0]
            if path != "/api/review" and snapshot:
                with SNAPSHOT_LOCK:
                    review = SNAPSHOTS.get(snapshot)
                if review is None:
                    self._send_json({"error": "Snapshot expired. Reload the recording before exporting."}, HTTPStatus.CONFLICT)
                    return
                if review["session_id"] != session or review["category"] != category or review["filters"] != filters:
                    self._send_json({"error": "Snapshot does not match the selected filters."}, HTTPStatus.CONFLICT)
                    return
            else:
                review = build_review(log_path, category, filters)
            if path == "/api/review":
                snapshot = uuid.uuid4().hex
                review["snapshot"] = snapshot
                with SNAPSHOT_LOCK:
                    SNAPSHOTS[snapshot] = review
                    while len(SNAPSHOTS) > 24:
                        SNAPSHOTS.popitem(last=False)
                self._send_json(review)
                return
            if path == "/api/review.csv":
                table = query.get("table", ["summary"])[0]
                if table == "markers":
                    rows = [{**event, "metric": "notification_markers", "storage_category": category,
                             "evidence_label": review["evidence_label"], "export_eligibility": review["export_eligibility"]}
                            for event in review["timeline"] if event.get("event_type") == "notification_onset"]
                    fields = list(dict.fromkeys(field for row in rows for field in row)) or [
                        "session_id", "event_id", "event_type", "monotonic_clock_ms", "condition_id", "storage_category", "evidence_label"]
                    self._send_bytes(csv_bytes(rows, fields), "text/csv; charset=utf-8", f"{session}_notification_markers.csv")
                    return
                if table in {"cleaned", "excluded"}:
                    panel = query.get("panel", ["accuracy"])[0]
                    rows = cleaned_rows(review, panel, excluded=table == "excluded")
                    reference = rows or cleaned_rows(review, panel, excluded=table != "excluded")
                    fields = list(dict.fromkeys(field for row in reference for field in row)) or [
                        "session_id", "event_id", "metric", "included_in_metric", "exclusion_reason", "storage_category", "evidence_label"]
                    self._send_bytes(csv_bytes(rows, fields), "text/csv; charset=utf-8", f"{session}_{panel}_{table}.csv")
                    return
                if table not in {"summary", "trials", "visual", "motion", "events"}:
                    self.send_error(HTTPStatus.BAD_REQUEST, "Unknown table")
                    return
                self._send_bytes(csv_bytes(review[table]), "text/csv; charset=utf-8", f"{session}_{table}.csv")
                return
            if path in {"/api/review.svg", "/api/review.png", "/api/review.pdf"}:
                kind = query.get("kind", ["conditions"])[0]
                if kind not in {"conditions", "motion", "accuracy", "rt", "confidence", "visual", "trajectory", "altitude"}:
                    self.send_error(HTTPStatus.BAD_REQUEST, "Unknown figure")
                    return
                format = path.rsplit(".", 1)[1]
                with FIGURE_LOCK:
                    figure = make_figure(review, kind, format)
                content_type = {"svg": "image/svg+xml", "png": "image/png", "pdf": "application/pdf"}[format]
                self._send_bytes(figure, content_type,
                                 f"{session}_{kind}.{format}", download=query.get("download") == ["1"])
                return
            self.send_error(HTTPStatus.NOT_FOUND)
        except (OSError, ValueError, KeyError, TypeError) as error:
            self._send_json({"error": type(error).__name__ + ": " + str(error)}, HTTPStatus.UNPROCESSABLE_ENTITY)

    def _send_bytes(self, body: bytes, content_type: str, filename: str, download: bool = True) -> None:
        """返回下载内容或嵌入图件，避免缓存陈旧会话。"""
        self.send_response(HTTPStatus.OK)
        self.send_header("Content-Type", content_type)
        self.send_header("Content-Length", str(len(body)))
        self.send_header("Cache-Control", "no-store")
        if download:
            self.send_header("Content-Disposition", f'attachment; filename="{filename}"')
        self.end_headers()
        self.wfile.write(body)

    def _read_logs(self) -> list[dict[str, str]]:
        """Return UTF-8 event logs without changing the original files."""
        if not DATA_ROOT.is_dir():
            return []
        logs: list[dict[str, str]] = []
        for log_path in sorted(DATA_ROOT.glob("session_*/event_log.jsonl")):
            try:
                logs.append(
                    {
                        "name": log_path.name,
                        "relative_path": str(log_path.relative_to(DATA_ROOT)),
                        "text": log_path.read_text(encoding="utf-8"),
                    }
                )
            except OSError:
                continue
        return logs

    def _send_json(self, payload: object, status: HTTPStatus = HTTPStatus.OK) -> None:
        """Write a JSON response with an explicit UTF-8 content type."""
        body = json.dumps(payload, ensure_ascii=False, allow_nan=False).encode("utf-8")
        self.send_response(status)
        self.send_header("Content-Type", "application/json; charset=utf-8")
        self.send_header("Content-Length", str(len(body)))
        self.send_header("Cache-Control", "no-store")
        self.end_headers()
        self.wfile.write(body)

    def log_message(self, format: str, *args: object) -> None:
        """Keep routine local requests out of the launcher terminal."""


def main() -> None:
    """Start the read-only local server on the requested loopback port."""
    global DATA_ROOT, COLLECTION_ROOT
    parser = argparse.ArgumentParser(description="Serve Audio V0 local logs.")
    parser.add_argument("--port", type=int, default=8091)
    parser.add_argument("--data-root", type=Path,
                        help="AudioV0 root containing technical_demo, pilot and participant_run folders")
    options = parser.parse_args()
    if options.data_root is None and sys.platform != "darwin":
        parser.error("--data-root is required outside macOS; use the path shown by Unity")
    COLLECTION_ROOT = (options.data_root or Path.home() / "AudioV0_Data").expanduser().resolve()
    if not COLLECTION_ROOT.is_dir():
        parser.error(f"data root does not exist: {COLLECTION_ROOT}")
    DATA_ROOT = COLLECTION_ROOT / "technical_demo"
    server = ThreadingHTTPServer(("127.0.0.1", options.port), AudioV0RequestHandler)
    print(f"Audio V0 local viewer: http://127.0.0.1:{options.port}", flush=True)
    server.serve_forever()


if __name__ == "__main__":
    main()
