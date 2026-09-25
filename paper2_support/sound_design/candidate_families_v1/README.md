# Non-speech sound-family candidate bank V1

Status: stimulus-development material only. These files are not formal-study stimuli and are not imported into Unity yet.

This bank contains four alternative families, each with three transition cues:

| Family | Shared identity | T01 | T02 | T03 | Main risk to screen |
| --- | --- | --- | --- | --- | --- |
| `contour` | harmonic tone with smooth envelope | rising contour | falling contour | two-stage rising contour plus late tick | route predictability may make mappings too easy |
| `interval` | same timbre and short pitch motifs | ascending interval | descending interval | three-note ascending motif | pitch direction may dominate meaning |
| `rhythm` | same carrier and timbre | one pulse | two pulses | three pulses | response may rely on counting rather than learned meaning |
| `brightness` | same fundamental with controlled harmonic change | darker | medium | brighter plus late tick | spectral brightness may be confused with urgency |

All files are deterministic mono 44.1 kHz PCM16 WAV, 1.35 s, with the same envelope, target digital RMS of 0.15, and peak ceiling of 0.75. The only intended family-level changes are the cue parameters described above.

Run `python3 generate_candidate_families.py` to recreate the bank, then `python3 analyze_candidate_families.py` to create `candidate_analysis.json`. The analysis reports digital peak/RMS and file integrity only; it is not acoustic calibration, SPL, SNR, listener audibility, or a perceptual evaluation.

## Why there are many sounds

The many files are candidate stimuli for a sound-design and learnability screen, not extra conditions in the main 2 x 2 experiment. The main experiment should freeze one three-cue family after screening; otherwise family, timbre, rhythm, pitch contour and learnability become uncontrolled experimental factors.

## Screening plan

1. Check audibility, comfort, startle, and intelligibility-equivalent detectability in quiet and simulated noise.
2. Teach all three mappings with a fixed training script and record learning trials and errors.
3. Test transition identification after training, plus confidence and perceived relatedness of the three-cue set.
4. Reject families that produce ceiling performance before learning, floor performance after training, strong urgency/startle, or a cue-to-transition interpretation that is not the intended routine-flight meaning.
5. Select one family using prespecified criteria, version and checksum the three files, then use that family consistently in the formal non-speech condition.

## Evidence direction

The design is informed by earcon and auditory-display work, recent vehicle earcon evaluation, sonification studies in highly automated vehicles, and evidence that warning-sound structure can affect working-memory or response outcomes. The adjacent-vehicle evidence supports design and screening decisions; it does not establish which family is best for UAM passengers. Candidate sources to verify and cite in the manuscript include:

- Ho et al. (2023), *Earcons to reduce mode confusions in partially automated vehicles*, DOI: `10.1016/j.ijhcs.2023.103044`.
- Nadri et al. (2024), *Sonification Use Cases in Highly Automated Vehicles*, DOI: `10.1080/10447318.2023.2180236`.
- Basantis et al. (2021), *Novel Auditory Displays in Highly Automated Vehicles*, DOI: `10.1109/THMS.2021.3106892` (corrected against the corpus mapping recorded in the sound-family manual; the former DOI belonged to Nadri 2021).
- Kim et al. (2024), *How Manoeuvre Information via Auditory ... Can Enhance Trust and Acceptance in Automated Driving*, DOI: `10.1016/j.trf.2023.11.007`.
- *The Impact of Different Types of Auditory Warnings on Working Memory* (2022), DOI: `10.3389/fpsyg.2022.780657`.
- *Beyond Beeps: Evaluating Soundscapes for Take-Over Situations in Automated Vehicles* (2025), DOI: `10.1080/10447318.2025.2537782`.

These references are a design-evidence shortlist, not yet a claim that the manuscript has been updated with them.
