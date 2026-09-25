"""Generate controlled contour-soft screening candidates for Paper #2.

This script keeps the current contour family as C0 and creates three
single-factor variants. It supports a pre-screening comparison only. The
digital properties it controls are not HMD calibration, ear-side SPL, SNR, or
listener evidence, and no generated family is a final study stimulus.
"""

from __future__ import annotations

import hashlib
import math
import wave
from pathlib import Path


SAMPLE_RATE = 44_100
DURATION_SECONDS = 1.35
AMPLITUDE = 0.28
TARGET_RMS = 0.15
MAX_PEAK = 0.75
OUTPUT_ROOT = Path(__file__).resolve().parent
TARGETS = ("T01_cruise_entry", "T02_descent_begin", "T03_landing_preparation")


def envelope(progress: float, candidate: str) -> float:
    """Return the baseline or slower screening envelope for one cue."""
    bounded = max(0.0, min(1.0, progress))
    exponent = 1.65 if candidate == "C3_smoother_envelope" else 0.85
    return math.sin(math.pi * bounded) ** exponent


def cue_parameters(target: int, progress: float) -> tuple[float, float, float]:
    """Return the unchanged contour frequency and low-rate accent settings."""
    if target == 0:
        return 290.0 + 370.0 * progress, 3.2, 0.16
    if target == 1:
        return 620.0 - 410.0 * progress, 2.4, 0.13
    if progress <= 0.65:
        frequency = 260.0 + 130.0 * (progress / 0.65)
    else:
        frequency = 390.0 + 130.0 * ((progress - 0.65) / 0.35)
    return frequency, 1.8, 0.11


def synthesize(candidate: str, target: int) -> list[float]:
    """Synthesize one baseline or single-factor contour-soft screening cue."""
    count = round(SAMPLE_RATE * DURATION_SECONDS)
    harmonic_weight = 0.10 if candidate == "C2_reduced_harmonic" else 0.26
    remove_late_tick = candidate == "C1_no_late_tick"
    phase = 0.0
    samples: list[float] = []
    for index in range(count):
        time = index / SAMPLE_RATE
        progress = time / DURATION_SECONDS
        frequency, accent_rate, accent_depth = cue_parameters(target, progress)
        phase += 2.0 * math.pi * frequency / SAMPLE_RATE
        carrier = math.sin(phase) + harmonic_weight * math.sin(2.01 * phase)
        tick = 0.0
        if target == 2 and progress > 0.72 and not remove_late_tick:
            tick = 0.12 * math.sin(2.0 * math.pi * 1040.0 * time)
        accent = accent_depth * math.sin(2.0 * math.pi * accent_rate * time)
        samples.append(AMPLITUDE * envelope(progress, candidate) * (carrier + tick + accent))
    current_rms = math.sqrt(sum(sample * sample for sample in samples) / len(samples))
    if current_rms <= 0:
        raise ValueError(f"Silent cue generated for {candidate}/{target}")
    scale = min(TARGET_RMS / current_rms, MAX_PEAK / max(abs(sample) for sample in samples))
    return [sample * scale for sample in samples]


def write_wav(path: Path, samples: list[float]) -> None:
    """Write one mono 16-bit PCM WAV file with the checked digital headroom."""
    path.parent.mkdir(parents=True, exist_ok=True)
    with wave.open(str(path), "wb") as output:
        output.setnchannels(1)
        output.setsampwidth(2)
        output.setframerate(SAMPLE_RATE)
        frames = bytearray()
        for sample in samples:
            clipped = max(-0.98, min(0.98, sample))
            frames.extend(int(clipped * 32767).to_bytes(2, "little", signed=True))
        output.writeframes(frames)


def sha256(path: Path) -> str:
    """Return the checksum recorded in the candidate manifest."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main() -> None:
    """Generate C0--C3 candidates and write their exact screening manifest."""
    candidates = (
        "C0_current_contour",
        "C1_no_late_tick",
        "C2_reduced_harmonic",
        "C3_smoother_envelope",
    )
    manifest_lines = [
        "status=screening_candidates_only",
        "sample_rate_hz=44100",
        "duration_seconds=1.35",
        "format=mono_pcm16",
        "target_digital_rms=0.15",
        "maximum_digital_peak=0.75",
        "C0_current_contour=baseline copied algorithmically from candidate_families_v1/contour",
        "C1_no_late_tick=only T03 1040 Hz late tick removed",
        "C2_reduced_harmonic=only second-harmonic weight reduced from 0.26 to 0.10",
        "C3_smoother_envelope=only envelope exponent changed from 0.85 to 1.65",
        "parameter_status=engineering_screening_values_not_final_stimulus_values",
    ]
    for candidate in candidates:
        for target_index, target_name in enumerate(TARGETS):
            output_path = OUTPUT_ROOT / candidate / f"{target_name}.wav"
            write_wav(output_path, synthesize(candidate, target_index))
            manifest_lines.append(f"sha256.{candidate}/{target_name}.wav={sha256(output_path)}")
    (OUTPUT_ROOT / "candidate_manifest.txt").write_text(
        "\n".join(manifest_lines) + "\n", encoding="utf-8"
    )


if __name__ == "__main__":
    main()
