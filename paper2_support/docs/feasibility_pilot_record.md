# Feasibility Pilot Record

Date: 2026-09-23
Status: blank 2--3 person feasibility record. This record assesses procedure viability and must not be used to estimate condition effects, select a preferred notification, or report confirmatory results.

Current author decision: four complete ground-to-ground journeys (`context_supported_full_flight_v1_candidate` / `full_city_journeys_v5_210s_candidate`), targeting no more than 210 seconds (3:30) per route pending normal-clock rerun and pilot; brief tutorial with six sound examples, two GO/WAIT items, one identification/confidence item and one instruction check. Every route also uses shared direction-matched left/right turn cues, which require pilot checks for side mapping, audibility, comfort and task interference but are not a third factor. Incorrect responses receive an explanation and may continue, with voluntary replay. M1/V4 are selected development candidates. The current four-route normal-clock synthetic QA passed; no human pilot, calibration or main-study freeze is recorded here.

The [runtime audit](/Users/trusoegn/GitHub/Audio-VR/Audio-VR/temp/20260923-full-route-revision/runtime_audit.json) records 12 notifications/transitions, 11 identification/confidence responses, one deliberate timeout and 5,485 motion samples. The log's total route-clock duration is 1,109.197 s; total four-route QA was 1,112.533 s including brief pause/block transitions, not a participant session duration. The pause probe occurred between notifications and did not test an open response window. These records cannot populate the participant rows below.

## 1. Scope and safeguards

This pilot starts only after ethics coverage, consent, withdrawal handling, HMD acceptance and a four-condition technical dry run are documented. Pilot participants receive pseudonyms only; names and consent forms stay outside Unity data directories. Each pilot session uses `storage_category=pilot`, a `run_record.json`, a linked calibration ID, and an immutable event log.

## 2. Pilot configuration

| Field | Entry |
| --- | --- |
| Pilot protocol version | |
| Stimulus and noise version | |
| Route and motion profile | |
| Visual task and difficulty version | |
| Response mapping / timeout / practice rule | |
| HMD and audio-chain version | |
| Calibration record ID | |
| Ethics/consent version checked | |
| Operator | |

## 3. Session register

| Pseudonym | Run ID | Date | Counterbalance sequence | All four blocks complete? | Withdrawal / deviation link | Export validation | Include in feasibility summary? |
| --- | --- | --- | --- | --- | --- | --- | --- |
| P001 | | | | | | | |
| P002 | | | | | | | |
| P003 | | | | | | | |

`Include in feasibility summary` means that the run is usable for documenting procedural observations. It never makes the run a main-study observation.

## 4. Per-session feasibility observations

Complete one row per participant and preserve source notes separately from any summary.

| Pseudonym | Practice understood without in-block coaching? | Input conflict / accidental presses | Speech audibility quiet / noise | Non-speech learnability | Motion comfort / stop events | Visual-task floor or ceiling signal | Total session duration | Questionnaire comprehension / burden | Key change request |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| P001 | | | | | | | | | |
| P002 | | | | | | | | | |
| P003 | | | | | | | | | |

## 5. Technical evidence audit

| Check | P001 | P002 | P003 | Disposition |
| --- | --- | --- | --- | --- |
| One `session_end`; normally 12 notification/transition onsets, with every interruption or missing event explained rather than fabricated | | | | |
| Three notification onsets in each condition | | | | |
| No error event or technical-demo marker | | | | |
| `run_record.json` and calibration link complete | | | | |
| GO/WAIT scored opportunities, interrupted events and denominators exported | | | | |
| Operator/debug content absent from participant view | | | | |
| Formal audio policy had no prohibited technical playback | | | | |

## 6. Feasibility decision log

| Domain | Evidence reviewed | Decision: retain / change / remove / unresolved | Version impact and required rerun |
| --- | --- | --- | --- |
| Route speed, turns and visual flow | | | |
| Speech wording, voice and delivery | | | |
| Non-speech family and learning procedure | | | |
| Noise, output path and calibration method | | | |
| Visual task trajectory and scoring window | | | |
| Input mapping, timeout and confidence item | | | |
| Confidence burden, interview and break schedule | | | |
| Context-supported judgement and reported use of route/order/sound cues | | | |
| Counterbalancing / route allocation | | | |
| Log export and data quality | | | |

## 6.1 Identification validity check

The selected complete journeys present T01/T02/T03 in flight order, allowing ordinal position and scenery to support the response. Main-task performance therefore concerns context-supported notification judgement. A successful pilot or a lack of reported strategy does not establish sound-only identification or remove the structural contribution of context.

Record reliance on sound, phase order and scenery; duration of each route and the full session; tutorial replay/help; comprehension; comfort and usability. A separate sound-only assessment may support independent discriminability claims if designed and conducted, but it must not be inferred from route-task scores or tutorial completion. Browser QA and synthetic Unity logs must not populate the participant rows above.

2026-09-23 current update: complete routes supersede the earlier independent-excerpt default. The normal 12-event/three-target checklist applies to complete, uninterrupted journeys; preserve invalid or missing events when a run is interrupted. Historical excerpt logs retain their repeated-target allocation and must be analysed under their original protocol. Follow the current interpretation in `identification_validity_gate.md` and the participant interview in runbook §7. No human observations have been entered.

## 7. Gate outcome

Select exactly one outcome and explain it with links to the evidence above.

| Outcome | Rationale | Next action |
| --- | --- | --- |
| `hold`: a critical comfort, audibility, input, logging or ethics issue remains | | fix, version and rerun the affected technical/HMD check |
| `iterate`: no critical issue, but one or more candidate parameters must change | | update configuration, rerun affected dry run, then repeat feasibility checks as needed |
| `freeze candidate`: all predefined feasibility checks are acceptable | | complete `docs/protocol_freeze_record.md`, sample-size/precision rationale and recruitment review |
