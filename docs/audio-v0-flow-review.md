# Audio V0 实验流程评审与 Unity 优化方案

日期：2026-09-14。状态：源码与文献评审、待实施方案；本次未修改 Unity 脚本、场景或实验参数，未进行 HMD 实测。

## 1. 结论与适用范围

四条件被试内设计、飞行中的识别与视觉任务、段后评价、分段休息和原始日志值得保留。当前程序适合技术演示；正式研究前需要完成刺激映射、分配平衡、无提示响应、计数修复、训练与设备验证。

建议核心问题表述为：经过规定熟悉化后，在安静或模拟客舱噪声下，两套通知设计如何影响乘客的阶段识别表现、并行视觉任务表现与主观体验？本实验比较完整通知设计，不能单独归因于音色、空间化或时长。

研究边界沿用 [现有 protocol](<E:/Audio-paper/audio_v0_protocol_and_prototype_checklist.md>)：四条件、三个常规阶段、无 no-notification 条件、无 catch windows。增加正式单任务基线、额外试次或新条件均是待确认设计变更，不在本文件中默认为已批准。

事实来源：当前 `Assets/Scripts` 源码优先于历史运行描述；[交接文档](audio-v0-handoff.md)提供既有运行证据；[gate 文档](audio-v0-gate-measurement.md)定义已有测量框架。后者仍有“仅完成单路线 block”的旧表述，与交接中的四段回归记录不一致，不能据此否定四段流程，也不能把历史回归当成本次验证。

## 2. 当前流程实际是什么

1. 启动运行时场景，创建客舱环境、UI、控制器及技术声音，读取 Profile。
2. 用户选择第一条路线和第一个条件，点击开始四段实验。
3. `AudioV0ExperimentSequenceController` 将所选条件放首位，其余从候选表补齐；路线按起始索引循环。
4. `SessionController` 预检、锁定条件、启动路线和视觉任务；噪声条件启动 `CabinNoisePlayer`。
5. 每个 block 在三个阶段前播放通知；通知软件事件打开三选一答题卡，默认窗口 8 秒。
6. 同时显示橙色菱形/蓝色方块，默认显示 2.2 秒、到期后间隔 2.8 秒；界面直接标记“按下 TARGET”或“忽略”。
7. 飞行完成后回答六个探索性 1–7 分条目，前三段评分后等待手动继续，第四段结束并关闭日志。

该主链路没有完整串接设备熟悉化、声音学习达标、综合练习和正式分配载入。`practiceMode` 标记本身不等于已实现训练流程。

## 3. 源码中需要处理的问题

P0 表示阻止正式采集；P1 表示影响测量解释或流程可靠性；P2 表示体验和维护改进。以下是静态证据，不是本次运行复现。

| 优先级 | 核验事实与位置 | 影响 | 优化与验收 |
| --- | --- | --- | --- |
| P0 | `NotificationAudio.cs` 的 `SelectNotificationClip` 只按 transition ID 哈希选 `variantClips`，不按 targetState/condition；`AudioV0RuntimeScenario.cs:198` 配入三个技术音效 | 实际声音含义没有与评分目标建立明确契约；不同条件名不能证明真的呈现 speech/non-speech | 固定 `(design, target, stimulusVersion) → asset` 映射；六类组合逐个试听并比对日志；正式模式缺资源必须失败 |
| P0 | `ConditionController.cs` 的正式分支检查 speechClips/nonSpeechClips，但当前脚本中未见其接入 NotificationAudio 选音；技术分支允许占位资源 | 资源检查与播放脱节；只改 storageCategory 不会完成正式模式转换 | 分开记录研究阶段、存储类别和执行模式；正式 preflight 检查实际将播放的映射；manifest 禁止 technical 占位混入正式集 |
| P0 | `VisualTaskController.cs:182` 到期关闭非目标时不增加 CorrectRejectionCount；误报后的 offset 也会标成 correct_rejection_window | 段后计数和原始事件含义冲突 | 每刺激仅一个最终 outcome；用已知输入序列复核 hit/miss/FA/CR 和分母 |
| P1 | `RouteAllocationPlanner.cs:9` 候选为 ABDC、BDAC、CDBA、DACB，且 ValidateOrder 只查每条件一次 | 第二维位置中 D 重复、C 缺失；第四位置 C 重复、D 缺失，未达位置平衡 | 使用经过计数验证的分配表，禁止参与者选第一条件；联合检查 route×condition 与 route×position |
| P1 | `AudioV0RuntimeDemoUI.cs:530` 随通知显示答题卡；`:111` 操作面板显示当前阶段，且默认创建可见 | 视觉提示可提醒声音发生；阶段和进度可辅助猜测；漏答不能直接解释为没听到 | 正式模式隐藏操作信息；识别控件稳定呈现或使用固定实体按键，无通知同步弹窗、闪烁或使能变化 |
| P1 | 8 秒答题窗口与默认 4 秒预告提前量并存 | 窗口后半可能已出现真实阶段变化，回答可利用运动线索 | 记录回答相对 transition 的时刻；优先在独立声音测试判断声义识别，在 VR 中限定为情境下识别；若缩短窗口，须确认语音可完整听完 |
| P1 | `VisualTaskController.cs:76–97` 暂停保留旧 deadline，恢复不延长 active stimulus 的 deadline | 长暂停后恢复可能立即产生 miss；暂停时 UI 仍可保留刺激 | 推荐中断当前刺激并单独标记，恢复后用新 ID 开始；非目标中断也须记录，不当 CR |
| P1 | `RegisterResponse` 提交后立即关闭刺激；deadline 只由 Update 检查，响应入口无 deadline 复核 | 实际显示时长依赖反应；帧边界可能接收过期输入 | 回答锁定 outcome 但保持刺激至既定 offset；输入入口检查统一时间戳和窗口 |
| P1 | `IdentificationController.SubmitResponse` 也未独立核查 deadline，未校验非空 target 是否属于三种合法状态 | 不同 Update 顺序下可能接受逾期响应；错误绑定可进入评分 | 响应入口校验 session 状态、合法枚举、deadline、一次性消费，记录拒绝原因 |
| P1 | 正式飞行仍调用 ambience、phase cue、turn cue、UI click/error 等演示声音 | 额外声音可能提示阶段、掩蔽刺激或污染 quiet/noise 对比 | 正式模式默认关闭装饰性阶段/UI 声；必要背景固定并纳入声学记录；quiet 明确定义是否含共同背景 |
| P1 | 每个条件三条识别观测；视觉目标为独立 Bernoulli 抽样，每段同 seed 重置 | 个体条件正确率仅四档；目标数量不固定且可能学会重复序列 | 固定并平衡视觉目标数量与重叠机会；正式 trial 数依据精度/功效规划，增加时长本身不会增加三阶段通知数 |
| P2 | `AudioV0ExperimentProfile.ToManifestDetails` 未包含全部设置，如 target rate、speed、lead、seed 和 XR 尺寸 | 单靠该片段不能重建配置，即使部分字段散见其他事件 | 导出完整配置快照及 hash；保留软件/音频/路线/输入版本 |

### 3.1 分配表的具体修正建议

A=NS_Q，B=NS_N，C=S_Q，D=S_N。可采用现有执行规格原本提出的四行候选：ABDC、BCAD、CDBA、DACB。

这四行在每个位置各出现 A/B/C/D 一次，十二个有向相邻组合也各一次；这是可计算检查的性质。当前代码第二行是 BDAC，与该候选不同。即使修正候选，`BuildConditionOrder` 强行置顶参与者所选条件仍可能破坏平衡。

条件顺序平衡不等于路线平衡。正式名单应预先生成 participant×position×condition×route 表，再对整体计数；重试沿用原分配、新 attempt，不重新抽签。人数不能整除完整方案时，预先说明允许的不平衡与分配原则。

### 3.2 视觉任务的直接验收例子

输入五个刺激：target+有效按键、target+无响应、non-target+按键、non-target+无响应、non-target+中途暂停。

预期 hit=1、miss=1、false_alarm=1、correct_rejection=1、interrupted=1；第五个不进入有效正确率分母。每个刺激只能有一个最终 outcome，重复输入只增加 rejected 事件，不改变计数。再测试 deadline 前后、同帧两个任务输入、提前结束与暂停后恢复。

没有有效窗口的按键记录为 stray response，不自动当成对 non-target 的 false alarm；误报分母必须明确。本轮不据当前计数输出有效 d′。

## 4. 相关论文怎么做，哪些可以借鉴

### 4.1 UAM 乘客研究：Papenfuss 等，2025

原始论文 §4.2 使用设备操作熟悉化和一次适应飞行，然后进入实验；陪同/无陪同的呈现顺序平衡，每轮后有问卷和短访谈，并监测不适。§6.1 指出重复同一路线产生熟悉感。[原文](https://link.springer.com/article/10.1007/s13272-025-00823-4)

对本项目的建议：把熟悉化独立出来、每段后评价、安排休息并控制路线学习。该论文研究的是陪同与改道体验，不能直接证明本项目声音方案有效，也不应照搬它的总时长、设备或问卷作为本项目冻结参数。

### 4.2 声音识别研究：Stevens 等，2006

ATSB 原始报告 §2.1.4 先教声音与事件的对应关系，再反复测试达到规定标准，然后在视觉加法并行任务中测试声音；报告分别记录学习试次、识别正确率、RT 和并行任务表现。[报告正文，印刷页 7](https://www.atsb.gov.au/sites/default/files/media/32715/grant_b20050120_001.pdf)

对本项目的建议：把“是否学会非语音含义”和“学会后噪声下能否识别”分开；练习有反馈，正式测试无正确答案反馈。它是航空告警及 iconic/abstract 研究，不能直接替代 routine passenger speech/non-speech 设计，也不应照抄其训练阈值。

### 4.3 双任务研究：Ayala 等，2024

原始研究将模拟飞行与 auditory oddball 任务结合，比较单任务与双任务表现，并记录眼动。[原始论文](https://www.frontiersin.org/journals/psychology/articles/10.3389/fpsyg.2024.1439401/pdf)

对本项目的建议：没有匹配单任务基线时，应报告“四种通知条件下视觉任务表现差异”，不要把它直接命名为相对于无通知的绝对双任务代价。添加匹配基线会扩大当前方案，需要另行决定；只在练习做一次单任务不能自动成为正式、平衡的对照。该论文对象是飞行员，其主任务和乘客任务不同。

以上为本次在线核查的原始来源；更具体的方法核查记录见 [文献笔记](research/audio-v0-flow-literature.md)。本文件中的 Unity 设计是基于这些先例和源码提出的建议，不是论文原话。

## 5. 建议的完整参与者流程

### 5.1 实验前：实验员准备

1. 核对研究版本、参与者编号与分配表；完成相应知情同意和筛查程序。
2. 检查头显、控制器、音频设备、校准记录、实际音频映射和日志可写性。
3. 输出 session manifest；缺任何必需依赖时不进入正式流程。

### 5.2 入座、熟悉设备与声音

1. 中性座舱中调整座位与视点，练习识别按键、视觉任务按键、暂停/退出；记录输入版本。
2. 静止场景逐个介绍阶段声音，展示对应含义，允许重听；speech 也进行等规则的界面熟悉化。
3. 将声音测试顺序打乱，让参与者只根据声音三选一；练习显示答案反馈并记录混淆和学习次数。
4. 达到预先规定的训练标准后继续；标准、最大训练次数及未达标处理均待冻结。若研究目标是首次使用易学性，应将初次表现作为独立结果，不能通过训练后数据代替。

### 5.3 单任务练习与综合适应

1. 先单独练习视觉任务：用示例说明 target/non-target；测试阶段去掉“按下/忽略”的文字答案。
2. 再进入练习路线，组合声音、识别、视觉任务和暂停。检查按钮是否抢占、是否看不清、是否出现不适。
3. 练习日志标记 practice，与正式分析隔离。练习结束后给休息与重定位机会。

### 5.4 正式四个条件 block

1. 显示“第 X 段/共 4 段”和中性准备信息，不显示条件代码、下一阶段或答案。
2. 实验员确认，参与者就绪后开始预分配路线；正式条件锁定。
3. 视觉任务按预生成时间表连续运行；在三个预定义阶段前播放映射好的通知。
4. 识别响应使用稳定的三选一布局或固定按键；声音发生时不弹出提示卡、不闪烁、不改变按钮外观来提示 onset。
5. 识别窗口仍可在后台存在；窗口外输入完整记录，不用反馈音提醒是否有效。正确答案只写日志。
6. 各任务拥有独立输入。遇到同帧冲突按预先定义的规则记录冲突与拒绝，不将系统拒绝直接解释为感知漏报。
7. 完成飞行后做段后评价，再休息、确认继续；暂停或中断按已冻结规则处理，不默默重播已听过的 trial。
8. 完成四个条件后做简短偏好比较和访谈，关闭日志并生成完整性报告。

正式阶段必须说明任务优先级：例如“优先保持视觉任务表现，同时尽快准确识别声音”为一个待 pilot 检查的候选，不能让不同实验员自由改变说明。

### 5.5 单次通知的时序与解释边界

`预定播放 → 软件/DSP 播放证据 → 识别响应或超时 → 真实阶段变化 → 后续任务`

记录 planned onset、software onset、可获得的 measured acoustic onset、回答、transition onset 和实际声音时长。语音的语义可能在句末才充分明确，RT 包含刺激时长和信息展开；默认比较完整通知设计，不声称纯认知处理速度。

先保证声音能够表达完毕且有可行响应时间，再配置 lead/window。若允许 transition 后回答，导出 `response_before_transition` 并如实报告情境信息可用；不要为了避免线索而任意把窗口从 8 秒砍到 4 秒。

固定的“巡航→下降→着陆”顺序始终可预测。随机化事件时刻只能降低时间可预测性，不能消除类别可预测性。推荐把随机顺序、去情境的声音学习检查作为补充证据；VR 主结果应称情境下通知识别。要将随机独立识别作为正式主实验，需另立分析与分配设计。

## 6. Unity 如何落地

| 现有组件 | 建议修改 | 完成证据 |
| --- | --- | --- |
| AudioV0ExperimentSequenceController | 在四段流程前增加 Familiarization、CueTraining、CombinedPractice 状态；载入冻结分配，保留休息与退出分支 | 不通过准备/训练不得启动正式 block；完整状态日志 |
| ConditionController + NotificationAudio | 将实际 asset、target、design 绑定成唯一可检查映射；禁止哈希跨语义选音 | 三阶段×两设计的逐项播放核验；缺失资源负例 |
| RouteAllocationPlanner | 修正候选并检验整张分配表；不再通过用户首选重排正式表 | 每条件/路线/位置计数与相邻条件计数报告 |
| AudioV0RuntimeDemoUI | 区分 operator 与 participant 显示；去阶段/条件/计分泄露；稳定识别控件 | HMD 视野截图、声音静音时无 onset 视觉提示的技术检查 |
| VisualTaskController | 单刺激单 outcome；固定 offset；修复暂停/中断/CR；预生成数量受控的刺激时间表 | 已知事件序列与导出计数严格一致 |
| IdentificationController | 合法目标、deadline、一次响应校验；练习反馈与正式模式分离 | 错误、超时、重复、暂停、非法目标均有明确事件 |
| AudioV0RuntimeAudioDirector | 正式模式关闭装饰声音；如保留共同背景则固定播放和增益 | quiet/noise 实际输出核验、音轨和校准清单 |
| AudioV0ExperimentProfile + EventLogger | 完整配置快照、allocation/stimulus hash、设备、阶段/模式、attempt 与中断原因 | 单凭 manifest+版本库可重建运行配置 |
| 数据导出工具 | 从不可变 JSONL 生成既定 CSV 和验证报告；不手改原始日志 | 完整四段和中断 session 均可重建，无悄然丢弃 |

优先扩展已有组件，不重建第二套实验系统。参与者 UI 是否固定在客舱或随头部移动，需要目标 HMD 上比较可读性、视线遮挡和点击成本；若固定在头部，说明其不同于实体座舱屏幕，不自动宣称更真实。

Unity `AudioSource.PlayScheduled` 可按 DSP 绝对时间预排播放，降低逐帧触发带来的调度依赖；迁移时必须同步处理暂停、取消及 route clock 与 DSP clock 的映射。[Unity 官方文档](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AudioSource.PlayScheduled.html)

DSP 调度不等于耳端实际 onset。保留回录/设备测量，也不要把计划 offset 或 `isPlaying` 采样当成已测得的完整声音呈现。

## 7. 完成顺序与停止条件

1. **数据与刺激正确性**：修复映射、分配、视觉 outcome 和暂停；先通过确定输入的逻辑测试及实际声音抽查。
2. **测量流程**：接入训练，分开操作员/参与者呈现，消除额外提示和声音，明确窗口与输入规则。
3. **目标设备干跑**：用实际 HMD 和独立 build 跑完四段；记录帧时间、掉帧、跟踪、声学与交互证据。
4. **数据闭环**：导出四张既定结果表与验证报告；按照预期 block/transition 数逐项核对，异常会话保留 incomplete。
5. **可行性 pilot**：沿用原计划的 2–3 次作为初步可行性检查，记录学习、可听性、操作、不适、疲劳及 floor/ceiling；这不足以证明有效或稳定估计效应量。
6. **正式冻结**：确定 trial 数、样本量/精度依据、问卷和分析、训练阈值、时长、校准、输入、退出/排除规则；再做最终干跑。

时长、样本量、训练阈值、路线速度、lead/window、SPL/SNR、目标比例和正式设备均保持 provisional/pilot-dependent。原始计划中的 N=20 或 5–6 分钟/block 不能作为本次核实的合理参数。

建议验收目标：另一位实验员仅依赖说明和冻结配置，能够完成“准备→训练→四段→退出→数据检查”，且无需在运行中改 Inspector、猜条件或修原始数据。

## 8. 本次评审的修改边界

- 新增本评审和配套原始文献笔记，保留既有历史方案与运行证据。
- 没有修改脚本、场景、刺激资源、日志或参数；上文优化全部待实施。
- 没有把源码检查报告为编译、运行、声学或 HMD 验证。
- 后续实施时再更新 handoff、模板和 gate 的对应行为说明，并保留历史验证日期。
