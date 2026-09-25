import {ScreenRun, TARGETS, LABELS} from './identification_core.mjs';
const $ = id => document.getElementById(id);
const names = ['T01_cruise_entry.wav','T02_descent_begin.wav','T03_landing_preparation.wav'];
let ctx, buffers = [], run, activeSource, timer, started = false, loading = false;
const heard = new Set();
const fail = error => {
  $('error').textContent = String(error.message || error);
  if (run) { run.abort(String(error.message || error)); finish(); }
};
const stopAudio = () => { if (activeSource) { activeSource.onended = null; try { activeSource.stop(); } catch {} activeSource = null; } };
const disableAnswers = disabled => [...$('answers').children].forEach(b => b.disabled = disabled);
function finish() {
  clearTimeout(timer); stopAudio(); disableAnswers(true);
  $('test').hidden = true; $('training').hidden = true; $('finish').hidden = false;
  const r = run.record, trials = r.trials;
  const answered = trials.filter(t => t.outcome === 'response');
  const timeouts = trials.filter(t => t.outcome === 'timeout');
  const valid = answered.length + timeouts.length;
  $('result').textContent = `${r.status === 'complete' ? 'Completed' : 'Stopped'}: ${answered.filter(t=>t.correct).length} correct / ${valid} scored trials; ${timeouts.length} timeouts. This is a QA record, not a pass/fail decision or participant evidence.`;
  $('record').value = JSON.stringify(r, null, 2);
}
function refreshAfterResponse() {
  clearTimeout(timer); disableAnswers(true);
  if (activeSource) { $('status').textContent = 'Response recorded. Finishing sound…'; return; }
  if (run.record.status === 'complete') finish();
  else { $('status').textContent = 'Response recorded. Continue when ready.'; $('play').disabled = false; }
}
function playBuffer(index) {
  stopAudio();
  const source = ctx.createBufferSource(); source.buffer = buffers[index]; source.connect(ctx.destination);
  const delay = .05, at = ctx.currentTime + delay;
  const onset = performance.now() + delay * 1000;
  activeSource = source;
  source.onended = () => { activeSource = null; if (started && run && !run.pending) refreshAfterResponse(); };
  source.start(at);
  return {source, onset, scheduled_audio_context_s: at};
}
$('load').onclick = async () => {
  if (loading || run) return;
  loading = true; $('load').disabled = true; $('error').textContent = ''; $('loadstate').textContent = 'Loading and checking audio files…';
  try {
    const family = $('family').value, count = Number($('count').value), windowMs = Number($('window').value) * 1000;
    // 参数先验证，音频预加载成功才开放训练。
    const candidate = new ScreenRun(count, windowMs, {family});
    ctx = new AudioContext(); await ctx.resume();
    const response = await fetch('candidate_analysis.json'); if (!response.ok) throw Error('Cannot load stimulus manifest');
    const manifest = await response.json(), records = [];
    buffers = await Promise.all(names.map(async name => {
      const path = `${family}/${name}`, expected = manifest.files.find(f => f.file === path);
      if (!expected) throw Error(`Missing manifest entry: ${path}`);
      const response = await fetch(path); if (!response.ok) throw Error(`Cannot load ${path}`);
      const data = await response.arrayBuffer();
      const hash = [...new Uint8Array(await crypto.subtle.digest('SHA-256',data))].map(v=>v.toString(16).padStart(2,'0')).join('');
      if (hash !== expected.sha256) throw Error(`Checksum mismatch: ${path}`);
      const decoded = await ctx.decodeAudioData(data);
      records.push({target: name.slice(0,3), file: path, sha256:hash, decoded_duration_s:decoded.duration});
      return decoded;
    }));
    candidate.record.configuration.stimuli = records.sort((a,b)=>a.target.localeCompare(b.target));
    candidate.record.configuration.audio_context_sample_rate = ctx.sampleRate;
    candidate.record.configuration.audio_output_latency_s = ctx.outputLatency ?? null;
    candidate.record.configuration.audio_base_latency_s = ctx.baseLatency;
    run = candidate; $('setup').hidden = true; $('training').hidden = false;
    ctx.onstatechange = () => { if(started && run && !['complete','aborted'].includes(run.record.status) && ctx.state !== 'running') fail(Error('Audio context interrupted')); };
  } catch(error) { $('loadstate').textContent = 'Sounds not ready.'; fail(error); $('load').disabled = false; if(ctx) await ctx.close(); }
  finally { loading = false; }
};
TARGETS.forEach((target, i) => {
  const example = document.createElement('button'); example.textContent = LABELS[i];
  example.onclick = async () => {
    if (!run || started || activeSource) return;
    try {
      await ctx.resume(); const {source,onset} = playBuffer(i);
      [...$('examples').children].forEach(b => b.disabled = true);
      $('start').disabled = true; $('trainingstatus').textContent = `Playing: ${LABELS[i]}.`;
      const event = {target, software_onset_ms:onset, complete:false}; run.record.training.push(event);
      source.onended = () => { activeSource = null; event.complete = true; heard.add(target);
        [...$('examples').children].forEach(b => b.disabled = false);
        $('trainingstatus').textContent = `${heard.size} of 3 examples played. Replay any example, or start when ready.`;
        $('start').disabled = heard.size !== 3; };
    } catch(error) { fail(error); }
  };
  $('examples').append(example);
  const answer = document.createElement('button'); answer.textContent = LABELS[i]; answer.disabled = true;
  answer.onclick = () => {
    if (!run || !run.pending) return;
    const accepted = run.respond(target, performance.now());
    if(accepted || !run.pending) refreshAfterResponse();
  };
  $('answers').append(answer);
});
$('start').onclick = () => {
  if (!run || heard.size !== 3 || activeSource || started) return;
  started = true; $('training').hidden = true; $('test').hidden = false;
};
$('play').onclick = async () => {
  if(!run || run.pending || ['complete','aborted'].includes(run.record.status)) return;
  $('play').disabled = true;
  try {
    await ctx.resume();
    if(document.hidden) throw Error('Tab not visible');
    const target = run.record.schedule[run.record.trials.length];
    const scheduled = playBuffer(TARGETS.indexOf(target));
    run.begin(scheduled.onset);
    run.pending.scheduled_audio_context_s = scheduled.scheduled_audio_context_s;
    $('status').textContent = 'Listen, then choose one answer.'; disableAnswers(false);
    // 到期检查使用同一 monotonic clock，拒绝 deadline 后的点击，避免 timer 调度延迟造成越窗接受。
    timer = setTimeout(() => {
      if (run.timeout(performance.now())) refreshAfterResponse();
    }, run.record.configuration.window_ms + 60);
  } catch(error) { fail(error); }
};
$('abort').onclick = () => { run.abort('operator_end'); finish(); };
$('export').onclick = () => {
  const blob = new Blob([$('record').value],{type:'application/json'}), url = URL.createObjectURL(blob);
  const link = document.createElement('a'); link.href = url; link.download = `cue_check_qa_${run.record.started_utc.replace(/[:.]/g,'-')}.json`;
  link.click(); setTimeout(()=>URL.revokeObjectURL(url),1000);
};
document.addEventListener('visibilitychange',()=>{
  if(document.hidden && run && started && !['complete','aborted'].includes(run.record.status)) { run.abort('tab_hidden'); finish(); }
});
window.addEventListener('beforeunload',event=>{
  if(run && run.record.status !== 'ready') {event.preventDefault();event.returnValue='';}
});
