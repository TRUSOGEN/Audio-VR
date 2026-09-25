"""为 20 人四条件短飞行片段生成可审计的候选分配表。

此表只用于设计核对：每次片段独立重置，目标在个人三个片段内
允许重复；正式使用前还需场景、pilot、精度和伦理审核。
"""

from __future__ import annotations

import argparse
import itertools
import json
import random
from collections import Counter, defaultdict
from pathlib import Path


CONDITIONS = ("NS_Q", "NS_N", "S_Q", "S_N")
TARGETS = ("T01", "T02", "T03")
ROUTES = ("R1", "R2", "R3", "R4")
CONDITION_ROWS = (
    ("NS_Q", "NS_N", "S_N", "S_Q"),
    ("NS_N", "S_Q", "NS_Q", "S_N"),
    ("S_Q", "S_N", "NS_N", "NS_Q"),
    ("S_N", "NS_Q", "S_Q", "NS_N"),
)


def route_rows() -> dict[tuple[int, int], tuple[str, ...]]:
    """在 20 人中精确平衡条件×路线，并尽量平衡路线×区段位置。"""
    rows = {
        (group, person): tuple(ROUTES[(position + person) % 4] for position in range(4))
        for group in range(4)
        for person in range(4)
    }
    best: tuple[int, tuple[tuple[str, ...], ...]] | None = None

    def extend(extra: list[tuple[str, ...]], used_pairs: set[tuple[str, str]]) -> None:
        """为每组第五人找条件×路线不重复且位置偏差最小的排列。"""
        nonlocal best
        group = len(extra)
        if group == 4:
            counts = Counter((position, route) for row in extra for position, route in enumerate(row))
            penalty = sum((counts[position, route] - 1) ** 2 for position in range(4) for route in ROUTES)
            candidate = tuple(extra)
            if best is None or penalty < best[0] or (penalty == best[0] and candidate < best[1]):
                best = penalty, candidate
            return
        for row in itertools.permutations(ROUTES):
            pairs = {(CONDITION_ROWS[group][position], route) for position, route in enumerate(row)}
            if pairs.isdisjoint(used_pairs):
                extend([*extra, row], used_pairs | pairs)

    extend([], set())
    if best is None:
        raise RuntimeError("No exact condition-route allocation exists")
    for group, row in enumerate(best[1]):
        rows[group, 4] = row
    return rows


def targets_for_condition(rng: random.Random) -> dict[tuple[int, int], str]:
    """每条件×路线×位置给三个目标分配 2/2/1，允许个人内重复。"""
    singleton_rows = [
        TARGETS,
        TARGETS[1:] + TARGETS[:1],
        TARGETS[2:] + TARGETS[:2],
        TARGETS,
    ]
    rng.shuffle(singleton_rows)
    allocation: dict[tuple[int, int], str] = {}
    for route, singleton_row in enumerate(singleton_rows):
        for position, singleton in enumerate(singleton_row):
            values = [target for target in TARGETS for _ in range(1 if target == singleton else 2)]
            rng.shuffle(values)
            for person_on_route, target in enumerate(values):
                allocation[route, person_on_route * 3 + position] = target
    return allocation


def build(seed: int) -> dict:
    """生成固定种子的候选表；目标文件在正式研究前必须保密。"""
    rng = random.Random(seed)
    routes = route_rows()
    target_tables = {condition: targets_for_condition(rng) for condition in CONDITIONS}
    route_person_index = defaultdict(int)
    rows = []
    for group in range(4):
        for participant_in_group in range(5):
            participant = f"P{group * 5 + participant_in_group + 1:03d}"
            for block_position, condition in enumerate(CONDITION_ROWS[group], start=1):
                route = routes[group, participant_in_group][block_position - 1]
                person_on_route = route_person_index[condition, route]
                route_person_index[condition, route] += 1
                for event_position in range(1, 4):
                    rows.append({
                        "participant_slot": participant,
                        "block_position": block_position,
                        "condition_id": condition,
                        "route_id": route,
                        "event_position": event_position,
                        "target_id": target_tables[condition][ROUTES.index(route), person_on_route * 3 + event_position - 1],
                        "excerpt_id": f"{route}_{event_position}",
                    })
    result = {
        "status": "schedule_candidate_not_protocol_freeze",
        "design": "four_conditions_three_independent_excerpts_per_condition",
        "seed": seed,
        "instruction": "Do not expose this allocation to participants; route/excerpt assets do not yet exist.",
        "events": rows,
    }
    audit(result)
    return result


def audit(schedule: dict) -> dict:
    """验证试次、条件、路线、位置与目标平衡；不验证视觉真实性。"""
    rows = schedule["events"]
    if len(rows) != 240:
        raise ValueError(f"Expected 240 events, found {len(rows)}")
    groups: dict[str, list[dict]] = defaultdict(list)
    for row in rows:
        groups[row["participant_slot"]].append(row)
    if len(groups) != 20:
        raise ValueError("Expected 20 participant slots")
    for participant, events in groups.items():
        if len(events) != 12:
            raise ValueError(f"{participant}: not 12 events")
        if len({(r["block_position"], r["event_position"]) for r in events}) != 12:
            raise ValueError(f"{participant}: duplicate or missing event slot")
        if tuple(r["condition_id"] for r in events if r["event_position"] == 1) not in CONDITION_ROWS:
            raise ValueError(f"{participant}: invalid condition order")
        if any(r["condition_id"] not in CONDITIONS or r["route_id"] not in ROUTES or r["target_id"] not in TARGETS for r in events):
            raise ValueError(f"{participant}: invalid condition, route or target")
        for condition in CONDITIONS:
            target_events = [r for r in events if r["condition_id"] == condition]
            if sorted(r["event_position"] for r in target_events) != [1, 2, 3]:
                raise ValueError(f"{participant}: invalid positions in {condition}")
            if len({r["route_id"] for r in target_events}) != 1:
                raise ValueError(f"{participant}: route changes inside {condition}")
            if any(r["excerpt_id"] != f'{r["route_id"]}_{r["event_position"]}' for r in target_events):
                raise ValueError(f"{participant}: excerpt mismatch")
        if Counter(r["route_id"] for r in events) != Counter({route: 3 for route in ROUTES}):
            raise ValueError(f"{participant}: route repeated across conditions")
    sequence_counts = Counter(tuple(r["condition_id"] for r in events if r["event_position"] == 1) for events in groups.values())
    if any(sequence_counts[row] != 5 for row in CONDITION_ROWS):
        raise ValueError("condition sequence imbalance")
    condition_route = Counter((r["condition_id"], r["route_id"]) for r in rows)
    condition_target = Counter((r["condition_id"], r["target_id"]) for r in rows)
    route_target = Counter((r["condition_id"], r["route_id"], r["target_id"]) for r in rows)
    condition_block = Counter((r["condition_id"], r["block_position"]) for r in rows)
    route_block = Counter((r["route_id"], r["block_position"]) for r in rows)
    target_position = Counter((r["condition_id"], r["event_position"], r["target_id"]) for r in rows)
    target_route_position = Counter((r["condition_id"], r["route_id"], r["event_position"], r["target_id"]) for r in rows)
    if any(condition_route[c, route] != 15 for c in CONDITIONS for route in ROUTES):
        raise ValueError("condition-route imbalance")
    if any(condition_target[c, target] != 20 for c in CONDITIONS for target in TARGETS):
        raise ValueError("condition-target imbalance")
    if any(route_target[c, route, target] != 5 for c in CONDITIONS for route in ROUTES for target in TARGETS):
        raise ValueError("route-target imbalance")
    if any(condition_block[c, pos] != 15 for c in CONDITIONS for pos in range(1, 5)):
        raise ValueError("condition-position imbalance")
    if any(route_block[route, pos] < 12 or route_block[route, pos] > 18 for route in ROUTES for pos in range(1, 5)):
        raise ValueError("route-position imbalance exceeds one participant")
    if any(target_position[c, pos, target] not in (6, 7) for c in CONDITIONS for pos in range(1, 4) for target in TARGETS):
        raise ValueError("target-position imbalance")
    if any(target_route_position[c, route, pos, target] not in (1, 2)
           for c in CONDITIONS for route in ROUTES for pos in range(1, 4) for target in TARGETS):
        raise ValueError("target-route-position imbalance")
    if any(count != 5 for count in Counter((r["condition_id"], r["route_id"], r["event_position"])
                                      for r in rows).values()):
        raise ValueError("condition-route-position imbalance")
    adjacent = Counter(
        (events[0]["condition_id"], events[1]["condition_id"])
        for group in range(4)
        for events in [
            [r for r in groups[f"P{group * 5 + 1:03d}"] if r["event_position"] == 1][i:i+2]
            for i in range(3)
        ]
    )
    if any(adjacent[a, b] != 1 for a in CONDITIONS for b in CONDITIONS if a != b):
        raise ValueError("condition carryover imbalance")
    repeated_target_blocks = 0
    triple_repeated_target_blocks = 0
    for participant in groups:
        for condition in CONDITIONS:
            targets = [r["target_id"] for r in groups[participant] if r["condition_id"] == condition]
            repeated_target_blocks += len(set(targets)) < 3
            triple_repeated_target_blocks += len(set(targets)) == 1
    return {
        "participants": 20,
        "blocks": 80,
        "events": len(rows),
        "events_per_condition": 60,
        "events_per_condition_target": 20,
        "events_per_condition_route_target": 5,
        "condition_occurrences_per_block_position": 5,
        "route_occurrences_per_block_position": {
            route: [route_block[route, pos] // 3 for pos in range(1, 5)] for route in ROUTES
        },
        "target_occurrences_per_condition_event_position": [6, 7],
        "max_ordinal_position_accuracy_without_schedule": 7 / 20,
        "max_condition_route_position_accuracy_from_schedule": 2 / 5,
        "blocks_with_repeated_target": repeated_target_blocks,
        "blocks_with_same_target_three_times": triple_repeated_target_blocks,
        "visual_leakage_checked": False,
    }


def main() -> None:
    """将候选分配表和审计结果分别写入 temp。"""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--seed", type=int, default=20260923)
    parser.add_argument("--output-dir", type=Path, default=Path("temp/independent_excerpts_20260923"))
    args = parser.parse_args()
    args.output_dir.mkdir(parents=True, exist_ok=True)
    schedule = build(args.seed)
    report = audit(schedule)
    (args.output_dir / "candidate_schedule.json").write_text(json.dumps(schedule, indent=2) + "\n")
    (args.output_dir / "allocation_audit.json").write_text(json.dumps(report, indent=2) + "\n")
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
