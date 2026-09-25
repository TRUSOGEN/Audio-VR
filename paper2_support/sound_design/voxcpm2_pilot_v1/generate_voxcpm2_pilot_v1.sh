#!/usr/bin/env bash
# Generates the three provisional speech stimuli with one fixed VoxCPM2 voice-design contract.
set -euo pipefail

: "${VOXCPM2_CLI:?Set VOXCPM2_CLI to the voxcpm2-cli binary.}"
: "${VOXCPM2_BASE_MODEL:?Set VOXCPM2_BASE_MODEL to VoxCPM2-BaseLM-Q8_0.gguf.}"
: "${VOXCPM2_ACOUSTIC_MODEL:?Set VOXCPM2_ACOUSTIC_MODEL to VoxCPM2-Acoustic-F16.gguf.}"

OUTPUT_DIR="${OUTPUT_DIR:-$(cd "$(dirname "$0")" && pwd)}"
RAW_DIR="${OUTPUT_DIR}/raw"
VOICE_DESCRIPTION="A neutral adult Australian English voice, medium pitch, clear and natural, calm professional informational tone, moderate pace, low emotional arousal, concise delivery, no urgency, no smile, no breathiness, no dramatic emphasis."
CFG="2.0"
TIMESTEPS="10"
TEMPERATURE="1.0"
SEED="1701"

generate() {
    local id="$1"
    local message="$2"
    local output="${RAW_DIR}/${id}_VoxCPM2_pilot_v1_raw.wav"
    "${VOXCPM2_CLI}" \
        --text "(${VOICE_DESCRIPTION})${message}" \
        --output "${output}" \
        --cfg "${CFG}" \
        --timesteps "${TIMESTEPS}" \
        --temperature "${TEMPERATURE}" \
        --seed "${SEED}" \
        "${VOXCPM2_BASE_MODEL}" \
        "${VOXCPM2_ACOUSTIC_MODEL}"
}

mkdir -p "${RAW_DIR}"
generate "T01" "Flight update. Cruising altitude reached."
generate "T02" "Flight update. Descent will begin shortly."
generate "T03" "Flight update. Prepare for landing."
