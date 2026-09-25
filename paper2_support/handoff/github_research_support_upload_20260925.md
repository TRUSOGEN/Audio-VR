# Audio V0 GitHub 研究支持资料上传交接

日期：2026-09-25（Australia/Sydney）。目标仓库：`https://github.com/TRUSOGEN/Audio-VR`。本次把 Mac 上的 Paper #2 支持资料交给 Windows 工程使用；用户在 Windows 上继续修改的 Unity 软件、声音播放、VR 设置和画面是当前工程版本。

## 已完成

- 2026-09-25 约 19:40–19:45 AEST，从 `/Users/trusoegn/论文2/` 复制近期 handoff、完整 `sound_design/` 候选库、分析工具和相关研究文档到 GitHub 仓库新增的 `paper2_support/`。
- 第一笔支持资料提交为 `50abb912fa23acca6e815f95ac4e7294d5d11ca7`，已通过 TRUSOGEN SSH 凭据推送到 `main`，并用 GitHub 远端回读确认。
- `paper2_support/README.md` 明确 Windows Unity 优先级和数据工具入口；`source_manifest_20260925.csv` 对每个上传内容文件记录来源修改时间、封包修改时间、快照时间、字节数和 SHA-256。声音库含 60 个 WAV/MP3 候选或原始导出，属于带日期的 Mac 快照。
- 数据服务副本新增 `--data-root`，可明确指向 Windows Unity 实际 `AudioV0` 数据目录；Windows `.cmd` 启动器已加入。Mac `.command` 副本改用相对路径寻找服务脚本。

## 范围与版本边界

本次只新增 `paper2_support/`，不修改仓库 `Assets/`、`Packages/`、`ProjectSettings/`、根 README 或 Unity 工程文档。远端在上传前仍停留于 2026-09-14 基线；因此远端 Unity 工程不能代表 Windows 9 月 24 日及之后的软件改动。`turn_cue_quest3_continuous_audio_handoff_20260924.md` 和声音清单只描述其日期对应的 Mac technical candidate；不能据此覆盖 Windows 声音文件或认定 Windows 当前间隔、空间定位及视觉设置。当前仍未取得 Windows/Quest 实机验证结果。

本次没有上传 `AudioV0_Data` 原始 JSONL、参与者资料、Mac 构建或 Unity 场景及资源文件。`handoff/windows_unity_quest3_migration_20260924.md` 保留作历史，9 月 25 日修订版优先。

## 校验

- 源清单 168 个内容文件的工作树与 Git 暂存 blob 均按 SHA-256/字节校验；Git 169 个新增路径还包括清单本身。
- `.gitattributes` 将声音分析的 MATLAB `.mat` 文件按二进制保留，避免 Unity 根规则把它当作文本换行转换。
- Python 工具静态编译通过。Mac 端只读查看器对 `/Users/trusoegn/AudioV0_Data` 返回 health/index，索引到 67 个 `technical_demo` 会话，首页 HTTP 200。
- 选定 2026-09-24 的技术日志导出成功：1 个 QA 会话、6,265 个事件、12 条通知、161 条视觉事件、3,913 条运动样本，`validation_errors=[]`。这验证导出器副本在 Mac 上的运行，不证明 Windows 路径、Windows Unity 当前日志字段或正式人类数据可用。

## Windows 下一步

从 GitHub 拉取 `paper2_support/` 后，用 Windows Unity 实际显示的日志根目录启动 `paper2_support/tools/start_data_viewer_windows.cmd`；对新生成的 Windows `technical_demo` 会话核对文件可见、版本字段、原始 JSONL、导出 `validation_report.json` 与图表。Windows 声音如已改动，记录新文件的修改时间、SHA-256、Unity clip 导入设置与 `spatialTurnCueVersion`，以 Windows 的当前文件为准。Quest 实听、校准和 pilot 仍需独立记录。
