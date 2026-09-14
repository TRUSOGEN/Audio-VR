# UAM / AV 声音通知与乘客体验：测量证据

更新时间：2026-09-10  
用途：为 AudioV0 论文的 G05（通知识别）、G06（视觉双任务）、G08/G10（主观体验与 VR 可行性）、G12（pilot）提供原始研究证据。  
范围：优先阅读原始实验论文、开放论文或研究机构原始报告；不把综述、产品页面或新闻稿当作效果证据。

## 先给 AudioV0 的结论

1. **主结果应保留行为指标。** 对每个通知记录 `detected/accepted`、三选一 `selected_target`、`correct`、漏答、误答、从声明的通知 onset 到有效回答的 RT，以及刺激与条件的版本。航空警报研究反复使用识别准确率、漏报率和 RT；这些指标比“听起来清楚”更能回答 AudioV0 的 transition understanding/identification 问题。[Stevens et al., 2006](https://www.atsb.gov.au/publications/research-and-analysis-report/2022/aviation/design-and-evaluation-auditory-icons)、[Wenzel, 1986](https://doi.org/10.1177/154193128603001209)、[Ayala et al., 2024](https://doi.org/10.3389/fpsyg.2024.1439401)
2. **视觉任务应保留独立的主任务代价。** 至少输出 visual hit、miss、false alarm、correct rejection 及其分母，并同时输出 auditory identification 的 accuracy/RT；双任务研究显示“听到了但没有及时报告”和“视觉任务本身下降”可能分离。[Pierno et al., 2005](https://doi.org/10.1016/j.apergo.2004.11.002)、[Causse et al., 2022](https://doi.org/10.1016/j.brainres.2022.148035)、[Ayala et al., 2024](https://doi.org/10.3389/fpsyg.2024.1439401)
3. **不要把 Unity 时间戳冒充真实声学 onset。** 研究通常在受控设备中定义声音或事件呈现时刻；AudioV0 应继续区分 `requested/scheduled/playback_observed` 与经回录、声卡或耳机测得的 `measured_onset`，并在论文中注明 RT 的时间锚点。[Adelstein et al., 2022](https://doi.org/10.1016/j.ifacol.2022.10.245)
4. **舒适度、惊吓和接受度要按事件或 block 测量，不能只用末尾一个总分。** UAM 乘客研究使用飞行中 wellbeing、飞行后 sound/wellbeing、information needs、模拟病和接受意向；突发事件研究还使用 HR、瞳孔、眼动扫描和飞行表现。[Papenfuss et al., 2025](https://doi.org/10.1007/s13272-025-00823-4)、[Adelstein et al., 2022](https://doi.org/10.1016/j.ifacol.2022.10.245)、[Kinney & O’Hare, 2020](https://doi.org/10.1177/0018720819854830)
5. **AudioV0 不应宣称“音色导致了体验差异”。** 现有 UAM 乘客研究常同时操纵运动、路线、陪同人员、噪声、视景或情境；要把效果归因于 speech/non-speech 或 quiet/noise，必须冻结其他参数并保留真实 SPL/SNR、刺激版本、音频 onset 与噪声时序。[Adelstein et al., 2022](https://doi.org/10.1016/j.ifacol.2022.10.245)、[Stolz & Laudien, 2022](https://doi.org/10.1109/DASC55683.2022.9925775)

## 本轮重点：客观指标优先级

用户希望更关注客观测量，因此建议把论文的证据层级固定为“行为与设备证据为主，眼动与生理为辅，主观量表用于解释和安全性筛查”。客观指标也不自动等于因果证据；关键是事件触发、传感器时间同步、基线、伪迹处理和预先定义的分析窗口。

### P0：AudioV0 现在就应采集的客观结果

1. **通知识别行为**：每个通知的 `correct/incorrect/timeout`、有效回答 RT、重复输入、response-window 完整性和条件内分布。主结果优先报告 participant-level median RT、IQR 或预先定义的 trimmed mean，同时报告漏答率和错误率；不要只报告 pooled mean。
2. **视觉副任务行为**：每个 stimulus 的 `hit/miss/false_alarm/correct_rejection`、反应时、通知窗口内外表现、被通知打断的刺激数。若每个条件有足够 target/non-target，才探索 signal-detection 指标 `d'` 和 criterion；AudioV0 当前试次数较少时，原始计数和分母应是主结果。
3. **真实呈现时序**：保存 `requested`、`scheduled`、`playback_observed` 和经声卡/耳机回录确认的 `measured_onset`，并计算 `measured_onset - scheduled` 的延迟、抖动和缺失率。Unity 日志时间可以追踪软件流程，但不能替代声学 onset。
4. **设备与实验完整性**：每 trial 保存刺激 hash、耳机/声卡、SPL/SNR 校准信息、音频 clip、帧时间、掉帧、HMD tracking 状态和退出/暂停状态。否则 RT、漏报和不适无法判断是认知效应还是设备失败。

### P1：有设备时优先加入的客观指标

| 指标 | 建议的 trial/block 结果 | 解释边界 |
|---|---|---|
| 眼动与注视 | route/status/cue ROI 的首次注视延迟、dwell time、fixation duration、saccade、blink rate/duration、gaze entropy | 说明视觉注意如何分配；不能单独证明“听懂”或“信任” |
| 瞳孔 | 事件前基线归一化后的 peak、AUC、latency 和恢复时间 | 可作为注意捕获/认知负荷或唤醒的补充；必须控制 luminance、HMD 渲染和头动 |
| 心率与 HRV | 事件前后 HR、ΔHR、RMSSD/频域指标；同时记录呼吸率 | HRV 强烈受呼吸和运动影响，不能脱离呼吸信号解释；Adelstein 的 UAM 模拟器研究提供了 ECG、呼吸、BVP、EDA 等同步采集先例 |
| EDA 与呼吸 | tonic/phasic EDA、SCR 次数/幅度、呼吸率和变异性 | 更适合描述 arousal/负荷变化，不等于 startle 或 annoyance 的单一生理真值 |
| EEG/ERP | auditory oddball 的 P300 amplitude/latency、alpha/beta power、事件漏报对应的 ERP | 适合机制性 pilot；需要硬件触发、伪迹剔除和足够 trial，不宜作为 AudioV0 当前 gate 的必需项 |

### 已找到的客观测量先例

- **UAM 乘客模拟器已经使用生理和反应时数据。** Adelstein 等人在大运动 UAM 模拟器中同时使用 PVT 反应时、ECG/心率、呼吸、BVP、EDA、皮温和加速度；这比单一 comfort 分数更适合支撑“运动/任务负荷是否改变表现”的问题，但其主要操纵是 motion sickness 与 ride quality，不能直接证明通知差异。[Adelstein et al., 2022](https://doi.org/10.1016/j.ifacol.2022.10.245)
- **eVTOL 飞行品质研究已把眼动变成连续客观特征。** Li 等人的 eVTOL handling-qualities 研究在九个 Mission Task Elements 上同步记录飞行轨迹、眼动和主观评分，并分析 pupil size、gaze entropy、iris/pupil ratio 等特征与任务负荷和操纵品质的关系；这适合作为 AudioV0 设计眼动日志字段的技术先例，但研究对象是持证飞行员的操纵任务，不是乘客的音频识别。[Li et al., 2024](https://doi.org/10.3390/aerospace11121020)
- **驾驶舱报警研究支持瞳孔作为 auditory attention 的补充指标。** Sargent 等人的 auditory-alarm pupillometry 实验在阅读或视觉 serial-recall 任务中呈现被忽略的声音，deviant sound 仍能引起可靠的瞳孔扩张；对 AudioV0 而言，它支持记录事件锁定的 pupil response，但不替代正确率和 RT，也不直接证明乘客理解了阶段语义。[Sargent et al., 2018](https://doi.org/10.1016/B978-0-12-811926-6.00079-8)
- **模拟飞行中的负荷研究提醒 HRV 需要呼吸校正。** 模拟飞行任务研究同时记录心率、逐搏血压、呼吸和眨眼；任务难度影响 HR、血压与眨眼持续时间，但 HRV 的解释受到呼吸变化混淆，视觉加工增加时眨眼次数/持续时间下降。AudioV0 若采集 HRV，应同步呼吸并保留 blink/visual-load 变量。[Physiological indices of workload in a simulated flight task](https://doi.org/10.1016/0301-0511(95)05165-1)
- **AAM 真实飞行测试已使用眼动、移动脑成像和心率。** NASA 的 AAM Maturity Level 4 飞行测试报告记录飞行员在自动化系统下的眼动、移动脑成像和心率，用于评估 workload/cognitive engagement；它证明这些指标可以进入航空研究流程，但对象是飞行员和自动化操作，AudioV0 只能借鉴同步和数据治理方法。[NASA/TP–20240007669](https://ntrs.nasa.gov/citations/20240007669)
- **VR dual-task 研究应把客观 performance 作为主结果。** VR 运动研究同时考察 vection、动作/视觉任务表现和舒适度；这支持在 AudioV0 中把视觉副任务 hit/miss/RT 放在主观 comfort 之前，并将 motion sickness 作为安全和解释变量，而不是用一个总分概括全部体验。[Kio & Allison, 2024](https://doi.org/10.1177/10711813241278267)

### 推荐的客观数据结构

每个 participant、block、trial 都应能通过 `event_id` 对齐以下时间轴：

```text
baseline -> measured_audio_onset -> identification_response
         -> visual_stimulus_onset/offset -> visual_response
         -> physiology/eye window -> recovery/block rating
```

建议先冻结三个分析窗口，作为 planned/provisional 设计参数而不是文献事实：事件前约 10–2 s 作为个体基线，事件后 0–5 s 作为通知识别/注意捕获窗口，5–30 s 作为恢复窗口。若刺激间隔不足以支持恢复窗口，应缩短窗口或只分析 trial-level 行为，不能把相邻事件的生理反应混在一起。

当前 AudioV0 最稳妥的论文主分析是：`identification accuracy + measured-onset RT + visual-task cost + measured SPL/SNR + missing/device-failure rate`。眼动、瞳孔、HR/HRV、EDA、呼吸和 EEG 可作为预注册的次级或 pilot 结果；其中 EEG/P300 暂不应成为阻塞 G05/G06 的必需条件。

## 证据矩阵

| 研究 | 设计与样本 | 实际测量 | 对 AudioV0 的适配 | 不能直接迁移 |
|---|---|---|---|---|
| Papenfuss et al., 2025 | 30 人；混合现实固定座舱；熟悉飞行、有人/无人陪同飞行、一次改道非正常情境；每个 simulation block 后问卷与访谈 | 飞行中 1–5 wellbeing 图标量表；飞行后 sound/wellbeing 5 点 Likert；7 项 information-needs 5 点 Likert；飞行态度、飞行恐惧量表；访谈；监测模拟病症状，但没有系统施测模拟病问卷 | G08 可把 wellbeing、sound clarity/annoyance、information adequacy、unexpected-maneuver explanation 作为 block 后记录；G05 可将“是否知道即将发生的阶段/动作”作为理解性补充题；保留逐项原始答案与 missing | 被试是乘客而非飞行员；固定座舱、MR 和预录飞行与 AudioV0 的实时 Unity VR 不同；wellbeing 量表是改编量表且作者明确指出未做心理测量学验证；结果受社会期许、熟悉化和长达 2.5–3.5 h 的流程影响 |
| Adelstein et al., 2022 | 23 人；NASA 六自由度大运动模拟器；两次约 10 min UAM 四旋翼飞行；RPM 控制与 collective 控制，顺序平衡 | 5 min Psychomotor Vigilance Test（PVT，视觉-手动反应时）；Simulator Sickness Questionnaire（SSQ）；NASA-TLX；入组 Motion Sickness Susceptibility Questionnaire-short；ECG/心率、呼吸率、皮温、加速度、BVP、EDA、皮温；UAM 转子噪声通过座舱扬声器/耳机呈现 | 可借鉴“preflight vs inflight”基线、每段 PVT/视觉任务 RT、SSQ、NASA-TLX、HR/呼吸率和 EDA；G06 应保留视觉任务独立窗口，G10/G12 记录模拟病与生理设备状态 | 真实大运动、ISO 加权垂向加速度和 UAM 乘坐 roughness 是主要操纵；PVT 不是 AudioV0 的外围 target 任务；样本较小且被试是 NASA 员工/志愿者；不能把其运动病结果解释为通知设计效果；该研究的“audio”是沉浸与遮蔽外部噪声，不是三阶段语义通知比较 |
| Hanson et al., 2025 | 23 名 NASA 员工志愿者；VR motion-base passenger ride-quality simulator；每人约 15 min；一系列不同 sudden-heave 动作 | 每个动作后 5 点 comfort/rating，并指出哪一个动作不舒服；结束后问卷询问相同运动下真实 air taxi willingness-to-fly；峰值 heave acceleration 与 jerk 作为物理预测量 | 若 AudioV0 要研究突发声音与运动共同作用，可把“事件后不适/愿意继续飞行”作为 G12 pilot 指标；同时保存事件前后 motion/audio 参数 | 该论文的核心操纵是 heave acceleration/jerk，不是声音通知；公开摘要未给出可直接复用的完整量表条目；NASA 员工样本、VR motion-base 与 AudioV0 桌面/普通 HMD 体验不同；不能把 5 点 comfort 直接当作已验证的舒适度量表 |
| Stolz & Laudien, 2022 | UAM 城市场景 VR；flight height、visual density、drone noise 被试内操纵；18 个场景，含 baseline 和 air-taxi 场景 | 每场景 8 个 acceptance 项（comfort、disturbed、observed、computer anxiety、privacy、safety、unpredictability 等），air taxi 另加 noise perception；7 点同意度；是否看见/听见；前后态度与 concerns；重复测量 ANOVA、配对检验 | G08 可把 comfort、disturbed、unpredictable、noise audibility 作为短量表；G07/G12 可记录“听到/未听到”主观可听性并与正式设备校准分开 | 研究的是城市旁观者/社会接受，不是座舱乘客或三选一转场识别；drone noise 的存在、飞行高度和视觉密度混杂；“是否听到”是场景级二元判断，不是声学 onset 或 RT；没有直接证据支持语音/非语音阶段 cue 的可理解性 |
| Birrell et al., 2022 | 20 人；VR urban airport；6 个 customer-journey scenario，Latin Square 安排部分情境 | 每个 scenario 后开放问题、导航容易度、完成容易度（5 点）；全程后 shortened Presence in Virtual Environments：realism、responsiveness、involvement、adjustment（5 点）；开放回答做 inductive content analysis | G08 可借鉴“每情境即时易懂度/完成度 + 全程 presence/realism”；访谈可询问乘客如何理解路线与通知原因；适合验证 AudioV0 的 interaction/experience，而非主效果 | 研究对象是 vertiport 导航，不是飞行中 audio cue；没有 RT、识别正确率、视觉 target performance 或生理测量；开放分析依赖编码框架；不能替代 AudioV0 的 G05/G06 行为 gate |
| Boucher et al., 2024 | 40 人；NASA Langley Exterior Effects Room；136 个 4.6 s UAM rotor-noise stimulus；两个飞行状态；另有与不同 loudness reference 的比较 | 每个声音的 annoyance 标准量表；保持 loudness 时改变 blade-passage frequency、broadband self-noise、tonal motor noise；分析 sharpness、tonality、impulsiveness、roughness 等 psychoacoustic predictors | G07 的音频 manifest 应保留刺激时长、来源、版本、SPL/loudness 与 psychoacoustic 参数；G12 可加入 annoyance、tonality/roughness 的 pilot 记录，帮助解释 noise 条件下的识别失败 | 研究是外部 UAM rotor noise 的烦扰，不是座舱内通知；没有 transition understanding、visual dual-task 或 acceptance/trust；“annoyance”不能替代 comfort、startle 或 workload；AudioV0 的 simulated cabin noise 需要另外校准 |

## 通知识别、反应时与航空警报

### 1. Stevens et al.：auditory icons 与抽象警报

[Design and Evaluation of Auditory Icons as Informative Warning Signals](https://www.atsb.gov.au/publications/research-and-analysis-report/2022/aviation/design-and-evaluation-auditory-icons)（ATSB Research Report, 2006；原始研究报告，稳定链接）比较 iconic 与 abstract warning、单模态与双模态、低/高任务需求。Experiment 1 有 178 名参与者，训练与测试都要求对 9 个 critical events 的警报作答；Experiment 2 在 Advanced Aviation Training Device 中比较 4 个 iconic 与 4 个 abstract auditory warnings。

实际指标包括达到学习标准所需训练次数、recognition accuracy、recognition/reaction time，以及低/高并发任务下的表现。报告称双模态 warning 的识别一致性和准确率最高；高任务需求下，auditory iconic warning 的准确率可接近双模态；在训练设备中反应时约 1 s。这里可直接支持 AudioV0 的三选一识别、练习 criterion、正式 block accuracy、漏答和 RT 设计。

适配建议：把 `communicated_target`、`selected_target`、`correct`、`response_time_ms`、`timeout`、`practice/formal` 分开；练习阶段记录达到 criterion 的次数，正式阶段不显示正误反馈；若 non-speech cue 需要学习，应把“学习后识别”写成研究问题，不宣称 cue 天然可理解。

限制：ATSB 报告的 warning 与事件是飞行员警报语境，且 iconic/abstract 的语义关系、任务需求和设备不等同于 UAM 乘客的三阶段 flight-state cue；约 1 s 不能作为 AudioV0 的 RT 预注册阈值。

### 2. Wenzel：双耳/语义消息与视觉-运动追踪

[Effects of Stimulus Characteristics and Task Demands on Pilots’ Perception of Dichotic Messages](https://doi.org/10.1177/154193128603001209)（1986）在直升机 pursuit 模拟中呈现 dichotic auditory advisory messages，并有/无 concurrent visual-motor pursuit task；操纵 pitch difference、semantic content 和竞争/任务需求。

实际指标是 auditory message 的 percent correct 与正确人工回答的 reaction time。结果显示刺激和任务变量的效应大体可加，增加任务需求通常削弱但不完全消除刺激特征带来的处理优势。

适配建议：在 G06 中同时比较 visual-task load 下的 transition identification accuracy 和 RT；不要只分析平均 RT，应保留错误、漏答和任务条件交互。

限制：被试/任务是飞行员追踪模拟，不是乘客 VR；dichotic listening、语义消息和竞争计分不能直接映射到 AudioV0 的 speech/non-speech × quiet/noise 四条件。

### 3. Wheale：audio、voice 与视觉警告的 RT/主任务代价

[Decrements in performance on a primary task associated with the reaction to voice warning messages](https://doi.org/10.3109/03005368209081471)（1982）要求参与者维持 psychomotor tracking，同时响应 audio、voice 和 visual indicators。

实际指标是警告响应 RT 与 primary tracking task 的 performance decrement。研究摘要报告 audio warning RT 最快，voice 次之，视觉 indicator 最慢；不同 warning 组合在 primary-task decrement 上未能区分。

适配建议：如果 AudioV0 将 speech 与 non-speech 都保留，主结果应同时包含通知识别/RT 和 visual-task performance，不能仅凭 speech 的主观清晰度推断“更安全”或“双任务代价更小”。

限制：1982 年 synthesized voice、警告组合和 tracking task 与现代 HMD、UAM passenger cue、噪声混合不同；摘要没有提供可复用的完整音频参数或量表条目；不能从该研究推断 AudioV0 的具体语音文案最优。

## 视觉双任务、漏报与 workload

### 4. Pierno et al.：VR 中视觉负荷对听觉引导目标获取的影响

[Effects of increasing visual load on aurally and visually guided target acquisition in a virtual environment](https://doi.org/10.1016/j.apergo.2004.11.002)（2005）在虚拟环境中比较 auditory cue、visual cue、audio-visual cue 和 uncued 条件；通过 secondary visual task 增加 visual load。

实际指标是定位 visual target 的 acquisition time 与 errors。结果显示高 visual load 下，auditory cue 引导的目标获取更慢且更易错；audio-visual cue 与 visual cue 的表现更好。

适配建议：AudioV0 的 visual task 不应只记录“有无反应”，应在通知锚定窗口内计算 target acquisition/RT 或等价的 hit/miss/FA/CR；把 notification cue 与视觉任务的时间重叠写入日志。

限制：该研究的 auditory cue 是空间化目标定位提示，不是语音或非语音阶段语义；任务是目标获取而非乘客持续视觉检测；不能把其结果解释为 quiet/noise 的直接效应。

### 5. Ayala et al.：模拟飞行中的 auditory oddball dual-task

[The effects of a dual task on gaze behavior examined during a simulated flight in low-time pilots](https://doi.org/10.3389/fpsyg.2024.1439401)（2024）让低飞行经验参与者完成飞行 circuit、单独 auditory oddball 或二者同时进行；每个条件 3 次，总计 9 个约 10 min trial；dual-task 内又区分 cruise（低负荷）与 landing（高负荷）。

实际指标包括 auditory target-tone response time、accuracy、dual-task cost；飞行任务包括 airspeed/vertical speed variability、landing completion time、landing accuracy；同时记录 gaze behavior；每个 trial 后使用 Situational Awareness Rating Technique（SART）。

适配建议：AudioV0 可把识别 RT/accuracy 与 visual task 的 hit/miss/FA/CR 分开，再计算 condition-specific dual-task cost；G08 若加入主观负荷，应明确是 NASA-TLX 还是 SART，不能把两者合并成未定义的“workload”。

限制：参与者是低时数飞行员，primary task 是操纵飞行，不是乘客视觉任务；auditory oddball 是 tone detection，不是三选一 transition understanding；研究中的“高负荷”由 landing phase 定义，不能直接等同于 AudioV0 的 simulated cabin noise。

### 6. Causse et al.：高精神负荷与 auditory load 下的漏报/P3b

[Busy and confused? High risk of missed alerts in the cockpit: an electrophysiological study](https://doi.org/10.1016/j.brainres.2022.148035)（2022）让 60 名参与者完成模拟航空 landing task，同时做 tone-detection oddball；mental load 为 no/low/high，auditory load 为 1、2、3 种 tone。

实际指标包括 target miss/false-alarm 等行为检测表现，以及 EEG 的 P3b amplitude。研究报告高 mental load 与高 auditory load 使 miss rate 明显增加，P3b 反映两种 load 的变化，且 P3b 与个体 alarm miss rate 反相关。

适配建议：G05 的 timeout/missing 必须是真实分母的一部分；G06 的 visual task 也应输出 false alarm，避免只报正确率；若未来做 EEG，需在 audio onset、视觉刺激 onset、回答和 block 状态处写硬件 trigger marker。

限制：tone oddball 与 AudioV0 的语义识别不同；EEG/P3b 需要实验室硬件与伪迹处理，不能用 Unity event log 替代；论文结果支持“负荷可能增加漏报”，不能直接证明某个 UAM cue 在噪声下会漏报多少。

### 7. Giraudet et al.：P300 与 inattentional deafness

[P300 Event-Related Potential as an Indicator of Inattentional Deafness?](https://doi.org/10.1371/journal.pone.0118556)（2015）采用简化 aviation landing decision task 与 auditory oddball dual-task；16 名年轻男性志愿者，交错 low-load/high-load trial；使用 EEG 记录。

实际指标包括 landing decision accuracy、auditory alarm detection、P300 amplitude；研究还记录了 8 个 EEG 电极、EOG，并以 200 ms 前到 stimulus 后 1000 ms 的 epoch 分析 P300。报警漏报伴随 auditory P300 amplitude 明显降低。

适配建议：把“没有回答”与“回答错误”分开；以后若用 pupil/EEG/生理指标，保存每个通知的精确 stimulus onset、条件、任务负荷和 hit/miss 标签，而不是只保存 block 均值。

限制：样本是年轻男性学生，任务和 tone oddball 都是 cockpit decision surrogate；P300 是神经生理指标，不等于主观理解；不能把其 EEG 结果直接外推到 passenger comfort 或 UAM acceptance。

### 8. Massé et al.：疲劳、负荷与报警检测的 EEG/频带指标

[Classification of Electrophysiological Signatures With Explainable Artificial Intelligence: The Case of Alarm Detection in Flight Simulator](https://doi.org/10.3389/fninf.2022.904301)（2022）在 flight simulator 中进行 auditory oddball alarm detection，并改变 flight-task cognitive load 与 time-on-task；研究目标包括区分 alarm hit 与 omission 的神经信号。

实际指标包括 alarm detection/omission、P300 和 EEG 的 alpha、delta、beta 等频带功率，并用分类器做单试次 hit/miss 区分。论文报告 low-load 下检测更好，SVM 等模型可以高于机会水平区分部分 omission trials。

适配建议：这项研究支持 AudioV0 将 omission 作为独立 outcome，并保留 trial-level event markers；不建议在当前论文中引入“实时脑机检测”作为必要 gate，可把它列为后续机制研究。

限制：分类性能依赖 EEG 预处理、个体化训练和动作伪迹控制；研究是飞行员/模拟飞行语境，不是乘客；模型准确率不是通知可用性或接受度指标。

## 惊吓、突发事件与生理指标

### 9. Kinney & O’Hare：unexpected event 的 HR、瞳孔与扫描变化

[Responding to an Unexpected In-Flight Event: Physiological Arousal, Information Processing, and Performance](https://doi.org/10.1177/0018720819854830)（2020）让 GA pilots 在固定基座飞行模拟器中经历 expected 与 unexpected engine failure。

实际指标包括 heart rate、eye tracking、flight data，以及 unexpected/expected 条件的飞行表现。unexpected event 引起更大的 HR 和 pupil dilation、较少扫描区域，并且表现受损；表现受损不与更高 arousal 简单等同。

适配建议：若 AudioV0 未来测试意外提前/未提前的 cue，可在事件前后记录 HR、瞳孔/眼动、视觉任务遗漏和恢复时间；主观上可单独问 startle、surprise、calmness，而不要将三者合成一个分数。

限制：事件是 engine failure/stall 类异常，不是正常的 cruise/descent/landing-preparation 通知；被试是飞行员；飞行表现和 passenger visual task 的优先级不同；该研究不能给出 AudioV0 的合适声压或 rise-time。

### 10. Thackray：突发高强度声音的 RT、心率和皮肤电

[Correlates of Reaction Time to Startle](https://doi.org/10.1177/001872086500700109)（1965）用高强度 unexpected auditory stimuli 触发 startle，要求参与者快速移动 control stick，并连续记录 heart rate 和 skin resistance。

实际指标是 startle stimulus onset 到 control-stick response 的 latency、heart-rate reactivity 和 skin resistance；自主神经反应与响应延迟相关。

适配建议：AudioV0 的正式通知不应为了制造 startle 而随意使用高强度突发声音；若研究 startle，需单独冻结 SPL、rise time、crest factor、是否预告，并预注册安全停止与不适退出规则。

限制：该实验使用极高强度声音与简单 stick response，和普通 UAM passenger notification 的伦理与生态情境不同；不能把 startle RT 当作 transition identification RT；1965 年设备与测量精度也不能直接等同于现代 HMD/耳机系统。

## 研究设计建议：把论文指标落到 AudioV0 日志

### 最小主结果集

每个 transition instance 至少导出：

| 层级 | 字段/结果 | 解释 |
|---|---|---|
| 通知 | `notification_id`、`transition_id`、`condition_id`、`asset_id/hash`、`notification_anchor`、`evidence_kind` | 识别答案与声音版本可追溯；`evidence_kind` 区分 software scheduled、playback observed、measured acoustic |
| 识别 | `communicated_target`、`selected_target`、`accepted`、`correct`、`timeout`、`rejected_input` | 正确、错误、漏答、输入被拒绝不能混成一个 accuracy |
| 反应时 | `rt_from_measured_onset_ms`、`rt_from_playback_observed_ms`、`rt_from_scheduled_ms` | 若只有软件时间，论文中只能标为 software RT；没有实测 onset 时不填 measured RT |
| 视觉任务 | stimulus-level `target_flag`、`onset`、`response`、`hit/miss/false_alarm/correct_rejection`、`interrupted` | 允许计算通知窗口内和窗口外的视觉任务表现，并处理暂停/中断 |
| 主观 | block 后 clarity、audibility、annoyance、calmness/startle、comfort、workload、trust/usefulness | 版本化题目、锚点、量表方向和 missing code；不要用临时 UI 占位题冒充已验证量表 |
| 设备/生理 | headphone/device、SPL/SNR、calibration date、FPS/frame time、HR/respiration/EDA/pupil/eye tracking（如采集） | 设备测量与 Unity 软件音量分离；未采集字段写 null，不补造 |

### 与当前 Gate 的对应关系

- **G05：transition identification**：首要 gate 是 correct/incorrect/missing、RT、重复输入和 response-window 完整性；Stevens、Wenzel、Ayala 的识别/RT设计可直接作为方法先例。
- **G06：visual secondary task**：首要 gate 是 hit/miss/FA/CR、每个刺激唯一 ID、通知窗口归属和双任务 cost；Pierno、Ayala、Causse 提供了视觉负荷、dual-task 和漏报的依据。
- **G07：四条件资源与噪声**：必须有刺激 hash、设备、SPL/SNR、噪声 onset/offset 和版本；Boucher 与 Stolz/Laudien 提醒“噪声烦扰/可听见”不能替代座舱内 cue audibility。
- **G08/G12：主观体验与 pilot**：每 block 记录 comfort、sound、information adequacy、annoyance、calmness/startle、workload；Papenfuss、Adelstein、Hanson 提供 UAM 乘客/模拟器的测量先例，但都需要明确改编边界。
- **G10：VR/XR**：除帧时间和头部跟踪外，需记录模拟病、退出、视觉舒适度；UAM VR 研究证明沉浸式环境可用于体验研究，但不能把 presence/realism 当作声学或识别效度。

## 不能从现有文献直接写入论文的结论

1. 不能写“speech cue 比 non-speech cue 更快/更安全”，除非 AudioV0 在同一 participant、同一 transition、同一练习与四条件下完成预注册比较。
2. 不能写“quiet/noise 造成了视觉任务下降”，除非背景噪声的 SPL/SNR、频谱、持续时间和实际耳机输出被校准，并且 route、visual load、cue onset 和设备保持可比。
3. 不能写“VR 中的舒适度已被验证”，因为 UAM 研究的 motion-base、MR、静态 VR、座舱、城市旁观者情境不同，且量表版本并不一致。
4. 不能用 P300、HR、EDA、瞳孔或呼吸率替代行为识别结果；这些指标最多提供 attention/arousal/workload 的补充证据，并需要独立设备、时间同步和伪迹处理。
5. 不能把“听到/没听到”的自报当成物理 audibility；它应与 measured SPL/SNR、设备状态和行为检测并列记录。
6. 不能把飞行员 cockpit 警报研究的 RT 阈值、警报强度或 startle 结果直接迁移为 UAM 乘客通知的设计规格；AudioV0 仍需 pilot 冻结 lead time、response window、音频参数和安全停止规则。

## 参考文献入口

- Adelstein, B. D., Toscano, W. B., Espinosa, F. A., & Cowings, P. S. (2022). *Passenger Experience of Simulated Urban Air Mobility Ride Quality: Responses to Large-Scale Motion*. IFAC-PapersOnLine. [DOI](https://doi.org/10.1016/j.ifacol.2022.10.245) · [NASA PDF](https://hsi.arc.nasa.gov/publications/IFAC_HMS_2022_Adelstein_etal_20220916.pdf)
- Papenfuss, A., et al. (2025). *Experiencing urban air mobility: how passengers evaluate a simulated flight with an air taxi*. CEAS Aeronautical Journal. [DOI](https://doi.org/10.1007/s13272-025-00823-4)
- Hanson, C., Ramia, S., & Barnes, K. (2025). *Urban Air Mobility Passenger Discomfort Evaluations of Sudden Heave Motion in a Virtual Reality Motion-Base Simulator*. [DOI](https://doi.org/10.4050/F-0081-2025-0027)
- Stolz, M., & Laudien, T. (2022). *Assessing Social Acceptance of Urban Air Mobility using Virtual Reality*. [DOI](https://doi.org/10.1109/DASC55683.2022.9925775) · [DLR record](https://elib.dlr.de/190177/)
- Birrell, S., Payre, W., Zdanowicz, K., & Herriotts, P. (2022). *Urban air mobility infrastructure design: Using virtual reality to capture user experience within the world’s first urban airport*. [DOI](https://doi.org/10.1016/j.apergo.2022.103843)
- Boucher, M., et al. (2024). *A Psychoacoustic Test for Urban Air Mobility Vehicle Sound Quality*. [DOI](https://doi.org/10.4271/2023-01-1107)
- Stevens, C., Perry, N., Wiggins, M., & Howell, C. (2006). *Design and Evaluation of Auditory Icons as Informative Warning Signals*. [ATSB original report](https://www.atsb.gov.au/publications/research-and-analysis-report/2022/aviation/design-and-evaluation-auditory-icons)
- Wenzel, E. M. (1986). *Effects of Stimulus Characteristics and Task Demands on Pilots’ Perception of Dichotic Messages*. [DOI](https://doi.org/10.1177/154193128603001209)
- Wheale, J. (1982). *Decrements in performance on a primary task associated with the reaction to voice warning messages*. [DOI](https://doi.org/10.3109/03005368209081471)
- Pierno, A. C., Caria, A., Glover, S., & Castiello, U. (2005). *Effects of increasing visual load on aurally and visually guided target acquisition in a virtual environment*. [DOI](https://doi.org/10.1016/j.apergo.2004.11.002)
- Ayala, A., et al. (2024). *The effects of a dual task on gaze behavior examined during a simulated flight in low-time pilots*. [DOI](https://doi.org/10.3389/fpsyg.2024.1439401)
- Causse, M., et al. (2022). *Busy and confused? High risk of missed alerts in the cockpit: an electrophysiological study*. [DOI](https://doi.org/10.1016/j.brainres.2022.148035)
- Giraudet, L., St-Louis, M.-E., Scannella, S., & Causse, M. (2015). *P300 Event-Related Potential as an Indicator of Inattentional Deafness?* [DOI](https://doi.org/10.1371/journal.pone.0118556)
- Massé, E., Bartheye, O., & Fabre, L. (2022). *Classification of Electrophysiological Signatures With Explainable Artificial Intelligence: The Case of Alarm Detection in Flight Simulator*. [DOI](https://doi.org/10.3389/fninf.2022.904301)
- Kinney, L., & O’Hare, D. (2020). *Responding to an Unexpected In-Flight Event: Physiological Arousal, Information Processing, and Performance*. [DOI](https://doi.org/10.1177/0018720819854830)
- Thackray, R. I. (1965). *Correlates of Reaction Time to Startle*. [DOI](https://doi.org/10.1177/001872086500700109)
- Li, Y., et al. (2024). *An Objective Handling Qualities Assessment Framework of Electric Vertical Takeoff and Landing*. Aerospace. [DOI](https://doi.org/10.3390/aerospace11121020)
- Sargent, J., et al. (2018). *Toward an Online Index of the Attentional Response to Auditory Alarms in the Cockpit: Is Pupillary Response Robust Enough?* [DOI](https://doi.org/10.1016/B978-0-12-811926-6.00079-8)
- *Physiological indices of workload in a simulated flight task* (1996). Biological Psychology. [DOI](https://doi.org/10.1016/0301-0511(95)05165-1)
- NASA (2024). *Physiological and Subjective Responses of Pilots during Advanced Air Mobility Flight Testing with Automated Systems*. NASA/TP–20240007669. [NTRS](https://ntrs.nasa.gov/citations/20240007669)
- Kio, O. G., & Allison, R. S. (2024). *Vection and Performance During Attention-Demanding Tasks in Virtual Reality*. [DOI](https://doi.org/10.1177/10711813241278267)
