function plot_m1_paper_figure()
%PLOT_M1_PAPER_FIGURE 从既有 M1 WAV 绘制可复现的论文图，不重合成或修改音频。
% 波形、全文件 FFT 与 STFT 使用共享绝对数字标尺，不做逐文件最大值归一化。
% 输出说明声音结构，不能用于推断感知响度、低紧迫性或辨识表现。

root = fileparts(mfilename('fullpath'));
outputRoot = fullfile(root, 'paper_figure');
if ~isfolder(outputRoot), mkdir(outputRoot); end
targets = {'T01_cruise_entry', 'T02_descent_begin', 'T03_landing_preparation'};
labels = {'T01  Entering cruise', 'T02  Beginning descent', 'T03  Preparing to land'};
designs = {'Rising contour: 330 to 495 Hz', 'Falling contour: 495 to 330 Hz', 'Two pulses: 392 Hz'};
allWavs = dir(fullfile(root, '*', '*.wav'));
beforeHashes = cell(numel(allWavs), 1);
for k = 1:numel(allWavs)
    beforeHashes{k} = fileSha256(fullfile(allWavs(k).folder, allWavs(k).name));
end

config = struct('file_duration_s', 1.8, 'sample_rate_hz', 44100, ...
    'fft_window', 'symmetric Hann over all 79380 samples', ...
    'fft_nfft', 131072, 'stft_window', 'symmetric Hann', ...
    'stft_window_samples', 2048, 'stft_hop_samples', 256, ...
    'stft_nfft', 4096, 'amplitude_reference', 1, ...
    'normalization', 'abs(FFT(x.*w))/sum(w); double interior one-sided bins only', ...
    'db_formula', '20*log10(max(single_sided_amplitude,1e-6))', ...
    'frequency_limits_hz', [0 1200], 'time_limits_s', [0 1.8], ...
    'waveform_limits', [-0.45 0.45], 'fft_limits_db', [-100 0], ...
    'stft_limits_db', [-80 -5], 'per_file_peak_normalization', false, ...
    'window_zero_padding_note', 'Zero padding samples the spectrum more densely; it does not increase intrinsic frequency resolution.', ...
    'figure_width_mm', 190, 'figure_height_mm', 164);
sourceAudit = struct([]);
spectralData = struct([]);
fig = figure('Visible', 'off', 'Color', 'white', 'Units', 'centimeters', ...
    'Position', [1 1 config.figure_width_mm/10 config.figure_height_mm/10], ...
    'Renderer', 'painters');
set(fig,'PaperUnits','centimeters','PaperSize',[config.figure_width_mm config.figure_height_mm]/10, ...
    'PaperPosition',[0 0 config.figure_width_mm config.figure_height_mm]/10, ...
    'PaperPositionMode','manual');
cleanup = onCleanup(@() close(fig)); %#ok<NASGU>
set(fig, 'DefaultAxesFontName', 'Arial', 'DefaultTextFontName', 'Arial', ...
    'DefaultAxesFontSize', 8, 'DefaultTextFontSize', 8);
layout = tiledlayout(fig, 3, 3, 'TileSpacing', 'compact', 'Padding', 'compact');
title(layout, 'M1 non-speech notification family', 'FontSize', 11, 'FontWeight', 'bold');
subtitle(layout, 'Measured from the three exported WAV files; identical scales within each column', ...
    'FontSize', 8, 'Color', [0.25 0.25 0.25]);
lineColor = [0.12 0.35 0.51];
lightColor = [0.68 0.77 0.83];
axesHandles = gobjects(3,3);

for k = 1:3
    path = fullfile(root, 'M1_two_pulse', [targets{k} '.wav']);
    [x, fs] = audioread(path);
    info = audioinfo(path);
    assert(fs == config.sample_rate_hz && info.NumChannels == 1 && info.BitsPerSample == 16);
    assert(numel(x) == round(config.file_duration_s*fs));
    assert(all(isfinite(x)) && max(abs(x)) < 1);
    time = (0:numel(x)-1)'/fs;

    % 全文件 Hann FFT 是有限时长信号的频率内容，并非声压级或响度。
    wholeWindow = symmetricHann(numel(x));
    wholeAmplitude = oneSidedAmplitude(x.*wholeWindow, config.fft_nfft, sum(wholeWindow));
    frequency = (0:config.fft_nfft/2)'*fs/config.fft_nfft;
    spectrumDb = 20*log10(max(wholeAmplitude, 1e-6));

    % 每个短时帧使用相同窗口与参考值；不按帧或文件单独归一化。
    frameLength = config.stft_window_samples;
    starts = 1:config.stft_hop_samples:numel(x)-frameLength+1;
    frameWindow = symmetricHann(frameLength);
    frameAmplitude = zeros(config.stft_nfft/2+1, numel(starts));
    for frameIndex = 1:numel(starts)
        segment = x(starts(frameIndex):starts(frameIndex)+frameLength-1);
        frameAmplitude(:,frameIndex) = oneSidedAmplitude( ...
            segment.*frameWindow, config.stft_nfft, sum(frameWindow));
    end
    frameFrequency = (0:config.stft_nfft/2)'*fs/config.stft_nfft;
    frameTime = ((starts-1)+(frameLength-1)/2)/fs;
    frameDb = 20*log10(max(frameAmplitude, 1e-6));

    ax = nexttile(layout, (k-1)*3+1);
    axesHandles(k,1) = ax;
    plot(ax, time, x, 'Color', lightColor, 'LineWidth', 0.2);
    % 5 ms 极值范围保持原样，深色边界提高缩版后的包络可见性。
    frameSize = round(0.005*fs);
    edges = 1:frameSize:numel(x);
    minimum = zeros(size(edges)); maximum = zeros(size(edges));
    for binIndex = 1:numel(edges)
        segment = x(edges(binIndex):min(numel(x), edges(binIndex)+frameSize-1));
        minimum(binIndex) = min(segment); maximum(binIndex) = max(segment);
    end
    hold(ax,'on');
    binTime = ((edges-1)+frameSize/2)/fs;
    plot(ax, binTime, maximum, 'Color', lineColor, 'LineWidth', 0.5);
    plot(ax, binTime, minimum, 'Color', lineColor, 'LineWidth', 0.5);
    xlim(ax,config.time_limits_s); ylim(ax,config.waveform_limits);
    xticks(ax,[0 0.6 1.2 1.8]); yticks(ax,[-0.4 0 0.4]);
    xlabel(ax,'Time (s)'); ylabel(ax,'Sample amplitude');
    title(ax,{sprintf('%c  %s',char('a'+(k-1)*3),labels{k}),designs{k}}, ...
        'FontSize',8,'FontWeight','normal');

    ax = nexttile(layout,(k-1)*3+2);
    axesHandles(k,2) = ax;
    plot(ax,frequency,spectrumDb,'Color',lineColor,'LineWidth',0.75);
    xlim(ax,config.frequency_limits_hz); ylim(ax,config.fft_limits_db);
    xticks(ax,[0 400 800 1200]); yticks(ax,[-100 -50 0]);
    xlabel(ax,'Frequency (Hz)'); ylabel(ax,'Amplitude (dB re 1)');
    title(ax,sprintf('%c  Whole-file FFT',char('b'+(k-1)*3)), ...
        'FontSize',8,'FontWeight','normal');

    ax = nexttile(layout,(k-1)*3+3);
    axesHandles(k,3) = ax;
    imagesc(ax,frameTime,frameFrequency,frameDb);
    axis(ax,'xy'); xlim(ax,config.time_limits_s); ylim(ax,config.frequency_limits_hz);
    clim(ax,config.stft_limits_db);
    xticks(ax,[0 0.6 1.2 1.8]); yticks(ax,[0 400 800 1200]);
    xlabel(ax,'Time (s)'); ylabel(ax,'Frequency (Hz)');
    title(ax,sprintf('%c  Short-time spectrum',char('c'+(k-1)*3)), ...
        'FontSize',8,'FontWeight','normal');

    row = struct('target',targets{k},'relative_path',['M1_two_pulse/' targets{k} '.wav'], ...
        'sha256',fileSha256(path),'sample_rate_hz',fs,'samples',numel(x), ...
        'duration_s',numel(x)/fs,'digital_rms',sqrt(mean(x.^2)), ...
        'digital_peak',max(abs(x)),'fft_peak_amplitude_db_re_1',max(spectrumDb), ...
        'stft_peak_amplitude_db_re_1',max(frameDb,[],'all'), ...
        'stft_frames',numel(starts));
    if isempty(sourceAudit), sourceAudit=row; else, sourceAudit(k)=row; end %#ok<AGROW>
    dataRow = struct('target',targets{k},'time_s',time,'samples',x, ...
        'fft_frequency_hz',frequency,'fft_amplitude_db_re_1',spectrumDb, ...
        'stft_time_s',frameTime,'stft_frequency_hz',frameFrequency, ...
        'stft_amplitude_db_re_1',frameDb);
    if isempty(spectralData), spectralData=dataRow; else, spectralData(k)=dataRow; end %#ok<AGROW>
end

for k=1:numel(axesHandles)
    ax=axesHandles(k);
    ax.Box='off'; ax.TickDir='out'; ax.LineWidth=0.5;
    ax.XColor=[0.2 0.2 0.2]; ax.YColor=[0.2 0.2 0.2];
    ax.XGrid='off'; ax.YGrid='off';
end
colormap(fig,parula(256));
cb=colorbar(axesHandles(3,3));
cb.Layout.Tile='east'; cb.FontSize=8; cb.Ticks=[-80 -60 -40 -20 -5];
cb.Label.String='Short-time amplitude (dB re 1)'; cb.Label.FontSize=8;

stem='m1-waveform-fft-stft';
pngPath=fullfile(outputRoot,[stem '.png']);
pdfPath=fullfile(outputRoot,[stem '.pdf']);
% print 的显式纸张尺寸避免 macOS Retina 对 exportgraphics 物理尺寸的影响。
print(fig,pngPath,'-dpng','-r600');
print(fig,pdfPath,'-dpdf','-painters','-r600');
save(fullfile(outputRoot,'spectral_source_data.mat'),'config','sourceAudit','spectralData','-v7');
for k = 1:numel(allWavs)
    assert(strcmp(beforeHashes{k},fileSha256(fullfile(allWavs(k).folder,allWavs(k).name))), ...
        'Existing WAV changed during plotting.');
end
metadata=struct('status','author_selected_engineering_candidate_human_validation_pending', ...
    'matlab_version',version,'created_at_utc', ...
    char(datetime('now','TimeZone','UTC','Format',"yyyy-MM-dd'T'HH:mm:ss'Z'")), ...
    'plot_generator_sha256',fileSha256(mfilename('fullpath')+".m"), ...
    'config',config,'sources',sourceAudit,'existing_wavs_unchanged',numel(allWavs), ...
    'pdf_sha256',fileSha256(pdfPath),'png_sha256',fileSha256(pngPath), ...
    'boundary','Signal structure only; no listener discrimination, urgency, loudness, or SPL inference.');
writeUtf8(fullfile(outputRoot,'figure_audit.json'),jsonencode(metadata,'PrettyPrint',true));
writetable(struct2table(sourceAudit),fullfile(outputRoot,'source_summary.csv'));
fprintf('M1_PAPER_FIGURE_PASS: %s\n',pdfPath);
fprintf('MATLAB: %s; unchanged existing WAVs: %d\n',version,numel(allWavs));
end

function w=symmetricHann(n)
%SYMMETRICHANN 不依赖 Signal Processing Toolbox，明确采用对称 Hann。
w=0.5-0.5*cos(2*pi*(0:n-1)'/(n-1));
end

function amplitude=oneSidedAmplitude(windowedSamples,nfft,windowSum)
%ONESIDEDAMPLITUDE 用窗口 coherent gain 校正，DC/Nyquist 不翻倍。
assert(mod(nfft,2)==0 && windowSum>0);
amplitude=abs(fft(windowedSamples,nfft))/windowSum;
amplitude=amplitude(1:nfft/2+1);
amplitude(2:end-1)=2*amplitude(2:end-1);
end

function hash=fileSha256(path)
%FILESHA256 对原文件字节计算 checksum，以区分来源和绘图输出。
fid=fopen(path,'rb'); assert(fid>=0,'Cannot read %s',path);
cleanup=onCleanup(@() fclose(fid)); %#ok<NASGU>
bytes=fread(fid,Inf,'*uint8');
digest=java.security.MessageDigest.getInstance('SHA-256');
digest.update(bytes);
hash=lower(reshape(dec2hex(typecast(digest.digest(),'uint8'),2)',1,[]));
end

function writeUtf8(path,content)
%WRITEUTF8 明确文本编码并在结束时关闭审计文件。
fid=fopen(path,'w','n','UTF-8'); assert(fid>=0,'Cannot write %s',path);
cleanup=onCleanup(@() fclose(fid)); %#ok<NASGU>
fprintf(fid,'%s\n',content);
end
