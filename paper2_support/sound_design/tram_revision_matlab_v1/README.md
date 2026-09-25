# 9.23 Tram 反馈后的 MATLAB 声音候选

这是可重复生成的**工程候选**；2026-09-23 后续反馈中用户明确选择 **M1 两脉冲版**，作为当前 Unity 使用和后续拟 pilot 的声音家族。该选择是作者设计决定，没有完成独立听者验证、耳侧校准或主实验冻结。M2 和旧 `C0` 原文件完整保留作设计历史。自动生成和数字审计没有证明 M1 更可辨、更舒适或更像常规机舱通知。

## 要求、依据与构造

| 设计要求 | 来源或工程判断 | 本轮实现 | 仍需验证 |
| --- | --- | --- | --- |
| T02 与 T03 有不同结构 | Tram 9.23 11:20：下降/着陆准备难区分 | T02 连续下降轮廓；T03 固定音高的两个或三个分离音组 | 同样训练后是否减少两类混淆 |
| 不让 T03 只靠短促尾音表意 | 同段反馈 T03 感知偏短；旧三文件实际均为 1.35 s | 三声统一为 1.80 s；T03 的结构覆盖前后部分 | 主观持续感、是否拖沓、实际任务打断 |
| 保持同一家族身份 | Tram 21:31：协调、可区分、不警报化 | 全部正弦基音加 0.10 倍二次谐波；共享数字 RMS、格式与平滑边缘 | coherence、routine appropriateness、startle/annoyance |
| 避免把 urgency 作为编码目标 | 本研究 routine phase-transition 定位 | 无 siren、无快速重复、无高频 late tick；M1 使用较少 pulse | 这些工程约束不保证听者不会将其当作 alarm |
| 能说明为什么比较这些候选 | Kim/Nadri 迭代设计先例；详见声音手册 §4.1 | 两个完整 family 的 T01/T02 完全一致，仅比较 T03 两组/三组节律 | 记录每个保留/拒绝决定及证据 |

文献支持“有依据地构造、独立筛选、按反馈修订”的过程；没有规定下列频率、时长、谐波权重或 RMS。这些数值是本轮作者工程选择，仍可被试听/pilot 修改。与旧 C0 相比同时改变了时长、音色和映射，不能把后续差异归因为某一个特征。

## 三个候选包

| 包 | T01 entering cruise | T02 beginning descent | T03 preparing to land | 用途 |
| --- | --- | --- | --- | --- |
| `reference_C0_1p35s` | 旧上升轮廓 | 旧下降轮廓 | 旧两段上升及 late tick | 原 WAV 字节复制，历史对照 |
| `M1_two_pulse` | 330→495 Hz 平滑上升后短持音 | 495→330 Hz 平滑下降后短持音 | 392 Hz 两个柔和音组 | **本轮用户选择，用于当前技术版本及拟 pilot**；独立听者验证未做 |
| `M2_three_pulse` | 与 M1 相同 | 与 M1 相同 | 392 Hz 三个柔和音组 | 对照候选；检查多一组是否利于辨识或增加警报感 |

选择 M1 的设计理由是较少的重复、与连续 T02 不同的时间结构，以及用户本轮偏好；**没有听者效果优劣的证据**。M2 保留为历史对照，当前不将两个 family 同时加入四条件。拟 pilot 版本仍须播放链和学习检查，并记录实际使用的版本。此处不决定研究 response window、通知提前量或 headset 音量。

## 参数与可控范围

- 新文件：mono、44,100 Hz、PCM16、1.80 s；目标数字 RMS 0.150，数字 peak 上限 0.75。
- 新 T01/T02：前 1.50 s 用 smoothstep 改变基频，之后短持音；前后各 0.18 s 半正弦平方 fade。
- 新 T03：固定 392 Hz，音组为正弦平方包络；M1 起点 0.09 / 0.99 s、每组 0.72 s；M2 起点 0.06 / 0.66 / 1.26 s、每组 0.48 s。两者总音组支持时间均为 1.44 s，时间分布不同。
- 全部新声：二次谐波为精确 2× 基频、权重 0.10，不含旧 2.01× 轻微失谐、加性低频 accent 或 T03 的 1040 Hz late tick。
- RMS 以整个文件计算。文件时长相等不等于感知时长；另记录包含 95% 能量的时间区间，仅供数字描述。
- 相同数字 RMS 不代表相同主观响度、耳侧 SPL、masker SNR 或听觉可懂度；正式投送仍使用现有校准记录。

## 重现与审计

```sh
/Applications/MATLAB_R2024b.app/bin/matlab -batch "addpath('/Users/trusoegn/论文2/sound_design/tram_revision_matlab_v1'); generate_tram_revision" -logfile '/Users/trusoegn/论文2/temp/sound_matlab_20260923/matlab_generation.log'
```

生成器只调用 base MATLAB 的合成、WAV、FFT、绘图功能，不使用随机性、AI 音频或外部声音版权资产，也不会播放声音。WAV 回读断言检查格式、时长、RMS、峰值、削波、DC、零端点；T01/T02 的 M1/M2 文件必须 SHA-256 相同，reference 必须与旧文件 SHA-256 相同。

- `candidate_analysis.json` / `.csv`：实际 WAV 的审计值、checksum、MATLAB 版本、生成时间和生成器 checksum。
- `*_audit.png`：实际 WAV 的波形、完整 FFT 与短时频谱；统一时间/幅度/频率坐标。
- `audition.html`：标签明确的研究者试听入口，无自动播放、无参与者数据上传。
- `../../temp/sound_matlab_20260923/`：实际运行日志与独立格式/数字检查。

实际执行已完成：MATLAB `24.2.0.3070828 (R2024b) Update 7`，生成/审计 exit 0；Python 标准库独立 PCM 复核 9/9 通过。六条新文件 RMS 为 0.15000001–0.15000009，T01/T02 peak 0.229980、T03 peak 0.392700，没有削波，首尾样本为零。生成日志含两条非阻断 Java `sun.awt.X11` package warning；第一次运行的空 struct 初始化错误已修复并完整重跑，原失败日志保留。

两套新 family 的波形/频谱已实际查看，连续轮廓与分离音组符合代码规定的结构。尚未播放试听或操作 HMD；不据图推断舒适、alarm 感或辨识效果。需要重新生成试听页/独立检查时运行 `python3 build_audition_and_verify.py`。

## M1 论文图与论证

`paper_figure/m1-waveform-fft-stft.pdf` 和同名 600 dpi PNG 为真正 MATLAB 绘制的 3×3 组合图：每行一个 target，每列分别为实际 WAV 波形、全文件 Hann FFT 和短时频谱。共享坐标和数字参考值保留文件之间的峰值差异；没有逐文件或逐帧最大值归一化。全文件 FFT 描述有限时长信号的频率内容，不是感知响度。

复现命令（只读九条既有 WAV，不重新合成）：

```sh
/Applications/MATLAB_R2024b.app/bin/matlab -batch "addpath('/Users/trusoegn/论文2/sound_design/tram_revision_matlab_v1'); plot_m1_paper_figure" -logfile '/Users/trusoegn/论文2/temp/20260923-m1-paper-figures/matlab_plot.log'
```

`paper_figure/figure_audit.json` 记录输入/输出 checksum、MATLAB 版本、窗口/步长/标尺和旧 WAV 未修改检查；`spectral_source_data.mat` 保存实际绘图数据；`paper_figure/design_rationale.md` 给出原文页码、允许主张、英文正文和图注。`paper_figure/figure_insert.tex` 是可直接使用的 LaTeX 插图片段，图件副本位于 `../../overleaf/figures/`。图的结论限于上升/下降轮廓、两脉冲时间结构和共享谐波构造；不证明辨识、舒适或低紧迫性。

## 下一步顺序

1. 研究者按相同播放链试听 reference、M1、M2，参照已有 `../contour_soft_screening_v1/researcher_audition_record.md` 的字段，在筛选记录中标明本次新 candidate ID；不要把代理生成日志当试听结果。
2. 记录断裂/噪声/音色不一致、T02/T03 混淆、T03 持续感、常规或警报解释、学习负担；在 quiet 与实际 masker 下复核。
3. 在参与者筛选前写定学习标准、覆盖三 target 的规则、同样训练程序、技术无效处理、保留与并列规则，并确认伦理覆盖。
4. 按 `../../docs/sound_family_design_and_screening_manual.md` 完成播放链/校准和形成性筛选；锁定 pilot 候选而非主实验 final。
5. Unity 四条件技术 dry run 后，再做完整 feasibility pilot；声音修改必须新版本与重新审计，最后冻结正式刺激。
