# Audio V0 Windows / Quest 3 Link 问题处理交接

更新时间：2026-09-25（Australia/Sydney）  
适用工程：`E:\Audio-paper\Audio-Vr\Audio-VR-Windows-project-20260924`  
Unity：`6000.6.0f1`；主场景：`Assets/AudioV0.unity`；运行方式：**Quest 3 通过 Quest Link 显示电脑上的 Unity Play Mode**。本文不描述 Quest 原生 APK 的验收结果。

## 先看当前状态

| 事项 | 当前结论 | 证据或限制 |
|---|---|---|
| 单眼画面、偏移及异常射线 | 用户在头显中确认画面恢复；Quest 手柄射线和点击曾恢复工作 | 已改 Windows Standalone OpenXR 渲染及 XR UI 输入；仍以再次戴机复查为准 |
| 转向提示 | 新的温和 F 大三和弦已经接入 Unity；每声 2.10 s，转弯期间每 3.00 s 开始一次 | 用户认可电脑端三次重复的试听文件；**尚未在 Quest 3 内听过这一新版** |
| 卡顿、闪蓝 | 画质调整后用户感到顺了一些、文字清楚，但密集建筑区仍偶发卡顿或闪蓝 | 真实会话记录到 0.653 s 等异常帧；最近一次减少 Console 输出后尚无头显复测，不能宣称解决 |
| 数据目录 | Windows 新会话写到 `E:\Audio-paper\实验数据`；分析工具位于 `E:\Audio-paper\分析工具` | 实际新会话、`session_end` 和 JSONL 格式已核对；旧 AppData 会话保留原位 |
| 编辑器 | 本轮最后已退出 Play Mode；最近一次脚本重编译后 Unity 显示 0 error / 0 warning | 下次戴机前重新进入 Play Mode；不要将“编译通过”当成头显验收 |

工程工作区原本有大量未提交改动，最近只读状态检查约 **286** 条。继续工作时按文件核对差异，保留用户原有修改，不批量清理或重置。

## 处理过程、原因与结果

### 1. Quest 3 单眼显示、座舱 UI 偏移、红线和手柄无响应

最初通过 Quest Link 看 Unity Play Mode 时，用户报告右眼没有正常画面、座舱屏幕偏左、左眼出现异常红线，手柄既无射线也不能点击。此时应优先检查 Windows PC VR 的双眼提交和 XR 输入链，不能仅根据桌面 Game View 判断头显结果。

- Windows **Standalone** OpenXR 设置 `Assets/XR/Settings/OpenXR Package Settings.asset` 的 `m_renderMode` 从单通道方案改为 `0`（MultiPass），使左右眼分别渲染。文件中另有 WebGL `m_renderMode: 1`，不要混淆平台段。
- 保留并启用 Quest Touch / Touch Plus 交互配置。`Assets/Scripts/AudioV0QuestControllerUIBootstrap.cs` 在 Quest Link 聚焦后建立左右 `XRRayInteractor`，以 controller trigger 作为 `UI Press`，并接通 `XRUIInputModule` 和 Canvas 的 `TrackedDeviceGraphicRaycaster`。
- 场景中的旧 `AudioV0 Gaze Ray` 已关闭，避免与手柄射线叠加或把旧指示线误当作 Quest 指针。此前项目记录称修改前留过临时备份，但当前已找不到该备份目录；回退前必须先检查实际文件和 Git 差异。
- 用户之后表示此前画面问题恢复，手柄可以进入正式飞行。该反馈确认当时可用；没有单独的双眼几何或光学校准记录，后续更改 XR 后要在两眼复查。

声音一度缺失时，用户随后判断是自己未开头显声音；本轮没有为这件事修改声音播放链。

### 2. 建筑密集区卡顿与蓝色闪现

早期头显运行快照曾见约 150–340 万 triangles、738–1587 draw calls，GPU 负载最高约 98%、温度约 89°C；这是不同时刻的瞬时值。先减轻明确可见的渲染和编辑器开销，再让用户在同一建筑航段复查。

| 改动 | 为什么这样做 | 当前结果 |
|---|---|
| `Assets/Settings/PC_Renderer.asset` 关闭全屏 SSAO | VR 双眼全屏效果成本高，且本场景的转向识别不依赖 SSAO | 已写入；没有单独量出 SSAO 的增益 |
| `Assets/Settings/PC_RPAsset.asset` 主光阴影 2048→1024、4→2 cascades、距离 50→30 m，关闭额外灯光阴影 | 缩短远景与附加光阴影开销，保留近处方向和城市形态 | 已写入；没有单独量出各参数增益 |
| 同一 URP 资产 `m_RenderScale` 1.00→0.85 | 降低双眼像素处理量；用户明确仍能读清座舱文字 | 用户认为更顺一些，但仍偶发闪蓝 |
| `Assets/Scripts/AudioV0EnvironmentAssets.cs` 稀疏远离航线的建筑窗格 | 保留建筑体块、碰撞及近航线两列窗格，减少重复小几何 | `AudioV0_AerialCityChunk_11Mesh` 约 31,058→17,894 triangles（约 42%）；这是局部网格检查，不是整体 FPS |
| `Assets/Scripts/EventLogger.cs` 不把 `flight_motion_sample`、`route_segment_enter` 逐条镜像到 Editor Console | JSONL 实验记录保持逐条写入；减少 Editor Console 的文字和堆栈开销 | 前一项已在 Play Mode 检查 JSONL 仍写入；后一项新改后已编译，尚待完整航段复查 |

`Assets/Scripts/AudioV0RuntimeScenario.cs` 的城市场景版本标记同步为 `street_readability_batched_v4_vr_windows_candidate`。GPU 是 RTX 4060 Laptop GPU。`nvidia-smi` 曾显示约 85–91°C，且驱动有累计 thermal slowdown 计数；这些计数没有按当时异常帧对时，**热降频只是待验证假设**。

在 `E:\Audio-paper\实验数据\technical_demo\session_20260924_170552_126_b109a8d1531f4190b75d171caef7bccd\event_log.jsonl` 中，`flight_motion_sample.details.frame_dt_s` 于本机 03:07:03–03:07:41 出现 7 次超过 50 ms 的采样，最大为 **03:07:30 的 0.65278 s**；03:07:40–41 还有约 125 ms 的采样。这与用户“偶尔卡”的反馈方向一致，但 5 Hz 采样不覆盖所有帧，无法单凭它判断 CPU、GPU 或 Link 编码/合成器的责任。03:14:29 的 **372.68910 s** 是后续编辑器长时间停顿，已经不属于当时的头显飞行感受，不应和上述七次合并。

此前用 UnitySkills 高频实时轮询渲染统计本身也可能给 Editor 增负；下次优先使用 Meta Quest Link 的 Performance HUD 或被动记录，别在用户戴机时持续高频调用编辑器 RPC。蓝色闪现与 Link 合成器丢帧有关是**待证实的解释**，目前没有相应的 compositor 计数。

### 3. 转向声从密集尖锐提示改为温和和弦

旧 `airy_breath_soft_turn.wav` 长 1.30 s，却每 1.00 s 再触发一次，相邻声音重叠约 0.30 s。用户希望转弯时继续重复提醒，但不要不间断，也认为旧声过响、偏刺耳。先试过 2.25 s 间隔、降低音量和音高；用户听后仍觉尖锐，于是改为新的低音域和弦，而没有继续对旧音频做只降音量的处理。

- 当前资源：`Assets/Resources/AudioV0Stimuli/TurnCues/gentle_chord_turn_v1.wav`。由 `sound_design/generate_gentle_chord_turn.py` 可重复生成，F3、A3、C4 三音缓慢叠入，约 0.36 s 渐起、0.78 s 渐隐，不再向高音滑动。44.1 kHz、mono、PCM16，时长 2.10 s；SHA-256：`7b5748cb2b975834062130d76c0fe0014ab734bfb9e962760f2a3ab9797215cb`。
- `Assets/Scripts/AudioV0RuntimeScenario.cs` 从 `Resources` 加载新资源；缺失时仍写 `TURN_CUE_ASSET_MISSING`，不会用假音频掩盖问题。
- `Assets/Scripts/AudioV0RuntimeAudioDirector.cs` 中，转弯开始立即发第一声，持续转弯时相邻起点间隔 `3.00 s`；声音长 `2.10 s`，理论空档约 `0.90 s`。空间音源左右移动周期也调为 `3.00 s`；专用音源 `volume=0.55`，`PlayOneShot` 系数 `0.50`，`pitch=1.00`，继续用同一 mono 资源由空间位置区分左/右。
- `Assets/Scripts/AudioV0ExperimentProfile.cs` 和 `Assets/AudioV0.unity` 的日志版本为 `gentle_f_major_chord_v1_2100ms_interval_3s_gain055x050_candidate`。旧 `airy_breath_soft_turn.wav` 保留，不作为当前映射。
- 用户听过连续三声的电脑端试听文件 `gentle_chord_v1_3s_preview.wav`，并明确表示“这个可以”。当前工程中已找不到该临时文件；`sound_design/generate_gentle_chord_turn.py` 定义了它的生成路径 `temp/turn-cue-preview/`，再次运行脚本会同时重写 Unity 资源与试听文件。试听**没有模拟 Quest 的空间化、传输链或耳侧响度**。

Unity 实际导入查询返回 `AudioClip` 已加载、2.10 s、1 channel、44,100 Hz、`DecompressOnLoad` / PCM；Play Mode 音源查询返回 `volume=0.55`、`pitch=1`、`spatialBlend=1`、`spatialize=true`。新会话 `runtime_demo_ready` 已记录新版标记；最近编译后 Console 为 0 error / 0 warning。尚未在 Quest 3 内听过新版，也尚未在一段真实转弯日志中核对连续 `spatial_turn_cue` 的 3 s 时间差，不能宣称头显声学或实际节奏已经通过。

### 4. Windows 数据和分析工具目录

- `Assets/Scripts/EventLogger.cs` 在 Windows Editor / Standalone 将数据根目录设为 `E:\Audio-paper\实验数据`；macOS 仍为 `~/AudioV0_Data`，Android 等平台仍使用其 `Application.persistentDataPath/AudioV0`。
- 新 Windows 会话以 `technical_demo/session_.../event_log.jsonl` 形式写入。修改后 Play Mode 实测 `DataRootPath` 正确、logger ready、无 fatal error；一场停止后的会话有 17 条事件、0 条无效 JSON 行及 `session_end`。旧 Windows `AppData/LocalLow` 会话没有迁移或删除。
- `E:\Audio-paper\分析工具\查看会话.ps1` 是只读会话结构概览，可查看事件数、运动采样、无效行和结束标记；它不是 macOS 论文数据中心或正式 CSV exporter 的完整迁移。说明在同目录 `README.md`。
- 目前所有这些都是 technical demo 数据，不得混作正式被试结果；正式采集前仍须确认声学校准、协议参数、识别分母和导出器。

## 下一步：按 P0 → P1 → P2 执行

### P0：下一次戴 Quest 3 时立即验证，未通过前不开始正式采集

1. **确认实际运行入口。** 连接 Quest Link，确认头显进入 PC VR；用 Unity `6000.6.0f1` 打开上述工程与 `Assets/AudioV0.unity`，进入 Play Mode，完成快速教程后点击 `START FOUR FLIGHTS`。最后状态是退出 Play Mode，需重新启动。观察两眼均有完整画面、座舱字仍清楚、两支手柄射线与 trigger 点击正常。
2. **验收新版转向声。** 在首个左转和右转分别听声：方向一致、音量舒适、音色不刺耳；长转弯中继续重复且有可感知空档。对相同一段转弯，检查 JSONL 的 `spatial_turn_cue`：首声进入转弯即触发，后续 `pulse_route_time_s` 的差值接近 3.00 s（弯段边界可有短间隔）。确认 `clip=gentle_chord_turn_v1`、无 `spatial_turn_cue_unavailable` 或 `TURN_CUE_ASSET_MISSING`。若头显听感与试听差异大，先记录具体方向、响度和时刻，再仅调该专用声源，不牵动 T01–T03 研究通知。
3. **定位卡顿/闪蓝。** 在同一建筑密集航段至少走两次，记下闪蓝或停顿时刻。用安装在 `D:\Meta Horizon\Support\oculus-diagnostics\OculusDebugTool.exe` 的 **Performance HUD** 看 Application Frames Dropped、Compositor Frames Dropped、App CPU/GPU 时间与余量；对照 `event_log.jsonl` 的 `timestamp_ms` 和 `frame_dt_s`，同时记录 GPU 温度、时钟及当时是否 thermal slowdown。HUD 可显示到头显内，测试后关闭；不要仅凭桌面 Game View、单张 Unity 统计快照或蓝色本身归因。
4. **按计数分流修复。** App 掉帧且 GPU 时间超预算：优先继续查城市几何、阴影和过度绘制；App 掉帧且 CPU 尖峰：用 Unity Profiler 定位 GC、场景生成、日志或导入/Editor 工作；App 时间正常但 compositor/Link 掉帧：检查 Quest Link 连接、编码设置、USB/无线链路、ASW 以及 Meta 运行时。每轮只改一类设置，用相同航段复查。验收标准是用户不再看到蓝色闪现或可察觉停顿，且对应掉帧计数不再在该航段异常增加。
5. **收尾日志。** 在 `E:\Audio-paper\实验数据\technical_demo` 中记录本次 session 路径和是否有 `session_end`、错误、cue 次数、卡顿时刻；实验中断要如实标记，不拿前次 session 充当本次验证。

Meta 官方参考：[Oculus Debug Tool](https://developers.meta.com/horizon/documentation/native/pc/dg-debug-tool/)、[Performance Head-Up Display](https://developers.meta.com/horizon/documentation/native/pc/dg-hud/)、[Link PC-VR 性能工具](https://developers.meta.com/horizon/documentation/unity/unity-perf/)。这些工具帮助区分应用与合成器掉帧，具体根因须结合本机读数判断。

### P1：P0 测量之后的工程收敛

1. 根据 P0 的瓶颈记录做定向修改。优先保留已经被用户确认清楚的座舱文字；若再调 `m_RenderScale`，必须复查地图、视觉任务和近处按钮。若需要更激进的建筑简化，保持路线净空、楼体与近航线视觉线索，并记录整体与局部 mesh/draw call 前后值。
2. 如果 Unity Editor 特有尖峰持续存在，做一次同配置的 Windows PC VR Player 对照运行，用相同航段区分 Editor 开销与游戏本身开销；记录构建版本和 Quest Link 状态。Player 路径与 Editor 路径各自写新 session，不混为一次实验。
3. 将旧 macOS 分析导出能力按当前 Windows JSONL 结构迁入 `E:\Audio-paper\分析工具`，至少完成条件/区块、转向提示、视觉任务、识别 RT、缺失与设备失败的可审计导出；核对事件数、重复、`session_end`、分母和异常值。现有 `查看会话.ps1` 只做基础结构检查。
4. 若用户确定新和弦用于 pilot，记录 Quest 3 实际输出设备、系统音量、测得的声压级（SPL）、信噪比（SNR）、左右可辨性和舒适度。`gentle_f_major_chord_v1` 仍是技术候选，任何声学校准或刺激冻结都需单独留证。

### P2：正式研究与交付整理

1. 完成 feasibility pilot、研究条件和路线分配、样本量/精度规划、设备校准及伦理 gate，再决定是否将该转向声写入正式 Methods；技术会话、试听偏好与正式被试结果分开。
2. 在 Windows 与 macOS 分别核验场景打开、资源导入、正式导出流程与文件夹入口；不要把本次 Quest Link Play Mode 结果写成 Android 原生 Quest 构建结果。
3. 将已经实测通过的参数并入项目正式原理文档和使用手册；保留本 handoff 的过程和被替代方案，但以当前源码、实际数据及之后的新验证记录为事实来源。

## 文件、回退与核验入口

| 目的 | 路径 |
|---|---|
| Windows 工程 | `E:\Audio-paper\Audio-Vr\Audio-VR-Windows-project-20260924` |
| 主场景与声音版本 | `Assets/AudioV0.unity`、`Assets/Scripts/AudioV0ExperimentProfile.cs` |
| Quest 手柄/UI | `Assets/Scripts/AudioV0QuestControllerUIBootstrap.cs` |
| 转向调度/空间声 | `Assets/Scripts/AudioV0RuntimeAudioDirector.cs`、`Assets/Scripts/AudioV0RuntimeScenario.cs` |
| 当前 WAV 与生成器 | `Assets/Resources/AudioV0Stimuli/TurnCues/gentle_chord_turn_v1.wav`、`sound_design/generate_gentle_chord_turn.py` |
| 试听生成路径（当前文件缺失） | `temp/turn-cue-preview/gentle_chord_v1_3s_preview.wav`，由上列生成器定义 |
| 渲染及城市 | `Assets/Settings/PC_Renderer.asset`、`Assets/Settings/PC_RPAsset.asset`、`Assets/Scripts/AudioV0EnvironmentAssets.cs` |
| 实验数据与分析 | `E:\Audio-paper\实验数据`、`E:\Audio-paper\分析工具\查看会话.ps1` |
| 项目内增量记录 | `docs/audio-v0-handoff.md`、`docs/audio-v0-user-manual.md` |
| 临时备份（当前目录缺失） | 旧项目记录提到 `Temp/codex-recovery/`，2026-09-25 检查时该目录不存在 |

工程含其他未提交改动，**不要用整目录覆盖当前工程**。当前没有核实到上述临时备份；修改前确认 Unity 已退出 Play Mode，先检查当前文件和 Git 差异，再只回退明确造成问题的单个设置。最近一次 Unity 编译已成功，但本轮没有做完整四航程、Quest 3 新和弦听感或正式实验验收。
