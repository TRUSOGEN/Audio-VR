# Paper #2 Formal Data Integrity and Pilot Records Handoff

Date: 2026-09-18

## Scope

This handoff continues the Audio V0 technical-to-pilot preparation. It records implementation and documentation changes only. No human audition, HMD run, calibration, feasibility participant, formal participant session, or thesis result was created.

## Completed and Verified

1. Corrected the flight-motion handoff: the normal-speed desktop technical QA measured 76.739--79.219 s from route start to completion, superseding the earlier 71--73 s distance/speed approximation. The saved profile is still the candidate 18 m/s, 4 s lead time and 12 deg/s heading.
2. Updated the stimulus register with the actual V3 speech files mapped by Unity: 44.1 kHz mono MP3 at 128 kbps; T01 2.351 s, T02 2.508 s and T03 2.116 s; each has a SHA-256 checksum. Loudness, true peak, acoustic calibration and human audition remain unmeasured.
3. Extended `EventLogger` so every event and refreshed session manifest record protocol, task, apparatus, calibration-record and counterbalance versions. Formal mode now rejects an incomplete, missing or candidate/provisional configuration with `FORMAL_RUN_CONFIGURATION_INCOMPLETE` before a block starts. It also rejects a manifest refresh failure.
4. Extended `tools/export_audio_v0_events.py`. A formal `pilot` or `participant_run` now requires `run_record.json`, a matching session directory/category and refreshed static manifest, consistent static fields in every raw event, non-empty dynamic stimulus/noise/route versions, one session closure, 12 notification and transition onsets, 3 notification onsets per condition, and no error or technical marker. Dynamic sound/noise/route values intentionally vary across the four blocks and are retained event by event.
5. Added fillable records:
   - `docs/hmd_audio_calibration_record.md`
   - `docs/feasibility_pilot_record.md`
   - `docs/protocol_freeze_record.md`
6. UnitySkills forced a source recompilation after these code changes: 0 errors and 0 warnings. The first targeted test attempt was blocked because the Editor was in Play Mode; after stopping the unsaved technical Play Mode, discovery found the test class and `AudioV0.Tests.AudioV0ExperimentDefaultsTests` passed 15/15. The Python exporter compiled successfully; an in-memory validator check accepted valid four-condition dynamic sound/noise/route records and rejected a missing `run_record.json`.

## Current Readiness Boundary

The system is more resistant to wrongly labelled data, but it is not protocol-frozen or recruitment-ready. The desktop technical QA remains `technical_demo` with `operator_test`; no item from it may enter a pilot summary, statistical analysis or manuscript Results.

The formal-mode guard cannot replace pilot evidence. It only ensures that a future formal-label attempt fails unless a frozen-looking audit configuration is present. Human study approval, actual apparatus selection, HMD routing, at-ear measurement, noise mix, visual-task choice, questionnaire set, training, response window, sample-size/precision rationale and analysis model are still open gates.

## Next Gate Order

1. Complete a desktop human full-set audition, recording speech wording/persona consistency and quiet/noise audibility without treating it as participant evidence.
2. Complete `hmd_audio_calibration_record.md` on the selected HMD/headphone chain; record the measurement method and limitations rather than estimating SPL/SNR.
3. Run the four-condition HMD technical dry run with a participant-view capture, formal audio-policy check, input-concurrency check and exported log audit.
4. After ethics/consent confirmation, run the 2--3 person feasibility pilot and complete `feasibility_pilot_record.md`.
5. Only after all feasibility decisions, sample-size/precision work and recruitment checks are complete, fill and approve `protocol_freeze_record.md`, update Methods to match the frozen version, and permit `participant_run` collection.

## Canonical Files

- Unity source: `/Users/trusoegn/GitHub/Audio-VR/Audio-VR`
- Paper workspace: `/Users/trusoegn/论文2`
- Current motion/data handoff: `handoff/flight_motion_and_paper_data_readiness_20260918.md`
- This handoff: `handoff/formal_data_integrity_and_pilot_records_20260918.md`
