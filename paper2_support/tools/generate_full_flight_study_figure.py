"""生成完整航程方案的论文流程图。

该图表达四条件、每航程三次通知与固定响应窗口的关系，不表示实测时长。
输入为当前作者设计决定；输出矢量 PDF/SVG 与 PNG，保留旧独立片段图。
"""

from pathlib import Path

import matplotlib

matplotlib.use("Agg")
import matplotlib.pyplot as plt
from matplotlib.patches import FancyBboxPatch


ROOT = Path(__file__).resolve().parents[1]
INK, BLUE, GOLD, LINE = "#183442", "#147f98", "#ad741c", "#afc0c8"


def box(ax, x, y, width, height, text, color="white", edge=LINE, size=8):
    """绘制固定几何的文本框，避免流程标签与连线相交。"""
    ax.add_patch(FancyBboxPatch((x, y), width, height, boxstyle="round,pad=0.007,rounding_size=0.02", linewidth=.8, edgecolor=edge, facecolor=color))
    ax.text(x + width / 2, y + height / 2, text, ha="center", va="center", fontsize=size, color=INK, linespacing=1.35)


def main():
    """按当前完整航程方案生成可插入 Methods 的示意图并保存源版本。"""
    plt.rcParams.update({"font.family": "DejaVu Sans", "font.size": 8, "pdf.fonttype": 42, "svg.fonttype": "none"})
    fig = plt.figure(figsize=(7.2, 3.9), facecolor="white")
    ax = fig.add_axes([.025, .035, .95, .93]); ax.set_xlim(0, 1); ax.set_ylim(0, 1); ax.axis("off")
    ax.text(0, .97, "a  Four conditions within each participant", weight="bold", color=INK, fontsize=10)
    ax.text(.30, .88, "Quiet", ha="center", color=INK)
    ax.text(.56, .88, "Added cabin noise", ha="center", color=INK)
    ax.text(.005, .784, "Non-speech", va="center", color=INK)
    ax.text(.005, .664, "Speech", va="center", color=INK)
    for x, y, label in [(.18,.74,"M1 cues"),(.44,.74,"M1 cues + noise"),(.18,.62,"Spoken notices"),(.44,.62,"Spoken notices + noise")]:
        box(ax,x,y,.23,.09,label,color="#eff6f8")
    box(ax,.72,.62,.265,.21,"One full flight per condition\nThree notifications per flight\nBreak after landing",color="#fbf6eb",edge="#d3b27d",size=7.8)
    ax.text(0, .54, "b  One continuous flight", weight="bold", color=INK, fontsize=10)
    xs=[.012,.176,.34,.504,.668,.832]; labels=["Take-off", "Climb", "Cruise\nleft + right turns", "Descent", "Landing\npreparation", "Touchdown"]
    for i,(x,label) in enumerate(zip(xs,labels)):
        box(ax,x,.365,.145,.105,label,color="#f5f7f8",size=7.6)
        if i<5: ax.annotate("",xy=(xs[i+1]-.004,.417),xytext=(x+.15,.417),arrowprops={"arrowstyle":"->","lw":.8,"color":LINE})
    for x,t in [(.327,"T01"),(.491,"T02"),(.655,"T03")]:
        ax.text(x,.326,t,ha="center",color=BLUE,fontweight="bold",fontsize=8)
        ax.plot([x,x],[.35,.37],color=BLUE,lw=1.1)
    ax.text(.012,.275,"Each notice precedes its corresponding phase change; motion continues during responses.",color=INK,fontsize=7.5)
    labels=["Notice onset", "Identification\nfixed window", "Confidence*\nfixed window", "Margin", "Phase change"]
    xs=[.01,.195,.415,.635,.79]; widths=[.15,.185,.185,.12,.195]
    for i,(x,w,label) in enumerate(zip(xs,widths,labels)):
        box(ax,x,.11,w,.10,label,color="#eaf4f6" if i<3 else "#fbf6eb",size=7.4)
        if i<4: ax.annotate("",xy=(xs[i+1]-.004,.16),xytext=(x+w+.005,.16),arrowprops={"arrowstyle":"->","lw":.8,"color":LINE})
    ax.text(.01,.041,"* Responders only. The GO/WAIT activity continues during both response windows.",fontsize=7.1,color=INK)
    ax.text(.99,.0,"Schematic, not to time scale",ha="right",fontsize=7,color="#536a75")
    output=ROOT/"overleaf/figures"; output.mkdir(exist_ok=True)
    for ext in ("pdf","svg","png"):
        fig.savefig(output/f"full-flight-study-design.{ext}",dpi=300,facecolor="white",metadata={"Creator":"Paper 2 study figure generator"} if ext=="pdf" else None)
    plt.close(fig)


if __name__ == "__main__":
    main()
