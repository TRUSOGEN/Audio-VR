function audit_speech_exports()
%AUDIT_SPEECH_EXPORTS 用 MATLAB 解码 V3/V4 实际 MP3，保存数字审计和对照图。
% 只读取 raw；不播放、不修剪、不归一化，不推断可懂度或耳侧响度。
root = fileparts(mfilename('fullpath'));
auditRoot = fullfile(root,'..','..','temp','speech_v4_20260923');
if ~isfolder(auditRoot), mkdir(auditRoot); end
production = jsondecode(fileread(fullfile(root,'production_record.json')));
oldNames = {'T01_cruising_altitude_raw.mp3','T02_descent_raw.mp3','T03_landing_raw.mp3'};
oldText = {'Flight update. Cruising altitude reached.', ...
    'Flight update. Descent will begin shortly.', 'Flight update. Prepare for landing.'};
records = struct([]);
fprintf('MATLAB %s\nDecoder: audioread, unmodified provider MP3 files.\n',version);
for k = 1:3
    sourcePaths = {fullfile(root,'..','elevenlabs_broadcast_v3','raw',oldNames{k}), ...
        fullfile(root,production.files(k).raw_file)};
    fig = figure('Visible','off','Color','white','Position',[100 100 1500 720]);
    layout = tiledlayout(2,2,'TileSpacing','compact','Padding','compact');
    title(layout,sprintf('T%02d | V3 / V4 digital waveform comparison; no listening validation',k));
    for v = 1:2
        path = sourcePaths{v};
        hashBefore = fileSha256(path);
        if v == 2
            assert(strcmp(hashBefore,production.files(k).sha256),'V4 source hash mismatch.');
            versionLabel = 'V4_candidate'; text = production.files(k).text; speed = 0.90;
            relativePath = production.files(k).raw_file;
        else
            versionLabel = 'V3_reference'; text = oldText{k}; speed = 1.00;
            relativePath = ['../elevenlabs_broadcast_v3/raw/' oldNames{k}];
        end
        [samples,fs] = audioread(path);
        info = audioinfo(path);
        assert(~isempty(samples) && all(isfinite(samples(:))),'Decode failed or non-finite samples.');
        assert(fs == 44100 && size(samples,2) == 1,'Unexpected MP3 format; inspect before continuing.');
        assert(strcmp(hashBefore,fileSha256(path)),'Raw file changed during read.');
        [leading,trailing,activeStart,activeEnd,frameTimes,frameRmsDb] = silenceBoundaries(samples,fs);
        values = samples(:); peak = max(abs(values)); rmsValue = sqrt(mean(values.^2));
        row = struct('version',versionLabel,'target',sprintf('T%02d',k),'text',text, ...
            'speed_ui',speed,'relative_path',relativePath,'sha256',hashBefore, ...
            'sample_rate_hz',fs,'channels',size(samples,2),'decoded_samples',size(samples,1), ...
            'decoded_duration_s',size(samples,1)/fs,'audioinfo_duration_s',info.Duration, ...
            'digital_rms',rmsValue,'digital_rms_dbfs',20*log10(rmsValue), ...
            'digital_peak',peak,'digital_peak_dbfs',20*log10(peak), ...
            'decoded_abs_ge_1_samples',sum(abs(values)>=1), ...
            'decoded_abs_ge_0p999_samples',sum(abs(values)>=0.999), ...
            'dc_mean',mean(values),'leading_below_threshold_s',leading, ...
            'trailing_below_threshold_s',trailing,'active_start_s',activeStart, ...
            'active_end_s',activeEnd,'silence_threshold_dbfs',-50,'silence_frame_ms',10);
        if isempty(records), records=row; else, records(end+1)=row; end %#ok<AGROW>
        % 保存解码浮点样本作独立数值复核；无需二次有损编码或音量处理。
        binaryPath = fullfile(auditRoot,[versionLabel '_' row.target '_decoded_float64.bin']);
        fid=fopen(binaryPath,'w','ieee-le'); assert(fid>=0); fwrite(fid,values,'double'); fclose(fid);
        fprintf('%s %s: %.6f s, %d Hz, RMS=%.7f, peak=%.7f, full-scale samples=%d, low-energy edges=%.3f/%.3f s, sha256=%s\n', ...
            versionLabel,row.target,row.decoded_duration_s,fs,rmsValue,peak,row.decoded_abs_ge_1_samples,leading,trailing,hashBefore);
        nexttile(v); plot((0:numel(values)-1)/fs,values,'Color',[0.12 0.34 0.45]);
        title(sprintf('%s | UI speed %.2f',strrep(versionLabel,'_',' '),speed));
        xlabel('Time (s)'); ylabel('Decoded amplitude'); ylim([-1 1]); xlim([0 4.5]); grid on;
        nexttile(v+2); plot(frameTimes,frameRmsDb,'Color',[0.12 0.34 0.45]);
        yline(-50,'--','-50 dBFS threshold'); xlabel('Time (s)'); ylabel('10 ms frame RMS (dBFS)');
        ylim([-100 0]); xlim([0 4.5]); grid on;
    end
    exportgraphics(fig,fullfile(root,sprintf('T%02d_waveform_comparison.png',k)),'Resolution',150);
    close(fig);
end
metadata = struct('status','digital_decode_audit_only_human_validation_pending', ...
    'matlab_version',version,'decoder','MATLAB audioread', ...
    'audited_at_utc',char(datetime('now','TimeZone','UTC','Format',"yyyy-MM-dd'T'HH:mm:ss'Z'")), ...
    'generator_sha256',fileSha256(mfilename('fullpath')+".m"), ...
    'silence_definition','First/last non-overlapping 10 ms frame with RMS > -50 dBFS; threshold estimate, not phonetic/acoustic onset.', ...
    'clipping_boundary','Decoded abs>=1 count; a zero count does not exclude upstream clipping or codec artifacts.', ...
    'records',records);
fid=fopen(fullfile(root,'waveform_audit.json'),'w','n','UTF-8'); assert(fid>=0);
fprintf(fid,'%s\n',jsonencode(metadata,'PrettyPrint',true)); fclose(fid);
writetable(struct2table(records),fullfile(root,'waveform_audit.csv'));
fprintf('PAPER2_SPEECH_DECODE_AUDIT_PASS: all 6 raw MP3 files read without mutation.\n');
end

function [leading,trailing,startTime,endTime,frameTimes,frameRmsDb] = silenceBoundaries(samples,fs)
%SILENCEBOUNDARIES 用预先明确的窗长/绝对阈值描述低能量边缘，不自动裁剪。
frameLength = round(0.010*fs);
starts = 1:frameLength:numel(samples);
frameRms = zeros(size(starts));
for j=1:numel(starts)
    frame=samples(starts(j):min(starts(j)+frameLength-1,numel(samples)));
    frameRms(j)=sqrt(mean(frame.^2));
end
active=find(frameRms>10^(-50/20));
assert(~isempty(active),'No frame exceeds the stated speech activity threshold.');
startTime=(starts(active(1))-1)/fs;
endTime=min(starts(active(end))+frameLength-1,numel(samples))/fs;
leading=startTime; trailing=numel(samples)/fs-endTime;
frameTimes=((starts-1)+frameLength/2)/fs;
frameRmsDb=20*log10(max(frameRms,1e-6));
end

function hash = fileSha256(path)
%FILESHA256 哈希原始文件，防止将新导出与旧版本混淆。
fid=fopen(path,'rb'); assert(fid>=0,'Cannot read %s',path);
cleanup=onCleanup(@() fclose(fid)); %#ok<NASGU>
bytes=fread(fid,Inf,'*uint8');
digest=java.security.MessageDigest.getInstance('SHA-256'); digest.update(bytes);
hash=lower(reshape(dec2hex(typecast(digest.digest(),'uint8'),2)',1,[]));
end
