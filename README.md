# Audio V0 / UAM Passenger VR

这是一个基于 Unity 的 UAM 乘客 VR 音频通知实验原型。项目包含运行场景、实验控制器、视觉副任务、通知识别、结构化 JSONL 日志，以及实验流程和研究方法文档。

## 打开项目

1. 安装与 `ProjectSettings/ProjectVersion.txt` 完全一致的 Unity Editor：`6000.6.0f1`。
2. 从 Git clone 后，在 Unity Hub 中选择项目根目录，等待 Unity 根据 `Assets`、`Packages` 和 `ProjectSettings` 重建本机 `Library`。
3. 打开 `Assets/AudioV0.unity`。当前主场景也是 Build Settings 的首个场景。
4. 首次打开先等待 Package Manager 完成解析；项目包含一个 Git URL 依赖 `com.besty.unity-skills`，需要网络访问 GitHub。
5. 先使用桌面模式完成一段 technical demo，再连接目标 OpenXR 头显进行 VR 验证。

## 重要边界

- `technical_demo` 日志只能证明软件链路，不能作为正式实验结果。
- 正式采集前仍需完成语音/非语音刺激映射、声学校准、条件与路线分配、HMD 实测、pilot 和数据导出验证。
- UnitySkills 的编辑器 relay 是开发辅助工具，不是运行实验的必要依赖；在 macOS 上应按该工具自己的 macOS 安装方式重新配置，不能复用 Windows `.exe` 路径。
- `Application.persistentDataPath` 会按操作系统生成数据目录，文档中的 Windows 路径只代表 Windows 示例。

## 文档入口

- `docs/audio-v0-handoff.md`：当前实现、验证边界和交接状态。
- `docs/audio-v0-flow-review.md`：完整流程评审、论文依据和 Unity 优化方案。
- `docs/research/audio-v0-flow-literature.md`：原始文献方法核验与迁移限制。
- `docs/audio-v0-gate-measurement.md`：G00–G12 测量与验收 gate。
- `docs/audio-v0-experiment-template.md`：场景复制、Profile、VR 和数据仓库说明。

## 版本控制

提交 `Assets`、`Packages`、`ProjectSettings`、`docs`、README 和所有 `.meta` 文件。`Library`、`Temp`、`Logs`、`Obj`、`UserSettings`、`.vs` 和 Plastic SCM 本机元数据不进入 Git；这些内容会在每台机器上重新生成或由本机工具维护。
