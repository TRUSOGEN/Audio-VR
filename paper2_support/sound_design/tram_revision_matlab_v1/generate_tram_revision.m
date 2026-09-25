function generate_tram_revision()
%GENERATE_TRAM_REVISION 重现 9.23 反馈后的非语音工程候选并审计导出文件。
% 使用 base MATLAB 合成、WAV 回读、波形/频谱输出，不需要音频工具箱。
% 不执行声音播放；数字审计不构成人耳筛选、响度匹配或耳侧校准。

root = fileparts(mfilename('fullpath'));
fs = 44100;
duration = 1.8;
targetRms = 0.15;
maxPeak = 0.75;
targets = {'T01_cruise_entry', 'T02_descent_begin', 'T03_landing_preparation'};
families = {'M1_two_pulse', 'M2_three_pulse'};
records = struct([]);
fprintf('MATLAB %s\nCandidate root: %s\n', version, root);

% 旧版本只复制，不重新合成；保留供对照的实际历史 WAV 字节。
referenceRoot = fullfile(root, 'reference_C0_1p35s');
if ~isfolder(referenceRoot), mkdir(referenceRoot); end
for targetIndex = 1:3
    oldPath = fullfile(root, '..', 'contour_soft_screening_v1', ...
        'C0_current_contour', [targets{targetIndex}, '.wav']);
    newPath = fullfile(referenceRoot, [targets{targetIndex}, '.wav']);
    assert(isfile(oldPath), 'Missing reference file: %s', oldPath);
    copyfile(oldPath, newPath);
    assert(strcmp(fileSha256(oldPath), fileSha256(newPath)), 'Reference checksum changed.');
    row = inspectWav(newPath, 'reference_C0_1p35s', targets{targetIndex});
    if isempty(records), records = row; else, records(end+1) = row; end %#ok<AGROW>
end

for familyIndex = 1:numel(families)
    familyRoot = fullfile(root, families{familyIndex});
    if ~isfolder(familyRoot), mkdir(familyRoot); end
    for targetIndex = 1:3
        samples = synthesizeCue(fs, duration, targetIndex, familyIndex);
        rawRms = sqrt(mean(samples.^2));
        assert(rawRms > 0 && all(isfinite(samples)), 'Invalid synthesized samples.');
        samples = samples * (targetRms / rawRms);
        assert(max(abs(samples)) <= maxPeak, 'RMS target violates peak limit; do not silently rescale.');
        outputPath = fullfile(familyRoot, [targets{targetIndex}, '.wav']);
        audiowrite(outputPath, samples, fs, 'BitsPerSample', 16);
        row = inspectWav(outputPath, families{familyIndex}, targets{targetIndex});
        assert(row.sample_rate_hz == fs && row.channels == 1 && row.bits_per_sample == 16);
        assert(abs(row.duration_s - duration) < 1/fs && abs(row.digital_rms - targetRms) < 0.0001);
        assert(row.digital_peak <= maxPeak && row.clipped_samples == 0);
        assert(abs(row.dc_mean) < 0.0001 && abs(row.first_sample) < 1/32768 && abs(row.last_sample) < 1/32768);
        records(end+1) = row; %#ok<AGROW>
        fprintf('%s/%s: duration=%.4f RMS=%.8f peak=%.6f SHA256=%s\n', ...
            row.family, row.target, row.duration_s, row.digital_rms, row.digital_peak, row.sha256);
    end
end

% 两个候选只有 T03 pulse 数不同；T01/T02 必须逐字节相同。
for targetIndex = 1:2
    assert(strcmp(fileSha256(fullfile(root, families{1}, [targets{targetIndex}, '.wav'])), ...
        fileSha256(fullfile(root, families{2}, [targets{targetIndex}, '.wav']))));
end

metadata = struct('status', 'engineering_candidates_human_screen_pending', ...
    'matlab_version', version, 'generator_sha256', fileSha256(mfilename('fullpath') + ".m"), ...
    'generated_at_utc', char(datetime('now','TimeZone','UTC','Format',"yyyy-MM-dd'T'HH:mm:ss'Z'")), ...
    'sample_rate_hz', fs, 'duration_s', duration, 'target_digital_rms', targetRms, ...
    'max_digital_peak', maxPeak, 'harmonic_weight', 0.10, ...
    'T01_frequency_hz', [330 495], 'T02_frequency_hz', [495 330], ...
    'T03_frequency_hz', 392, 'M1_pulse_onsets_s', [0.09 0.99], ...
    'M1_pulse_duration_s', 0.72, 'M2_pulse_onsets_s', [0.06 0.66 1.26], ...
    'M2_pulse_duration_s', 0.48, 'records', records);
writeUtf8(fullfile(root, 'candidate_analysis.json'), jsonencode(metadata, 'PrettyPrint', true));
writetable(struct2table(records), fullfile(root, 'candidate_analysis.csv'));
for family = [{'reference_C0_1p35s'}, families]
    drawFamily(root, family{1}, targets);
end
fprintf('PAPER2_MATLAB_GENERATION_AND_AUDIT_PASS: 6 new WAVs + 3 unchanged reference WAVs.\n');
fprintf('Human audition, HMD playback, calibration, confusion screen and pilot remain pending.\n');
end

function samples = synthesizeCue(fs, duration, targetIndex, familyIndex)
%SYNTHESIZECUE 共享柔和谐波音色；T01/T02 连续轮廓，T03 慢速等音脉冲。
t = (0:round(fs*duration)-1)' / fs;
if targetIndex <= 2
    u = min(1, t/1.50);
    smoothU = 3*u.^2 - 2*u.^3;
    if targetIndex == 1, f = 330 + 165*smoothU; else, f = 495 - 165*smoothU; end
    envelope = ones(size(t));
    fade = 0.18;
    attack = t < fade;
    release = t > duration-fade;
    envelope(attack) = sin(pi/2 * t(attack)/fade).^2;
    envelope(release) = sin(pi/2 * (duration-t(release))/fade).^2;
else
    f = 392*ones(size(t));
    envelope = zeros(size(t));
    if familyIndex == 1, onsets = [0.09 0.99]; pulseDuration = 0.72;
    else, onsets = [0.06 0.66 1.26]; pulseDuration = 0.48; end
    for onset = onsets
        local = (t-onset)/pulseDuration;
        active = local >= 0 & local <= 1;
        envelope(active) = sin(pi*local(active)).^2;
    end
end
phase = 2*pi*cumsum(f)/fs;
samples = envelope .* (sin(phase) + 0.10*sin(2*phase));
% 数字波形的两个端点精确置零，后续 WAV 回读断言确认。
samples([1 end]) = 0;
end

function row = inspectWav(path, family, target)
%INSPECTWAV 从实际 PCM 文件重读并量化文件、电平和能量时间范围。
[samples, fs] = audioread(path);
info = audioinfo(path);
energy = cumsum(samples.^2);
low = find(energy >= energy(end)*0.025, 1, 'first');
high = find(energy >= energy(end)*0.975, 1, 'first');
row = struct('family', family, 'target', target, 'relative_path', [family '/' target '.wav'], ...
    'sha256', fileSha256(path), 'sample_rate_hz', fs, 'channels', info.NumChannels, ...
    'bits_per_sample', info.BitsPerSample, 'samples', numel(samples), 'duration_s', numel(samples)/fs, ...
    'digital_rms', sqrt(mean(samples.^2)), 'digital_peak', max(abs(samples)), ...
    'crest_factor_db', 20*log10(max(abs(samples))/sqrt(mean(samples.^2))), ...
    'dc_mean', mean(samples), 'clipped_samples', sum(abs(samples) >= 32767/32768), ...
    'first_sample', samples(1), 'last_sample', samples(end), ...
    'energy_95_start_s', (low-1)/fs, 'energy_95_end_s', (high-1)/fs, ...
    'energy_95_span_s', (high-low)/fs);
end

function hash = fileSha256(path)
%FILESHA256 计算实际文件字节的 SHA-256；与 PCM 数据校验分开记录。
fid = fopen(path, 'rb');
assert(fid >= 0, 'Cannot read %s', path);
cleanup = onCleanup(@() fclose(fid)); %#ok<NASGU>
bytes = fread(fid, Inf, '*uint8');
digest = java.security.MessageDigest.getInstance('SHA-256');
digest.update(bytes);
hash = lower(reshape(dec2hex(typecast(digest.digest(),'uint8'), 2)', 1, []));
end

function drawFamily(root, family, targets)
%DRAWFAMILY 绘制实际导出文件的波形、FFT 和短时频谱，固定可比坐标。
fig = figure('Visible','off','Position',[100 100 1600 1100], 'Color','white');
cleanup = onCleanup(@() close(fig)); %#ok<NASGU>
layout = tiledlayout(3,3,'TileSpacing','compact','Padding','compact');
title(layout, strrep([family ' | digital audit only; no listener validation'], '_', ' '));
for k = 1:3
    [x,fs] = audioread(fullfile(root, family, [targets{k} '.wav']));
    t = (0:numel(x)-1)'/fs;
    nexttile(k); plot(t,x,'Color',[0.12 0.34 0.45]);
    title(strrep(targets{k},'_',' ')); xlabel('Time (s)'); ylabel('Amplitude');
    xlim([0 1.8]); ylim([-0.5 0.5]); grid on;
    w = 0.5 - 0.5*cos(2*pi*(0:numel(x)-1)'/(numel(x)-1));
    fftSize = 2^nextpow2(numel(x));
    spectrum = 2*abs(fft(x.*w, fftSize))/sum(w);
    frequency = (0:fftSize-1)'*fs/fftSize;
    nexttile(k+3); plot(frequency,20*log10(max(spectrum,1e-6)),'Color',[0.12 0.34 0.45]);
    xlabel('Frequency (Hz)'); ylabel('Windowed amplitude (dBFS)');
    xlim([0 2000]); ylim([-100 0]); grid on;
    frameLength = 2048; hop = 256; nfft = 4096;
    frameStarts = 1:hop:numel(x)-frameLength+1;
    window = 0.5 - 0.5*cos(2*pi*(0:frameLength-1)'/(frameLength-1));
    frames = zeros(nfft/2+1,numel(frameStarts));
    for j = 1:numel(frameStarts)
        frame = x(frameStarts(j):frameStarts(j)+frameLength-1).*window;
        frameFft = 2*abs(fft(frame,nfft))/sum(window);
        frames(:,j) = frameFft(1:nfft/2+1);
    end
    nexttile(k+6);
    imagesc(((frameStarts-1)+frameLength/2)/fs,(0:nfft/2)*fs/nfft,20*log10(max(frames,1e-6)));
    axis xy; xlim([0 1.8]); ylim([0 2000]); clim([-80 -5]);
    xlabel('Time (s)'); ylabel('Frequency (Hz)'); colorbar;
end
exportgraphics(fig,fullfile(root,[family '_audit.png']),'Resolution',150);
end

function writeUtf8(path, content)
%WRITEUTF8 写入可读的完整审计文件，避免默认平台编码改变中文路径。
fid = fopen(path,'w','n','UTF-8');
assert(fid >= 0, 'Cannot write %s',path);
cleanup = onCleanup(@() fclose(fid)); %#ok<NASGU>
fprintf(fid,'%s\n',content);
end
