# Paper #2：独立飞行片段识别程序候选与 20 人分配审计

日期：2026-09-23（Australia/Sydney）  
状态：候选程序与离线分配审计完成；场景、pilot 和样本精度未验证；论文线上正文未据此改写。

## 要解决的问题

现行 Unity 连续航程和 Methods 每条件固定经历 T01 进入巡航、T02 开始下降、T03 准备着陆。2026-09-22 的完整 synthetic technical log 中，只按每段第 1／2／3 条通知选 T01／T02／T03 的无声策略可匹配 12／12；这不是参与者成绩。现有 RQ1 比较两种通知设计的识别准确率与反应时间，RQ2 比较模拟舱内噪声及其与设计的交互。固定顺序使这些指标难以解释为声音帮助识别的证据。

## 候选程序

1. 保留四条件（non-speech／speech × quiet／cabin noise）、坐姿乘客舱、已选定的无声 GO/WAIT 视觉活动、三类目标、每人每条件三次识别。目标 20 名完成者属于作者的规划偏好，未完成精度论证。
2. 一个条件仍为一个 block，但其中三次通知分别属于**独立、明确提示边界的短飞行片段**，不假装是一段可任意重排物理阶段的连续航程。每段开始前明确告知“新的独立飞行片段”，并重置可见航程进度。目标由隐藏分配表给定；事先告知参与者同一 block 可再次出现同一目标，不能让其用“每类恰好一次”排除答案。
3. 通知前到识别截止，座舱、GO/WAIT、外部景物、运动、HUD、路线标记和音频以外的提示都需对三目标不可辨；显示内容不能露出当前／下一个阶段。参与者继续 GO/WAIT，听通知后按一次阶段选项。第一次有效选择用于准确率与 RT；timeout 单独计数，不能用事后答案补为正确。
4. 从通知起点 `t0` 开始，识别窗在预设 `t0+W` 截止；若保留事件级 confidence，所有已作答者在同一个随后固定窗口 `C` 回答，未作答者不显示该题；目标阶段变化只在预设 `t0+L` 发生，其中 `L > W+C` 并留有余量。`W`、`C`、`L` 和语音长度均由 pilot 决定；阶段变化不能由参与者的回答时间触发，也不能在 confidence 前揭晓正确答案。若这使 advance lead 不合理，应调整或删除事件级 confidence，而不是暗中延长真实飞行情境。
5. 阶段变化后展示对应的真实运动／景物，再明确结束该片段；下一片段独立开始。四条件中预通知、回答和阶段展示的结构保持一致。声音 cue、masker、控制器和声学校准仍按既有 gate 检查。

此程序不能靠调换现有 `FlightStateMachine.cs` 的 T01／T02／T03 规则直接实现：规则读取 `RoutePlayer` 物理进度和状态谓词，`SessionController.cs` 当前每 block 启动一条连续路线。新的片段资产、重置逻辑、固定时序和日志 schema 都尚不存在。

## 20 人分配表与实际检查

- 生成器：`../tools/build_independent_excerpt_schedule.py`；候选种子 `20260923`。
- 候选表：`../temp/independent_excerpts_20260923/candidate_schedule.json`；审计：`../temp/independent_excerpts_20260923/allocation_audit.json`。表含目标答案，只供研究者／运行时使用，不呈现在参与者界面。
- 20 个 participant slots × 4 conditions × 3 excerpts = 240 条计划事件；每人 12 条。四条 Williams 条件顺序各给 5 人；四种顺序合计覆盖全部 12 个不同的有向相邻条件对，实际 20 人各出现五次。
- 每条件 60 条，每目标 20 条；每条件×路线 15 条，每条件×路线×目标 5 条；每条件×路线×试次位置 5 条，目标数为 2／2／1。只看条件和试次位置的最优多数类频率为 35%；即使知道条件、路线和试次位置，表内多数类频率为 40%。这些是**分配表的离线频率**，不是任何人类或视觉画面的真实猜测率。
- 80 个 block 中 58 个有重复目标，其中 5 个三次目标相同。因此不会形成“每类各一次”的个人内排除规则。
- 条件在每个 block 位置各出现 5 人；路线则各位置为 4–6 人，不能宣称完全均衡。四条件顺序与四路线在 20 人且每人各见一条路线的约束下，这份表保留条件×路线精确均衡，接受路线×位置一人偏差；分析和报告应考虑 route／position。
- `python3 tools/build_independent_excerpt_schedule.py` 已成功生成并自审计；另以四个种子检查确定性和平衡，并对 target、excerpt、route 字段错误做三次拒绝检查。尚未把表导入 Unity、没有生成可用片段，也没有做 HMD 或 participant 测试。

## 关键效度与可行性 gate

**P0：视觉／飞行情境冲突。** 进入巡航、开始下降、准备着陆的真实前置飞行状态不同；如果窗口外高度、地形、航线地图或运动已经暴露状态，目标随机表并不能消除无声推断。把画面遮黑曾在 Tram 会议中作为想法讨论，未被确认采用，并可能偏离 visually occupied passenger 场景。本候选只保留乘客舱与视觉任务，要求在实际片段中证明通知前的视听以外信息不会泄露目标；无法证明时应维持 contextual-performance 解释，而非在论文声称独立的声音识别效应。

1. **先做静音／无通知 dry run。** 用拟使用的三个目标片段和参与者可见 HUD，要求不知道目标表的人仅靠画面、航段边界与历史试次预测下一个目标；同时审查录屏的通知前窗口。预设合格规则、猜测基线和异常处理，不能用少数 Agent 截图代替人类 pilot。
2. **核对片段可信度与时序。** 判断独立片段的重置是否让参与者理解为不同航程、GO/WAIT 是否持续但不过度打断、`L` 是否仍是可信的预告时间、confidence 是否会推迟真实转场；用 Unity 日志检查 notification、response deadline、confidence、transition 的先后和时钟来源。
3. **用 20 人最终设计做精度分析。** 三次／条件很少；无声猜测、接近满分的准确率、timeout 和错误 RT 缺失会影响设计×噪声交互。根据预期准确率范围和被试内相关做仿真，决定交互能否作为确认性结果，以及是否要收窄结论。
4. 完成 Quest 输入／可读性、声学校准、cue 学习、2–3 人 feasibility pilot、最终统计方案与伦理／招募 gate；满足后再冻结参数和正式表。

## 论文 change map（未执行）

2026-09-22 核对过的线上 `main.tex` 的 RQ1／RQ2 保留；研究问题无需重写。若上述 gate 通过，`methods.tex` 需将“每个 block 连续、自然顺序的三个 transition”和“fixed sequence limits conclusions”改为独立片段的真实程序，交代重复目标、固定通知到转场间隔、visual-only gate、route／target 分配、confidence 时间和限制，并同步 `figures/study-design.pdf` 的说明。现有 Methods 描述当前连续路线及其限制，不能在 Unity 仍为旧流程时先写成已实施的新设计。

正文可采用的精简表达（**仅在片段实现与效度 gate 通过后**）：

> Each condition contains three separate flight excerpts. Targets follow a counterbalanced schedule and may repeat within a condition. Participants continue the visual activity, identify the announced transition, and respond before the corresponding flight change occurs at a fixed time. The excerpt ends after that change. Condition order and condition--route pairing are counterbalanced across participants; the visible scene and display are checked for advance phase cues before recruitment.

此段仍需按最终 `W`、`C`、`L`、场景和 allocation 实际结果修订，不能直接贴进线上 Methods。
