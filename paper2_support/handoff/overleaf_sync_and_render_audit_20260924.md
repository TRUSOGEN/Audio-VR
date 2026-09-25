# Paper #2 Overleaf 同步与渲染核验

日期：2026-09-24。范围：Audio Experiment 论文线上稿；承接 [转弯提示、教程与 Overleaf gate](audio_v0_turn_cues_tutorial_overleaf_gate_20260924.md) 中的待上传项。Scoping Review 和 Unity 工程本轮未修改。

## Completed

- 以已登录的 [Audio-YegenWu Overleaf 项目](https://www.overleaf.com/project/6a799d68583a7077568f0ad5) 为基线，核对并逐处替换 `main.tex` 一段、`methods.tex` 两段：共享转弯 pulse/sweep、0.55/1.10 s 技术候选，以及 130/160 m、1.92--2.14 km 的路线审计值。保留 Quest、声音校准和 pilot 未关闭措辞。没有把 23 文件 ZIP 整包覆盖线上项目。
- 两个 `.tex` 在线全文复制回读与本地 `overleaf/` 逐字一致：`main.tex` SHA-256 `4444c34b...0100f55`，`methods.tex` `d37fb221...8ba85d`。
- 将 `figures/full-flight-routes.pdf` 覆盖、`.png` 上传；线上重新下载后两种文件均与本地 SHA-256 一致。实际编译引用 PDF，图 3 显示 130/160 m 与 192.3--206.1 s 路线。
- 对较早 handoff 提到的图件逐一核对：Figure 1 `figures/full-flight-study-design.pdf` 在线与本地已一致。`figures/four-flight-technical-review.pdf` 仍是旧的宽版；修正本地论文版顶部证据标记裁切后覆盖线上，下载回读与本地 SHA-256 `4a30d5c9...8cb01a` 一致。图件只显示 synthetic desktop QA，不是参与者结果。
- 在线以 `main.tex`、pdfLaTeX / TeX Live 2025 重编译：11 页，面板 Errors 0 / Warnings 1 / Info 2。Warning 是 `Command \showhyphens has changed.`，两个 Info 是 Underfull `\vbox`；原始日志另有 Figure 1 PDF 1.7 高于 pdfLaTeX 输出 1.5 的 inclusion warning，图在成品中正常显示。
- 下载最终 PDF 到 [final-online.pdf](../temp/20260924-overleaf-sync/final-online.pdf)，SHA-256 `ed289c63...9f0084c`。逐页检查 1–11 页；技术图更新后 1–9 页渲染与更新前逐像素一致，并再次检查第 10–11 页。图 1、3、4、5、正文和参考文献没有观察到裁切、重叠或缺图。第 11 页为参考文献续页，留白较多；未为追求旧版 10 页而压缩字号或改模板。

完整哈希与核验范围见 [verification.json](../temp/20260924-overleaf-sync/verification.json)。`temp/20260924-final-delivery/overleaf-upload-20260924.zip` 是同步前打包版本，技术图已在本轮修正，后续不得将其作为最新线上快照整包覆盖。

## Pending / Provisional / Pilot-dependent

- **Pending（排版）**：第 11 页只有参考文献续页；若要提交到页数严格受限的模板，再按目标格式做整体排版调整。原始日志的 Figure 1 PDF 版本提示可在图件下一次重导时处理，当前渲染可读。
- **Provisional（设计）**：0.55 s pulse、1.10 s sweep、四条路线时长和 20 人计划样本仍按稿件标注为候选或规划值；本轮仅同步已记录的技术事实。
- **Pilot-dependent（研究）**：Quest 3/HRTF 或 SDK、耳机校准、真人可听性和舒适度、feasibility pilot、伦理、条件×路线冻结、样本精度及正式分析未因本次线上编译而关闭。

下一 gate 仍按前一 handoff：在唯一 Unity Editor 核对最后的 guard 修复并受控 QA，随后重建 Player 和听感检查，再推进 Quest/校准与 pilot。本文档替代前一 handoff 中“Overleaf 尚未上传”的状态，不替代其 Unity gate。
