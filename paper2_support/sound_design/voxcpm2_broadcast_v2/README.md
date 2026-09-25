# VoxCPM2 Broadcast Voice Candidates V2

Status: rejected after the researcher's human audition on 2026-09-18. The selected female candidate was still insufficiently professional and broadcast-like. These files are retained only as an audit record and must not be loaded into Unity or used as candidate stimuli.

## Purpose

V1 was rejected after human audition because its voice was insufficiently broadcast-like. V2 keeps the message, output format, digital level treatment, and model family controlled while varying only the proposed speaker persona for T01.

| Candidate | Intended persona | Seed |
| --- | --- | ---: |
| `female_broadcast` | Mature, formal Australian-English female cabin announcer | 2417 |
| `male_broadcast` | Mature, formal Australian-English male cabin announcer | 3841 |

The choice is a production decision. It does not create a voice-gender experimental condition. Once one identity is selected after human audition, all three messages will be regenerated from one fixed persona contract and then checked for consistency before Unity integration.

## Controlled Generation

- Provider/runtime: local `openbmb/VoxCPM2` inference through `llama.cpp-omni`.
- Message: `Flight update. Cruising altitude reached.`
- Output: raw 48 kHz mono WAV, followed by terminal-silence trimming, a 10 ms fade-out, 20 ms trailing silence, and digital RMS matching to 0.025.
- Shared settings: CFG 2.0, CFM timesteps 12, temperature 0.85.

The audio must be assessed by human listening for broadcast quality, naturalness, pronunciation, absence of artifacts, and appropriateness. File inspection only validates the technical format. Apparent age, gender, accent, HMD intelligibility, SPL/SNR, spatial delivery, and participant acceptability remain pilot checks.

## Generated Asset Record

Generation date: 2026-09-18. The local CPU runtime completed both direct WAV exports. The following checks confirm a valid PCM-16, mono, 48 kHz file with finite sample values and a shared digital RMS. They do not assess perceived broadcast quality or actual playback.

| Asset | Duration (s) | Peak | RMS | Leading non-silent onset (ms) | SHA-256 |
| --- | ---: | ---: | ---: | ---: | --- |
| `T01_female_broadcast.wav` | 3.702917 | 0.270233 | 0.025000 | 0.000 | `574b114afbf32880a6f2c18f0c781753478842e4fdcf6ed2a1cb1c1d79750ab9` |
| `T01_male_broadcast.wav` | 3.011271 | 0.173340 | 0.025000 | 15.250 | `e63639c27fd6c57dba9d991316782cda0d3d271769024875bec7f2b44da9e4e2` |

No V2 WAV has been copied to Unity, played through a headset, mixed with simulated cabin noise, SPL/SNR calibrated, or used with a participant.
