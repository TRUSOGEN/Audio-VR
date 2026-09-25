# Paper #2 Audio V0 研究支持资料

初始上传快照：2026-09-25 19:40 AEST（Australia/Sydney），从 Mac 上的 `/Users/trusoegn/论文2/` 复制。`source_manifest_20260925.csv` 记录该初始快照的源修改时间、大小与 SHA-256；后续加入的 Windows handoff 和工具不属于这份历史清单。Git 不保存文件修改时间，因此按各自来源记录追溯时间，不能把 GitHub 上传日当成声音定稿日。

## Windows 工程优先级

Windows 电脑上继续修改的 Unity 工程是当前软件版本。这里的 2026-09-24 Mac 交接、声音候选和技术 QA 是带日期的历史快照，不能用它们覆盖 Windows 的 `Assets/`、`Packages/`、`ProjectSettings/`、画面、VR 设置或实际播放声音。本次 GitHub 更新只新增 `paper2_support/`；不声称已核对 Windows 9 月 24 日及其后的工程内容。

先读 `handoff/windows_unity_quest3_migration_20260925.md`、`handoff/turn_cue_file_inventory_and_editing_20260925.md` 和 `handoff/turn_cue_quest3_continuous_audio_handoff_20260924.md`。其中的 Unity 参数与声音 SHA-256 只适用于对应日期的 Mac 版本。`windows_unity_quest3_migration_20260924.md` 是被 9 月 25 日修订的历史记录。其他 handoff 也按文件日期与适用版本阅读；Mac 绝对路径是原始审计线索，不是 Windows 路径。

## 内容

- `handoff/`：近期 Audio Experiment、Windows 迁移、声音、数据与论文交接；旧记录保留原文。
- `sound_design/`：Mac 快照里的完整声音设计库，包括 60 个 WAV/MP3、候选生成脚本、试听页、分析和清单。候选不等于正式冻结刺激。Windows 已更新过的声音以 Windows 实际文件及新 checksum 为准。
- `tools/`：JSONL 导出、只读数据中心、审阅图件及历史路线图辅助脚本。未上传任何原始会话数据。
- `docs/`：操作、数据字典、测量、校准、声音筛选和 protocol gate 文件；其中版本叙述以原文日期为准。

## Windows 数据工具

先在 Windows Unity 运行界面或 `session_manifest.json` 确认实际 `AudioV0` 数据根目录，目录下应有 `technical_demo`、`pilot` 或 `participant_run` 子目录。当前 Windows 工作区 `Audio-VR-Windows-project-20260924` 的 `EventLogger.GetDataRootPath()` 明确返回 `E:\Audio-paper\实验数据`；此工作区以该源码路径为准。其他项目副本仍需按其实际源码和会话清单确认路径。Python 需要 3.9 或更新版本；图件导出另需 `matplotlib`。

在命令提示符中，从仓库根目录运行：

```bat
paper2_support\tools\start_data_viewer_windows.cmd "E:\Audio-paper\实验数据"
```

启动后在浏览器打开 `http://127.0.0.1:8091/`。HTML 页面由本机只读数据服务提供；直接双击 `audio_v0_log_viewer.html` 不会连接 Unity 数据。服务只监听本机并只读已有 JSONL；类别分开审阅。当前工作区也提供 PowerShell 结构概览：

```powershell
pwsh -NoProfile -File "paper2_support\tools\查看会话.ps1" -DataRoot "E:\Audio-paper\实验数据"
```

该脚本列出会话、事件数、运动采样数、无效行和结束标记，不生成论文统计。正式 CSV 导出使用：

```bat
py -3 paper2_support\tools\export_audio_v0_events.py "C:\实际路径\AudioV0" "C:\新的空输出目录"
```

仅技术 QA 才加 `--allow-technical`。查看 `validation_report.json` 和 `sessions.csv` 后再使用 CSV。`compose_full_flight_views.py` 是依赖 2026-09-23 Mac 截图目录的历史制图脚本，不是 Windows 数据中心入口。macOS 可用 `tools/打开 Audio V0 数据中心.command`，该副本使用自身所在目录寻找服务脚本。

如需替换 Unity 声音，先核对 Windows 工程实际资源、波形、时长、导入设置与版本字段，并记录新的时间和 SHA-256；不要把本目录候选直接复制进 `Assets/` 后仍沿用旧版本标签。
