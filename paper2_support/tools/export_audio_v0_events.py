#!/usr/bin/env python3
"""Validate and normalise Audio V0 event logs for pilot or participant-run analysis.

The script never treats technical-demo runs as participant evidence unless an
explicit QA-only flag is supplied. It reads immutable JSONL logs, writes
normalised CSV tables and a validation report, and does not fit models or
manufacture missing values.
"""

from __future__ import annotations

import argparse
import csv
import json
import math
import sys
from collections import Counter, defaultdict
from pathlib import Path
from typing import Any, Iterable


FORMAL_CATEGORIES = {"pilot", "participant_run"}
FORMAL_RUN_RECORD_FIELDS = (
    "run_id",
    "storage_category",
    "participant_id",
    "protocol_version",
    "stimulus_set_version",
    "noise_version",
    "route_version",
    "task_version",
    "apparatus_version",
    "calibration_record_id",
    "counterbalance_sequence",
    "deviation_record",
)
MANIFEST_RUN_RECORD_FIELDS = {
    "protocol_version": "protocol_version",
    "task_version": "task_version",
    "apparatus_version": "apparatus_version",
    "calibration_record_id": "calibration_record_id",
    "counterbalance_sequence": "counterbalance_sequence",
}
STATIC_EVENT_RUN_RECORD_FIELD_MAP = {
    "protocol_version": "protocol_version",
    "task_version": "task_version",
    "apparatus_version": "apparatus_version",
    "calibration_record_id": "calibration_record_id",
    "counterbalance_sequence": "counterbalance_sequence",
}
DYNAMIC_EVENT_VERSION_FIELDS = ("stimulus_set_version", "noise_version", "route_version")
EVENT_COLUMNS = [
    "session_id", "attempt_id", "participant_id", "block_id", "block_position",
    "route_id", "route_version", "condition_id", "notification_design",
    "listening_condition", "transition_id", "excerpt_id", "transition_target", "event_type",
    "event_sequence", "timestamp_ms", "monotonic_clock_ms", "route_time_ms",
    "monotonic_time_s", "stimulus_set_version", "noise_version",
    "control_binding_version", "application_version", "protocol_version",
    "task_version", "apparatus_version", "calibration_record_id",
    "counterbalance_sequence", "event_id", "export_category", "export_eligibility", "details",
]
NOTIFICATION_COLUMNS = [
    "session_id", "attempt_id", "participant_id", "block_id", "block_position",
    "route_id", "condition_id", "transition_id", "excerpt_id", "transition_target", "validation_mode", "technically_valid",
    "notification_design", "listening_condition", "stimulus_set_version",
    "notification_onset_ms", "transition_onset_ms", "transition_route_time_ms", "software_lead_s",
    "clip", "playback_observed", "response_status", "selected_target",
    "communicated_target", "correct", "response_time_ms", "response_time_s",
    "timeout", "confidence", "confidence_status", "confidence_response_time_ms", "data_quality_flag", "export_eligibility",
]
TRACKING_COLUMNS = [
    "session_id", "attempt_id", "participant_id", "block_id", "block_position",
    "route_id", "condition_id", "route_time_ms", "task_elapsed_s", "target_x",
    "target_y", "cursor_x", "cursor_y", "tracking_error", "inside_target",
    "trajectory_id", "difficulty_version",
]
VISUAL_COLUMNS = [
    "session_id", "attempt_id", "participant_id", "block_id", "condition_id",
    "task_version", "stimulus_id", "icon_id", "is_target", "onset_ms", "offset_ms",
    "onset_task_s", "offset_task_s", "target_task_s", "response_window_s",
    "hit_window_start", "hit_window_end", "schedule_seed", "sequence", "task_run",
    "response_time_ms", "timing_error_ms", "outcome", "control_id", "scorable",
    "response_count", "early_count", "late_count", "repeated_count",
    "onset_event_id", "offset_event_id", "scored_response_event_id",
    "data_quality_flag", "export_eligibility",
]
VISUAL_RESPONSE_COLUMNS = [
    "session_id", "attempt_id", "participant_id", "block_id", "condition_id",
    "event_id", "event_sequence", "monotonic_clock_ms", "task_version", "task_run",
    "stimulus_id", "icon_id", "is_target", "task_elapsed_s", "response_index",
    "response_time_ms", "timing_error_ms", "stimulus_progress", "outcome", "accepted",
    "control_id", "export_eligibility",
]
MOTION_COLUMNS = [
    "session_id", "attempt_id", "block_id", "condition_id", "route_id", "route_version",
    "route_time_ms", "motion_version", "x_m", "y_m", "z_m", "vx_mps", "vy_mps", "vz_mps",
    "progress", "frame_dt_s", "time_scale", "planned_duration_s", "export_eligibility",
    # 末尾追加独立片段字段，保持旧连续路线列的顺序与含义。
    "event_type", "event_id", "event_sequence", "monotonic_clock_ms", "excerpt_id",
    "excerpt_elapsed_s", "heading_deg", "boundary_opacity", "pre_transition", "map_visible",
]


def visual_response_rows(events: list[dict[str, Any]]) -> list[dict[str, Any]]:
    """逐次导出视觉点击，保留早/晚、空窗、重复和已接受响应，避免只留下第一次点击。"""
    rows = []
    for event in events:
        if event.get("event_type") != "visual_response":
            continue
        row = {column: event.get(column, "") for column in VISUAL_RESPONSE_COLUMNS}
        row.update(parse_details(str(event.get("details", ""))))
        rows.append(row)
    return rows


def validate_visual_timing(onset: dict[str, str], offset: dict[str, str], responses: list[dict[str, str]]) -> list[str]:
    """用 active-task 时钟独立复算节奏判定，不能只信客户端 progress 或 RT。"""
    flags: list[str] = []
    try:
        start = float(onset["task_elapsed_s"])
        duration = float(onset["response_window_s"])
        window_start = float(onset["hit_window_start"])
        window_end = float(onset["hit_window_end"])
        end = float(offset["task_elapsed_s"]) if offset else None
        values = [start, duration, window_start, window_end] + ([] if end is None else [end])
        if not all(math.isfinite(value) for value in values) or duration <= 0 or not 0 <= window_start < window_end <= 1:
            raise ValueError("invalid timing configuration")
    except (KeyError, ValueError):
        return ["missing_or_invalid_task_timing"]
    if end is not None and end < start:
        flags.append("offset_before_onset")
    if offset.get("outcome") == "interrupted" and onset.get("is_target", "").lower() == "true" and end is not None and end > start + duration * window_end + 0.001:
        flags.append("interrupted_after_scoring_deadline")
    for response in responses:
        try:
            response_at = float(response["task_elapsed_s"])
            rt = float(response["response_time_ms"])
            progress = float(response["stimulus_progress"])
            timing_error = float(response["timing_error_ms"])
            if not all(math.isfinite(value) for value in (response_at, rt, progress, timing_error)):
                raise ValueError("nonfinite response")
        except (KeyError, ValueError):
            flags.append("missing_or_invalid_response_timing")
            continue
        elapsed = response_at - start
        computed_progress = elapsed / duration
        if elapsed < -0.001 or (end is not None and response_at > end + 0.001):
            flags.append("response_outside_stimulus_lifetime")
        # task_elapsed_s 精度 0.1 ms；容差覆盖两个端点的舍入，不覆盖整帧错误。
        if abs(rt - elapsed * 1000) > 1:
            flags.append("response_time_clock_mismatch")
        if abs(progress - computed_progress) > 0.001:
            flags.append("progress_clock_mismatch")
        if abs(timing_error - (elapsed - duration * (window_start + window_end) / 2) * 1000) > 1:
            flags.append("timing_error_clock_mismatch")
        if response.get("outcome") == "hit" and not window_start - 0.0001 <= computed_progress <= window_end + 0.0001:
            flags.append("hit_outside_clock_window")
    return list(dict.fromkeys(flags))


def visual_rows(events: list[dict[str, Any]]) -> list[dict[str, Any]]:
    """每个刺激保留最终结局；新契约允许早点击重试，旧日志仍保留原始异常标记。"""
    groups: dict[tuple[str, str, str, str], list[dict[str, Any]]] = defaultdict(list)
    rows: list[dict[str, Any]] = []
    for event in events:
        if event.get("event_type") not in {"visual_stimulus_onset", "visual_stimulus_offset", "visual_response"}:
            continue
        detail = parse_details(str(event.get("details", "")))
        stimulus = detail.get("stimulus_id", "")
        if not stimulus:
            row = {column: event.get(column, "") for column in VISUAL_COLUMNS}
            row.update(detail)
            row["scorable"] = "False"
            row["data_quality_flag"] = "response_without_stimulus"
            rows.append(row)
            continue
        key = tuple(str(event.get(field, "")) for field in ("session_id", "attempt_id", "block_id")) + (stimulus,)
        groups[key].append(event)
    for group in groups.values():
        onsets = [e for e in group if e["event_type"] == "visual_stimulus_onset"]
        offsets = [e for e in group if e["event_type"] == "visual_stimulus_offset"]
        responses = [e for e in group if e["event_type"] == "visual_response"]
        reference = (onsets or group)[0]
        row = {column: reference.get(column, "") for column in VISUAL_COLUMNS}
        onset = parse_details(str(reference.get("details", "")))
        row.update(onset)
        offset = parse_details(str(offsets[0].get("details", ""))) if offsets else {}
        response_details = [parse_details(str(e.get("details", ""))) for e in responses]
        scored = [e for e, d in zip(responses, response_details)
                  if d.get("accepted", "").lower() == "true" or
                  ("accepted" not in d and d.get("outcome") in {"hit", "false_alarm", "miss", "correct_rejection"})]
        scored_detail = parse_details(str(scored[0].get("details", ""))) if scored else {}
        row.update(onset_ms=onsets[0].get("monotonic_clock_ms", "") if onsets else "",
                   offset_ms=offsets[0].get("monotonic_clock_ms", "") if offsets else "",
                   onset_task_s=onset.get("task_elapsed_s", ""), offset_task_s=offset.get("task_elapsed_s", ""),
                   onset_event_id=onsets[0].get("event_id", "") if onsets else "",
                   offset_event_id=offsets[0].get("event_id", "") if offsets else "",
                   scored_response_event_id=scored[0].get("event_id", "") if scored else "")
        for field in ("response_time_ms", "timing_error_ms", "control_id"):
            row[field] = scored_detail.get(field, "")
        row["outcome"] = offset.get("outcome", "")
        if row["outcome"] in {"", "already_scored"}:
            row["outcome"] = scored_detail.get("outcome", row["outcome"])
        row["scorable"] = offset.get("scorable", "True" if offsets and row["outcome"] in
                                    {"hit", "miss", "false_alarm", "correct_rejection"} else "False")
        row["response_count"] = len(responses)
        counts = Counter(d.get("outcome", "") for d in response_details)
        for field, outcome in (("early_count", "early"), ("late_count", "late"), ("repeated_count", "repeated")):
            row[field] = counts[outcome]
        flags = []
        if len(onsets) != 1:
            flags.append("onset_count_" + str(len(onsets)))
        if len(offsets) != 1:
            flags.append("offset_count_" + str(len(offsets)))
        if len(scored) > 1 or (len(responses) > 1 and all("accepted" not in d for d in response_details)):
            flags.append("duplicate_response")
        if offset.get("response_count", str(len(responses))) != str(len(responses)):
            flags.append("response_count_mismatch")
        if offset.get("outcome") in {"hit", "false_alarm"} and not scored:
            flags.append("missing_scored_response")
        if scored and offset.get("outcome") not in {None, "", "already_scored", scored_detail.get("outcome")}:
            flags.append("response_offset_outcome_mismatch")
        if row["outcome"] == "hit" and "hit_window_start" in onset:
            try:
                progress = float(scored_detail["stimulus_progress"])
                if not float(onset["hit_window_start"]) <= progress <= float(onset["hit_window_end"]):
                    flags.append("hit_outside_timing_window")
            except (KeyError, ValueError):
                flags.append("missing_or_invalid_hit_progress")
        if row.get("task_version") == "rhythm_timing_go_nogo_candidate_v2":
            flags.extend(validate_visual_timing(onset, offset, response_details))
        if flags or not onsets or not offsets:
            row["scorable"] = "False"
        row["data_quality_flag"] = ";".join(dict.fromkeys(flags))
        rows.append(row)
    return rows


def visual_summary_checks(events: list[dict[str, Any]], trials: list[dict[str, Any]]) -> list[dict[str, Any]]:
    """对新节奏契约核对 block 汇总与逐刺激/逐点击计数，保留完整可追踪差异。"""
    checks = []
    responses = visual_response_rows(events)
    for end in events:
        if end.get("event_type") != "visual_task_ended":
            continue
        detail = parse_details(str(end.get("details", "")))
        if detail.get("task_version") != "rhythm_timing_go_nogo_candidate_v2":
            continue
        identity = {field: str(end.get(field, "")) for field in ("session_id", "attempt_id", "block_id")}
        identity["task_run"] = detail.get("task_run", "")
        def belongs(row: dict[str, Any]) -> bool:
            """避免同段重启或另一个 attempt 的计数混入当前 run。"""
            return all(str(row.get(field, "")) == value for field, value in identity.items())
        run_trials = [row for row in trials if row.get("stimulus_id") and belongs(row)]
        run_responses = [row for row in responses if belongs(row)]
        outcomes = Counter(row.get("outcome") for row in run_trials)
        attempts = Counter(row.get("outcome") for row in run_responses)
        expected = {
            "stimuli": len(run_trials),
            "scheduled_targets": sum(str(row.get("is_target", "")).lower() == "true" for row in run_trials),
            "hits": outcomes["hit"], "misses": outcomes["miss"],
            "false_alarms": outcomes["false_alarm"], "correct_rejections": outcomes["correct_rejection"],
            "interrupted": outcomes["interrupted"],
            "interrupted_targets": sum(row.get("outcome") == "interrupted" and
                str(row.get("is_target", "")).lower() == "true" for row in run_trials),
            "early_responses": attempts["early"], "late_responses": attempts["late"],
            "stray_responses": attempts["stray"], "repeated_responses": attempts["repeated"],
        }
        mismatches = [field + ":logged=" + detail.get(field, "missing") + ",reconstructed=" + str(count)
                      for field, count in expected.items() if detail.get(field) != str(count)]
        checks.append({**identity, "passed": not mismatches, "mismatches": mismatches,
                       "source_event_id": end.get("event_id", ""), "reconstructed_counts": expected})
    return checks


def motion_rows(events: list[dict[str, Any]]) -> list[dict[str, Any]]:
    """兼容连续路线和独立片段采样；缺失字段留空，不混用时钟或插值补造帧。"""
    rows = []
    for event in events:
        if event.get("event_type") not in {"flight_motion_sample", "excerpt_motion_sample"}:
            continue
        row = {column: event.get(column, "") for column in MOTION_COLUMNS}
        row.update(parse_details(str(event.get("details", ""))))
        rows.append(row)
    return rows


def parse_details(value: str) -> dict[str, str]:
    """Parse the project's semicolon-separated event-detail contract."""
    parsed: dict[str, str] = {}
    for part in (value or "").split(";"):
        if "=" in part:
            key, item = part.split("=", 1)
            parsed[key.strip()] = item.strip()
    return parsed


def read_jsonl(path: Path) -> tuple[list[dict[str, Any]], list[str]]:
    """Read one JSONL log without silently discarding malformed lines."""
    records: list[dict[str, Any]] = []
    errors: list[str] = []
    for line_number, raw_line in enumerate(path.read_text(encoding="utf-8").splitlines(), start=1):
        if not raw_line.strip():
            continue
        try:
            record = json.loads(raw_line)
        except json.JSONDecodeError as error:
            errors.append(f"{path}:{line_number}: invalid JSON ({error.msg})")
            continue
        if not isinstance(record, dict):
            errors.append(f"{path}:{line_number}: JSON record is not an object")
            continue
        records.append(record)
    return records, errors


def find_session_logs(data_root: Path) -> Iterable[tuple[str, Path, Path]]:
    """Yield category, session folder and JSONL path under the AudioV0 data root."""
    for category_dir in sorted(path for path in data_root.iterdir() if path.is_dir()):
        for session_dir in sorted(path for path in category_dir.iterdir() if path.is_dir()):
            log_path = session_dir / "event_log.jsonl"
            if log_path.exists():
                yield category_dir.name, session_dir, log_path


def write_csv(path: Path, fieldnames: list[str], rows: list[dict[str, Any]]) -> None:
    """Write a stable UTF-8 CSV table with missing fields represented as empty values."""
    with path.open("w", encoding="utf-8", newline="") as output:
        writer = csv.DictWriter(output, fieldnames=fieldnames, extrasaction="ignore")
        writer.writeheader()
        writer.writerows(rows)


def notification_rows(events: list[dict[str, Any]]) -> list[dict[str, Any]]:
    """按会话/attempt/区块/片段/目标联结；旧日志保留空片段键，重复目标不再合并。"""
    grouped: dict[tuple[str, str, str, str, str], list[dict[str, Any]]] = defaultdict(list)
    excerpt_events: dict[tuple[str, str, str, str], list[dict[str, Any]]] = defaultdict(list)
    for event in events:
        scope = tuple(str(event.get(field, "")) for field in ("session_id", "attempt_id", "block_id", "excerpt_id"))
        if scope[3]:
            excerpt_events[scope].append(event)
        transition_id = str(event.get("transition_id", ""))
        if transition_id:
            key = (str(event.get("session_id", "")), str(event.get("attempt_id", "")), str(event.get("block_id", "")), str(event.get("excerpt_id", "")), transition_id)
            grouped[key].append(event)

    rows: list[dict[str, Any]] = []
    for (_, _, _, excerpt_id, transition_id), group in sorted(grouped.items()):
        by_type: dict[str, list[dict[str, Any]]] = defaultdict(list)
        for event in group:
            by_type[str(event.get("event_type", ""))].append(event)
        visual_only = "visual_only_prompt" in by_type
        if "notification_onset" not in by_type and "notification_requested" not in by_type and not visual_only:
            continue

        reference = (by_type.get("notification_onset") or by_type.get("notification_requested") or by_type.get("visual_only_prompt"))[0]
        onset = (by_type.get("notification_onset") or by_type.get("visual_only_prompt") or [None])[0]
        requested = (by_type.get("notification_requested") or [None])[0]
        transition = (by_type.get("transition_onset") or [None])[0]
        response = (by_type.get("identification_response") or [None])[0]
        timeout_event = (by_type.get("identification_timeout") or [None])[0]
        onset_details = parse_details(str((onset or requested or {}).get("details", "")))
        requested_details = parse_details(str((requested or {}).get("details", "")))
        response_details = parse_details(str((response or {}).get("details", "")))
        confidence_events = by_type.get("confidence_response", [])
        confidence_details = parse_details(str((confidence_events[0] if confidence_events else {}).get("details", "")))
        confidence_status = "response" if confidence_events else "timeout" if by_type.get("confidence_timeout") else "interrupted" if by_type.get("confidence_interrupted") else "missing" if by_type.get("confidence_window_open") else "not_collected"
        onset_ms = (onset or requested or {}).get("monotonic_clock_ms", "")
        response_ms = (response or {}).get("monotonic_clock_ms", "")
        quality_flags: list[str] = []
        if by_type.get("excerpt_invalid"):
            quality_flags.append("excerpt_invalid")
        if by_type.get("identification_interrupted") or by_type.get("confidence_interrupted"):
            quality_flags.append("response_interrupted")
        if excerpt_id:
            scope = tuple(str(reference.get(field, "")) for field in ("session_id", "attempt_id", "block_id", "excerpt_id"))
            scoped = excerpt_events[scope]
            ends = [event for event in scoped if event.get("event_type") == "excerpt_end"]
            if not ends:
                quality_flags.append("excerpt_end_missing")
            elif any(parse_details(str(event.get("details", ""))).get("invalid", "").lower() == "true" for event in ends):
                quality_flags.append("excerpt_ended_invalid")
            if any(event.get("event_type") in {"error", "session_withdrawn"} for event in scoped):
                quality_flags.append("excerpt_terminated")
        if len(confidence_events) > 1:
            quality_flags.append("duplicate_confidence_response")
        if confidence_events and confidence_details.get("parent_event_id") != response_details.get("parent_event_id"):
            quality_flags.append("confidence_parent_mismatch")
        if confidence_status == "missing":
            quality_flags.append("confidence_closure_missing")
        if confidence_events and confidence_details.get("value") not in {str(i) for i in range(1, 8)}:
            quality_flags.append("invalid_confidence_value")
        if onset is None:
            quality_flags.append("notification_onset_missing")
        if transition is None:
            quality_flags.append("transition_onset_missing")
        if response is None and timeout_event is None:
            quality_flags.append("response_or_timeout_missing")
        if response is not None and timeout_event is not None:
            quality_flags.append("response_timeout_conflict")

        response_time_ms: Any = response_details.get("response_time_ms", "")
        observed_response_time_ms: Any = ""
        if onset_ms != "" and response_ms != "":
            try:
                observed_response_time_ms = int(response_ms) - int(onset_ms)
                if observed_response_time_ms < 0:
                    quality_flags.append("negative_response_time")
            except (TypeError, ValueError):
                quality_flags.append("response_time_unparseable")
        if response_time_ms == "" and observed_response_time_ms != "":
            response_time_ms = observed_response_time_ms
        elif response_time_ms != "" and observed_response_time_ms != "":
            try:
                if abs(int(response_time_ms) - observed_response_time_ms) > 100:
                    quality_flags.append("response_time_clock_mismatch")
            except ValueError:
                quality_flags.append("response_time_unparseable")

        rows.append({
            "session_id": reference.get("session_id", ""),
            "attempt_id": reference.get("attempt_id", ""),
            "participant_id": reference.get("participant_id", ""),
            "block_id": reference.get("block_id", ""),
            "block_position": reference.get("block_position", ""),
            "route_id": reference.get("route_id", ""),
            "condition_id": reference.get("condition_id", ""),
            "transition_id": transition_id,
            "excerpt_id": excerpt_id,
            "validation_mode": "visual_only" if visual_only else "notification",
            "technically_valid": "False" if quality_flags else "True",
            "transition_target": reference.get("transition_target", ""),
            "notification_design": reference.get("notification_design", ""),
            "listening_condition": reference.get("listening_condition", ""),
            "stimulus_set_version": reference.get("stimulus_set_version", ""),
            "notification_onset_ms": onset_ms,
            "transition_onset_ms": (transition or {}).get("monotonic_clock_ms", ""),
            "transition_route_time_ms": (transition or {}).get("route_time_ms", ""),
            "software_lead_s": requested_details.get("observed_software_lead_s", ""),
            "clip": onset_details.get("clip", ""),
            "playback_observed": "True" if any(parse_details(str(e.get("details", ""))).get("is_playing", "").lower() == "true" for e in by_type.get("playback_observed", [])) else "False",
            "response_status": "response" if response else ("timeout" if timeout_event else "missing"),
            "selected_target": response_details.get("selected_target", ""),
            "communicated_target": response_details.get("communicated_target", ""),
            "correct": response_details.get("correct", ""),
            "response_time_ms": response_time_ms,
            "response_time_s": "" if response_time_ms == "" else round(float(response_time_ms) / 1000, 4),
            "timeout": "True" if timeout_event else "False",
            "confidence": confidence_details.get("value", ""),
            "confidence_status": confidence_status,
            "confidence_response_time_ms": confidence_details.get("response_time_ms", ""),
            "data_quality_flag": ";".join(quality_flags),
            "export_eligibility": reference.get("export_eligibility", ""),
        })
    return rows


def tracking_rows(events: list[dict[str, Any]]) -> list[dict[str, Any]]:
    """Normalise raw tracking samples while retaining every recorded observation."""
    rows: list[dict[str, Any]] = []
    for event in events:
        if event.get("event_type") != "tracking_sample":
            continue
        details = parse_details(str(event.get("details", "")))
        row = {column: event.get(column, "") for column in TRACKING_COLUMNS}
        for column in (
            "task_elapsed_s", "target_x", "target_y", "cursor_x", "cursor_y",
            "tracking_error", "inside_target", "trajectory_id", "difficulty_version",
        ):
            row[column] = details.get(column, "")
        rows.append(row)
    return rows


def validate_formal_session(
    records: list[dict[str, Any]],
    session_id: str,
    category: str,
    manifest: dict[str, Any],
    run_record: dict[str, Any] | None,
) -> list[str]:
    """Return explicit reasons why a formal run is incomplete or contaminated."""
    issues: list[str] = []
    if run_record is None:
        issues.append(f"{session_id}: missing required run_record.json")
    else:
        missing_fields = [
            field for field in FORMAL_RUN_RECORD_FIELDS
            if not str(run_record.get(field, "")).strip()
        ]
        if missing_fields:
            issues.append(f"{session_id}: run_record.json missing values for {', '.join(missing_fields)}")
        if str(run_record.get("run_id", "")).strip() != session_id:
            issues.append(f"{session_id}: run_record.json run_id does not match session directory")
        if str(run_record.get("storage_category", "")).strip() != category:
            issues.append(f"{session_id}: run_record.json storage_category does not match parent directory")
        manifest_participant = str(manifest.get("participant_id", "")).strip()
        if str(run_record.get("participant_id", "")).strip() != manifest_participant:
            issues.append(f"{session_id}: run_record.json participant_id does not match session manifest")
        for run_record_field, manifest_field in MANIFEST_RUN_RECORD_FIELDS.items():
            expected_value = str(run_record.get(run_record_field, "")).strip()
            manifest_value = str(manifest.get(manifest_field, "")).strip()
            if expected_value != manifest_value:
                issues.append(f"{session_id}: run_record.json {run_record_field} does not match session manifest")
    event_types = Counter(str(record.get("event_type", "")) for record in records)
    if records and run_record is not None:
        for run_record_field, event_field in STATIC_EVENT_RUN_RECORD_FIELD_MAP.items():
            expected_value = str(run_record.get(run_record_field, "")).strip()
            observed_values = {str(record.get(event_field, "")).strip() for record in records}
            if observed_values != {expected_value}:
                issues.append(
                    f"{session_id}: event {event_field} values {sorted(observed_values)} do not match run_record.json"
                )
        for event_field in DYNAMIC_EVENT_VERSION_FIELDS:
            if any(not str(record.get(event_field, "")).strip() for record in records):
                issues.append(f"{session_id}: one or more events are missing {event_field}")
    if event_types["session_end"] != 1:
        issues.append(f"{session_id}: requires exactly one session_end, found {event_types['session_end']}")
    if event_types["notification_onset"] != 12 or event_types["transition_onset"] != 12:
        issues.append(
            f"{session_id}: requires 12 notification_onset and 12 transition_onset events, found "
            f"{event_types['notification_onset']} and {event_types['transition_onset']}"
        )
    condition_counts = Counter(
        str(record.get("condition_id", ""))
        for record in records if record.get("event_type") == "notification_onset"
    )
    expected_conditions = {"NS_Q", "NS_N", "S_Q", "S_N"}
    if set(condition_counts) != expected_conditions or any(condition_counts[item] != 3 for item in expected_conditions):
        issues.append(f"{session_id}: requires three notification onsets in each formal condition, found {dict(condition_counts)}")
    for record in records:
        detail = str(record.get("details", ""))
        if record.get("event_type") == "error":
            issues.append(f"{session_id}: error event: {detail}")
        if "technical_only=true" in detail or "technical_demo" in detail:
            issues.append(f"{session_id}: technical-demo marker found in formal run")
            break
    return issues


def main() -> int:
    """Run validation and export. Exit nonzero when requested formal data is invalid."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("data_root", type=Path, help="AudioV0 root containing category/session folders")
    parser.add_argument("output_dir", type=Path, help="Empty or new directory for normalised CSV outputs")
    parser.add_argument("--allow-technical", action="store_true", help="QA only: include technical_demo records, marked non-participant")
    parser.add_argument("--session-id", help="Export only this exact session directory name")
    arguments = parser.parse_args()

    if not arguments.data_root.is_dir():
        parser.error(f"data root does not exist: {arguments.data_root}")
    arguments.output_dir.mkdir(parents=True, exist_ok=True)

    all_events: list[dict[str, Any]] = []
    session_report: list[dict[str, Any]] = []
    validation_errors: list[str] = []
    excluded_sessions: list[dict[str, str]] = []
    invalid_formal_sessions: set[str] = set()

    for category, session_dir, log_path in find_session_logs(arguments.data_root):
        if arguments.session_id and session_dir.name != arguments.session_id:
            continue
        manifest_path = session_dir / "session_manifest.json"
        run_record_path = session_dir / "run_record.json"
        manifest: dict[str, Any] = {}
        run_record: dict[str, Any] | None = None
        if manifest_path.exists():
            try:
                manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
            except json.JSONDecodeError as error:
                validation_errors.append(f"{manifest_path}: invalid JSON ({error.msg})")
        if run_record_path.exists():
            try:
                candidate_record = json.loads(run_record_path.read_text(encoding="utf-8"))
                if isinstance(candidate_record, dict):
                    run_record = candidate_record
                else:
                    validation_errors.append(f"{run_record_path}: JSON record is not an object")
            except json.JSONDecodeError as error:
                validation_errors.append(f"{run_record_path}: invalid JSON ({error.msg})")
        records, errors = read_jsonl(log_path)
        validation_errors.extend(errors)
        participant_id = str(manifest.get("participant_id", records[0].get("participant_id", "") if records else ""))
        eligible = category in FORMAL_CATEGORIES and participant_id and participant_id.lower() != "operator_test"
        if not eligible and not arguments.allow_technical:
            excluded_sessions.append({"session": session_dir.name, "category": category, "reason": "not_pilot_or_participant_run"})
            continue
        for record in records:
            record["export_category"] = category
            record["export_eligibility"] = "formal_candidate" if eligible else "qa_only"
        all_events.extend(records)
        if eligible:
            formal_issues = validate_formal_session(records, session_dir.name, category, manifest, run_record)
            validation_errors.extend(formal_issues)
            if formal_issues or errors:
                invalid_formal_sessions.add(str(records[0].get("session_id", session_dir.name)) if records else session_dir.name)
        session_report.append({
            "session_id": session_dir.name,
            "category": category,
            "participant_id": participant_id,
            "event_count": len(records),
            "eligible_for_formal_analysis": eligible,
            "run_record_present": run_record is not None,
            "source_path": str(log_path),
        })

    visual_trials = visual_rows(all_events)
    summary_checks = visual_summary_checks(all_events, visual_trials)
    formal_session_ids = {str(event.get("session_id", "")) for event in all_events
                          if event.get("export_eligibility") == "formal_candidate"}
    for row in visual_trials:
        flag = row.get("data_quality_flag", "")
        session_id = str(row.get("session_id", ""))
        # 空窗点击是有效记录的行为；缺失、双评分与时钟错误属于完整性失败。
        if session_id in formal_session_ids and flag and flag != "response_without_stimulus":
            validation_errors.append(session_id + ": visual integrity failure: " + str(flag))
            invalid_formal_sessions.add(session_id)
    for check in summary_checks:
        if check["session_id"] in formal_session_ids and not check["passed"]:
            validation_errors.append(check["session_id"] + ": visual summary mismatch: " + ";".join(check["mismatches"]))
            invalid_formal_sessions.add(check["session_id"])
    for event in all_events:
        if str(event.get("session_id", "")) in invalid_formal_sessions:
            event["export_eligibility"] = "invalid_formal"
    for row in session_report:
        if row["session_id"] in invalid_formal_sessions:
            row["eligible_for_formal_analysis"] = False
    visual_trials = visual_rows(all_events)
    event_rows = [{column: event.get(column, "") for column in EVENT_COLUMNS} for event in all_events]
    if arguments.session_id and not session_report and not excluded_sessions:
        validation_errors.append("requested_session_not_found: " + arguments.session_id)
    write_csv(arguments.output_dir / "events.csv", EVENT_COLUMNS, event_rows)
    write_csv(arguments.output_dir / "notification_events.csv", NOTIFICATION_COLUMNS, notification_rows(all_events))
    write_csv(arguments.output_dir / "tracking_samples.csv", TRACKING_COLUMNS, tracking_rows(all_events))
    write_csv(arguments.output_dir / "visual_events.csv", VISUAL_COLUMNS, visual_trials)
    write_csv(arguments.output_dir / "visual_responses.csv", VISUAL_RESPONSE_COLUMNS, visual_response_rows(all_events))
    write_csv(arguments.output_dir / "flight_motion_samples.csv", MOTION_COLUMNS, motion_rows(all_events))
    write_csv(arguments.output_dir / "sessions.csv", list(session_report[0].keys()) if session_report else ["session_id", "category", "participant_id", "event_count", "eligible_for_formal_analysis", "run_record_present", "source_path"], session_report)

    report = {
        "export_kind": "qa_only" if arguments.allow_technical else "formal_candidate_only",
        "included_session_count": len(session_report),
        "included_event_count": len(all_events),
        "event_type_counts": dict(Counter(str(event.get("event_type", "")) for event in all_events)),
        "excluded_sessions": excluded_sessions,
        "validation_errors": validation_errors,
        "visual_summary_checks": summary_checks,
        "visual_data_quality_flags": [
            {field: row.get(field, "") for field in ("session_id", "attempt_id", "block_id", "stimulus_id", "data_quality_flag")}
            for row in visual_trials if row.get("data_quality_flag")
        ],
        "outputs": ["events.csv", "notification_events.csv", "tracking_samples.csv", "visual_events.csv", "visual_responses.csv", "flight_motion_samples.csv", "sessions.csv"],
    }
    (arguments.output_dir / "validation_report.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, indent=2))
    return 1 if validation_errors else 0


if __name__ == "__main__":
    sys.exit(main())
