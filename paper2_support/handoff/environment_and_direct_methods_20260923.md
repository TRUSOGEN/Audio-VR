# Paper #2：环境细化与直接方法表述

日期：2026-09-23，Australia/Sydney。接续 `tram_alignment_and_materials_20260923.md`。

## 权威与本轮目标

用户要求继续细化树、地面、建筑、天空/云/大气、玻璃和器械，并按 Tram 邮件把已经选择的方法直接写定，减少 proposal 表达。

- 论文权威：[线上 Overleaf](https://www.overleaf.com/project/6a799d68583a7077568f0ad5)，主文件 `main.tex`，方法 `methods.tex`；pdfLaTeX / TeX Live 2025。
- Unity 权威：`/Users/trusoegn/GitHub/Audio-VR/Audio-VR`，Unity 6000.6.0f1，URP，`Assets/AudioV0.unity`。未 commit/reset，保留既有未提交改动。
- 原始邮件摘录、改前基线、18 处 patch、diff、完整线上回读和截图：`../temp/environment_and_prose_20260923/`。

## 线上写作修改

Tram 邮件明确要求：已作决定用现在或过去时；每项指标解释 operationalisation；每句简单易懂；完成声音/模拟后补 §2.1、2.2、2.3。本轮继续使用 academic-paper revision 流程及既有 reviewer 建议，依据线上新基线局部替换，无新增引用或实验结果。

| 范围 | 本轮变化 |
| --- | --- |
| Introduction，2 处 | 移除 proposed / will 的设计描述，以现在时说明通知用途、参与者响应及 complete-design comparison |
| Methods，16 处 | Williams 条件顺序、GO/WAIT、练习/四条件程序、筛查和分析描述使用直接表达；Australian-English voice 和 Quest 3 表述为已选设计；20 人写为完成四条件的目标 |
| 待验证部分 | 精度论证、声音筛选、设备与校准仍明确未完成；时长/难度/学习标准等集中到 Pilot，避免各段重复 placeholder |
| 指标 | 保留 accuracy 分母、首次响应、timeout/technical failure、RT 起止、confidence 原题与 1–7 锚点及 GO/WAIT 评分定义 |

“The target is 20 adults who complete all four conditions” 是作者目标，不是已有样本或已证明样本充足；精度评估仍可要求修订事件数或比较范围。现时态描述程序不代表该程序已经在人类受试者中执行。

每个文件的 expected 与完整 online_verified 字节一致；本地 `overleaf/main.tex`、`methods.tex` 已按线上回读同步。SHA256 见 verification.json。本地不是完整可编译镜像，未本地编译；保留同步前本地备份。

线上编译：6 页，Errors 0 / Warnings 1 / Info 0。warning 为 `Command \showhyphens has changed.`；raw log 仍有图示 PDF 1.7 > 输出 1.5 的 inclusion warning。最终逐页检查 1–6 页排版；未见正文溢出、缺字、问号引用或图示裁切。没有为清除模板 warning 进行无关模板修改。

## Unity 已实现与验证

- 树：44 棵，树皮 diffuse/normal/roughness/AO；枝干与分簇实体叶片，去掉实心球树冠。修复街道合批对多 submesh 材质的处理，叶片合批前后均 126,720 triangles。
- 地面/建筑：沿用前阶段沥青、混凝土、草地微表面；18 栋近景楼接入立面表面，部分窗格确定性变色。城市仍为简化几何。
- 天空/云/大气：4K 实拍多云 HDRI、曝光/环境光/薄雾调整；云为静态全景照片，不是体积云。
- 玻璃：5 块窗玻璃和 1 块 HUD 玻璃用 URP/Lit，独立透明排序；HUD 不显示直射高光。
- 器件：通风口金属边框、固定件和控制台分缝，与织物/皮革区分。
- 新增 Poly Haven CC0 树皮四通道及 4K HDRI，共 5 文件，官方 MD5 逐项匹配。来源与派生文件规则已更新 `Assets/ThirdParty/SOURCES.md`。

桌面截图实际检查前视、树木/地面近景、侧舱玻璃和座椅。审计采样 540 draw calls、1,376,751 triangles；前阶段约 503 / 1,191,433，新增几何有成本。不能由该采样推断 Quest 帧率或内存合格。

最终 Editor 已停止 Play、未在编译；Console Error 0 / Warning 0。未运行全量 EditMode tests；本轮检查为导入/编译、运行呈现和网格/材质审计。截图有暂停状态和开发轮廓，只作 `synthetic_visual_review_only`，不是正常航程、HMD 或参与者证据。

源码备份、逐文件 diff、资源 manifest、runtime_audit、verification：Unity `temp/environment_refinement_20260923/`。README 和 Unity handoff 已同步。

## 下一步与保持开放的 gate

1. 识别程序效度：连续固定三阶段仍可能由序号/视觉推断。独立片段只到候选表，未因本轮文字/材质修改变成已实现。按 `docs/identification_validity_gate.md` 先解决结构，再检查无声视觉预测。
2. 声音与装置：T01 advance wording、人类听音筛选、噪声资产/级差及实际播放链校准；完成后补具体参数到 §2.1–2.3。
3. Quest 3：输入/可读性、玻璃反射干扰、帧时间和内存，尤其检查 4K 天空与新增叶片几何。桌面通过不关闭此 gate。
4. 研究冻结：体验构念/标准工具仍需明确；完成精度论证、分析/缺失/排除/多重比较规则、四条件头显 dry run、伦理覆盖下的 2–3 人 feasibility pilot 后冻结。

没有新的 participant/pilot 数据，未发送邮件，没有代替 Tram 批准最终设计。
