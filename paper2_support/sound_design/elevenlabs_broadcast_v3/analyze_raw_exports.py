"""Verify the technical properties of ElevenLabs V3 raw speech exports.

The script records source-file hashes and the format metadata reported by macOS
``afinfo``. It does not modify the provider exports or create Unity assets.
Peak, RMS, and silence-boundary measurements remain explicitly pending until a
validated local decoder is available.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import subprocess
from datetime import datetime, timezone
from pathlib import Path
import re


EXPECTED_SAMPLE_RATE = 44_100
EXPECTED_CHANNELS = 1


def sha256(path: Path) -> str:
    """Return the SHA-256 digest for one source file."""
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for block in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def inspect_afinfo(source: Path) -> dict[str, float | int | None]:
    """Read provider-export metadata through the system audio inspector."""
    result = subprocess.run(
        ["afinfo", str(source)],
        check=False,
        capture_output=True,
        text=True,
    )
    if result.returncode:
        raise RuntimeError(f"afinfo failed for {source.name}: {result.stderr.strip()}")
    channels_match = re.search(r"Data format:\s+(\d+) ch,\s+(\d+) Hz", result.stdout)
    duration_match = re.search(r"estimated duration:\s+([0-9.]+) sec", result.stdout)
    bitrate_match = re.search(r"bit rate:\s+(\d+) bits per second", result.stdout)
    if not channels_match or not duration_match or not bitrate_match:
        raise ValueError(f"Unexpected afinfo output for {source.name}")
    return {
        "channels": int(channels_match.group(1)),
        "sample_rate_hz": int(channels_match.group(2)),
        "duration_s": float(duration_match.group(1)),
        "bit_rate_bps": int(bitrate_match.group(1)),
        "peak_dBFS": None,
        "rms_dBFS": None,
        "leading_silence_s": None,
        "trailing_silence_s": None,
        "sample_measurements_status": "pending validated local MP3 decoder",
    }


def inspect_export(source: Path) -> dict[str, object]:
    """Inspect one raw provider export without changing the audio data."""
    measurements = inspect_afinfo(source)
    return {
        "filename": source.name,
        "sha256": sha256(source),
        "bytes": source.stat().st_size,
        **measurements,
        "format_checks": {
            "mono": measurements["channels"] == EXPECTED_CHANNELS,
            "sample_rate_44100_hz": measurements["sample_rate_hz"] == EXPECTED_SAMPLE_RATE,
            "nonzero_duration": measurements["duration_s"] > 0,
            "no_digital_clipping": None,
        },
    }


def main() -> None:
    """Analyse exactly the three final raw files and save an audit JSON report."""
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).parent)
    args = parser.parse_args()
    raw_dir = args.root / "raw"
    sources = [
        raw_dir / "T01_cruising_altitude_raw.mp3",
        raw_dir / "T02_descent_raw.mp3",
        raw_dir / "T03_landing_raw.mp3",
    ]
    missing = [source.name for source in sources if not source.is_file()]
    if missing:
        raise FileNotFoundError(f"Missing raw exports: {', '.join(missing)}")

    records = [inspect_export(source) for source in sources]

    report = {
        "generated_at_utc": datetime.now(timezone.utc).isoformat(),
        "scope": "provider raw-export technical verification only",
        "records": records,
    }
    (args.root / "raw_analysis.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
