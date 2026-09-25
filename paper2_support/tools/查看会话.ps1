param(
    [string]$DataRoot = (Join-Path (Split-Path -Parent $PSScriptRoot) '实验数据'),
    [string]$CsvPath
)

# 只读汇总原始 JSONL；不推断实验或参与者数据的有效性。
$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath $DataRoot -PathType Container)) {
    throw "找不到实验数据目录：$DataRoot"
}

$root = (Resolve-Path -LiteralPath $DataRoot).Path
$results = foreach ($file in Get-ChildItem -LiteralPath $root -Filter 'event_log.jsonl' -File -Recurse) {
    $relative = [System.IO.Path]::GetRelativePath($root, $file.DirectoryName)
    $parts = $relative -split '[\\/]'
    $category = if ($parts[0] -like 'session_*') { 'legacy' } else { $parts[0] }
    $counts = @{}
    $lineCount = 0
    $invalidLines = 0

    $stream = [System.IO.File]::Open($file.FullName, [System.IO.FileMode]::Open,
        [System.IO.FileAccess]::Read, [System.IO.FileShare]::ReadWrite)
    try {
        $reader = [System.IO.StreamReader]::new($stream)
        try {
            while ($null -ne ($line = $reader.ReadLine())) {
                $lineCount++
                try {
                    $record = $line | ConvertFrom-Json -ErrorAction Stop
                    if ([string]::IsNullOrWhiteSpace([string]$record.event_type)) {
                        $invalidLines++
                        continue
                    }
                    $eventType = [string]$record.event_type
                    if (-not $counts.ContainsKey($eventType)) { $counts[$eventType] = 0 }
                    $counts[$eventType]++
                }
                catch {
                    $invalidLines++
                }
            }
        }
        finally {
            $reader.Dispose()
        }
    }
    finally {
        $stream.Dispose()
    }

    [pscustomobject]@{
        Category = $category
        Session = $file.Directory.Name
        Events = $lineCount - $invalidLines
        MotionSamples = if ($counts.ContainsKey('flight_motion_sample')) { $counts['flight_motion_sample'] } else { 0 }
        InvalidLines = $invalidLines
        HasSessionEnd = $counts.ContainsKey('session_end')
        LastWriteTime = $file.LastWriteTime
        LogPath = $file.FullName
    }
}

$results = @($results | Sort-Object LastWriteTime -Descending)
if ($CsvPath) {
    $results | Export-Csv -LiteralPath $CsvPath -NoTypeInformation -Encoding utf8
    Write-Host "已写入会话概览：$CsvPath"
}
$results
