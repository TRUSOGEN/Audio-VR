# 快速教程、四段完整航程、M1 与可下载数据图交接

日期：2026-09-23。当前任务来自用户对上一轮教程、路线、声音论证、数据查看器和论文语言的五项修订，另追加要求每张图及对应清洗数据均可下载。

## 当前有效入口

- Unity：`/Users/trusoegn/GitHub/Audio-VR/Audio-VR`，Unity 6000.6.0f1，`Assets/AudioV0.unity`。
- 线上论文：[Audio-YegenWu](https://www.overleaf.com/project/6a799d68583a7077568f0ad5)，主文件 `main.tex`，方法在 `methods.tex`。
- 本地论文：`/Users/trusoegn/论文2/overleaf/`，本轮源文件与线上回读逐字一致。
- 数据查看器：[http://127.0.0.1:8091/](http://127.0.0.1:8091/)，原有 `tools/打开 Audio V0 数据中心.command` 可重启。
- 当前协议：`docs/protocol_freeze_record.md` §0.7；识别范围：`docs/identification_validity_gate.md`；本次执行：`docs/plans/20260923-tram-meeting-execution.md`。

本记录取代上一份 `unity_tutorial_motion_and_sound_candidates_20260923.md` 对默认教程和路线的描述；旧的独立片段及其 QA 保留为历史/显式技术选项。

## 回顾与纠正

上一轮为控制视觉和顺序线索，默认使用独立空中片段，并将教学做成多次练习和必须答对才能继续。这偏离了用户要快速进入四段完整旅程的目标，也没有充分权衡体验成本。

本轮按用户决定恢复完整飞行。自然阶段顺序、路线图及窗外运动可能支持预测，因此论文改为比较飞行情境中的识别（context-supported identification）。不再宣称默认实验实现了 sound-only discrimination 或排除了视觉/顺序帮助。2×2 因素仍为 non-speech/speech × quiet/noise；用户在 2026-09-24 决定把方向匹配的左右转提示音纳入四条件，作为四条件共享的 journey delivery，不构成第三因素，也不计入 T01--T03 的 12 个识别机会。正式流程需记录其 source side、spatial blend、HRTF/roll-off 和 pilot 可用性检查。

## 已实现与验证

### 1. 教程不再错误循环

- `participant_tutorial_quick_v2_candidate`：六个带含义标签的声音示例自动播放/推进；一次 GO、一次 WAIT；一次非语音识别与信心；一道理解题。
- 错答解释后继续；可自愿重听。到 Ready 后仍须明确按 `START FOUR FLIGHTS`，避免自动进入实验。
- 练习与正式数据在同一原始日志中有不同标签，练习不进入实验派生统计。
- 正常时钟、真实按钮 QA 故意漏 GO、点击 WAIT、答错识别和理解题，仍约 32.1 s 到 Ready；这是脚本完成时间，不能写成人类教学时间或理解达标。
- 完整飞行会话内：两个问题各只有一次错误回答，随后 Ready；从 tutorial_started 到 tutorial_ready 的日志间隔为 30.444 s。32.1 s 包括验证器外围流程。
- 最后源码同步后再跑教程：32.064 s；真实明确开始后确认正式副标题正确恢复，实际相机截图已检查。此后续会话只做 UI 检查，不是第二次四航程验证。

证据：Unity `temp/20260923-quick-tutorial/runtime-full-flight-session/` 为原教程证据；`runtime/` 为最终 UI 教程副本；`temp/20260923-full-route-revision/post_sync/source_sync_audit.json` 为最终检查。

### 2. 四条地面到地面的完整路线

- 当前默认 `useIndependentExcerptsCandidate=false`；采用 `FullFlightRouteGeometry` → `FlightMotionProfile` → `RoutePlayer`。
- 四条宽曲线路径包含 ground departure、climb、cruise、left/right turns、descent、landing preparation、touchdown。
- 模型路线约 2.05–2.16 km，根坐标 y=1.92 m → 180 m → 1.92 m；根坐标不是起落架高度，落地间隙另由机体几何检查。
- 航程地图显示路径、当前位置和终点；没有条件代码或累计成绩。
- 完整正常时钟 QA 共 1,112.533 s（18 min 32.5 s，含短暂停与切换）；四段日志 route time 为 282.419 / 282.419 / 272.178 / 272.181 s，合计 1,109.197 s。运动模型计划合计为 1,109.259604 s，不混称精确实测。
- 真实场景 collider 净空检查：4,018 采样点、24 m 半径、间距 ≤4 m，0 碰撞点、0 几何修复。
- 12 个通知与 12 个转场齐全；软件提前量 17.995–18.001 s。
- T03 对齐 landing_preparation 语义 waypoint；默认 8 s 识别 +8 s 信心 +2 s 余量。这些均是暂定工程参数。
- 实际通知间暂停 2.149 s：位置和 route clock 完全不变，恢复后继续。未在这次完整运行中实测响应窗口内暂停；其 interruption 行为由源码与既有定向测试支持，不扩大证据范围。

完整原始日志：
`/Users/trusoegn/AudioV0_Data/technical_demo/session_20260923_104134_834_74b3946cd7be4de8b919baa6941f9499/event_log.jsonl`

对应 Unity 证据目录 `temp/20260923-full-route-revision/`：
- `runtime_audit.json`、`event_log.snapshot.jsonl`：7,634 事件、5,485 motion、4 段完成、12 通知/转场、11 回答/信心、1 故意错误、1 timeout、0 应用 error。
- `geometry_audit.json`、`actual_route_geometry.csv`、`pause_probe.json`：几何、计时和暂停证据。
- `route_02_*.png`：同一航程六阶段原始截图；论文六图由这些实际相机截图排版，没有生成虚拟结果。
- 第一段早期截图含旧教学副标题，论文未采用；第二段及之后为修正版。永久源码修复已再次实际验证。

### 3. M1 声音与 MATLAB 图

用户明确选 M1 作为当前工程及拟 pilot 候选。M2、C0 留作设计记录。

- T01：330→495 Hz 连续上升；T02：495→330 Hz 连续下降；T03：392 Hz 两个分离音组。
- 三声均 1.80 s、44.1 kHz、PCM16、mono，基音加幅度比例 0.10 的第二谐波，whole-file digital RMS 约 0.15。
- 上/下方向有 Nadri 2024 的相邻车辆声音构造先例；两脉冲对应 Tram 9.23 11:20 对旧 T02/T03 难区分的反馈。Kim 2024 支持迭代与独立检查流程。
- 具体频率、时长、谐波权重和选择 M1 属作者设计决定；文献不证明这些值最优、两脉冲更有效、相同响度或低紧迫性。
- Edworthy 1991 本轮只核验出版者摘要（全文 403），仅支持参数可能影响 perceived urgency 的概括提醒。
- MATLAB R2024b 实际生成三行×三列图：波形、FFT、STFT；统一幅度尺度，不做逐文件 peak normalization。独立 NumPy 复算通过，九条旧 WAV hashes 不变。

详细依据、原文页码、MATLAB 参数和数值检查：
`sound_design/tram_revision_matlab_v1/paper_figure/design_rationale.md`。

### 4. 数据查看器与逐图下载

六张独立图分别提供 SVG、PNG、PDF、Cleaned CSV、Excluded CSV：accuracy、correct RT、confidence、GO/WAIT、horizontal trajectory、vertical position。高度图另有通知 marker events CSV；组合图和汇总 CSV 继续可用。

- 单会话审阅，按 evidence category、完成状态、condition、block、route 筛选；不同证据类别不合并。
- 所有下载与图件共用同一不可变快照，防止运行中的新日志造成图表/CSV 分母漂移。
- Cleaned CSV 是图实际采用的逐通知、逐视觉机会或逐运动采样数据，保留 ID、时间、条件和有效性；Excluded CSV 保留未进入该指标的记录与原因。
- 误答和 timeout 保留于 accuracy；correct RT 只含正确且被接受的回答；缺失不补 0。原始 JSONL 不修改。
- 本次四航程：accuracy 12 行；RT 10 行、排除 2；confidence 11 行、缺失 1；GO/WAIT 220 行、4 interrupted；trajectory/altitude 各 5,485 行。
- 本轮脚本未模拟 GO 点击，因此 GO hit=0 只是 QA 控制行为。
- 19 项聚合/HTTP 测试通过；实际浏览器点击下载六份 Cleaned CSV，逐字节匹配快照；另核验四份 SVG、两份 PNG、独立 accuracy PDF、RT excluded CSV、12 行通知 markers。
- 两个主图页面均有实际浏览器截图并目视检查；Event audit 页尚未完成最终实点补查（浏览器连接中断），其 API 与静态路径已检查。

正式字段说明见 `docs/participant_runbook_and_data_dictionary.md` §4.1；实现与下载证据见 `temp/20260923-viewer-revision/README.md`。

### 5. 论文正文与五组图已线上写入

基于当前线上源建立备份，完成 18 个逐段/块替换，另按独立审核做三处短句修正；保留无关内容及评论，不整文件覆盖线上稿。

| 改动 | 论文位置/资产 |
|---|---|
| 完整飞行、12 通知、情境推断边界 | Introduction、Method、Analysis、Pilot |
| 六示例、简短练习、错答可继续 | Study Procedure |
| M1 构造、选择过程及证据边界 | Notification Designs |
| 2×2 与完整航程流程 | `figures/full-flight-study-design.pdf` |
| 三声波形、FFT、STFT | `figures/m1-waveform-fft-stft.pdf` |
| 四路径平面/高度剖面 | `figures/full-flight-routes.pdf` |
| 同一航程六阶段实际截图 | `figures/full-flight-unity-views.png` |
| 四条件技术数据图 | `figures/four-flight-technical-review.pdf` |

最终线上编译：`main.tex`，pdfLaTeX / TeX Live 2025，10 页，Errors 0、Warnings 1、Info 1。warning 为既有 `Command \showhyphens has changed.`；Info 为 page 3 的 `Underfull \vbox`。五组资产均在实际编译日志中出现，引用 key 无缺失。源文件与线上最后回读一致。

独立 Agent 只读核验了 M1、路线图、截图来源、计时、暂停源码、Williams 候选顺序及正文推断边界；没有发现 P0/P1，已采纳 practice “tagged separately and excluded from experimental summaries”的 P2 修正。

**逐页 PDF 检查已完成基线轮**：恢复后从 Overleaf 下载当时的 10 页线上编译 PDF，并检查每页。第 3 页 Underfull vbox 没有造成可见截断；第 9 页合成图缩到栏宽后标签偏小，已重排为 5.95 英寸论文版、普通文字 7.5 pt 并本地目视核查；第 8 页样本句已在本地改为计划招募。Chrome 扩展随后再次失联，因此这两个修订尚未上传线上，最终稿的重编译与逐页复查仍未完成。不要将基线下载版称作最终稿。

论文证据目录：`temp/20260923-full-flight-paper/`，含线上 before/after、最终 diff、编译 DOM/raw log、`final_audit.json`。不要将目录内早期 proposed 或旧下载 PDF 当作最终渲染结果。

## 最终工程状态与剩余事项

- 定向 Unity `AudioV0.Tests`：92/92 passed；本轮未把旧 974 项全回归宣称为新结果。
- 最后强制编译：0 errors、8 条既有 Editor API deprecation warnings；新 QA 代码自身相关警告已修复。
- Unity 最后恢复 Play / Welcome、SessionReady、sequence 未开始、route time 0、地面位置；没有后台自动跑四航程。
- 声音的人类区分/紧迫性检查、Quest 输入/可读性/舒适性、耳侧校准、真人 pilot、样本精度、正式路线×条件分配及冻结仍未完成。
- 最小后续：恢复浏览器 → 下载当前 10 页 PDF → 逐页检查尤其 p3 空白与 p4/p6/p9 图件缩版 → 必要时做最小排版修正并重新编译 → 更新本记录和 final_audit。
- 当前无邮件发送、共享权限修改或对外承诺；Scoping Review 语料与分母未改。
