"""Generate deterministic non-speech notification-family candidates for Paper #2.

The script creates four candidate families with three transition cues each. All
files share sample rate, duration, fade envelope, peak headroom, and mono WAV
format so that a later listening screen can compare family design rather than
file-format artefacts. These are candidate stimuli only and are not imported
into the formal Unity condition until a documented screen and pilot are done.
"""

from __future__ import annotations

import math
import wave
from pathlib import Path


SAMPLE_RATE = 44_100
DURATION_SECONDS = 1.35
AMPLITUDE = 0.28
TARGET_RMS = 0.15
MAX_PEAK = 0.75
OUTPUT_ROOT = Path(__file__).resolve().parent


def envelope(progress: float) -> float:
    """Return a smooth onset/offset envelope for a cue."""
    return math.sin(math.pi * max(0.0, min(1.0, progress))) ** 0.85


def write_wav(path: Path, samples: list[float]) -> None:
    """Write mono floating-point samples as 16-bit PCM WAV with headroom."""
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


def synthesize(family: str, target: int) -> list[float]:
    """Synthesize one candidate cue from a family and transition index."""
    count = round(SAMPLE_RATE * DURATION_SECONDS)
    samples: list[float] = []
    phase = 0.0
    for index in range(count):
        time = index / SAMPLE_RATE
        progress = time / DURATION_SECONDS
        if family == "contour":
            if target == 0:
                frequency = 290.0 + 370.0 * progress
                accent_rate, accent_depth = 3.2, 0.16
            elif target == 1:
                frequency = 620.0 - 410.0 * progress
                accent_rate, accent_depth = 2.4, 0.13
            else:
                frequency = 260.0 + 130.0 * min(progress / 0.65, 1.0)
                if progress > 0.65:
                    frequency = 390.0 + 130.0 * ((progress - 0.65) / 0.35)
                accent_rate, accent_depth = 1.8, 0.11
            phase += 2.0 * math.pi * frequency / SAMPLE_RATE
            carrier = math.sin(phase) + 0.26 * math.sin(2.01 * phase)
            tick = 0.12 * math.sin(2.0 * math.pi * 1040.0 * time) if target == 2 and progress > 0.72 else 0.0
            value = carrier + tick + accent_depth * math.sin(2.0 * math.pi * accent_rate * time)
        elif family == "interval":
            motifs = ((330.0, 495.0), (495.0, 330.0), (330.0, 415.0, 495.0))
            motif = motifs[target]
            slot = min(len(motif) - 1, int(progress * len(motif)))
            local = (progress * len(motif)) - slot
            frequency = motif[slot]
            phase += 2.0 * math.pi * frequency / SAMPLE_RATE
            value = math.sin(phase) + 0.20 * math.sin(2.0 * phase)
            value *= 0.92 + 0.08 * math.cos(2.0 * math.pi * local)
        elif family == "rhythm":
            pulse_starts = ((0.18,), (0.18, 0.68), (0.18, 0.58, 0.98))[target]
            # The longer decay keeps the one-pulse cue at the same RMS without a peak-heavy click.
            pulse = sum(math.exp(-((time - start) / 0.120) ** 2) for start in pulse_starts)
            frequency = 430.0 + (45.0 * target)
            phase += 2.0 * math.pi * frequency / SAMPLE_RATE
            value = pulse * (math.sin(phase) + 0.18 * math.sin(2.0 * phase))
        elif family == "brightness":
            frequency = 360.0
            phase += 2.0 * math.pi * frequency / SAMPLE_RATE
            brightness = (0.10, 0.30, 0.52)[target]
            value = math.sin(phase) + brightness * math.sin(2.0 * phase)
            if target == 2:
                value += 0.10 * math.sin(2.0 * math.pi * 1100.0 * time) * (progress > 0.70)
        else:
            raise ValueError(f"Unknown family: {family}")
        samples.append(AMPLITUDE * envelope(progress) * value)
    current_rms = math.sqrt(sum(sample * sample for sample in samples) / len(samples))
    if current_rms <= 0:
        raise ValueError(f"Silent cue generated for {family}/{target}")
    scale = min(TARGET_RMS / current_rms, MAX_PEAK / max(abs(sample) for sample in samples))
    return [sample * scale for sample in samples]


def main() -> None:
    """Generate all family candidates and a manifest of deterministic settings."""
    families = ("contour", "interval", "rhythm", "brightness")
    targets = ("T01_cruise_entry", "T02_descent_begin", "T03_landing_preparation")
    for family in families:
        for target_index, target_name in enumerate(targets):
            write_wav(
                OUTPUT_ROOT / family / f"{target_name}.wav",
                synthesize(family, target_index),
            )
    manifest = OUTPUT_ROOT / "candidate_manifest.txt"
    manifest.write_text(
        "sample_rate=44100\nduration_seconds=1.35\nformat=mono_pcm16\n"
        "target_digital_rms=0.15\nmaximum_digital_peak=0.75\n"
        "families=contour,interval,rhythm,brightness\n"
        "targets=T01_cruise_entry,T02_descent_begin,T03_landing_preparation\n",
        encoding="utf-8",
    )


if __name__ == "__main__":
    main()
