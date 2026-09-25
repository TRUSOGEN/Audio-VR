# Audio V0 Windows / Quest 3 迁移包修订交接

日期：2026-09-25。此记录修订 2026-09-24 的 Windows 迁移包布局，指出 ZIP 根目录原本没有项目文件夹封套；新版在 ZIP 内增加 Audio-VR/ 目录，让解压后可以明确找到 Unity 项目根目录。新版替代旧 ZIP，旧包只留作追溯。

## 当前交付

- 最新相关 Unity 交接：turn_cue_quest3_continuous_audio_handoff_20260924.md。
- 权威源工程：/Users/trusoegn/GitHub/Audio-VR/Audio-VR。
- Unity：6000.6.0f1；主场景：Assets/AudioV0.unity。
- 当前 Git 状态仍有 281 项未提交/未跟踪修改，包必须传送完整工作树，不能用远端 clone 代替。

新版迁移包：
- /Users/trusoegn/论文2/temp/windows-migration-20260925/Audio-VR-Windows-project-20260925.zip
- SHA-256：9a395fde397bde4864d8a9bd7ebd5c5d68ff0b1a9f853692e821596325774bea
- Windows 指南：/Users/trusoegn/论文2/temp/windows-migration-20260925/WINDOWS-Quest3-开始运行.md
- ZIP 单一顶层目录 Audio-VR/；其中含 .git、Assets、Packages、ProjectSettings、README.md、docs 和配置文件。
- 5,682 个文件；ZIP CRC 校验通过；Unity 主场景、manifest.json 与 ProjectVersion.txt 均在包内。
- 仍排除可重建的 Library、Mac Build、Logs、UserSettings 和本机 .agents。

## Windows 打开步骤

1. 在 Windows 右键 ZIP > 全部解压到例如 D:\\UnityProjects。
2. 进入解压目录下的 Audio-VR 文件夹；确认直接可见 Assets、Packages、ProjectSettings。
3. Unity Hub 用 Unity 6000.6.0f1 打开该 Audio-VR 目录，等待首次依赖解析和资源导入。
4. 需要 Quest APK 时装 Android Build Support、SDK/NDK、OpenJDK；需要 Windows Player 时装 Windows Build Support (IL2CPP)。
5. 项目含 Git URL 包，第一次解析需网络；失败时确认 Git for Windows 可用且能访问 GitHub。

## Quest 路线与阻塞

- Windows Meta Horizon Link PCVR 可串流运行，不需侧载 APK；具体 Link 配对和 Unity PCXR loader 尚未在头显上验证。
- 目前保存的 Standalone XR General Settings 的 loader 列表为空；Windows Unity 里需检查 Project Settings > XR Plug-in Management/OpenXR 设置后再期待 Play 进入 VR。
- Android APK 路线需要设备所有者账号开启 Developer Mode；用户当前被“联系设备所有者”提示阻塞。迁移项目不会变更 Quest 账号所有者。
- Windows Unity 导入、编译、PCVR Link、APK部署、Quest输入/音频/帧时间/日志与pilot 均待实测；既有桌面 synthetic QA 不升级成 HMD 或参与者证据。
