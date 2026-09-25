@echo off
setlocal
if "%~1"=="" (
  echo Usage: start_data_viewer_windows.cmd "C:\path\to\AudioV0"
  exit /b 2
)
if not exist "%~1\" (
  echo AudioV0 data root does not exist: %~1
  exit /b 2
)
py -3 "%~dp0audio_v0_data_server.py" --data-root "%~1" --port 8091
