# Azure Speech V1 Export

This is an opt-in generator for the provisional speech candidate. It does not create an Azure account, resource, payment method, or stored credential.

After an Azure AI Speech resource is available in Australia East, run:

```bash
export AZURE_SPEECH_KEY='resource key entered only in this shell'
export AZURE_SPEECH_REGION='australiaeast'
bash generate_azure_speech_v1.sh
```

The script sends the three checked-in SSML files to Azure's REST endpoint and writes non-empty 24 kHz mono WAV files to the Unity `Resources` path only after file checks pass. It prints the SHA-256 for each export. It must still be followed by content listening, condition mapping, device calibration, HMD dry run, and pilot checks.
