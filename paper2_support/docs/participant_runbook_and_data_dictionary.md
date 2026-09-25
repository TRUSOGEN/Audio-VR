# Participant Runbook and Data Dictionary

桌面使用见 [Audio V0 使用手册](/Users/trusoegn/GitHub/Audio-VR/Audio-VR/docs/audio-v0-user-manual.md)。

## 当前完整航程程序（协议决定 2026-09-23；手册核验 2026-09-24）

- 当前工程与拟 pilot 候选是四段完整的地面到地面航程，每段包含起飞、爬升、巡航、左右转弯、下降和着陆；候选结构为四种条件各一个 block。最终条件/路线顺序、counterbalance 配置和其他研究参数尚未冻结，不可从技术演示顺序推断。
- 每条航程按飞行阶段顺序呈现 T01、T02、T03。路线进度、景物和阶段顺序可能帮助判断，因此记录与论文中的识别结果应称为 context-supported notification judgement，不据此主张 sound-only identification 或排除了视觉/顺序线索。
- 快速教程包含六个带标签的声音示例、两次 GO/WAIT 练习、一次识别/信心练习和一道理解题；答错后解释并允许继续，可自愿重听。练习事件保留在原始日志，但不进入实验指标。该教程尚未验证为充分的学习标准。
- 当前工程候选把每条航程目标设为 ≤210 秒（3:30）；2026-09-24 实际场景运动表为约 201.85 / 206.13 / 192.26 / 192.17 s，净空审计 0 碰撞、0 自动修补；正常时钟与回归的精确证据范围见当日软件交接。仍需 feasibility pilot，尚未成为冻结的真人时长或研究参数。18 秒软件通知提前量、答题窗口及其他 profile 参数同样保持候选状态。
- 四条件正式流程均包含方向匹配的左右转提示音：左转从左侧、右转从右侧播放，并记录为 `spatial_turn_cue`。它是四条件共有的旅程提示，不增加实验因子，也不计入 T01--T03 识别分母；pilot 需要单独检查左右映射、可听性、舒适度和是否干扰视觉任务。
- 暂停、退出、设备故障和缺失事件按日志记录；保留原始事件，不补造 response、confidence 或运动样本。`technically_valid` 只表示导出完整性与错误标记检查，不表示人体效度或正式分析资格。

### 历史：独立片段模式（2026-09-23；已被 protocol §0.7 取代）

- 旧候选在四个条件中各呈现三个独立片段，目标由随机流有放回抽取；同一目标可能重复，必须用 `excerpt_id` 区分事件，不能只按 T01/T02/T03 合并。
- 旧片段暂停会使当前片段无效；`visualOnlyValidation` 是独立线索检查候选，不属于当前四条件比较。
- 旧片段的 boundary 2 s、前段 4 s、识别/信心各 8 s、余量 2 s、后段 5 s 和 18 s 软件提前量都是未 pilot 冻结的工程参数，不适用于当前完整航程，也不表示真人时长。
- 如审阅旧 excerpt 数据，必须按其原始版本单独解释，不能与当前完整航程合并。

Date: 2026-09-24
Status: executable preparation and data-handling record. The current protocol release fields remain open. This document does not by itself authorise recruitment, participant collection, or confirmatory analysis. A feasibility pilot may support documented process decisions only after the applicable ethics, consent, data-handling and protocol checks.

## 1. Run Classification

| Category | Permitted purpose | May enter thesis Results? |
| --- | --- | --- |
| `technical_demo` | code, UI, routing and audio-chain diagnostics | No |
| `pilot` | 2--3 person feasibility assessment after protocol/ethics checks | Only as feasibility/process evidence, not confirmatory effects |
| `participant_run` | frozen, approved main-study collection | Yes, after data-quality checks |

The Unity formal-mode guard rejects `technical_demo` and `operator_test`. A participant run must have a pseudonymous ID such as `P001`; names, emails and consent forms do not enter Unity logs.

Evidence category is a provenance label, not a quality score or proof that a recording contains human data. `technical_demo` is for software/operator checks and cannot be treated as participant results; `pilot` identifies an approved feasibility run and supports process evidence only; `participant_run` is reserved for frozen, approved main-study collection and remains subject to validation. Synthetic or operator markers can downgrade a recording stored in another category to technical/QA-only. The viewer's label does not replace the formal export validation.

## 2. Operator Sequence

1. Before any human session, confirm ethics coverage, consent/withdrawal procedures, protocol version, participant pseudonym and the institution-approved data location. The raw Unity folder below is the current local capture location, not an approved long-term archive. The read-only project/archive destination is not yet specified here; freeze it only after the ethics/institutional data-security gate.
2. Complete `docs/hmd_audio_calibration_record.md`: record headset/build version, headphones/audio interface, volume settings, noise file checksum, speech/non-speech clip checksums, measured notification and masker levels, measurement position/method and operator initials.
3. Open the participant scene with `storageCategory=pilot` or `participant_run`; check that the selected profile has the approved route, speed, lead time, task, input map, counterbalance sequence and questionnaire versions.
4. Run preflight. Any formal-audio, mapping, logger, input or trajectory error ends the run as a protocol deviation; do not repair settings mid-block and continue under the same session label.
5. Brief participant, obtain consent, administer screening and discomfort baseline, and complete neutral VR acclimatisation.
6. Complete practice for the selected visual task, phase response controls and non-speech mapping. Apply the pre-approved learning/stop rule.
7. Once the four-condition order and route allocation have been approved and frozen, complete the four full-flight blocks. Each candidate route presents T01/T02/T03 in flight order; accepted identifications are followed by the configured 1–7 confidence item. Follow the approved profile for breaks and task procedure. The current Methods specifies a post-study interview, not the legacy six-item post-block form. Record route/order/scenery cue use, tutorial replay/help, comprehension, comfort, interruptions, withdrawal and hardware deviations. Additional questionnaires require an explicit construct/method decision.
8. After Unity has created the session directory and before starting the first block, create `run_record.json` from the schema below using the frozen configuration. After post-session measures, interview and debrief, update only `deviation_record`, close the Unity session and verify that the JSONL has exactly one `session_end` event before the raw directory is made read-only.
9. Preserve the raw session directory unchanged. After the approved read-only archive destination has been frozen, copy the complete session folder there and verify the copied files; do not edit JSONL, rename event files, merge sessions or overwrite calibration records.
10. Run `export_audio_v0_events.py` without `--allow-technical`; inspect `validation_report.json` and `sessions.csv`. This normalises and validates logs; it does not fit statistical models or approve analysis.

Before treating a session as a pilot, verify the actual Unity profile and the `storage_category` in its new session folder. At this manual update, `Assets/AudioV0.unity` still serializes `formalStudyMode=false`, `storageCategory=technical_demo`, `participantId=operator_test` and candidate/unfrozen version values; this is technical QA configuration, not a pilot-ready scene. Formal mode requires a pseudonymous participant ID, `pilot` storage, approved non-candidate configuration values and a refreshed manifest before the first block. If those checks are incomplete, stop at technical QA. Do not rename or move a technical log into `pilot` to change its evidence category.

### 2.1 Mac raw-data location

On the current macOS Unity build, `EventLogger` creates one folder per session at:

```text
~/AudioV0_Data/<storage_category>/<session_id>/
```

For a feasibility session, `<storage_category>` is `pilot`; `<session_id>` is generated by Unity and has the form `session_<UTC timestamp>_<unique id>`. Unity automatically creates `event_log.jsonl` and `session_manifest.json` in this folder. The operator creates `run_record.json` there after the folder appears and before the first block. Keep the raw JSONL unchanged. The launcher opens the existing `technical_demo` subfolder; pilot logs are in its sibling `pilot` folder and are also indexed by the viewer.

This is the local capture location, not an approved long-term archive. The institution-approved read-only raw archive and derived-output destinations are still pending the ethics/data-security gate; do not infer either from this local path. Keep consent forms and direct identifiers outside the Unity data tree.

## 3. Minimum Formal-Run Configuration Record

| Field | Required value |
| --- | --- |
| `run_id` | pseudonymous participant/run identifier |
| `storage_category` | `pilot` or `participant_run` |
| `protocol_version` | frozen document/date identifier |
| `stimulus_set_version` | speech/non-speech version plus checksums |
| `noise_version` | file checksum, spectrum/level record and playback setting |
| `route_version` | route geometry, speed, lead time and transition schedule |
| `task_version` | task/UI/input/trajectory/difficulty version |
| `apparatus_version` | HMD, computer, build, refresh rate and audio chain |
| `calibration_record_id` | linked dated calibration sheet |
| `counterbalance_sequence` | pre-generated sequence and route assignment |
| `deviation_record` | `none`, or dated explanation and disposition |

### Required `run_record.json`

Each formal session directory must contain a UTF-8 JSON object named `run_record.json`. First start the configured session so Unity creates its unique `session_...` folder and `session_manifest.json`; before the first block, copy that exact folder ID into `run_id` and create `run_record.json` beside the raw log. Copy values from the approved protocol, profile, apparatus and linked calibration record; do not copy candidate/example values as if frozen. After the session, update only `deviation_record`. This record supports integrity checks but does not replace the immutable event log. The exporter rejects a formal candidate when the file is absent, required values do not match the folder/category/manifest/events, or the event contract fails. Sound, masker and route values remain in events because they can vary across blocks.

```json
{
  "run_id": "session_<exact Unity-generated folder name>",
  "storage_category": "pilot",
  "participant_id": "P001",
  "protocol_version": "<approved version>",
  "stimulus_set_version": "<approved versions and checksums>",
  "noise_version": "<approved version and checksum>",
  "route_version": "<approved full-flight route version>",
  "task_version": "<approved task version>",
  "apparatus_version": "<approved HMD/build/audio-chain version>",
  "calibration_record_id": "CAL_YYYYMMDD_01",
  "counterbalance_sequence": "<approved sequence ID>",
  "deviation_record": "none"
}
```

The values shown are a schema template only. A real session requires approved, internally matching values and the corresponding calibration and freeze records. The current project has not frozen these release fields; do not use this template to imply that a participant run is ready.

## 4. Event Contract

| Table | Source events | Planned use | Required quality condition |
| --- | --- | --- | --- |
| `events.csv` | all raw JSONL records | audit trail | no malformed JSONL; monotonic event sequence within a session |
| `notification_events.csv` | notification onset/request, transition onset, response/timeout | identification accuracy and response time | exactly one notification and one transition event for each planned T01--T03; no negative response time |
| `tracking_samples.csv` | `tracking_sample` | legacy QA only | continuous tracking is not the selected task |
| `visual_events.csv` | `visual_stimulus_onset`, `visual_stimulus_offset`, `visual_response` | GO/WAIT feasibility; hit/miss/false-alarm audit | join by session, attempt, block and stimulus; preserve interrupted/missing offsets |
| `flight_motion_samples.csv` | `flight_motion_sample`, `excerpt_motion_sample` | continuous-route and independent-excerpt quality assurance | preserve recorded positions/velocities and source clocks; no interpolation or invented legacy fields |
| `sessions.csv` | session manifest and log | screening/analysis inclusion audit | formal storage category and pseudonymous participant ID |

`notification_events.csv` preserves missing responses and timeouts. It does not impute an answer, set missing response time to zero or infer auditory audibility from a software playback event.

2026-09-21 measurement alignment: `confidence`, `confidence_status`, and `confidence_response_time_ms` are joined within session/attempt/block/transition, with `parent_event_id` checked against the accepted identification. Status is `response`, `timeout`, `interrupted`, `missing` (opened without closure), or `not_collected` (legacy/no accepted identification). A confidence value never replaces identification RT. Duplicate, out-of-range or mismatched-parent responses are quality flags. Current confidence window is a provisional 8 s. See `measurement_implementation_alignment.md` for the live Overleaf and 9.17 meeting comparison.

2026-09-21 export correction: `notification_onset_ms` and `transition_onset_ms` both use the monotonic session clock. The older exporter incorrectly placed route-clock time in `transition_onset_ms`; that value now has the explicit column `transition_route_time_ms`. `software_lead_s` comes from `notification_requested`, and `playback_observed` requires `is_playing=True`, rather than merely the existence of a playback record. Re-export older logs before comparing these columns.

For one software QA session, use `--allow-technical --session-id session_<exact_id>`. The added visual and motion tables retain `export_eligibility=qa_only`. Synthetic verification inputs carry `synthetic_verification_*` control IDs and are not participant performance. An accelerated run is useful for event closure only; it does not validate normal-speed notification lead, audibility or comfort.

2026-09-23 motion export compatibility: the existing `flight_motion_samples.csv` filename and all legacy columns retain their order and meaning. Both motion event types export recorded `motion_version`, position and `vx_mps/vy_mps/vz_mps`. Appended columns are `event_type`, `event_id`, `event_sequence`, `monotonic_clock_ms`, `excerpt_id`, `excerpt_elapsed_s`, `heading_deg`, `boundary_opacity`, `pre_transition` and `map_visible`. `route_time_ms` and `excerpt_elapsed_s` remain distinct clocks; missing `progress`, `frame_dt_s`, `time_scale` or `planned_duration_s` values remain blank for excerpts rather than being inferred.

Tutorial events use the exact `tutorial_*` event types and remain in `events.csv`. Notification and GO/WAIT derivations accept their explicit study-task event types, so tutorial responses and successes do not enter those tables or block summaries. Technical exports remain `qa_only`; a successful software check is not participant evidence. Focused compatibility/tutorial tests and the closed-session snapshot verification are in `temp/excerpt_motion_export_20260923/`: 1,725 actual excerpt-motion rows matched source fields, 50 tutorial events were retained without changing task tables, and the deliberately paused excerpt remained technically invalid.

### 4.1 Local session review and per-figure exports (2026-09-23)

Open `http://127.0.0.1:8091/` using `tools/打开 Audio V0 数据中心.command`. The launcher starts the local read-only viewer and opens the `technical_demo` folder; pilot sessions are stored in the sibling `~/AudioV0_Data/pilot/` folder. The viewer indexes the three categories under `~/AudioV0_Data/`, reviews one session at a time and supports category, completion, participant/session search, condition, block and route filters. Source JSONL files are never edited.

`Evidence category` identifies the selected session's storage category plus any technical/operator/synthetic markers. `technical_demo` plots are marked `NOT PARTICIPANT RESULTS`; a pilot is labelled feasibility evidence, not a passed analysis; a `participant_run` remains review-pending. A category or label never overrides the session's ethics, protocol, `run_record.json` or export-validation requirements.

Choose an evidence category and session, then use **Condition comparison** for accuracy, correct-response RT, confidence and GO/WAIT, or **Flight recording** for horizontal trajectory and vertical-position timeline plots. Each individual plot has SVG, PNG, PDF, **Cleaned CSV** and **Excluded CSV** links; the timeline also has notification marker events. Use the combined-view export links below the plots for a combined SVG/PNG/PDF or summary CSV. **Event audit** exports the selected filter scope as raw event rows. These are review/download controls; they do not approve data for analysis.

| Plot | Cleaned CSV rows contributing to the plot | Records retained in Excluded CSV |
| --- | --- | --- |
| Accuracy | one technically valid notification trial, including wrong answers and timeouts | interrupted/incomplete/conflicting trials, duplicate or mismatched-parent records |
| Correct RT | one correct accepted identification before the transition, with finite recorded RT | wrong answers, timeouts, missing RT and technically invalid trials |
| Confidence | one valid recorded 1–7 response associated with a valid notification trial | missing/timeout/interrupted confidence and technically invalid trials |
| GO/WAIT | one valid settled opportunity in the current task version; GO/WAIT series is explicit | interrupted, invalid or other-version opportunities |
| Horizontal trajectory | one recorded finite x/z motion sample | samples missing coordinates or with duplicate event identity |
| Vertical-position timeline | one recorded finite world-y/session-clock sample | samples missing position/clock or with duplicate event identity |

Cleaned data are derived event-level rows, not altered source logs. They preserve session/attempt/block/excerpt/target identity, source event IDs, clocks, response status, values, evidence category and filtering scope. `included_in_metric` and `exclusion_reason` document disposition. Missing values stay blank; incorrect answers are retained in the accuracy data. Empty exports still include a CSV header. World-y is not height above local terrain. Different blocks/excerpts are drawn separately rather than connected across resets.

The figures are not a live stream. Selecting a session or changing a filter loads a new immutable in-memory snapshot; its charts, per-figure CSVs and summary use the same source records even while Unity appends events. Click **Reload local recordings** to refresh the session list and selected log, including events recorded since the prior load. Reload after a server restart or a snapshot-expiry error before exporting. The latest 24 snapshots are retained in memory only.

The review layer reuses `export_audio_v0_events.py` for notification, visual and motion joins and adds explicit response-parent/duplicate/late-response flags. It does not replace that script's formal-session validation. Evidence labels use the storage category and technical/operator/synthetic markers; `details.mode=main_study` is not evidence of participant data. Technical charts carry `NOT PARTICIPANT RESULTS`, including their standalone downloads. Pilot and participant-run directories do not themselves establish analysis eligibility.

Implementation: `tools/audio_v0_data_server.py` (read-only HTTP), `tools/audio_v0_review.py` (shared aggregation and cleaned rows), `tools/audio_v0_review_figures.py` (Matplotlib) and the existing `tools/audio_v0_log_viewer.html` interface. This interface is served locally; direct standalone HTML/file-drop review is superseded by the scoped local library. Original UI backups and the 19 aggregation/HTTP tests are in `temp/20260923-viewer-revision/`.

Read-only endpoints: `/api/index`; `/api/review?category=...&session=...&condition=...&block=...&route=...`; `/api/review.csv?table=cleaned|excluded&panel=accuracy|rt|confidence|visual|trajectory|altitude`; `/api/review.svg|png|pdf?kind=<panel>` (or `conditions|motion` for a combined view). Export requests include the same filter parameters and returned `snapshot` token. `table=markers` exports the timeline markers. `summary|trials|visual|motion|events` remain available as broader audit tables.

### 4.2 Formal CSV export and validation

Viewer downloads are for reviewing one session and one selected scope; they do not replace the formal exporter. For a pilot, run the exporter without `--allow-technical`, using a new or empty derived-output directory and the exact Unity session ID:

```sh
python3 /Users/trusoegn/论文2/tools/export_audio_v0_events.py \
  "$HOME/AudioV0_Data" \
  "<new-empty-output-dir>" \
  --session-id "<exact-session_id>"
```

Choose the output directory only after the approved derived-data location has been decided; that location is currently not frozen. The exporter writes `events.csv`, `notification_events.csv`, `tracking_samples.csv`, `visual_events.csv`, `visual_responses.csv`, `flight_motion_samples.csv`, `sessions.csv` and `validation_report.json`. A non-zero exit or entries in `validation_errors` require review before using the export. The report checks session completeness and metadata consistency; it does not perform inferential analysis. `--allow-technical` is only for explicitly identified software QA and marks those records `qa_only`.

## 5. Planned Analysis Boundary

Current scoring and analysis roles follow `measurement_implementation_alignment.md`, section “2026-09-22 正文评分口径”, and live Methods §2.6. This supersedes the older Candidate A/continuous-tracking proposal and does not freeze the full analysis plan.

Identification accuracy includes timeouts as unsuccessful identification and excludes separately reported technical failures. Response-time comparisons use correct first accepted answers within the response window; missing times are not imputed. Responses at or after transition onset are flagged and cannot establish advance identification. Window duration and notification lead remain provisional.

GO/WAIT is the selected activity. Block-level hit and false-alarm rates are descriptive supporting measures, with interrupted or invalid opportunities excluded and zero denominators undefined. They do not isolate sound-only interruption or establish continuous gaze.

The current structure has 12 planned notification events per participant, only three per condition. Precision assessment must jointly consider sample size, event count and the interaction. A binomial mixed-effects model is a candidate, not a frozen formula. Random-effects structure, covariates, RT distribution, exclusions, multiplicity and fallback rules require pre-recruitment specification. Confidence is a study-specific secondary item supporting interpretation of identification. The interview separately explores passenger experience under RQ1. Fixed phase order already permits no-sound prediction; a lack of reported guessing cannot resolve this structural issue. Follow `identification_validity_gate.md` before any sound-identification claim.

## 6. Feasibility Exit Criteria

Use the 2--3 person feasibility pilot to document procedural viability, not to test condition effects, pick a winning notification, or report confirmatory results. After each session, validate the raw record and export, enter its pseudonym/run ID and validation status in `docs/feasibility_pilot_record.md`, and record the observations and any protocol deviation there. Summarise whether to hold, iterate or freeze a candidate only against the predefined exit checks. The pilot does not establish sound-only identification: the complete routes present T01/T02/T03 in order, so scenery and phase order can support guesses. Record reported cue use; do not infer absence of context effects from performance or interview alone.

The pilot should document:

- whether route pacing, optical flow and turns are comfortable in the HMD;
- whether all three speech and non-speech cues are audible in quiet and noise at the calibrated settings;
- whether non-speech learning, visual-task instructions, left/right input mapping and response windows are usable;
- whether visual-task performance avoids obvious floor/ceiling and can be exported for all four conditions;
- whether the per-identification confidence item and post-study interview are understood and tolerable; whether participants rely on phase order or the map;
- whether all logs, calibration links, deviations and session closures are complete.

Any change to pace, audio, task, input, questionnaire, window or route creates a new version. Repeat the affected dry run, update `docs/feasibility_pilot_record.md`, and update `docs/protocol_freeze_record.md` before main-study collection.

## 7. Post-study interview guide (2026-09-23 candidate)

Purpose: explore subjective experience under RQ1, separately from identification effectiveness and confidence. This is a qualitative guide, not a validated questionnaire. Final wording, duration, recording consent and coding procedure remain subject to feasibility review. Conduct after all four conditions with the headset removed; do not teach target mappings or disclose correctness before the interview.

1. “How would you describe your experience of receiving the notifications while doing the visual activity?”
2. “What, if anything, stood out about the spoken notifications and the non-speech sounds?”
3. “How did the notifications fit into the passenger experience? Was anything comfortable, uncomfortable, distracting, or easy to ignore?” Use these prompts only after the open response; do not imply that distraction occurred.
4. “How clear were the notifications, and how easy or difficult were they to hear? Did that change when there was background noise?”
5. “Which design, if either, would you prefer for these routine flight updates, and why? In what circumstances might your preference change?”
6. “When choosing a flight phase, what information did you use?” Then neutrally probe sound, scene, map, motion, order or memory; keep this procedural-validity account separate from experience coding.
7. “Was anything difficult about responding, rating your confidence, or continuing the visual activity? Is there anything else you would change?”

Use a neutral reminder of the participant's actual block order if requested, identifying spoken/non-speech and background-noise context without target answers or evaluative labels. Accept no preference, no difference, uncertainty and inability to recall a specific block. Do not force a general comment into a condition-specific interpretation.

Preserve pseudonym, actual block order, prompt, verbatim response, recording/transcript reference, design/listening context as stated (including unknown), interviewer probes and analytic notes separately. Describe themes with supporting extracts and divergent accounts; do not convert convenience theme counts into comparative effects. Confidence is neither trust nor a substitute for experience. A standard experience construct/tool remains an open design choice in the freeze record.
