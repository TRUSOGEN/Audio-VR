# Audio V0：论文对齐的 Codex／Unity 自动化执行规格

状态：待实施规格；2026-09-10 核对。本文交付不代表功能已实现、MCP 已连接或 pilot 已完成。

## 1. 使用入口与执行边界

项目根目录：`E:/unity/My project`；主场景：`Assets/AudioV0.unity`。
论文源目录：`E:/Audio-paper/论文本体`，以 `main.tex` 和 `methods.tex` 为研究目标依据。
先读 `E:/Audio-paper/audio_v0_protocol_and_prototype_checklist.md`、`audio_v0_execution_guide.md` 和 `audio_v0_unity_build_walkthrough.md`。
论文中的设计意图约束研究行为；源码和运行证据决定实现状态。旧搭建手册的示例代码仅为入门原型，发生冲突时按本文缺陷清单修复并记录依据。
`experiment_timeline.tex` 明确标记为历史计划，其 N=18、catch trials 和日程不用于当前实现。
本文使用 ask-matt 的规格与依赖分解思路、writing-for-agents 的步骤和完成判据；按用户要求集中成一个 Markdown 文件，未建立外部 issue tracker、未改个人规则。
首次执行读取适用 AGENTS.md、当前源码、场景、包版本；保留未提交修改和所有原始运行日志。
实施一关、编译一关、运行一关、检查输出一关；依赖未通过时不得把后续结果标为完成。

## 2. 给 Codex 的启动任务

```text
读取本文件和指定论文／协议来源，在 E:/unity/My project 完成 AudioV0 自动化原型。
先执行 G00，核实 Unity MCP 可调用、项目路径正确、当前场景和 Console 状态。
连接可用后按 G01 至 G12 依赖顺序实施；逐关执行实际测试并保存证据。
修改现有四个脚本，复用当前客舱、XR Origin 和场景，不另起 Unity 项目。
文件操作用工作区工具；场景、组件、引用、运行和截图优先用实际发现的 Unity MCP 工具。
每次调用先核对工具 schema，不臆造工具名、端口、返回结果和播放回调。
不能访问 Editor 时先完成不依赖 Editor 的代码与静态验证，运行验证标为 blocked。
遇到设备试听、校准、pilot 参数冻结等人工 gate，列明缺少的证据，继续不依赖它的工作。
每关报告修改、编译、运行、输出检查和仍未验证的范围；不以 Console 零报错代替验收。
创建 temp/audio-v0/<run-id>/ 保存测试日志、测试报告和截图；正式原始日志保留在运行数据目录。
不自动重启 Codex，不通过关闭沙箱改变全局安全配置，不自动招募或写入已完成实验结论。
完成后更新本文末尾执行记录，提供可复跑入口和日志位置。
```

## 3. 研究需求与实现映射

| 来源／要求 | Unity 行为 | 验收证据 |
|---|---|---|
| Methods：2×2 被试内设计 | non_speech/speech × quiet/simulated_cabin_noise | 四条件 allocation 与 block 日志一致 |
| 每条件三个 routine transitions | cruise_entry、descent_begin、landing_preparation 各一次 | 状态证据与唯一 ID |
| 提前通知 | notification 先于对应 transition，lead time 可配置 | 分别记录计划与实际软件时间 |
| 识别准确率 | 三选一回答、正确性、漏答和重复输入 | 一通知最多一个有效回答 |
| 连续视觉任务 | 外围 target/non-target 检测持续运行 | 每个刺激与反应有可关联 ID |
| 固定评分区间 | 以通知 onset 为锚点，回答不提前结束区间 | 各条件评分区间规则相同 |
| 四条路线 | 同设备、视点、显示布局与可比视觉复杂度 | route manifest 与分配平衡表 |
| 学习与练习 | 独立练习路线、非语音映射学习 | 学习次数、结果和终止原因 |
| 主观评价 | block 后问卷、末尾 usefulness 和访谈入口 | 题目版本、逐项原始回答 |
| 可审计实验 | raw JSONL、manifest、派生 CSV、异常状态 | 验证器报告与人工检查 |

不增加 no-notification condition 或 catch windows；不把 transition 当第三实验因素。
N=20、5–6 分钟/block、2–3 次 pilot 都是当前计划；速度、lead time、response window、评分区间、设备和 SPL/SNR 未冻结。
本研究比较完整声音设计，不能将效果归因于单一音色、时长或学习因素；视觉任务无无通知基线，不能宣称绝对中断成本。

## 4. 当前证据与必须修复的缺口

已核对版本：Unity 6000.6.0f1；URP 17.6.0；Input System 1.20.0；XRI 3.6.0；OpenXR 1.18.0；Assistant 2.19.0-pre.2；执行时重新检查 manifest。
历史运行日志证明客舱移动、三个 ID 出现、文件输出和 Console 输出；没有本次端到端复测。
现有脚本：EventLogger、RoutePlayer、FlightStateMachine、NotificationAudio；技术音实际试听尚无完整证据。

| 严重度 | 缺口 | 必须处理 |
|---|---|---|
| P1 | 百分比触发可把上升标成下降 | 依据带语义的路线段和状态 predicate |
| P1 | 通知在 transition 时才播放 | 单独的提前通知调度与实际 transition |
| P1 | 等待 clip.length 被写成真实播放结束 | 分开 scheduled、observed、estimated、measured |
| P1 | session_end 单调时间曾倒退 | 独立 session 时钟；统一结束入口 |
| P1 | 秒级 session ID＋覆盖写入 | UUID／毫秒 ID 与 CreateNew 防覆盖 |
| P1 | 写入失败没有停止实验 | Logger 失败进入不可继续的 block error |
| P2 | Resume 可在未开始时启用路线 | 显式生命周期和非法状态错误 |
| P2 | 路线点丢失、非法速度和暂停未完整检查 | 预检＋运行防护＋回归测试 |

## 5. G00：连接 Unity，获取可操作能力

依赖：无。完成判据：实际工具读取目标项目场景和 Console，保存结果；配置文件存在不算通过。
本次工具发现没有可调用的 Unity MCP；本地 `C:/Users/Trusogen/.unity/relay/relay_win.exe` 存在。
本地包说明位于 `Library/PackageCache/com.unity.ai.assistant@8d1e3e89e3a7/Documentation~/integration/unity-mcp-get-started.md`。
同包 `snippets/mcp-deprecation-notice.md` 标记 Unity MCP deprecated，建议 Unity CLI；版本更新后重读现有包文档。

1. 打开目标 Unity 项目，确认已退出 Play，记录未保存场景，先保存用户已有编辑。
2. 打开 Edit → Project Settings → AI → Unity MCP Server；若该版本页面存在，检查 Unity Bridge Running；Stopped 时启动。
3. 从 Integrations／Example Configuration 获取当前 relay 路径与参数；显式绑定项目路径。
4. Codex 配置需保留原 MCP 项；以下是待连接的 TOML 示例，不代表已写入或连接成功。

```toml
[mcp_servers.unity_audio_v0]
command = 'C:\Users\Trusogen\.unity\relay\relay_win.exe'
args = ['--mcp', '--project-path', 'E:\unity\My project']
```

5. 先检查是否已有等价配置，避免重复服务；配置变更后按客户端支持方式刷新连接。
6. Unity 出现 Pending Connection 时由用户确认客户端；这是 Unity 首次外部客户端连接机制。
7. 发现工具并记录实际名称／schema：读场景、查对象、查组件、设置序列化字段、创建对象、保存、读 Console、Play/Stop、截图、测试运行。
8. 用只读请求核对 project path、active scene、Editor 版本；多个 Editor 时必须匹配目标路径。
9. 每次脚本编译或 domain reload 后重新确认连接；超时先读实际状态，再决定是否重试写操作。
10. 能力缺失时使用现有 CLI 或受控 Editor 脚本；先读当前 CLI help，禁止猜测命令。
11. MCP 页面不存在或 relay 失败时，记录错误和版本；优先现有 Unity CLI，必要时单独评估社区 MCP 的版本、来源和安装影响。
12. 阻塞时完成文档／代码静态工作，明确 Editor 运行未验证；不反复执行应用修复或重启。

## 6. 目标结构与组件契约

```text
AudioV0
├─ PassengerCabin（移动根）
│  ├─ CabinFloor / SeatBase / SeatBack / 窗口边框
│  └─ XR Origin (VR) / Camera Offset / Main Camera
├─ RouteEnvironment（静止）/ Ground / RouteWaypoints
├─ Systems（静止，现有组件优先保留）
│  ├─ SessionController / EventLogger / RoutePlayer / FlightStateMachine
│  ├─ NotificationAudio / ConditionController / InputFocusController
│  ├─ IdentificationController / VisualTaskController
│  └─ NotificationPlayer / CabinNoisePlayer（独立 AudioSource）
└─ UI / OperatorCanvas / IdentificationCanvas / VisualTaskCanvas / BlockEndCanvas
```

组件可挂同一个 Systems；树中的职责不要求每个类创建一个 GameObject。
SessionController 统一预检、开始、暂停、结束；其他组件不能自行重启整个 block。
RoutePlayer 提供 route time、segment、位置、速度、下一语义边界和完成状态；Progress 只做显示和诊断。
FlightStateMachine 判断允许前驱、状态进入证据和 once-per-block；NotificationAudio 只负责音频生命周期。
EventLogger 接受结构化事件并返回 event_id／失败结果；输入和刺激通过 ID 关联，不解析 details 字符串作为数据接口。
新增配置优先使用一个版本化 session 配置与路线／刺激资源引用；无需为每个字段建立独立管理器。
需要测试隔离时只抽象时钟、输入、音频观测和文件输出边界；不重写整个架构。

## 7. G01：可靠日志与可读 Console

依赖 G00 的 Editor 验证；代码编写可先行。
1. 修改 EventLogger：session clock 用 Stopwatch；timestamp_ms 用 UTC Unix 毫秒；route_time_ms 单独暂停计时。
2. 用 GUID 保证 session_id、event_id 唯一；transition_id 是 block 内实例 ID，另存 transition_target 和 T01/T02/T03 标签。
3. 开始前验证目录并 CreateNew；显式处理异常，logger 未就绪禁止开始 block。
4. Log 返回写入结果；失败通过 Console／operator UI 显示 LOG_WRITE_FAILED，停止刺激与路线，不递归调用失败 logger。
5. EndSession 幂等：正常按钮、退出、OnDestroy 共用关闭路径；捕获结束时间后再释放资源。
6. 原始日志持续落盘；崩溃后缺少结束事件标为 incomplete，不补造 clean completion。
7. Console 默认摘要：序号、session 秒、block、事件、目标；完整 JSON 用开关控制，文件仍完整。
8. UI 提供当前 LogPath 和打开输出目录按钮；桌面之外显示可复制路径或导出入口。
验收：同秒两次运行不覆盖、暂停后 session clock 不倒退、重复结束只写一次、不可写目录阻止开始。

## 8. G02：语义路线和唯一状态

依赖 G01。先在桌面技术模式实现 climb → cruise → descent → landing_preparation → complete。
1. 备份当前场景并保留 meta；通过 Editor API 做对象和引用修改，避免手工拼接 scene YAML。
2. RouteEnvironment／RouteWaypoints scale=1、rotation=0；Cabin scale=1；检查所有 waypoint 是静止根的子节点。
3. 技术候选点依次为 (0,0,0)、(0,5,10)、(0,5,30)、(0,2,40)、(0,1,45)、(0,0,50)，只用于 smoke test。
4. 给路段显式语义标签；进入巡航须完成 climb 并进入水平段；下降须来自 cruise 且持续满足下降 predicate。
5. 持续下降阈值与持续时间写入 route manifest；landing preparation 要求来自 descent 且进入版本化区域。
6. Start 校验所有点、正有限速度、非零长度、语义顺序和必要段长；运行丢失引用立即报错。
7. 位移预算跨段连续消费，避免低帧率一帧只走一段造成明显时序漂移；暂停保持当前位置。
8. 每个 transition 在一个 block 最多一次；restart 生成新 block／attempt，不复用原实例 ID。
9. Resume 只允许 Paused；Complete／Error／Ready 中 Resume 应拒绝并记录。
验收：日志记录 segment、position、vertical velocity、predicate、前驱与 route_time；逐一和实际场景对照。

## 9. G03：技术音与提前通知

依赖 G02。保留原 technical clip，仅作为播放检查；测试资源登记 technical_only=true。
1. 创建独立通知 AudioSource：Play On Awake=false、Loop=false、Spatial Blend=0；初始低音量试听。
2. NotificationAudio 引用指定 AudioSource 和 logger；预检 clip、mute、音量、启用状态和监听器。
3. 路线控制器根据版本化路线预测 upcoming transition；用相同 route clock 触发提前通知，实际状态仍由 predicate 判定。
4. lead_time_ms 为正的技术配置；三目标都必须有足够预告空间，禁止第一帧即发第一个事件。
5. 区分 notification_requested、notification_scheduled、playback_observed、notification_offset；明确 onset 的 evidence_kind。
6. DSP 调度仅证明软件计划，isPlaying 仅证明采样观测；若无真实回调则记录 unavailable，不伪造 callback。
7. 实际声学 onset/offset 留待回录／校准验证；derived RT 必须声明采用哪个时间锚点。
8. 通知结束、回答窗口和视觉评分窗口必须能在 block 完成前结束；预检拒绝过短路线。
9. 暂停时取消未发调度并在恢复后重算；已开始音频中断标 interrupted，默认该 block incomplete，不无记录重播。
10. planned lead 与 observed software lead 同时导出；偏差容限由目标设备测试明确配置。
验收：每目标一请求、一调度、一个清楚标记证据类型的播放生命周期；实际 transition 晚于软件通知锚点。

## 10. G04：一个可操作的完整 block

依赖 G03。新增 SessionController 和 OperatorCanvas；关闭 RoutePlayer.playOnStart。
1. OperatorCanvas 放 Start、Pause、Resume、EndSession、Status、LogPath；按钮事件连接控制器公开入口。
2. 状态：Ready → Preflight → Running → Completed；Running ↔ Paused；任意运行状态可进入 Error／Withdrawn。
3. 开始前锁定配置、route、condition、stimulus version，写 block_started 后才开始路线。
4. 重复 Start 返回 rejected；结束按钮停止音源、任务、输入窗口、路线，再结束日志。
5. Completed 前等待最后音频和评分窗口结束；之后允许休息和 NextBlock。
验收：仅靠 UI 完成一轮，不在 Inspector 手动切换运行参数；退出留下准确 completion status。

## 11. G05：三选一识别和输入仲裁

依赖 G04。建立 IdentificationCanvas 的三个英文答案按钮；每事件按钮布局一致，不突出正确答案。
1. 保存 pending notification event_id，打开可配置 response window；有效提交后关闭本次回答。
2. 记录 selected_target、communicated_target、correct、response_time_ms、control_id、parent_event_id。
3. 错答／漏答／超时／重复输入分开；漏答结果在窗口关闭时生成，不能当正确或零反应时。
4. 正式 block 不显示正误反馈；练习模式可反馈，并写 mode=practice。
5. InputFocusController 收集同帧输入后统一裁决：系统安全 > pause > identification > visual task。
6. 使用独立可配置 Input Actions；保留 accepted 和 rejected、frame、focus、debounce_reason。
7. 暂停关闭回答窗口并标本事件 interrupted；恢复不把中断时间作为正常 RT。
验收：三种回答、漏答、双击、同帧暂停与回答、窗口外输入都有确定结果和可关联事件。

## 12. G06：视觉任务与固定区间评分

依赖 G05。先选外围 target/non-target 检测作为 candidate，不当作已冻结实验任务。
1. 建立小型 VisualTaskCanvas，使用同一位置和布局；保存 task_version、位置、大小、刺激池和随机种子。
2. 显示时生成 stimulus_id、target flag、onset、planned/observed offset；每个输入关联刺激或明确 no_active_stimulus。
3. 目标按键为 hit，目标无回答为 miss，非目标按键为 false alarm；非目标无回答按规则记 correct rejection。
4. 配置 duration、minimum interval、event rate、response window；避免同一输入可归于多个刺激。
5. 与通知同时发生的 retain/defer/suppress 作为显式策略并记理由；默认技术测试 retain，尚待 pilot。
6. 评分区间 [notification_anchor, notification_anchor+W) 在四条件一致；按刺激 onset 是否入窗确定归属。
7. 暂定保存 hit/miss/false_alarm/correct_rejection 原始计数与各分母；最终单一主评分规则待冻结。
8. 识别提交不结束 W；区间重叠应预检避免，确需允许时写明重复归属规则。
验收：手工可核对的小序列与导出计数一致；任务不中断飞行；输入冲突被仲裁记录。

## 13. G07：四条件、刺激登记与预检

依赖 G06。条件固定为 NS_Q、NS_N、S_Q、S_N；technical 与 practice 不混入四条件数据。
1. 准备三个 speech 与三个 non-speech 候选；保留源文件、导出设置、许可、作者、哈希与修改原因。
2. speech 候选为 Cruise phase starting.／Descent beginning.／Preparing to land.；最终文案仍待确认。
3. non-speech 保留共同家族特征和区分特征的设计记录；先通过学习流程，不声称未学习即可理解。
4. 独立 CabinNoisePlayer 循环背景；quiet 使用同链但不加噪声；记录噪声何时开始、是否贯穿 transitions、何时结束。
5. ConditionController 在开始前锁定映射；缺 cue／noise／route／log path 阻止开始，不静默代换。
6. 运行中设备改变或软件检测到 mute 进入错误；软件无法判断物理耳机断开时标明检测范围。
7. 软件音量不填作 SPL；校准档案单独记录设备、方法、SPL、SNR 和测试日期。
验收：四条件资源与日志一致；技术完整 session 每条件一次、每条件三个事件，合计 12。

## 14. G08：练习、休息、问卷和 session 流程

依赖 G07。状态扩展：Preparation → Acclimatisation → Practice → SoundLearning → Blocks → FinalRatings → Debrief → End。
1. 中性适应场景不移动；桌面 operator_checklist 记录准备状态，不伪造同意／伦理确认。
2. 练习覆盖两种通知格式的响应操作；非语音另行学习；训练 criterion、最大次数、失败处理均配置并标 provisional。
3. 练习路线 ID 与四正式路线不同；学习次数和每次选择记录，禁止混入正式分母。
4. 每个 block 后显示版本化 clarity/audibility/interruption/appropriateness/calmness/annoyance 问卷占位。
5. 占位量表仅用于 UI 技术测试，正式题目、锚点、来源和评分冻结前不能启动 main_study 模式。
6. 末尾 usefulness 与访谈提示；访谈录音需要实际同意，录音和转写是否存在必须可核查。
7. 任意退出保留 completed blocks 和 unfinished block，显示 partial log 路径。
验收：operator 能从准备运行到结束；缺失回答与主动跳过有区别；没有自动补填主观答案。

## 15. G09：四路线与分配验证

依赖 G07；可与 G08 独立开发，集成在 G11。
1. 为四条路线保存几何、阶段边界、总时长、速度曲线与版本；保持座舱／视点／UI 一致。
2. 改变几何和转变时序，同时验证各路线通知、回答窗口、评分窗口都完整落在 block 内。
3. 条件顺序 Williams 候选：ABDC、BCAD、CDBA、DACB；A=NS_Q、B=NS_N、C=S_Q、D=S_N。
4. 验证每条件每位置一次及十二种非自身有序前驱对各一次；执行器应计算验证，不依赖肉眼。
5. N=20 若保留，每条件序列五人；联合 route 分配应每人每路线一次、每 route×condition 五次。
6. route×position 也生成列联表并核对预定目标；求解失败应报告约束冲突，不随机补齐。
7. operator dry-run 可人工选择完整四条件；正式表必须版本化，override 记录理由，不以表现调整顺序。
验收：分配验证报告无重复／遗漏，输出 condition×position、route×condition、route×position 与前驱表。

## 16. G10：目标设备 VR 与环境

依赖 G04，完整交互验收依赖 G08。目标 headset／PCVR 或 standalone 尚待实际设备确认。
1. 在现有 OpenXR/XRI 配置验证中性场景、头部跟踪、退出和控制器；记录实际 provider 与 build target。
2. XR Origin 与 Cabin scale=1；根据 Device/Floor tracking origin 设置眼高，避免 Camera Offset 与地板跟踪重复加高。
3. 主 Camera 保留跟踪驱动与唯一 Audio Listener；不将手动 FOV 当 VR 实际视场证据。
4. UI 在 headset 中核对距离、大小、可读性和遮挡；operator 信息不显示正确答案给 participant。
5. 先短静默路线，再音频，再双任务；记录帧时间、掉帧、舒适度与设备限制。
6. 环境模型最后接入；记录资源许可／版本；测试 streaming、缺网和加载延迟，不让在线加载决定事件时序。
7. 在选定目标上 Build and Run；保留 Editor 与构建版本差异、实际设备日志与运行截图。
验收：真人试听和头显验证由实际观察确认；MCP 截图或 PlayMode 测试不能替代舒适度和声学校准。

## 17. G11–G12：端到端干跑与 pilot 门槛

G11 依赖 G08、G09、G10；桌面集成可先行，目标设备验收单列。
1. 干跑四条件 session；检查 4 blocks、12 transitions、12 通知生命周期、每通知回答或明确 missing。
2. 验证视觉任务计数、ID 引用、单调时钟、condition allocation、版本和部分结束路径。
3. 故障注入仅用测试配置：缺 clip、缺 waypoint、非法速度、不可写日志目录、播放中断、同帧输入、重复 Start、暂停后结束。
4. 每种错误显示稳定 code、recoverable、retry_allowed、partial_log_path；新 attempt 保留旧记录。
5. 导出 raw JSONL、manifest、allocation、派生 identification.csv／visual_task.csv 和 validation_report.json。
6. CSV 可重建，原始 JSONL 保留；新 schema 不覆盖旧日志，旧数据解释记录 clock/source/version。
G12 依赖 G11：安排 2–3 次计划中的可行性 pilot，保留逐次 checklist、reviewer、freeze/iterate/block。
正式招募 gate 还需伦理、刺激／噪声校准、任务、输入、训练、评分、分配、问卷、排除替换规则、分析和样本评估完成。
pilot 不作为效果量结论；N=20 只有三识别事件/条件，需要按论文计划验证估计精度和模型稳定性。

## 18. 日志字段契约与自动验证

公共字段：schema_version、session_id、participant_id、mode、block_id、attempt_id、event_id、event_sequence、event_type、timestamp_ms、monotonic_clock_ms、route_time_ms、route_id、route_version、block_position、condition_id、notification_design、listening_condition、transition_target、transition_id、parent_event_id、stimulus_set_version、application_version、write_status。
非适用字段使用明确 null／空值规则；技术条件 TECH_TEST 只在 technical 模式允许，不混入正式条件。
transition payload：state_source、entry_state_id、entry_predicate、allowed_predecessor_state_id、observed_state、position、segment、velocity、interruption_path。
audio payload：asset_id/hash、device、requested/scheduled/observed time、DSP clock、evidence_kind、lead_time、completion/interruption。
input payload：action、control_id、frame、focus、accepted、rejection_reason、debounce_result；response 包含 selected_target 和 correct。
manifest：clock sources、schema、构建哈希、包版本、配置哈希、噪声版本、控制版本、分配版本、数据目录、结束状态。
验证器断言：JSON 可解析；event_id 唯一；sequence 连续；monotonic 不倒退；parent ID 存在；block／transition 关联一致。
正常 session 断言四条件各一次和 12 transitions；partial session 使用独立规则，不能强迫补齐缺失事件。
派生 RT 仅针对有效回答和已声明 onset evidence；遗漏、错误、负值和中断分别输出，不静默删行。

## 19. 自动化执行和恢复细则

每关：读取状态 → 最小修改 → 等待编译完成 → 获取 Console error 与 warning → 保存 → 运行 → 检查文件内容 → 记录结果。
所有场景创建操作幂等：按稳定路径／标识查现有对象再更新；重试不会复制 Systems、AudioSource 或 EventSystem。
UI 绑定后读取 serialized references 回验；仅工具返回 success 不足以确认引用正确。
脚本修改后保留 .meta；出现编译错误先解决错误，避免继续挂载失效组件。
Editor 正在打开同项目时不启动第二个 batchmode Editor 抢占项目；使用当前 Editor 测试能力或停机窗口。
EditMode 测试覆盖状态、时钟、分配和评分；PlayMode 覆盖场景连线、开始暂停结束、真实资源加载。
音频 fake 只用于逻辑测试；报告明确 simulation，不作为扬声器／耳机实测。
每关 evidence 存 temp/audio-v0/<run-id>/Gxx；正式受试者日志不含姓名／邮箱，权限和留存依机构规范另行确认。

## 20. 执行记录与完成标准

| Gate | 依赖 | 当前状态 | 必需证据 |
|---|---|---|---|
| G00 连接 | 无 | 本轮已验证 | UnitySkills health、project、scene、Console |
| G01 日志 | G00 | 基础实现已验证；故障注入待补 | 唯一 ID、连续序列、CreateNew、session/route clock、写入状态 |
| G02 路线 | G01 | 当前四点路线已验证；版本化路线清单待补 | 语义转场、剩余距离、路线时钟、完成状态 |
| G03 音频 | G02 | 软件调度链已验证；声学 onset 待设备验证 | scheduled/requested/playback_observed/estimated offset |
| G04 block UI | G03 | runtime-only 操作面板已验证；场景持久化绑定待完成 | 开始暂停结束运行 |
| G05 回答 | G04 | runtime-only 三阶段回答按钮已验证；正式 UI/XR 绑定待完成 | 正误漏答与输入仲裁 |
| G06 视觉任务 | G05 | runtime-only 视觉任务已验证；正式刺激与 pilot 待完成 | 计数与固定窗口核对 |
| G07 四条件 | G06 | 四条件锁定与噪声预检代码已编译；正式资源待登记 | 资源预检与 12 事件 |
| G08 流程 | G07 | 流程状态机代码已编译；operator UI 和真实流程待验证 | 练习休息问卷退出 |
| G09 分配 | G07 | 候选顺序生成和唯一性校验已编译；完整平衡表待生成 | 三张平衡表与前驱验证 |
| G10 VR | G04；集成 G08 | 待设备验证 | 实际 headset 与 build |
| G11 干跑 | G08/G09/G10 | 待实施 | 正常／异常全链路 |
| G12 pilot | G11 | 未执行 | 人工 checklist 与冻结决定 |

完成一关须填写：实际修改文件、配置版本、编译结果、测试结果、代表性日志、截图、限制、下一依赖。
本文是实施规格和执行入口；论文只有在对应证据验证后才将 implementation facts 回写，计划保持计划语态。

## 21. 本轮执行记录：G00–G03

执行时间：2026-09-10（Unity Editor `6000.6.0f1`，UnitySkills `2.8.2`）。执行使用当前已打开的 `AudioV0` 场景，未启动第二个 Unity Editor。

已完成：

1. G00：health、项目元数据、场景层级、序列化引用、Console 与编译状态均已读取；surface profile 为 `guide`，因此没有绕过工具限制进行场景写入。
2. G01：`EventLogger` 使用 GUID `session_id`／`event_id`／路线级 `attempt_id`、`FileMode.CreateNew`、UTC Unix ms、独立路线时钟、连续 `event_sequence` 和 `write_status`；路线检测到 logger fatal error 时停止。
3. G02：`RoutePlayer` 使用语义段标签和 predicate 驱动 T01/T02/T03，支持跨 waypoint 消费剩余位移，暂停不消耗路线时钟，运行结束写入 `route_complete`。
4. G03：`NotificationAudio` 使用独立非空间化 AudioSource，执行 clip/source/listener 预检，按路线时间提前调度并记录 `notification_scheduled`、`notification_requested`、`playback_observed`、`notification_onset` 和 estimated offset；暂停／错误路径会清除待播放项并记录 interruption。

本轮 PlayMode 证据：`temp/audio-v0/20260910-g00-g03/validation_report.md`，原始日志路径记录在报告中。事件共 22 行，`event_sequence` 为 0–21 且连续，`event_id` 全部唯一，`session_end` 1 次，T01/T02/T03 各 1 次，通知 scheduled/requested/playback_observed/onset 各 3 次，最终路线时钟 4.120 s，Console errors=0、warnings=0。

边界：本轮是 `TECH_TEST` 的桌面 PlayMode 验证，不等于论文正式四条件 session、VR 头显体验或真实扬声器／耳机声学校准。G04–G12 仍需按依赖顺序实施；G01 的不可写目录／重复 Start／异常退出等故障注入和 G03 的真实声学 onset 尚未宣称完成。

## 22. 后续代码层执行记录：G04–G09

在不绕过 UnitySkills `guide` surface profile 的前提下，新增并编译了 `SessionController`、`IdentificationController`、`VisualTaskController`、`ConditionController`、`CabinNoisePlayer`、`ExperimentFlowController` 和 `RouteAllocationPlanner`。

这些组件分别覆盖 block 生命周期、三选一回答窗口、视觉刺激原始计数、NS_Q／NS_N／S_Q／S_N 条件锁定、独立背景噪声、Preparation→End 流程和四条件候选顺序唯一性校验。正式模式缺少 speech／noise 资源时会显式返回错误并阻止开始，不会静默回退到 technical clip。

代码层编译结果：`debug_check_compilation` 成功，`debug_get_errors` 为 0。随后未修改场景的 `AudioV0` 回归 PlayMode 成功，路线时钟 4.119 s，T01/T02/T03 各一次，Console errors=0、warnings=0；回归原始日志为 `temp/audio-v0/20260910-g00-g09-regression.jsonl`。

未完成项：当前 surface profile 禁止把新增组件和 Canvas 持久化写入场景，因此本轮通过运行时 bootstrap 提供桌面 technical demo，不把 runtime-only 绑定写成正式场景资产。G07 正式刺激资源、G09 完整分配表、G10 真实 headset/build、G11 四条件干跑和 G12 pilot 仍不能标为完成。

## 23. 丰富 UAM 体验的 runtime technical demo

为满足“用户能够连续体验并测试，而不是几秒钟飞上去再落下”的要求，本轮在不改写 `AudioV0.unity` 场景 YAML 的前提下增加运行时体验层：

- `AudioV0RuntimeScenario` 在 `AudioV0` 场景启动时创建 4 条路线：`route_alpine`、`route_coastal`、`route_mountain`、`route_desert`；每条路线 12 个 waypoint、11 个语义航段，包含明显的 S 型横向转弯和多段爬升/下降，当前演示速度为 2.5 m/s，目标时长约 60–80 s。
- 路线阶段显示为 `boarding_safety_briefing`、`takeoff_climb`、`cruise_city_view`、`enroute_turn_cruise`、`descent_approach`、`landing_preparation`、`arrived_postflight_review`，并作为 `experience_phase` 写入 JSONL。
- `AudioV0RuntimeDemoUI` 改为 CanvasScaler + VerticalLayoutGroup + GridLayoutGroup 的响应式面板，移除截图中导致错位的绝对坐标按钮；增加清晰的状态卡、路线卡、条件卡、控制卡、阶段回答卡和双语切换。主字体优先使用 Microsoft YaHei UI / Segoe UI，不再用难读的像素字体承载长文本。
- 路线按钮在运行中可用：切换时记录 `route_switch_requested`，结束当前 block 并自动启动新路线；当前演示仍保留每个 block 独立的日志边界。
- 视觉刺激不再只停留在日志：视觉任务激活时，右侧出现高对比度半透明事件区和大尺寸视觉方块，显示 stimulus ID 但不透露 target 属性，避免操作面板提前泄漏答案。
- 通过 `AudioV0RuntimeAudioDirector` 分离环境底噪、阶段 cue、UI click/confirm/error 和到达 jingles；`NotificationAudio` 支持按 transition ID 选择多种通知变体，所有声音都只服务于 technical demo，不替换正式刺激定义。
- `BuildRuntimeCity` 以固定 seed=1701 生成低多边形方块建筑和 2 个 landing pad，建筑整体移到航路两侧，路线本身保持可见；当前生成 37 个建筑方块和 2 个 landing pad。
- 运行时通过 `InputSystemUIInputModule` 创建 `EventSystem`，使面板按钮具备实际输入路径；该 Canvas 是 technical demo 的运行时对象，不代表正式 XR 交互绑定完成。
- 已下载并实际加载 Kenney UI Pack、Kenney Prototype Kit、Kenney Sci-fi Sounds、Kenney Interface Sounds 和 Kenney Music Jingles；资源来源、许可证和实际路径记录在 `Assets/ThirdParty/SOURCES.md`。
- Poly Pizza cockpit GLB 仅作为候选参考保存在 `temp/asset-downloads`，当前没有引入 glTF 依赖或把它冒充为已使用资源。

最新完整桌面 PlayMode 验证：`C:/Users/Trusogen/AppData/LocalLow/DefaultCompany/My project/AudioV0/session_20260909_184858_639_be0849ae63b947c380d9d7be9fff7f36/event_log.jsonl`。该次日志 101 行，`event_sequence` 为 0–100，路线 `route_alpine` 的路线时钟约 50.459 s，状态最终为 `Completed`，包含 1 次 `route_start`、1 次 `route_complete`、1 次 `block_end`、1 次 `session_end`；T01/T02/T03 各 1 次；五类通知生命周期各 3 次；视觉刺激 onset/offset 各 28 次；Console 编译错误为 0。

这项验证支持“桌面技术演示已经形成可操作的多阶段 UAM 体验”，不支持“论文正式四条件、语音／非语音正式刺激、噪声校准、XR 舒适度、正式分配或用户 pilot 已完成”。

## 24. UI / audio / route-switch overhaul validation

2026-09-10 的最新回归日志为 `C:/Users/Trusogen/AppData/LocalLow/DefaultCompany/My project/AudioV0/session_20260910_022507_725_ae4c7458a40a443a9fb80c80898b73b1/event_log.jsonl`。通过真实 Play Mode 帧推进完成一条完整路线，结果包括：`runtime_city_ready=1`（37 个建筑方块、2 个 landing pad）、`audio_phase_change=6`、`route_complete=1`、通知 requested/onset/playback_observed/offset 各 3 次、视觉刺激 onset/offset 各 28 次，最终阶段为 `arrived_postflight_review`；停止 Play Mode 后 `debug_get_errors.count=0`。

运行中路线切换已在代码层闭环：`AudioV0RuntimeDemoUI` 的路线按钮在 Running/Paused 状态仍可触发 `AudioV0RuntimeScenario.SelectRoute`，该方法记录 `route_switch_requested`、结束旧 block、应用新 route，并自动启动新 block。由于当前 guide surface 不开放脚本注入或 UI 点击回放，本次运行证据覆盖了按钮/对象生成和完整路线回归，运行中真实点击切换仍需在 Game View 手动点一次作为最终交互验收。
