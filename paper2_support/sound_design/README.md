# Audio Stimulus Design Library

This directory is the working record for Paper #2 notification stimuli. It is separate from Unity delivery code and from the formal study protocol.

- `audio_stimulus_design_v1.md` preserves early stimulus-design history; current candidates and validation boundaries are maintained in the directories and manual below.
- `../docs/sound_family_design_and_screening_manual.md` is the end-to-end guide for candidate-family development, screening, calibration, Unity integration, and protocol freeze.
- `azure_speech_v1/` contains the fixed candidate SSML and an opt-in generator for the provisional speech files.
- `tram_revision_matlab_v1/` contains two complete 1.80 s non-speech candidates, unchanged C0 references, executed MATLAB generators, WAV checksums and audition materials. The user selected M1 for the engineering build and prospective pilot; human listening and calibration remain pending. `paper_figure/` contains the verified three-sound waveform/FFT/STFT figure and source-grounded rationale; the figure describes signal structure, not listener effects.
- `elevenlabs_broadcast_v4_candidate/` contains three newly generated speech candidates with the same existing voice/model, speed UI 0.90, and T01 advance wording. MATLAB decoded all V3/V4 files for waveform-level audit; `audition.html` compares the unchanged raw exports by target. Human audition and calibration remain pending.
- Unity-ready assets belong in `Audio-VR/Assets/Resources/AudioV0Stimuli/` only after their file integrity and provenance have been recorded here.
- The current M1/V4 full-flight desktop QA completed all four routes and 12 notifications; see `../docs/protocol_freeze_record.md` §0.7 for the closed-log evidence and limits. This synthetic run does not establish human audibility, headset calibration or learning sufficiency.

Nothing in this directory freezes a main-study stimulus, playback level, SNR, spatialisation setting, or participant procedure.
