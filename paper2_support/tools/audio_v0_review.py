"""为现有本地查看器提供统一的筛选、描述性聚合与论文图导出。

复用正式导出器的逐事件联结和质量标记；界面、CSV 和图件消费同一个
review 对象。技术演示、pilot 和正式候选数据始终按 session 分开审阅。
"""

from __future__ import annotations

import csv
import io
import math
import statistics
from collections import Counter, defaultdict
from pathlib import Path
from typing import Any

from export_audio_v0_events import motion_rows, notification_rows, parse_details, read_jsonl, visual_rows


CONDITIONS = ("NS_Q", "NS_N", "S_Q", "S_N")
LABELS = {"NS_Q": "Non-speech · quiet", "NS_N": "Non-speech · noise",
          "S_Q": "Speech · quiet", "S_N": "Speech · noise"}
COLORS = {"NS_Q": "#23527C", "NS_N": "#73A6C8", "S_Q": "#A65A20", "S_N": "#DBAA72"}
TASK_VERSION = "rhythm_timing_go_nogo_candidate_v2"


def number(value: Any) -> float | None:
    """仅转换实际有限数值；空缺不作为零。"""
    try:
        result = float(value) if value not in (None, "") else None
        return result if result is not None and math.isfinite(result) else None
    except (TypeError, ValueError):
        return None


def truth(value: Any) -> bool:
    """解析日志布尔值。"""
    return str(value).lower() == "true"


def median(values: list[float]) -> float | None:
    """空样本保持缺失。"""
    return statistics.median(values) if values else None


def key(row: dict[str, Any]) -> tuple[str, ...]:
    """与通知导出器使用相同的 session/attempt/block/excerpt/target 主键。"""
    return tuple(str(row.get(field, "")) for field in
                 ("session_id", "attempt_id", "block_id", "excerpt_id", "transition_id"))


def csv_bytes(rows: list[dict[str, Any]], fields: list[str] | None = None) -> bytes:
    """导出统一 review 表，缺失为留空，不伪装成零值。"""
    stream = io.StringIO(newline="")
    if rows or fields:
        fields = fields or list(dict.fromkeys(field for row in rows for field in row))
        writer = csv.DictWriter(stream, fieldnames=fields, extrasaction="ignore")
        writer.writeheader()
        writer.writerows(rows)
    return stream.getvalue().encode("utf-8-sig")


def build_review(log_path: Path, category: str, filters: dict[str, str] | None = None) -> dict[str, Any]:
    """读取一个 session 并应用 condition/block/route 筛选，不跨会话合并。"""
    events, errors = read_jsonl(log_path)
    return review_events(events, category, filters or {}, errors)


def review_events(events: list[dict[str, Any]], category: str, filters: dict[str, str],
                  parse_errors: list[str] | None = None) -> dict[str, Any]:
    """生成唯一的描述性审阅模型；不作统计推断或正式数据批准。"""
    parse_errors = parse_errors or []
    source = events
    fields = {"condition": "condition_id", "block": "block_id", "route": "route_id"}
    events = [event for event in source if all(not filters.get(name) or filters[name] == event.get(field)
                                             for name, field in fields.items())]
    practice_count = sum(str(e.get("event_type", "")).startswith("tutorial_") for e in events)
    task_events = [e for e in events if not str(e.get("event_type", "")).startswith("tutorial_")]
    ids = Counter(e.get("event_id") for e in source if e.get("event_id"))
    duplicate_ids = sum(count - 1 for count in ids.values() if count > 1)
    synthetic = any("synthetic" in str(e.get("details", "")).lower() for e in source)
    technical_marker = any(str(e.get("participant_id", "")).lower() in {"operator_test", "technical_demo"}
                           or "storage_category=technical_demo" in str(e.get("details", "")).lower()
                           or "technical_only=true" in str(e.get("details", "")).lower() for e in source)
    category_labels = {"technical_demo": "Technical demonstration", "pilot": "Feasibility pilot",
                       "participant_run": "Participant run (review pending)"}
    evidence = category_labels.get(category, "Unclassified data")
    if technical_marker and category != "technical_demo":
        evidence = "Technical / operator data (stored in " + category + ")"
    if synthetic:
        evidence += " · synthetic inputs"
    scope = "qa_only" if category == "technical_demo" or synthetic or technical_marker else "descriptive_review_only"
    grouped: dict[tuple[str, ...], list[dict[str, Any]]] = defaultdict(list)
    for event in task_events:
        grouped[key(event)].append(event)
    trials = notification_rows(task_events)
    for row in trials:
        flags = [flag for flag in str(row.get("data_quality_flag", "")).split(";") if flag]
        group = grouped[key(row)]
        for event_type, output_field in (("notification_onset", "notification_event_id"),
                                        ("identification_response", "response_event_id"),
                                        ("confidence_response", "confidence_event_id")):
            row[output_field] = next((e.get("event_id", "") for e in group if e.get("event_type") == event_type), "")
        counts = Counter(e.get("event_type") for e in group)
        for kind in ("notification_onset", "identification_response", "identification_timeout", "transition_onset"):
            if counts[kind] > 1:
                flags.append("duplicate_" + kind)
        if any(ids[e.get("event_id")] > 1 for e in group if e.get("event_id")):
            flags.append("duplicate_event_id")
        if row.get("validation_mode") != "notification":
            flags.append("visual_only_validation")
        if row.get("response_status") == "response":
            onsets = [e for e in group if e.get("event_type") == "notification_onset"]
            answers = [e for e in group if e.get("event_type") == "identification_response"]
            if onsets and answers:
                answer_details = parse_details(str(answers[0].get("details", "")))
                if answer_details.get("parent_event_id") != onsets[0].get("event_id"):
                    flags.append("response_parent_mismatch")
                if "accepted" in answer_details and not truth(answer_details["accepted"]):
                    flags.append("response_not_accepted")
            if str(row.get("correct", "")).lower() not in {"true", "false"}:
                flags.append("missing_correctness")
            rt = number(row.get("response_time_ms"))
            if rt is None or rt < 0:
                flags.append("missing_or_invalid_response_time")
            onset, transition = number(row.get("notification_onset_ms")), number(row.get("transition_onset_ms"))
            if rt is not None and onset is not None and transition is not None and onset + rt >= transition:
                flags.append("response_at_or_after_transition")
            windows = [number(parse_details(str(e.get("details", ""))).get("response_window_s"))
                       for e in group if e.get("event_type") == "identification_window_open"]
            if rt is not None and any(w is not None and rt > w * 1000 + 1 for w in windows):
                flags.append("response_after_window")
        if parse_errors:
            flags.append("malformed_source_json")
        row["review_quality_flags"] = ";".join(dict.fromkeys(flags))
        row["review_valid"] = not flags
        row["export_eligibility"] = scope

    visuals = visual_rows(task_events)
    motion = motion_rows(task_events)
    for row in visuals + motion:
        row["export_eligibility"] = scope
    source_by_id = {e.get("event_id"): e for e in task_events if e.get("event_id")}
    for row in motion:
        event = source_by_id.get(row.get("event_id"), {})
        row["phase"] = parse_details(str(event.get("details", ""))).get("phase", "")
        row["review_quality_flags"] = "duplicate_event_id" if ids[row.get("event_id")] > 1 else ""
    for row in visuals:
        flags = [flag for flag in str(row.get("data_quality_flag", "")).split(";") if flag]
        if row.get("task_version") != TASK_VERSION:
            flags.append("other_task_version")
        if not truth(row.get("scorable")):
            flags.append("not_scorable")
        if row.get("outcome") not in {"hit", "miss", "false_alarm", "correct_rejection"}:
            flags.append("unscored_" + str(row.get("outcome") or "missing_outcome"))
        if any(ids[row.get(field)] > 1 for field in ("onset_event_id", "offset_event_id", "scored_response_event_id") if row.get(field)):
            flags.append("duplicate_event_id")
        if parse_errors:
            flags.append("malformed_source_json")
        row["review_quality_flags"] = ";".join(dict.fromkeys(flags))
        row["review_valid"] = not flags
    condition_ids = [c for c in CONDITIONS if not filters.get("condition") or filters["condition"] == c]
    condition_ids += sorted({str(row.get("condition_id")) for row in trials + visuals + motion}
                            - set(CONDITIONS) - {"", "None", "TECH_TEST", "PRACTICE"})
    summaries = []
    for condition in condition_ids:
        selected = [row for row in trials if row.get("condition_id") == condition]
        valid = [row for row in selected if row["review_valid"]]
        correct = [row for row in valid if row["response_status"] == "response" and truth(row["correct"])]
        rts = [number(row["response_time_ms"]) for row in correct]
        rts = [value for value in rts if value is not None]
        confidence = [number(row["confidence"]) for row in valid if row["confidence_status"] == "response"]
        confidence = [value for value in confidence if value is not None and 1 <= value <= 7]
        visual = [row for row in visuals if row.get("condition_id") == condition and row.get("task_version") == TASK_VERSION]
        usable_visual = [row for row in visual if row["review_valid"]]
        outcomes = Counter(row.get("outcome") for row in usable_visual)
        go_n, wait_n = outcomes["hit"] + outcomes["miss"], outcomes["false_alarm"] + outcomes["correct_rejection"]
        summaries.append({
            "session_id": source[0].get("session_id", "") if source else "", "storage_category": category,
            "evidence_label": evidence, "export_eligibility": scope, "condition_id": condition,
            "block_filter": filters.get("block", ""), "route_filter": filters.get("route", ""),
            "recorded_notifications": len(selected), "identification_denominator": len(valid),
            "correct": len(correct), "accuracy": len(correct) / len(valid) if valid else None,
            "timeouts": sum(row["response_status"] == "timeout" for row in valid),
            "invalid_notifications": len(selected) - len(valid),
            "correct_rt_n": len(rts), "median_correct_rt_ms": median(rts),
            "confidence_n": len(confidence), "median_confidence": median(confidence),
            "go_hits": outcomes["hit"], "go_denominator": go_n,
            "go_hit_rate": outcomes["hit"] / go_n if go_n else None,
            "wait_false_alarms": outcomes["false_alarm"], "wait_denominator": wait_n,
            "wait_false_alarm_rate": outcomes["false_alarm"] / wait_n if wait_n else None,
            "visual_interrupted": sum(row.get("outcome") == "interrupted" for row in visual),
            "visual_invalid": sum(not row["review_valid"] and row.get("outcome") != "interrupted" for row in visual),
            "visual_other_versions": sum(row.get("condition_id") == condition and row.get("task_version") != TASK_VERSION for row in visuals),
            "motion_samples": sum(row.get("condition_id") == condition for row in motion),
        })
    metadata_fields = ("participant_id", "route_version", "stimulus_set_version", "noise_version", "task_version",
                       "protocol_version", "apparatus_version", "calibration_record_id", "counterbalance_sequence")
    metadata = {field: sorted({str(event.get(field, "")) for event in task_events}) for field in metadata_fields}
    metadata["motion_version"] = sorted({str(row.get("motion_version", "")) for row in motion})
    sequence = [e for e in events if e.get("event_type") in {"route_start", "block_started", "excerpt_start",
                "notification_onset", "identification_response", "identification_timeout", "transition_onset",
                "pause", "block_paused", "excerpt_end", "experiment_block_complete", "session_end", "error"}]
    return {
        "session_id": source[0].get("session_id", log_id(source)) if source else "empty", "category": category,
        "evidence_label": evidence, "synthetic_inputs": synthetic, "export_eligibility": scope,
        "filters": filters, "filter_options": {name: sorted({str(e.get(field)) for e in source if e.get(field)}) for name, field in fields.items()},
        "summary": summaries, "trials": trials, "visual": visuals, "motion": motion, "timeline": sequence,
        "events": events, "metadata": metadata,
        "quality": {"malformed_json": len(parse_errors), "parse_errors": parse_errors, "duplicate_event_ids": duplicate_ids,
                    "logged_errors": sum(e.get("event_type") == "error" for e in events),
                    "tutorial_events_excluded": practice_count, "invalid_notifications": sum(not row["review_valid"] for row in trials),
                    "session_end_count": sum(e.get("event_type") == "session_end" for e in source),
                    "four_blocks_complete": any(e.get("event_type") == "experiment_sequence_complete" for e in source),
                    "total_source_events": len(source), "filtered_events": len(events)},
    }


def log_id(events: list[dict[str, Any]]) -> str:
    """未带 session 身份的输入保持显式未知。"""
    return "unknown_session"


def cleaned_rows(review: dict[str, Any], panel: str, excluded: bool = False) -> list[dict[str, Any]]:
    """返回单图实际使用的逐事件/逐采样数据；其他记录保留在明确标记的排除表。"""
    if panel not in {"accuracy", "rt", "confidence", "visual", "trajectory", "altitude"}:
        raise ValueError("Unknown panel: " + panel)
    source = review["trials"] if panel in {"accuracy", "rt", "confidence"} else review["visual"] if panel == "visual" else review["motion"]
    rows = []
    for original in source:
        row = dict(original)
        reasons = [flag for flag in str(row.get("review_quality_flags", "")).split(";") if flag]
        if panel == "rt":
            if row.get("response_status") != "response":
                reasons.append("no_accepted_response_" + str(row.get("response_status", "missing")))
            elif not truth(row.get("correct")):
                reasons.append("incorrect_response")
            if number(row.get("response_time_ms")) is None:
                reasons.append("response_time_missing")
        if panel == "confidence" and (row.get("confidence_status") != "response" or number(row.get("confidence")) is None):
            reasons.append("confidence_" + str(row.get("confidence_status", "missing")))
        if panel == "trajectory" and any(number(row.get(field)) is None for field in ("x_m", "z_m")):
            reasons.append("horizontal_position_missing")
        if panel == "altitude" and any(number(row.get(field)) is None for field in ("y_m", "monotonic_clock_ms")):
            reasons.append("position_or_session_clock_missing")
        if bool(reasons) != excluded:
            continue
        row.update(metric=panel, included_in_metric=not reasons,
                   exclusion_reason=";".join(dict.fromkeys(reasons)), storage_category=review["category"],
                   evidence_label=review["evidence_label"], condition_filter=review["filters"].get("condition", ""),
                   block_filter=review["filters"].get("block", ""), route_filter=review["filters"].get("route", ""))
        if panel == "visual":
            row["metric_series"] = "GO" if truth(row.get("is_target")) else "WAIT"
        rows.append(row)
    return rows


def make_figure(review: dict[str, Any], kind: str = "conditions", format: str = "svg") -> bytes:
    """延迟导入绘图后端，聚合与HTTP读取不依赖绘图初始化。"""
    from audio_v0_review_figures import render_figure
    return render_figure(review, kind, format)
