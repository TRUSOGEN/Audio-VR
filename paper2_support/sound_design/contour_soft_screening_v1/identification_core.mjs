// 独立于 DOM 的筛选状态与计分规则；所有时间均为浏览器软件时间。
export const TARGETS = ['T01', 'T02', 'T03'];
export const LABELS = ['Entering cruise', 'Beginning descent', 'Preparing to land'];

// 拒绝抽样避免 uint32 对 3 取模偏差；各 trial 独立，有放回，不强制轮换。
export function drawTarget(randomUint32 = () => crypto.getRandomValues(new Uint32Array(1))[0]) {
  let value;
  do { value = randomUint32(); } while (value >= 4294967295);
  if (!Number.isInteger(value) || value < 0) throw Error('Invalid random source');
  return TARGETS[value % 3];
}

export class ScreenRun {
  constructor(count, windowMs, config, draw = drawTarget) {
    if (!Number.isInteger(count) || count < 3 || count > 120) throw Error('Trial count must be 3–120');
    if (!Number.isFinite(windowMs) || windowMs < 1000 || windowMs > 30000) throw Error('Window must be 1–30 seconds');
    this.record = {schema: 'cue-identification-screen-v1', category: 'researcher_screening_qa',
      evidence: 'uncalibrated_browser_software_only', started_utc: new Date().toISOString(),
      configuration: {...config, count, window_ms: windowMs, sampling: 'independent_uniform_with_replacement'},
      schedule: Array.from({length: count}, draw), training: [], trials: [], status: 'ready'};
    if (this.record.schedule.some(t => !TARGETS.includes(t))) throw Error('Invalid target');
    this.pending = null;
  }
  begin(onsetMs) {
    if (this.pending || ['complete','aborted'].includes(this.record.status)) throw Error('Run cannot start trial');
    if (!Number.isFinite(onsetMs)) throw Error('Invalid onset');
    const i = this.record.trials.length;
    this.pending = {trial: i + 1, target: this.record.schedule[i], software_onset_ms: onsetMs,
      deadline_ms: onsetMs + this.record.configuration.window_ms};
    this.record.status = 'running';
    return {...this.pending};
  }
  respond(target, nowMs) {
    if (!this.pending || !TARGETS.includes(target) || !Number.isFinite(nowMs)) return false;
    if (nowMs < this.pending.software_onset_ms) return false;
    if (nowMs >= this.pending.deadline_ms) { this.timeout(nowMs); return false; }
    this.close({response: target, outcome: 'response', correct: target === this.pending.target,
      response_ms: nowMs, rt_ms: nowMs - this.pending.software_onset_ms});
    return true;
  }
  timeout(nowMs) {
    if (!this.pending || nowMs < this.pending.deadline_ms) return false;
    this.close({response: null, outcome: 'timeout', correct: false, response_ms: null, rt_ms: null});
    return true;
  }
  close(result) {
    this.record.trials.push({...this.pending, ...result});
    this.pending = null;
    if (this.record.trials.length === this.record.configuration.count) this.record.status = 'complete';
  }
  abort(reason) {
    if (['complete','aborted'].includes(this.record.status)) return;
    if (this.pending) this.close({response: null, outcome: 'technical_interruption', correct: null, rt_ms: null});
    this.record.status = 'aborted'; this.record.abort_reason = reason;
  }
}
