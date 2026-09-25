#!/usr/bin/env bash
# Generate the three provisional Azure Speech files without storing credentials.
set -euo pipefail

: "${AZURE_SPEECH_KEY:?Set AZURE_SPEECH_KEY in the current shell.}"
: "${AZURE_SPEECH_REGION:=australiaeast}"

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
project_root="$(cd "${script_dir}/../.." && pwd)"
unity_output_dir="${project_root}/../GitHub/Audio-VR/Audio-VR/Assets/Resources/AudioV0Stimuli/ProvisionalV1/Speech"
endpoint="https://${AZURE_SPEECH_REGION}.tts.speech.microsoft.com/cognitiveservices/v1"

if ! command -v afinfo >/dev/null 2>&1; then
  echo "afinfo is required to validate the WAV exports." >&2
  exit 1
fi

mkdir -p "${unity_output_dir}"

for stimulus_id in T01_CruiseEntry T02_DescentBegin T03_LandingPreparation; do
  input_ssml="${script_dir}/${stimulus_id}.ssml"
  output_wav="${unity_output_dir}/${stimulus_id}.wav"
  temporary_wav="$(mktemp "${TMPDIR:-/tmp}/audio-v0-azure.XXXXXX.wav")"

  cleanup() {
    rm -f "${temporary_wav}"
  }
  trap cleanup EXIT

  curl --fail --silent --show-error --location \
    --request POST "${endpoint}" \
    --header "Ocp-Apim-Subscription-Key: ${AZURE_SPEECH_KEY}" \
    --header "Content-Type: application/ssml+xml" \
    --header "X-Microsoft-OutputFormat: riff-24khz-16bit-mono-pcm" \
    --header "User-Agent: Paper2AudioStimulusGenerator/1.0" \
    --data-binary "@${input_ssml}" \
    --output "${temporary_wav}"

  if [ ! -s "${temporary_wav}" ] || [ "$(wc -c < "${temporary_wav}")" -le 4096 ]; then
    echo "Azure returned an empty or header-only WAV for ${stimulus_id}." >&2
    exit 1
  fi

  if ! afinfo "${temporary_wav}" | rg -q "estimated duration: [0-9]+\\.[0-9]+"; then
    echo "Azure output for ${stimulus_id} is not a readable WAV." >&2
    exit 1
  fi

  mv "${temporary_wav}" "${output_wav}"
  trap - EXIT
  echo "${stimulus_id}: $(shasum -a 256 "${output_wav}")"
done
