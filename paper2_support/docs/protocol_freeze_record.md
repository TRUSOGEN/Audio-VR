# Protocol Freeze Record

Date: 2026-09-23
Status: full-flight journeys, brief tutorial and M1 selected for development; full protocol not frozen. Section 0.7 is the current author decision. Final release requires feasibility pilot, evidence review and recruitment/ethics checks. Empty release fields remain a stop condition for main-study collection.

## 0. Visual-task design decision (2026-09-21)

The user explicitly approved selecting the silent GO/WAIT task after agreeing that task selection and parameter validation are separate decisions. This section supersedes earlier statements that the task type is unselected. It records an author design decision, not evidence of validity.

- Selected: gold GO balls require a tap anywhere in the activity area during their full display period; grey WAIT balls require no tap. No precise timing requirement, pass/fail progression, elimination, cumulative score or additional sound.
- Selected: the activity continues during identification and confidence responses, using separate response controls and separately recorded response times.
- Provisional: exposure and gap duration, GO probability, trajectory/speed, display dimensions, controller mapping, practice, notification/confidence response windows, final scoring definitions and analysis role. Full-exposure GO acceptance is selected; its duration in seconds is not frozen.
- Pending evidence: Quest 3 readability and simultaneous input, sustained visual engagement, difficulty, route-map anticipation, confidence burden and comfort.
- Existing technical_candidate IDs, raw logs and formalStudyMode remain unchanged. This decision does not authorise participant_run collection or recruitment.
- If formative testing shows the activity cannot meet its purpose, reopen this design decision explicitly and record the reason.

## 0.1 Manuscript clarification (2026-09-22)

The 9/22 Methods clarification specifies first-response accuracy, timeout treatment, correct-response RT and descriptive block-level GO/WAIT rates. See `measurement_implementation_alignment.md` for the exact rules. These specifications remain subject to final protocol review; subsequent implementation evidence and author decisions are recorded in §§0.5–0.7. T01 wording has been revised; route inference follows the current context-supported decision. HMD input, acoustic calibration and design-specific precision remain unresolved. All release fields below remain open.

## 0.2 Sample planning preference (2026-09-23)

The author prefers a target of 20 completed adult participants. This confirms the planning target already stated in Methods; it does not establish that 20 provides sufficient precision for the design-by-noise interaction. Before recruitment, assess precision using the final identification procedure, event count, expected accuracy and missing-response scenarios. If 20 cannot support the intended confirmatory claims, revise the claim scope or design and record that choice here. The frozen sample-size rationale and release decision below remain open.

## 0.3 Historical independent-excerpt design candidate (2026-09-23; implementation status superseded below)

At this earlier checkpoint, continuous routes exposed the three targets in a fixed order. The proposed allocation used three independent flight excerpts in each of the four conditions, with the target assigned separately for each excerpt; repeated targets within a condition were permitted. It retained 12 identification events per participant and the planned 20 completed participants. The historical schedule and its limitations are recorded in `../handoff/independent_excerpt_identification_candidate_20260923.md`.

This was then a design candidate without a Unity implementation; §§0.5–0.6 record its later implementation. Its target-independent pre-notification requirements apply to that excerpt procedure, not to the current full-flight design. Historical software completion is bounded by the recorded version; human visual-only validation was not completed. Section 0.7 supersedes the default procedure and its inference scope.

## 0.4 Historical Tram alignment checkpoint (2026-09-23)

Live RQ1 now explicitly explores passenger experience through the post-study interview, independently of identification confidence. RQ2 remains a comparison of accuracy and RT. The interview guide is in `participant_runbook_and_data_dictionary.md` §7; whether a standard experience construct/tool is needed remains an explicit open design decision. No scale has been added or approved by implication.

At this earlier alignment checkpoint, identification validity was on hold because the continuous route permitted ordinal prediction and independent excerpts were not yet implemented. That implementation state has been superseded by §§0.5–0.6. `identification_validity_gate.md` retains the technical and human checks; desktop implementation does not establish human cue independence, HMD comfort or performance. All release fields below remain open.

## 0.5 Historical independent-excerpt implementation (2026-09-23; default superseded by 0.7)

At this checkpoint, the author requested resolution of fixed-order cues and authorised Unity and manuscript changes. The procedure was implemented as `independent_excerpts_v1`: three excerpts per condition, independently sampled targets with replacement, a shared pre-response scene, no route map or phase/progress display, and fixed identification/confidence windows before a post-response illustration. The old pre-generated balanced candidate schedule was not the runtime target generator. The later full-flight decision in §0.7 supersedes this default procedure.

Desktop evidence supports removal of the deterministic ordinal rule, target-independent pre-response position, separate repeated-target records, fixed normal-path timing and explicit invalidation for pause/withdrawal/fault. These are software checks. Blind human prediction, episode credibility, HMD input/readability, acoustic calibration, final sound selection and precision remain open. The manuscript now accurately describes the implementation and these limits; no human validity pass is claimed. The 3-events-per-condition setting, 8 s windows and 18 s software lead are development parameters, not a completed pilot freeze.

## 0.6 Historical curved excerpts and full tutorial (2026-09-23; superseded by 0.7)

`independent_excerpts_v1` now uses motion `excerpt_curved_climb_v2_candidate`: a shared curved climb and path-following heading precede the fixed response/confidence deadlines; target-specific movement follows them. Fades conceal trial resets and pause the visual-task clock while the display is obscured. `participant_tutorial_v1_candidate` provides demonstrations, GO/WAIT, identification/confidence practice and three instruction checks; a ready screen requires an explicit start before the four conditions. These replace the earlier straight-motion and auto-start implementation states.

Unity now loads non-speech `matlab_m1_two_pulse_v1_candidate` and speech `elevenlabs_broadcast_v4_candidate_speed090`, including the revised T01 advance text. M1/V4 are versioned engineering candidates with digital audits, not human-screened or frozen stimuli. The older C0 bank and Broadcast V3 remain development references. Details and actual asset checks are in the [current Unity handoff](/Users/trusoegn/GitHub/Audio-VR/Audio-VR/docs/audio-v0-handoff.md) and [sound-family manual](sound_family_design_and_screening_manual.md).

The complete normal-speed desktop run contains 9 practice identification/confidence pairs, then 12 notification opportunities across four blocks: 8 identification/confidence pairs, 3 identification timeouts and 1 deliberately paused invalid excerpt. Its 1,725 motion samples export without loss; 50 tutorial events remain in the raw table and do not enter task-derived tables. All inputs are synthetic and all exported data are `qa_only`; these counts are software-path evidence, not participant performance. See the [closed-session export verification](../temp/excerpt_motion_export_20260923/runtime_snapshot_verification.json).

Validation is version-bounded: [974 EditMode tests](/Users/trusoegn/GitHub/Audio-VR/Audio-VR/temp/20260923-route-sound-tutorial/all_tests.json) yielded 972 passed, 2 skipped and 0 failed before later editor/UI changes. The [compilation at that checkpoint](/Users/trusoegn/GitHub/Audio-VR/Audio-VR/temp/20260923-route-sound-tutorial/presentation-final/compile_status.json) had 0 errors and 10 existing CS0618 warnings. The [subsequent presentation check](/Users/trusoegn/GitHub/Audio-VR/Audio-VR/temp/20260923-route-sound-tutorial/presentation-final/presentation_audit.json) verified break/final-page layout and restored Welcome/Ready; it is explicitly synthetic presentation, not another full run or full regression.

Human visual-only testing, Quest input/readability/frame time, acoustic onset/SPL/SNR calibration, sound screening, complete feasibility pilot and precision analysis remain open. No formal freeze value or final-release checkbox below is filled by these technical results.

## 0.7 Current full-flight and brief-tutorial decision (2026-09-23; no release freeze)

The user requested four complete journeys with ground take-off, cruise, visible left and right turns, descent and ground landing, and rejected the long correction-until-correct tutorial. The default now uses `context_supported_full_flight_v1_candidate` and the retimed `full_city_journeys_v5_210s_candidate`; independent excerpts remain a historical development mode. The engineering target is at most 210 seconds per route, pending a normal-clock desktop rerun and feasibility-pilot review. The earlier 4.5–4.7 minute run in this section is a superseded timing record, not the current duration target.

The author then decided to include direction-matched left/right turn cues in the current four-condition study. The cue is shared by all four cells, logged as `spatial_turn_cue`, and delivered from the corresponding side. It is a common journey feature and pilot/usability check, not a third factor and not a T01--T03 response target. A formal run must keep its spatial delivery settings constant across conditions and record them in the calibration manifest.

Each route presents T01, T02 and T03 in flight order. The four-condition comparison concerns notification judgements supported by the flight context. Route progress, scenery and ordinal position can help predict the target; therefore accuracy and RT cannot establish sound-only identification or the absence of visual/order cues. Preserve this `context_supported` boundary in the manuscript, session metadata, analysis and figures. Route-condition allocation, missing-response rules, comfort and precision require review for the final procedure.

`participant_tutorial_quick_v2_candidate` provides six labelled sound demonstrations, two GO/WAIT opportunities, one identification/confidence response and one instruction check. Incorrect responses receive an explanation and can progress; optional replay remains available. The Ready screen requires an explicit start of the four routes. These are onboarding choices, not a validated learning criterion; pilot observations must record extra replay, help and comprehension. Tutorial data remain practice-only.

The [current runtime audit](/Users/trusoegn/GitHub/Audio-VR/Audio-VR/temp/20260923-full-route-revision/runtime_audit.json) passed on a closed synthetic log: 7,634 events, 5,485 motion samples, 12 notifications and transitions, 11 identification/confidence responses and one deliberate timeout. The tutorial deliberately answered the identification and instruction questions incorrectly once each and still reached Ready; its scripted start-to-Ready time was 30.444 s, not a measured human training time. Logged route-clock durations total 1,109.197 s (about 18.49 min); the kinematic model predicts 1,109.259604 s. The [runtime status](/Users/trusoegn/GitHub/Audio-VR/Audio-VR/temp/20260923-full-route-revision/runtime_status.json) records 1,112.533 s of total four-route QA including short pause/block transitions. These are not full participant-session durations.

Observed software notification lead was 17.995–18.001 s. All routes returned to ground-root height and zero endpoint velocity. The [pause probe](/Users/trusoegn/GitHub/Audio-VR/Audio-VR/temp/20260923-full-route-revision/pause_probe.json) exercised the participant pause button between notifications and verified held position followed by resumption; it did not test interruption during an open identification/confidence window. The [geometry audit](/Users/trusoegn/GitHub/Audio-VR/Audio-VR/temp/20260923-full-route-revision/geometry_audit.json) reports 4,018 sampled building-clearance checks with no collisions or geometry repairs; it does not establish continuous collision safety, aircraft realism or headset comfort. All data remain synthetic technical evidence.

The user selected M1 (`matlab_m1_two_pulse_v1_candidate`) for the current engineering build and prospective pilot, with V4 speech retained. M2, C0 and previous speech versions remain traceable references. Selection does not assert superior discrimination, low urgency, equal perceived loudness, completed calibration or a main-study freeze. The MATLAB waveform/FFT/STFT figure and original-source rationale are in `../sound_design/tram_revision_matlab_v1/paper_figure/`; they describe digital structure only.

Quest checks, human listening, calibration, complete feasibility pilot, final analysis and sample precision remain open. An additional randomised sound-only assessment is needed only if the study seeks independent acoustic-identification claims; full-route performance cannot substitute for that evidence. All release fields below remain unfilled.

## 1. Freeze decision

| Field | Entry |
| --- | --- |
| Protocol version | |
| Freeze date and decision maker | |
| Feasibility pilot record reviewed | |
| Ethics approval / amendment and consent version checked | |
| Recruitment and data-handling approval checked | |
| Main-study storage category | `participant_run` |
| Decision | `not frozen` / `freeze approved` / `hold` |
| Rationale and unresolved issues | |

## 2. Frozen implementation manifest

Every item requires a version, immutable reference/checksum where applicable, and an evidence link.

| Domain | Frozen value | Version / checksum | Evidence link |
| --- | --- | --- | --- |
| Unity build, scene and application version | | | |
| HMD, connection mode, computer and refresh rate | | | |
| Audio output path and spatial delivery configuration | | | |
| Speech clips, wording and voice identity | | | |
| Non-speech clips and learning materials | | | |
| Masker provenance, spectrum, level and SNR procedure | | | |
| Calibration record ID and measurement limitations | | | |
| Route geometry, motion speed, lead time and transition schedule | | | |
| Condition and route counterbalance schedule | | | |
| Visual task, trajectories, controls and scoring window | | | |
| Identification mapping, timeout and confidence item | | | |
| Questionnaires, anchors, timing and scoring | | | |
| Data schema, run-record schema and export script version | | | |

## 3. Frozen analysis plan

| Item | Specification |
| --- | --- |
| Primary outcome(s) and estimand(s) | |
| Secondary / exploratory outcomes | |
| Correct, incorrect and timeout coding | |
| Response-time eligibility and transformation/distribution check | |
| GO/WAIT denominators and missing-data handling | |
| Inclusion, exclusion, withdrawal and protocol-deviation rules | |
| Mixed-model formulae, covariates and convergence fallback | |
| Multiplicity and contrast plan | |
| Sample-size / precision rationale and simulation version | |
| Reporting boundary for pilot and technical logs | |

## 4. Final release checks

| Gate | Pass / fail | Evidence |
| --- | --- | --- |
| Four-condition desktop technical run complete and audited | | |
| Four-condition HMD run complete and audited | | |
| HMD/audio calibration record complete | | |
| 2--3 person feasibility pilot completed without unresolved critical issue | | |
| Formal-mode guard, formal audio policy and participant-view checks pass | | |
| Exporter rejects incomplete/technical data and accepts the frozen schema | | |
| Manuscript Methods matches the frozen implementation only | | |
| Recruitment materials match the approved protocol | | |

Only when every final-release check is passed and the decision is `freeze approved` may a session use `storage_category=participant_run`. A later parameter change reopens this record, increments the protocol version, and requires the affected dry run and documentation review again.
