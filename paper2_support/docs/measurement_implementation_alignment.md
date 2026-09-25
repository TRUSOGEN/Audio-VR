# 测量、交互和线上 Methods 对照

> Current decision (2026-09-21): silent GO/WAIT task type and basic interaction are selected by the user. See `protocol_freeze_record.md` section 0, the authoritative selection record. Earlier unselected/candidate wording below is historical for task selection; implementation parameters and study release remain pending.

核对日期：2026-09-23。线上项目：<https://www.overleaf.com/project/6a799d68583a7077568f0ad5>（Audio-YegenWu）。本页当前决定已与源码核对；后续线上同步与编译以该轮论文审计为准，历史实现证据保留其原日期。

## 2026-09-23 当前：完整航程、短教程与 M1 选择

用户明确要求四条完整路线、快速完成教学并选择M1。本节取代下文独立片段及长教程作为默认模式；正式参数仍未冻结。`protocol_freeze_record.md` §0.7 记录作者决定，`identification_validity_gate.md` 定义允许结论。

| 项目 | 当前实现与边界 |
| --- | --- |
| 路线 | `context_supported_full_flight_v1_candidate` / `full_city_journeys_v5_210s_candidate`；地面起飞、巡航、左右转弯、下降、地面降落；目标≤210 s/route，正常时钟重跑与 pilot 待确认；方向匹配左右转提示音四条件共享，participant session另计 |
| 目标与推断 | 每条路线按飞行顺序呈现T01/T02/T03，共四条件；正确率/RT为context-supported判断，不能解释为纯声音识别或无视觉/序号线索 |
| 教程 | `participant_tutorial_quick_v2_candidate`：6示例+2次GO/WAIT+1次辨识/信心+1题检查；错误解释后可继续、可自愿重播，Ready后明确启动；practice数据不进入四条件统计 |
| 声音 | 用户选择MATLAB M1两脉冲版作为工程及拟pilot家族；V4 speech保留；M2/C0为历史对照。波形/FFT/STFT为数字构造证据，不是听者效果、耳侧声级或正式冻结 |
| 验证 | 当前闭合synthetic日志7,634事件、5,485运动采样、12通知/12转场、11回答/11信心、1timeout；教程辨识/检查各故意答错一次仍到Ready。下方9次教程/12片段/1,725采样只属旧版本 |

暂停、中断与缺失按当前route/event日志核验，不能沿用历史excerpt“跳过当前片段”的处理假定。学习充分性、Quest可用性、校准、feasibility pilot、样本精度和正式分析仍未完成。

当前证据：[runtime_audit.json](/Users/trusoegn/GitHub/Audio-VR/Audio-VR/temp/20260923-full-route-revision/runtime_audit.json)。软件提前量17.995–18.001秒，日志route-clock合计1,109.197秒；1,112.533秒QA总时长含短暂停/段间切换，不是完整participant session。通知间pause probe验证位置保持/恢复，没有本轮开窗中断实测；退出/缺失和正式统计解释仍按其各自证据范围。

## 2026-09-23 历史：弯曲片段、长教程与 M1/V4 刺激

本节替代下文历史直线运动、自动开始、旧声音和“独立片段未实现”的状态；评分定义仍按既有来源维护，正式参数未冻结。权威实现与验证阶段见 [Unity 当前 handoff](/Users/trusoegn/GitHub/Audio-VR/Audio-VR/docs/audio-v0-handoff.md)。

| 项目 | 当前实现与边界 |
| --- | --- |
| 路线与视觉任务 | `independent_excerpts_v1` / `excerpt_curved_climb_v2_candidate`；固定转场前各目标共用弯曲爬升、速度与航向，之后才分支。渐隐遮挡复位期间暂停 GO/WAIT 时钟，识别/信心窗内继续任务；不是完整飞行模型 |
| 教程与开始 | `participant_tutorial_v1_candidate`；六声示范、GO/WAIT、识别/信心练习、反馈及三道理解检查，Ready 后须明确开始。教程只写 `tutorial_*` / practice，不加入实验识别或视觉统计 |
| 声音 | 已加载 MATLAB M1 三声（1.80 s）与 ElevenLabs V4 candidate（三声约2.638/2.926/2.351 s），T01 输入改为预告，speech speed UI 0.90；数字审计/软件播放不等于可懂度、自然度、响度匹配或正式素材冻结 |
| 完整桌面 QA | 正常速度 synthetic 输入：9次教程识别+9次教程信心；四条件12通知中8次识别+8次信心、3次timeout、1个pause-invalid。12片段结束、11次阶段转场；没有补造暂停片段的转场/回答 |

[导出核验](../temp/excerpt_motion_export_20260923/runtime_snapshot_verification.json) 确认2,209条原始事件保留，1,725条当前 motion 逐字段导出，50条教程事件与任务派生表隔离，60个视觉机会的四区块汇总匹配。全部为 `qa_only`，不能作为识别率、RT、训练充分性或真人 GO/WAIT 表现；软件记录的 onset 不等于声学 onset。

[全量回归](/Users/trusoegn/GitHub/Audio-VR/Audio-VR/temp/20260923-route-sound-tutorial/all_tests.json) 为974项中972通过、2跳过、0失败，早于后续UI修改；[最终编译](/Users/trusoegn/GitHub/Audio-VR/Audio-VR/temp/20260923-route-sound-tutorial/presentation-final/compile_status.json) 为0 errors/10条既有warning。之后[结束/休息页呈现检查](/Users/trusoegn/GitHub/Audio-VR/Audio-VR/temp/20260923-route-sound-tutorial/presentation-final/presentation_audit.json) 仅为synthetic UI，恢复Welcome/Ready，不算重新完成四条件或全量测试。人类visual-only、Quest、校准、声音筛选、完整pilot及precision仍开放。

## 2026-09-23 较早独立片段更新（历史实现）

当时程序 `independent_excerpts_v1`，呈现 `passenger_excerpt_v9`。四条件每条件3片段，每片段1通知；目标三等概率有放回抽取，不保证目标覆盖。地图、进度和阶段标签隐藏。识别窗结束后才打开固定信心窗，转场示意按固定时钟开始，与答案无关。信心开窗/响应统一 `event_confidence_fixed_v2`；1和7的题项锚点独立显示，已核对英文截图。

新联结键为 session + attempt + block + excerpt + transition。暂停跳过当前片段，不重播/补齐；退出、故障和主动结束记录 invalid 与 excerpt_end。导出需要完整证据链，缺末尾/转场或出现中断不能标为有效。技术无效不进识别正确率分母；有效 timeout 仍计不成功。退出后的数据保留政策另依同意书执行。

`temp/excerpt_resolution_20260923/` 保存技术日志、回归结果、受控论文补丁和英文截图；尚无参与者、人类视觉线索检查或Quest验证。以下“每条件三个目标/立即信心/旧路线”的表述属于历史实现。

## 有效来源

- `跟tram开会记录/文字/9.17.txt` 08:36–10:06：识别问题后一道信心题，1–7 分；不是七道信心问题。
- 同记录 13:48–16:32：质疑单题 workload/comfort 等自编量表，短识别问题可留 VR，长主观量表放头显外。
- 同记录 22:12：clarity、audibility、interruption、appropriateness、calmness、annoyance 可作为访谈追问；应先明确 construct 与目的。
- 线上 `methods.tex` 的 Study Procedure（编辑器可见行 75–89）与 PDF §2.6：每次识别后一道 1–7 信心题；四条件后短访谈。当前线上没有要求每段六/七道探索性评分。
- 2026-09-22 的历史同步包含 `main.tex`、`methods.tex`、`references.bib`；当时图示尚未完整导出，该记录不代表后续镜像状态。历史 timeline 不作为现行流程。

## 对照与实现决策

| 目标 | 线上规定 | Unity / 导出 | 验证与限制 |
| --- | --- | --- | --- |
| 识别正确性 | 每条通知第一次三选一 | `identification_response`, `correct`, `parent_event_id` | 识别后不显示正确答案；timeout 单独记录 |
| 识别 RT | notification onset 到第一次选择 | `response_time_ms`，导出核对 monotonic 时钟 | 软件 onset，不等于耳侧声学 onset |
| 信心 | 每次识别后一道 1–7 题 | 新增 `confidence_window_open/response/timeout/interrupted`，使用同一 parent event | 独立评分耗时不覆盖识别 RT；窗口暂为 8 s，pilot-dependent |
| 每段评分 | 当前线上未要求 | 六道历史探索题默认禁用，保留回归入口 | 禁止把单题 workload 当 NASA-TLX 或已验证量表 |
| 视觉活动 | 已选 silent GO/WAIT；block-level 次要描述指标 | 全 GO 曝光期间点击活动区；逐刺激结局与逐点击尝试分开记录 | 不能由成绩证明持续注视或纯声音 interruption；HMD/pilot 待完成 |
| 后访谈 | 所有条件结束后；解释清晰度、舒适/分心和判断过程 | 终场提示摘头显进入访谈 | Unity 不自动生成访谈数据；录音、同意、转写需另行执行 |
| 条件/路线 | 当前决定为四条完整路线、每条按T01/T02/T03顺序；线上以本轮同步记录为准 | `context_supported_full_flight_v1_candidate` 及完整版本日志 | 不把默认顺序称为已验证counterbalance，不将主任务成绩解释为纯声音识别 |
| 设备与声学 | 设备、calibration、noise provenance 待明确 | 用户已指定 Quest 3；软件参数可核验 | 耳侧声级、SNR、standalone 性能、舒适度尚待真实设备 |

## UI 为测量服务的约束

1. 视觉任务在识别和信心评分时继续运行，使用独立区域避免遮挡；任何遮挡、暂停或段间状态不得累计不可见机会的miss。评分引入的双任务成本仍须在分析或时间窗定义中考虑。
2. 当前完整路线允许场景与阶段顺序帮助判断。界面不直接给出正确答案，但这不能证明音频独立性；识别解释限定为context-supported。下方共同前段位置/速度/航向的一致性只适用于历史独立片段。
3. 不显示条件代码、累计成绩或识别正确性。段间主动继续允许休息；终场不向参与者暴露删除日志控件。
4. 信心必须依附已接受的识别响应；识别超时后不补问“刚才答案的信心”。暂停、下一通知与结束均记录未完成信心的原因。
5. 旧 technical_demo 数据保留原貌；没有逐次信心的历史记录导出为 `not_collected`，不能从段末评分补齐。

## 尚未关闭的测量 gate

- 当前完整路线正常时钟runtime QA已完成，范围见首节；下方独立片段、v7/v8及呈现验证数字均为各自历史版本。Quest 3 同时输入与可读性、最终声音与masker资产冻结仍未关闭。
- 视觉任务的评分时间窗、difficulty 与 stage prediction；信心额外负荷及超时规则。
- 设备实测、音频到达时间、正式 counterbalance/precision 与样本量依据。
- 线上 §2.6.1 已补回定量分析；具体模型、exclusions、multiplicity 与样本量精度论证仍待冻结。详见下节。


## 2026-09-23 RQ1 主观体验与识别效度

- 原始要求：`跟tram开会记录/文字/9.10.txt:98` 要求 RQ1 包含 subjective experience；`9.17.txt:107` 将 confidence 归入 effectiveness，不能用它替代 experience。
- 线上 RQ1 已补回对两种设计的体验描述，RQ2 限定准确率与 RT 的 design × noise 比较。识别信心为解释识别判断的次要测量；终场访谈单独探索舒适、分心、情境适合度、清晰度、可听性与偏好。访谈不是已验证的体验量表，也不支持定量体验交互效应。
- 测量链：RQ1 → 乘客对通知设计的主观体验 → 开放式描述与中性追问 → 上述 Tram 原始记录 → 四条件结束、摘下头显后 → 保留原话、设计/听音情境与不能区分条件的回答 → 定性主题分析。英文提纲见 `participant_runbook_and_data_dictionary.md` §7。
- 仍待决定：需要哪个标准体验构念、是否需要对应的完整验证工具及其来源、时点、评分和分析角色。本轮未自动恢复 trust、UEQ 或 NASA-TLX，不能宣称已满足全部体验测量要求。
- 固定阶段顺序允许无声推断，先前曾用独立片段应对；本轮用户选择完整航程后，推断明确收窄为context-supported。高准确率或未报告猜测均不能建立纯声音识别证据，正式release仍待其余gate。
- 该早轮线上八处补丁、逐字回读及六页渲染见 `../temp/tram_next_step_20260923/`；当时图示未完整导出，后续写作状态以最新论文交接为准。

## 2026-09-22 正文评分口径

以下为当前 manuscript specification，未据此宣称导出器已完成全部统计实现或正式 protocol 已冻结。

- Accuracy：技术有效通知中，窗口内第一次回答正确的比例；timeout 计识别失败并单列，技术失败从分母排除并单列。
- RT：软件 notification onset 至首次接受的回答；条件比较采用窗口内正确回答。错误和 timeout 保留在 accuracy 中，缺失 RT 不填窗口上限。
- Response window 与提前通知间隔仍 provisional。按 transition onset 标记提前与非提前回答；后者不能作为提前识别证据。
- Confidence：每次已接受的识别回答后一道 1–7 自编题；识别 timeout 不补问，评分耗时不覆盖识别 RT。
- GO hit rate = hit / scorable GO；WAIT false-alarm rate = false alarm / scorable WAIT。中断和技术无效机会排除并报告，零分母不定义。按 block 描述；不归因于声音单独造成的干扰。
- 定量分析比较 design、noise 及 interaction，考虑参与者内重复观测；binomial mixed-effects 是 accuracy 候选，RT 分布、公式、诊断、排除与多重比较规则仍待正式分析计划。
- 当前完整路线恢复固定cruise–descent–landing顺序，场景和顺序可辅助判断；以上评分定义保留，但其解释限定为context-supported，不外推为纯声音识别。独立有放回抽样只属于历史片段模式。

## 2026-09-21 节奏候选与数据契约更新

本节替代上述历史 Candidate A 图标检测实现描述。`rhythm_timing_go_nogo_candidate_v2` 为新技术候选，不可把旧图标任务的 hit rate 与新任务直接混合。整个游戏区只提交点击意图；`VisualTaskController` 的同一 task clock 决定球进度与命中，UI 不再用球的几何距离替代计分。

- GO 球通过的完整时长来自 `responseWindowSeconds`；有效时间窗为 `hitWindowStart..hitWindowEnd`（用户追加反馈后默认 0..1，全曝光 3.5 s 接受；旧四段验证为 0.4..0.6，两者均 provisional）。WAIT 不点击；targetRate、间隔、窗口和轨迹未经 HMD/pilot 冻结。
- 每个刺激最终恰有一个结局：GO 的 `hit/miss`，WAIT 的 `false_alarm/correct_rejection`，或判定尚未完成时的 `interrupted`。GO 超过 hitWindowEnd 即记 miss，即使球尚在舞台，随后结束也保留 miss；窗口截止前结束才记 interrupted。WAIT 观察期结束前终止仍记 interrupted。已 hit/false_alarm 后提前结束保留已完成结局；中断不进入可评分分母。
- `early/late/repeated/stray` 是点击尝试，不能当作额外试次。早点击后允许同一 GO 在窗口内重试；WAIT 的首次点击即误报；重复点击不会追加计分。非判定区输入与暂停输入记 `input_rejected`，不能伪装成正确拒绝。
- `response_time_ms` 是点击距刺激出现的 active-task time：逐刺激表仅保留最终计分响应的 RT，逐点击表同时保留 early/late/repeated 尝试的 RT；没有关联 onset 的 stray 不产生 RT。`timing_error_ms` 是点击距命中窗中心的带符号时差。两者不能替代通知识别 RT。漏答 RT 保持空值，不能补成窗口上限。
- 暂停同时冻结刺激进度、下一刺激倒计时、反馈时钟；wall clock 继续在公共 `monotonic_clock_ms` 留痕。通知/识别/信心过程仍自然占用任务注意，未自动暂停副任务。
- 每个 block 固定参数快照和 seed；目标与图标使用分离 RNG，离线 `ScheduledTarget` 与真实事件顺序一致。每次 BeginBlock 生成带 run 序号的 stimulus ID；重启时尚未决且未过评分截止点的旧机会记中断，已 hit/false_alarm/miss 保留原结局。
- 掉帧不补造未显示刺激：下个 onset 以实际可呈现帧为锚，并保存 `schedule_lateness_s`；offset 保存 `offset_lateness_s`。这些事件仅证明软件调度，非实际屏幕像素呈现时刻。

导出命令仍用 `tools/export_audio_v0_events.py`；原始 JSONL 只读：

| 派生文件 | 粒度与审计字段 |
| --- | --- |
| `visual_events.csv` | 每刺激最终结局；onset/offset 的 monotonic 与 task 时间、目标时间、window/seed/version、是否 scorable、逐类尝试计数、源 event IDs；历史空窗误报保留附加行 |
| `visual_responses.csv` | 每次点击原样保留；accepted、outcome、response_index、progress、RT、signed timing error、control_id、公共 event_id/event_sequence |
| `validation_report.json` | `visual_summary_checks` 从逐刺激/点击重建 block/run 汇总并核对；`visual_data_quality_flags` 报告缺失/重复 onset、offset、双重评分、点击数量不符、最终结局冲突和越窗 hit；空窗 stray 作为行为保留，不删除 |

联结键为 session + attempt + block + stimulus；逐点击表保留独立行为，不再因为第一下 early 把最终 hit 覆盖。新契约允许多次未接受尝试；旧契约多次响应仍标记 duplicate。主观信心、通知 RT 与副任务的时差分别维护。

当前验证：Python 导出回归 14 tests 通过（包括早后重试、重复点击、迟点击后漏答、中断、不跨 block 合并、缺失与越窗审计）。Unity 定向 EditMode 15/15 通过；全量 937 项中 935 passed、2 skipped、0 failed。全量测试早于随后一处 XR raycaster 条件对齐，该修改另经编译和正常速度运行验证。最终四段数据审计与证据路径见 `../handoff/game_interaction_data_validation_20260921.md`；Quest 3 和人体验证仍未关闭。

CLI 合成夹具验证：`temp/rhythm_data_20260921/synthetic_input/` 明确为 synthetic/technical_demo；成功导出 3 个刺激和 3 次点击，结局 hit、correct_rejection、interrupted；逐 block 汇总重建一致且质量标记为空。该验证只覆盖数据处理路径，不冒充 Unity 运行、参与者或 pilot 数据。

独立审查后的关闭项：视觉配置现在通过 SessionController preflight 在路线启动前校验，旧 task version、逆序窗口和非有限参数明确拒绝，异常失败会暂停路线与背景声。EventLogger 的新增 enum4 初始化分支与 runtime 的缺省版本解析一致。控制器使用 `DefaultExecutionOrder(-200)` 先推进任务帧时钟再供默认 EventSystem/UI 采样；日志声明 `timing_clock=active_task_frame`，仍是帧分辨率的软件时间，不等同像素显示/控制器硬件时间。

导出按 onset/response/offset 的 active-task 时间独立复核 RT、progress、signed timing error 和 hit window，越生命周期响应或评分截止后误标中断会产生质量标记；缺失 onset/offset 或质量异常的试次 `scorable=False`。正式数据存在视觉完整性失败或汇总不一致时退出码非零，相关 session/events 标为 `invalid_formal` 并撤销分析资格；QA 保留所有行供审计。通过的 15 个 Unity 定向 cases 覆盖截止前/后结束、窗口两端、迟按、暂停、preflight 与早后重试。
