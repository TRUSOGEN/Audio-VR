# Audio V0 可见本地数据目录迁移交接

日期：2026-09-21（Sydney）

## 已完成

- macOS Unity 的 `EventLogger.GetDataRootPath()` 已改为 `/Users/trusoegn/AudioV0_Data/`；数据中心与 Unity 的“打开数据目录”共用该根目录。
- 原隐藏目录 `/Users/trusoegn/Library/Application Support/DefaultCompany/My project/AudioV0/` 未删除、未改写，作为历史备份保留。
- 已按原目录结构复制并逐文件 SHA-256 校验：358 个文件、178 个 `technical_demo/session_*`，清单在 `/Users/trusoegn/论文2/temp/data_folder_20260921/migration_manifest.json`。
- 数据服务 `/Users/trusoegn/论文2/tools/audio_v0_data_server.py` 与启动脚本 `打开 Audio V0 数据中心.command` 均已改为读取可见目录，并会自动创建目录。
- 数据中心仍只提供本机只读查看和导出；不会上传或修改日志。

## 实际验证与边界

- Python 语法及 zsh 启动脚本语法检查通过；本次修改的 Unity 文件 `git diff --check` 通过。
- 数据服务已重启，`/api/health` 返回新目录；`/api/sessions` 返回 179 个 session（178 个历史 session + 1 个本轮 QA）。所有 API 日志内容与新目录文件逐项一致。
- Unity 6000.6.0f1 batchmode 重编译并执行真实 `EventLogger` 初始化、事件写入和关闭，退出码 0；路径与 `AudioV0DataRepository.RootPath` 一致，JSONL 含 session_end，manifest 存在。QA participant ID 为 `TECHNICAL_DATA_PATH_QA`，用途明确为存储验证，不是被试数据。
- 初次受限启动因 `Failed to initialise UDS client in the Editor` 退出；环境外重试成功。编译日志有 4 条现有 `AudioV0InteractionAudit.cs` 弃用 API warning（日志重复打印），未见 C# error；不声称零 warning。
- 本次是实际 logger 集成写入验证，没有重新跑四段游戏或 Quest。浏览器控制的自动审批曾超时，未新增浏览器截图；HTTP 接口读取验证已完成。
- 临时验证入口已从 Assets/Editor 移除，复现源码、Unity 原始日志、写入结果、API 核验和迁移清单均保留在 `/Users/trusoegn/论文2/temp/data_folder_20260921/`。
- 最终再次核对迁移清单，旧目录与新目录的 358 个原文件仍全部匹配；旧 JSONL 内历史路径未回写。

## 当前入口

- 文件夹：`/Users/trusoegn/AudioV0_Data/`
- 查看器：`http://127.0.0.1:8091/`
- 双击：`/Users/trusoegn/论文2/tools/打开 Audio V0 数据中心.command`

## 后续使用

重新打开 Unity 正常运行即可，新 session 直接写入可见目录。macOS Editor/桌面构建已改路径；其他平台（包括 Quest/Android）继续使用原有持久化目录。历史研究参数与游戏/HMD 待验证项仍见 `game_interaction_data_validation_20260921.md`。
