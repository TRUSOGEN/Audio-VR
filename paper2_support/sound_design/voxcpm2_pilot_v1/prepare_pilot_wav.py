"""Create pilot WAV files from raw VoxCPM2 exports with documented level and tail treatment."""

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
    """Trim generated tail silence, add a common fade and pad, then match digital RMS."""
    samples, sample_rate = sf.read(source, always_2d=False)
    magnitude = np.abs(samples)
    non_silent = np.flatnonzero(magnitude >= SILENCE_THRESHOLD)
    if not len(non_silent):
        raise ValueError(f"The source contains no samples above the silence threshold: {source}")

    retained = samples[: non_silent[-1] + 1].copy()
    fade_frames = min(round(FADE_SECONDS * sample_rate), len(retained))
    retained[-fade_frames:] *= np.linspace(1.0, 0.0, fade_frames, endpoint=True)
    padding = np.zeros(round(TRAILING_PAD_SECONDS * sample_rate), dtype=retained.dtype)
    processed = np.concatenate([retained, padding])

    source_rms = float(np.sqrt(np.mean(processed**2)))
    gain = TARGET_RMS / source_rms
    processed *= gain
    peak = float(np.max(np.abs(processed)))
    if peak >= 1.0:
        raise ValueError(f"The selected RMS target would clip {source}: peak={peak}")

    output = output_dir / source.name.replace("_raw.wav", ".wav")
    sf.write(output, processed, sample_rate, subtype="PCM_16")
    return {
        "source": source.name,
        "output": output.name,
        "trimmed_tail_s": (len(samples) - len(retained)) / sample_rate,
        "duration_s": len(processed) / sample_rate,
        "gain": gain,
        "peak": peak,
        "sample_rate": sample_rate,
    }


def main() -> None:
    """Process all raw V1 exports and print a compact reproducibility record."""
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).parent)
    args = parser.parse_args()
    raw_dir = args.root / "raw"
    sources = sorted(raw_dir.glob("T*_VoxCPM2_pilot_v1_raw.wav"))
    if len(sources) != 3:
        raise ValueError(f"Expected exactly three raw stimuli in {raw_dir}, found {len(sources)}")
    for source in sources:
        print(process_file(source, args.root))


if __name__ == "__main__":
    main()
