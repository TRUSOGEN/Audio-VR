# Audio V0 完整教程与数据工具分析

更新：2026-09-24。本文把桌面技术演示的操作、正式实验前的 gate、数据中心每个网页的读法和导出分析放在一起。当前 Unity 工程、刺激、路线、时长、空间转弯提示和实验参数仍有 candidate/provisional 标记；技术演示或 synthetic 日志不能写成真人结果。

## 1. 入口与证据边界

| 内容 | 入口 |
| --- | --- |
| Unity 工程 | `/Users/trusoegn/GitHub/Audio-VR/Audio-VR`，Unity `6000.6.0f1` |
| Unity 场景 | `Assets/AudioV0.unity` |
| macOS Player | `Builds/AudioV0-macOS/AudioV0.app` |
| 数据中心 | `/Users/trusoegn/论文2/tools/打开 Audio V0 数据中心.command`，`http://127.0.0.1:8091/` |
| 原始数据 | `~/AudioV0_Data/technical_demo/`、`pilot/`、`participant_run/` |
| 正式导出器 | `/Users/trusoegn/论文2/tools/export_audio_v0_events.py` |

`technical_demo` 用于代码、路线、UI、音频链和数据查看器检查；`pilot` 只能支持流程可行性；`participant_run` 才可能进入正式研究分析，并且仍要通过 protocol、伦理、校准和质量检查。`Evidence category` 是来源标签，不是质量分数。`operator_test`、`synthetic` 或软件 QA 标记会把记录限制为 QA-only，即使文件被放在别的目录。

## 2. 启动前检查

1. 只打开一个 Unity Editor，确认场景为 `Assets/AudioV0.unity`，等待 Package、Asset 和 C# 编译结束；不要删除 `Library` 或同时开第二个 Editor。
2. 人体实验前确认伦理与同意流程、匿名 participant ID、协议版本、条件/路线顺序、设备、校准记录和数据保存位置已冻结。当前场景默认是 `technical_demo / operator_test`，不能直接招募。
3. Quest/耳机实验要另做 HMD 追踪、输入、帧时间、舒适度、左右声像映射、声级与 masker 校准。`spatialize=true` 和 `spatialBlend=1` 只证明 Unity 请求 3D 声源，不证明 HRTF 插件已经生效或人耳已经确认方向。
4. 检查输出设备、系统音量和 Player 窗口；先听教程示例，再进入正式段落。不要用盲目增大音量代替校准。

## 3. 快速教程与四段航程

### 3.1 教程

1. 启动 Unity Play 或 macOS Player，看到 `QUICK START` 后点击开始按钮。
2. 依次听六个带标签示例：三种非语音提示和三种语音提示。示例用于学习，不进入实验分母。
3. 完成 GO/WAIT 练习：金色 GO 出现时点击游戏区域一次，灰色 WAIT 时不点击。
4. 听一条练习通知，在回答区选择阶段，再选择 `1–7` 信心。
5. 回答理解题。答错会显示解释并允许继续；`REPLAY` 只在自愿重听时使用。
6. 看到 `READY TO FLY` 后，必须明确点击 `START FOUR FLIGHTS`；没有完成明确启动不会自动开飞。

### 3.2 正式候选流程

- 每段从地面起飞，经历 climb、cruise、left/right turn、descent、landing preparation 和 touchdown，落地后等待操作者点击继续。
- 每段有 T01/T02/T03 三次识别，四段共 12 次。阶段选项为 `Entering cruise`、`Beginning descent`、`Preparing to land`；识别后选择 1–7 信心。
- 当前候选是 context-supported judgement：景物、地图、自然阶段顺序和运动可能帮助回答，不能称为 sound-only identification。
- 左右转提示是四条件共享的 journey delivery，不是第三个实验因素，也不进入 T01–T03 分母。左转声源在乘客舱中线到左侧范围内往返 sweep，右转对称；每个转弯持续期间按固定 pulse 节奏重复播放，连续采样不会以每帧无限制地产生额外 pulse。
- 点击乘客面板 `PAUSE` 会冻结路线和视觉任务；恢复后继续，不能把中断补写成回答。`F1`/`fn+F1` 是研究者控制台，`END BLOCK` 是提前结束，不等于完整完成。

### 3.3 当前软件候选参数

四条路线的运动计划约为 201.85、206.13、192.26、192.17 秒，目标上限 210 秒；速度上限 18 m/s、爬升 5 m/s、下降 4 m/s。通知提前量约 18 秒、识别和信心窗口约 8 秒，均需 pilot 后冻结。它们是软件候选值，不是人体舒适度或真实 eVTOL 性能结论。

## 4. 左右转声音如何验收

### 4.1 代码行为

`AudioV0RuntimeAudioDirector` 在 `RoutePlayer.CurrentSegmentLabel == "turn"` 期间持续运行转弯声轨。每约 `0.55 s` 播放一个短 pulse；音源的乘客舱本地横向位置以约 `1.10 s` 为周期从中线扫向当前转弯侧再回到中线，因此左转会反复出现“中间到左侧”的空间移动，右转对称。声源配置为：`spatialBlend=1`、`spatialize=true`、`spatializePostEffects=true`、`dopplerLevel=0`、Logarithmic rolloff、`minDistance=2.5 m`、`maxDistance=12 m`，左右最大横向偏移为 `2.2 m`。直航或航程结束时重置到中线；路线时间倒退时清除上一段的 sweep 状态。旧版把声源改挂在 2.2 倍机体根节点，左右实际距离被放大；当前源码已将锚点改回乘客舱，须在新 Player 和 Quest 音频链路中验证实际响度、移动感和空间化。

### 4.2 日志验收

在原始 JSONL 中筛选 `event_type == spatial_turn_cue`，逐行检查：

- `side=left/right` 与 `source_local_position.x` 符号一致；
- 每个物理转弯进入后，转弯持续期间按固定节奏重复播放短提示音；
- 音源在乘客舱横向从中线扫向对应侧再回到中线，左转和右转保持镜像；
- 每个 pulse 都记录 `pulse_route_time_s`、`side` 和 `source_local_position`，可在日志中检查重复触发与空间移动；
- `spatial_blend=1.00`、`spatialize=True`、clip 非空；
- 没有意外的 `spatial_turn_cue_unavailable`；
- route 重启后计数重新开始；不同 condition 的共享转弯提示不进入识别统计。

旧 session `session_20260924_050536_021_65e7953ab2d04b1191015b1886a314a1` 已显示首段 6 次、第二段 6 次左右 cue，且日志中的空间参数正确；第三段在旧 QA 中途退出，不能当作完整四段证据。新源码修改后必须产生新的 session 和新的 QA 目录。

### 4.3 人耳与 HRTF gate

桌面日志不能证明真人听见声音，也不能证明 HRTF。pilot 中应逐一记录：左/右映射、可听性、与 masker 的相对声级、定位稳定性、是否干扰 GO/WAIT、舒适度和是否需要重播。若项目没有外部 spatializer，结论应写成 Unity 3D panning/spatialization request；只有在指定耳机/HMD 链路实测后，才可写 HRTF 或人耳空间化结果。

## 5. 数据中心网页逐页分析

打开启动器后访问 `http://127.0.0.1:8091/`，新增日志后点击 `Reload local recordings`。页面是只读审阅器：它读取本机 JSONL，不上传 participant 数据，也不会修改原始文件。先选 `Evidence category`，再选 session；不要跨 category 合并。

### 5.1 Session library / 总览

先确认 session ID、category、participant marker、`four_blocks_complete`、`closed` 和 synthetic/technical 标签。`closed=true` 只表示发现 `session_end`，不代表每个 block 有效；`four_blocks_complete=true` 也不能替代 exporter 的 validation。中断、缺答和 error 要保留原状。

### 5.2 Session review / 条件筛选

选择 session 后按 `condition`、`block`、`route` 缩小范围。记录当前筛选，因为图、Cleaned CSV、Excluded CSV 和下载链接都应来自同一个 snapshot。查看 metadata：participant ID、route/stimulus/task/protocol version、noise、apparatus、calibration、counterbalance；出现多个版本或空 calibration 时，先标记 deviation，不直接比较条件。

### 5.3 Condition comparison / 条件比较

这是描述性比较面板，不是自动统计检验。对每个条件报告识别分母、正确数、timeout、accuracy、正确 RT 的 n/median、confidence n/median，以及 GO/WAIT 的 hit、false alarm 和各自分母。准确率分母包含普通错误和 timeout，排除单独标记的技术无效事件；正确 RT 只含 response window 内、首次接受且正确的回答，不对缺失插值。

若某条件没有有效机会，比例显示为 undefined；不要把 0 当作没有数据。当前每条件事件数很少，任何 mixed model、协变量、随机效应和多重比较都仍是 analysis-plan gate，不能从这张图自动得出显著性或条件优胜者。

### 5.4 Flight recording / 航程与阶段

检查 `route_start`、`route_segment_enter`、`route_complete`、`pause`、`resume`、`session_end` 的顺序和 route clock。比较实际 route duration、阶段标签、起降是否完整、暂停期间位置/时间是否冻结。路线图支持软件几何和流程 QA，不支持真实飞行动力学或舒适度结论。

### 5.5 Event audit / 事件审计

按 event sequence 和 event ID 检查重复、跳号、缺失 parent、重复 notification、response-after-window、response-at-or-after-transition、error 和 malformed JSON。`identification_timeout` 是明确的未回答，不要补成错误选项；tutorial/practice 事件必须与正式 task 分开。`spatial_turn_cue` 只在音频审计中检查，不放入 T01–T03 accuracy。

### 5.6 Cleaned CSV / Excluded CSV

Cleaned CSV 是某个 panel 实际使用的逐 trial、逐视觉机会或逐 motion sample；Excluded CSV 保存被排除的记录与 `exclusion_reason`。常见原因包括 technical invalid、interrupted、wrong task version、duplicate event ID、missing parent、late response、malformed source 和 response after transition。先看 excluded 数和理由，再读 cleaned 数；不能只下载 cleaned 后声称“没有缺失”。

### 5.7 图件与 snapshot

单图支持 SVG、PNG、PDF；每个 panel 也有对应 Cleaned/Excluded CSV。图和 CSV 要使用同一 snapshot token；如果 snapshot expired 或筛选条件改变，先 Reload/review，再重新下载。常用 panel：`accuracy`、`rt`、`confidence`、`visual`、`trajectory`、`altitude`；高度图另下载 notification markers。图是审阅输出，不应覆盖正式 exporter 的校验结果。

## 6. 指标计算口径

| 指标 | 使用记录 | 分母与排除 |
| --- | --- | --- |
| Identification accuracy | notification + accepted response/timeout | 正常错误和 timeout 计入失败；技术无效、重复、损坏单独排除 |
| Correct RT | 正确且首次接受的 response | 只保留 response window 内且早于 transition onset；不插补 |
| Confidence | 1–7 confidence response | 仅有对应有效识别机会的回答；缺失保留为缺失 |
| GO hit rate | 可评分 GO opportunities | interrupted/invalid 排除；分母为 GO 的 hit+miss |
| WAIT false-alarm rate | 可评分 WAIT opportunities | 分母为 false alarm+correct rejection |
| Trajectory | flight motion samples 的 x/z 坐标 | 检查缺失水平位置和重复 event ID；按 route/block 分开读 |
| Altitude/motion | flight motion samples | 用于路线和阶段 QA；不直接作为乘客舒适度结论 |
| Spatial turn cue | `spatial_turn_cue` events | 共享 delivery 审计；不进入识别准确率、RT 或 confidence 分母 |

推荐报告顺序：先给原始记录数与 excluded 数，再给有效分母、分子、比例和 median/n，最后给质量标记与版本。小样本和每条件仅三个 planned notification 时，优先描述性和精度评估，不把百分比当稳定总体效应。

## 7. 正式导出与复现

技术 QA 才可使用：

```bash
python3 /Users/trusoegn/论文2/tools/export_audio_v0_events.py \
  /Users/trusoegn/AudioV0_Data \
  /Users/trusoegn/论文2/temp/audio-v0-export \
  --allow-technical --session-id '<exact_session_id>'
```

pilot/participant_run 不使用 `--allow-technical`。输出目录必须是新建或空目录；先读 `validation_report.json` 和 `sessions.csv`，再读 notification、visual、motion CSV。非零退出、`validation_errors`、缺失 session_end、重复 event ID 或 metadata 不一致都要先处理。导出器验证完整性，不替你完成统计模型或伦理批准。

## 8. 实验前 gate 与交付记录

正式采集前依次关闭：伦理/同意、participant pseudonym、Quest/HMD 输入与可读性、耳机/空间声校准、声音在 quiet/noise 下的可听性、左右映射、视觉任务 floor/ceiling、暂停/退出、日志闭合、counterbalance/route freeze、样本精度/分析计划。2–3 人 feasibility pilot 只能回答流程是否可行、哪里需要修订，不能选择“最佳声音”或证明 sound-only identification。

每次运行保存：源码/Player build 版本、刺激 checksum、profile、路线与条件顺序、校准记录、原始 JSONL、manifest、export validation、cleaned/excluded CSV、图件 snapshot、deviation record 和分析备注。源码改动后必须新开 session；不要改写旧 JSONL 或把 synthetic/QA 移成 participant evidence。

## 9. 当前复现边界

本文件描述当前工程和 viewer 的实际接口与候选口径。已知技术证据包括 Unity Editor 的完整四航程历史 QA、空间 cue 日志参数和本地 viewer 的只读导出路径；最终改动后的四段新 QA、macOS Player 完整四段、Quest/HRTF、人耳可听性、pilot、样本精度和正式分析仍需独立记录。每次交接应在 `/Users/trusoegn/论文2/handoff/` 保存日期化 handoff，并明确 completed、pending、provisional、pilot-dependent。
