# Audio V0 Asset and Stimulus Register

创建日期：2026-09-15

## Register boundary

This register separates technical-demo resources from research stimuli. An item may enter a formal-study build only when its source, licence, version, import result, and intended role are recorded. Missing provenance is a stop condition for formal use, not a reason to guess.

## Baseline record

| Field | Recorded value |
| --- | --- |
| Unity working source | `/Users/trusoegn/GitHub/Audio-VR/Audio-VR` |
| Scene | `Assets/AudioV0.unity` |
| Unity editor | `6000.6.0f1` |
| Render pipeline | URP `17.6.0` from `Packages/manifest.json` |
| Git branch | `main` |
| Baseline commit | `06719bc` |
| Scene file timestamp checked | `2026-09-14 20:12:13` |
| Current evidence boundary | Technical demo only; no HMD, frame-rate, comfort, or acoustic calibration claim |
| Formal mode | Must remain `formalStudyMode=false` until the pilot gates close |

The current saved scene diff contains the runtime objects `AudioV0_RuntimeExperienceAudio`, `AudioV0_RuntimeCabinNoise`, and `AudioV0_RuntimeSpatialTurnCue`, plus their audio-source components. The current repository also contains existing user/agent modifications across scripts, package lock, scene, and documentation; these are retained and are not treated as a clean baseline.

## Existing resources

| asset_id | source_url | publisher | licence | file_size | pipeline_result | import_date | used_objects | owner | decision |
| --- | --- | --- | --- | ---: | --- | --- | --- | --- | --- |
| `proto_vehicle_fbx` | Source URL not present in the current local register; provenance recovery required | Kenney attribution recorded in the 2026-09-14 visual handoff | Confirm the source licence before formal use | local file | Imported and found under `Resources/AudioV0Prototype/vehicle.fbx`; hero suitability not established | 2026-09-14 | Distant technical traffic only | Audio Experiment | Retain for technical mode; do not use as hero eVTOL |
| `technical_sci_fi_ogg_set` | Source URL not present in the current local register; provenance recovery required | Public-material set recorded in project resources | Confirm source licence before formal use | local files | Loads as technical UI/ambience resources | pre-existing | Interface and technical ambience | Audio Experiment | Never substitute for formal transition stimuli |
| `runtime_procedural_city` | Project-authored runtime code | Audio Experiment prototype | Project-authored | runtime generated | URP material construction currently compiles; visual quality and performance remain unverified | 2026-09-14 | Technical runtime skyline | Audio Experiment | Replace or layer after visual baseline |
| `runtime_procedural_sky` | Project-authored runtime code | Audio Experiment prototype | Project-authored | runtime generated | Uses `Skybox/Procedural`, fog, and ambient colours; cloud geometry absent | 2026-09-14 | Technical runtime sky | Audio Experiment | Retain as default sky and add controlled cloud cards |
| `runtime_custom_hero_evtol_v1` | Project-authored `AudioV0RuntimeAircraftBuilder.cs` | Audio Experiment prototype | Project-authored | runtime generated | Assembly and Play Mode hierarchy verified; no marketplace eVTOL used | 2026-09-15 | Technical exterior corridor | Audio Experiment | Retain for technical demo; HMD visibility/performance pending |
| `runtime_cloud_layer_v1` | Project-authored `AudioV0RuntimePresentation.cs` and `AudioV0RuntimeScenario.cs` | Audio Experiment prototype | Project-authored | runtime generated | Five groups x four puffs, transparent material and drift component verified in Play Mode | 2026-09-15 | Technical sky/environment | Audio Experiment | Retain; visual frame and performance gate pending |
| `runtime_state_cue_set_v1` | Project-authored `AudioV0RuntimeCueGenerator` | Audio Experiment prototype | Project-authored | runtime generated AudioClip | 44.1 kHz mono procedural clips compile and are assigned to T01/T02/T03 arrays; loudness calibration not measured | 2026-09-15 | Technical and provisional non-speech notification path | Audio Experiment | Pilot prototype only; do not report as final stimulus |

## Candidate external resources

These are candidates, not imported assets. Each must pass the URP, visual, licence, and performance checks before entering the project.

| asset_id | source_url | publisher | licence | file_size | pipeline_result | import_date | used_objects | owner | decision |
| --- | --- | --- | --- | ---: | --- | --- | --- | --- | --- |
| `assetstore_lowpoly_modern_city_buildings` | https://assetstore.unity.com/packages/3d/environments/urban/lowpoly-modern-city-buildings-set-64427 | k karboosx | Standard Unity Asset Store EULA | 9.7 MB | Legacy package; URP result not listed and requires isolated material test | not imported | Mid/far skyline only if compatible | Audio Experiment | Candidate, conditional |
| `assetstore_low_poly_city_assets` | https://assetstore.unity.com/packages/3d/environments/urban/low-poly-city-assets-234586 | PS Paulina Sroka | Standard Unity Asset Store EULA | 7.8 MB | Page lists Built-in compatibility and no URP compatibility | not imported | None until converted and tested | Audio Experiment | Reject as first choice |
| `assetstore_cope_free_skybox_pack` | https://assetstore.unity.com/packages/2d/textures-materials/sky/cope-free-skybox-pack-22252 | S Scrycoast | Standard Unity Asset Store EULA | 25.7 MB | Legacy skybox package; import test required | not imported | Optional sky reference only | Audio Experiment | Candidate, lower priority |

## Research stimulus register

| stimulus_id | condition_design | target_transition | source_file | sample_rate_hz | bit_depth | duration_s | integrated_lufs | peak_dbtp | version | sha256 | author | revision_reason |
| --- | --- | --- | --- | ---: | ---: | ---: | ---: | ---: | --- | --- | --- | --- |
| `T01_Cruise_NonSpeech` | `non_speech` | `cruise_entry` | Runtime `AudioV0CueGenerator` | 44100 | 32-bit float runtime | 1.35 | not measured | not measured | provisional `v1-runtime` | not applicable | Audio Experiment | Prototype rising/resolving contour; export and calibration still required |
| `T02_Descent_NonSpeech` | `non_speech` | `descent_begin` | Runtime `AudioV0CueGenerator` | 44100 | 32-bit float runtime | 1.35 | not measured | not measured | provisional `v1-runtime` | not applicable | Audio Experiment | Prototype falling contour; export and calibration still required |
| `T03_LandingPrep_NonSpeech` | `non_speech` | `landing_preparation` | Runtime `AudioV0CueGenerator` | 44100 | 32-bit float runtime | 1.35 | not measured | not measured | provisional `v1-runtime` | not applicable | Audio Experiment | Prototype calm approach/resolution contour; export and calibration still required |
| `T01_Cruise_Speech` | `speech` | `cruise_entry` | `Resources/AudioV0Stimuli/T01_ElevenLabs_Broadcast_v3.mp3` | 44100 | MP3 mono, 128 kbps | 2.351 | not measured | not measured | provisional `ElevenLabs_Broadcast_v3` | `09cddda0a3555a7421c7e048144eefc412a2da47b97bcbffffe32ea356261e13` | ElevenLabs Voice Design, `Paper2_AU_Female_Broadcast_V3` | Technical mapping verified; wording/persona audition, loudness measurement and calibration remain required |
| `T02_Descent_Speech` | `speech` | `descent_begin` | `Resources/AudioV0Stimuli/T02_ElevenLabs_Broadcast_v3.mp3` | 44100 | MP3 mono, 128 kbps | 2.508 | not measured | not measured | provisional `ElevenLabs_Broadcast_v3` | `faa43a45ea4d2137f8246e93fc4d1fc95f2aa5a38c557ebe336eeeb4adc583a2` | ElevenLabs Voice Design, `Paper2_AU_Female_Broadcast_V3` | Technical mapping verified; wording/persona audition, loudness measurement and calibration remain required |
| `T03_LandingPrep_Speech` | `speech` | `landing_preparation` | `Resources/AudioV0Stimuli/T03_ElevenLabs_Broadcast_v3.mp3` | 44100 | MP3 mono, 128 kbps | 2.116 | not measured | not measured | provisional `ElevenLabs_Broadcast_v3` | `a8350a46455f1aca41bc5b6a59214ac7cde2ff129af381326d41bb99c1c9950a` | ElevenLabs Voice Design, `Paper2_AU_Female_Broadcast_V3` | Technical mapping verified; wording/persona audition, loudness measurement and calibration remain required |

## Baseline capture status

- Operator-view capture: not retained in this register because the current Unity foreground surface is the UnitySkills control window rather than the project Game view.
- Participant-view capture: not retained because a reliable live participant frame was not available at the baseline check.
- The absence of captures is recorded as an evidence gap; it is not evidence that either layout is correct.

## Required update rule

Every imported asset or generated clip must add a complete row before it is mapped into runtime code. If the value is not known, record the item as unverified and keep the item out of formal mode.
