# Audio V0 Windows / Quest 3 迁移交接

日期：2026-09-24。目标：保留当前 Unity 工作树并迁到 Windows，完成 Windows Editor/Quest 3 technical run；不把迁移计划当成 Windows 或 HMD 验收。

## 交接入口与当前版本

- 最新关联交接：turn_cue_quest3_continuous_audio_handoff_20260924.md（2026-09-24 22:13）；软件交付依据：unity_software_delivery_20260924.md。
- 权威 Unity 工程：/Users/trusoegn/GitHub/Audio-VR/Audio-VR。
- Unity Editor：6000.6.0f1；首场景：Assets/AudioV0.unity。
- 工程含 XR Interaction Toolkit、XR Management、OpenXR 包；ProjectVersion/Packages/Assets 与所有 .meta 已纳入当前工作副本。
- 当前 Mac 只安装 Mac Standalone 与 WebGL 构建模块；无 Android Build Support，因此没有 Quest APK。
- 当前 Quest 的设备所有者账号不是用户账号；手机 App 报“需要联系设备所有者”。开发者 Team 建在用户当前账号下，不能替代设备所有者权限。
- EventLogger.GetDataRootPath() 在 macOS 使用用户目录下 AudioV0_Data；其他平台使用 Application.persistentDataPath/AudioV0。

## 可迁移包

文件：/Users/trusoegn/论文2/temp/windows-migration-20260924/Audio-VR-Windows-project-20260924.zip

SHA-256：086f79671fa188c6bf438a79c7f5d0ba6da15001d5ec48218b6867a246f379a8

压缩包约 992 MB，包含当前 .git、Assets、Packages、ProjectSettings、docs、README 和配置文件。它保留当前未提交和未跟踪的工作树文件；Git 状态共有 281 行改动，因此直接从远端 git clone 会缺少这些内容。

排除可在新电脑重建或不能在 Windows 使用的 Library、Builds/AudioV0-macOS、Logs、UserSettings、.agents 与 macOS Finder 文件。Unity 会在 Windows 首次导入时重建 Library。文件级清单与 ZIP CRC 校验通过；压缩包与源文件清单完全一致（5,682 个文件）；Windows 保留字/非法字符、大小写冲突与符号链接检查均为 0。

## Windows 初次打开顺序

1. 将 ZIP 和本 handoff 通过外接 SSD（建议 exFAT）或可靠云盘传到 Windows。
2. 解压到短英文路径，例如 D:\UnityProjects\Audio-VR；不要从压缩包内直接打开，也不要放在 OneDrive 同步目录。
3. 用 Unity Hub 安装完全匹配的 Unity 6000.6.0f1。目标为 Quest APK 时同时安装 Android Build Support、Android SDK & NDK Tools、OpenJDK；若要产出 Windows .exe，另装 Windows Build Support (IL2CPP)。
4. Unity Hub > Add/Open 选择解压后同时含 Assets、Packages、ProjectSettings 的项目根目录，打开后等待首次 Package Manager 解析和完整资源导入。清单含 Git URL 包 com.besty.unity-skills，需要网络；若解析失败，先安装 Git for Windows、确认可访问 GitHub，再重新解析。
5. 打开 Assets/AudioV0.unity，检查 Console 完成导入后是否有编译错误，再做桌面或头显检查。不要把导入成功等同于 Quest 实机验证。

## Quest 运行方式

### Windows PCVR：经 Meta Horizon Link 串流

- Meta Horizon Link 电脑端仅支持 Windows；它可让 Windows Unity/PCVR 内容串流到头显，不需要把 APK 侧载到头显。用户目前被 Developer Mode 所有者限制时，可优先尝试此路线，但 Link 的设备配对和 PCVR 运行仍待实际确认。
- 安装 Meta Horizon Link（Windows），USB-C 数据线连接 Quest，在头显中进入 Quick Settings 并打开 Link。
- Unity 中检查 Project Settings > XR Plug-in Management 的 Standalone/OpenXR loader 与控制器 profile；当前保存的 XR General Settings 有 Standalone 设置资产，但 loader 列表为空，不能预先声称 Editor Play 已配置为 VR。
- 先由 Unity Editor Play 做短时技术检查，再验证画面、控制器射线与 trigger 点击、左右声音 cue 和日志。若需要独立 Windows 应用，安装 Windows Build Support 后打 Player，经 Link 启动。
- Windows Player 数据路径按 Unity persistentDataPath 规则落在 %USERPROFILE%\AppData\LocalLow\DefaultCompany\Audio V0\AudioV0\；需要在真实运行后检查日志文件是否生成。

### Quest Standalone：构建 Android APK

- 需在 Meta Horizon App 中由 Quest 设备所有者打开 Developer Mode；手机 USB 调试授权后，Windows 可通过 Unity Build And Run/ADB 部署。
- 由于当前设备所有者不是用户，需由该所有者账号加入/接受开发者 Team、满足验证条件并开启开发者模式；若用户确实拥有硬件但无法联系原所有者，再评估恢复出厂后由用户重新设置。恢复出厂会清除头显本地数据和账号设置，不要未经备份执行。
- Windows 上需 Android Build Support（含 SDK/NDK/OpenJDK）以及 Meta 的 Windows ADB 驱动。Unity 内核验 Android 的 XR Plug-in Management/OpenXR 设置、Quest controller profile 和场景输入。
- 打开头显并允许 USB debugging，再 Build And Run。安装成功只证明部署；随后单独检查启动、射线、trigger、可读性、帧时间、音频方向/响度与日志写入。

## 证据边界与未完成 Gate

- 本次完成了源文件审计、Windows 路径可移植性检查和 ZIP 完整性校验；没有 Windows 主机、Unity Windows Editor、Link 实连或 Quest APK 可供本次实测。
- 迁移后首次导入/编译、Windows PCVR Link、Quest APK/Developer Mode、HMD 输入与声音、帧时间、数据导出均未完成。
- 所有既有完整航程 QA 均为 synthetic technical evidence；不能改写成 Quest/HMD、pilot 或参与者证据。
- 后续顺序：Windows 解压/导入 → package/compile 检查 → 选择 Link PCVR 或 standalone APK → Quest 实机短流程 → 检查 JSONL 及输出目录 → 再进入声音校准/可行性 pilot gate。
