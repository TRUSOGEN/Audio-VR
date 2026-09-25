# VoxCPM2 Pilot Speech Stimuli V1

Status: rejected after the researcher's human audition on 2026-09-18. These files are retained only as an audit record and must not be loaded into Unity or used as candidate stimuli. See `../voxcpm2_broadcast_v2/` for the replacement audition candidates.

## Persona Decision

The target persona is a clear, natural, low-arousal adult Australian English informational voice at a moderate pace. It is deliberately not specified as male, female, young, or old. The prompt asks for medium pitch and an adult voice but cannot guarantee how listeners perceive gender or age. Those perceptions are possible confounds and must be checked in the feasibility pilot rather than assumed from the generation prompt.

This is a controlled production choice, not a voice-gender or voice-age manipulation. The same prompt, seed, sampling parameters, text framing, output format, and post-processing status apply to all three messages. The speech/non-speech comparison remains a complete notification-design comparison.

## Evidence Boundary

Zhang, Yang, and Robert (2025, Scientific Reports, DOI: 10.1038/s41598-025-00884-9) found that voice-gender similarity and gender-role congruity can affect trust in an online automated-vehicle explanation study. The authors recommend customizable or gender-neutral options rather than assuming a universal optimal male or female voice. It is adjacent evidence, not a UAM passenger result.

Schreibelmayr et al. (2022, Frontiers in Psychology, DOI: 10.3389/fpsyg.2022.787499) found in a laboratory study of five female-sounding robot voices that higher vocal human-likeness was associated with more pleasantness, less eeriness, and acceptance; information and navigation were among the assessed application contexts. It does not establish a preferred gender, age, accent, level, or UAM voice persona.

Wang et al. (2022, AutomotiveUI, DOI: 10.1145/3543174.3546830) found conversational agents were generally preferred and associated with better takeover-quality measures in a conditionally automated-driving simulator. This supports concise, intelligible speech as a design consideration but does not validate this wording, voice, or UAM experiment.

## Generation Contract

Provider: VoxCPM2 local inference through `llama.cpp-omni` at commit `64d092c60db4b4ee45768476bd752f03fdcc98ea`.

Runtime: `voxcpm2-cli`, Metal backend compiled on Apple Silicon. Model family: `openbmb/VoxCPM2`; the Q8 GGUF adapter repository is linked from the official VoxCPM README. The model and code are described by OpenBMB as Apache-2.0. The local model files used for this generation are temporary dependencies and are not bundled with this project.

| Item | Value |
| --- | --- |
| Voice description | `A neutral adult Australian English voice, medium pitch, clear and natural, calm professional informational tone, moderate pace, low emotional arousal, concise delivery, no urgency, no smile, no breathiness, no dramatic emphasis.` |
| Seed | 1701 |
| CFG | 2.0 |
| CFM timesteps | 10 |
| Temperature | 1.0 |
| Processing | Raw direct WAV export, then common tail-silence removal, 10 ms fade-out, 20 ms trailing silence, and digital RMS matching to 0.025 |
| T01 | `Flight update. Cruising altitude reached.` |
| T02 | `Flight update. Descent will begin shortly.` |
| T03 | `Flight update. Prepare for landing.` |

Run `generate_voxcpm2_pilot_v1.sh` only with explicit paths supplied through `VOXCPM2_CLI`, `VOXCPM2_BASE_MODEL`, and `VOXCPM2_ACOUSTIC_MODEL`. It exports raw WAV files only. Then run `prepare_pilot_wav.py` from an environment containing `numpy` and `soundfile` to produce the final pilot WAV files. The raw assets are retained for audit.

## Generated Asset Record

Generation date: 2026-09-18. CPU-only execution was used because the interactive environment could not create an Apple Metal command queue. This affects generation speed, not the target WAV format. The direct exports used VoxCPM2's 48 kHz WAV output and were then processed by `prepare_pilot_wav.py`; no content edits, trimming of speech onset, denoising, equalisation, compression, or change of sample rate was applied.

| Asset | Duration (s) | Peak | RMS | Tail removed (s) | Gain | SHA-256 |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| `T01_VoxCPM2_pilot_v1.wav` | 2.579938 | 0.270538 | 0.025000 | 0.000063 | 1.075392 | `e053821e8fbb4207da9f8c36746694f602a67d8c07259de9682d6778c60481ca` |
| `T02_VoxCPM2_pilot_v1.wav` | 3.158833 | 0.206909 | 0.025000 | 0.061167 | 1.575081 | `492ee8334bab25f6b6dffa25116fb2e055acb044f69cff70b32ffde332178218` |
| `T03_VoxCPM2_pilot_v1.wav` | 2.866771 | 0.220367 | 0.025000 | 0.193229 | 0.728840 | `a5a04a4eb1951a461a9d3b93b36fb9315d8a450835b6be989414c62661ce5a40` |

The final files are PCM signed 16-bit, mono, 48 kHz, finite-valued, and have no material leading silence. The final files have not been human-auditioned, transcribed by an independent recognizer, loaded into Unity, played with cabin noise, HMD-tested, or calibrated. A file-validity check is not evidence that the intended wording, accent, voice consistency, intelligibility, comfort, or spatial delivery has passed.

## Required Pilot Checks

1. Confirm every file contains the intended wording, has no clipping or accidental silence, and retains an acceptably consistent persona across T01--T03.
2. Confirm the apparent gender and age are not materially distracting or systematically different across files; record the exact question and response pattern as a pilot check, not a confirmatory outcome.
3. Compare the quiet and simulated-noise playback through the selected HMD. Measure the actual notification level and SNR with the chosen calibration chain.
4. Apply one documented loudness/duration policy across speech and non-speech designs only after pilot evidence. Do not normalize from Unity volume alone.
5. Load speech clips through explicit condition-to-asset mapping, then run and audition all four conditions before any participant pilot.
