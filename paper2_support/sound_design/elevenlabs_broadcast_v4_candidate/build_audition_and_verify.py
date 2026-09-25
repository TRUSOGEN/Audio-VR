"""独立复算 MATLAB 解码样本，生成 V3/V4 同目标语音对照试听页。

不解码或修改 raw MP3，不进行播放和主观判断；数值复核是同一次解码输出的
独立计算，不能视为第二个解码器的交叉验证。
"""

from __future__ import annotations

import array
import hashlib
import html
import json
import math
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parent
AUDIT_ROOT = ROOT.parents[1] / "temp" / "speech_v4_20260923"


def verify_record(row: dict) -> dict:
    """检查 raw hash 及保留的解码双精度样本，独立计算 RMS/peak。"""
    path = ROOT / row["relative_path"]
    assert hashlib.sha256(path.read_bytes()).hexdigest() == row["sha256"]
    binary = AUDIT_ROOT / f'{row["version"]}_{row["target"]}_decoded_float64.bin'
    values = array.array("d", binary.read_bytes())
    if sys.byteorder != "little":
        values.byteswap()
    assert len(values) == row["decoded_samples"]
    rms = math.sqrt(math.fsum(x*x for x in values)/len(values))
    peak = max(abs(x) for x in values)
    assert abs(rms-row["digital_rms"]) < 1e-12
    assert abs(peak-row["digital_peak"]) < 1e-12
    full_scale = sum(abs(x) >= 1 for x in values)
    assert full_scale == row["decoded_abs_ge_1_samples"]
    # 复算 10 ms / -50 dBFS 能量阈值的首尾区间。
    frame_length = round(row["sample_rate_hz"]*0.010)
    active = []
    for start in range(0, len(values), frame_length):
        frame = values[start:start+frame_length]
        frame_rms = math.sqrt(math.fsum(x*x for x in frame)/len(frame))
        if frame_rms > 10**(-50/20):
            active.append(start)
    start_s = active[0]/row["sample_rate_hz"]
    end_s = min(active[-1]+frame_length, len(values))/row["sample_rate_hz"]
    assert abs(start_s-row["active_start_s"]) < 1e-12
    assert abs(end_s-row["active_end_s"]) < 1e-12
    return {"target": row["target"], "version": row["version"], "sha256_verified": True,
            "samples": len(values), "rms": rms, "peak": peak, "silence_boundaries_verified": True}


def render_page(metadata: dict) -> str:
    """从实测 JSON 呈现原文、生产参数与每目标旧新音频，不自动播放。"""
    sections = []
    for target in ("T01", "T02", "T03"):
        cards = []
        for row in metadata["records"]:
            if row["target"] != target:
                continue
            name = "V3 · 旧版 reference" if row["version"] == "V3_reference" else "V4 · 新候选"
            cards.append(f'''<article><h3>{name}</h3><p class="text">{html.escape(row["text"])}</p>
            <p>Provider UI speed: <strong>{row["speed_ui"]:.2f}</strong></p>
            <audio controls preload="none" src="{html.escape(row["relative_path"], quote=True)}"></audio>
            <dl><dt>解码时长</dt><dd>{row["decoded_duration_s"]:.3f} s</dd>
            <dt>数字 RMS / peak</dt><dd>{row["digital_rms"]:.4f} / {row["digital_peak"]:.4f}</dd>
            <dt>首 / 尾低能量区间</dt><dd>{row["leading_below_threshold_s"]:.3f} / {row["trailing_below_threshold_s"]:.3f} s</dd>
            <dt>解码满幅样本数</dt><dd>{row["decoded_abs_ge_1_samples"]}</dd></dl>
            <a href="{html.escape(row["relative_path"], quote=True)}" download>下载原始 MP3</a>
            <details><summary>SHA-256</summary><code>{row["sha256"]}</code></details></article>''')
        sections.append(f'''<section><h2>{target} · 同目标对照</h2><div class="cards">{"".join(cards)}</div>
        <details><summary>查看实际波形与能量包络</summary><a href="{target}_waveform_comparison.png"><img loading="lazy" src="{target}_waveform_comparison.png" alt="{target} V3/V4 decoded waveform comparison"></a></details></section>''')
    return '''<!doctype html><html lang="zh-CN"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
    <title>Paper #2 · V3 / V4 speech candidate</title><style>
    body{font:16px/1.65 system-ui,sans-serif;max-width:1100px;margin:40px auto;padding:0 24px;color:#193840;background:#f6f8f7}
    h1{font-size:30px;line-height:1.3}h2{font-size:23px}h3{font-size:19px;margin-top:0}.status{background:#e7ede4;padding:4px 10px;border-radius:5px}
    section{background:white;border:1px solid #d8e1de;border-radius:12px;padding:24px;margin:30px 0}.cards{display:grid;grid-template-columns:1fr 1fr;gap:20px}
    article{background:#f2f6f5;padding:20px;border-radius:8px}.text{font-weight:600;min-height:54px}audio{width:100%;margin:12px 0}
    dl{display:grid;grid-template-columns:1fr 1fr;gap:7px;font-size:14px}dd{margin:0;text-align:right}dt{color:#526c70}a{color:#1c6771}details{margin-top:16px}summary{cursor:pointer}
    code{display:block;font-size:12px;overflow-wrap:anywhere}img{width:100%;margin-top:14px}.note{border-left:3px solid #748a76;padding-left:16px}
    @media(max-width:750px){.cards{grid-template-columns:1fr}body{margin:24px auto;padding:0 16px}section{padding:16px}.text{min-height:0}}</style>
    <header><span class="status">工程候选 · 主观试听待做</span><h1>语音预告与播报节奏：V3 / V4 对照</h1>
    <p>保持同一 ElevenLabs voice identity 与 Eleven Multilingual v2；V4 的 speed UI 从 1.00 调至 0.90，T01 改为未来式预告。原始 MP3 未裁剪、未归一化或变速。</p>
    <p class="note">T01 同时改变文案和 speed 设置，不能把听感或时长差异只归因于语速。Provider 设置不保证实际词速恰好下降 10%；是否自然、清晰且符合常规机舱广播仍需人耳检查。</p>
    <p><a href="README.md">设计与生产记录</a> · <a href="production_record.json">原始生成记录</a> · <a href="waveform_audit.json">数字审计</a> · <a href="waveform_audit.csv">CSV</a></p></header>''' + "".join(sections) + '''
    <section><h2>审计如何解释</h2><p>首尾区间定义为：使用不重叠 10 ms 窗，查找 RMS 超过 −50 dBFS 的首尾窗；它只是数字阈值估计，不等于词语、感知或实际扬声器声音的起止时间。</p>
    <p>RMS 与 peak 来自 MATLAB 对实际 MP3 的解码样本；解码满幅样本为 |x| ≥ 1 的计数，0 不排除上游 clipping 或其他 codec 伪影。没有将相同数字电平当成响度匹配或耳侧校准。</p>
    <p>试听重点：准确读出文案、三个 target 的同一说话人身份、句间停顿、自然语速、干净结尾、quiet/noise 中可懂度。当前是研究者有标签试听入口，尚未完成人类筛选、HMD 投送或完整 pilot。</p>
    <p>本页没有自动播放或上传；播放新一条会暂停其他条目。</p></section>
    <script>document.querySelectorAll('audio').forEach(a=>a.addEventListener('play',()=>document.querySelectorAll('audio').forEach(b=>{if(b!==a)b.pause()})));</script></html>'''


def main() -> None:
    """验证全部六条解码结果及源 hash，保存边界明确的复核报告。"""
    metadata = json.loads((ROOT/"waveform_audit.json").read_text())
    assert len(metadata["records"]) == 6
    checks = [verify_record(row) for row in metadata["records"]]
    assert hashlib.sha256((ROOT/"audit_speech_exports.m").read_bytes()).hexdigest() == metadata["generator_sha256"]
    (ROOT/"audition.html").write_text(render_page(metadata), encoding="utf-8")
    report = {"status": "pass", "records": checks, "decoder": "MATLAB audioread", "independent_decoder": False,
              "scope": "Independent arithmetic on the retained MATLAB float64 samples, plus original file hash verification.",
              "human_audition": False, "raw_files_modified": False}
    (AUDIT_ROOT/"independent_numeric_verification.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(json.dumps({"status": "PASS", "verified_records": len(checks), "audition": str(ROOT/"audition.html")}))


if __name__ == "__main__":
    main()
