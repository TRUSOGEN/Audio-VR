# Turn Cue / Quest 3 交接记录（2026-09-24）

## 本次目标

本次交接把“转弯期间持续有声”作为硬性验收条件，同时保留声音 family 的试听决策。当前 `airy_breath_soft` 已作为 Unity technical candidate 接入；正式 family freeze 和 Overleaf 更新仍待 Quest 3 实听与研究 gate，不在本次修改范围内。

## 声音候选与试听位置

候选声音文件夹：

`/Users/trusoegn/论文2/sound_design/turn_cue_candidates_v1/`

现有五个候选 family：

- `airy_glass/turn_left.wav`、`turn_right.wav`：空灵、清亮、轻微 shimmer
- `soft_bloom/turn_left.wav`、`turn_right.wav`：温和、圆润、较暖
- `breath_chime/turn_left.wav`、`turn_right.wav`：轻呼吸感、低噪声亮度
- `airy_breath_soft/turn_left.wav`、`turn_right.wav`：`airy_glass` 与 `breath_chime` 的柔和融合版，降低高频和起音攻击性
- `suspended_fifth/turn_left.wav`、`turn_right.wav`：稳定悬置音程、轻 vibrato

试听页：

`http://127.0.0.1:8092/audition.html`

若本地服务未运行：

```sh
python3 -m http.server 8092 --bind 127.0.0.1 --directory /Users/trusoegn/论文2/sound_design/turn_cue_candidates_v1
```

除 `airy_breath_soft` 外，每个 WAV 为 mono、44.1 kHz、PCM16、约 0.72 s；当前导入 Unity 的 `airy_breath_soft` 已延长为 1.30 s，用于覆盖 1.00 s 的 cue 起始间隔。每个 family 的左右文件内容相同，方向由 Unity 的 mono `AudioSource` 空间位置承担，避免用左右不同音色代替空间方向。`airy_breath_soft` 是当前 Unity technical candidate，正式研究刺激仍未冻结。

### 新增 `airy_breath_soft`

该候选将 `airy_glass` 的空灵主体与 `breath_chime` 的轻呼吸调制结合，并降低高次谐波、放慢起音/衰减。当前 1.30 s 文件的数字审计结果为 RMS `0.2478`、峰值 `0.5260`；这只是数字层面的柔和度检查，最终是否舒适仍需真人和 Quest 3 实听。

## Unity 已完成的修改

权威工程：`/Users/trusoegn/GitHub/Audio-VR/Audio-VR`

关键文件：

- `Assets/Scripts/AudioV0RuntimeAudioDirector.cs`
- `Assets/Scripts/AudioV0QuestControllerUIBootstrap.cs`
- `Assets/Scripts/AudioV0RuntimeDemoUI.cs`

### 持续转弯声音

- 转弯段进入时立即发出第一条空间 cue，不再等待固定的初始延迟。
- 后续 cue 固定按 `1.00 s` 起始间隔播放；当前 `airy_breath_soft` 为 `1.30 s`，相邻 cue 保留约 `0.30 s` 重叠，转弯期间不会出现静音空档。
- 当前 cue 仍由同一个 mono 资源通过空间 `AudioSource` 承担左右方向，版本记录为 `airy_breath_soft_v2_1300ms_interval_1s_candidate`。
- 初始转向角度很小时，会从后续路线几何解析左右方向，避免转弯刚开始没有声音。
- cue 使用空间 `AudioSource`，转弯声源沿左右方向扫动；左右方向仍由空间化承担。

### Quest 3 手柄点击

- 运行时创建左右 `XRRayInteractor`。
- 使用 Quest controller trigger 作为 `UI Press`。
- 自动启用 `XRUIInputModule` 和世界空间 Canvas 的 `TrackedDeviceGraphicRaycaster`。
- 保留现有 UI 和场景结构，只补运行时 XR 点击链路，目标是写入 Quest 3 后可用手柄射线点击。

### 视觉提示

底部白色视觉提示文字已隐藏，活动区域仍保持可点击；不改变主要实验流程。

## 最新修订与验证状态（2026-09-24）

- 已重新生成 `/Users/trusoegn/论文2/sound_design/turn_cue_candidates_v1/airy_breath_soft/turn_left.wav` 与 `turn_right.wav`，时长 `1.30 s`；左右 SHA-256 相同，Unity 资源与源文件 SHA-256 相同。
- 已将同一 mono 资源导入 `Assets/Resources/AudioV0Stimuli/TurnCues/airy_breath_soft_turn.wav`，并把调度间隔固定为 `1.00 s`。
- 已为 `RoutePlayer` 与 `VisualTaskController` 增加生命周期守卫，避免运动剖面或目标 RNG 缺失时抛出无上下文的 `NullReferenceException`；这类状态现在写入显式错误码并停止当前尝试。
- EditMode 回归为 `965/967 passed`、`2 skipped`、`0 failed`；本次四航程运行后 Console 为 `0 error / 0 warning`。
- Overleaf 本次未修改；没有需要写入论文正文的参数决定。当前变化只属于 Unity technical candidate 与声音交接记录。

## 软件 QA 证据

QA 输出目录：

`/Users/trusoegn/GitHub/Audio-VR/Audio-VR/temp/20260924-final-delivery/run_20260924_115337/`

运行状态：

- `runtime_status.json`: `status=complete`
- `completed_routes=4`
- `time_scale=1`
- `route_complete=4`
- `spatial_turn_cue=385`
- `spatial_turn_cue_unavailable=0`
- 首个转弯在 route 1 的 `61.806 s` 产生第一条 cue
- 四条路线按 attempt 分组的 cue 间隔中位数均为 `1.000 s`；起始边界帧造成的最小间隔约 `0.17–0.48 s`，其余连续 cue 均按 1.00 s 触发
- 所有 cue 的 `clip=airy_breath_soft_turn`，四条路线均无 `error` 事件
- `four_full_flights_complete.png`、左右转弯截图均未出现底部白色椅形视觉残留

运行日志：

`/Users/trusoegn/AudioV0_Data/technical_demo/session_20260924_115223_627_52ac544fd75c46a5ae225a59001fd3fb/event_log.jsonl`

Unity 编译/Console 检查已完成：0 个 Console error、0 个 warning；Editor 已停止并回到 Edit Mode。

当前 Unity 安装的 `PlaybackEngines` 只有 `MacStandaloneSupport` 与 `WebGLSupport`，没有 Android/Quest 模块。因此本次已完成资源、脚本、XR 手柄点击链路和桌面 technical QA 的准备，但尚未生成 Quest 3 APK；需要在 Unity Hub 为 `6000.6.0f1` 安装 Android Build Support（含 SDK/NDK/OpenJDK）后再做 Android 构建与头显实测。

## 证据边界

以上是 Unity technical demo / synthetic 软件证据，不等同于 Quest 3 实机证据，也不等同于 HRTF、耳侧响度、真人听感、舒适度或正式 pilot 结果。仍需在 Quest 3 上实测：

1. 左右手柄射线和 trigger 点击是否稳定命中 UI。
2. 空间方向、响度和连续 cue 是否在耳机/头显中清楚且舒适。
3. 选定 family 后的最终声音是否比当前临时宽带 cue 更空灵、舒缓，且不会产生刺耳或扫地般的粗糙感。

## 下一步顺序

1. 在 Quest 3 上写入当前 technical candidate，完成手柄射线 trigger 点击、空间方向和 1.00 s 连续 cue 听感实测。
2. 若 Quest 3 实听通过，再记录声学校准、响度和舒适度，并决定是否冻结该 family 为正式研究刺激。
3. 实机结果单独记录，不把 software QA 写成 Quest/pilot 结果。
