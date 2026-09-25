"""Audit generated candidate-family WAV files without treating them as calibrated audio."""

from __future__ import annotations

import hashlib
import json
import math
import wave
from pathlib import Path


ROOT = Path(__file__).resolve().parent


def audit_wav(path: Path) -> dict[str, object]:
    """Return reproducible file and digital-signal metadata for one mono WAV file."""
    with wave.open(str(path), "rb") as source:
        channels = source.getnchannels()
        sample_rate = source.getframerate()
        frame_count = source.getnframes()
        sample_width = source.getsampwidth()
        raw = source.readframes(frame_count)
    if channels != 1 or sample_width != 2:
        raise ValueError(f"Expected mono PCM16 WAV, got {path}")
    samples = [int.from_bytes(raw[index:index + 2], "little", signed=True) / 32768.0 for index in range(0, len(raw), 2)]
    return {
        "file": str(path.relative_to(ROOT)),
        "sha256": hashlib.sha256(path.read_bytes()).hexdigest(),
        "channels": channels,
        "sample_rate_hz": sample_rate,
        "sample_width_bits": sample_width * 8,
        "duration_seconds": round(frame_count / sample_rate, 6),
        "peak_digital_amplitude": round(max(abs(sample) for sample in samples), 6),
        "rms_digital_amplitude": round(math.sqrt(sum(sample * sample for sample in samples) / len(samples)), 6),
    }


def main() -> None:
    """Validate all generated candidates and emit a machine-readable audit record."""
    results = [audit_wav(path) for path in sorted(ROOT.glob("*/*.wav"))]
    if len(results) != 12:
        raise ValueError(f"Expected 12 candidate WAV files, found {len(results)}")
    errors = [
        result["file"]
        for result in results
        if result["channels"] != 1
        or result["sample_rate_hz"] != 44_100
        or result["sample_width_bits"] != 16
        or result["duration_seconds"] != 1.35
        or result["peak_digital_amplitude"] > 0.75
        or abs(float(result["rms_digital_amplitude"]) - 0.15) > 0.0001
    ]
    if errors:
        raise ValueError(f"Candidate format/level audit failed: {errors}")
    (ROOT / "candidate_analysis.json").write_text(
        json.dumps({"status": "digital_signal_audit_only", "files": results}, indent=2) + "\n",
        encoding="utf-8",
    )


if __name__ == "__main__":
    main()
