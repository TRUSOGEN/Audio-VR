# Audio Stimulus Design V1

Date: 2026-09-18

Scope: Paper #2 UAM passenger notification experiment

Status: provisional technical-demo and pilot candidate; not approved for a main study

## Design Target And Boundary

The experiment compares two complete notification designs during a visually occupied UAM passenger journey: a learnable non-speech cue set and short spoken messages. Each design must distinguish the same three routine transitions.

| ID | Target transition | Candidate spoken message |
| --- | --- | --- |
| T01 | entering cruise | "Flight update. Cruising altitude reached." |
| T02 | beginning descent | "Flight update. Descent will begin shortly." |
| T03 | preparing to land | "Flight update. Prepare for landing." |

The messages describe the same target states shown in the response interface. They avoid commands, emergency language, social small talk, and claims about safety. This is a complete-design comparison. Any observed difference cannot be attributed to speech, pitch trajectory, duration, spatialisation, or a single acoustic feature in isolation.

The `quiet` and `simulated cabin noise` factors remain separate from the design factor. The current Unity technical-demo background bed is not a calibrated cabin-noise stimulus. No target SPL, SNR, response window, headset output mode, or spatial source position is frozen here.

## Candidate Non-Speech Family

The current Unity source generates three mono 44.1 kHz procedural cues at runtime in `AudioV0RuntimeCueGenerator.CreateStateCue`. They share a 1.35 s raised-cosine-like onset/offset envelope, a sinusoidal carrier with a restrained second harmonic, and a moderate peak multiplier. Their discriminating contour is intentionally the main semantic difference.

| ID | Contour | Additional feature | Intended learned association |
| --- | --- | --- | --- |
| T01 | 290 to 660 Hz rising sweep | 3.2 Hz accent | entering cruise |
| T02 | 620 to 210 Hz falling sweep | 2.4 Hz accent | beginning descent |
| T03 | 260 to 390 to 520 Hz rising resolution | late high tick | preparing to land |

These are candidate mappings, not naturally self-explanatory signals. Format-specific familiarisation must teach and check the three mappings before any participant block. The pilot must test discrimination, learnability, perceived startle, comfort, and whether the target states are predictable from the visual route even without the cue.

A direct calculation from the current generator gives T01/T02/T03 peak amplitudes of 0.4304/0.3960/0.4307 and RMS values of 0.1565/0.1547/0.1549 respectively. These are digital-signal properties only. They are not headset SPL, listener-level RMS, or SNR measurements.

## Candidate Speech Voice

The speech condition will use one synthetic English voice, one neutral speaking style, and the three fixed messages above. Holding one persona, rate, and processing chain constant prevents voice identity from varying by transition. The V1 production candidate is Azure AI Speech `en-AU-NatashaNeural`, exported in 24 kHz mono PCM WAV from Australia East. This is a reproducibility and locale choice, not a gender or age manipulation. It will be retained only after an intelligibility and comfort check with the actual headset and listening conditions.

On 2026-09-18, TTSMaker publicly described its service as neural text-to-speech and stated that generated audio may be used commercially without a fee. Its free conversion screen required a CAPTCHA. No CAPTCHA was completed and no audio was downloaded. Crikk also publicly stated that guest users can generate English MP3 files without registration and that generated files can be used commercially. A first conversion was started but the browser disconnected before completion; the Downloads and Unity asset directories contained no resulting audio. The macOS `say` command produced zero-duration files in this runtime environment; those invalid temporary files were removed. Therefore, no speech file exists yet and `speechClips` must remain explicitly incomplete.

Azure bills successfully processed text-to-speech requests by character count according to its official documentation; this three-message candidate set is intended as a low-volume pay-as-you-go generation, not a subscription commitment. The project includes opt-in V1 SSML and a generator under `azure_speech_v1/`. It requires an Azure Speech key supplied only through the current shell environment and writes no credential to the repository. When generated, register provider, voice identifier, export date, usage terms, exact text, language/accent, speaking-rate and style settings, format, duration, peak/RMS measurement, checksum, processing, and Unity `Resources` path.

## Evidence Used For V1

Tram's 2026-09-17 meeting direction is the primary project-design source: it separates sound character from Unity delivery, asks for a documented design process, accepts AI voice as a possible production method, and treats headset playback/spatial settings as apparatus decisions. It does not approve a voice gender, a level, an SNR, a spatial setting, or a final questionnaire.

Wang et al. (2022, DOI: 10.1145/3543174.3546830) studied speech style and embodiment in a conditionally automated-driving simulator. Its context is adjacent rather than UAM passenger travel. It supports treating speech style and visual distraction as design considerations; it does not establish that one voice persona or speech design is superior for this UAM study.

The project's verified adjacent automated-vehicle sources also motivate keeping auditory design and spatial delivery separately documented: Kim, van Egmond and Happee (2024, DOI: 10.1016/j.trf.2023.11.007) compares auditory manoeuvre information with visual UI, and Lungershausen et al. (2025, DOI: 10.1016/j.ijhcs.2025.103481) addresses spatialised predictive sonification. Neither source by itself validates a UAM-cabin setting or the candidate cue family. All final stimulus decisions require the planned HMD dry run and 2--3-person feasibility pilot.

## Unity Delivery Contract

Unity currently has functioning technical non-speech software playback, but its technical mode selects the procedural `variantClips` for all condition labels. It must not label S_Q or S_N as genuine speech until a three-file speech array is loaded and a condition-specific audition has confirmed the content.

Before a four-condition technical audition, the runtime must log for every notification: selected condition ID and stimulus-set version; selected clip name and T01/T02/T03 mapping; notification schedule, play call, software `isPlaying` sample and estimated offset; cabin-noise lifecycle where applicable; and a human audition result kept separate from software-playback evidence.

The current `AudioSource.spatialBlend=0` for notifications is a technical default, not a formal delivery decision. If spatialised notifications are later tested, their source location, blend, rolloff, HRTF/output route and control condition must be identical across both notification designs unless spatiality becomes a separately justified factor.

## Required Gates Before Pilot Freeze

1. Obtain three lawful TTS exports and inspect for non-zero duration, intelligible text, clipped samples, silent leading/trailing regions, and consistent format.
2. Assign the speech clips and the three non-speech clips through explicit condition mapping; do not rely on condition labels alone.
3. Perform quiet and noise technical auditions for NS_Q, NS_N, S_Q and S_N; retain logs and the human audition record.
4. Measure device output with the chosen calibration chain and document the resulting level and SNR. Do not infer either value from Unity volume.
5. Run the selected Quest/HMD block and the feasibility pilot. Confirm auditory comfort, intelligibility, mapping learnability, route/visual-task confounds, input compatibility, and absence of floor or ceiling performance.
6. Version, checksum and freeze the final assets, messages, training procedure, delivery settings and any processing only after the pilot and protocol gates.
