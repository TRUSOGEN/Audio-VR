# Audio V0 可复用实验模板

## 目的

本项目已经把“体验 demo”提升为可复制的实验场景模板。复制场景后，保留系统契约并替换客舱模型、外部环境或路线锚点，即可重新运行同一套四条件、事件记录、视觉任务、评分、导出与 VR UI 流程。运行时不再依赖场景名必须叫 `AudioV0`，只要新场景保留 `Systems` 契约就会自动初始化。

## 场景契约

新场景应保留以下对象与组件：

| 对象 | 必需内容 | 替换时允许改变 |
|---|---|---|
| `Systems` | `EventLogger`、`RoutePlayer`、`FlightStateMachine`、`NotificationAudio`、`AudioV0ExperimentProfile`、`AudioV0DataRepository` | Profile 参数、音频资源、条件定义 |
| `passengercabin` | 作为 `RoutePlayer.movingRoot` 的移动根节点 | 子级的 UAM 客舱或飞行器模型、材质、座椅、窗景 |
| `XR Origin (VR)` | 位于移动根节点下，主相机和 InputActionManager 保持可用 | HMD 高度和相机 offset |
| `RouteEnvironment` | 地面、城市或风景 | 完整外部场景和落地点模型 |

不要改动 `Systems`、移动根节点或 XR Origin 的父子关系；替换模型时只替换 `passengercabin` 下的可视子物体。四条运行时路线、低多边形地标和环境会自动生成，因此新的场景不需要手工复制临时路线对象。

## Profile 参数

在 Inspector 选择 `Systems > AudioV0ExperimentProfile` 可调整，无需改代码：

- `visualTargetWindowSeconds`：视觉 target 的有效显示窗口，当前为 2.2 秒。
- `visualInterStimulusSeconds`：每次刺激结束到下一次 onset 的最短间隔，当前为 2.8 秒。
- `identificationWindowSeconds`：音频阶段识别的答题窗口，当前为 8 秒。
- `visualTargetRate`、`randomSeed`、版本号：冻结后应写入研究记录，确保可复现。
- `showRouteGuidesAtStart`：正式参与者运行默认关闭；操作员可使用 UI 的显示/隐藏路线按钮临时查看。
- `enableXRPresentation`、`xrPanelDistanceMeters`、`xrCanvasScale`：OpenXR 设备激活时的头显 UI 参数。

## VR 运行状态

项目已安装 `com.unity.xr.interaction.toolkit`、`com.unity.xr.management` 和 `com.unity.xr.openxr`。当前场景包含一个 `XR Origin (VR)`、`XR Interaction Manager`、带 `XRUIInputModule` 的 EventSystem，以及挂在主相机上的 `AudioV0 Gaze Ray`。运行时检测到激活的 OpenXR 设备时，UI 会从桌面 overlay 切换为头显前方的 world-space panel，并使用 Tracked Device Graphic Raycaster 接收 XR 指向输入。

桌面环境只能验证 XRI 场景配置，不能代替真实 HMD 的追踪、触发器和舒适度测试。首次接入目标头显时，应运行一个完整 block，检查凝视射线、trigger 点击、F1/隐藏界面、空间转弯音效和退出/日志写入。

## 数据仓库

所有运行使用 `Application.persistentDataPath/AudioV0/`，Windows 当前对应：

`C:/Users/Trusogen/AppData/LocalLow/DefaultCompany/My project/AudioV0/`

这是 Windows 示例路径。跨平台代码应使用 `Application.persistentDataPath/AudioV0/`；macOS 的实际目录由 Unity 运行时决定，实验员应从最终页复制当前 session 路径，不要手写 Windows 路径。

技术测试数据固定放在 `technical_demo/` 下，每个 session 文件夹包含：

- `event_log.jsonl`：不可手改的原始事件流。
- `session_manifest.json`：session ID、数据分类、创建时间、参与者占位 ID 与 schema 版本。

最终页可复制本次 JSONL 路径、打开数据根目录，并一键清理已完成的技术测试数据。清理操作拒绝在日志仍打开时执行，且不清理未来独立的 `pilot/`、`participant_run/` 数据分类。

## 每次正式采集前

1. 冻结 Profile 的版本、随机种子、路线、视觉窗口和音频资源版本。
2. 设置 `EventLogger.storageCategory` 为 `pilot` 或 `participant_run`，不要继续使用 `technical_demo`。
3. 核对 headset、耳机、OpenXR runtime、声压校准和参与者编号。
4. 完成后检查 `session_manifest.json`、`event_log.jsonl`、`session_end` 和 `experiment_sequence_complete`。
