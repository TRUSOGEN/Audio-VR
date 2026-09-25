# Turn cue candidates V1

这是给研究者试听的左右转提示音候选包。当前仅 `airy_breath_soft` 作为 Unity technical candidate 接入，其他 family 仍保留在试听包中；任何 family 都不代表正式研究刺激已经冻结。

## 候选类型

| Family | 听感方向 | 设计重点 |
| --- | --- | --- |
| `airy_glass` | 空灵、清亮 | 平滑上行、轻微高频 shimmer |
| `soft_bloom` | 温和、圆润 | 较暖的五度感和慢起音 |
| `breath_chime` | 轻呼吸感 | 很低的空气感成分，避免宽带噪声 |
| `airy_breath_soft` | 空灵与呼吸感的柔和融合 | 降低高频谐波、放慢起音、轻微呼吸调制 |
| `suspended_fifth` | 稳定、舒缓 | 悬置音程和极轻 vibrato |

每个 family 都有 `turn_left.wav` 与 `turn_right.wav`。两者内容刻意相同，以免用音色差异代替空间方向；左右方向应由 Unity 的 mono `AudioSource` 空间位置承担。除 `airy_breath_soft` 为 1.30 s 以支持 Unity 中 1.00 s 的连续转弯 cue 间隔外，其余 family 仍为 0.72 s；全部为 44.1 kHz、mono、PCM16，峰值上限约 0.78。数字审计不能替代 Quest 3 耳侧响度、HRTF、舒适度和可听性检查。

## 试听

在此目录启动只读本地服务器：

```sh
python3 -m http.server 8092 --bind 127.0.0.1 --directory /Users/trusoegn/论文2/sound_design/turn_cue_candidates_v1
```

然后打开 `http://127.0.0.1:8092/audition.html`。页面可以逐个试听五类声音，并在左/右位置播放同一候选。也可以直接打开各 family 下的 WAV 文件。

## 选择边界

等你试听并选定一个 family 后，才把该 family 的一个 mono WAV 接入 Unity `turnCues`。在此之前，Unity 仍保留当前运行时生成的临时宽带 cue，避免把未听过的候选误当作已选方案。
