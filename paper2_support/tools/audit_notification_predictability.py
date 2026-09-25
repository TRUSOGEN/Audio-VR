"""审计通知答案能否由段内序号预测；只读日志，不计算参与者成绩。

读取指定 JSONL，按 session/attempt/block 分组，比较预先给定的
T01/T02/T03 序号策略。缺失、重复或不完整 block 明确列为不可判定。
"""
from __future__ import annotations
import argparse
import hashlib
import json
from collections import defaultdict
from pathlib import Path

EXPECTED = ('T01', 'T02', 'T03')


def audit(path: Path) -> dict:
    """返回固定序号策略的可预测性及原始文件校验值。"""
    raw = path.read_bytes()
    rows = [json.loads(line) for line in raw.decode('utf-8').splitlines() if line.strip()]
    groups = defaultdict(list)
    for row in rows:
        if row.get('event_type') == 'notification_onset':
            key = tuple(row[k] for k in ('session_id', 'attempt_id', 'block_id'))
            groups[key].append(row)
    blocks = []
    correct = total = 0
    for key, events in groups.items():
        ordered = sorted(events, key=lambda e: e['event_sequence'])
        ids = [e['transition_id'] for e in ordered]
        complete = len(ids) == 3 and set(ids) == set(EXPECTED)
        unique = len({e['event_sequence'] for e in ordered}) == len(ordered)
        valid = complete and unique
        matches = sum(a == b for a, b in zip(EXPECTED, ids)) if valid else None
        if valid:
            correct += matches
            total += 3
        blocks.append({'session_attempt_block': key, 'condition': ordered[0].get('condition_id'),
                       'observed_order': ids, 'eligible_for_order_audit': valid,
                       'reason': None if valid else 'missing_duplicate_or_incomplete_notifications',
                       'ordinal_rule_matches': matches})
    return {'purpose': 'software_schedule_audit_not_human_performance',
            'source': str(path.resolve()), 'sha256': hashlib.sha256(raw).hexdigest(),
            'prediction_rule': list(EXPECTED), 'blocks': blocks,
            'predictable_events': correct, 'eligible_events': total,
            'ordinal_rule_accuracy': correct / total if total else None,
            'interpretation': 'This rule uses event position only, never audio or participant responses. '
            'It assumes block boundaries and event position are known; it does not measure human use of this strategy.'}


def main() -> None:
    """执行指定日志审计并写入明确命名的 JSON 文件。"""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('logs', type=Path, nargs='+')
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    report = {'audits': [audit(p) for p in args.logs]}
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(report, ensure_ascii=False, indent=2) + '\n')
    for item in report['audits']:
        print(f"{Path(item['source']).parent.name}: {item['predictable_events']}/{item['eligible_events']} ordinal matches")


if __name__ == '__main__':
    main()
