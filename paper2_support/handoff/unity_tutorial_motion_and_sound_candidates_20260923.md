# Paper #2：Unity 教程、飞行运动与声音候选交接

日期：2026-09-23。范围：Audio Experiment；状态：本轮授权的工程修复、声音生成、软件验证与线上 Methods 同步完成，正式研究未冻结。

## 接续与有效来源

- 上一轮 [声音设计论证交接](sound_design_rationale_revision_20260923.md) 完成九处 Methods 修订，当时未修改 Unity/声音。本轮接续实现其工程项；历史状态不再作为当前待办。
- 原始反馈：[Tram 9.23 会议](../跟tram开会记录/文字/tram会议%209.23.txt)；[执行方案](../docs/plans/20260923-tram-meeting-execution.md) 已更新两轮进度。
- Unity 权威工程：`/Users/trusoegn/GitHub/Audio-VR/Audio-VR`，Unity `6000.6.0f1`，场景 `Assets/AudioV0.unity`；现有 dirty tree 保留，没有提交或回退用户修改。
- 写作权威：[Overleaf Audio-YegenWu](https://www.overleaf.com/project/6a799d68583a7077568f0ad5)。主文件 `main.tex`，实际编译器 pdfLaTeX / TeX Live 2025；本地 `overleaf/methods.tex` 与本轮线上回读一致。
- 2×2 speech/non-speech × quiet/noise 保持；2026-09-24 后方向匹配的左/右转提示音纳入四条件，作为 shared journey delivery，不是第三因素或 T01--T03 评分通知。Scoping Review 的语料、分母和数据未修改。

## 问题、修复与版本

| 问题 | 本轮实际改变 | 证据边界 |
| --- | --- | --- |
| 片段绕过旧waypoints，位置直行且heading固定 | `excerpt_curved_climb_v2_candidate`：共同轻缓弯曲爬升，平面heading随解析速度；固定响应窗后3秒平滑分支为巡航、下降、着陆准备减速 | 技术运动候选；未验证航空动力学或HMD舒适度 |
| 重置跳变及遮挡期间积累miss | 首尾遮挡与减速尾段，默认片段31秒；GO/WAIT与tracking分别保留operator/boundary暂停原因 | 参数provisional；XR遮挡平面仍须Quest检查 |
| 直接进入任务，没有足够入门引导 | Welcome→六声示范→四次GO/WAIT→九次识别/信心→三题理解检查→Ready→显式启动四条件；支持重播/重试/暂停/退出 | 全部练习标为practice，次数不等于已验证学习标准 |
| 教程选项、结尾文字被裁切，结尾残留待答提示 | 固定控件尺寸、选项字体；理解检查逐题截图；休息/完成提示显式分行且隐藏standby | 实际桌面渲染已查看；后续仅UI呈现验证不冒充完整运行 |
| T02/T03混淆、T03感知偏短 | MATLAB生成M1两脉冲、M2三脉冲；T01上升轮廓/T02下降轮廓共享，三声统一1.80秒；旧C0字节保留 | M1是优先技术接入候选，没有人类优劣结论 |
| speech偏快，T01旧词为完成状态 | 同ElevenLabs voice/model重新生成V4，speed UI 0.90，T01改为“We will enter cruise shortly.” | 速度设置不等于实测语速下降10%；实际发音/自然度待听者检查 |
| 当前运动事件被exporter漏掉 | 接受旧flight_motion_sample与新excerpt_motion_sample，旧列不变、追加身份/heading/fade等字段；练习不进入正式派生表 | 缺值留空，不补造暂停后的transition；全部QA数据仍qa_only |

声音runtime版本：`matlab_m1_two_pulse_v1_candidate` 与 `elevenlabs_broadcast_v4_candidate_speed090`。教程事件为 `tutorial_*`、`event_scope=practice`、condition `PRACTICE`；未走正式 NotificationAudio/Identification 记录。

## 声音设计过程与交付

1. 需求来自9.23明确口头反馈；Kim/Nadri仅支持迭代设计与独立筛选过程。频率、谐波、包络、时长、pulse数量是作者工程选择。
2. [MATLAB候选说明](../sound_design/tram_revision_matlab_v1/README.md) 保存生成器、参数、WAV、hash、数字审计与波形/频谱。MATLAB R2024b Update 7实际执行成功；Python独立PCM复核9/9。六条新声为mono/44.1 kHz/PCM16/1.80 s，RMS约0.15；相同数字RMS不代表同主观响度。
3. [V4语音生产说明](../sound_design/elevenlabs_broadcast_v4_candidate/README.md) 保存现有voice ID、Multilingual v2、共同设置、输入文案、raw MP3及hash。实际三次生成使用121个既有credits，无购买。原MP3未trim、normalize或离线变速。
4. MATLAB实际解码V3/V4六条语音；独立数字复算6/6。V4时长T01/T02/T03为2.638367/2.925714/2.351020秒。生成与审计日志各有两条非阻断Java package warning；初次MATLAB错误修复后重跑，失败日志保留。
5. Unity导入mapping/hash/格式及基础AudioSource检查通过；方向提示音的左右 source position/spatial blend 仍需 HMD/HRTF 校准和 pilot 检查。教程六声有实际 `AudioSource.isPlaying` 观察。没有将其解释为耳侧声压或内容已由听者确认。

试听入口（已在Chrome保留，服务为本机8092）：

- [C0、M1、M2非语音对照](http://127.0.0.1:8092/tram_revision_matlab_v1/audition.html)，持久文件 [audition.html](../sound_design/tram_revision_matlab_v1/audition.html)。
- [V3/V4语音对照](http://127.0.0.1:8092/elevenlabs_broadcast_v4_candidate/audition.html)，持久文件 [audition.html](../sound_design/elevenlabs_broadcast_v4_candidate/audition.html)。

本轮没有主观试听结论。下一步按[声音设计与筛选手册](../docs/sound_family_design_and_screening_manual.md)记录研究者试听、统一播放链、quiet/noise、耳侧校准和独立筛选，再锁定pilot候选。

## 实际软件验证

Unity证据目录：`/Users/trusoegn/GitHub/Audio-VR/Audio-VR/temp/20260923-route-sound-tutorial/`。

- 已强制import/compile后运行：AudioV0定向85/85；全EditMode 974项，972通过、2跳过、0失败。`all_tests.json`保存结果。全量之后还加入净空Editor审计、终场fade、QA时序与UI布局修复；这些后续更改分别经编译、实际运行或呈现检查，没有重跑全量测试。
- 完整正常时钟synthetic流程：`meeting_verification.json`为complete，四条件完成。原日志为 `AudioV0_Data/technical_demo/session_20260923_091836_262_5649c23a709e45e0954ffec7c8d06c70/event_log.jsonl`。
- 教程50事件、9识别+9信心；教程期间0正式识别。实验流程12片段、12通知、8识别+8信心、3timeout、1故意pause-invalid。只有11次transition onset；暂停片段 `block_03_excerpt_02` 不重放、不补齐。
- 运动1,725样本；实时Physics场景862建筑colliders、3targets×156时点（每0.2秒）、24米探针半径未发现碰撞。是采样净空检查，不是连续或真实飞行安全证明。
- `tutorial-only/meeting_verification.json` 的09:33:59 UTC快照停Ready且未自动启动。该日志随后09:37:26明确启动过并产生3通知，09:38:58退出，不能将整份日志称作纯教程。
- 最新 `presentation-final/compile_status.json`：09:45:12 UTC，0 errors/10条既有CS0618 warnings（InteractionAudit、SurfaceMaterialTools、RuntimeScenario）。旧 `compile_final.json` 是更早的14条反馈/8个唯一diagnostics快照，不作为最新编译计数。
- `presentation-final/` 真实渲染休息/完成/还原Welcome画面，全文清晰且standby关闭；`presentation_audit.json`明确synthetic_presentation=true、full_sequence_run=false。该probe不写区块或序列完成事件。
- 最后核验Unity处于Play、未暂停、Tutorial Welcome、Session Ready、sequence未启动；用户可点击BEGIN PRACTICE。自动QA已结束，状态可能随用户随后操作变化。

导出证据：[runtime_snapshot_verification.json](../temp/excerpt_motion_export_20260923/runtime_snapshot_verification.json)。4项focused+14项已有Python回归通过；2,209原始事件与快照hash相同，1,725运动行全部逐字段匹配，12通知行保留，四个GO/WAIT汇总核对通过。去掉50教程事件后，通知及GO/WAIT派生结果保持一致（比较内存数据结构，未重导所有CSV做字节比较）；所有行仍为qa_only。未出现参与者数据。

## 线上论文五处change map与核验

| 段落 | 已同步事实 |
| --- | --- |
| §2.1 non-speech | 四个初始families与新MATLAB M1/M2的关系、supervisor feedback来源、learned mapping及未验证边界 |
| §2.1 speech | V4共同voice/model、speed0.90、三句文案与T01未来预告，听者检查仍open |
| §2.3 VR | 共同弯曲爬升/heading、固定时窗后分支、fade；受控片段及非验证动力学边界 |
| §2.4 practice | 完整练习后Ready，显式启动四条件 |
| §2.4 trials | 识别/信心期间视觉任务继续，边界遮挡时暂停隐藏机会 |

[审计目录](../temp/20260923-unity-sound-sync/) 保存线上基线、五处唯一段落patches、candidate、线上全文回读、diff、verification、编译原始日志及1–8页截图。回读与candidate和本地Methods逐字一致；Study Participants整段、main、图件和既有批注未改，未accept/reject/resolve任何批注。

线上编译为8页，Errors 0 / Warnings 1 / Info 2。warning为 `Command \showhyphens has changed`；两条Underfull vbox为1484/2073。原始日志另有 `Trial design.pdf` 的PDF1.7/输出1.5 inclusion warning，无undefined citations/references。已实际逐页查看1–8页，图、正文、公式与文献未见裁切或重叠；第8页为最后两条参考文献，未压缩字号/间距。没有本地LaTeX编译，本轮不以旧Downloads PDF作为新结果。

## 未关闭项与下一步

1. 人类试听与内容核对，特别是T02/T03混淆、T03持续感、alarm解释、V4发音及播报节奏；按同样训练筛选并记录保留/拒绝原因。
2. Quest3全流程、输入、文字可读性、帧时间、XR遮挡、播放链及耳侧声级/噪声校准；桌面渲染不能代替。
3. [识别效度gate](../docs/identification_validity_gate.md) 的盲化visual-only与片段可信度检查；不存在人类“无视觉泄露”证据。
4. 伦理材料、房间/时段与完整约两人feasibility pilot；记录真实exposure和session时长，培训次数/31秒/声音参数均未冻结。
5. N=20精度论证、最终分析与排除规则、[protocol freeze](../docs/protocol_freeze_record.md)；保持formalStudyMode=false，未进入participant_run。
6. OneDrive、联系Tram与预约没有执行；数据中心的元数据透视与图表是计划中的后续功能，本轮只修复CSV数据链。
