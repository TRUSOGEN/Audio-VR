"""绘制Audio V0的独立研究图与组合图，始终使用查看器相同快照与清洗表。"""
from __future__ import annotations

import io
from collections import defaultdict
from typing import Any

from audio_v0_review import COLORS, LABELS, cleaned_rows, number

PANELS = {
    'accuracy': ('Identification accuracy', 'Correct / valid notifications (%)'),
    'rt': ('Correct-response time', 'Median (s), correct accepted answers only'),
    'confidence': ('Identification confidence', 'Median rating (1–7), valid recorded answers'),
    'visual': ('GO / WAIT visual activity', 'Rate (%)'),
    'trajectory': ('Recorded horizontal trajectory', 'Unity world x (m)'),
    'altitude': ('Recorded vertical position', 'Session clock (s)'),
}


def draw_panel(ax: Any, review: dict[str, Any], panel: str, letter: str = '') -> None:
    """单图与组合图调用相同绘图逻辑；不连接跨区块的运动采样。"""
    from matplotlib.lines import Line2D
    from matplotlib.ticker import MaxNLocator
    title, xlabel = PANELS[panel]
    ax.set(title=(letter + '  ' if letter else '') + title, xlabel=xlabel)
    if panel in {'trajectory', 'altitude'}:
        groups: dict[tuple[str, ...], list[dict[str, Any]]] = defaultdict(list)
        for row in cleaned_rows(review, panel):
            identity = tuple(str(row.get(field, '')) for field in ('attempt_id', 'block_id', 'excerpt_id', 'condition_id'))
            groups[identity].append(row)
        for identity, rows in groups.items():
            color = COLORS.get(identity[-1], '#647588')
            if panel == 'trajectory':
                x = [number(r['x_m']) for r in rows]
                y = [number(r['z_m']) for r in rows]
                ax.plot(x, y, color=color, lw=1.6, alpha=.85)
                ax.scatter(x[0], y[0], s=20, color=color, marker='o')
                ax.scatter(x[-1], y[-1], s=28, color=color, marker='x')
            else:
                ax.plot([number(r['monotonic_clock_ms']) / 1000 for r in rows],
                        [number(r['y_m']) for r in rows], color=color, lw=1.5)
        if panel == 'trajectory':
            ax.set(ylabel='Unity world z (m)', aspect='equal')
        else:
            ax.set_ylabel('Unity world y (m)')
            for event in review['timeline']:
                instant = number(event.get('monotonic_clock_ms'))
                if event.get('event_type') == 'notification_onset' and instant is not None:
                    ax.axvline(instant / 1000, color=COLORS.get(event.get('condition_id'), '#647588'), alpha=.35, ls=':', lw=.8)
        conditions = list(dict.fromkeys(identity[-1] for identity in groups))
        if conditions:
            ax.legend([Line2D([0], [0], color=COLORS.get(c, '#647588'), lw=2) for c in conditions],
                      [LABELS.get(c, c) for c in conditions], loc='best', fontsize=7.5, framealpha=.85)
        else:
            ax.text(.5, .5, 'No usable recorded motion samples', ha='center', va='center', transform=ax.transAxes)
        ax.grid(True, zorder=0)
        return

    rows = review['summary']
    labels = [LABELS.get(row['condition_id'], row['condition_id']) for row in rows]
    ax.set_yticks(range(len(rows)), labels)
    ax.set_ylim(len(rows) - .5, -.5)
    ax.grid(axis='x')
    if panel == 'visual':
        ax.set_xlim(-4, 116)
        for i, row in enumerate(rows):
            for field, offset, marker, label, numerator, denominator in (
                    ('go_hit_rate', -.19, 'o', 'GO', row['go_hits'], row['go_denominator']),
                    ('wait_false_alarm_rate', .19, 'D', 'WAIT false alarm', row['wait_false_alarms'], row['wait_denominator'])):
                value = row[field]
                if value is None:
                    ax.text(.025, i + offset, f'{label}: no scorable opportunities', transform=ax.get_yaxis_transform(), fontsize=8, va='center')
                    continue
                color = COLORS.get(row['condition_id'], '#647588')
                ax.scatter(value * 100, i + offset, s=42, marker=marker, color=color,
                           facecolors=color if marker == 'o' else 'none', zorder=3)
                ax.text(.985, i + offset, f'{label} {numerator}/{denominator}', transform=ax.get_yaxis_transform(),
                        ha='right', va='center', fontsize=8)
        return
    field, scale, limits, count_key = {
        'accuracy': ('accuracy', 100, (0, 105), 'identification_denominator'),
        'rt': ('median_correct_rt_ms', .001, None, 'correct_rt_n'),
        'confidence': ('median_confidence', 1, (1, 7), 'confidence_n'),
    }[panel]
    max_value = max([float(row[field]) * scale for row in rows if row[field] is not None] + [1])
    ax.set_xlim(*(limits or (0, max_value * 1.3)))
    for i, row in enumerate(rows):
        value, n = row[field], row[count_key]
        if value is None:
            ax.text(.03, i, 'Not available (n=0)', color='#758493', transform=ax.get_yaxis_transform(), va='center', fontsize=9)
            continue
        plotted = value * scale
        ax.scatter(plotted, i, s=65, color=COLORS.get(row['condition_id'], '#647588'), zorder=3, clip_on=False)
        label = f"{plotted:.1f}%  ({row['correct']}/{n})" if panel == 'accuracy' else f'{plotted:.2f}  (n={n})'
        right = plotted > ax.get_xlim()[1] * .68
        ax.annotate(label, (plotted, i), xytext=(-9 if right else 9, -10), textcoords='offset points',
                    ha='right' if right else 'left', fontsize=9)
    if panel == 'confidence':
        ax.set_xticks(range(1, 8))
    else:
        ax.xaxis.set_major_locator(MaxNLocator(5))


def render_figure(review: dict[str, Any], kind: str = 'conditions', format: str = 'svg') -> bytes:
    """输出同一指标的独立图或组合图，每件保留证据类别、筛选和来源身份。"""
    import matplotlib
    matplotlib.use('Agg')
    import matplotlib.pyplot as plt
    if kind not in set(PANELS) | {'conditions', 'motion'}:
        raise ValueError('Unknown figure: ' + kind)
    plt.rcParams.update({'font.family': 'DejaVu Sans', 'font.size': 10, 'svg.fonttype': 'none',
                         'axes.spines.top': False, 'axes.spines.right': False, 'axes.edgecolor': '#C5D0DA',
                         'axes.labelcolor': '#34475A', 'text.color': '#17324B', 'xtick.color': '#526679',
                         'ytick.color': '#526679', 'grid.color': '#E6EBEF', 'axes.titleweight': 'bold'})
    if kind == 'conditions':
        panels = ['accuracy', 'rt', 'confidence', 'visual']
        fig, axes = plt.subplots(2, 2, figsize=(12.8, 7.6))
        title = 'Four-condition session review'
    elif kind == 'motion':
        panels = ['trajectory', 'altitude']
        fig, axes = plt.subplots(1, 2, figsize=(12.8, 5.5))
        title = 'Flight recording review'
    else:
        panels = [kind]
        fig, axes = plt.subplots(1, 1, figsize=(7.6, 5.1))
        title = PANELS[kind][0]
    all_axes = list(axes.flat) if hasattr(axes, 'flat') else [axes]
    for i, (ax, panel) in enumerate(zip(all_axes, panels)):
        draw_panel(ax, review, panel, chr(65 + i) if len(panels) > 1 else '')
        if len(panels) == 1:
            ax.set_title('')
    fig.suptitle(title, x=.025, y=.985, ha='left', fontsize=17 if len(panels) > 1 else 15, weight='bold')
    boundary = review['evidence_label']
    if review['export_eligibility'] == 'qa_only':
        boundary += ' | NOT PARTICIPANT RESULTS'
    fig.text(.025, .935 if kind == 'conditions' else .895, boundary,
             fontsize=10.5 if len(panels) > 1 else 8, color='#995121', weight='bold')
    if kind in {'motion', 'trajectory', 'altitude'}:
        note = 'Recorded samples; blocks/excerpts are not connected. Start: circle; end: cross.\nDotted lines: notifications. World y is not height above local terrain.'
    else:
        note = 'Descriptive audit; accuracy includes timeouts. No imputation or uncertainty intervals.\nInvalid notification trials and interrupted/invalid visual opportunities are excluded.'
    fig.text(.025, .07, note, fontsize=7.5, color='#526679', linespacing=1.45)
    filters = '; '.join(f"{k}={v or 'all'}" for k, v in review['filters'].items()) or 'All conditions, blocks and routes'
    fig.text(.025, .013, review['session_id'] + '\n' + filters, fontsize=6.5, color='#526679', linespacing=1.3)
    fig.tight_layout(rect=(0, .16, 1, .90 if kind == 'conditions' else .865), h_pad=2.4, w_pad=2.6)
    output = io.BytesIO()
    fig.savefig(output, format=format, dpi=240, facecolor='white', metadata={'Creator': 'Audio V0 local data review'})
    plt.close(fig)
    return output.getvalue()
