"""复核 MATLAB 导出的实际 PCM 文件，生成无需服务器的研究者试听页面。

使用 Python 标准库独立检查 WAV 格式、数字值和 checksum，不合成或处理音频。
审计结果保存在项目 temp，试听页仅呈现候选，不播放、上传或收集参与者资料。
"""

from __future__ import annotations

import array
import hashlib
import html
import json
import math
import sys
import wave
from pathlib import Path


ROOT = Path(__file__).resolve().parent
AUDIT_ROOT = ROOT.parents[1] / "temp" / "sound_matlab_20260923"
FAMILIES = {
    "M1_two_pulse": ("M1 · 两个柔和音组", "优先技术接入候选；尚未进行人耳筛选"),
    "M2_three_pulse": ("M2 · 三个柔和音组", "T01/T02 与 M1 完全相同；只更换 T03 节律"),
    "reference_C0_1p35s": ("C0 · 旧版 reference", "历史文件原字节保留；与新版本比较包含多个设计变化"),
}
TARGETS = {
    "T01_cruise_entry": "T01 · Entering cruise",
    "T02_descent_begin": "T02 · Beginning descent",
    "T03_landing_preparation": "T03 · Preparing to land",
}


def verify_pcm(row: dict) -> dict:
    """从 PCM16 帧独立复算数字值，并与 MATLAB 的 WAV 回读结果核对。"""
    path = ROOT / row["relative_path"]
    with wave.open(str(path), "rb") as source:
        channels, width, sample_rate, frame_count, codec, _ = source.getparams()
        assert (channels, width, sample_rate, codec) == (1, 2, 44100, "NONE")
        raw = source.readframes(frame_count)
    integers = array.array("h", raw)
    if sys.byteorder != "little":
        integers.byteswap()
    values = [sample / 32768.0 for sample in integers]
    rms = math.sqrt(math.fsum(sample * sample for sample in values) / frame_count)
    peak = max(abs(sample) for sample in values)
    digest = hashlib.sha256(path.read_bytes()).hexdigest()
    assert digest == row["sha256"], f"Checksum mismatch: {path}"
    assert frame_count == row["samples"]
    assert abs(rms - row["digital_rms"]) < 1e-12
    assert abs(peak - row["digital_peak"]) < 1e-12
    assert peak <= 0.75
    if row["family"].startswith("M"):
        assert frame_count == 79380 and values[0] == values[-1] == 0
        assert abs(rms - 0.15) < 1e-4
    else:
        original = ROOT.parent / "contour_soft_screening_v1" / "C0_current_contour" / path.name
        assert hashlib.sha256(original.read_bytes()).hexdigest() == digest
    return {"path": row["relative_path"], "sha256": digest, "frames": frame_count,
            "digital_rms": rms, "digital_peak": peak, "verified": True}


def render_page(metadata: dict) -> str:
    """以原生音频控件和真实审计值构造静态、可离线打开的试听页面。"""
    sections = []
    for family, (name, description) in FAMILIES.items():
        cards = []
        for row in metadata["records"]:
            if row["family"] != family:
                continue
            src = html.escape(row["relative_path"], quote=True)
            cards.append(f'''<article>
              <h3>{TARGETS[row["target"]]}</h3>
              <audio controls preload="none" src="{src}">浏览器不支持 audio，请下载 WAV。</audio>
              <dl><dt>文件时长</dt><dd>{row["duration_s"]:.2f} s</dd>
              <dt>数字 RMS / peak</dt><dd>{row["digital_rms"]:.4f} / {row["digital_peak"]:.4f}</dd>
              <dt>95% 能量跨度</dt><dd>{row["energy_95_span_s"]:.3f} s</dd></dl>
              <a href="{src}" download>下载 WAV</a>
              <details><summary>SHA-256</summary><code>{row["sha256"]}</code></details>
              </article>''')
        sections.append(f'''<section><h2>{name}</h2><p>{description}</p>
          <div class="cards">{"".join(cards)}</div>
          <details><summary>查看三声波形、FFT 与短时频谱</summary>
          <a href="{family}_audit.png"><img src="{family}_audit.png" alt="{name}：三声实际 WAV 的数字审计图" loading="lazy"></a></details>
          </section>''')
    return '''<!doctype html><html lang="zh-CN"><meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>Paper #2 · 9.23 MATLAB 声音候选试听</title>
    <style>body{font:16px/1.65 system-ui,sans-serif;max-width:1180px;margin:40px auto;padding:0 22px;color:#18333b;background:#f6f9f8}
    h1{font-size:30px;line-height:1.3}h2{font-size:23px;margin-bottom:5px}h3{font-size:17px;margin-top:0}
    .status{display:inline-block;padding:3px 10px;border-radius:5px;background:#e7ede4;font-weight:600}
    section{margin:32px 0;padding:24px;background:#fff;border:1px solid #d8e1de;border-radius:12px}
    .cards{display:grid;grid-template-columns:repeat(3,minmax(0,1fr));gap:18px;margin:20px 0}
    article{padding:16px;background:#f3f6f5;border-radius:8px}audio{display:block;width:100%;margin:18px 0}
    dl{display:grid;grid-template-columns:1fr 1fr;gap:6px;font-size:14px}dt{color:#536a6c}dd{margin:0;text-align:right}
    a{color:#185f68}details{margin-top:14px}summary{cursor:pointer}code{display:block;overflow-wrap:anywhere;font-size:12px}
    img{max-width:100%;margin-top:16px}li{margin:6px 0}.note{border-left:3px solid #71876d;padding-left:16px}
    @media(max-width:820px){.cards{grid-template-columns:1fr}body{margin:24px auto}section{padding:18px}}</style>
    <header><span class="status">工程候选 · 人耳筛选待做</span><h1>同一家族，重新区分下降与着陆准备</h1>
    <p>依据 9 月 23 日反馈，用 MATLAB 实际合成两套新候选：三声均为 1.80 秒，共享柔和谐波音色；T03 用慢速等音音组与 T02 连续下降轮廓区分。</p>
    <p class="note">M1 优先用于技术接入；这不代表已证明它更容易辨识。M1/M2 只有 T03 不同，旧 C0 与新版本则有多项变化。相同数字 RMS 不等于相同主观响度；T03 的瞬时峰值高于 T01/T02，需在实际播放链中检查。</p>
    <p><a href="README.md">设计理由与流程</a> · <a href="candidate_analysis.json">完整数字审计</a> · <a href="candidate_analysis.csv">CSV</a> · <a href="generate_tram_revision.m">MATLAB 生成器</a></p></header>
    ''' + "".join(sections) + '''
    <section><h2>试听时记录什么</h2><ul>
    <li>固定同一播放设备和系统音量；先学习三个标签，再检查 T02/T03 是否混淆，记录重听次数与具体问题。</li>
    <li>比较 T03 两组/三组：持续感、是否显得急促或像警报、是否仍与 T01/T02 属于同一家族。</li>
    <li>记录断裂、click、粗糙感、过度打断、噪声条件可听性；数字波形通过不等于这些听感已通过。</li>
    <li>当前页面是有标签的研究者试听，不提供随机化参与者实验或识别率结果；HMD 校准、独立形成性筛选和完整 pilot 仍待完成。</li>
    </ul><p>本页无自动播放，不上传或保存个人资料；点击一条新音频会暂停其他音频。</p></section>
    <script>document.querySelectorAll('audio').forEach(a=>a.addEventListener('play',()=>{document.querySelectorAll('audio').forEach(b=>{if(b!==a)b.pause()})}));</script>
    </html>'''


def main() -> None:
    """检查全部九条文件、匹配成对控制，并导出试听页和独立审计证据。"""
    metadata = json.loads((ROOT / "candidate_analysis.json").read_text())
    assert len(metadata["records"]) == 9
    checked = [verify_pcm(row) for row in metadata["records"]]
    hashes = {row["path"]: row["sha256"] for row in checked}
    for target in list(TARGETS)[:2]:
        assert hashes[f"M1_two_pulse/{target}.wav"] == hashes[f"M2_three_pulse/{target}.wav"]
    assert hashes["M1_two_pulse/T03_landing_preparation.wav"] != hashes["M2_three_pulse/T03_landing_preparation.wav"]
    generator_hash = hashlib.sha256((ROOT / "generate_tram_revision.m").read_bytes()).hexdigest()
    assert generator_hash == metadata["generator_sha256"]
    for family in FAMILIES:
        assert (ROOT / f"{family}_audit.png").is_file()
    (ROOT / "audition.html").write_text(render_page(metadata), encoding="utf-8")
    AUDIT_ROOT.mkdir(parents=True, exist_ok=True)
    report = {"status": "pass", "checks": checked, "new_files": 6, "reference_files": 3,
              "matlab_generator_hash_verified": True, "human_audition_performed": False,
              "note": "Independent stdlib PCM validation; no acoustic or listener claims."}
    (AUDIT_ROOT / "independent_pcm_verification.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(json.dumps({"status": "PASS", "files_checked": 9, "audition_page": str(ROOT / "audition.html")}))


if __name__ == "__main__":
    main()
