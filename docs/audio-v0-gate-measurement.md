# Audio V0 Gate 测量与记录方案

本文把论文中的 G00–G12 拆成“Unity 能记录什么”和“必须在 Unity 外测什么”。当前项目是 technical demo，JSONL 能证明软件链路，不等于真实声学、VR 舒适度或被试实验已经完成。

## 统一数据源

每次运行保留一份原始 `event_log.jsonl`，不覆盖旧文件。公共字段包括 `session_id`、`participant_id`、`block_id`、`attempt_id`、`event_id`、`event_sequence`、`timestamp_ms`、`monotonic_clock_ms`、`route_time_ms`、`route_id`、`route_version`、`block_position`、`condition_id` 和 `write_status`。

分析时从 JSONL 派生表，不手工改原始日志：

- `blocks.csv`：每个 block 的条件、路线、开始/结束、完整性和错误。
- `transitions.csv`：T01/T02/T03 的通知、transition onset、target、回答、正确性和反应时。
- `visual_task.csv`：刺激 onset/offset、target、response、hit、miss、false alarm、correct rejection。
- `block_ratings.csv`：每个 block 的 6 个探索性 1–7 分条目；题目、方向和量表范围与回答分开保留。
- `audio_calibration.csv`：设备、声卡、耳机、SPL、SNR、校准日期和 operator；这些字段不能用 Unity 软件音量代替。
- `validation_report.json`：每个 gate 的 pass/fail、规则版本、日志路径和 reviewer。

## Gate 对照表

| Gate | 要测的指标 | Unity 当前体现 | 仍需在 Unity 外完成的证据 |
|---|---|---|---|
| G00 | UnitySkills health、项目、场景、Console、编译状态 | 工具健康检查和编辑器验证 | 无，被视为工程环境 gate |
| G01 | ID 唯一、sequence 连续、时间单调、文件可写、异常退出 | `EventLogger`、JSONL、`write_status`、`session_end` | 故障注入和备份/恢复测试 |
| G02 | 路线段、位置、升降、转弯、predicate、route clock、完成状态 | `RoutePlayer`、`experience_phase`、`transition_onset`、`route_complete` | 真实视点、几何可比性和路线 manifest 审核 |
| G03 | scheduled/requested/onset、软件 lead time、pause interruption | `NotificationAudio`、`audio_phase_change`、通知生命周期事件 | 耳机/声卡实际 onset、SPL、设备延迟 |
| G04 | Start/Pause/Resume/End、block 完整性、条件锁定 | `SessionController`、`block_started`、`block_end`、pause/resume | operator checklist 和正式场景/XR 绑定 |
| G05 | 每个 transition 的 selected target、communicated target、correct、response time、missing | `IdentificationController` 写入 `identification_window_open`、`identification_response`、timeout/reject | 正式三选一界面、控制器输入和 pilot response window |
| G06 | 刺激数量、固定窗口、hit/miss/FA/CR、输入冲突 | `VisualTaskController` 写入 visual onset/offset/response，UI 显示计数 | 最终任务难度、正式刺激集和 pilot 可行性 |
| G07 | 四条件资源、噪声开关、刺激版本、音频 manifest | `ConditionController` 锁定 NS_Q/NS_N/S_Q/S_N，资源缺失会拒绝启动 | 三个 speech、三个 non-speech、许可/hash、SPL/SNR 校准 |
| G08 | 练习、熟悉化、休息、问卷、退出路径 | 部分流程控制器已存在，technical demo 有 block 操作 | 完整 participant flow、问卷和 consent/ethics |
| G09 | condition×position、route×condition、route×position、Williams 顺序 | `RouteAllocationPlanner` 有候选生成/唯一性检查 | 冻结正式 allocation 表并由 reviewer 签字 |
| G10 | FPS、掉帧、头部跟踪、控制器、comfort、build | 目前不作为完成 gate | 实际 HMD、目标电脑、渲染和舒适度记录 |
| G11 | 4 blocks、12 transitions、12 通知生命周期、完整导出 | 当前只完成 technical demo 单路线完整 block | 四条件端到端干跑和导出重建 |
| G12 | 2–3 次 pilot 的可行性、audibility、startle、fatigue、motion sickness、日志完整性 | Unity 可提供事件原始数据 | 人工 checklist、reviewer、freeze/iterate/block 决定 |

## 现在如何测量

1. 先跑 G01–G06 的桌面技术 block，保存原始 JSONL，不把 `TECH_TEST` 混入正式四条件数据。
2. 用 `event_sequence`、`event_id`、`session_id` 和 `attempt_id` 检查日志完整性；任何 `error`、`input_rejected`、`notification_interrupted` 都进入异常清单，不删除。
3. 对每个 T01/T02/T03 配对 `notification_onset` 与 `transition_onset`，计算软件 `lead_time = transition_route_time - notification_route_time`，并单独标记 `playback_observed` 和 `estimated_clip_duration`。
4. 对识别事件按 `accepted=true`、`correct=true/false`、timeout 和 rejected 分母计数；`response_time_ms` 已写在 `identification_response` 中，当前锚定软件通知 onset。它在正式论文中只能叫 software RT；回录/声卡确认的 `measured_onset` 才能支撑声学 RT。
5. 对视觉任务按 stimulus ID 重建固定窗口，计算 hit、miss、false alarm、correct rejection 与 `visual_response.response_time_ms`；暂停或 block 切换产生 interrupted，不强行计入 miss。
6. 对每个完成 block 保存 `block_rating_response`：`cue_clarity`、`cue_audibility`、`cue_annoyance`、`flight_comfort`、`perceived_workload`、`transition_confidence`。它们是探索性单项，不替代正式 NASA-TLX、SSQ 或经验证的接受度量表。
7. 只有当 Unity 外的校准表、设备表、allocation 表、pilot checklist 和 reviewer 决定都齐全，才把对应 gate 从“软件实现”提升为“研究 gate 通过”。

## 参与者实际怎么操作

开始时画面显示两步：先选一条路线和一个实验条件，再点击 **开始 4 段实验 / START 4-BLOCK JOURNEY**。这会固定本次 session 的四条件和四路线顺序；飞行过程中不需要不停点击。

1. **阶段识别。** 只有系统播放阶段提示音且 `identification_window_open` 出现时，右下角才会弹出乘客答题卡。听提示音后，在 **8 秒**内选择其所表示的下一阶段：CRUISE、DESCENT 或 LANDING。提交有效答案会写出 `identification_response`；没有答题卡时按钮被禁用，因此不会再把普通点击伪装成有效数据。
2. **视觉副任务。** 画面右侧出现橙色菱形时，立刻点击 `TARGET`；出现蓝色方块时不点击。每次刺激都有唯一 ID，代码记录 hit、miss、false alarm、correct rejection 与有效视觉反应时。
3. **路线和视点。** 4 条路线均有爬升、巡航、S 弯、下降与着陆准备。客舱根节点随路径平滑偏航，因此乘客视角会在转弯时转向；转弯段同时触发相对客舱左/右侧的 `spatial_turn_cue`，记录方向、转角、clip 与 `spatial_blend=1`。
4. **完成后。** 飞行完成弹出 6 题 1–7 分短评。前三个 block 完成评分后，中央提示卡会显示下一条件和路线；点击 **开始下一段 / START NEXT BLOCK** 后继续，不会回到难以理解的空闲画面。第四段评分完成后显示谢幕与感谢，并提供 **复制数据路径 / COPY DATA PATH**。
5. **暂停与界面。** `F1` 或 `HIDE UI` 可隐藏操作面板，`F1` 或 `SHOW UI` 可恢复。不要在识别卡出现之前预先点击答案；这样的点击被明确记录为 `input_rejected`，不计入有效反应。

## 结束后到哪里找数据

1. 在最终感谢页点击 **复制数据路径 / COPY DATA PATH**，再粘贴到当前操作系统的文件管理器地址栏即可直接打开本次 `event_log.jsonl`；Windows 使用文件资源管理器，macOS 使用 Finder 的“前往文件夹”。
2. Windows 示例根目录为：`C:/Users/Trusogen/AppData/LocalLow/DefaultCompany/My project/AudioV0/`；实际根目录由 `Application.persistentDataPath/AudioV0/` 决定。技术测试会自动写入 `technical_demo/session_<timestamp>_<uuid>/`，每个 session 都有 `event_log.jsonl` 和 `session_manifest.json`，因此不会覆盖上一位参与者的数据。
3. 原始 `event_log.jsonl` 是唯一审计源。每一行是一条 JSON 事件；使用脚本派生 `transitions.csv`、`visual_task.csv` 与 `block_ratings.csv`，不要在原始文件中手工改数据。
4. 本 demo 的分析最小单元是一个完成的 block：路线事件、三次 notification/transition、视觉副任务事件、6 次评分和 `experiment_block_complete`。整个四段 session 结束后还会有一条 `experiment_sequence_complete`。
5. 最终感谢页的 **打开数据文件夹 / OPEN DATA FOLDER** 会打开根目录；**删除技术测试数据 / CLEAR TEST DATA** 只删除 `technical_demo` 和旧版顶层 `session_*` 技术日志。它会拒绝删除仍在写入的 session，且不会触及未来的 `pilot` 或 `participant_run` 分类。

## 四个实验条件的意思

| ID | 通知设计 | 客舱背景 |
|---|---|---|
| `NS_Q` | 非语音提示（non-speech） | 安静（quiet） |
| `NS_N` | 非语音提示（non-speech） | 模拟客舱噪声（noise） |
| `S_Q` | 语音提示（speech） | 安静（quiet） |
| `S_N` | 语音提示（speech） | 模拟客舱噪声（noise） |

当前项目把这四个条件作为技术预览路径进行锁定和记录，但正式 speech/non-speech 刺激、SPL/SNR 校准和耳机回录尚未冻结，不能把 demo 的条件差异报告成论文主结果。

## 当前项目的诚实结论

Unity 中已经体现 G01–G06 的主要事件记录骨架，并能验证桌面 technical demo 的路线、声音调度、识别窗口和视觉任务。G07 只有条件锁定和技术资源预检；G08–G12 仍需要正式刺激、设备、分配、四条件干跑和人工 pilot 证据，不能仅靠 Unity Console 的 0 errors 判定通过。
