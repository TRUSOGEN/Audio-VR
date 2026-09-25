# Professional Broadcast Voice V3

Status: three provider raw exports generated and technically verified at the metadata level. These are pilot-candidate assets only; Unity import, playback, calibration, and feasibility checks remain open.

## Production Decision

Use the selected licensed female ElevenLabs Voice Design identity rather than further VoxCPM prompt-only generation. The target is a professional, studio-quality Australian-English aviation cabin-announcement delivery, not a friendly virtual assistant or an actor's character voice.

The voice remains one fixed production identity across T01, T02, and T03. Perceived speaker gender and age are not experimental factors.

## Actual Export Record

The researcher selected the second Voice Design candidate and accepted the voice identity. The provider voice is `Paper2_AU_Female_Broadcast_V3` (`voice_id: to2uGLddpS3srynUCVAr`). The three final raw exports were generated interactively on 2026-09-18 with the same identity and settings:

| Field | Actual setting |
| --- | --- |
| Provider/model | ElevenLabs / Eleven Multilingual v2 |
| Voice | `Paper2_AU_Female_Broadcast_V3` / `to2uGLddpS3srynUCVAr` |
| Speed | UI value `1.00` (the provider download filenames encode `sp99`) |
| Stability | `0.50` |
| Similarity | `0.75` |
| Style exaggeration | `0.00` |
| Speaker boost | enabled |
| Output | mono MP3, 44.1 kHz, 128 kbps |

The researcher rejected a speed-`0.75` T01 take as too slow. It is retained only as an audit record at `rejected/T01_cruising_altitude_speed075_rejected.mp3`; it is not a final candidate and must not enter Unity.

Final provider raw exports:

| ID | File | Duration (s) | SHA-256 |
| --- | --- | ---: | --- |
| T01 | `raw/T01_cruising_altitude_raw.mp3` | 2.351000 | `09cddda0a3555a7421c7e048144eefc412a2da47b97bcbffffe32ea356261e13` |
| T02 | `raw/T02_descent_raw.mp3` | 2.507750 | `faa43a45ea4d2137f8246e93fc4d1fc95f2aa5a38c557ebe336eeeb4adc583a2` |
| T03 | `raw/T03_landing_raw.mp3` | 2.115875 | `a8350a46455f1aca41bc5b6a59214ac7cde2ff129af381326d41bb99c1c9950a` |

`raw_analysis.json` is the machine-readable metadata audit. It confirms nonzero duration, mono, 44.1 kHz, and 128 kbps for all three files. Peak/RMS and silence-boundary measurements are deliberately marked pending because this machine has no validated local MP3 decoder; no number was inferred from metadata.

2026-09-23 update: the above pending fields describe the original metadata-only audit. MATLAB R2024b `audioread` has now decoded these three unchanged V3 files alongside V4; actual RMS, peak, threshold-defined silence boundaries and hashes are in `../elevenlabs_broadcast_v4_candidate/waveform_audit.json`. The original `raw_analysis.json` remains an historical record; listener validation and calibration are still pending.

## Voice-Design Prompt

```text
Professional female aviation cabin-announcement broadcaster, approximately 35 to 45 years old, contemporary Australian English, polished studio recording. A resonant low-to-mid pitch with firm but reassuring authority. Broadcast diction with clean consonants, deliberate phrase boundaries, a measured pace, stable breath support, and controlled sentence endings. Formal public-address delivery for calm routine eVTOL flight updates. Neutral and concise, never conversational or customer-service cheerful. No smile, breathiness, vocal fry, theatrical performance, emergency alarm, background noise, music, room reverb, or radio distortion.
```

## Text and Delivery Contract

| ID | Text | Delivery notes |
| --- | --- | --- |
| T01 | `Flight update. Cruising altitude reached.` | Neutral opening; terminal statement ends cleanly. |
| T02 | `Flight update. Descent will begin shortly.` | Informational forecast; no urgency or warning contour. |
| T03 | `Flight update. Prepare for landing.` | Clear transition cue; firm but not commanding. |

Keep the generated take with the most stable identity, intelligible Australian pronunciation, clean onset, controlled ending, and no artifacts. Do not insert SSML pauses or change the wording across messages unless a documented pilot issue requires it.

## Cost and Account Boundary

ElevenLabs' public pricing page was checked on 2026-09-18: the Free tier lists Voice Design and 10k credits/month; Starter lists USD 6/month, 30k credits/month, and a commercial license. The actual account terms at the time of generation remain authoritative.

The three messages are very short. Start with the Free tier for voice audition. Use Starter only if the project needs the provider's commercial license or free-tier limits prevent selection. Do not use voice cloning or imitate a real broadcaster.

## Generation and Acceptance Sequence

1. Complete the speech/non-speech x quiet/noise Unity technical audition with the three raw files mapped explicitly to T01/T02/T03.
2. Obtain waveform-level peak/RMS and silence-boundary measures using a validated decoder, then decide whether identical trim, fade, and digital-level treatment are needed. Do not normalize to a study level before device calibration.
3. Verify the final three messages by human audition for wording, persona consistency, natural phrase boundaries, and absence of artifacts.
4. Only then perform HMD playback, cabin-noise mixing, SPL/SNR calibration, and the feasibility-pilot checks.
