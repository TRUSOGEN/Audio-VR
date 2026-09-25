# Audio V0 转弯提示、教程与线上稿 Gate

日期：2026-09-24。范围：Paper #2 Audio Experiment 的 Unity 转弯提示音修复、路线图更新、实验教程与数据中心分析说明。Scoping Review 未改。

## 当前有效入口

- Unity 权威工程：`/Users/trusoegn/GitHub/Audio-VR/Audio-VR`，Unity `6000.6.0f1`，场景 `Assets/AudioV0.unity`。
- 线上 Overleaf：`https://www.overleaf.com/project/6a799d68583a7077568f0ad5`，主文件 `main.tex`，方法文件 `methods.tex`。
- 本次教程：`/Users/trusoegn/论文2/docs/audio_v0_complete_tutorial_and_tool_analysis_20260924.md`。
- 本次路线图脚本：`/Users/trusoegn/论文2/temp/20260924-final-delivery/render_current_route_figure.py`。
- 待上传 ZIP：`/Users/trusoegn/论文2/temp/20260924-final-delivery/overleaf-upload-20260924.zip`；SHA-256 清单：`/Users/trusoegn/论文2/temp/20260924-final-delivery/overleaf-upload-20260924.sha256`。
- 数据中心：`http://127.0.0.1:8091/`，数据根目录 `~/AudioV0_Data/`。

## 本次已实施

1. `AudioV0RuntimeAudioDirector.cs`
   - 转弯期间约每 `0.55 s` 重复播放一个短 pulse，而不是整段弯道只响一次。
   - 音源以约 `1.10 s` 周期从乘客舱中线扫向当前转弯侧，再回到中线。
   - 左转使用左侧，右转使用右侧；直航、航程结束或策略关闭时回到中线并清空状态。
   - 声源保持 `spatialBlend=1`、`spatialize=true`、`spatializePostEffects=true`、Logarithmic rolloff、`dopplerLevel=0`；`minDistance=2.5 m`、`maxDistance=12 m`，左右最大横向偏移为 `2.2 m`。缺少 source/clip 时记录 `spatial_turn_cue_unavailable`。
2. `AudioV0RuntimeScenario.cs`
   - 机体运行时绑定后，空间 cue 的父节点改回 1:1 的乘客舱，而不是 2.2 倍的 hero aircraft root，避免左右位置和声级距离被放大。
3. `论文2/overleaf/methods.tex`
   - Methods 路线描述更新为场景审计中的 130/160 m 平台、1.92--2.14 km 路线和不超过 210 s 候选时长；保留 pilot/Quest/空间声未关闭边界。
4. `论文2/overleaf/figures/full-flight-routes.pdf/png`
   - 基于 `run_20260924_051232/actual_route_geometry.csv` 重绘为两栏图：实际平面路线及高度/时间；本地输出已目视检查。
5. 新教程文档覆盖：Unity/Player 操作、教程与四航程、左右 cue 验收、Evidence category、Session library、Condition comparison、Flight recording、Event audit、Cleaned/Excluded CSV、图件 snapshot、指标分母、正式 exporter、pilot/Quest/伦理 gate。

## 证据与边界

2026-09-24 新增 synthetic software QA session `session_20260924_065517_415_315d78e7fc654725859797e3207222b2`：记录到 76 条 `spatial_turn_cue`，其中 right=40、left=36；同一侧 pulse 的 route time 以约 0.55 s 递增，右侧样本的 `source_local_position.x` 在 0.00/2.20 m 间交替，左侧样本在 0.00/-2.20 m 间交替；`spatial_blend=1.00`、`spatialize=True`，没有 `spatial_turn_cue_unavailable`。这证明软件触发、重复节奏和左右位置 sweep 已按新代码运行；它仍然是 technical_demo synthetic 证据，不等于 Quest/HRTF 或真人听感证据。

新的 macOS Player 构建任务 `db48367e` 已返回 `succeeded`，输出路径为 `/Users/trusoegn/GitHub/Audio-VR/Audio-VR/Builds/AudioV0-macOS/AudioV0.app`；这只证明本地构建成功，尚未替代实际试听、Quest/HRTF 或 pilot gate。

- 项目最新编译状态为 `success=true`、`errorCount=0`、`warningCount=0`；新 synthetic QA 已实际经过修改后的 AudioDirector。旧完整 QA 日志 `session_20260924_050536_021_65e7953ab2d04b1191015b1886a314a1` 的首段 6 次、第二段 6 次 `spatial_turn_cue` 仍保留作历史参考，但不能替代本次新 sweep 证据。
- 旧完整基线 `session_20260924_011325_716_70328e5cab1444e7b8fb1525fecd2a58` 有 13 次 spatial cue，证明软件链曾产生日志，但不证明本次新父节点和滞回修改已运行。
- Unity `ProjectSettings/AudioManager.asset` 的 `m_SpatializerPlugin` 为空；目前只能声称 Unity 3D spatialization request/default panning 配置，不能声称 HRTF 生效、Quest 已部署或真人已听到。
- 在本 handoff 之后又将 `TechnicalAudioAllowed` 的早退顺序前移，避免策略关闭时重新触发转弯 pulse；该最后一行源码修复尚未在当前 Editor 中重新编译，需在唯一 Editor reload 后重跑一次受控 QA。
- 新路线审计 `temp/20260924-final-delivery/run_20260924_051232/geometry_audit.json`：3962 采样、0 碰撞、0 自动修补；路线时长 201.849673 / 206.133982 / 192.260056 / 192.174623 s；平台 130 / 160 / 130 / 130 m。该 run 的完整四段仍被中断，不能与旧完整基线合并成“新源码四段完成”。
- 已对基线线上 PDF 第 1–10 页做本地逐页目视检查。基线截图目录：`/Users/trusoegn/论文2/temp/20260923-full-flight-paper/baseline-01.png` 至 `baseline-10.png`。它们不是本次新图和新 Methods 的最终渲染。
- 本地数据中心已在 `http://127.0.0.1:8091/` 启动；health 指向 `~/AudioV0_Data/technical_demo`，index 读到 59 个会话。用旧完整 QA session 的 `/api/review` 核对到 `qa_only`、四段完成、`session_end=1`、四条件各 3 个识别机会。这是只读 API 验证，浏览器逐页点击仍受 CUA 连接故障限制。

## 未完成 Gate

1. 本地已更新 `methods.tex`、`main.tex` 和 `full-flight-routes.pdf/png`，并已打包待上传 ZIP；本轮尚未上传线上。两次 CUA 初始化均返回 `Sky Computer Use native pipe startup failed`，浏览器列表为空，`iab`/`chrome` 均不可用；直接项目请求返回 403，Overleaf Git 在无凭据模式无法认证，因此尚未重新编译，也尚未检查新的 10 页 PDF。
2. Unity 源码主体已重新编译并完成受控 synthetic QA；最新 session 已验证重复 pulse 和左右 sweep。最后的 `TechnicalAudioAllowed` 顺序 guard 仍需在唯一 Editor 中 reload/compile 后重跑 QA；随后再重建 Player 并完成至少一段实际听感检查。
3. 既有 macOS Player 构建任务 `db48367e` 曾成功，输出为 `/Users/trusoegn/GitHub/Audio-VR/Audio-VR/Builds/AudioV0-macOS/AudioV0.app`；该 app 的文件时间早于本轮源码修改，不能证明包含本轮父节点修复。重建后仍需检查窗口、空间 cue 日志和实际听感，不能把桌面构建结果写成 Quest/HRTF 或 pilot 证据。
4. Quest 3、HRTF/SDK、耳机校准、真人可听性/舒适度、feasibility pilot、伦理、条件×路线冻结、样本精度和正式分析仍未关闭。

## 恢复顺序

1. 恢复已登录 Chrome 或提供项目 Git 认证，以上线项目为标准，上传 `main.tex`、`methods.tex` 和两种路线图，Compile。
2. 记录 Errors、Warnings、Info 和最终页数，下载最终 PDF；逐页查看 1–10，特别检查 p3 空白、p4 声音条件、p5 路线图、p6 截图、p9 技术图和参考文献是否裁切/重叠。
3. 回到唯一 Unity Editor，确认源码 reload；完成教程 Ready 后启动 `Verify Four Full Flights`。原始 JSONL 和 QA 目录保留，所有输入标记 synthetic。
4. 在唯一 Editor 中重建 `Builds/AudioV0-macOS/AudioV0.app`，检查窗口、空间 cue 日志和至少一段实际听感；不要把桌面结果升级为 Quest/HRTF/人体证据。
5. 用本文件替换为下一次日期化 handoff，明确 completed、pending、provisional、pilot-dependent。
