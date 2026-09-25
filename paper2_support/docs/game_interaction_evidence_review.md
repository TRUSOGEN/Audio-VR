# 游戏交互与测量证据核查

日期：2026-09-21，Sydney。状态：技术候选设计审查；非正式任务选择或参与者结果。

## 1. 结论与适用范围

本次读取 `handoff/current_integrated_feedback_handoff_20260921.md`、`handoff/chi_hud_hmi_review_20260921.md`、现有视觉任务规格、测量对照以及权威 Unity 工程的 UI/VisualTaskController。旧 CHI/HMI handoff 中部分“可迁移原则”超出了已取得的论文证据。本文件对其研究解释作限定；历史文件保留为当时记录，后续论证以本次核验边界为准。

当前应优先完成一个**静音、规则单一、状态可恢复、数据可重建的视觉时机任务**。固定目标、可预测球轨迹、即时短反馈和独立通知响应是作者设计假设，不能称为 CHI 已验证的最佳游戏。可玩性提升与足够视觉占用可能存在取舍；不能把最高命中率直接当作最合适的副任务。

没有修改论文正文、正式分析、量表或冻结参数。代码审查只说明进入本轮时的问题；实现和运行验收以主交接及最新源码为准。

## 2. 已核对原文

### S1：VR 通知、任务类型与打扰的取舍

Rzayev, R., Mayer, S., Krauter, C., & Henze, N. (2019). *Notification in VR: The Effect of Notification Placement, Task, and Environment*. CHI PLAY. DOI：[10.1145/3311350.3347190](https://doi.org/10.1145/3311350.3347190)。[作者全文](https://sven-mayer.com/wp-content/uploads/2019/08/rzayev2019notification.pdf)。本次取得 13 页 PDF，核对 Methods、Results、Discussion and Limitations。

- **方法位置**：PDF pp.3–6。24 人比较四种视觉通知位置、三类任务和三类环境；设备为 HTC Vive。游戏任务是走动收集球，另有文本理解和卡片分类；并非坐姿节奏游戏。
- **原文发现**：pp.6–7、9–10。任务类型及通知位置影响漏报和响应时间；头部固定 HUD 通知最显眼、响应更快、漏报更少，同时更 intrusive/disturbing。学习任务的漏报和 RT 高于游戏任务。
- **可用方法先例**：pp.3–4。通知使用独立的控制器 trackpad 响应，避免必须瞄准通知；pp.5–6 先熟悉任务与通知，成组条件后摘头显填问卷。主观量表按 placement 汇总，不能据此推断各个 task 的独立主观得分。
- **重要统计限定**：p.6 将漏报通知 RT 赋值为 15 秒。该方法是论文自己的操作化，不是本项目的必选规则；本项目保留未响应 RT 缺失值与 timeout 状态，避免当成真实观测 RT。
- **不能推出**：中央游戏＋左侧地图＋底部识别的具体尺寸、最佳位置；听觉通知效果；UAM 乘客生态有效性；当前球游戏优于阅读或追踪。

### S2：VR 阅读尺寸、距离与偏好

Dingler, T., Kunze, K., & Outram, B. (2018). *VR Reading UIs: Assessing Text Parameters for Reading in VR*. CHI Extended Abstracts, LBW094. DOI：[10.1145/3170427.3188695](https://doi.org/10.1145/3170427.3188695)。[作者全文](https://kaikunze.de/papers/pdf/dingler2018vr.pdf)。本次取得并读取 6 页 PDF。

- **方法位置**：pp.2–3。18 人在 Oculus Rift CV1 中自行调整文本角尺寸、vergence distance（双眼会聚距离）、文本框及显示偏好。它是舒适度偏好探索，未检验本项目的中文字体或 Quest 3。
- **原文发现**：p.4 Tables 1–2。13/18 偏好深色背景浅色文字，14/18 偏好 sans-serif；结果以角尺寸报告，说明相同像素字号不足以描述观看大小。
- **限定位置**：p.5 Discussion/Conclusion。作者指出偏好变异较大，后续还要检验阅读理解、视觉疲劳和 motion sickness。不能把这些初步偏好写成已证明提高识别准确率或降低晕动。
- **设计迁移**：给提示和响应项稳定衬底、减少无关文字，并记录世界空间尺寸及眼到面板距离；在最不利坐姿/头位、亮暗背景检查辨字、误触及遮挡。
- **不能直接移植**：文中的约 3 m、字体角尺寸或范围不是 Quest 3 的强制值，也不能证明当前面板必须移到 3 m。

### S3：中断后的恢复必须独立测量

Monk, C. A., Trafton, J. G., & Boehm-Davis, D. A. (2008). *The Effect of Interruption Duration and Demand on Resuming Suspended Goals*. Journal of Experimental Psychology: Applied, 14(4), 299–313. DOI：[10.1037/a0014402](https://doi.org/10.1037/a0014402)。[公开全文](https://interruptions.net/literature/Monk-JEPA08.pdf)。本次取得 15 页 PDF，重点核对印刷 pp.301–303、309–311。

- **方法位置**：印刷 pp.302–303 / PDF pp.4–5。VCR 编程与追踪交替；同时只有一个任务可见。resumption lag 定义为切回 VCR 到首次界面按钮点击的时间，并另外检查恢复错误与追踪表现。
- **原文发现**：印刷 pp.309–311 / PDF pp.11–13。更长、认知需求更高的中断通常带来更长恢复时间；整体完成时间可能掩盖恢复阶段的代价。
- **限定位置**：印刷 p.310 / PDF p.12。环境线索在各条件均存在，作者明确说其独立作用尚未被分离。因此“显示保留状态有助恢复”在本项目仍是待验设计假设，不能声称本文证明了暂停画面或轨迹预览的效果。
- **迁移边界**：这里是强制交替的桌面任务，Audio-VR 是视觉任务与音频/识别/信心并发；不能把通知结束到下一次游戏点击直接命名为同等的认知 resumption lag，因为它还包含等待下一次可命中机会的时间。

## 3. 对旧 handoff 四篇引用的修正

下表区分“本次取得了什么”和“可支持到哪里”。Crossref 元数据与 OpenAlex 摘要只用于限定范围，未升级为全文证据。

| 旧来源 | 本次取得 | 可保留与需收回的解释 |
| --- | --- | --- |
| T9+HUD，10.1145/3003715.3005453 | Crossref 元数据、OpenAlex 摘要、Aalto 作者机构页面；机构 PDF 地址实际返回 HTML，全文未成功取得 | 是方向盘物理键盘＋HUD 的驾驶文本输入比较；可定位视觉分心问题。不能引用为三区布局或固定区优于中央堆叠的实验证据 |
| Focal Plane Distance，10.1145/3580585.3607166 | OpenAlex 元数据及摘要；ACM PDF 403 | 摘要报告操纵焦平面距离、进行知觉距离匹配。不能说本研究验证了尺寸、对比度、背景对目标定位的共同影响；也不能将车载 AR-HUD 焦平面实验当成 Quest 光学验证 |
| Safety, speed, and style，10.1145/1520340.1520383 | OpenAlex 元数据及摘要；未取得全文 | 摘要为跨学科团队车载多模态界面的设计案例。完成时间、误操作等可作为本项目评价建议，但不能在未查全文时说这篇已逐项测量或验证 |
| Stretchertainment，10.1145/3131726.3131739 | Crossref 元数据及 OpenAlex 摘要；未取得全文 | 是车辆停下时引导驾驶者身体拉伸的 HUD infotainment。收回其支持短回合可恢复视觉游戏、核心区/状态区分隔、减少运动元素的具体实证主张 |

这四篇未提供“至少三种布局×三档球速”的必要性或最优数值；该比较数量只能作为工程筛选计划，不能称为论文要求。

## 4. source → finding → design hypothesis → metric → boundary

| 来源与发现 | 本项目设计假设 | 指标与记录时机 | 允许结论与限制 |
| --- | --- | --- | --- |
| S1：不同视觉任务影响通知表现 | 先做清楚可学的单规则静音时机任务；同一轮候选验证固定 task/difficulty version | 每个机会的 hit、miss、false_alarm、correct_rejection、interrupted；early/late/repeated/stray 另记逐次输入；通知另记正确率、漏报、有效 RT；block 后问理解和负担 | 技术链路正确＋pilot 难度合理；不能据一套任务断言所有 visually occupied passengers 均适用 |
| S1：更显眼通知兼有更强打扰 | 通知识别/信心保留独立稳定区域，避免额外全屏闪烁或遮盖球轨迹 | 通知窗口重叠游戏机会的命中与 timing error；主观打扰/理解分别报告 | 显眼程度、效率、体验需并列；不能只按最低 RT 选方案 |
| S2：显示偏好涉及角尺寸、距离与背景 | 文字稳定衬底、短提示、足够辨识的中英文，球与背景分离 | Q3 实机不同头位/座高的提示复述、误读、误触；记录面板宽高、距离、渲染/字号版本 | 当前硬件条件下的可读性检查；桌面截图不能证明 Q3 舒适度 |
| S3：恢复时间不同于全程表现 | 每次机会独立结算，错误后进入下一机会，不清空整个 block；显示可理解当前状态 | 探索性记录通知/识别/信心结束到下次可用机会，以及该机会表现；事件先留原始时间 | 机会级恢复描述，不直接等于标准 resumption lag；若未观察真实注意切换，不声称测得注意恢复 |
| 工程测量契约，非论文效果 | operator pause 冻结游戏 active clock，恢复继续保留进度；结束的未完成机会标 interrupted | pause/resume 两端 monotonic timestamp、task elapsed、active stimulus ID、reason；结束计数对账 | 可以确认软件没有把暂停当 miss；不证明参与者恢复无成本 |
| 本研究听觉设计操纵边界 | 游戏没有额外节拍/命中音效，各条件任务提示一致 | 配置记录 visual feedback mode、audio policy、task version；核查实际音源 | 避免新增声音混杂；不表示静音时机游戏已有生态有效性 |

## 5. 判定与数据契约建议

1. **一种任务对应一种语义。** 进入本轮时 UI 的“球进圈”与 controller 的随机 target/non-target 共存，`insideTarget` 被当作 `targetControl` 使用。修改应删除该混合语义或明确分支，升级 task ID/version；不得用新日志覆盖旧检测任务。
2. **同一时钟驱动显示与判定。** 显示相位、理想命中点、有效窗均由 controller 的 active task time 求值。UI 不再通过屏幕上球的位置反推成绩；否则缩放布局会改变判定。
3. **区分时间指标。** `response_time_ms = response_task_time − stimulus_onset_task_time`；`timing_error_ms = response_task_time − ideal_hit_task_time`。前者是出现到响应耗时，后者可正可负；报告绝对时机误差时另外命名。
4. **每个机会只能有一个最终结果。** 本轮最终 GO/WAIT 契约中，GO 结局为 hit/miss/interrupted，WAIT 结局为 false_alarm/correct_rejection/interrupted。early、late、repeated、stray 是逐次输入结果，单独计数；early 可重试，不能把其次数加进刺激结局分母。GO 在有效命中窗结束后尚未命中即应结算 miss，仍可继续显示球；其后 late 输入不再增加 miss，结束 block 也不能将已结算 miss 改成 interrupted。无活动刺激的 stray 与 WAIT 上的 false_alarm 分开。
5. **分母可重建。** `scheduled = resolved + interrupted + active_unresolved`；已结束 block 应无 active_unresolved，已结算但仍显示的刺激属于 resolved。`resolved = hit + miss + false_alarm + correct_rejection`。GO 命中率为 `hit / (hit + miss)`，WAIT 误报率为 `false_alarm / (false_alarm + correct_rejection)`；并列报告两类 scheduled、interrupted 和逐次输入计数，不能将所有刺激混作 GO 命中率分母。空分母输出缺失值，缺失/冲突日志需要质量判定，不能只信 offset 的 scorable 标志。
6. **暂停不等于通知中断。** operator pause 冻结任务时间；音频通知、阶段识别和信心题按当前测量对照保持视觉任务运行。若三者时间重叠，保存各窗口，不把 confidence 带来的额外负担全归因于声音。
7. **保护比较条件。** 技术演示可显示分数和即时反馈；研究候选若改变反馈政策须记录 mode/version 并在所有声音条件一致。是否正式保留累计分数需 protocol 决策，不能静默改变既有候选不显示分数的规则。
8. **不自动调整正式难度。** 难度可在熟悉化/研究者技术模式调整并记录；block 中随表现自动变速会使各声音条件暴露不一致。任何正式自适应政策需另行设计和冻结。

## 6. 下一验证 gate

- **软件**：真实 UI 输入一次命中、提前/延后输入、重复点、漏掉机会、暂停跨判定窗、恢复、结束/重启；核对 UI 与导出逐事件一致。测试器调用 API 不替代鼠标/控制器实际可点。
- **桌面四条件**：任务版本、窗口与音频策略一致，各 block 的机会序列与其记录的 seed 可重建对应；不同 route/block 可产生不同序列，不要求四条件完全同序列。查看低空/巡航/下降的背景遮挡，以及识别/信心是否覆盖任务。
- **Quest 3**：坐姿佩戴、独立输入、射线选择、文字和当前状态理解、帧时间与不适；未进行前保留未验证状态。
- **feasibility pilot**：观察是否可理解、可恢复、是否只靠记节拍而不看画面、是否 floor/ceiling、是否降低乘客情境可信度。样本、速度、窗宽、反馈政策和评分窗口仍 provisional/pilot-dependent。

本次没有找到并核验直接证明“弹跳球进圈”适用于 UAM 音频通知实验的原始研究；不以一般游戏化或 motor timing 论文替代该缺口。

## 7. 复核材料与边界

原文和元数据保存在 `../temp/game_interaction_sources/`：`rzayev.pdf/.txt`、`reading.pdf/.txt`、`monk.pdf/.txt`；`oa_t9.json`、`oa_focal.json`、`oa_safety.json`、`oa_stretch.json` 用于限定旧引用范围。仅本地研究核验使用，不再分发全文。下载返回 HTML 的旧 `t9.pdf` 已移除，避免误认取得论文。

本次进行了网页/DOI 元数据查证、三篇 PDF 文本核对、针对性源码与文档只读审查；未运行 Unity、未测试 HMD、未做参与者研究，也未选择正式统计模型。nature-reader 的来源定位方法用于核验；本任务交付是限定范围的证据审查，不是全文翻译读本。
