# Audio V0 软件交付与使用手册

日期：2026-09-24。范围：Unity 桌面技术交付、使用手册、可复现验证；不含 Overleaf、Scoping Review 或正式人类研究结果。

## 交付入口

- 权威工程：`/Users/trusoegn/GitHub/Audio-VR/Audio-VR`，Unity `6000.6.0f1`，场景 `Assets/AudioV0.unity`。
- 使用手册：`/Users/trusoegn/GitHub/Audio-VR/Audio-VR/docs/audio-v0-user-manual.md`。
- macOS 应用目标：工程 `Builds/AudioV0-macOS/AudioV0.app`；最终构建结果见下方验证节。
- 数据中心：双击 `/Users/trusoegn/论文2/tools/打开 Audio V0 数据中心.command`，访问 `http://127.0.0.1:8091/`。
- 原始数据：`/Users/trusoegn/AudioV0_Data/technical_demo/`。所有本次自动响应均为 synthetic，不能作为参与者数据。

本记录替代 `paper2_audio_v0_status_20260924.md` 中“编译仍失败、v5 尚未运行”的软件状态；原论文图表、线上同步和研究 gate 不因本次修复关闭。

## 修复内容

1. 降低巡航高度后，真实建筑触发自动净空修补，导致河道路线约224.78秒并出现额外升降。将必要净空直接写入路线：第二条巡航160 m，其余130 m；后两条按各自走廊调整，不保留不符合实际建筑的强制镜像断言。
2. 第二条在 T01 巡航边界前完成爬升，修复“进入巡航后仍升高”的语义冲突。路线回归覆盖整个平台高度稳定。
3. 终场截图前检查 logger 是否仍开放，避免 session_end 后写 figure_capture 导致 LOG_WRITE_FAILED。
4. 更新弃用的 FindObjectsByType 调用；强制刷新后重新核对编译。
5. 四航程验收器按实际 cruise/turn 阶段截图，不再依赖旧180 m高度；每轮输出独立目录，避免覆盖旧证据。
6. 旧独立片段验收器在关闭 Domain Reload 后残留 active，真实栈证据显示它会恢复用户刚暂停的完整航程。修复退出 Play 时清理状态，并要求 active technical excerpt 与 Session 实际绑定一致，防止干预完整航程。
7. 修正参与者呈现日志：完整航程记录 live_route，旧片段模式才记录 map=hidden。
8. 新增中文使用手册，更新 README、实验模板与论文2的执行/数据手册入口。

## 当前行为与参数

快速教程 → 明确 START FOUR FLIGHTS → 四个地面到地面完整航程 → 段间手动继续 → 感谢页。每段 T01/T02/T03 合计12次识别；每次有效选择后1–7信心。GO/WAIT与回答区域独立；共享左右空间 cue 不计入识别分母。

路线版本 `full_city_journeys_v5_210s_candidate`，速度上限18 m/s、爬升5 m/s、下降4 m/s、切向加速度1 m/s²、曲率限速1.2 m/s²。实际场景运动表：

| 路线 | 时长 s | 巡航平台 m |
| --- | ---: | ---: |
| civic_greenway | 201.849673 | 130 |
| river_corridor | 206.133982 | 160 |
| west_loop | 192.260056 | 130 |
| east_loop | 192.174623 | 130 |

这些是软件候选值。路线互异、非严格镜像、非等时；条件与路线 counterbalance 仍须正式冻结。上下文、地图、阶段顺序可能帮助识别，不能主张 sound-only identification。

## 验证与证据

证据根目录：`/Users/trusoegn/GitHub/Audio-VR/Audio-VR/temp/20260924-final-delivery/`。

- `cruise-regression-before.json` 复现第二条已标 cruise 后仍上升；`route-tests-after.json` 路线专项9/9通过。
- 根目录的 `runtime_status.json` / `runtime_audit.json` 是本轮早期完整四段正常时钟基线：四段完成，12通知/12转场、11识别/11信心、1故意超时，5869事件，3916运动样本，0error、0重复event ID。此基线第二条在控制点3仍130 m，不能当成后来160 m修正的同版本整场证据。
- 基线原日志：`session_20260923_193431_826_fb4821d1040544c5a3224f4f5b033d55`。导出 `export-baseline/validation_report.json` 为 qa_only，validation_errors为空；四段视觉汇总重建均匹配。
- `run_20260924_005347/geometry_audit.json` 是最终路线实际场景：3962采样、24 m半径、≤4 m间距、0碰撞、0自动修补，四条均≤210 s。
- `run_20260924_005347/` 正常时钟增量复验完整完成前两段（包括最终河道几何），第三段约29秒时退出Play；原日志 `session_20260924_005315_639_84a2e23510f5411ea7b809d599ffaba0`。不能把此目录写成四段完成。其暂停被旧片段验收器自动恢复，修复后需要新的独立验证。
- `audit_delivery.py` 重算原始JSONL计数、连续事件序号、重复ID、通知提前量与空间方向，保留原日志快照与SHA-256；不修改源数据。
- 已目视检查真实相机的识别页与最终河道巡航页；新截图覆盖巡航和左右转，不再使用旧高度门槛。

### 最终软件流程验收

- `tests-final.json`：最终项目专项 EditMode 78/78 passed，0 failed；不是把历史全量963项改写为本次结果。
- `run_20260924_011359/`：最终运行代码正常 timeScale=1，四段全部完成，799.061 s，37张真实相机截图。日志 `session_20260924_011325_716_70328e5cab1444e7b8fb1525fecd2a58`，5850事件、3864运动样本、12通知/12转场、11识别/11信心、1故意缺答、13空间cue，0应用error、0重复ID，事件序号连续，软件通知提前量17.959–17.997 s。
- `pause-regression.json`：主动注入旧片段QA残留，通过真实参与者按钮暂停3秒，位置/route clock保持不变，随后恢复；整场pause/resume各一次，0 input_rejected。
- `export-final/validation_report.json`：qa_only、validation_errors为空，四段视觉汇总与原事件重建全部一致。
- 终场截图和原始JSONL均存在；退出Play后移除临时自动runner，复现脚本留在temp，不保留后台自动点击入口。
- 编译刷新记录：2026-09-24T01:32:29.9438370Z，0 errors / 0 warnings。Console本轮曾出现1条记录，清理/重载后查询为空；整场原始应用日志0error，不将该Console计数声称为始终为零。

### 独立应用修订

初次 macOS arm64 构建0 errors / 349 warnings，缓存后的增量构建0 errors / 2 warnings。用户在独立应用中指出全屏、标题My project和灰色窗外；该版不能视为视觉验收通过。日志确认实拍天空未进入Player而回退，运行时透明玻璃缺少打包变体引用；进一步核对URP导入器发现Premultiply模式会清掉_ALPHAPREMULTIPLY_ON，而运行时又启用该关键字。修正为Alpha + Preserve Specular，让资源与运行时关键字一致。

修复：productName=Audio V0、默认1600×900窗口，启动器额外覆盖旧fullscreen偏好；构建前自动生成并保留Resources中的天空、玻璃、HUD玻璃及透明材质变体。最终Player验收已通过：应用标题 Audio V0、1600×900窗口，QUICK START正常显示；实际前窗及侧窗可见建筑、道路和实拍云层，灰色风挡已消失。`player-release-smoke.json`对应新会话 `session_20260924_014123_287_80567343de3e4ea393257ec75b82f27f`，记录 `polyhaven_kloofendal_partly_cloudy_4k`，无应用error。

`build-report-release.json`：Succeeded、0 errors、350 warnings、1,386,251,343 bytes、build GUID `b434d49a80f245768d73143c71cf5542`。警告包括Unity包/Metal shader与未配置的Unity Pipeline，不声称零warning。Player仍提示未使用的景深/Panini后处理pass被裁剪；已检查实际所需画面正常，未把这些pass写成已启用。应用为arm64本机包，未公证。

最后完整四航程在Editor验证；之后仅修正玻璃材质模式、材质打包引用及窗口/应用名称，并在最终独立Player做启动、渲染和日志冒烟。没有声称最终独立Player或Quest又完整跑过四段。

完整交付目录：`/Users/trusoegn/GitHub/Audio-VR/Audio-VR/Builds/AudioV0-macOS/`，含AudioV0.app、窗口模式启动器、使用手册、开始使用和第三方来源。`release-source-sha256.json`绑定本次源码及材质。所有临时Editor runner/bridge已移出Assets，Editor停在非Play状态；最终应用留在QUICK START供用户使用。

## 未关闭的外部 gate

Quest 3输入/追踪/可读性/帧时间、耳侧声学校准、真人声音筛选、左右 cue 可听性与映射、feasibility pilot、伦理、最终路线条件分配与参数/样本精度/分析冻结仍未完成。当前机器只有MacStandalone与WebGL模块，没有Android构建模块；本次不提供已验证Quest APK。

保留用户原有dirty tree与历史证据，未commit/reset，未修改线上Overleaf或Scoping Review。

最终脚本编译：2026-09-24T01:37:34.4094390Z，errors=0，warnings=0。
