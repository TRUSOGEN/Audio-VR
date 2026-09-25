# ElevenLabs V4 语音候选：预告措辞与播报节奏

状态：2026-09-23 已通过现有 ElevenLabs 网页生成三条 raw MP3，并用 MATLAB 实际解码审计；尚未完成人耳试听、HMD 校准或人类筛选。V4 是工程候选，不是主实验 final。原文件未裁剪、未归一化、未做离线变速；旧 V3 保留供对照。

## 设计要求与生产决定

- Tram 9.23 15:52–16:13 认为 speech 略快，希望接近常规机舱广播：在同 voice/model 下将 provider speed UI 从 V3 的 `1.00` 调为 `0.90`，重新生成三条。不将这个设置直接解释为真实词速精确降低 10%。
- 旧 T01 输入文字 “Cruising altitude reached” 表示已经达到巡航，与 transition 前通知不一致：V4 改为 “We will enter cruise shortly.”。此处记录输入文案，不代表已通过听者或自动语音识别核对实际发音。
- 保留共同开头、固定 voice identity 和共同设置；不加入紧急、安全、额外解释或空间声内容。T01 同时改变了文案与 speed，不能从 V3/V4 对照中分离两者效果。

| 字段 | V4 实际生产记录 |
| --- | --- |
| Provider / model | ElevenLabs / Eleven Multilingual v2 |
| Voice identity | `Paper2_AU_Female_Broadcast_V3` |
| Voice ID | `to2uGLddpS3srynUCVAr` |
| Speed | UI `0.90`；provider 下载文件名编码 `sp89`，同时保留两个记录 |
| Stability / similarity / style | `0.50` / `0.75` / `0.00` |
| Speaker boost / audio effects | enabled / disabled |
| 生成 | 三次 generation；账户 credits 9361→9240，共 121；本轮没有购买 |
| 原始证据 | `production_record.json` 保留输入文字、provider filename、raw path、SHA-256 和设置 |

| Target | V3 原始输入文案 | V4 原始输入文案 |
| --- | --- | --- |
| T01 entering cruise | Flight update. Cruising altitude reached. | Flight update. We will enter cruise shortly. |
| T02 beginning descent | Flight update. Descent will begin shortly. | Flight update. Descent will begin shortly. |
| T03 preparing to land | Flight update. Prepare for landing. | Flight update. Prepare for landing. |

## 实际数字审计

MATLAB `24.2.0.3070828 (R2024b) Update 7` 的 `audioread` 已解码六条新旧 MP3；均为 mono、44,100 Hz，所有解码样本有限且可用。读取前后的 raw SHA-256 一致。下表数值来自实际解码样本，不从 speed 设置推算。

| Target / version | 解码时长 s | 整文件 RMS | Peak | 首部低能量 s | 尾部低能量 s |
| --- | ---: | ---: | ---: | ---: | ---: |
| T01 V3 | 2.351020 | 0.101338 | 0.454041 | 0.010 | 0.091020 |
| T01 V4 | 2.638367 | 0.123554 | 0.536469 | 0.010 | 0.178367 |
| T02 V3 | 2.507755 | 0.107693 | 0.490448 | 0.010 | 0.087755 |
| T02 V4 | 2.925714 | 0.114109 | 0.509552 | 0.010 | 0.235714 |
| T03 V3 | 2.115918 | 0.110562 | 0.552368 | 0.010 | 0.105918 |
| T03 V4 | 2.351020 | 0.109596 | 0.522461 | 0.050 | 0.191020 |

首尾“低能量”按不重叠的 10 ms 窗、绝对 RMS 阈值 −50 dBFS 定义：使用首个/最后一个超过阈值的窗边界计算。它是明确阈值下的数字估计，不是语言、感知或实际耳侧 onset/offset，不能直接用于声学 RT 校正。阈值以下也不一定是无声，不据此自动 trim。

六条的解码 `|x| ≥ 1` 样本数和 `|x| ≥ 0.999` 样本数均为 0；这不排除生成前的 clipping、压缩失真或其他伪影。三张对照图已实际检查，波形与能量区间正常导出，没有凭图作可懂度或舒适度判断。相同 voice/settings 不保证三声响度相等，亦未将 raw speech 的数字电平匹配至 non-speech。

旧 V3 README 的时长是此前系统元数据值，本轮按 `decoded_samples / 44100` 得到更细的解码时长，差异至多两个样本左右；两个记录均保留，不重写历史文件。`waveform_audit.json` 同时保存 `audioinfo_duration_s`、解码长度、RMS/peak（含 dBFS）、full-scale 计数、DC、低能量区间和 SHA-256。

## 文件与重现

- `raw/`：三条原始 V4 MP3；没有修改 provider exports。
- `audition.html`：逐 target 的 V3/V4 对照，含原文与 speed，无自动播放。
- `waveform_audit.json` / `.csv`：六条实际解码审计。
- `T01_waveform_comparison.png` 至 `T03_waveform_comparison.png`：统一横纵轴的实际波形/10 ms 能量图。
- `../../temp/speech_v4_20260923/matlab_speech_audit.log`：MATLAB exit 0，含两条非阻断 Java `sun.awt.X11` warning。
- 同一 temp 目录的 `*_decoded_float64.bin` 与 `independent_numeric_verification.json`：Python 独立复算 RMS/peak/首尾区间，并重新核查六条原文件 hash，6/6 通过；这是同一次 MATLAB 解码输出的独立计算，不是第二个解码器验证。

```sh
/Applications/MATLAB_R2024b.app/bin/matlab -batch "addpath('/Users/trusoegn/论文2/sound_design/elevenlabs_broadcast_v4_candidate'); audit_speech_exports" -logfile '/Users/trusoegn/论文2/temp/speech_v4_20260923/matlab_speech_audit.log'
python3 build_audition_and_verify.py
```

下一步：用相同播放链核对实际发音/文案、同一说话人身份、自然语速、句间停顿、结尾与伪影，再检查 quiet/noise 可懂度；记录 calibration 与候选选择后进入完整 feasibility pilot。Unity 是否已接入以工程刺激 manifest 和实际 runtime 日志为准；本目录的数字审计不证明声音实际播放到耳侧。
