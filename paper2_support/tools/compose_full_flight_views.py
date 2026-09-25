"""将同一条实际 Unity 完整航程的六张原始截图排成论文图，保留所有像素内容。"""
from pathlib import Path
import shutil
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt


def main():
    """读取第二条航程的实际阶段截图，添加独立面板标签并导出论文资产。"""
    source = Path('/Users/trusoegn/GitHub/Audio-VR/Audio-VR/temp/20260923-full-route-revision')
    target = Path(__file__).resolve().parents[1] / 'overleaf/figures'
    phases = [('ground', 'A  Ground departure'), ('climb', 'B  Climb'),
              ('left_turn', 'C  Left turn'), ('right_turn', 'D  Right turn'),
              ('descent', 'E  Descent'), ('landed', 'F  Touchdown')]
    plt.rcParams.update({'font.family':'DejaVu Sans', 'font.size':10})
    fig, axes = plt.subplots(3, 2, figsize=(10, 9.05))
    for ax, (key, title) in zip(axes.flat, phases):
        ax.imshow(plt.imread(source / f'route_02_{key}.png'))
        ax.set_title(title, loc='left', pad=5, fontweight='bold')
        ax.set_axis_off()
    fig.subplots_adjust(left=.01, right=.99, top=.97, bottom=.01, hspace=.14, wspace=.04)
    fig.savefig(target / 'full-flight-unity-views.png', dpi=300, facecolor='white')
    fig.savefig(target / 'full-flight-unity-views.pdf', dpi=300, facecolor='white')
    shutil.copy2(source / 'full_route_geometry.pdf', target / 'full-flight-routes.pdf')
    shutil.copy2(source / 'full_route_geometry.png', target / 'full-flight-routes.png')
    print(target / 'full-flight-unity-views.png')


if __name__ == '__main__':
    main()
