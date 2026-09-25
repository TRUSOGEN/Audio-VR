"""Generate soft left/right turn-cue candidates for researcher audition.

The generator writes deterministic mono PCM16 WAV files. Left and right files
are intentionally identical within a family so the Unity spatial source, not a
timbral difference, carries the direction cue.
"""

from __future__ import annotations

import hashlib
import json
import math
import struct
import wave
from pathlib import Path


SAMPLE_RATE = 44_100
DURATION_SECONDS = 0.72
AIRY_BREATH_SOFT_DURATION_SECONDS = 1.30
ROOT = Path(__file__).resolve().parent


def smoothstep(value: float) -> float:
    """Return a cubic smoothstep envelope value in the inclusive unit interval."""
    value = max(0.0, min(1.0, value))
    return value * value * (3.0 - 2.0 * value)


def write_wav(path: Path, samples: list[float]) -> None:
    """Write normalised mono floating samples as little-endian PCM16 WAV."""
    peak = max(max(abs(sample) for sample in samples), 1e-9)
    scale = min(0.78 / peak, 1.0)
    pcm = b"".join(struct.pack("<h", int(max(-1.0, min(1.0, sample * scale)) * 32767)) for sample in samples)
    path.parent.mkdir(parents=True, exist_ok=True)
    with wave.open(str(path), "wb") as handle:
        handle.setnchannels(1)
        handle.setsampwidth(2)
        handle.setframerate(SAMPLE_RATE)
        handle.writeframes(pcm)


def candidate_samples(family: str) -> list[float]:
    """Synthesize one restrained, non-speech cue family."""
    duration = AIRY_BREATH_SOFT_DURATION_SECONDS if family == "airy_breath_soft" else DURATION_SECONDS
    count = round(SAMPLE_RATE * duration)
    samples: list[float] = []
    phase = 0.0
    for index in range(count):
        time = index / SAMPLE_RATE
        progress = index / max(1, count - 1)
        attack = smoothstep(progress / 0.20)
        release = smoothstep((1.0 - progress) / 0.34)
        envelope = attack * release
        if family == "airy_glass":
            glide = 440.0 + 190.0 * smoothstep(progress)
            phase += 2.0 * math.pi * glide / SAMPLE_RATE
            shimmer = math.sin(phase) * 0.78 + math.sin(phase * 2.01) * 0.16
            shimmer += math.sin(phase * 3.01 + 0.7) * 0.06
            value = shimmer * envelope
        elif family == "soft_bloom":
            glide = 300.0 + 180.0 * smoothstep(progress)
            phase += 2.0 * math.pi * glide / SAMPLE_RATE
            value = (math.sin(phase) * 0.78 + math.sin(phase * 1.5 + 0.2) * 0.15
                     + math.sin(phase * 2.0) * 0.07) * envelope
        elif family == "breath_chime":
            glide = 360.0 + 120.0 * smoothstep(progress)
            phase += 2.0 * math.pi * glide / SAMPLE_RATE
            airy = math.sin(phase) * 0.76 + math.sin(phase * 2.7 + 0.5) * 0.11
            slow = 0.92 + 0.08 * math.sin(2.0 * math.pi * 2.1 * time)
            value = airy * envelope * slow
        elif family == "airy_breath_soft":
            # Combine the airy body with a restrained breath modulation while
            # keeping upper partials low enough for comfortable repeated cues.
            glide = 380.0 + 140.0 * smoothstep(progress)
            phase += 2.0 * math.pi * glide / SAMPLE_RATE
            body = math.sin(phase) * 0.78
            breath = math.sin(phase * 2.02 + 0.18) * 0.095
            air = math.sin(phase * 2.65 + 0.42) * 0.035
            slow = 0.94 + 0.06 * math.sin(2.0 * math.pi * 1.35 * time + 0.4)
            value = (body + breath + air) * envelope * slow
        elif family == "suspended_fifth":
            base = 392.0 + 90.0 * smoothstep(progress)
            vibrato = 1.0 + 0.012 * math.sin(2.0 * math.pi * 4.1 * time)
            phase += 2.0 * math.pi * base * vibrato / SAMPLE_RATE
            value = (math.sin(phase) * 0.70 + math.sin(phase * 1.5 + 0.35) * 0.23
                     + math.sin(phase * 2.0) * 0.05) * envelope
        else:
            raise ValueError(f"unknown family: {family}")
        samples.append(value * 0.58)
    return samples


def main() -> None:
    """Generate all families, duplicate each for left/right, and write a manifest."""
    families = {
        "airy_glass": "smooth high airy tone with a restrained upper shimmer",
        "soft_bloom": "warm fifth-like bloom with a rounded onset",
        "breath_chime": "quiet airy body with minimal noise-like brightness",
        "airy_breath_soft": "softened airy glass body blended with a restrained breath modulation",
        "suspended_fifth": "stable suspended interval with very light vibrato",
    }
    manifest = {"sample_rate_hz": SAMPLE_RATE, "families": {}}
    for family, description in families.items():
        samples = candidate_samples(family)
        family_record = {
            "description": description,
            "duration_s": AIRY_BREATH_SOFT_DURATION_SECONDS if family == "airy_breath_soft" else DURATION_SECONDS,
            "direction_files_identical": True,
            "files": {},
        }
        for side in ("left", "right"):
            path = ROOT / family / f"turn_{side}.wav"
            write_wav(path, samples)
            family_record["files"][side] = {
                "path": str(path.relative_to(ROOT)),
                "sha256": hashlib.sha256(path.read_bytes()).hexdigest(),
            }
        manifest["families"][family] = family_record
    (ROOT / "candidate_manifest.json").write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")


if __name__ == "__main__":
    main()
