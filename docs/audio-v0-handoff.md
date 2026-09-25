# Audio V0 / UAM Passenger Lab Handoff

更新时间：2026-09-25

## 2026-09-25 Quest 转向提示听感调整

- 用户试听 2.25 s 版本后仍觉尖锐，希望间隔 3 s、单声更长并使用柔和和弦。现已生成新的 mono 44.1 kHz PCM16 `gentle_chord_turn_v1.wav`：F3、A3、C4 缓慢叠入，2.10 s，0.36 s 渐起、0.78 s 渐隐，不再高频上滑；3.00 s 起点间隔，约 0.90 s 空档。生成脚本为 `sound_design/generate_gentle_chord_turn.py`，资源 SHA-256 为 `7b5748cb2b975834062130d76c0fe0014ab734bfb9e962760f2a3ab9797215cb`。试听片段位于 `temp/turn-cue-preview/gentle_chord_v1_3s_preview.wav`。
- Unity 现加载新和弦，专用声源的音量 0.55、单次播放系数 0.50、音高 1.00，左右移动周期随间隔改为 3.00 s。场景和 Profile 的版本标记同步为 `gentle_f_major_chord_v1_2100ms_interval_3s_gain055x050_candidate`。数字峰值为 0.44、RMS 为 0.1478；这些只证明文件特性，不代表 Quest 3 实际响度或真人舒适度，仍待戴机试听。
- 前一版 `airy_breath_soft_turn.wav`（1.30 s）曾从 1.00 s 重叠播放改为 2.25 s 间隔、0.90 音高和较低响度；用户试听后仍觉尖锐，因此此参数组合只保留为历史试案，不是当前 Unity 映射。当前和弦仍属 technical candidate，正式实验声音未冻结。

## 2026-09-25 Windows Quest Link 性能排查

- 头显运行时 GPU 曾达 98%、89°C；场景快照为约 150–340 万 triangles、738–1587 draw calls、191–400 shadow casters，飞行采样还以约 5 Hz 写入带堆栈的 Editor Console。这些是不同视角和时刻的瞬时值，不是稳定帧率测量。
- `EventLogger` 继续逐条写入 `event_log.jsonl`，仅停止把 `flight_motion_sample` 重复打印到 Console；PC Renderer 关闭全屏 SSAO，PC URP 主光阴影改为 1024、2 cascades、30 m，关闭额外灯光阴影。修改前渲染资产备份位于 `Temp/codex-recovery/`。
- 重新进入 Play Mode 后脚本未在编译；Quest Link 后续达到 focused，右手柄点击进入正式飞行。新会话中 223 条飞行采样已写入 JSONL，Console 未再输出该事件；八次瞬时帧时间多数约 13–15 ms，另有一次 92 ms 停顿。用户对蓝色区域与交互流畅度的复查仍待反馈，不能把这组快照写成问题已解决或稳定 HMD 帧率。
- 用户进一步确认建筑密集航段仍卡。城市网格保留建筑体块、碰撞与近航线两列窗格，其他有窗列每隔一层和一列生成窗格；`AerialCityChunk_11` 从 31,058 降至 17,894 triangles，运行日志的城市场景样式更新为 `street_readability_batched_v4_vr_windows_candidate`。Quest 实际舒适度复查正在进行，静态面数下降不等于帧率达标。
- 用户在 Quest 再测后表示有所改善但偶有卡顿/闪蓝；同轮抓到 28 和 45 ms 瞬时帧。PC URP 渲染比例从 1.0 降至 0.85 后已重新进入 Play Mode，需再次在头显确认座舱文字可读性与密集航段表现，不能从渲染比例推断症状已消失。
- 0.85 再测：用户确认文字清楚，但建筑密集段仍偶发卡顿或闪蓝。实时快照中的一段约 13–15 ms，并未覆盖每次偶发异常；问题仍未关闭。
- 该轮原始 JSONL 在 03:07:03–03:07:41 记录 7 个大于 50 ms 的采样帧，其中 03:07:30 为 0.653 s；03:14:29 的 372.689 s 为后续编辑器长时间停顿，不能归因于当时头显体验。原始记录含 143 个 `route_segment_enter`，它们与高频轨迹一样曾同步镜像到 Editor Console 并采集堆栈。现让航段事件仅写 JSONL，保留正式数据且减少编辑器输出负担；是否改善 Quest 体验仍需复测。

## 2026-09-25 Windows 数据目录

用户指定后续 Windows 实验记录写入 `E:\Audio-paper\实验数据\`，分析工具放在并列的 `E:\Audio-paper\分析工具\`。`EventLogger.GetDataRootPath()` 是日志与 Unity 内“打开数据文件夹”按钮的共同入口；macOS 保留 `~/AudioV0_Data/`，Android 等平台保留各自 `Application.persistentDataPath/AudioV0/`。此前 Windows 会话仍留在 `AppData/LocalLow`，不迁移、不混入新目录。

强制重新编译后实际 Play Mode 的 `DataRootPath` 为 `E:\Audio-paper\实验数据`、`IsReady=true`、`HasFatalError=false`；一场新的 `technical_demo` 写出日志和会话清单，停止后只读概览读到 17 条事件、0 条格式错误及 `session_end`。`分析工具/查看会话.ps1` 是基础结构检查，不能替代 macOS 未拷贝的数据中心或正式研究导出器。

## 2026-09-24 当前软件交付

操作入口是 [使用手册](audio-v0-user-manual.md)，精确修复与逐轮验证记录归档在 `/Users/trusoegn/论文2/handoff/unity_software_delivery_20260924.md`；该记录覆盖下方9.23的当前参数描述。

- v5实际场景时长为201.849673 / 206.133982 / 192.260056 / 192.174623 s；18 m/s上限、5 m/s爬升、4 m/s下降、1 m/s²切向加速度、1.2 m/s²曲率限速。
- 第二条160 m，其余130 m；巡航平台在T01前达到。3962次24 m净空采样为0碰撞、0自动修补；路线不是严格镜像或等时，counterbalance仍待研究冻结。
- 终场截图在logger关闭后不再写事件；旧片段QA退出Play时重置，并拒绝操控当前完整航程。当前完整航程日志正确记录live_route。
- 当前测试、构建、应用画面及其限制以当日交接最后的交付验收为准；本轮证据在 `temp/20260924-final-delivery/`，旧四段基线和中断复验分开保留。
- 独立应用路径 `Builds/AudioV0-macOS/AudioV0.app`；默认窗口模式，启动器与手册位于同目录。首次独立构建发现天空/透明玻璃变体被裁剪，修复与复验必须以Player实际画面为准。
- 以下旧证据仍保留为历史，不当作新版重复运行或Quest验证。

## 2026-09-23 历史：快速教程与四段完整城市飞行

本节替代本日早轮默认独立片段和长教程；保留旧实现、资源与证据，不回退dirty tree。权威场景 `Assets/AudioV0.unity`，Unity `6000.6.0f1`；研究结论与正式参数仍由论文2的协议记录维护。

### 当前行为

- `participant_tutorial_quick_v2_candidate`：六条带标签声音自动示范、GO与WAIT各一次、一次识别/信心、一题说明检查；错误给出解释后继续，自选重播。Ready后必须明确点击 `START FOUR FLIGHTS`，不自动开实验，也不强制重复到全对。
- 默认 `useIndependentExcerptsCandidate=false`，Session不挂入excerpt控制器。`FullFlightRouteGeometry` 的 `full_city_journeys_v5_210s_candidate` 提供四条地面到地面的宽S形路线，每条包含垂直起飞、爬升、左右转、巡航、下降和垂直落垫；工程目标为每段不超过 210 s（3:30），仍需当前代码的正常时钟重跑和 feasibility pilot 确认。旧 `full_city_journeys_v4_candidate`、`independent_excerpts_v1` / `excerpt_curved_climb_v2_candidate` 保留为历史候选。
- `RoutePlayer` 使用 `bounded_full_flight_v3_candidate`；路径用保形三次插值，机体yaw随路径且无pitch/roll。继续复用18 m/s巡航上限、3 m/s爬升、2 m/s下降、0.8 m/s²切向加速度及1.2 m/s²曲率限速；首尾速度零，起止根节点y=1.92 m，对齐地面起降垫和机体landing rail。
- 四条实际场景路线仍需以当前 `AudioV0FullFlightVerification` 导出的 geometry audit 为准；旧版 282/272 s 计时不再作为 v5_210s candidate 的当前证据。高度平台是世界 y=180 m（相对起飞根节点约178 m）。这些是技术候选，不是 pilot 冻结的时长或有效气动力学模型。
- 每段按真实阶段依次通知T01巡航、T02下降、T03着陆准备；三次通知均提前18 s，对应8 s识别、8 s信心、2 s余量。T03跟随实际landing_preparation边界，取消任意0.85距离阈值。响应速度不改变运动时间表。
- `context_supported_full_flight_v1_candidate`明确记录有上下文的理解任务。阶段顺序、外景和当前路线地图可辅助回答，不能宣称sound-only识别效度或visual-only gate已通过；旧独立片段预回答不泄露证据只适用于旧候选。
- 四条件仍为NS_Q/NS_N/S_Q/S_N，各出现一次；条件使用四个候选序列，technical模式优先操作者选择的首条件，路线从所选index循环。正式分配/随机化仍须冻结。当前QA为conditions=NS_Q,NS_N,S_N,S_Q，routes=0,1,2,3。
- Pause冻结路线与视觉任务时钟，停止通知、将开放识别/信心标interrupted；Resume继续原位置和route time，已播放通知去重。恒定背景masker在当前实现中仍继续；段末才停止。当前GUI显示实际航迹/当前位置/终点、GO/WAIT与响应区，不显示条件代码或累计成绩。
- 声音保留用户选定M1：三条1.80 s MATLAB cue，`matlab_m1_two_pulse_v1_candidate`；语音仍为V4，`elevenlabs_broadcast_v4_candidate_speed090`。资源/数字审计不能证明主观响度匹配、真人可懂度或Quest声学校准。
- 每条路线在实现的左、右转段播放方向匹配 `spatial_turn_cue`；左转从左侧、右转从右侧。该 cue 在四个 speech/non-speech × quiet/noise cell 中共享，不构成第三因素，也不计入 T01--T03 识别分母；source position、spatial blend、HRTF/SDK 和 roll-off 仍需校准与 pilot 检查。

### 证据与复现

- 强制import后定向 `AudioV0.Tests` 92/92通过，包括5个新增完整route几何/运动测试和快速教程错误继续回归。`temp/20260923-quick-tutorial/tests.json`。早轮全量974项（972通过/2跳过）只作为历史基线，不宣称本轮又跑了全量。
- 快速教程实际正常时钟32.100 s，故意GO漏答、WAIT误按、错误识别及错误理解题后仍Ready；18个教程事件、1次识别+1次信心、0教程期正式识别。`temp/20260923-quick-tutorial/runtime-full-flight-session/meeting_verification.json`与七张实际截图。32.100 s是此synthetic操作路径的实测，不承诺真人固定时长。
- 完整route证据目录 `temp/20260923-full-route-revision/`；`runtime_status.json`、`runtime_audit.json`、原始日志path与SHA-256快照共同核对四段结束、通知顺序、响应关联、起止高度、速度、heading与normal clock。完整QA正常editor耗时1112.533 s，7634 events、5485 motion、12通知+12转场、11识别+11信心、1错误、1timeout、0error；软件lead为17.995–18.001 s。全部synthetic/qa_only，不是参与者结果。
- `geometry_audit.json`：同次真实场景Physics，4018个采样、24 m半径、间隔≤4 m，0建筑碰撞、0几何自动修补。初次JSON末字段格式错误原件保留为`.raw.json`，完整run修正件为`geometry_audit.full_run.json`；修复QA序列化后重新创建真实场景并审计，当前`geometry_audit.json`可直接解析且仍为4018/0/0。不是连续碰撞或实际航空安全证明。
- `pause_probe.json`：block02 route time205.178 s，实际参与者PAUSE按钮暂停2.149 s后恢复；机体位置与route clock不变，恢复后继续。该次发生在响应窗之外，未代替响应中断分支测试。
- `actual_route_geometry.csv`由场景真实waypoints与其运动时间表导出；`plot_routes.py`生成论文候选PDF/SVG/PNG。`route_02_ground/climb/left_turn/right_turn/descent/landed.png`为同一路线实际1920×1080相机呈现，已逐张检查。`figure_provenance.md`说明数据、caption与边界。
- 第一段前半截图残留教程副标题，已通过runtime Text属性修正并回读；`ui_copy_fix.json`保留此次纯文案修复。论文优先使用第二段截图；该文案已并入源码，最终UI真实开始检查保存在`post_sync/`。最后11:18:05 UTC Editor增量编译为0 errors/8条既有warnings，见`post_sync_capture_compile.json`；这是该次编译快照，不将早轮0/12说成0 warning。
- 最终源码检查：保留原quick/fullflight同session证据至`runtime-full-flight-session/`，重新quick QA用32.064 s到Ready，再真实点击开始并回读/渲染副标题，与源码“Tap gold GO · Wait on grey · Respond to the notice”完全相同，未用runtime注入文本。`post_sync/source_sync_audit.json`与`participant_source_start.png`只证明这次新教程/启动呈现，不是第二次四route完成；原始首次断言误把中点当圆点，已按源码核对，不是UI内容错误。
- 最后恢复状态：`post_sync/restored_welcome_state.json`实际回读为Play、Tutorial Welcome、Session Ready、sequence未启动、RoutePlayer不播放、route time=0、位置(0,1.92,0)。用户可从快速教程开始；无人自动运行QA。手动截图入口两次调用均实际重写当前图片（`repeated_capture_verification.json`），不会因自动QA去重返回旧图；`participant_current.png`为最终Welcome实际画面。
- QA结束截图首版恰在UI刷新前拍摄，`four_full_flights_complete.png`只显示最终落地；完成状态由日志确认。QA已加0.25 s呈现等待，此工具更改未重跑四route。
- QA入口：完成快速教程至Ready后执行 `Tools/Audio V0/Verify Four Full Flights`，它点击真实开始按钮；运行期间不要改Assets触发reload。正常四路线约18.5 min加暂停/区块切换。只允许technical_demo、formalStudyMode=false、timeScale=1。

旧教程/独立片段的完整审计在 `temp/20260923-route-sound-tutorial/`，其规范归档为 `/Users/trusoegn/论文2/handoff/unity_tutorial_motion_and_sound_candidates_20260923.md`。旧的50教程事件/9+9练习、12片段/1725motion及pause-invalid只适用于该旧会话。Quest3输入/可读性/舒适度/帧时间、耳侧声学校准、真人筛选、约两人feasibility pilot与正式冻结仍未完成。

## 2026-09-21 历史：宽松看球、并行答题与数据修复

当前 `passenger_hud_attention_v8_candidate` 替代下文窄窗节奏/图标交互：整块游戏舞台可点，金球 3.5 s 全曝光接受、灰球等待、间隔 1.5 s，GO 概率暂留 0.25；没有通关、淘汰和累计成绩。参数均待形成性体验与 pilot。游戏与答题区域独立，v8 实际日志确认识别 pending 时舞台点击成功。信心 pending 的游戏点击尚未单独运行验证。

任务 v2 契约保留每机会唯一结局、逐次尝试和参数快照；导出器独立复核时间、配对和分母，旧版本不混算。`block_started` 移到成功 StartRoute 之后，修复 attempt 偏移。项目默认 timeScale 恢复 1；街道按材质/区域批处理，18 栋近景楼加入净空审计。数据中心已增加信心筛选、GO/WAIT分母、中断与质量提示。

v7 窄窗正常速度四段 846.208 s 完整结束，12 通知/识别/信心、170 刺激和 70 合成点击配对通过；该证据不证明后续 v8 难度或人体可用性。v8 全量 943 项初次 939 passed/2 failed/2 skipped，失败为新测试夹具缺航点；修正夹具后相关类18/18通过。Python14/14、查看器JS8/8；浏览器和Unity实际渲染已检查。最终已退出Play并恢复QA窗口。

完整证据、历史版本和未完成 gate：`/Users/trusoegn/论文2/handoff/game_interaction_data_validation_20260921.md`。Quest 3、校准、视觉占用/趣味性和正式参数冻结仍未完成。本节覆盖下文同日历史状态，保留原记录供追溯。

## 2026-09-21 后续 HUD 版本

`passenger_hud_v5_candidate` 替代下文实体大屏：客舱固定透明组合玻璃、分区投影、中央透景、文字独立深色衬底；移除显示器外壳/支座，靠背添加闭合厚度，增加侧台收纳盖。Supernal 官方概念座舱与 Collins 官方 HUD 照片已查看；仅参考设计，不代表量产机型或光学 HUD 验证。最新交接为 `/Users/trusoegn/论文2/handoff/passenger_hud_reference_and_calibration_20260921.md`。Quest 耳侧声学校准仍缺实际链路与测量硬件；不得将 7 个 clip 数字审计标为实测。

## 用户否决旧座舱后的曲面内饰修订

下方 v3 细杆框架视觉未获用户认可，已被 `PassengerCabinRefit` 的连续拱顶、厚圆角窗套与密封边、收腰座椅和肩带替代。参考 Archer 提交的 Midnight 内饰图（A' Design Award，ID=154977，N=4），不声称机型精确复刻。屏幕组件缩放 0.76 后约 1.254 × 0.777 m，z=1.40 m；测量流程保持不变。最终证据及待验证项以论文 handoff `cabin_display_measurement_quest3_alignment_20260921.md` 的后续修订为准。新增结束日志守卫回归与信心测试共 5/5 通过；前一轮 924 项全量结果早于本次几何修订。

## 2026-09-21 座舱显示、实时地图与逐次信心

本节覆盖下方同日早轮的静态地图、head-locked 参与者屏和旧段末评分；历史数字保留。

- 参与者表面固定在实体仪表台，分为地图/视觉事件区与底部识别/信心区；屏幕约 1.65 × 1.023 m，字体现打包 Noto Sans CJK。修复实际渲染中仪表台遮挡底部按钮的问题。研究者 F1 面板独立保留。
- 实时地图约 20 Hz 更新真实位置、朝向与已飞路径；不显示阶段名称或精确倒计时。四路线左右偏置终点 ±420 m，Z=1270 m；对应起降盘和进近净空同步调整，实际净空检查 valid=true、repairs=0。
- 圆角屏幕外壳、座椅/头枕/扶手、门饰/把手、通风口、窗框和灯带已生成；内饰合并为 9 个启用的 mesh renderers。城市冷暖窗光使用自发光材质，无每窗动态灯源；这不是 Quest 帧率验证。
- 核对 9.17 08:53 会议原话及线上 Overleaf §2.4/2.6：每次识别后一道 1–7 信心题。默认禁用旧六项段末自编评分，日志改为逐事件 `confidence_window_open/response/timeout/interrupted`，父通知关联、评分耗时与识别 RT 分开；段末仍保留休息，终场进入访谈。
- 导出器新增信心字段及质量标记；识别表增加 block 维度避免跨段同名 transition 合并。7 项 Python 测试通过；4 项新信心边界测试通过。全量回归 924 项，922 passed、2 skipped、0 failed；后续仅呈现与几何改动经编译和实际渲染复核。
- 真实运行时数字音频审计覆盖 7 个实际 clip：软件输出 48 kHz stereo，通知 spatialBlend=0、volume=0.55。保留原 non-speech procedural v1；它与离线 contour-soft WAV 并非同一波形。噪声仍为 demo bed，不是实测 eVTOL 噪声。
- 目标设备用户已指定 Quest 3。声学实测、实际输出链、控制器使用、帧时间/舒适度和 pilot 未完成；本轮没有修改或声称同步线上 Overleaf。
- 正式交接及最新证据入口：`/Users/trusoegn/论文2/handoff/cabin_display_measurement_quest3_alignment_20260921.md`；工程 `temp/cabin_measurement_20260921/`；测量对照 `/Users/trusoegn/论文2/docs/measurement_implementation_alignment.md`。

## 2026-09-21 航线、信息屏与记录链路

本节替代下文有关恒速短航线、默认副任务关闭、参与者阶段 HUD 与候选试听装饰音的旧状态；历史验证保留供追溯。

- 当前场景启用 Candidate A 技术候选，`formalStudyMode=false`。横向中央信息屏使用真实静态路线图、Material Design 图标和独立 RESPOND；追踪光标只在 tracking 模式显示。参与者界面不提供阶段名称、实时高度／进度等辅助答案；F1 研究者面板保留。
- 论文核验：Kim & Ji，DOI `10.1080/10447318.2024.2313289`，PDF 第 6 页 §3.3–3.5、第 7 页 Figures 2–3。迁移中央客舱屏幕、跨城区弯曲航线和分阶段起降的设计思路；未迁移其 movement/hazard HMI 实验操纵、避障危险事件、视频任务或 7.8 km/290 s 参数。该研究没有要求视频答题，不能作为本项目事件检测任务有效性的直接证据。
- 四条左右镜像／宽度变体的跨城区航线使用 265 个采样点，横向跨度约 280–297 m，巡航 180 m。高度和前进轴使用单调 Hermite 插值，横向使用 Catmull-Rom；巡航／下降语义边界与实际高度平台一致。
- `FlightMotionProfile` 为每条路线生成距离、速度和时间表：18 m/s 巡航上限、3 m/s 爬升、2 m/s 下降、0.8 m/s² 切向加速度、1.2 m/s² 曲率限速参数，首尾速度为零。通知提前量查同一时间表。参数为 provisional，属于运动学模拟，尚无气动／推力模型或 jerk 上限验证。
- 新城市每栋楼都有保守 BoxCollider，24 m 净空审计覆盖合并渲染网格；净空未通过时禁止开始区块。本轮路线没有自动抬升修补。
- 修复 `session_start.task_version` 初始化顺序及跨段 `block_position`。增加 `participant_presentation_ready`、约 5 Hz `flight_motion_sample` 和 `flight_frame_summary`；遥测速度由解析时间表计算，避免高帧率下坐标 float 差分噪声。
- 已配置的技术音频试听与 formal 模式均禁止装饰阶段、转弯和 UI 音效；视觉任务空窗点击可记录 false alarm，无奖励音。
- 导出器在 `/Users/trusoegn/论文2/tools/export_audio_v0_events.py`；新增 `visual_events.csv`、`flight_motion_samples.csv`、`--session-id`。通知表的两个 onset 时间改为同一 monotonic 时钟，另保留 `transition_route_time_ms`，修复软件 lead 和播放观察来源。
- 本轮验证入口与证据：工程 `temp/ui_flight_20260921/`；正常速度单段与加速四条件检查分开标记。精确最终统计以该目录 `log_audit.json`、导出 validation report 及 `/Users/trusoegn/论文2/handoff/` 最新归档为准。
- 最终回归：920 tests，918 passed、2 skipped、0 failed；Python 导出 5/5。正常速度一段约 219 s 完成；加速四条件 12 次通知、49 个视觉刺激配对、4,303 条运动采样及最终 session_end 全部通过。规范交接：`/Users/trusoegn/论文2/handoff/urban_route_ui_motion_and_data_20260921.md`。
- 未关闭：HMD 字体／输入／舒适度、实际设备帧时间、声学 onset/SPL/SNR、任务选定与 pilot。当前头显 Canvas 仍为 head-locked，尚未验证真正客舱固定屏幕的可用性；本轮未更新论文 Methods 或 Overleaf。

## 2026-09-18 通知声缺失：增量修复

- 用户指定的完整任务交接仍为 `/Users/trusoegn/论文2/handoff/audio_vr_audio_methods_and_visual_next_steps_20260918.md`；本节补充其音频 P0 的最新工程证据，其他 gate 未推进。
- 真实桌面运行在 NS_Q 下调度 T01/T02/T03 后分别记录 `AUDIO_MISSING_ASSET_OR_SOURCE`，`playback_observed` 和 `notification_onset` 均为 0。
- `AudioV0RuntimeCueGenerator.stateCues` 仅检查静态数组是否存在，未检查其中 Unity `AudioClip` 是否已经销毁；关闭 Domain Reload 的 Play Mode 可复用失效元素。UI cue 的 `??=` 同样不处理 Unity 的 destroyed-object null 语义。
- 现已逐元素检查三个 state clips，并将三个 UI cue 改为 Unity `== null` 检查；未改波形、音量或研究参数。
- 回归先在未修复代码上产生 4 个失败，修复后定向用例 6/6 passed。正常 18 m/s 路线运行得到三次通知的 scheduled/requested/playback_observed/onset/estimated-offset，`is_playing=True`，缺失音频错误为 0；退出后再次进入也观察到 T01 播放。
- 证据目录：`temp/audio-v0/20260918-notification-audio/`。原始 JSONL 保持不改写，副本分别标记 before/full-route/reentry。
- 修复后全量 EditMode：877 tests、875 passed、2 skipped、0 failed；最后脚本编译为 0 errors、0 warnings。
- **音频 gate 未关闭**：当前 `speechClips=null`，technical 模式 S_Q/S_N 仍复用 non-speech placeholder；真实 speech/non-speech 四条件试听、输出设备、声学 onset、SPL/SNR、Quest/HMD 与 pilot 均未验证。不能把 software onset 或 `isPlaying` 写成实际可听性证明。
- 运行时通用反射读取 AudioSource 已废弃的 minVolume/maxVolume/rolloffFactor 属性会让 UnitySkills 自己产生 Console errors；本轮已停止使用该查询，这些错误不作为通知声根因。

当前场景：`Assets/AudioV0.unity`

当前定位：桌面 technical demo，已具备可复制实验模板、基础 VR/XRI 配置与结构化日志；不是已完成的正式被试研究。

## 目标与当前体验

项目模拟 UAM 乘客在起飞、巡航、转弯、下降和着陆过程中的音频通知与阶段识别。secondary activity 尚未选择；项目内的视觉事件和 continuous tracking 只保留为默认关闭的技术候选。一次参与者运行由四个连续 block 构成：

1. 选择路线和初始条件，点击“开始 4 段实验”。
2. 每个 block 按路线飞行，包含音频阶段通知和空间转弯声效；secondary activity 默认为关闭。
3. 当前 UI 保留探索性评分流程用于技术演练，但正式 construct、量表、题项和时点仍待 Tram 复核与 pilot，不作为已冻结测量。
4. 前三个 block 通过“开始下一段”继续；第四段结束显示感谢页、日志路径、数据目录和技术数据清理入口。

四个条件为 `NS_Q`、`NS_N`、`S_Q`、`S_N`。初始选择会成为第一段，之后的条件和路线以无重复顺序完成余下三段。

## 当前重要实现

| 能力 | 实现位置 | 当前行为 |
|---|---|---|
| 四段连续流程 | `Assets/Scripts/AudioV0ExperimentSequenceController.cs` | block 评分结束后显式提示下一段；最终显示谢幕和复制路径动作。 |
| Secondary-task 候选 | `Assets/Scripts/VisualTaskController.cs`、`ContinuousTrackingTaskController.cs` | Profile 默认为 `None`；两个候选都不进入正式 outcome，只有显式选择候选模式才运行。 |
| 阶段识别 | `Assets/Scripts/IdentificationController.cs` | 仅在音频通知后显示答题卡，默认答题窗口 8 秒，记录 `response_time_ms`。 |
| UI | `Assets/Scripts/AudioV0RuntimeDemoUI.cs` | 默认 participant view、紧凑旅程 HUD、识别/评分/连续流程覆盖层；F1 打开研究者控制台，路线和数据管理只在控制台中出现。 |
| 路线和城市 | `Assets/Scripts/AudioV0RuntimeScenario.cs` | 四条约 1.27–1.31 km 平滑路线，约 150–178 m 巡航高度，S 弯、升降、2.2 km 城市地面、道路/人行道/公园/水道/树带、分层建筑、街灯、动态地面交通与 3 个 vertiport landing pad。 |
| eVTOL 与客舱 | `Assets/Scripts/AudioV0RuntimeAircraft/AudioV0RuntimeAircraftBuilder.cs`、`AudioV0RuntimeScenario.cs` | 流线机身、全景玻璃、主翼四推进器和尾部两推进器、三叶桨片、细杆起落架；客舱采用无竖向 A 柱的连续前风挡和后置侧窗分隔条。 |
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

- 默认 `participantViewAtStart=true`，参与者只看到飞行阶段、旅程进度和需要作答时出现的卡片；条件代码、路线选择、tracking 指标和数据管理不在默认视野中。
- 默认 `autoStartDemo=true`；点击 Unity Play 后，场景和审计日志初始化完成便通过 `AudioV0ExperimentSequenceController.BeginSequence()` 自动启动完整四段 technical-demo sequence。
- 运行时设置 `Application.runInBackground=true`，因此 Game 视图失去焦点时路线和实验时钟仍继续推进。
- `F1` 只供实验员打开或关闭研究者控制台。
- 运行时客舱包含地板、顶棚、四块侧窗玻璃、无竖向 A 柱的全景前风挡、后置侧窗分隔条、座椅、扶手、动态 `ALT / JOURNEY` 仪表屏、顶棚软包、三条低亮度氛围灯带和两盏无阴影补光；参与者相机不渲染与视点相交的外部 hero 机体，避免中央视野穿模。
- 出发区有 H 形 touchdown pad、环形引导灯和道路层；城市加入离散冷暖窗格、人行道、车道边线、街灯、动态地面车辆、服务道路、横向道路、公园、水道、树带和远景地形。
- 默认 `showRouteGuidesAtStart=false`，所以参与者不会看到彩色路线线条或运行时路标。
- 顶部“显示路线 / SHOW ROUTE”按钮仅用于操作员检查；再次点击可隐藏。
- 每次开关写出 `route_guides_visibility`，但不会改写 waypoint、路线时间、路线完成逻辑或实验结果。
- `secondaryTaskCandidate=None` 是当前研究基线。后续只有在确认任务适合 VR、符合乘客情境并通过 pilot 后，才能选择候选模式和冻结参数。

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

2026-09-21：macOS 使用可见的 `~/AudioV0_Data/`，本机为 `/Users/trusoegn/AudioV0_Data/`。`EventLogger.GetDataRootPath()` 同时控制新日志、数据目录按钮和技术数据清理范围；数据中心读取其中 `technical_demo/`。其他平台继续使用 `Application.persistentDataPath/AudioV0/`。旧 macOS 目录保留作历史备份，迁移不改写原始日志。验证与清单见 `/Users/trusoegn/论文2/handoff/visible_data_folder_20260921.md`。

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

## 2026-09-18 参与者体验升级与验证

- 最新客舱相机渲染：`Assets/Screenshots/audio_v0_cabin_open_canopy_20260918.png`。已检查前风挡不再有贯穿式竖向 A 柱和突出的上角框，侧窗分隔条位于座椅后方，动态仪表文字方向正确。
- 最新巡航相机渲染：`Assets/Screenshots/audio_v0_rebuild_cruise_20260918.png`。位置约为路线 39%、高度 161 m；近景建筑、远景地形和地平线连续，画面中看不到地图边缘。
- 最新外部 eVTOL 近景：`Assets/Screenshots/audio_v0_evtol_exterior_close_20260918.png`。已检查流线机鼻、全景玻璃、浅色机体、主翼四推进器、尾部两推进器、三叶桨片和细杆起落架；旧 Kenney 盒状交通模型与矩形底板均不再生成。
- 场景与运行时均确认 `routeSpeedMetersPerSecond=18`、`headingDegreesPerSecond=12`；`RoutePlayer` 使用 `Quaternion.RotateTowards` 限制每秒转向角。
- 路线净空运行时检查返回 `LastRouteClearanceValid=true`、`LastRouteClearanceRepairs=0`，检查半径 24 m、沿线采样间隔 4 m；四条路线均位于建筑走廊内侧。
- 为缩短自动化时间，本轮完整路径遍历只在 Play Mode 内临时把速度提高到 `1800 m/s`，没有保存到场景。该检查到达 `Progress=1`、`Session=Completed`、`ExperiencePhase=arrived_postflight_review`、`LastErrorCode=null`，只能证明路径和状态机可完整结束，不能证明正常速度的体验时长或转向观感。
- `useVisualTask=false`、`useContinuousTrackingTask=false`；secondary activity 仍未冻结。
- 建筑窗格改为合并单面 mesh，同栋屋顶细节也合并；桌面运行探针约 185,260 triangles、159,824 vertices、430 draw calls、21 set-pass calls。该数据不代表 Quest 帧率。
- 全量 EditMode：873 tests，871 passed，2 skipped，0 failed；eVTOL builder 定向回归 3/3 passed。
- 最终 Unity 诊断为健康、0 errors、0 warnings、未在编译；`autoStartDemo` 源码默认设为 `true`，Play 后在初始化完成时自动开始完整 sequence。

以上是 macOS Unity Editor 桌面运行时证据。尚未完成 Quest/HMD 佩戴、XR 控制器、帧率、声学或 participant pilot 验证。

## 早期已验证证据（历史快照）

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

## 早期推荐下一步（已由当前节更新）

1. 接入目标 VR 头显，完成 XR 实机 smoke test，并记录设备与 OpenXR runtime 版本。
2. 与 Tram 选择并论证 secondary activity；只有候选确定后，才用 2–3 名 pilot 调整其窗口、间隔、难度和路线速度。
3. 把正式 speech/non-speech 音频、噪声版本、SPL/SNR 测量表和许可信息纳入 manifest。
4. 为 `pilot` 与 `participant_run` 分类建立导出脚本，将 JSONL 稳定派生为 `blocks.csv`、`transitions.csv`、任务事件表和 `block_ratings.csv`；任务事件字段需在 secondary activity 选定后冻结。
5. 用一轮完整的四 block VR pilot 复核路线隐藏、数据分类、清理按钮与退出恢复。

## 相关文档

- `docs/audio-v0-experiment-template.md`：复制场景、Profile、VR 和数据仓库的使用说明。
- `docs/audio-v0-gate-measurement.md`：G00–G12、测量口径、参与者操作和数据位置。
- `docs/audio-v0-codex-execution.md`：原始执行要求、gate 与早期验证过程。
- `temp/audio-v0/20260910-rich-demo/validation_report.md`：丰富 demo 的实现与回归证据。

## 2026-09-23 表面材质与桌面检查

本节替代旧日间窗格自发光与纯色内饰/草坪的呈现描述；保留已有研究和交互实现。

- 修改 `AudioV0EnvironmentAssets` 的贴图绑定、`PassengerCabinRefit` 的座舱 UV/切线与表面、`AudioV0RuntimeScenario` 的道路/草地；新增 Editor 工具 `AudioV0SurfaceMaterialTools`。没有修改声音、路线参数或实验任务参数。
- Poly Haven 1K CC0 资源：三种已有立面的 normal/roughness/AO，以及 leather_white、denim_fabric、grass_ground。下载 25 文件逐项 MD5 核验，其中 21 个进入 runtime；不合适的红色花纹 4 文件已移至 `temp/surface_materials_20260923/rejected_pattern/`，不加载。
- 八组 roughness 离线打包为 mask（R=0、A=1−roughness）；normal 按 NormalMap 导入，normal/roughness/mask/AO 为线性数据。使用 mipmap、1K、aniso 4 与压缩；座舱高光另作哑光调制。
- 编译/import 后实际进入 Play Mode，前视固定同姿态截图及侧后座椅近景检查未见粉色材质或明显纹理拉伸；可见皮革/织物细节。侧后图含开发选择轮廓，只作工程证据。城市仍是简化几何，材质无法完全消除块状轮廓。
- 固定前视审计：draw calls 502→503；triangles 1,191,415→1,191,433（单帧 UI 等存在波动）；material count 46→46。座舱缺 UV/切线的顶点各从 21,544 降到 0。它们是编辑器快照，不是 Quest 性能结论，也不是严格恒定渲染成本保证。
- 最终 Console 查询 error=0、warning=0；Editor 已退出 Play，未编译中。没有运行全量 EditMode tests；本轮验证是导入、编译、实际呈现和运行时网格/材质审计。
- 证据：`temp/surface_materials_20260923/{before_audit,runtime_audit,verification,download_manifest}.json`，本轮前后源码 diff 和备份均保留。截图在 `Assets/Screenshots/surface_{before,final,cabin_detail}_20260923.png`。
- `Surface Review Pose` 暂停路线并移动相机，日志明确标为 synthetic_visual_review_only；不作为正常航程或人类证据。Quest 帧时间、内存、可读性、校准与 pilot 仍待完成。

研究部分本轮线上修订与识别效度 hold 见 `/Users/trusoegn/论文2/handoff/tram_alignment_and_materials_20260923.md`；该材质阶段尚未实现独立片段，后续已由本文当前节替代，研究gate仍未关闭。

## 2026-09-23 树木、天空、玻璃与器件细化

- 44 棵街树采用树皮 PBR 与分簇折面叶片，替代实心球树冠；独立随机流不改变实验随机序列。街道合批逐一处理 submesh，避免只保留第一种叶色。
- 天空升级为 4K 多云 HDRI，调整天空曝光、环境光和指数平方雾；云为静态照片。18 栋近景楼体接入已有立面表面，窗格加入少量确定性差异；地面沿用本轮前阶段的 normal/roughness/AO。
- 增加 5 块窗玻璃和受光 HUD 玻璃，透明面不参与不透明合批；HUD 关闭直射高光。通风口补金属边框、固定件与控制台分缝。
- 已经导入、编译并实际检查前视、树木/地面近景和侧舱。运行审计：44 棵树，叶片合批前后均 126,720 triangles，6 个受光玻璃 renderer；采样 540 draw calls、1,376,751 triangles。相对前阶段约 503 / 1,191,433 增加，不能宣称性能不变或 Quest 达标。
- 最终检查 Editor 已停止 Play，isCompiling=false；Console Error 0 / Warning 0。未运行全量 EditMode 测试或 HMD 测试。图中开发轮廓与暂停状态仅供工程检查，不作论文参与者图。
- 证据：`temp/environment_refinement_20260923/` 中 before、逐文件 diff、download_manifest、runtime_audit、verification；截图 `Assets/Screenshots/environment_{final,tree_final,cabin_final}_20260923.png`。
- 研究与线上论文最新交接：`/Users/trusoegn/论文2/handoff/environment_and_direct_methods_20260923.md`。

## 2026-09-23 早轮独立片段、UI与论文图示（已由当前节替代）

已以 `IndependentExcerptController` 替代本次演示中的固定三阶段路线，目标私有RNG有放回抽样，前段同城同运动，固定识别/信心后转场示意。T01后段保持巡航，不代表真实爬升至巡航过渡。视觉任务RNG独立；关闭技术阶段/按钮音，支持专用无通知视觉检查模式。

`SessionController` Pause跳过片段，Withdraw/Fail/EndBlock关闭并标invalid；导出需要完整链，不由“缺invalid事件”推断有效。信心开窗/响应版本统一fixed_v2，UI修复量表端点截断。首轮四条件12通知为synthetic，11完整、1暂停无效；位置函数10,001点×3目标一致，正常时序lead约18s。948 tests中946 pass、2 skip、0 fail；UI末改另经编译及真实英文截图检查。Console最终0 error/0 warning，Editor停止Play。

论文新增英文实机响应截图和独立片段流程图，主文件main.tex，线上权威。完整交接与Tram会议话术归档在 `/Users/trusoegn/论文2/handoff/independent_excerpt_resolution_and_tram_meeting_20260923.md`。未知人类线索、HMD、校准、cue筛选和精度gate仍开放；没有参与者数据或正式结果。
