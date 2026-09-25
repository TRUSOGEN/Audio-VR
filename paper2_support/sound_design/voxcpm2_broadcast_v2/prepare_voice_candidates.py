"""Prepare T01 voice candidates with identical trim, fade, and digital RMS treatment."""

from __future__ import annotations

import argparse
from pathlib import Path

import numpy as np
import soundfile as sf


TARGET_RMS = 0.025
SILENCE_THRESHOLD = 1e-4
FADE_SECONDS = 0.010
TRAILING_PAD_SECONDS = 0.020


def process_file(source: Path, output_dir: Path) -> dict[str, float | str]:
    """Trim terminal silence and write a PCM-16 candidate at the shared digital level."""
    samples, sample_rate = sf.read(source, always_2d=False)
    if samples.ndim != 1:
        raise ValueError(f"Expected mono audio: {source}")

    non_silent = np.flatnonzero(np.abs(samples) >= SILENCE_THRESHOLD)
    if not len(non_silent):
        raise ValueError(f"The source contains no audible samples: {source}")

    retained = samples[: non_silent[-1] + 1].copy()
    fade_frames = min(round(FADE_SECONDS * sample_rate), len(retained))
    retained[-fade_frames:] *= np.linspace(1.0, 0.0, fade_frames, endpoint=True)
    processed = np.concatenate(
        [retained, np.zeros(round(TRAILING_PAD_SECONDS * sample_rate), dtype=retained.dtype)]
    )
    source_rms = float(np.sqrt(np.mean(processed**2)))
    processed *= TARGET_RMS / source_rms
    peak = float(np.max(np.abs(processed)))
    if peak >= 1.0:
        raise ValueError(f"The RMS target would clip {source}: peak={peak}")

    output = output_dir / source.name.replace("_raw.wav", ".wav")
    sf.write(output, processed, sample_rate, subtype="PCM_16")
    return {
        "output": output.name,
        "duration_s": len(processed) / sample_rate,
        "peak": peak,
        "rms": float(np.sqrt(np.mean(processed**2))),
        "sample_rate": sample_rate,
    }


def main() -> None:
    """Process exactly the two T01 audition candidates and print their audio records."""
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).parent)
    args = parser.parse_args()
    raw_dir = args.root / "raw"
    sources = sorted(raw_dir.glob("T01_*_raw.wav"))
    if len(sources) != 2:
        raise ValueError(f"Expected exactly two T01 raw candidates, found {len(sources)}")
    for source in sources:
        print(process_file(source, args.root))


if __name__ == "__main__":
    main()
