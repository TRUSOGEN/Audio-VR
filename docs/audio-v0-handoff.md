# Audio V0 / UAM Passenger Lab Handoff

更新时间：2026-09-10  
当前场景：`Assets/AudioV0.unity`  
当前定位：桌面 technical demo，已具备可复制实验模板、基础 VR/XRI 配置与结构化日志；不是已完成的正式被试研究。

## 目标与当前体验

项目模拟 UAM 乘客在起飞、巡航、转弯、下降和着陆过程中的音频通知、阶段识别与视觉副任务。一次参与者运行由四个连续 block 构成：

1. 选择路线和初始条件，点击“开始 4 段实验”。
2. 每个 block 按路线飞行，包含 3 次音频阶段通知、视觉刺激和空间转弯声效。
3. 飞行结束后完成 6 个 1–7 分探索性评分题。
4. 前三个 block 通过“开始下一段”继续；第四段结束显示感谢页、日志路径、数据目录和技术数据清理入口。

四个条件为 `NS_Q`、`NS_N`、`S_Q`、`S_N`。初始选择会成为第一段，之后的条件和路线以无重复顺序完成余下三段。

## 当前重要实现

| 能力 | 实现位置 | 当前行为 |
|---|---|---|
| 四段连续流程 | `Assets/Scripts/AudioV0ExperimentSequenceController.cs` | block 评分结束后显式提示下一段；最终显示谢幕和复制路径动作。 |
| 视觉副任务 | `Assets/Scripts/VisualTaskController.cs` | target 默认显示 2.2 秒，刺激间隔默认 2.8 秒，target rate 默认 0.25。 |
| 阶段识别 | `Assets/Scripts/IdentificationController.cs` | 仅在音频通知后显示答题卡，默认答题窗口 8 秒，记录 `response_time_ms`。 |
| UI | `Assets/Scripts/AudioV0RuntimeDemoUI.cs` | 中英切换、F1/HIDE UI、视觉/识别/评分/连续流程覆盖层、路线开关、最终数据管理按钮。 |
| 路线和城市 | `Assets/Scripts/AudioV0RuntimeScenario.cs` | 四条 12 waypoint 路线，S 弯、升降、低多边形地标、运行时城市与 landing pad。 |
| 转弯空间声 | `Assets/Scripts/AudioV0RuntimeAudioDirector.cs` | 左右相对客舱位置播放 3D turn cue，记录 `spatial_turn_cue`。 |
| 实验参数 | `Assets/Scripts/AudioV0ExperimentProfile.cs` | 场景级 Inspector Profile，集中管理时长、随机种子、路线速度、通知 lead、路线可见性和 XR UI 参数。 |
| 数据仓库 | `Assets/Scripts/AudioV0DataRepository.cs`、`EventLogger.cs` | session 按分类归档，最终页支持打开目录和删除技术测试数据。 |

## 场景复制与换模型

运行时不再依赖场景名必须为 `AudioV0`。任何新场景只要保留以下契约，就会在启动时自动初始化：

- `Systems`：至少保留 `EventLogger`、`RoutePlayer`、`FlightStateMachine`、`NotificationAudio`、`AudioV0ExperimentProfile` 和 `AudioV0DataRepository`。
- `passengercabin`：必须继续是 `RoutePlayer.movingRoot`，可替换其下的可视模型、材质、座椅和窗景。
- `XR Origin (VR)`：应保持位于移动根节点下，使头显随客舱路线移动。
- `RouteEnvironment`：可换成新的城市、山地、海岸或测试场景。

推荐做法是复制 `Assets/AudioV0.unity`，替换 `passengercabin` 的模型子物体和 `RouteEnvironment` 的内容，不改变 `Systems`、`RoutePlayer.movingRoot` 或 XR Origin 的父子关系。

## 参与者呈现和路线开关

- 默认 `showRouteGuidesAtStart=false`，所以参与者不会看到彩色路线线条或运行时路标。
- 顶部“显示路线 / SHOW ROUTE”按钮仅用于操作员检查；再次点击可隐藏。
- 每次开关写出 `route_guides_visibility`，但不会改写 waypoint、路线时间、路线完成逻辑或实验结果。
- 视觉 target 已放慢到 2.2 秒；pilot 可在 `Systems > AudioV0ExperimentProfile` 调整 `visualTargetWindowSeconds` 和 `visualInterStimulusSeconds`，并在冻结后记录版本。

## VR 状态

已确认项目包含：

- `com.unity.xr.interaction.toolkit` 3.6.0。
- `com.unity.xr.management` 4.7.0。
- `com.unity.xr.openxr` 1.18.0。
- 场景中的 `XR Origin (VR)`、`XR Interaction Manager`、`EventSystem + XRUIInputModule`。
- 主相机下的 `AudioV0 Gaze Ray`，含 `XRRayInteractor` 与 line visual。

最近一次 `xr_check_setup` 返回：`issueCount=0`、`interactorCount=1`。当真实 OpenXR 设备激活时，运行时 UI 使用 world-space Canvas，固定在头显前方，使用 `TrackedDeviceGraphicRaycaster` 接收 XR 指向输入；未接头显时保持桌面 Screen Space UI。

尚未完成真实 HMD 实机验证。下一个执行者应使用目标头显完成一整个 block，重点检查：HMD 追踪、凝视射线/trigger 点击、世界空间 UI 尺寸、F1/Hide UI、空间转弯声效、退出与日志闭合。

## 数据目录与清理

Windows 当前数据根目录：

`C:/Users/Trusogen/AppData/LocalLow/DefaultCompany/My project/AudioV0/`

该路径只是 Windows 示例。运行时实际使用 `Application.persistentDataPath/AudioV0/`，macOS 会自动写入 macOS 的持久化数据目录；不要把 Windows 绝对路径复制到 Mac。`AudioV0DataRepository` 已对 Windows 文件资源管理器和其他平台的目录打开方式分支处理。

## Git 与 macOS 迁移

项目根目录现在包含 `.gitignore`、`.gitattributes` 和 `README.md`。Git 应提交 `Assets`、`Packages`、`ProjectSettings`、`docs`、README 及所有 `.meta` 文件；`Library`、`Temp`、`Logs`、`Obj`、`UserSettings`、`.vs` 和 `.plastic` 是本机生成或 Plastic SCM 元数据，不应跨机器复制。

项目版本固定在 `ProjectSettings/ProjectVersion.txt` 的 `6000.6.0f1`。Mac 应安装完全相同的 Unity Editor 版本和对应架构；首次打开需要重新解析 Git URL 包并重建本机 `Library`。编辑器能打开不等于目标 OpenXR 头显、音频设备和 UnitySkills relay 已在 Mac 上验证。

新 technical demo session 结构：

```text
AudioV0/
  technical_demo/
    session_<UTC timestamp>_<UUID>/
      event_log.jsonl
      session_manifest.json
```

`event_log.jsonl` 是唯一原始审计数据，禁止手改。`session_manifest.json` 存 session ID、分类、创建时间、参与者 ID 与 schema。最终感谢页支持：

- 复制本次 JSONL 完整路径。
- 打开数据根目录。
- 删除 `technical_demo` 和旧版顶层 `session_*` 技术日志。

清理逻辑会拒绝删除仍由 EventLogger 写入的 session，且不会触及未来的 `pilot`、`participant_run` 分类。正式 pilot/正式数据采集前，必须在 `EventLogger.storageCategory` 改为 `pilot` 或 `participant_run`。

已验证的新目录样例：

`C:/Users/Trusogen/AppData/LocalLow/DefaultCompany/My project/AudioV0/technical_demo/session_20260910_050745_617_3500a59407d44bec8a6dffc5b768c35d/`

该样例确认 `session_manifest.json` 已生成，且路线默认隐藏后可通过 UI 打开并写入两条 `route_guides_visibility` 事件。

## 已验证证据

- Unity：6000.6.0f1。
- UnitySkills：当前服务健康，Full/Bypass；脚本重编译时端口可能在 8090/8091 间重启，先查 `/health` 再调用。
- 已检查以下脚本，均为 0 compile errors：
  - `AudioV0RuntimeDemoUI.cs`
  - `AudioV0RuntimeScenario.cs`
  - `AudioV0DataRepository.cs`
  - `AudioV0ExperimentProfile.cs`
  - `EventLogger.cs`
  - `VisualTaskController.cs`
- Play Mode smoke test 确认 `IsConfigured=true`、`RouteGuidesVisible=false`、视觉时长为 2.2 秒、间隔为 2.8 秒；停止 Play Mode 后 Console 为 0 errors。
- 四段流程的上一轮完整回归日志：
  `C:/Users/Trusogen/AppData/LocalLow/DefaultCompany/My project/AudioV0/session_20260910_042708_674_9dbaf752a541416d853430231b277f56/event_log.jsonl`。
  该日志包含 4 条 `experiment_block_complete`、1 条 `experiment_sequence_complete`、24 条 `block_rating_response` 和 `session_end`。

## 研究边界

当前记录骨架覆盖桌面技术 demo 的 G01–G06 主要事件。它不能代替以下正式研究证据：

- 语音与非语音刺激的最终资源、许可和 hash。
- 耳机/声卡 SPL、SNR 与实际播放 onset 校准。
- 正式 condition × route × position allocation。
- HMD 性能、舒适度与 motion sickness 记录。
- 正式问卷、consent/ethics 与被试 pilot。

不要把当前 demo 的运行日志或 0 error 结果报告为论文正式实验结果。

## 推荐下一步

1. 接入目标 VR 头显，完成 XR 实机 smoke test，并记录设备与 OpenXR runtime 版本。
2. 用 2–3 名 pilot 调整视觉窗口、刺激间隔和路线速度；冻结 Profile 后不再随意修改。
3. 把正式 speech/non-speech 音频、噪声版本、SPL/SNR 测量表和许可信息纳入 manifest。
4. 为 `pilot` 与 `participant_run` 分类建立导出脚本，将 JSONL 稳定派生为 `blocks.csv`、`transitions.csv`、`visual_task.csv` 和 `block_ratings.csv`。
5. 用一轮完整的四 block VR pilot 复核路线隐藏、数据分类、清理按钮与退出恢复。

## 相关文档

- `docs/audio-v0-experiment-template.md`：复制场景、Profile、VR 和数据仓库的使用说明。
- `docs/audio-v0-gate-measurement.md`：G00–G12、测量口径、参与者操作和数据位置。
- `docs/audio-v0-codex-execution.md`：原始执行要求、gate 与早期验证过程。
- `temp/audio-v0/20260910-rich-demo/validation_report.md`：丰富 demo 的实现与回归证据。
