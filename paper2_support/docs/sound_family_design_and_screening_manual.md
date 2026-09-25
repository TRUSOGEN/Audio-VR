# Paper #2 声音家族设计与筛选手册

## 1. 这份手册解决什么问题

本研究设计一套能让视觉被占用的 UAM 乘客学习、辨认并接受的 routine flight-phase notification。Unity/VR 提供呈现与记录环境；论文的设计对象是声音家族及其与语音通知的比较。2026-09-23 后续用户选择 M1 和四条完整航程；完整航程的主任务按 context-supported 判断解释，不能用其成绩证明声音独立可辨。

目前主研究计划比较两个完整 notification designs：

| 设计因素 | 水平 |
| --- | --- |
| Notification design | 当前选择的 M1 non-speech candidate；固定女声 short spoken messages；听者/设备核验与正式冻结待做 |
| Listening condition | quiet；simulated cabin noise |
| 重复 transition targets | T01 entering cruise；T02 beginning descent；T03 preparing to land |

这是一项 `speech/non-speech x quiet/noise` 的 complete-design comparison。即使正式研究发现差异，也只能说明本研究中冻结的两个完整设计表现不同，不能单独归因给“语音”“音高”“节奏”“空间化”或某个单一声学特征。

## 2. 为什么不能直接生成三个声音

三个随机生成的提示音无法构成可辩护的 sound design process。它会留下四个核心问题：

1. 三个声音为什么属于同一个 family，却又能区分三个 transition？
2. 声音是否在学习后可辨认，还是参与者仅从可预测的飞行画面猜到答案？
3. 声音是否在 cabin-noise 条件下仍舒适、可听且不过度警报化？
4. 研究者为什么选择这一组，而不是另一个 pitch、rhythm 或 timbre 方案？

刺激开发已保留多个完整 family，并记录构造与修订依据。当前用户选择 M1 作为工程及拟 pilot 候选；若开展参与者 screen，应先写明同等训练、记录和判定规则。听者/设备检查及 pilot 反馈支持后续修订和主实验冻结。当前四条件只接入一套 non-speech family，其他候选保留设计历史，不增加主实验因素。

## 3. 当前资产状态

### 3.1 Non-speech candidate bank

位置：`/Users/trusoegn/论文2/sound_design/candidate_families_v1/`

已生成 4 个候选 family，每个含 T01、T02、T03，共 12 条 WAV：

| Family | 共享身份 | T01 | T02 | T03 | 主要筛选风险 |
| --- | --- | --- | --- | --- | --- |
| `contour` | smooth harmonic tone | rising sweep | falling sweep | two-stage rise + late tick | 画面/飞行方向可能使映射过于显然 |
| `interval` | 同一音色与短 pitch motif | ascending interval | descending interval | three-note rise | 用户可能只凭高低方向推断 |
| `rhythm` | 同一 carrier 与共振尾音 | one pulse | two pulses | three pulses | 用户可能只数拍，不形成 transition meaning |
| `brightness` | 同一 fundamental，harmonic balance 改变 | dark | medium | bright + late tick | brightness 可能被理解为 urgency |

所有候选均为 mono、44.1 kHz、16-bit PCM WAV、1.35 s、目标数字 RMS 约 0.150、数字 peak 不高于 0.75。该控制只减少文件格式与数字电平带来的偏差，不等于 headphone SPL、SNR、主观响度或听感相等。

资产入口：

- 试听页：`sound_design/candidate_families_v1/audition.html`。
- 可重复生成：`generate_candidate_families.py`。
- 数字信号审计：`analyze_candidate_families.py` 与 `candidate_analysis.json`。
- 本轮审计已确认 12/12 文件满足格式、时长、RMS 和 peak 门槛。

### 3.2 既有 Unity procedural family

Unity 目前的 runtime generator 也是一套候选，而不是最终答案：T01 为 290--660 Hz 上升 sweep，T02 为 620--210 Hz 下降 sweep，T03 为 260--390--520 Hz 两段上升并含后段 high tick。它主要保留为 technical regression fixture 和 `contour` 思路的实现参照。

### 3.3 Speech design

Speech 采用一个固定的 professional female Australian-English aviation-announcement identity：`Paper2_AU_Female_Broadcast_V3`。下表保留旧 V3 输入文字；9/23 新 V4 候选见 §3.7：

| ID | Text |
| --- | --- |
| T01 | Flight update. Cruising altitude reached. |
| T02 | Flight update. Descent will begin shortly. |
| T03 | Flight update. Prepare for landing. |

目前三条 raw exports 的技术元数据已记录：mono MP3、44.1 kHz、128 kbps；时长分别为 2.351 s、2.508 s、2.116 s。它们已用于 Unity desktop technical mapping，但尚未完成全套人耳试听、HMD playback 或 acoustic calibration。拒绝的 VoxCPM 候选和 0.75-speed T01 只保留为审计记录，不能进入正式刺激。

### 3.4 2026-09-22 预告措辞一致性待修订

旧 V3 T01 “Cruising altitude reached” 表达阶段已达到，与现行 Methods 的 transition 前通知不一致。候选修订为 “Flight update. We will enter cruise shortly.”；9/22 的“尚未生成”状态已由 9/23 V4 原始导出替代（§3.7），尚未人耳核对。最终 lead time 与事件定义仍需统一三条 speech、non-speech learning materials 和日志 target；生成后仍需可懂度、音量与 HMD 校准检查。不能把旧 V3 资产当作已完成的 advance-notification 刺激。

### 3.5 2026-09-23 Tram 口头试听反馈

Tram 在会议中试听候选并指出：T02 下降与 T03 着陆准备不易区分，T03 听起来过短；speech 略快，希望接近常规航班广播的节奏。会议还确认本研究不把 spatialisation 作为独立因素。Tram 初听认为 speech 更清楚，也提醒抽象声音可能随重复学习而变得可辨；这些是设计反馈，不是独立参与者筛选、声学验证或 pilot 结果（会议转写 11:20、15:52–16:13、18:02–19:31）。

会议试听还播放了当前 T01 “Cruising altitude reached”；它表示阶段已到达，与 transition 前通知的时间关系仍不一致。按 §3.4 先修订候选措辞并等待 lead time 决定，不能据此次播放声称 T01 的 advance wording 已验证。Tram 要求在主文或附录交代设计理由、候选筛选标准、家族内清晰区分与协调音色，并以 pilot 反馈修订后再确定正式刺激（会议转写 20:04–22:26）。

### 3.6 2026-09-23 MATLAB 工程候选修订

`sound_design/tram_revision_matlab_v1/` 已由 MATLAB R2024b Update 7 实际生成两套完整 family（M1/M2，各三条新 WAV），并复制旧 C0 的三条原文件作 reference。新文件统一 1.80 s、mono 44.1 kHz PCM16、数字 RMS 约 0.150；T01/T02 peak 0.230，T03 peak 0.393。MATLAB WAV 回读与独立 Python PCM 复核均通过，波形/频谱已检查；这些是数字文件证据，没有执行人耳试听。

两套候选共享 T01 330→495 Hz 上升、T02 495→330 Hz 下降、0.10 二次谐波和柔和包络；T03 改为固定 392 Hz 的两个（M1）或三个（M2）慢速音组。T01/T02 在 M1/M2 逐字节相同，仅比较 T03 节律结构。旧 C0 与新设计同时改变多个参数，不能用该对照单独归因。时长/频率是作者工程选择，文献没有规定这些数值。

本轮用户明确选择 **M1 两脉冲版** 用于当前工程及拟 pilot，取代“仅推荐”的状态；依据是用户偏好、较少重复及与 T02 连续轮廓不同的时间结构。独立辨识/舒适/噪声检查、耳侧校准和正式冻结未完成。两种新 T03 的峰值高于 T01/T02，相同数字 RMS 不代表相同响度。真正 MATLAB 生成的共享标尺波形/FFT/STFT图、英文理由与原文页码在该目录 `paper_figure/`；图只描述构造，不证明 M1 优越或低紧迫性。

### 3.7 2026-09-23 Speech V4 工程候选

`sound_design/elevenlabs_broadcast_v4_candidate/` 已用同一现有 voice/model 重新生成三条 raw MP3，speed UI 从 1.00 改为 0.90（provider filename `sp89`）；T01 输入改为 §3.4 预告文字，T02/T03 不变。其余 stability 0.50、similarity 0.75、style 0、speaker boost on 一致，原文件未 trim/normalize/离线变速。MATLAB 实际解码 V3/V4 共六条，raw hash 未变；V4 三声时长 2.638367 / 2.925714 / 2.351020 s，RMS 0.123554 / 0.114109 / 0.109596，peak 0.536469 / 0.509552 / 0.522461。六条均无解码满幅样本，不能据此排除上游失真。

同目录 `waveform_audit.json` 记录所有实测数字值及 10 ms / −50 dBFS 阈值定义的首尾低能量区间，`audition.html` 提供逐 target 的原文、speed 和 V3/V4 对照。人耳发音/文案检查、自然度、可懂度与校准仍未完成；数字审计不证明“播报已更自然”。T01 同时改变文案与 speed，不作单因素效果归因。Unity 接入状态以工程 manifest 和 runtime 日志为准。

## 4. 设计原则

候选 family 需要同时满足以下原则：

1. **Family coherence**：三条 cue 有可感知的共享 identity，避免像来自三个互不相关的系统。
2. **Transition discriminability**：训练后可稳定区分 T01、T02、T03，而不是只觉得“有提示”。
3. **Routine, not emergency**：表达日常状态变化，避免 siren、alarm、威胁、命令和高紧迫性解释。
4. **Low interruption**：在视觉任务期间可被注意到，但不过分夺取注意或诱发 startle。
5. **Noise robustness**：在同一校准条件下，quiet 和 simulated noise 都能保持可用。
6. **Production reproducibility**：所有最终文件必须有 generator/DAW processing record、版本、checksum、duration、digital measures 与 Unity asset path。
7. **Fair delivery**：speech 与 non-speech 使用同一输出链、同一方向匹配 turn-cue delivery 和同一 masker 规则；turn cue 在四个条件中保持一致，不作为第三因素或独立空间化对比。若改变 spatial delivery，必须对两种 designs 一致调整并重做受影响检查；pilot 仍需检查左右映射、可听性、HRTF/roll-off、舒适度和 masker interaction。

相邻 auditory-display 与 automated-driving 文献支持把 learnability、confusion、annoyance 和 use-case-specific design 作为筛选事项，但不证明任何一个 family 对 UAM 乘客一定更优。以下已按 corpus 原文核对，使用 PDF 文件页码：

| 原论文 | 可借鉴过程 | 本研究使用边界 |
| --- | --- | --- |
| Nadri et al. (2024)，DOI `10.1080/10447318.2023.2180236`，PDF pp. 3, 5–7 | 专家工作坊、用户焦点小组与模拟器评估相衔接，反馈进入声音实现 | Level-4 道路驾驶、多模态条件；支持可追溯的迭代过程，不证明本研究 cue 有效 |
| Kim et al. (2024)，DOI `10.1016/j.trf.2023.11.007`，PDF p. 6 | 多轮声音设计后另做听者验证，检查预期空间方向与烦扰度 | 道路 manoeuvre 场景、空间声及特定播放环境；借鉴独立验证方法，不移植其声学参数，也不把本研究的 shared turn-cue delivery 变成空间化对比 |

其他保留待核查先例为 Ho et al. (2023, DOI `10.1016/j.ijhcs.2023.103044`)、Basantis et al. (2021, DOI `10.1109/THMS.2021.3106892`) 和 2022 年 auditory-warning working-memory study (DOI `10.3389/fpsyg.2022.780657`)；此清单不是本轮完成原文核验的证据，使用前继续检查原文与迁移边界。

## 5. 从候选库到最终 family 的流程

```text
候选家族生成
  -> 数字信号审计与研究者伪影排除
  -> 在参与者 screen 前写定设计理由、筛选规则与 ethics coverage
  -> HMD/headphone 技术检查与校准记录
  -> sound-family formative screen
  -> 锁定供 feasibility pilot 使用的候选版本
  -> Unity 四条件 end-to-end dry run
  -> 约 2 人 feasibility pilot（完整研究流程）
  -> 根据 pilot 反馈修订并复核刺激
  -> 冻结主实验声音版本与 protocol，更新 Methods/register
```

**Pilot-candidate lock** 只标识 pilot 实际试听和运行的刺激版本，便于复现；它不是正式声音选择，也不是主实验冻结。筛选规则须在任何参与者 family screen 前确定，避免看过结果后改标准。约两人的 feasibility pilot 用于发现流程、可懂度与投送问题，不用于统计排名或证明效果。**Main-study freeze** 应在 pilot 后，依据预设筛选规则、pilot 发现、修订记录和最终技术检查共同完成；任何改过的声音须使用新版本并重做受影响检查。

## 6. 阶段 A：研究者试听与技术排除

### 目标

在招募前排除明显的技术问题和不符合研究意图的声音，而不是用研究者偏好选出“最喜欢的”声音。

### 操作

1. 在 `audition.html` 依次播放每个 family 的 T01--T03。
2. 在相同系统音量下记录断裂、click、clipping、过长静音、明显不一致的 timbre 或不自然尾音。
3. 标记任何明显被理解为 emergency、warning、danger、countdown、game reward 或 command 的声音。
4. 保留每个排除决定及其听到的问题，不删除原 WAV。
5. 只把通过技术排除的 family 带到 HMD/headphone 校准与人类 screen。

### 通过条件

每条文件可完整解码；没有可辨识的数字伪影；同 family 三声具有共享身份；没有明显超出 routine flight update 的紧迫性解释。

### 不能据此声称

研究者试听不等于 participant evidence，也不等于在 HMD 或 cabin noise 中可听。

## 7. 阶段 B：HMD/headphone calibration

### 必须固定的链路

- headset 型号和连接模式。
- computer、audio interface、headphone/HMD output device 与 Unity version。
- cabin-noise file、loop point、mix routing 和 notification delivery setting。
- notification level、measurement instrument、measurement position和方法。

### 记录位置

使用 `docs/hmd_audio_calibration_record.md`。记录实际方法、设备、数值、日期、操作者和局限。无法测量的项目必须保留为未完成，不能用 Unity volume 或 WAV RMS 猜测 SPL/SNR。

### 通过条件

在 quiet 与 simulated noise 下，所有保留 candidate 都可听、无明显不适，并有可复现的 level/SNR 记录。

## 8. 阶段 C：sound-family formative screen

### 样本与边界

此阶段应与主样本分开，定位为 formative stimulus-development screen。若涉及新的人员、数据用途或程序，先确认 ethics coverage；不可把没有批准的筛选数据伪装成正式主实验数据。

在首次向参与者呈现候选 family 前，先登记版本化筛选规则、训练方式、记录字段和排除/并列处理；选择规则见本节末的 criteria 清单。9 月 23 日 Tram 的单次试听意见不构成此阶段，也不能代替伦理覆盖或参与者数据。

### 统一 training

若开展独立 family screen，对各候选使用相同学习程序并记录次数、错误和求助，不以不一致的额外训练制造差异。当前 Unity 正式任务前的入门流程为六示例、2次GO/WAIT、1次辨识/信心和1题检查；错误解释后继续，自愿重播。该快速教程是作者选择，不是达到辨识标准的证明，也不要求一直重答至正确。

### 每个 family 的最小记录

| 域 | 必须记录 |
| --- | --- |
| Learnability | training trials、criterion、errors、未达标处理 |
| Identification | post-training T01/T02/T03 response、correctness、response time、confidence |
| Family perception | 三声是否相关、是否容易混淆、预期 transition meaning |
| Experience | audibility、comfort、startle、annoyance、routine appropriateness、distraction |
| Confounds | 是否从视觉 route timing、画面或实验顺序猜到 transition |
| Noise | quiet/noise 下的可听性与可理解性差异 |

### 2026-09-22 顺序线索审计与诊断入口

对已归档完整四条件 technical run 的预先固定序号策略审计得到 12/12 匹配：段内第 1/2/3 次直接回答 T01/T02/T03，不读取声音或参与者回答。它证明该技术序列允许不听声音的满分策略；不证明真人实际采用该策略。最新短运行没有完整三通知 block，报告为不可判定，未用旧日志冒充最新 v8 完整运行。

可重复入口：`tools/audit_notification_predictability.py`；结果在 `temp/identification_validity_20260922/order_audit.json`。因此即使 pilot 没有主动报告“猜顺序”，也不能据此声称主任务消除了顺序线索。

现有候选目录新增 `sound_design/contour_soft_screening_v1/identification.html`，在无地图/航线、无逐题反馈的视图里独立有放回抽取目标；浏览器先校验音频 SHA-256。当前只支持一个 non-speech family 的 quiet researcher QA，不包含 confidence、noise、GO/WAIT 或正式参与者模式。具体运行与数据边界见该目录 README。

该检查可作为声音独立可辨性的后续形成性入口，不能消除完整航程中的固定顺序解释。本轮用户选择完整路线，因此主任务明确限定为context-supported判断；若另需声音独立可辨的结论，须另行随机声音测试及相应分析。旧浏览器QA只绑定其旧素材，不能作为M1已完成人类验证的证据。

### 选择规则

以下规则需在开始 screen 前写成版本化文件：

- 学习是否在可接受训练负担内完成。
- 训练后识别是否避免 floor，且无训练前或路线预测造成的无意义 ceiling。
- quiet/noise 下是否均可听且不造成明显 discomfort。
- 是否没有系统性的 emergency/abnormal interpretation。
- 三个 cue 是否同时具备 relatedness 与 transition discrimination。
- 出现并列时的预先定义 tie-break，而不是事后按研究者喜好决定。

当前尚未设置具体数值阈值。screen 使用的门槛、筛选样本与判定方式须在任何参与者 screen 开始前，结合 ethics coverage 写入版本化规则；不得看过结果再改变门槛。正式主实验的样本量和分析方案则在完整流程 feasibility pilot 后按精度与可行性证据冻结。这两类决策不可混为同一个“声音冻结”。

## 9. Pilot 候选锁定与主实验声音冻结

用户已选择 M1 与现有 V4 作为拟 pilot 刺激；这确定开发方向，不等于听者验证、校准或主实验冻结。pilot 实际采用的文件和设备配置仍须登记，并完成以下记录和检查：

1. 给每个 pilot 候选 WAV 写入 version、family、target、generator/processing history、duration、format、digital peak/RMS 和 SHA-256。
2. 将这三条文件复制到 Unity 的指定 asset location，并在 manifest 中记录 asset path。
3. 建立 explicit `condition -> family -> target -> clip` mapping，禁止只依赖 `S_Q` 或 `NS_N` 等 condition label 推断播放内容。
4. 在同一版本上重新跑 `NS_Q`、`NS_N`、`S_Q`、`S_N` 四条件 technical dry run。
5. 保存 Unity logs、manifest、audition record、calibration record 和生成脚本。
6. 在 pilot record 中登记候选版本、实际完成的听者/设备检查和未决风险；没有做screen时明确记录未做，不补造结果或登记为主实验冻结。

Pilot 后依据预设规则和实际反馈决定保留或修订。若改动声音、trim、level、noise 或 delivery position，必须新建版本并重跑受影响的 screen/校准与四条件 dry run。完成修订和最终设备检查后，才在 `protocol_freeze_record.md` 登记主实验声音版本及其配置；这构成 main-study freeze 的一部分。

## 10. Unity 接入和数据日志

每个 notification event 至少应记录：

- participant/session/block/condition/route/transition ID。
- non-speech family version 或 speech voice/stimulus version。
- selected clip name、checksum 或 immutable stimulus ID。
- notification scheduled/requested/onset/offset time，transition onset time。
- audio playback callback、noise lifecycle、错误状态和 session closure。
- response、response time、timeout/missing、confidence，以及已选 visual task 的数据版本。

桌面 `AudioSource.isPlaying`、Play Mode、event logs 和 0-error compilation 只能说明 software chain；它们不能单独证明耳侧声压、HMD spatial audio、实际听感、舒适度或 participant outcome。

## 11. 与 speech condition 的公平比较

Speech 并不是 non-speech family 的“control beep”，而是另一套完整设计。为避免不必要的偏差：

- 三条 speech 使用同一个 selected voice identity、同一 model/settings、相同输出链和相同 space/delivery rule。
- 文本只表达目标 transition，不加入安全、紧急、情绪或额外解释信息。
- 每条 speech 的时长、loudness、trim、leading/trailing silence 和 checksum 都要记录。
- non-speech 与 speech 分别有不同的学习需求，这属于 complete-design difference；不能事后把它当作纯 timbre 或纯语音效应。
- 若 speech/non-speech 的 duration 不同，应在 Methods、stimulus register 与解释中透明呈现，而不是假设二者等价。

## 12. 什么可以写进论文

### 在冻结前可写

- 研究计划将进行 versioned sound-family development and screening。
- 候选 family 的设计维度和预注册式筛选逻辑。
- non-speech 与 speech 是完整 notification designs，播放链和 calibration 将被记录。
- 当前参数为 planned、provisional 或 pilot-dependent。

### 在冻结后、主实验前可写

- 最终 family 的结构、文件数量、duration、format、processing、training、delivery setting 和 calibration method。
- 筛选流程与为何保留该 family，但不要将 formative screen 夸大成 confirmatory effect evidence。

### 只有主实验后才能写

- 该 family 是否提高 transition identification、降低 visual-task cost、改善 experience，或在 noise 下优于 speech。
- 关于 UAM passengers 的效果估计、统计显著性和可推广结论。

## 13. 当前 checklist

| 工作 | 当前状态 | 下一动作 |
| --- | --- | --- |
| 4 x 3 candidate WAV bank | 已完成，数字审计通过 | 研究者试听并记录伪影/误解 |
| 9/23 MATLAB revision（M1/M2，各三声） | M1已由用户选为工程/拟pilot家族；数字复核和论文声学图完成 | 独立检查 T02/T03 混淆、T03持续感和警报解释；校准待做 |
| Speech V3/V4 raw exports | V4已生成；六条已实际解码、数字审计与独立复算 | 实际发音/自然度、一致性与噪声下试听；校准待做 |
| Candidate-family human screen | 未开始；9/23 Tram 口头试听仅为设计反馈 | 先写 selection rule 并确认 ethics coverage |
| HMD/headphone calibration | 未开始 | 完成 calibration record |
| Pilot candidate selection | M1/V4已选；实际设备配置和ready记录未完成 | 登记候选版本及听者/设备检查，不称final |
| Main-study stimulus freeze | 未冻结 | feasibility pilot 后按记录的规则修订、复核并冻结 |
| Unity explicit asset manifest | 当前完整四路线synthetic QA已完成，12通知/12转场；正式刺激映射未冻结 | 见protocol freeze §0.7及runtime audit；仍需真人/Quest/校准，修订后重跑受影响检查 |
| Feasibility pilot | 未开始 | 先锁定 pilot 候选版本；pilot 后再冻结主实验素材与 protocol |
| Overleaf Methods finalisation | 未完成 | protocol freeze 后更新并重新编译检查 |

## 14. 相关文件

- `sound_design/candidate_families_v1/README.md`：候选库说明与再生指令。
- `sound_design/candidate_families_v1/audition.html`：本地试听页。
- `sound_design/audio_stimulus_design_v1.md`：早期刺激设计与 Unity delivery boundary。
- `sound_design/elevenlabs_broadcast_v3/README.md`：speech voice、文本与 raw export record。
- `sound_design/elevenlabs_broadcast_v4_candidate/README.md`：9/23 新语速/预告候选、实际波形审计及旧新试听对照。
- `docs/hmd_audio_calibration_record.md`：HMD/headphone calibration record。
- `docs/feasibility_pilot_record.md`：可行性 pilot record。
- `docs/protocol_freeze_record.md`：最终 protocol freeze record。
- `handoff/sound_family_candidate_bank_20260919.md`：本轮候选库交接与验证边界。
- `跟tram开会记录/文字/tram会议 9.23.txt`：自动转写；声音设计与试听反馈见 11:20、15:52–16:13、19:20–22:26，原始语义明确的部分用于 §3.5。
- `/Users/trusoegn/论文项目/Final/final_corpus_26/data_charting_final_26_charted.xlsx`：`Charting_Final_26` 的 Excel row 10（Nadri 2024）与 row 6（Kim 2024）；对应 PDF 在同目录 `pdfs/`，分别为 *Sonification Use Cases in Highly Automated Vehicles*（PDF pp. 3, 5–7）与 *How Manoeuvre Information via Auditory (Spatial and Beep) and Visual UI Can Enhance Trust and Acceptance in Automated Driving*（PDF p. 6）。Nadri 2021 是同项目早期报告，不计为独立重复证据。
