# M1 论文图、设计理由与证据边界

日期：2026-09-23。本轮用户明确偏好 M1，选为当前工程版本和拟 pilot 声音家族；这是作者设计决定，独立听者验证、耳侧校准和正式刺激冻结仍未完成。

## 图的职责

图件为 3×3 quantitative grid，三行对应 T01/T02/T03，三列对应实际 WAV 波形、全文件 FFT、STFT。它展示导出资产如何实现连续上升、连续下降和两个分离音组，以及相同的基音/二次谐波构造。它不展示参与者结果，不证明声音更可辨、低紧迫性、相同响度或真实耳侧声压级。

论文图使用英文、190×164 mm、Arial 8 pt、PDF 和 600 dpi PNG；PDF 的文字和曲线为矢量，短时频谱为栅格图层。颜色只编码短时数字幅度，不编码效果优劣。波形使用原始样本，并叠加 5 ms 极值包络以保证论文缩版时可见。

## 为什么是这三种声音

| 选择 | 当前可解释的理由 | 依据类型与边界 |
| --- | --- | --- |
| T01 连续上升，T02 连续下降 | 用相反的音高变化构成可教的两个标识；T02 的下降方向可作为下降事件的学习线索 | 作者映射设计；Nadri 的升/降音高是相邻道路自动化的构造先例，不证明本 UAM 映射直观或无需学习 |
| T03 固定音高的两脉冲 | Tram 指出旧 T02/T03 难区分，因此让 T03 在时间结构上区别于连续的 T02 | 9.23 导师口头反馈引导修订，不是正式 confusion study；减少混淆是待检验目的 |
| M1 两脉冲，保留 M2 三脉冲作历史对照 | 本轮用户选择 M1；两脉冲已经提供间隔结构，使用较少重复 | 作者选择及工程简洁性；没有两脉冲优于三脉冲的行为证据 |
| 同一基音+0.10 二次谐波、平滑包络 | 为三声提供相同合成构造和连续边缘；使差异主要可描述为 contour/timing | 共享构造可由生成器核验；是否听为同一家族、是否柔和仍是听者判断 |
| 330–495 Hz、392 Hz、1.80 s、数字 RMS 0.15 | 使候选在统一格式和明确数字尺度下可重现 | 全部为作者工程参数，没有论文规定这些值最优；相同 RMS 不是响度匹配 |

不将形容词“柔和”“常规”“低紧迫性”作为已验证效果。Edworthy 的作用是提醒设计者：频率、谐波、包络和时间结构可能改变 perceived urgency，因而须对选定组合进行听者检查。

## 已核验原文与允许主张

1. **Kim, van Egmond, and Happee (2024)**，*How manoeuvre information via auditory (spatial and beep) and visual UI can enhance trust and acceptance in automated driving*，DOI `10.1016/j.trf.2023.11.007`，PDF p.6 / 印刷 p.27，§2.5.2.3。
   - 原文短语：`Sounds were designed over multiple iterations`；验证与 simulator evaluation 分开进行，检查 perceived spatiality 和 annoyance。
   - 本研究借鉴迭代与独立检查流程，不移植其空间化、音量、噪声值或结果。
   - 同页 Fig.5 显示 notification beep 的 spectrogram，为报告声学构造的直接出版先例；本文三联图仍是本研究自己的表达选择。
2. **Nadri et al. (2024)**，*Sonification Use Cases in Highly Automated Vehicles: Designing and Evaluating Use Cases in Level 4 Automation*，DOI `10.1080/10447318.2023.2180236`。
   - PDF p.3 / 印刷 p.3123，§2–3：专家工作坊、focus-group interviews、simulator study；专家产出 storyboard、design rationale 和 sound prototype。
   - PDF p.5 / 印刷 p.3125，§4.1.2：参加者可反复听取各 speech/non-speech 样例、评分并讨论。
   - PDF p.6 / 印刷 p.3126，§5.1.4：`Selection was based on perceived recognizability and user feedback during the FGI`；焦点小组反馈进入实现。
   - PDF p.7 / 印刷 p.3127，Table 3：automation level increase/decrease 用三音上升/下降 piano notes，battery 用升/降旋律。只支持使用变化方向作学习编码的先例，不支持本研究的具体频率、脉冲数、UAM 理解或有效性。
   - 2021 会议文是同项目早期报告，本论证不将它再算一次独立证据。
3. **Edworthy, Loxley, and Dennis (1991)**，*Improving Auditory Warning Design: Relationship Between Warning Sound Parameters and Perceived Urgency*，DOI `10.1177/001872089103300206`。
   - 本轮 Sage 全文页面返回 403；没有本地全文，因此不声称全文页码核验。
   - 已从 Crossref 的出版者元数据核验作者摘要：fundamental frequency、harmonic series、amplitude envelope、delayed harmonics，以及 speed、rhythm、pitch range、melodic structure 影响 perceived urgency。
   - 只保留这一概括性提醒，不据摘要声称 330/495/392 Hz、0.10 谐波权重或两脉冲必然低紧迫性；本轮新增正文不需要对其结果作更细节的推断。
4. **Tram 9.23 导师会议**，11:20：旧 T02/T03 区别不清，T03 主观偏短；20:04–21:31：须交代设计过程、选择理由、共享家族、可区分及不过度警报化。
   - 这是 supervisor design feedback。旧三个 WAV 实际时长均为 1.35 s，不能把主观偏短写成某文件更短。
   - 内部逐字稿有转写误差，只采用上下文清楚的反馈，不作参与者数据或公开文献引用。

原 PDF 的 checksum、逐页文本和 Edworthy 访问状态在 `../../../temp/20260923-m1-paper-figures/`；没有修改 Scoping Review 的语料或计数。

## 建议英文正文

以下是可并入现有 non-speech 段落的增量文字；不要与已有开发流程重复叠加：

> The selected development candidate, M1, uses three learned sound patterns (Figure~\ref{fig:m1-acoustics}). Entering cruise is represented by a rising contour (330--495 Hz), beginning descent by a falling contour (495--330 Hz), and preparing to land by two separated pulses at 392 Hz. The pulse pattern gives landing preparation a different temporal structure from the continuous descent cue, addressing confusion raised during supervisor listening feedback. All three sounds share the same synthesis structure: a sinusoidal fundamental, a second harmonic at 0.10 of its amplitude, and smooth amplitude envelopes. Their duration is 1.80 s and their whole-file digital RMS is approximately 0.15. These values are engineering choices; equal digital RMS does not establish equal perceived loudness. M1 was selected as the current development candidate, while the three-pulse alternative is retained in the design record. Listener checks and headset calibration remain necessary before the main-study stimuli are frozen.

来源与过程可用现有段落，或将其中重复句压缩为：

> Opposite pitch directions have been used to represent increases and decreases in automated-vehicle states \cite{nadri2024sonification}; here, their flight-transition meanings are taught explicitly. Iterative development and separate sound validation in automated driving \cite{kim2024manoeuvre}, together with user feedback used to select and refine sonifications \cite{nadri2024sonification}, inform the development process. These studies provide methodological precedents rather than evidence for the effectiveness of the present UAM cues. Because acoustic and temporal parameters can affect perceived urgency \cite{edworthy1991urgency}, the selected family will be checked for learned identification, confusion, coherence, comfort, and suitability for routine announcements.

## 图注与分析约定

完整 LaTeX 在 `figure_insert.tex`。关键约定：

- 全文件 FFT：79,380-sample symmetric Hann；131,072-point FFT；单边幅度除以 `sum(window)`，仅内部频率 bins 乘 2，DC/Nyquist 不加倍。
- STFT：2,048-sample symmetric Hann（46.44 ms）；hop 256（5.80 ms）；4,096-point FFT；相同 coherent-gain amplitude 约定。
- 两者用 `20 log10(amplitude / 1)`，1 是 PCM 解码的 full-scale sample amplitude；不是 SPL。窗口改变信号权重，因此全文件 FFT 与短时 FFT 不作峰值大小的听感比较。
- 时间 0–1.8 s；显示频率 0–1,200 Hz；波形范围 −0.45–0.45；FFT −100–0 dB；STFT 共用 −80–−5 dB。显示区间覆盖基频和二次谐波，未将更高频低幅能量解释为不存在。
- 零填充只加密频谱采样，不提高原始时间窗决定的频率分辨率；未进行逐文件/逐帧 peak normalization。
- 波形图可见 T03 峰值约 0.393，高于 T01/T02 约 0.230；相同整个文件 RMS 不会消除此差异。

## 验证记录

MATLAB `24.2.0.3070828 (R2024b) Update 7` 实际执行成功，exit 0。最终 PNG 九个 panel 已逐一查看：文字、坐标、共享色阶完整，无裁切或覆盖；T01/T02 相反方向和 T03 两音组清楚可见。PDF 为单页 1.4，实际约 190.15×164.04 mm；PNG 为 4488×3874 px（600 dpi 名义导出）。PDF 文字可提取，包含九个 panel 的标识；最终在论文中的缩放与排版由 Overleaf 插入后再检查。

独立 Python/NumPy 从 PCM16 原始字节重新计算 RMS、peak、whole-file FFT 和 STFT 最大幅度，三声所有值与 MATLAB 在 `1e-9` 内一致；另核验上升/下降趋势和两段音组。M1、M2、C0 九条旧 WAV 的 SHA-256 均与原 `candidate_analysis.json` 一致。验证结果保存在 `../../../temp/20260923-m1-paper-figures/independent_verification.json`，可运行同目录 `verify_figure.py` 重现。

图件已按内容相同的副本放入本地 `overleaf/figures/`，未在本子任务中修改线上或论文正文。MATLAB 启动输出两条非阻断 Java package warning；首轮空 struct 赋值错误已修复，失败日志保留；随后修正 Retina 导出物理尺寸并重跑，最终验证通过。
