# 城市航线、客舱 UI、运动与数据链路 — 2026-09-21

## 当前版本与范围

- 权威工程：`/Users/trusoegn/GitHub/Audio-VR/Audio-VR`；Unity `6000.6.0f1`；主场景 `Assets/AudioV0.unity`。
- 接续 `lightweight_aerial_city_environment_20260921.md`。本轮改变路线、运动、参与者 UI、数据记录和导出；保留用户之前的工作树，没有提交或回退。
- Candidate A 仍为 `technical_candidate`，`formalStudyMode=false`。本轮没有更新论文正文、Overleaf、样本量、量表或招募状态。

## 论文依据与迁移边界

已阅读用户指定 Kim & Ji 的 *Designing for Trust: How Human-Machine Interface Can Shape the Future of Urban Air Mobility*，DOI `10.1080/10447318.2024.2313289`，并查看 PDF 第 7 页 Figures 2–3 的实际渲染。

- PDF 第 6 页 §3.3–3.5：中央客舱视频、四种 movement/hazard 信息组合、分阶段升降、约 290 s／7.8 km 路线。
- PDF 第 7 页：跨城区弯曲航线、出发 D／避障 E／到达 A，以及前风挡视觉信息示例。
- 本轮参考中央屏幕位置、弧线与分阶段起降；未引入该研究的危险事件、视野航迹带或 hazard HMI，避免改变本项目音频条件。
- 原论文刻意不安排视频答题，不能把它写成当前事件检测任务的直接有效性依据。本文时长、速度和高度未直接复制论文参数。

## 已完成

1. **航线**：四条镜像／宽度变体的城市弧线，265 点，横向约 280–297 m，180 m 巡航；取消 ±72 m 限制。高度与前进轴单调 Hermite 插值，横向 Catmull-Rom；巡航标签在实际爬升结束后触发。
2. **运动**：新增 `FlightMotionProfile`，首尾速度为零；18 m/s 巡航上限、3 m/s 爬升、2 m/s 下降、0.8 m/s² 切向加速度、1.2 m/s² 曲率限速参数。通知提前量与路线共享解析时间表。移除 hero motion 对客舱朝向的第二次覆盖。
3. **净空**：864 栋合并渲染的城市建筑补充保守碰撞盒，纳入 24 m 净空检查；本轮四路线 `valid=True`、`repairs=0`，未通过时阻止开始。
4. **UI**：横向客舱信息屏、真实计划路线静态图、独立事件卡与 RESPOND；更大的文字、取消无功能 tabs、修复追踪光标误显示。参与者视图隐藏阶段答案、实时高度与进度；无累计分数。
5. **音频隔离**：已配置的候选试听禁止额外阶段、转弯和 UI 装饰音；视觉响应本身无 click/error/reward 音，空窗误报可记录。
6. **日志**：修复首条 `task_version` 与跨段 `block_position`；记录呈现版本、运动样本及每段帧时间摘要。速度使用解析值，避免 float 坐标差分在高帧率下产生伪尖峰。
7. **导出**：现有 `tools/export_audio_v0_events.py` 新增视觉刺激配对表、运动表、按 session 筛选；修复 notification CSV 两类时钟混用、软件提前量来源和播放观察判定。已有原始日志可重新导出，不需改写。

这是受约束、可复现的运动学模拟，不是已验证的 eVTOL 气动／推力模型；尚未验证 jerk 上限、HMD 舒适度或设备实时性能。

## 实际验证

### 正常速度单段

原始日志：

`/Users/trusoegn/Library/Application Support/DefaultCompany/My project/AudioV0/technical_demo/session_20260920_164803_670_2705473bc6f649929d0763099e6c03ba/event_log.jsonl`

- 第一段 NS_Q 完整起飞、巡航、下降、落地，路线约 219.367 s，T01/T02/T03 各一次，无应用 error；之后进入第二段再由开发者停止。
- 三次软件提前量为 3.996、3.996、3.993 s。它们是软件时钟证据，不是声学 onset。
- 含显式 synthetic 输入与一次暂停／恢复，不是参与者结果。
- 该轮之后又修正了 `block_position` 和速度遥测的数值噪声；正常速度单段并非所有最终代码的完整再测。
- 帧摘要最大帧约 337.7 ms，不能根据很低的平均 Editor Update 时间宣称稳定高帧率；目标设备性能 gate 保留。

### 最终四条件加速集成

原始日志：

`/Users/trusoegn/Library/Application Support/DefaultCompany/My project/AudioV0/technical_demo/session_20260920_165842_042_c400b26b314c46309c2e07e7b773f03d/event_log.jsonl`

- 开始后显式切到 `Time.timeScale=4`，因此日志有 1 和 4 两个值；验证结束恢复为 1。任务与音频部分使用实时钟，加速测试仅证明流程闭合，不能验证正常速度任务负荷、lead 或可听性。
- 四条件顺序 NS_Q → NS_N → S_N → S_Q，各一次；`block_position=1,2,3,4`。
- 5,735 条事件；12 次通知／阶段／识别响应，4 次 route complete，24 条探索性评分，1 次 sequence complete，1 次 session end。
- 49 个视觉 onset 与 offset 全部配对；包括合成 hit、false alarm、miss。4,303 条运动采样；解析速度最大爬升 3.0 m/s、下降 2.0 m/s。
- 连续 event sequence、monotonic clock、四段 T01/T02/T03 顺序与结束记录均通过；应用 error 为 0，无装饰转弯声事件。
- S 条件加载 `T01/T02/T03_ElevenLabs_Broadcast_v3`，NS 条件仍为原 procedural v1；没有偷偷切换或冻结声音候选。
- 已实际运行 exporter：12 条通知、49 条视觉事件、4,303 条运动样本；通知和视觉表无质量 flag，validation errors 为空，明确 `qa_only`。

### 回归与呈现

- 新运动测试 2/2 通过；最终全量回归 job `954c1f19`：920 项中 918 passed、2 skipped、0 failed，136 s 完成，结果保存于 `temp/ui_flight_20260921/final_editmode_results.json`。
- Python 导出测试 5/5 通过，覆盖跨区块同名刺激、缺失 offset、空窗误报、运动字段与通知时钟／播放观察来源。
- 实际 Game View 截图检查了地图渲染、文字布局、起飞／巡航／下降。截图实尺寸以 PNG 为准，UnitySkills 接口的 1920×1080 返回值不代表实际文件尺寸。
- 通过公开控制器入口执行自动输入；原生 UI 自动化后续出现窗口绑定错误，未将其写成鼠标点击或 HMD 控制器验证。
- 收尾处于非 Play Mode；最终运行没有应用 error。已有场景 YAML 和旧研究文档的空白格式问题保留，未改写不属于本轮的修改。

## 文件与继续入口

- 工程证据：`/Users/trusoegn/GitHub/Audio-VR/Audio-VR/temp/ui_flight_20260921/`。
- 核验：`log_audit.json`；导出：`export/validation_report.json`、`export/visual_events.csv`、`export/notification_events.csv`、`export/flight_motion_samples.csv`。
- 截图：工程 `Assets/Screenshots/urban_arc_ui_final_20260921.png`、`urban_arc_final_desktop_20260921.png`；证据目录 `accelerated_cruise.png`、`accelerated_descent.png` 明确属于加速运行。
- 开发验证工具：`Assets/Editor/AudioV0VerificationProbe.cs`；菜单 `Tools/Audio V0/Verify Normal Speed Sequence` / `Verify Accelerated Sequence`，只允许 technical_demo。所有自动输入标记 `synthetic_verification_*`。
- 操作：打开主场景并 Play；F1 为研究者面板，正常使用不用启动验证菜单。当前源默认正常速度。

## 下一 gate

1. 最终版本正常速度四段复核，包括真正鼠标／目标 HMD 控制器输入、视觉卡可读性、阶段识别与副任务冲突。
2. 目标 HMD 的帧时间、掉帧和舒适度；当前 XR Canvas 仍 head-locked，不能声称已完成客舱固定屏幕验证。
3. 解决天空过亮、远景衔接和合并城市性能；不根据 Editor 平均帧时间宣称 Quest 性能。
4. 任务选择、声音候选筛选、设备声学校准及 feasibility pilot；路线正常速度约 219–221 s，四段约 14.7 min，仅 flight exposure，完整 session 还需培训／休息／问卷。
5. 所有 motion、route、task 和 calibration 参数继续 provisional；正式采集前核对 exporter 的 run record／freeze 要求。
