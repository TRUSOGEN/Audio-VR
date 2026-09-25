# 游戏交互、数据与正常速度验证交接

日期：2026-09-21（Sydney）。工程：`/Users/trusoegn/GitHub/Audio-VR/Audio-VR`，Unity `6000.6.0f1`，场景 `Assets/AudioV0.unity`。

本交接替代同日 `current_integrated_feedback_handoff_20260921.md` 中球本体点击、独立几何判定、旧引用和未完成桌面验证的状态。历史记录保留，不能继续作为当前任务契约。

## 用户追加反馈后的宽松视觉占用版本

用户指出选择题时小球难以点击，任务应占用视觉而非通关。按此调整：

- 当前呈现为 `passenger_hud_attention_v8_candidate`，整个游戏舞台接收一次点击，小球与圆圈不拦截射线；游戏区和答题/信心区保持独立。当前 control_id 为 `ui_visual_activity_stage`。
- 默认金球显示 3.5 s，整个曝光期间（相位 0–1）都可点击；灰球只需看过。间隔 1.5 s，仍约 5 s 一球，GO 概率暂留 0.25。取消精准卡拍要求，漏点后自然继续，采用中性反馈，无通关/淘汰/累计分数。
- 这是按用户目标作出的 candidate 设计决定，不是文献证明或 pilot 冻结；个体反应差异、实际视觉占用和舒适难度仍需形成性验证。研究条件内不按实时表现自动改变难度。
- 底层数据契约仍为 v2，具体窗口/时长和 binding 写入运行快照；新全窗数据与下述窄窗数据不能直接合并作难度结论。
- 下述 14 分钟四条件验证使用旧 2.2 s、0.4–0.6 窄窗和 v7；它只证明该次流程与数据链路，新 v8 的验证在本节后续记录，不能套用旧截图证明新交互。

### v8 最终核验

- 全量 943 项：939 passed、2 skipped，2 个新生命周期用例最初因测试夹具仅两个航点失败；修正夹具为当前要求的三个航点后，相关类 18/18 通过。宽松窗口的 0.02/1.75/3.3 s 点击用例均在全量中通过；没有把初次失败隐藏成一次全绿。
- 当前运行日志确认 3.5 s、间隔 1.5 s、window 0–1 与 `ui_visual_activity_stage_hmd_pending` 已实际加载。
- v8 session `session_20260921_064854_443_a919590251f74fca99e4ba11b2e0f8f7`：识别窗口打开（event 2211）后，舞台点击在 2214 记 hit，随后 2218 才提交识别；说明答题 pending 期间游戏响应成功。2218 的答题和 2222 的信心点击没有附带 visual_response。后续 2223 舞台点击独立记 false_alarm。该组输入由实际 UI control_id 记录，未确认操作者身份，不冒称 Agent 全部完成。
- 两次识别 pending 快照确认舞台中心、球中心和两处空白点均射线命中 StimulusFlash。信心 pending 下的游戏点击尚未单独取得运行证据，不能将识别状态验证外推为全部完成。
- 仅开发 QA 临时通知/窗口明确标记，第二次因电脑控制延迟用 600 s QA window、航线暂定而游戏继续；实际配置仍 8 s。最后已 Restore QA Windows 并退出 Play，正常配置恢复，timeScale 1。早先 QA 的无效映射/超时保留于各自日志，不作为正式数据。
- 本轮 native screenshot 最后一次自动审批检查超时，未继续重试；原 UI 射线快照和实际 JSONL 已归档。新截图见 `../temp/game_upgrade_20260921/unity_evidence/runtime/20260921_064933_602.png`。

## 已实现

- **游戏交互**：固定 timing pad、可预测球轨迹、GO（金色）/WAIT（灰色）文字与颜色双重提示、判定窗高亮和无声反馈；轨迹与评分共用任务时钟。早按可在窗内重试；晚按、重复、空窗和最终结局分别记录。参与者不显示累计成绩，不新增节拍或命中音。
- **流程**：参与者暂停/恢复按钮、放大下一段和识别/1–7 信心按钮，底部作答区与游戏舞台分离；非交互背景不拦截点击。XR raycaster 与输入模块使用相同启用条件，但 Quest 射线尚未实测。
- **数据**：`rhythm_timing_go_nogo_candidate_v2`、每 block 不可变配置与 seed、独立目标 RNG、每机会唯一结局。GO 截止后立即 miss；截止前结束为 interrupted，已完成结果不被覆盖。配置 preflight 明确失败，防止路线带着无效任务继续。
- **导出**：`visual_events.csv` 每刺激一行，`visual_responses.csv` 每次点击一行；按原始事件重建汇总，并复核 RT、时差、相位和生命周期。正式数据质量失败会撤销分析资格并非零退出；技术 QA 保留异常行。
- **数据中心**：自动选最新 session，GO/WAIT 分母分开、中断排除、历史版本隔离、信心单列筛选、异常时不展示误导率；原始事件表加宽时间与事件列。仅本机读取。
- **城市**：道路/标线/人行道/绿带分层，树冠增加层次、灯柱取消发光材质；18 栋近景楼纳入碰撞净空检查。静态街道设施按材质与区域合并：本轮运行 776 个源 renderer 合并为 83 个启用 renderer，实际城市日志为 844 栋；旧 864 数字是历史。
- **速度**：发现项目残留 `m_TimeScale: 4`，已恢复默认 1。自动验证明确区分 normal/accelerated，退出验证恢复 1。

下述前轮 v7 验证参数（已被上述宽松默认替代）：GO 概率 0.25、球通过 2.2 s、间隔 2.8 s、判定相位 0.4–0.6（宽 0.44 s）；没有冻结正式任务、难度或样本量。

## 实际验证与边界

### 回归

- Unity 定向 15/15；全量 937 项，935 passed、2 skipped、0 failed。全量结果早于最后一处 XR raycaster 条件对齐，该小改另经编译和真实运行检查。
- Python 导出 14/14，包括异常/缺失日志、越窗命中、重试、中断与跨 block 隔离。
- 数据中心内嵌 JS 8/8；另在 Chrome 实际加载、筛选信心和视觉任务并查看渲染。Node DOM stub 测试不冒充浏览器证据。
- 正常四段完成时 Console：5980 logs、0 warnings、0 errors；这是该次查询快照，不覆盖历史所有运行。

### 真实桌面鼠标

session `session_20260921_053125_019_cb90c73dcd3d4ebbb87e820ce0b3984c`，明确标记 `developer_mouse_qa=true;synthetic_inputs=false`。

- 真鼠标暂停/恢复，两端 task elapsed 均为 138.5837 s。
- 真鼠标点击固定 timing pad，记录 `control_id=ui_rhythm_timing_pad`，WAIT 的 false_alarm；这证明输入可达，不是操作者表现研究。
- 真鼠标“下一段”从 NS_Q 进入 NS_N。
- 成功 hit、早按重试、miss 等更多边界来自控制器测试和合成输入，不声称全部由真人鼠标完成。

### 正常速度四条件

session `session_20260921_053934_228_015c00016d044a1e849321c31869180e`，明确标记 `synthetic_inputs=true;purpose=software_integrity_not_human_performance`。

- 完整 846.208 s，5980 条事件（sequence 0–5979 连续，ID 唯一，monotonic 非降）；4 route complete、1 session end。
- 条件 `NS_Q → NS_N → S_N → S_Q`；各 3 次通知、识别与信心，总计 12/12/12，逐事件父关联通过。
- 4169 个运动采样的 timeScale 全为 1；一次约 2.024 s 暂停保持任务时钟。
- 170 个刺激 onset/offset 配对：27 hit、12 miss、28 false_alarm、101 correct_rejection、2 interrupted；70 次点击含 15 early。四段汇总全部重建一致，视觉和通知质量 flags 为空。
- 计划软件 lead 3.995–4.000 s，软件实际 onset 到 transition 3.992–3.999 s；未测声学到达时刻。
- Editor Update 平均帧时间分段约 2.24–2.87 ms，最大单帧 359.1703 ms；不声称无卡顿、GPU 性能或 Quest 帧率。
- 此次原始日志的 `block_started.attempt_id` 沿用上段（首段空），因它先于 `BeginRouteClock` 写入；route_start 及通知/任务事件关联正确。审计以 block + route_start 重建并显式报告该限制；旧 JSONL 未回写修正。后续修复验证单列，不能倒改本次证据。

### 截图与可复现材料

统一证据目录：`/Users/trusoegn/论文2/temp/game_upgrade_20260921/`，避免仅依赖 Unity 的 Temp 清理周期。

- `unity_evidence/normal/`：正常速度 cruise、descent、confidence、arrival 截图和完成标记。arrival 在下一段启动同帧截图，可证明近地视景，不作为段间按钮状态证据。
- `rhythm_pause_verified_20260921.png`：真实暂停截图；`game_upgrade_baseline_20260921.png` 为修改前基线。
- `data_center_verified.png`：浏览器实际数据卡与事件表。
- `unity_evidence/runtime/`：按钮射线、输入模块、净空和 renderer 实测快照；探针不替代真实点击。
- `unity_evidence/full_test_result.json`、`targeted_test_result.json`：Unity 原始测试返回。
- `normal_export/validation_report.json`、`independent_audit.json`：最终四段导出与独立审计。
- `raw_sessions/`：真实鼠标与正常四段原始日志副本；审计保留 SHA-256。
- `before/`：修改前场景、配置和 TimeManager 备份；用户明确允许 Reload 后继续验证。

## 研究依据

已核查三篇原论文全文和旧引文元数据，详见 `../docs/game_interaction_evidence_review.md`，原文/文本/hash 清单位于 `../temp/game_interaction_sources/`。

Rzayev et al. (2019), DOI `10.1145/3311350.3347190`；Dingler et al. (2018), DOI `10.1145/3170427.3188695`；Monk et al. (2008), DOI `10.1037/a0014402`。这些用于限定通知位置、视觉任务与中断恢复问题；未找到直接证明当前球时机任务适合 UAM 的证据。旧 T9+HUD/Stretchertainment 过度表述已在证据审查中纠正，不能继续引用为当前 HUD 有效性证明。

未修改论文正文、线上 Overleaf、正式样本量或 pilot 结论。

## 未关闭与下一步

1. Quest 3 实机：文字角尺寸、坐姿头位、射线/控制器、双任务输入、GPU/帧时间与舒适度。
2. 真实输出链路、声音筛选、耳侧校准、声学 onset/SPL/SNR；软件播放事件不能代替可听性证据。
3. 形成性体验：能否理解 GO/WAIT、恢复成本、是否只记节拍、是否真的持续视觉占用、趣味性、floor/ceiling、信心额外负荷与地图阶段推断。
4. 当前参与者暂停与研究者暂停均经过 `SessionController.PauseBlock`，reason 仍为 `operator_pause`；日志不能据此区分发起者。
5. continuous tracking 的独立渲染坐标已恢复，但本次完整四段运行验证的是 timing 候选；tracking 需独立主流程检查。
6. 按上述证据选任务与参数后，再定 analysis plan、样本量/精度论证、pilot 与正式冻结；不将本次合成命中率视为难度证据。

## 使用入口

Unity 打开 `Assets/AudioV0.unity` 并 Play，正常速度自动开始技术演示；F1 为研究者面板，参与者可暂停并在段间继续。

数据目录已迁至 `/Users/trusoegn/AudioV0_Data/`，旧隐藏目录保留备份，详见 `visible_data_folder_20260921.md`。

数据中心：`http://127.0.0.1:8091/`；若服务未运行，使用原有“打开 Audio V0 数据中心.command”入口。

显式自动验证菜单：`Tools/Audio V0/Verify Normal Speed Sequence`，输入均带 synthetic 标记；`Stop Verification` 停止自动输入并恢复 timeScale 1。

导出（QA）：

```sh
python3 tools/export_audio_v0_events.py \
  '/Users/trusoegn/AudioV0_Data' \
  temp/game_upgrade_20260921/recheck_export \
  --allow-technical \
  --session-id session_20260921_053934_228_015c00016d044a1e849321c31869180e
```

工程仍保留用户原有未提交修改；本轮未执行 commit、reset 或清理原有数据。
