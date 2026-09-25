# Audio Experiment V0 Protocol and Prototype Checklist

## Purpose and boundary

This document turns the current 2×2 Audio Experiment into a testable Unity/VR V0. It is an implementation and pilot-planning document, not a frozen recruitment protocol. The research contribution is the notification design; Unity/VR is the study medium. All fields labelled `prototype candidate` require pilot evidence before they enter the final protocol.

Current decision (2026-09-24): [`protocol_freeze_record.md` §0.7](protocol_freeze_record.md) controls the four complete ground-to-ground journeys, short tutorial and M1 selection. The default is `context_supported_full_flight_v1_candidate`, replacing independent excerpts. The retimed route candidate targets no more than 210 seconds (3:30) per route; normal-clock rerun and pilot confirmation remain open. Every route includes the same direction-matched left/right turn cue in all four cells, logged for pilot/usability review but held constant and excluded from the T01--T03 notification denominator. Current normal-clock synthetic QA completed four routes, 12 notifications/transitions and 5,485 motion samples; detailed scope and evidence are in §0.7. Main-task accuracy/RT are context-supported judgements, not sound-only identification. Historical notes and software QA do not establish headset/pilot readiness; the pause probe only exercised notification-free intervals.

The current V0 implementation scope is four within-subject conditions: non-speech sound or short speech, each in quiet or simulated cabin noise. Each participant-facing block contains three routine transitions: entering cruise, beginning descent, and preparing to land. The current build has no no-notification condition and no catch windows. These are build constraints for V0, not a replacement for the final protocol freeze.

The following stay changeable until the pilot freeze: route variants and their allocation, exact simulation-state thresholds, speech copy, cue assets, noise mix, visual-task parameters, controller bindings, trial timing, scoring, and any formal study parameter. A change to one of these items requires a new version in the session manifest; it must not silently alter an existing condition.

## What the V0 must prove

The V0 must show that one complete participant session can be run and audited. It must verify the following before a pilot:

- A route can trigger the three target transitions from logged simulation states.
- The assigned notification is played once at each event with an identifiable timestamp.
- A participant can continue a visual task, submit a transition identification, and complete the block without input conflicts.
- The application records every required event in an exportable log.
- Quiet and noise conditions use the same playback chain and can be selected reproducibly.

The V0 does not establish that a cue works, that a sound level is comfortable, that the sample target is adequate, or that the final device is suitable for participant sessions.

## V0 session structure

| Stage | V0 behaviour | Status |
| --- | --- | --- |
| Preparation | Consent and formal screening are represented by a test operator checklist only. | Prototype candidate |
| Acclimatisation | A neutral scene and a short controller check run before the first block. | Prototype candidate |
| Practice | Six labelled sound demonstrations, two GO/WAIT items, one identification/confidence item and one check; errors receive explanations and may continue, with optional replay. Ready requires explicit start. | Current candidate; learning sufficiency unvalidated |
| Experimental block | One route, one 2×2 condition, three target transitions, visual task throughout, then post-block placeholder screen. | Required |
| Break and next block | The operator records completion and loads the assigned next condition and route. | Required |
| Exit | The application writes a session manifest and log before closing. | Required |

The first smoke test may use one route and one condition. The integrated V0 must expose all four conditions before the end-to-end pilot. Route variants and their joint allocation are not yet frozen.

## Transition and notification design

| Target transition | Operational definition required before cue production | Speech V0 text | Non-speech V0 requirement |
| --- | --- | --- | --- |
| Entering cruise | Advance notification for the logged cruise boundary. | “Flight update. We will enter cruise shortly.” | M1 rising contour, 330–495 Hz. |
| Beginning descent | Advance notification for the logged descent boundary. | “Flight update. Descent will begin shortly.” | M1 falling contour, 495–330 Hz. |
| Preparing to land | Advance notification for the logged landing-preparation boundary. | “Flight update. Prepare for landing.” | M1 two separated pulses, 392 Hz. |

Before implementing a route, the developer must fill a versioned route-state manifest for each transition with: `state_source`, `entry_state_id`, `entry_predicate`, `exit_state_id` or route-end predicate, `allowed_predecessor_state_id`, `route_time_reference`, and `interruption_path`. A transition fires only when its entry predicate becomes true after its allowed predecessor; it receives one `transition_id` and cannot fire again in the same block. Route restart, pause/resume, missing state, and a state-order violation must create a logged interruption or error, never an unlogged replay. The exact predicate values remain V0 configuration rather than frozen study parameters.

The speech strings are V0 wording, not final passenger copy. The sound designer must keep a stimulus register containing source files, export settings, duration, intended meaning, shared family features, distinguishing features, version, author, and revision reason. The V0 may use a technical test clip only to prove audio playback; it must be labelled `technical only` and never evaluated as a study notification.

Design goals for the non-speech set are recognisable shared identity, distinct transition meanings after training, ordinary rather than emergency-like character, and audibility under the V0 noise condition. The direction-matched turn cue is shared journey delivery across all four cells; do not treat it as an isolated spatialisation contrast or third factor. The study compares complete notification designs.

For V0, the audio manifest must state when simulated cabin noise starts and stops, whether it continues across transitions, the target notification asset readiness state, and the playback-complete callback expected from the audio layer. A missing callback, muted output, or audio-device change during a block is an error event, not evidence that a notification was delivered. Mix levels and calibration values remain pilot-dependent.

## Apparatus and input design

Quest 3 is the current intended headset for device checks; older Quest 2 records are prototype history. The final computer, output chain, calibration method, virtual seat position and head-movement setting require actual device evidence. Asset Store or GitHub environments require source-and-licence entries in the asset register.

The response and visual-task controls must not require the same action at the same moment. Implement controls through configurable bindings, not hard-coded meanings, so the pilot can test alternatives. Record the selected binding in the session manifest. Before a block, the control map must define separate bindings for `identification`, `visual_task_response`, and `pause`; a system/menu action cannot also be a study response. Focus priority is: system safety/menu, then pause, then identification while its response panel is open, then visual-task response. For inputs arriving in the same frame, accept only the higher-priority input; apply a versioned debounce and long-press rule; log both accepted and rejected inputs with the reason. The final mapping and timing are pilot-dependent.

The selected visual-task type is silent GO/WAIT: tap the activity area while the gold GO ball is visible and withhold taps for grey WAIT. It continues during identification and confidence with separate controls. Parameters, timing, input and scoring remain subject to device/pilot checks; the older peripheral icon task is historical. Log every scored opportunity, response and interruption, including pauses while the view is obscured. The task has no cumulative score, elimination or added sound.

## Condition controller

The condition controller must set and write these values before each block:

| Field | Allowed V0 values |
| --- | --- |
| `notification_design` | `non_speech`, `speech` |
| `listening_condition` | `quiet`, `simulated_cabin_noise` |
| `condition_id` | `NS_Q`, `NS_N`, `S_Q`, or `S_N` |
| `route_id` | versioned route identifier |
| `block_position` | 1 to 4 |
| `stimulus_set_version` | versioned cue and phrase set |
| `noise_version` | source and mix version, or `quiet` |
| `control_binding_version` | versioned controller mapping |

The application must create a block-to-condition table before the first block. For V0 it may be operator-selected, but it must contain each of the four `condition_id` values exactly once for a full-session dry run, prevent duplicate or missing assignments, and log any operator override with its reason. The formal counterbalancing schedule remains pilot-dependent.

The application must prevent an unlabelled block from starting. It must never silently substitute a missing cue, route, noise file, audio device, or log path. Before block start it must validate those dependencies; on failure it must display a stable error code, stop the block, record whether retry is allowed, and preserve any partial log with an incomplete-session status.

## Minimum event log

Each exported event row must include `session_id`, pseudonymous `participant_id`, `block_id`, `event_id`, `event_sequence`, `route_id`, `block_position`, `condition_id`, `notification_design`, `listening_condition`, `transition_target`, `transition_id`, `event_type`, `timestamp_ms`, `monotonic_clock_ms`, `stimulus_set_version`, `application_version`, and `write_status`. The session manifest must name the clock source. Events related to one transition share its `transition_id`; an input response records a `parent_event_id` when it responds to a specific notification or visual-task stimulus.

Record these event types:

| Event type | Required additional fields |
| --- | --- |
| `block_started` / `block_completed` | operator identifier or test mode; selected condition fields |
| `transition_onset` | logged simulation state and route time |
| `notification_onset` / `notification_offset` | cue or speech asset ID; audio playback device; requested start, actual playback callback result, and planned lead time |
| `identification_submitted` | selected target; correct/incorrect after matching to communicated target; response-control ID |
| `visual_task_stimulus` | target/non-target label; overlay state |
| `visual_task_response` | response-control ID; correct/incorrect after matching to the stimulus |
| `input_accepted` / `input_rejected` | control ID; action; focus state; rejection reason if applicable |
| `pause`, `withdrawal`, `error` | stable error code; reason; recoverable/non-recoverable status; retry result; partial-log location |

Use separate timestamps for transition onset, notification onset, and submitted identification. The final fixed visual-task scoring interval and treatment of late, missing, or incorrect identification responses remain to be decided after prototype testing.

## Build sequence

1. Create one repeatable route and log the three transition states without any study cue.
2. Add a technical audio playback test and verify onset/offset logging through an exported file.
3. Add the V0 speech and non-speech stimulus sets and the quiet/noise switch.
4. Add configurable identification controls and the visual-task candidate, then verify simultaneous use, simultaneous-frame priority, debounce, and rejected-input logging.
5. Add condition manifest checks, error states, and a session export check.
6. Add the four-condition allocation table, duplicate/missing-condition checks, and route-version identifiers.
7. Complete a full operator dry run before inviting any pilot participant.

## Dry-run acceptance checklist

- [ ] The Quest 3 candidate opens, renders the complete routes, and accepts the required controller inputs.
- [ ] Each of the three transitions is triggered once from a recorded state, not from an unlogged timer; the route-state manifest, state order, and any interruption are exportable.
- [ ] Every notification plays its assigned asset and produces an onset and offset log event.
- [ ] Quiet and noise are distinguishable in the controller and recorded in the manifest; the four-condition table completes each `condition_id` once with no silent duplicate or omission.
- [ ] Visual-task events and identification responses are both recorded while the route continues; a simultaneous-input test confirms priority, debounce, and accepted/rejected input logs.
- [ ] An intentional missing asset, audio-device, or log-path error stops the block with a visible error code, retry rule, and preserved partial log.
- [ ] The exported log contains all required fields, can be opened, matches the operator's condition record, and links transition, playback, and response events through IDs and event order.
- [ ] The application records a clean block completion, pause, and error pathway.
- [ ] Environment assets and third-party sound resources have provenance and licence records.

## Pilot plan and freeze gate

Run two to three end-to-end pilot sessions only after every dry-run item passes. Jeremy may be invited if available, but this is not a committed participant or data source. The pilot checks instructions, sound learning, speech wording, audibility, comfort, startle, motion sickness, visual-task difficulty, route timing, logging completeness, block duration, fatigue, and obvious floor or ceiling performance. The operator must retain one completed pilot checklist per session, identify the reviewer, and record a `freeze`, `iterate`, or `block` decision with its evidence. The final numerical thresholds for task variation, log completeness, and comfort are set before the pilot, not inferred afterwards; two to three sessions are feasibility checks, not inferential evidence.

Before recruitment, version and freeze the final stimuli, noise and calibration chain, visual task, control bindings, training criterion, response window, scoring, route-condition allocation, exclusion and replacement rules, questionnaire wording and anchors, and analysis specification. Then perform the sample-size or precision assessment on the final event structure. The current target of 20 completed participants is not a completed justification.

## Evidence to retain

Keep the Unity version, package list, scene and route versions, asset register, stimulus register, calibration record, session manifest, raw event logs, dry-run results, pilot feedback, and an explicit change log. These records are needed to explain the Methods and to distinguish verified prototype behaviour from planned study parameters.
