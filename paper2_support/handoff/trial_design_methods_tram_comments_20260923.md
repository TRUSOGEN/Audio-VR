# Trial design、Methods 与 Tram 评论：2026-09-23

## 范围与权威版本

- 线上 Overleaf 项目 `Audio-YegenWu`：https://www.overleaf.com/project/6a799d68583a7077568f0ad5 。本次直接修改线上 `methods.tex`，并为术语一致性小幅修改 `main.tex`。
- 用户已上传 `figures/Trial design.pdf`。线上稿是本次写作权威版本；随后已将线上 `main.tex`、`methods.tex` 同步到本地 `overleaf/`，并把 `Trial design.pdf` 放入 `overleaf/figures/` 以匹配图引用路径。本地保留原有 `overleaf/Figure 1 — Trial design.pdf`。9 月 23 日后续核对确认图中术语已改为 `condition` 和 `trial`。
- 本地未能编译：当前环境未安装 `latexmk`，也未提供 `pdftotext`；线上编译和 PDF 呈现核验见下文。
- 先读 `handoff/independent_excerpt_resolution_and_tram_meeting_20260923.md`，并核对 Overleaf 的 Review panel。用户要求读评论并修改正文，未要求代发评论回复；评论保持原状。

## Tram 评论与已实施改动

| 评论/问题 | 线上改动 |
|---|---|
| `flight excerpt` 是否等于 `flight phase`；`target` 是什么 | 全文 `excerpt` 改为 `trial`；Methods 首段定义 trial 为有一条通知和一次辨识机会的短模拟飞行片段，target 定义为通知传达的正确转变类别。三类等概率独立抽取，可在同一条件重复。 |
| `condition` 与 `block` 用语 | Methods 正文采用四个 condition、每 condition 三个 trial；程序、GO/WAIT 汇总和分析位置用 `condition`。后续更新的图已改为 `1 condition` / `Each condition: 3 separate trials`，与正文术语一致。 |
| 语音由什么软件生成 | Speech 段补充候选音色由 ElevenLabs Voice Design 创建，三条录音由 Eleven Multilingual v2 以同一音色和设置生成。 |
| Unity 版本 | VR 段写作 `Unity 6000.6.0f1`，保留当前仅在 Editor 运行、头显部署和输入检查未完成的边界。 |
| 图示与正文 | Figure 1 引用改为 `figures/Trial design.pdf`，图注与 Description 使用 trial；引言中两处 `flight excerpts` 改为 `trials`。 |

## 编译与呈现核验

- 首次编译因只写 `Trial design.pdf` 未包含 `figures/` 路径，出现 1 error、3 warnings，PDF 为文件名占位。已修正为 `figures/Trial design.pdf` 后重新编译。后续核对又发现 Figure 2 的 `participant-response-en.png` 引用缺少 `figures/` 前缀；已在线和本地都改为 `figures/participant-response-en.png`，再次编译后图片正常显示。
- 最终 Overleaf 编译面板：7 页、Errors 0、Warnings 1（`Command \showhyphens has changed.`，位于 `main.tex`）、Info 2（Underfull `\vbox`）。图文件缺失错误已消失。
- 已目视检查 PDF 第 2 页 Figure 1：更新图嵌入，流程箭头未见重叠；第 3 页语音段和 Unity 段渲染正常；第 4 页 Figure 2 图片已正常嵌入。PDF 文本层核对第 5 页的 trial/condition 用词。没有逐页检查整稿。

## 仍需后续处理

- T01 当前录音文字 `Cruising altitude reached`，需换成预告式文案后再用于正式测试；三条语音的可懂度、头显播放、校准与试听仍待核验。
- 计划样本 20、固定窗口时长、目标次数、头显表现、blind visual-only check 与 precision assessment 仍属现有未关闭 gate；本次仅修正文稿，不把它们表述为已完成。
- 本地同步已完成；后续写作仍以认证的线上稿为准，新同步前先比对差异。

## 2026-09-23 图片路径跟进

- 线上 `methods.tex` 对 Figure 2 的引用曾写成 `participant-response-en.png`，但资源位于 `figures/`，导致缺图。已在线改为 `figures/participant-response-en.png`，并同步到本地 `overleaf/methods.tex`。
- Overleaf 再次编译：7 页、Errors 0、Warnings 1（`\showhyphens` 模板提示）、Info 2（Underfull `\vbox`）。已目视确认第 4 页 Figure 2 恢复显示。
