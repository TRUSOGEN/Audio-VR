# AudioV0 流程文献核验与合理性审查

核验日期：2026-09-14。范围：UAM 乘客熟悉化、block 后问卷、听觉语义训练与双任务方法。
项目输入仅为 [现有测量证据](uam-av-measurement-evidence.md)；历史记录仅用于来源定位。本次重新核验下列原始研究全文的方法与限制，未检查 Unity 源码、场景或运行状态。
结论：熟悉化→声音映射训练→双任务练习→正式条件→条件后问卷有方法依据，但这是跨研究综合的拟议流程，没有一篇文献直接验证 AudioV0 的整套设计。

## 1. 直接 UAM 乘客证据：熟悉化与条件问卷

**Papenfuss et al. (2025)，Experiencing urban air mobility: how passengers evaluate a simulated flight with an air taxi。**
[出版商开放全文](https://link.springer.com/article/10.1007/s13272-025-00823-4)。定位：§4.2 Procedure、§4.6.1–4.6.3、Tables 4–5、§5.1、§6.1–6.2。

先静态试用，再熟悉飞行；正常飞行平衡陪同顺序。每次飞行后问卷与访谈，声音/身心感受和信息需求用五点评分。飞行中 wellbeing 未经心理测量学验证；不涉及声音识别训练。

## 2. 相邻航空警报证据：先学映射，再测并发识别

**Stevens, Perry, Wiggins & Howell (2006)，Design and Evaluation of Auditory Icons as Informative Warning Signals。**
[ATSB 原始研究报告全文](https://www.atsb.gov.au/sites/default/files/media/32715/grant_b20050120_001.pdf)。定位：§2.1.1–2.1.4、§2.2，印刷页 5–7（PDF 第 17–19 页）。这是原始实验报告，非期刊论文。

先训练警报—事件映射，反复训练至 28 个试次达到 100% 正确后进入视觉加法双任务；记录达标次数、识别正确率/RT及加法准确率。比较象似/抽象警报，未比较 speech/non-speech；门槛不能直接移植。

## 3. 相邻航空决策证据：双任务对照与问卷时间粒度

**Giraudet et al. (2015)，P300 Event-Related Potential as an Indicator of Inattentional Deafness?**
[PLOS 开放全文](https://journals.plos.org/plosone/article?id=10.1371/journal.pone.0118556)。定位：Method → Tasks、Procedure、Statistical analysis；Fig. 3；Discussion。

着陆与听觉 oddball 并发，另设听觉单任务；先训练并检查警报检测能力。双任务后填写 NASA-TLX，因负荷混排无法分条件解释。听觉对照固定最后；纯音检测不等于语义理解。

## 4. 相邻飞行员证据：Ayala 单任务基线与双任务

**Ayala et al. (2024)，The effects of a dual task on gaze behavior examined during a simulated flight in low-time pilots。**
[PMC 原论文全文](https://pmc.ncbi.nlm.nih.gov/articles/PMC11611592/)。精确定位：Methods → Experimental setup and apparatus → Scenario and task，Figure 2；Methods → Data processing and analysis，Table 1。

设听觉单任务、飞行单任务及双任务，伪随机排序。听觉代价相对听觉单任务计算；飞行表现分阶段比较单/双任务。明确飞行优先，每 trial 后填情境意识评定技术（SART）问卷。

- **基线细节（已核验）**：听觉单任务时飞机停在跑道；双任务沿用相同声音与响应方式。飞行单任务也包含正常 ATC 通信，因此不是“完全无听觉活动”基线。
- **解释边界（已核验）**：SART 无法拆成巡航/着陆评分，Table 1 的 SART 分析比较飞行单任务与双任务。研究对象为飞行员，听觉任务为纯音目标检测，不能直接证明乘客转场理解。

## 5. AudioV0 合理性审查与 Unity 流程优化建议

以下均为基于上述方法的设计推断，状态为 **planned / pilot-dependent**；不是已实现功能或已验证效果。

| 优先级 | 建议与理由 | 后续验收问题 |
|---|---|---|
| P1 | 分开设备/乘坐熟悉化、声音映射训练与双任务练习，避免把不会操作当成听不懂 | 每阶段是否有独立完成状态、练习标记和失败原因？ |
| P1 | 先确定研究“初次直觉理解”还是“学习后识别”；训练达标会改变研究对象 | 是否记录训练次数、错误类型与未达标者，而非只保留成功者？ |
| P1 | 语音与非语音采用明确、一致的训练政策；固定暴露量与按能力达标回答不同问题 | 若按能力训练，是否分别报告学习成本和正式表现？ |
| P1 | 若要声称双任务代价，需可比较的单任务基线；四条件全部并发只能直接比较条件差异 | 视觉单任务和听觉单任务是否分别提供所需参照，顺序如何控制？ |
| P1 | 明确任务优先级并分别记录视觉、听觉表现，避免只用识别 RT 判断优化 | 错答、超时、漏报、误报与设备失败是否分开，分母能否重建？ |
| P1 | 问卷绑定刚完成的条件，在 block 结束后出现，避免抢占正式识别窗口 | 是否保留 block/condition、题目版本、极性、缺失和暂停记录？ |
| P2 | 从研究问题选择少量主观维度；自拟 clarity/annoyance 题标为自拟或改编 | 是否避免把不同构念直接相加成未经验证的“体验总分”？ |
| P2 | 预先定义练习反馈、正式反馈、休息与退出政策；熟悉路线避免泄露正式提示答案 | 是否能追踪顺序/重复暴露，并区分声音识别与路线记忆？ |

## 6. 对旧证据的限定与未冻结事项

- 旧文档“每 block 问卷”有直接方法先例；“必须如此”及整套量表组合仍是设计判断。
- 旧文档“正式不显示正误反馈”不能由本次核验的 Stevens §2.1.4 直接推出；可作为拟议政策，但不写成其已报告方法。
- UAM wellbeing、声音感受、信息需求与工作负荷属于不同构念；不能把改编 wellbeing 称为已验证舒适度量表。
- AudioV0 的训练时长、达标门槛、重试上限、trial 数、响应窗、block 时长、休息时长和 SPL/SNR 本次均不赋值；需结合目标与 pilot 冻结。
- 本次是聚焦核验，非系统综述；Ayala 出版商入口超时后已通过 PMC 原论文全文完成方法核验。其他旧引文未自动升级为本次全文证据。
- 验证范围为原文方法核对与本文文件检查；未运行 Unity、未做性能测量、未实施流程或问卷。
