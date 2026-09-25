# Paper #2 / Audio V0 当前状态交接

日期：2026-09-24。范围：Unity Audio V0、Paper #2、数据查看器和 pilot 准备；Scoping Review 未修改。

## 已确认

- Unity 权威工程：`/Users/trusoegn/GitHub/Audio-VR/Audio-VR`，Unity 6000.6.0f1，场景 `Assets/AudioV0.unity`。
- 当前研究结构仍是四条件 `speech/non-speech × quiet/noise`。左右转弯提示音已纳入四条件，左转从左侧、右转从右侧播放，记录为 `spatial_turn_cue`；它是共享旅程投送，不是第三因素，也不计入 T01--T03 的 12 个识别机会。
- 当前路线候选是 `full_city_journeys_v5_210s_candidate`，每段目标 ≤210 s（3:30）。源码验证器已包含此上限断言，但当前改动后尚未重新完成正常时钟四路线运行，因此不能把旧版 282/272 s 日志当作 v5 证据。
- 场景已显式保存 `allowSpatialTurnCuesInTechnicalAudition: 1` 和 `includeSpatialTurnCuesInFormalStudy: 1`。HMD 耳侧映射、HRTF/SDK、roll-off、可听性、舒适度和视觉任务干扰仍需校准/pilot。
- 论文线上 Overleaf 是写作权威；正文已说明完整航程、≤210 s 候选、四条件共享方向 cue 和 context-supported identification 边界。Figure 1 使用 Figma frame [43:2](https://www.figma.com/design/5tsO6O3BIXNRssW9p5FQZD/Audio-photo?node-id=43-2)，本地资产和线上编译基线已检查无重叠；最后一次本地排版微调需重新上传并编译后才可称最终稿。

## 数据手册与测试入口

- 操作手册、Evidence category 分类、字段、下载和分析边界：`/Users/trusoegn/论文2/docs/participant_runbook_and_data_dictionary.md`。
- 原始数据根目录：`/Users/trusoegn/AudioV0_Data/`；Unity 新会话按 `technical_demo/`、`pilot/`、`participant_run/` 分目录写入。当前已有数据只有 `technical_demo`，不能改目录名冒充 pilot。
- 查看器启动：双击 `/Users/trusoegn/论文2/tools/打开 Audio V0 数据中心.command`；本地地址 `http://127.0.0.1:8091/`。图表是不可变快照，不是实时流；筛选或新日志写入后点击 **Reload local recordings**。Cleaned/Excluded CSV 与图表使用同一 snapshot，原始 JSONL 不修改。
- 2026-09-24 已重新启动服务并实际打开 **Researcher guide**；页面显示三类 Evidence category、pilot 原始路径、snapshot/Reload 规则和 pilot 后 exporter 步骤，当前技术会话被明确标为 synthetic / not participant results。
- `technical_demo` 只用于软件/操作检查；`pilot` 只能支持可行性与流程证据；`participant_run` 需伦理、冻结协议和正式采集授权。Pilot 数据进入 `pilot/<session_id>/`，先写 `run_record.json`，再用 `export_audio_v0_events.py` 验证，不能直接当 Results。

## 当前未完成与下一步

1. 在 Unity 完成缓存重建/编译后，执行 `Tools/Audio V0/Verify Four Full Flights`；检查四路线均 ≤210 s、每段左右 cue 的 side/local position/spatial blend、四条件一致性、cue 不进入 T01--T03 分母。
2. 保存新的 `temp/` QA 证据和原始日志路径；不要覆盖旧 `20260923` 证据，也不要在验证前宣称 v5 运行通过。
3. 将最新 Figure 1 和最后排版微调上传 Overleaf，重新编译并逐页检查 Figure 1、3、4、5 与第 9 页缩版文字。
4. Pilot 前完成 Quest 3 输入/可读性/帧时间、耳侧校准、伦理/同意、路线顺序与 counterbalance 冻结；pilot 结束后按手册记录 comfort、audibility、左右映射、任务干扰、缺失和偏差，再决定 hold/iterate/freeze。

本轮尝试通过 Unity batchmode 做导入检查时，Unity 报告该项目已被现有 Editor 占用，因此没有启动第二个实例；现有 Editor 的最新日志仍包含缓存重建期间的脚本编译错误，不能把本轮写成 compile pass。不要删除或覆盖当前场景；下一次应在现有 Editor 中等待 package/assembly refresh 完成后再检查 Console，并以新的编译日志作为证据。

## 证据边界

已有四路线日志、viewer 图表、截图和编译记录均为 synthetic/technical evidence。它们能证明软件链和文档路径，不能证明真人听感、HMD 空间听觉、舒适度、正式样本效应或论文 confirmatory Results。
