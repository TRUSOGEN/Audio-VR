# 固定顺序修复、英文 UI、论文与 Tram 会议

日期：2026-09-23，Australia/Sydney。接续 `environment_and_direct_methods_20260923.md`，本文件是本轮最终交接。

## 当前结果与权威

- 线上论文：<https://www.overleaf.com/project/6a799d68583a7077568f0ad5>，主文件 `main.tex`，方法 `methods.tex`；pdfLaTeX / TeX Live 2025。
- Unity：`/Users/trusoegn/GitHub/Audio-VR/Audio-VR`，Unity 6000.6.0f1，`Assets/AudioV0.unity`。保留全部既有改动；未 commit/reset。
- 论文审计、补丁、回读、图示、编译诊断、测试汇总：`../temp/excerpt_resolution_20260923/`。
- Unity 源码备份、运动核验、真实英文截图、异常路径测试日志：Unity `temp/excerpt_resolution_20260923/`。

## 1. 固定顺序问题：已改实现，验证范围明确

已选并实现 `independent_excerpts_v1`，替代本次运行的固定T01→T02→T03连续路线。四条件、每条件三个独立片段，每片段一条通知。每次目标以独立私有 RNG 三等概率有放回抽取，允许相邻、同条件乃至全部重复；不承诺每人/每条件覆盖全部目标。旧预生成平衡候选表不是当前运行时抽样器。

答题前使用相同城市、水平前向运动；不显示地图、进度、阶段文字或目标代码。目标随机流与GO/WAIT随机流分开。参与者被告知每段独立且目标可能重复，不能依据“每类只出现一次”排除答案。

时序为固定识别窗、固定信心窗、余量、后段运动示意。信心在识别截止后开放，不由首次回答触发；只有已回答者看到信心题。目标后段由固定时钟触发，不受快答、错答、timeout或缺信心影响。T01后段保持巡航，只是示意，不是真实爬升到巡航过渡。

当前开发设置：2 s边界、4 s前段、8 s识别、8 s信心、2 s余量、5 s后段；软件通知lead约18 s。这些值未pilot冻结，较长lead是否可信必须检查。

暂停使当前片段无效，恢复后跳过，不重播或替补。退出/故障/主动结束记录invalid和excerpt_end，停止当前片段。技术无效从准确率分母排除；正常timeout仍计不成功。退出者既往完整片段是否保留，另依同意/撤回政策执行。

专用 `visualOnlyValidation` 模式抑制通知内容并使用统一预测提示，仅支持独立线索检查；不是主研究第五条件。人类盲测的人数、可容忍线索优势及不确定性规则仍须预设。

## 2. UI、环境与截图

当前 `passenger_excerpt_v9`，默认英文；取消地图，保留静态简短说明、GO/WAIT活动区、独立三选一区、暂停按钮。修复信心题第二行被截断，1/7端点独立显示。回答与信心各有真实引擎截图，未拼接状态或生成虚构UI。

- `participant_response_en.png`：论文使用的响应状态。
- `participant_confidence_en.png`：会议展示与量表端点检查。
- 论文上传名 `participant-response-en.png`，保留1920×1080原始渲染。

树皮/叶簇、草地/混凝土/沥青、楼体表面、HDRI云天空、薄雾、玻璃和金属通风口的本轮材料工作见前一handoff。现在是更有表面层次的简化城市，不是高保真机舱/真实UAM数字孪生；静态HDRI云不是体积云。尚未证明Quest性能。

## 3. 实际验证

| 检查 | 结果 | 不能推出 |
| --- | --- | --- |
| 前段位置函数 | 10,001时点 × 3目标相同 | 所有渲染帧无任何可感知线索 |
| 正常速度四条件synthetic运行 | 12片段独立导出，11完整、1暂停无效；有效片段固定lead约18s | 参与者表现或学习/效度 |
| Unity异常路径定向测试 | 5/5通过：通知前后退出、回答后故障、截止拒绝、固定信心版本 | 真实设备断音、XR挂起全部覆盖 |
| Unity全量EditMode | 948项，946 passed，2 skipped，0 failed | Quest性能/校准/舒适性 |
| 导出回归 | 7检查通过，涵盖截断、转场缺失、退出、故障、中断无invalid、正常、旧完整日志 | 人类数据质量已经验证 |
| 最后一处UI端点修复 | 编译与真实英文截图通过；未因此重跑全量测试 | 头显中可读 |
| 最终Editor | 已停止Play，Console 0 error/0 warning | 独立设备部署通过 |

独立reviewer最初发现4项必修问题，现均复核关闭；详见 `reviewer_final.md`。首轮四条件记录早于中止路径与信心版本补丁；后续针对性测试覆盖新增分支，不能把首轮记录称为最终版本四条件完整重跑。

## 4. 论文受控修订与编译

以线上完整基线应用28处确定性补丁；Introduction两处，Methods程序/时序/图示/指标/分析及历史注释其余。简洁现在时描述已选设计；不用proposal条件句回避已作决定。声音/人类/HMD/精度真实缺口集中说明，无新增参与者结果或引用。

- Figure 1更新为四条件与单独片段时序，明确目标可重复、固定后段示意。
- Figure 2新增Unity英文实机截图，caption明确synthetic desktop demonstration及HMD边界。
- 识别准确率、有效分母、timeout、首次正确响应RT、信心题和1–7锚点、GO/WAIT分母均明确。
- 访谈移除旧route display提示；体验来自访谈，confidence不等同trust或总体experience。
- T01当前录音仍是“Cruising altitude reached”，明确需改为预告；不是已完成刺激冻结。
- 随机目标计数的波动纳入20人精度评估；不能保证均衡而假定交互有足够功效。

两个tex线上完整回读与expected逐字一致；随后同步本地镜像并保留同步前备份。Overleaf替换空框曾保留旧文字，7处残留已清除，最终全文件比对一致。

线上输出7页，Errors 0 / Warnings 1 / Info 2。warning为模板 `Command \\showhyphens has changed.`；Info为两条underfull vbox（1803、2073）。新PDF流程图未见旧PDF1.7 inclusion warning。页面截图归档page_1…page_7.png；逐页检查无正文溢出、缺图、乱码或问号引用。保留正常分页留白。没有声称本地LaTeX编译。

## 5. 明天会议：建议顺序

先开Overleaf Figure 1说明研究结构，随后Figure 2或Unity截图说明参与者操作；再展示测量定义，最后谈待验证项。不要从材质或测试数量开始汇报。

### 可直接说的英文（约90秒）

I revised the paper around your three comments: stating decisions directly, defining each measure, and using simpler language.

I also found a confound in the original simulation. The fixed cruise–descent–landing sequence allowed participants to predict the answer without identifying the sound. I replaced the continuous flight with independent excerpts. Each excerpt has one notification, targets can repeat, and the scene and motion are the same before the response windows close. The later phase illustration starts at a fixed time, independent of the participant’s answer.

The primary outcomes are first-response accuracy and correct-response time. Timeouts count as unsuccessful identification; technical interruptions are recorded separately. Identification confidence uses one seven-point item, while the final interview addresses the notification experience. The silent GO/WAIT activity continues during the responses.

I added a procedure diagram and an English screenshot to explain the task. The desktop checks pass, including repeated targets and interruption handling. This removes the deterministic sequence rule, but I still need a blind visual-only check, headset checks, final sound screening and calibration, and a precision assessment for the target of twenty participants.

The main trade-off is control versus realism: these are short independent excerpts, not complete flights. I would like to confirm that this scope fits our research questions before freezing the timings and event count.

### 请Tram判断的三个具体问题

1. **控制与生态效度**：是否接受独立片段作为主比较，明确研究“统一场景下通知识别”，而非完整航程体验；T01仅巡航示意的限制是否需要进一步调整？
2. **体验构念**：当前访谈是否足以回答RQ1的experience部分；如果需要标准量表，先确定具体构念和用途，再选工具，不把confidence当trust。
3. **精度与负担**：在20个完成样本目标下，是否优先增加每条件事件数/缩小交互主张；是否保留每次信心题，以及它导致的通知lead。用最终设计的精度评估决定，不以12×20个事件当作240个独立样本。

### 被问“现在可以招募了吗”

Not yet. The software structure is implemented, but the human cue check, headset and audio checks, and the precision rationale are not complete. I am treating these as the remaining steps before recruitment, rather than as completed validation.

## 6. 下一步执行顺序

1. 和Tram确认上述研究范围/体验构念；冻结视觉线索检查的容忍界限、样本/事件及缺失规则。
2. 替换T01语音，完成人类声音family筛选；在最终耳机/Quest链验证声学onset、通知/噪声级与可懂度。
3. 盲化visual-only检查和片段可信度检查；不凭“不显著高于1/3”判定没有线索。
4. 按最终事件数和缺失情景完成20人精度评估及分析/多重比较规范；不充分则调整设计或结论范围。
5. Quest输入/可读性/帧时间和内存实测、最终版本四条件dry run、伦理覆盖下2–3人feasibility pilot；再填写freeze record。

本轮没有人类参与者数据、HMD测量或正式研究结果；未发送邮件或代替Tram批准研究。
