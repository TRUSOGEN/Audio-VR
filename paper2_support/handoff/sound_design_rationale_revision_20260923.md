# Paper #2 声音设计论证与 9.23 会议修订交接

日期：2026-09-23。范围：Paper #2 Audio Experiment。用户要求阅读最新 handoff、今天会议、当前论文及原论文库，发现问题并改进；本轮完成线上 Methods 修订与文档同步，未把 Unity 工程需求或人类 pilot 报为完成。

## 有效版本与入口

- 写作权威：[Overleaf Audio-YegenWu](https://www.overleaf.com/project/6a799d68583a7077568f0ad5)。主文件 `main.tex`，实际编译器 pdfLaTeX / TeX Live 2025。
- 本地同步文件：`overleaf/methods.tex`；`main.tex` 线上复制与本地逐字一致，本轮未改。
- 原始会议：`跟tram开会记录/文字/tram会议 9.23.txt`，已实际读取。转写存在错词及说话人混淆，以语义清楚的上下文为准。
- 执行进度：`docs/plans/20260923-tram-meeting-execution.md`。
- 前序交接：`handoff/trial_design_methods_tram_comments_20260923.md`、`handoff/independent_excerpt_resolution_and_tram_meeting_20260923.md`。
- 声音设计与筛选的详细定义：`docs/sound_family_design_and_screening_manual.md`，本轮增量更新后为 281 行。
- 原论文证据库：`/Users/trusoegn/论文项目/Final/final_corpus_26/`；本轮仅借用原论文核验过程先例，没有修改 Scoping Review 数据或论文。

方向匹配左右转 cue 的当前边界：T01--T03 notification 仍使用同一 playback chain；左右转 cue 是四条件共享的 passenger-journey delivery，记录为 `spatial_turn_cue`，不计入 T01--T03 识别分母，也不构成 spatialisation 第三因素。其 HMD 映射、HRTF/SDK、roll-off、舒适度和 masker interaction 仍须 pilot/calibration 检查。

## 问题与改变

| 位置 | 问题和来源 | 实际改变 |
| --- | --- | --- |
| Methods 2.1 non-speech | 会议 06:44、08:35、20:04–21:31 要求解释设计理由、候选及筛选过程 | 补四个 candidate families：contour、interval、rhythm、brightness；家族内共享身份和差异设计；只选一套进入主实验 |
| Methods 2.1 rationale | 旧稿只有笼统的 audibility/comfort 等，没有把先例、规则、筛选和修订连接起来 | 加 Kim/Nadri 的相邻 AV 方法先例；说明 learned identification/confusion、coherence、comfort、interruption、两条件 audibility；筛选前写规则，记录保留/排除理由，pilot 后修订冻结 |
| Methods 2.1 speech | 会议 15:52–16:13 指出 speech 偏快；当前 T01 表示已经进入巡航，与预告不一致 | 补 routine cabin announcement 节奏目标与两条件 intelligibility/pacing 检查；保留 T01 需要替换的明确状态，未宣称已重录 |
| Methods 2.2 | 2026-09-24 用户决定把左右转提示音纳入四条件；需要公平投送比较 | 三个 T01--T03 notification 继续用同一 playback chain/noise source；方向匹配 turn cue 在四条件共享，source side/spatial blend/HRTF/roll-off 恒定，不构成第三因素；新增 masker 构成 listening manipulation |
| Methods 2.3 | 旧稿将 post-response motion 泛称为真实阶段呈现；会议反馈路线只直飞 | 改成当前源码支持的事实：T01 水平、T02 下降、T03 直线减速；尚无完整进近或转向路线 |
| Methods 2.4 | 会议试听时参与者式操作需要解释；完整流程需要休息与理解检查 | 加短示范、GO/WAIT/识别/信心练习和 instruction-comprehension check，pilot 检查引导是否充分；条件间提供休息 |
| Methods 2.7 | 缺少本次会议的具体 pilot 检查及完整路径 | 加信息说明/同意/筛查/教程/四条件/休息/访谈/退出/log export；明确 T02/T03 混淆、T03 感知时长、语速、教程检查；分别记录 exposure 与完整 session 时长 |
| Methods 2.7 | 20 人尚没有最终精度论证，但导师今天改成直接的 study includes 表述 | `Study Participants` 整段逐字保留；仅在 pilot 末段说明 20 为 planning value，condition contrasts 与 interaction 的精度仍待评估 |
| 声音手册 | screen/pilot/freeze 顺序容易被混淆；Basantis 的旧 DOI 错指 Nadri 2021 | 区分 pilot 候选锁定与正式冻结；规则先于 screen；补会议口头反馈边界；Basantis DOI 修正为 `10.1109/THMS.2021.3106892` |

论文共九处唯一段落替换，净增加 299 个英文词，保留两张图、既有结果指标和所有未修改正文。未接受、拒绝或 resolve 线上 tracked changes / comments，也未发布回复。

## 文献依据与允许结论

- Kim et al. (2024)，DOI `10.1016/j.trf.2023.11.007`：原 PDF 第 6 页 / 印刷第 27 页显示多轮声音设计和独立于主模拟器实验的 sound validation，包括 spatiality 与 annoyance。只借鉴过程，不移植其声级、噪声参数或 spatialisation 操纵。
- Nadri et al. (2024)，DOI `10.1080/10447318.2023.2180236`：原 PDF 第 3、5–7 页提供专家工作坊、焦点小组与模拟器评估衔接的设计先例。不是本研究 UAM 三类 cue 的有效性证据。
- `Charting_Final_26` 的 No.9 为 Nadri 2024 综合源，No.20 的 2021 报告是同项目较早报告。保持 26 reports / 25 independent studies 的独立综述边界。
- Basantis 的映射按 `data_charting_final_26_charted.xlsx` → `Charting_Final_26` 第 2 行和 `references.bib` 核对；误用的 `10.1177/1071181321651071` 在第 21 行属于 Nadri 2021。

## 实际验证

审计目录：`temp/20260923-sound-rationale/`。

1. 修改前通过编辑器全选复制保存 `methods.before.tex` 与 `main.before.tex`。线上 Methods 与旧本地仅空行和末尾换行不同；采用线上基线。
2. `final_paragraph_patches.json` 存九处 old/new 与理由，`methods.diff` 保存差异；每处旧段唯一匹配。独立只读审核后，将可辨性表述改为 `are intended to distinguish`，避免把设计目的写成已验证效果。
3. 通过 Overleaf 查找替换逐段写入，全文复制回读为 `methods.after-live.tex`；与 `methods.candidate.tex` 逐字一致，再同步本地 Methods。检查引用 key 均存在、两图保留、所有 `includegraphics` 本地目标存在。
4. 原本本地 `participant-response-en.png` 在 overleaf 根目录，本轮复制到稿件使用的 `overleaf/figures/participant-response-en.png`；保留根目录原文件。本轮没有替换线上图片。
5. 线上编译成功，7 页。面板 **Errors 0 / Warnings 1 / Info 3**：`Command \showhyphens has changed`；三条 Underfull vbox（badness 1484、2073、7012）。
6. 原始日志额外有 `figures/Trial design.pdf` 为 PDF 1.7、当前输出至多允许 1.5 的 inclusion warning；不在面板 warning 数里。未发现 undefined citations/references；不宣称零 warning。原始日志保存在 `compile.raw.log`，界面诊断保存在 `compile-panel.txt` 与 `raw-logs-panel.txt`。
7. 已逐页查看线上 PDF 第 1–7 页，截图 `page-1.png` 至 `page-7.png`。两图完整，正文、公式、引用与图注可见，未发现重叠或裁切。图 1 在当前输出正常呈现；未为消除非阻断 warning 修改模板。
8. `verification.json` 记录检查、编译边界和源文件 SHA-256。没有本地 LaTeX 编译；线上下载操作未产生可确认的新本地 PDF，因此交付以线上 PDF 与逐页截图为准，Downloads 中旧 PDF 不能当作本轮结果。

曾尝试临时测试文字和整文件替换，自动审批分别因污染正文与可能破坏批注锚点而拦截，均未执行。随后使用经审查的九处唯一段落替换完成任务；无待用户批准的写入。任务生成但未采用的早期草稿已删除，只保留最终补丁、基线、回读和验证材料。

## Unity 核对与下一步

本轮只读查看 Unity 编辑器及 README/源码，未修改或运行 Unity。权威工程是 `/Users/trusoegn/GitHub/Audio-VR/Audio-VR`，Unity 6000.6.0f1，`Assets/AudioV0.unity`。

- `IndependentExcerptController.cs` 当前前段为共同直线水平运动；旧 `RoutePlayer` waypoints 被片段驱动路径绕过。改变旧航速不会恢复规划路线。
- 通知前场景与运动必须维护目标不可辨性；路线改进同时需要技术 target-dependency 检查和人类 visual-only 检查。桌面检查不能证明没有视觉猜测。
- 当前生成器三条 non-speech cue 均为 1.35 秒；Tram 认为 T03 过短是口头听感反馈，不能据此改写为已发现 WAV 时长不同。

仍待完成：

1. 修订声音候选与 T01 预告录音；保留 voice/model、文件版本与处理记录。先确定 screening 规则，再做独立人类 screen、耳侧校准与候选选择。
2. 路线可信度、教程实际实现、Quest 输入/可读性/帧时间/播放链和完整数据导出验证。
3. 约两人完整 feasibility pilot。会议“五分钟”没有明确每条件/总暴露口径，不写成固定 trial 或 session 时长；实际记录 exposure 与完整 session 后再决定。
4. N=20 精度论证、最终分析/排除规则与 protocol freeze。小 pilot 不作为统计效果量或主实验可辨性的充分证明。
5. OneDrive 分享、Tram 的伦理材料、时段和房间仍待办理；本轮未发送任何外部消息或邮件。

新增量表没有冻结，本轮未引入 NASA-TLX、UEQ-S 或 trust；当前主指标仍是 first-response accuracy 与正确回答 RT，信心、GO/WAIT 与访谈各自保持既定用途。
