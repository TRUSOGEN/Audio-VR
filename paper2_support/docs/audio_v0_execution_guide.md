# Audio Experiment V0 执行指南

日期：2026-09-08  
适用范围：Paper #2 Audio Experiment 的 Unity/VR V0 prototype、dry run 和 pilot preparation

## 2026-09-24 软件使用入口

实际桌面操作、四航程、数据查看与故障处理以 [Audio V0 使用手册](/Users/trusoegn/GitHub/Audio-VR/Audio-VR/docs/audio-v0-user-manual.md) 为当前入口。本文件继续保留早期工程 gate 与设计依据。

## 2026-09-23 当前执行入口

本指南下方连续路线/状态 predicate 为早期 V0 历史。当前候选流程是四段完整地面到地面航程；独立片段、目标有放回抽取和共用前段只保留作历史/显式技术选项，详见 `participant_runbook_and_data_dictionary.md` 顶部及 `protocol_freeze_record.md` §0.7。技术设置不代表 pilot 或正式参数冻结。

## 1. 先记住这一句话

本项目要做的是一个自动飞行的乘客体验模拟器，不是 pilot flight simulator。

乘客不驾驶 eVTOL，也不需要理解空气动力学。V0 只需要让乘客坐在 cabin 中，看到一条可重复的自动飞行路线，在三个 flight-phase transition 期间完成通知识别和视觉任务，并把所有事件导出为可审计日志。

本指南是执行顺序，不是最终 recruitment protocol。所有标记为 `provisional`、`prototype candidate` 或 `pilot-dependent` 的内容都不能直接写成正式实验事实。

## 2. 当前技术决策

### 2.1 采用的方案

- 使用 Unity 作为 simulator platform。
- 使用 `3D (URP)` 作为项目基础模板；VR 能力通过 XR/OpenXR 配置接入，二者不是互斥选择。
- 使用 fixed-base seated VR：参与者坐在普通椅子上，佩戴 VR headset 和耳机，不需要 driving simulator、车架或 motion platform。
- 使用固定 passenger cabin、固定 camera 和预定义 route。
- 使用 `RoutePlayer` 播放自动飞行轨迹，不实现完整飞行动力学。
- 使用 `FlightStateMachine` 管理三个状态：`cruise_entry`、`descent_begin`、`landing_preparation`。
- 使用状态 predicate 触发 transition，不使用未记录的独立 timer。
- 使用耳机播放 speech、non-speech 和 simulated cabin noise。
- 使用单独的 visual-task control 和 identification control。
- 所有 transition、notification、input、response、pause 和 error 都写入可导出事件日志。

### 2.2 暂时不做的内容

- 不实现 pilot control、throttle、pitch、roll 或 yaw 操作。
- 不实现真实空气动力学、旋翼升力、天气、湍流或飞行控制器。
- 不购买或搭建 driving simulator、汽车座舱、六自由度运动平台或实体 eVTOL cabin。
- 不购买或拍摄 `Insta360 Pro 2` 8K 360°视频。
- 不使用 motion platform 或复杂 physical motion cueing。
- 不把 eVTOL 外观模型当作研究变量。
- 不在 prototype 阶段冻结最终 headset、音量、SPL/SNR、response window、N 或统计模型。

## 3. 原型的最小架构

```text
Unity Scene
  ├── PassengerCabin
  │     ├── seat
  │     ├── windows
  │     ├── passenger camera
  │     └── optional passenger display
  ├── RoutePlayer
  ├── FlightStateMachine
  ├── NotificationAudio
  ├── VisualTask
  ├── InputFocusController
  ├── ConditionController
  └── EventLogger
```

运行时的数据流应为：

```text
RoutePlayer
    -> FlightStateMachine
    -> transition_onset
    -> NotificationAudio + VisualTask
    -> identification / visual-task response
    -> EventLogger
```

## 4. 环境和 eVTOL 模型怎么处理

### 4.1 第一版允许没有完整 eVTOL 模型

第一版可以用 Unity primitive 搭建 cabin blockout，包括地板、座椅、窗口、内壁和 passenger camera。此版本的目标是验证座位视角、窗口运动、输入、状态触发和日志，不是展示最终设计。

### 4.2 eVTOL 模型候选

当前可检查的 passenger-oriented candidate 是 [VTOL Air Taxi by Annelida](https://sketchfab.com/3d-models/vtol-air-taxi-ecb6d12a90514aab9105061a34c7d604)。页面说明该模型包含一定的内部空间，许可证为 CC Attribution，但下载后必须检查是否真的有可用座椅、窗口、地板和乘客视角，并把来源记录进 asset register。

该模型约有 44 万个三角面，若用于 Quest 2，需要先测试和优化，不能直接假定适合 standalone VR。

可作为外部模型候选的 [amvlab/aircraft-models](https://github.com/amvlab/aircraft-models) 提供 `.GLB` eVTOL，许可证为 CC BY 4.0，但不能假定它包含完整 passenger cabin。

NASA [RAVEN](https://www.nasa.gov/raven/) 模型是公开的 eVTOL research aircraft geometry，可用于外形参考，但它不是本研究所需的 passenger air-taxi cabin，不作为默认模型。

### 4.3 模型验收规则

模型只有同时满足以下条件，才可以进入 V0 scene：

- 能在当前 Unity Editor 中成功导入或经过可复现转换导入。
- passenger camera 能放在合理的座位位置。
- 窗口和 cabin 内壁不会遮挡 participant view。
- 材质、贴图和 collider 不会造成明显渲染错误。
- 记录来源、作者、许可证、下载日期、文件版本和修改内容。
- 有一个低复杂度版本，能在目标设备上保持稳定运行。

如果模型不满足这些条件，回退到 primitive cabin，不要为了模型继续阻塞 route 和日志开发。

## 5. 执行 Gate 0：确认开发环境

### 任务

1. 确认用于开发的 computer、显卡、存储空间、目标 VR headset 和耳机。
2. 安装 Unity Hub。
3. 安装项目计划使用的 Unity Editor，并记录实际版本。
4. 选择需要的 build module，暂时不安装无关平台。
5. 创建一个空白 Unity project 并成功打开。

### 验收证据

- Unity Hub `Installs` 页面截图。
- Unity Editor 空白 project 成功打开的截图。
- Editor version、render pipeline、build target 和任何 error text 的记录。

### 通过条件

在没有稳定打开空白 project 之前，不进入 Cesium、模型或 VR input 集成。

普通椅子即可作为初始 seated setup。VR headset 负责视野，耳机负责声音，Unity 负责自动路线、状态触发和日志；物理车架不是本项目的前置条件。

## 6. 执行 Gate 1：建立最小 Unity 场景

### 6.1 先完成 VR smoke test

`3D (URP)` 只是渲染和项目基础模板。空白项目打开后，再通过 Package Manager 安装 XR Plug-in Management，并根据目标 headset 启用 OpenXR 或厂商要求的 XR provider。

第一轮只需要建立一个最小 VR scene，确认 headset 能看到 Unity 场景、头部转动能更新 camera，并能安全退出 Play mode。暂时不加入 Cesium、eVTOL 模型、声音或正式实验任务。

如果目标 headset 尚未确定，先保留 desktop mode，同时记录 VR provider 为 `provisional`，不要把当前配置写成最终实验设备。

### 推荐资源

使用 [Cesium for Unity Samples](https://github.com/CesiumGS/cesium-unity-samples) 作为城市环境学习和 smoke test 起点。它包含 Unity 场景、城市数据和 VR sample，但 Cesium ion 数据可能需要网络或账户，因此正式实验前必须考虑 preload、cache 或 localise。

### 任务

1. 下载或 clone Cesium sample project。
2. 记录 repository URL、commit/release、license 和使用的场景。
3. 打开 sample scene，先在 desktop mode 运行。
4. 确认城市、地形、camera 和基本 rendering 正常。
5. 建立项目自己的 `AudioV0` scene，不直接修改原始 sample scene。
6. 放入 primitive cabin 和 passenger camera。
7. 设定一个固定 seated viewpoint，并记录 camera height、seat position 和 forward direction。

### 验收证据

- sample scene 成功运行截图。
- `AudioV0` scene 的 hierarchy 截图。
- cabin、camera 和城市环境的运行截图。
- `asset_register` 初始记录。

## 7. 执行 Gate 2：实现自动 route 和三个状态

### RoutePlayer

RoutePlayer 只负责沿预定义 waypoints 或 curve 移动 cabin/root object，并记录 route time。第一版可以使用固定速度和固定时间，不需要真实飞行控制。

### FlightStateMachine

每个 transition 必须有版本化 route-state manifest，至少记录：

- `state_source`
- `entry_state_id`
- `entry_predicate`
- `exit_state_id` 或 route-end predicate
- `allowed_predecessor_state_id`
- `route_time_reference`
- `interruption_path`

### 状态规则

- `cruise_entry` 只能在允许的前置状态之后触发。
- `descent_begin` 只能在 cruise 状态和持续下降 predicate 成立后触发。
- `landing_preparation` 只能在指定 landing-preparation predicate 成立后触发。
- 每个 transition 在同一个 block 内只能触发一次。
- route restart、pause/resume、missing state 和 state-order violation 必须生成 interruption 或 error event。
- 不允许通过未记录的独立 timer 重新播放 transition。

### Gate 2 验收

无 notification、无 visual task 的 silent route 必须能够完整运行，并导出三次唯一的 `transition_onset`，每次都有唯一 `transition_id` 和 state evidence。

## 8. 执行 Gate 3：实现 EventLogger

每行事件至少包含：

```text
session_id
pseudonymous participant_id
block_id
event_id
event_sequence
route_id
block_position
condition_id
notification_design
listening_condition
transition_target
transition_id
event_type
timestamp_ms
monotonic_clock_ms
stimulus_set_version
application_version
write_status
```

必须分开记录：

- `transition_onset`
- `notification_onset`
- `notification_offset`
- `identification_submitted`
- `visual_task_stimulus`
- `visual_task_response`
- `input_accepted`
- `input_rejected`
- `pause`
- `withdrawal`
- `error`

事件之间通过 `transition_id`、`parent_event_id` 和 `event_sequence` 关联，不用 participant 的回忆补齐事件时间。

### Gate 3 验收

- 日志可以从 application 导出。
- 导出文件可以打开。
- 三个 transition 的 onset、response 和 event order 可追踪。
- application version、route version、stimulus version 和 clock source 都写入 manifest。
- 写入失败不会静默丢弃事件。

## 9. 执行 Gate 4：加入音频

### 顺序

1. 先加入 `technical only` test clip。
2. 验证 requested start、actual playback callback、onset 和 offset。
3. 加入 V0 speech strings。
4. 加入三个 non-speech candidate cues。
5. 加入 quiet/noise switch。
6. 记录 audio device、asset ID、stimulus version 和 playback result。

V0 speech wording 和 non-speech cues 都是 prototype candidates，不是最终 participant-facing stimuli。技术测试音频不能被当作 study notification 评价。

### 错误规则

以下情况必须记为 error，而不是当作通知已发送：

- missing asset
- muted output
- missing playback callback
- audio-device change
- unexpected playback interruption
- quiet/noise file missing

mix level、calibration、SPL/SNR 和 final headphones 保持 pilot-dependent。

## 10. 执行 Gate 5：加入 visual task 和 participant input

### Visual task

使用一个不会遮挡 flight view 的 peripheral target-detection overlay 作为 candidate。它必须记录 target/non-target、onset、response、overlay state 和 stimulus-set version。

visual stimulus 与 notification 同时发生时，系统必须选择并记录 retain、defer 或 suppress 其中一种行为及其理由。

### Input priority

系统应按以下优先级处理同一帧输入：

1. system safety/menu
2. pause
3. identification response
4. visual-task response

`identification`、`visual_task_response` 和 `pause` 必须使用分开的 configurable bindings。系统必须记录 accepted input、rejected input、control ID、focus state、debounce result 和 rejection reason。

### Gate 5 验收

- participant 可以在 route 继续时完成 visual task。
- participant 可以提交 transition identification。
- 同时输入测试符合优先级规则。
- rejected input 会被记录而不是静默丢弃。
- pause 后恢复不会重复触发同一个 transition。

## 11. 执行 Gate 6：条件控制和故障路径

每个 block 开始前必须写入：

```text
notification_design: non_speech | speech
listening_condition: quiet | simulated_cabin_noise
condition_id: NS_Q | NS_N | S_Q | S_N
route_id
block_position
stimulus_set_version
noise_version
control_binding_version
```

完整 V0 dry run 的 condition table 必须包含四个 `condition_id` 各一次，防止 duplicate、missing 或 unlabeled block。

如果缺少 cue、route、noise、audio device 或 log path，application 必须停止 block，显示稳定 error code，记录 retry 是否允许，并保留 incomplete partial log。

## 12. 执行 Gate 7：完整 operator dry run

只有 Gate 0 至 Gate 6 通过后，才运行四条件完整 dry run。

每次 dry run 应保留：

- operator ID 或 test mode
- application version
- route-state manifest
- condition allocation table
- session manifest
- raw event log
- exported event log
- error-path log
- pause/resume log
- asset and licence register
- dry-run checklist

### Dry-run 通过条件

- route 三个 transition 各触发一次。
- 四个 condition 各出现一次。
- 每个 notification 有 onset 和 offset。
- visual task 和 identification response 都有事件记录。
- simultaneous input test 通过。
- intentional missing asset/device/log-path test 产生稳定 error 和 partial log。
- clean block completion、pause 和 error pathway 都能运行。
- 导出的日志字段完整，并与 operator condition record 一致。

## 13. 执行 Gate 8：pilot 和 freeze

所有 dry-run item 通过后，再进行 2–3 次 end-to-end pilot。pilot 只用于检查 instructions、sound learning、speech wording、audibility、comfort、startle、motion sickness、visual-task difficulty、route timing、logging completeness、block duration、fatigue 和 floor/ceiling risk。

每次 pilot 必须保留一份 checklist，并由 reviewer 做出 `freeze`、`iterate` 或 `block` 决定，写明证据。

pilot 不是正式效果量证据，也不能用来直接证明 notification design 有效。

正式招募前必须冻结：

- final stimuli
- noise and calibration chain
- visual task
- control bindings
- training criterion
- response window
- scoring
- route-condition allocation
- exclusion and replacement rules
- questionnaire wording and anchors
- analysis specification
- sample-size or precision assessment

## 14. 暂时不要修改论文的内容

在 Gate 7 和 Gate 8 完成前，不把以下内容写成已验证事实：

- final eVTOL model
- final headset
- final Unity version as study apparatus
- final route timing
- final sound duration or mix
- final speech wording
- final noise level
- final visual-task difficulty
- final response window
- final session duration
- final N or power justification
- completed pilot
- completed ethics coverage

只有已经通过 prototype、dry run 或 pilot 证据验证的 implementation facts，才可以回写 `methods.tex`。

## 15. 现在立即执行的十个动作

1. 确认 Windows development computer 和目标 headset 是否可用。
2. 安装 Unity Hub 和记录实际 Editor version。
3. 打开一个空白 Unity project，保存截图和 error text。
4. 下载 Cesium for Unity Samples 并记录 release/commit/license。
5. 在 desktop mode 打开 Cesium sample scene。
6. 创建自己的 `AudioV0` scene。
7. 用 primitive 建立 passenger cabin、座椅、窗口和 camera。
8. 先不放声音，建立一条固定 silent route。
9. 实现三个 state-triggered transitions 和最小 event log。
10. 只有 silent route 和 log 通过后，再处理 eVTOL model import、audio 和 visual task。

## 16. 完成判据

当前阶段的完成不是“看起来像真的飞行”，而是另一名研究者可以打开项目、运行一条自动 passenger route、看到三个有定义的 flight-phase transitions，并从导出日志中核对每一次 transition、notification 和 response。

相关基线材料：

- [Audio Experiment Unity/VR V0 完整搭建手册](audio_v0_unity_build_walkthrough.md)
- [Audio Experiment V0 Protocol and Prototype Checklist](audio_v0_protocol_and_prototype_checklist.md)
- [Audio Experiment Full Audit Handoff 2026-09-07](../handoff/audio_experiment_full_audit_20260907.md)
