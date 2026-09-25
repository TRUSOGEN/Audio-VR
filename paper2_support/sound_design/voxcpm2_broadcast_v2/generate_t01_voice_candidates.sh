#!/usr/bin/env bash
# Generate two controlled T01 voice-identity candidates for human audition.
set -euo pipefail

: "${VOXCPM2_CLI:?Set VOXCPM2_CLI to the voxcpm2-cli binary.}"
: "${VOXCPM2_BASE_MODEL:?Set VOXCPM2_BASE_MODEL to VoxCPM2-BaseLM-Q8_0.gguf.}"
: "${VOXCPM2_ACOUSTIC_MODEL:?Set VOXCPM2_ACOUSTIC_MODEL to VoxCPM2-Acoustic-F16.gguf.}"

OUTPUT_DIR="${OUTPUT_DIR:-$(cd "$(dirname "$0")" && pwd)}"
RAW_DIR="${OUTPUT_DIR}/raw"
MESSAGE="Flight update. Cruising altitude reached."
CFG="2.0"
TIMESTEPS="12"
TEMPERATURE="0.85"

generate() {
    local identity="$1"
    local seed="$2"
    local description="$3"
    "${VOXCPM2_CLI}" \
        --text "(${description})${MESSAGE}" \
        --output "${RAW_DIR}/T01_${identity}_raw.wav" \
        --cfg "${CFG}" \
        --timesteps "${TIMESTEPS}" \
        --temperature "${TEMPERATURE}" \
        --seed "${seed}" \
        "${VOXCPM2_BASE_MODEL}" \
        "${VOXCPM2_ACOUSTIC_MODEL}"
}

mkdir -p "${RAW_DIR}"
generate "female_broadcast" "2417" "A polished adult female aviation cabin-announcement voice, trained broadcast presenter, clear Standard Australian English, mid-low pitch, calm authority, resonant but natural, precise consonants, steady breath support, measured cadence, clean phrase endings, formal public-address delivery, low arousal, no chatty tone, no smile, no urgency, no theatrical acting."
generate "male_broadcast" "3841" "A polished adult male aviation cabin-announcement voice, trained broadcast presenter, clear Standard Australian English, warm resonant mid-low pitch, calm authority, precise consonants, steady breath support, measured cadence, clean phrase endings, formal public-address delivery, low arousal, no chatty tone, no smile, no urgency, no theatrical acting."
