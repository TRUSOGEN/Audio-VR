# HMD Audio and Calibration Record

Date: 2026-09-18
Status: apparatus partly identified on 2026-09-21; acoustic measurement pending. Completing this record does not by itself approve recruitment, freeze the apparatus, or establish participant audibility.

## 1. Record identity

| Field | Entry |
| --- | --- |
| Calibration record ID | `CAL_YYYYMMDD_XX` |
| Date, local time and venue | |
| Operator initials | |
| Related Unity build / commit | |
| Scene and runtime profile | `Assets/AudioV0.unity` / |
| Intended run category | `technical_demo` / `pilot` / `participant_run` |
| Related run IDs | |

## 2. Apparatus and route

| Component | Make/model or version | Setting / serial / note |
| --- | --- | --- |
| HMD and operating system | Meta Quest 3; OS not yet recorded | User confirmed model on 2026-09-21 |
| Connection mode | `PCVR` / `standalone` | |
| Computer and GPU | | |
| Unity player / OpenXR version | | |
| Controller mapping | left tracking / right identification | |
| Audio output path | built-in speakers / headphones / interface | |
| Headphones | | |
| Volume control location and value | | |
| Seating position / head height / recentering procedure | | |
| Route profile | speed, lead time, route IDs | |
| Spatial delivery configuration | source position, spatial blend, HRTF/SDK, roll-off | |

## 3. Stimulus and masker provenance

| Material | Version / checksum | Playback role | Verification |
| --- | --- | --- | --- |
| T01 speech | `09cddda0...6261e13` | notification | file present / audition pending |
| T02 speech | `faa43a45...c583a2` | notification | file present / audition pending |
| T03 speech | `a8350a46...1c9950a` | notification | file present / audition pending |
| T01--T03 non-speech | `v1-runtime` | notification | export/checksum pending |
| Cabin-noise file | | added masker only | |
| Quiet condition | same apparatus, no added masker | control delivery | |

Record full checksums in the frozen stimulus register rather than relying on abbreviated values above.

## 4. Measurement method

| Field | Entry |
| --- | --- |
| Measurement instrument, class and calibration date | |
| Coupler / ear simulator / microphone and placement | |
| Weighting, time constant and metric | |
| Test signal and representative clip section | |
| Repetitions and channel procedure | |
| Room noise / test environment | |
| Notification level, quiet (measured value and unit) | |
| Masker level, noise condition (measured value and unit) | |
| Notification-to-masker SNR (calculation and unit) | |
| Peak/limiter check | |
| Operator-observed distortion, dropouts or channel imbalance | |

Do not enter estimated SPL/SNR values. If an at-ear measurement is not feasible with the chosen apparatus, record the limitation, the alternative method, and the exact level-control setting; keep SPL/SNR unfrozen.

## 5. HMD functional acceptance

| Check | Pass / fail / not tested | Evidence path or observation | Action if failed |
| --- | --- | --- | --- |
| Seated origin, horizon and recentering stable | | | |
| Participant UI readable and operator UI absent | | | |
| Left tracking and right identification operate concurrently | | | |
| T01--T03 speech audible in quiet | | | |
| T01--T03 speech audible with masker | | | |
| T01--T03 non-speech distinguishable after approved practice | | | |
| No phase, turn or UI technical audio in formal audio policy | | | |
| Audio route survives pause, reconnect and restart | | | |
| One complete technical log closes with `session_end` | | | |

## 6. Decision

| Decision | Selection and rationale |
| --- | --- |
| HMD/audio path ready for feasibility pilot? | `hold` / `ready with deviations` / `not ready` |
| Required corrective action | |
| Affected dry run that must be repeated | |
| Link to feasibility record or protocol freeze decision | |

## 7. Quest 3 核验与执行步骤（2026-09-21）

HUD 修订轮再次执行 `system_profiler SPAudioDataType SPUSBDataType -json`：Audio `_items=[]`、USB `[]`；PATH 未找到 adb。确认 REAPER、Logic Pro、Sonic Visualiser 仍在 Applications。此探针没有提供实际 Quest 播放或测量链路，不能区分未连接和当前环境枚举受限。已向用户询问输出、standalone/PCVR、测量麦克风/声级计与校准文件，尚无新增回答；因此本轮没有播放校准信号、录音或填写耳侧 SPL/SNR。既有数字 clip 审计只证明数字文件属性。

- 已确认：目标头显为 Quest 3。暂按内置扬声器准备；实际输出路径、standalone/PCVR、系统音量与测量设备尚未确认。
- 本机软件：REAPER、Logic Pro、Sonic Visualiser、MATLAB R2024b、Wwise Launcher、ArrayCalc V12、Soundvision。软件存在不代表有测量麦克风、校准文件或可用的耳模拟器。
- 本次 `system_profiler SPAudioDataType` 返回空设备列表；不能据此断言用户没有设备，也不能声称已连接 Quest 3 或已录得声学信号。
- 12 个 contour-soft 候选 WAV 再审计：均 1.35 s，最高数字 peak 0.448883，数字 RMS 范围 0.149983–0.149984。数字 RMS 一致不等于主观响度或耳侧 SPL 一致。

### 可复现的实测步骤

1. 固定最终播放链路，记录 Quest OS、应用 build、内置扬声器/耳机、连接方式、系统和 Unity 音量、spatial blend、HRTF、音源位置、混音器路由；关闭会改变电平的自动处理。先从低音量进行功能检查。
2. 使用有校准记录的测量系统。对于 Quest 3 近耳开放式扬声器，优先可重复的头部/耳位固定装置或合适的探针麦克风方法；普通手持声级计在远处测得的数值不能冒充耳侧声级。未确定装置前不填写 SPL。
3. 在 REAPER 中建立原始测量轨，关闭插件、归一化和自动增益，保留仪器输入增益与 WAV；先做仪器规定的校准检查，保存校准前后读数。没有已知灵敏度或校准源时只记录相对电平。
4. 固定佩戴位置，分别测量左右耳位；同一设置分别录制环境底噪、masker-only、每条 cue-only、cue+masker，并至少重复装戴检查位置敏感性。重复次数和接受阈值仍需方法确认。
5. 在 Sonic Visualiser 检查波形、频谱、起止与掉音；用经核对的测量流程计算同一时间窗/频率权重下的声级。SNR 用相同耳位及相同计量口径的 cue-only 与 masker-only 之差，不能直接从混合录音 RMS 相减，也不能把 dBFS 写成 dB SPL。
6. 对短 cue 同时报时长、测量时间窗和所用声级指标；声压峰值、数字 sample peak、LAeq 不能混写。可感知响度匹配、speech intelligibility、舒适度和 startle 仍需人耳检查。
7. 从最终 Unity build 播放同一批资产，核对所有条件和实际日志；声音/输出链/系统音量/空间化变化都需要重新检查。所有实测原始录音、设置和 checksum 链接回本记录。

### 参考依据与适用边界

- ISO 11904-1，*Determination of sound immission from sound sources placed close to the ear — Part 1: Technique using a microphone in a real ear*；ISO 11904-2，Part 2: Technique using a manikin。作为选择近耳测量方法的标准入口；采用版本及原文细则尚需实验室核对，本轮未执行标准测量，未声称符合标准。
- ISO 7731:2003 面向危险提示信号，不能据此为本研究日常通知指定报警声级或声称安全合规；线上 Methods 也明确不从它推导本实验的具体电平。
- Nadri et al. (2024), DOI `10.1080/10447318.2023.2180236`，原 PDF pp. 5–6 已核对：先做使用情景与声音评价，再按可识别性及反馈选择进入驾驶模拟器的声音。这支持迭代设计方法，不支持将其 takeover 的 880/1760 Hz 紧急提示直接用于日常 UAM 阶段通知。
- 三个 contour cue 的具体频率、1.35 s、谐波与包络属于候选设计参数；共享音色与不同 contour 的目的分别是 family identity 和训练后区分。频率/音色影响 urgency 的依据见现有 `contour_soft_candidate_research_plan.md`，最终保留理由必须来自对应听觉筛选，不是文献为某一音色背书。
- 女声 speech V3 是目前制作选择；未有证据证明女声对本 UAM 信息任务优于男声。Tram 9.17 31:41 要求解释 voice persona 的选择，不能把她举例的文献偏好改写为已证实的本项目效应。

### 本轮真实 Unity 运行时数字审计

证据：Unity 工程 `temp/cabin_measurement_20260921/runtime_audit.json`；可用 Editor 菜单 `Tools/Audio V0/Audit Cabin and Digital Audio` 在 Play Mode 重新运行。读取实际 AudioClip 解码样本，不用离线候选库冒充运行时刺激。

| 资产 | 时长 s | RMS dBFS | sample peak dBFS |
| --- | ---: | ---: | ---: |
| Procedural v1 T01 | 1.350 | -16.108 | -7.323 |
| Procedural v1 T02 | 1.350 | -16.212 | -8.046 |
| Procedural v1 T03 | 1.350 | -16.201 | -7.317 |
| Speech V3 T01 | 2.351 | -19.859 | -7.005 |
| Speech V3 T02 | 2.508 | -19.321 | -6.229 |
| Speech V3 T03 | 2.116 | -19.103 | -5.088 |
| Demo cabin bed v2 | 8.000 | -24.668 | -17.175 |

- 全部单声道 44.1 kHz，Editor 输出 48 kHz stereo；T01--T03 通知 AudioSource volume=0.55、spatialBlend=0，AudioListener volume=1。方向匹配 turn cue 使用独立 AudioSource，目标 spatialBlend=1、左右 source position 对应转向；其 HRTF/roll-off/耳侧映射仍须在最终 HMD 链路中校准，不能把桌面运行写成已验证的人体空间听感。
- 通知与噪声原始 sample peak 均小于 1；这是单个解码 clip 的无数字削波检查，不是输出总线、声学失真或听感验证。
- Speech 全文件 RMS 低于 non-speech，且时长不同；不可声称匹配了响度。静音分布、频谱及 Quest 传递函数都影响可听性，不能通过简单 RMS 拉平就冻结音量。
- JSON 同时保存解码 float32 样本 SHA-256；它识别本机解码波形，不替代原始 MP3/WAV 文件 checksum，跨平台解码可能不同。
- 重要版本差异：Unity procedural v1 使用 `sin(2π t f(t))`，离线 contour-soft generator 使用累积相位。二者不是相同波形；线性 sweep 中前者瞬时频率为 `f(t)+t f'(t)`，不能把生成器变量的频率端点直接写成实测频率范围。旧 v1 保留为技术回归刺激，本轮没有将它冒充通过筛选的 contour-soft family；正式接入前必须选定实际 WAV 并重跑声音 gate。
