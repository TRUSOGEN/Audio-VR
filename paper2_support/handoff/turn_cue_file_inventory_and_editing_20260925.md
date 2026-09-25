# 转向声音文件清单与修改说明

更新时间：2026-09-25  
适用工程：`/Users/trusoegn/GitHub/Audio-VR/Audio-VR`  
对应 Unity：`6000.6.0f1`

## 先看结论

- Unity 当前实际使用的音频资源是：
  `/Users/trusoegn/GitHub/Audio-VR/Audio-VR/Assets/Resources/AudioV0Stimuli/TurnCues/airy_breath_soft_turn.wav`
- 该资源为 mono、44.1 kHz、PCM 16-bit、1.30 s。
- 当前转向 cue 的触发间隔为 **1.00 s（起始到起始）**，代码在：
  `/Users/trusoegn/GitHub/Audio-VR/Audio-VR/Assets/Scripts/AudioV0RuntimeAudioDirector.cs`
- 当前 1.30 s 音频比 1.00 s 间隔长约 0.30 s，所以连续转弯期间相邻 cue 会重叠，不会留下静音空档。
- 间隔可以自行调长；只需修改 `SpatialTurnCueIntervalSeconds`。如果仍要求连续有声，音频时长应至少等于间隔，实际建议比间隔长 0.05–0.15 s。
- 本次只新增这份文件清单，没有修改 Overleaf。

## 1. 源声音文件夹

所有候选声音的源文件都在：

`/Users/trusoegn/论文2/sound_design/turn_cue_candidates_v1/`

| family | 左文件 | 右文件 | 时长 | 用途 |
|---|---|---|---:|---|
| `airy_glass` | `airy_glass/turn_left.wav` | `airy_glass/turn_right.wav` | 0.72 s | 空灵、清亮、轻微 shimmer |
| `soft_bloom` | `soft_bloom/turn_left.wav` | `soft_bloom/turn_right.wav` | 0.72 s | 温和、圆润、较暖 |
| `breath_chime` | `breath_chime/turn_left.wav` | `breath_chime/turn_right.wav` | 0.72 s | 轻呼吸感、较低噪声亮度 |
| `airy_breath_soft` | `airy_breath_soft/turn_left.wav` | `airy_breath_soft/turn_right.wav` | 1.30 s | `airy_glass` 与 `breath_chime` 的柔和融合版；当前 Unity candidate |
| `suspended_fifth` | `suspended_fifth/turn_left.wav` | `suspended_fifth/turn_right.wav` | 0.72 s | 稳定悬置音程、轻 vibrato |

所有 WAV 当前均为 mono、44.1 kHz、PCM 16-bit。每个 family 的 left/right 文件内容刻意相同；左右方向由 Unity 的空间 `AudioSource` 位置承担。因此修改当前声音时，通常要把同一份编辑结果同时导出为 `turn_left.wav` 和 `turn_right.wav`。

### 当前 `airy_breath_soft` 文件校验

| 文件 | SHA-256 |
|---|---|
| `airy_breath_soft/turn_left.wav` | `7436a1d77d70d1961c5af6cfc04041ba783cfed7edef36387a09d07a9cbf2204` |
| `airy_breath_soft/turn_right.wav` | `7436a1d77d70d1961c5af6cfc04041ba783cfed7edef36387a09d07a9cbf2204` |

候选清单和各 family 的 SHA-256 记录在：

`/Users/trusoegn/论文2/sound_design/turn_cue_candidates_v1/candidate_manifest.json`

## 2. 试听与生成文件

- 试听页面：
  `/Users/trusoegn/论文2/sound_design/turn_cue_candidates_v1/audition.html`
- 候选生成脚本：
  `/Users/trusoegn/论文2/sound_design/turn_cue_candidates_v1/generate_turn_cue_candidates.py`
- 候选包说明：
  `/Users/trusoegn/论文2/sound_design/turn_cue_candidates_v1/README.md`

启动试听页面：

```sh
python3 -m http.server 8092 --bind 127.0.0.1 --directory /Users/trusoegn/论文2/sound_design/turn_cue_candidates_v1
```

然后打开：`http://127.0.0.1:8092/audition.html`

## 3. Unity 中与转向声音直接相关的文件

### 音频资源

当前导入 Unity 的文件：

`/Users/trusoegn/GitHub/Audio-VR/Audio-VR/Assets/Resources/AudioV0Stimuli/TurnCues/airy_breath_soft_turn.wav`

对应 Unity 导入设置文件：

`/Users/trusoegn/GitHub/Audio-VR/Audio-VR/Assets/Resources/AudioV0Stimuli/TurnCues/airy_breath_soft_turn.wav.meta`

Unity 通过 `Resources.Load("AudioV0Stimuli/TurnCues/airy_breath_soft_turn")` 加载该资源。Unity 工程里目前只有这一个转向 WAV，不分别加载 left/right 两个文件。

当前 Unity WAV 的 SHA-256 为：

`7436a1d77d70d1961c5af6cfc04041ba783cfed7edef36387a09d07a9cbf2204`

它与当前 `airy_breath_soft/turn_left.wav` 和 `turn_right.wav` 相同。

### 调度与选择代码

1. `/Users/trusoegn/GitHub/Audio-VR/Audio-VR/Assets/Scripts/AudioV0RuntimeAudioDirector.cs`
   - 转向 cue 的触发、间隔、连续播放和空间位置。
   - 当前参数：

     ```csharp
     private const double SpatialTurnCueIntervalSeconds = 1.00d;
     ```

2. `/Users/trusoegn/GitHub/Audio-VR/Audio-VR/Assets/Scripts/AudioV0RuntimeScenario.cs`
   - 加载 `airy_breath_soft_turn` 并赋给 `audioDirector.turnCues`。

3. `/Users/trusoegn/GitHub/Audio-VR/Audio-VR/Assets/Scripts/AudioV0ExperimentProfile.cs`
   - 记录 `spatialTurnCueVersion`，当前值为：
     `airy_breath_soft_v2_1300ms_interval_1s_candidate`

4. `/Users/trusoegn/GitHub/Audio-VR/Audio-VR/Assets/Scripts/FormalAudioPolicy.cs`
   - 控制 technical audition 和 formal mode 是否允许共享转向 cue 通道。

5. `/Users/trusoegn/GitHub/Audio-VR/Audio-VR/Assets/AudioV0.unity`
   - 场景中的 `AudioV0_RuntimeSpatialTurnCue`、空间 `AudioSource` 及实验配置。

## 4. 如何把转向间隔调长

打开：

`/Users/trusoegn/GitHub/Audio-VR/Audio-VR/Assets/Scripts/AudioV0RuntimeAudioDirector.cs`

把：

```csharp
private const double SpatialTurnCueIntervalSeconds = 1.00d;
```

改成例如：

```csharp
private const double SpatialTurnCueIntervalSeconds = 1.50d;
```

这里的数值是相邻 cue 的**起始时间间隔**，不是额外静音时长。

| 间隔 | 1.30 s 当前音频的结果 |
|---:|---|
| 1.00 s | 重叠约 0.30 s，连续覆盖 |
| 1.25 s | 重叠约 0.05 s，基本连续 |
| 1.50 s | 会出现约 0.20 s 空档，不满足严格连续覆盖 |
| 2.00 s | 会出现约 0.70 s 空档 |

如果你想用 `1.50 s` 或更长间隔，同时保持转弯期间没有空档，请把最终 WAV 延长到至少约 `1.55–1.65 s`，再替换 Unity 资源。若接受短暂空档，只改代码中的间隔即可。

当前代码在进入转弯段时立即发出第一条 cue；之后按该固定间隔继续发出。转向持续时间较短时，最后一条 cue 仍可能在离开转弯段后自然播放完，这是正常的音频尾部，不是新的转弯事件。

## 5. 在其他音频软件中修改后如何接回 Unity

建议流程：

1. 先复制备份 `airy_breath_soft/`，保留原始 1.30 s 版本。
2. 在 Audacity、REAPER、Logic Pro 或其他软件中编辑。
3. 导出为 **mono / 44.1 kHz / PCM 16-bit WAV**，保留平滑 fade-in/fade-out，避免削波。
4. 将最终文件导出为同一内容的：
   - `turn_left.wav`
   - `turn_right.wav`
5. 若仍使用 `airy_breath_soft`，把编辑后的 WAV 复制到 Unity，并保持文件名：
   `Assets/Resources/AudioV0Stimuli/TurnCues/airy_breath_soft_turn.wav`
6. 回到 Unity，等待资源重新导入，确认 Inspector 中时长、采样率和 `3D` 空间音频设置正确。
7. 若改了间隔或声音版本，同步更新 `AudioV0ExperimentProfile.cs` 中的 `spatialTurnCueVersion`，便于日志追踪。
8. 至少完成一次桌面运行检查：转弯第一条 cue 是否立即出现、后续间隔是否符合设定、是否有 `spatial_turn_cue_unavailable` 或 `TURN_CUE_ASSET_MISSING`。

## 6. 当前文件的注意事项

`airy_breath_soft_turn.wav.meta` 中的 `userData` 仍写着旧值：

```text
selected_candidate=airy_breath_soft_v1
source_sha256=c6a0ac465b0773b88a7938643561356719ec69e815a9590c709163bd878a9cb6
```

当前实际文件是 1.30 s，SHA-256 为上文列出的 `7436a1...f2204`。这个旧 `userData` 不影响 Unity 播放，但在下一次音频冻结前应更新，避免把旧版本误当成当前版本。

## 7. Overleaf 与 Quest 3 边界

- 本次转向声音文件清单不需要修改 Overleaf；声音候选和 Unity technical candidate 仍属于实现与试听交接记录。
- 当前已有桌面 technical QA 记录，但这不等同于 Quest 3 实机听感或手柄点击验证。
- 写入 Quest 3 前，仍需在 Unity Hub 为 `6000.6.0f1` 安装 Android Build Support（含 SDK/NDK/OpenJDK），再生成 APK 并在头显中检查空间方向、连续性、响度和 trigger 点击。

## 相关交接记录

上一份连续转弯声音交接：

`/Users/trusoegn/论文2/handoff/turn_cue_quest3_continuous_audio_handoff_20260924.md`
