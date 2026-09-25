#!/bin/zsh
# 双击后同时打开 Audio V0 的实际日志目录与本地查看器。

set -eu

COLLECTION_ROOT="$HOME/AudioV0_Data"
DATA_ROOT="$COLLECTION_ROOT/technical_demo"
SERVER="$(cd "$(dirname "$0")" && pwd)/audio_v0_data_server.py"
PORT="8091"

mkdir -p "$DATA_ROOT"
print "数据目录：$DATA_ROOT（Unity 后续运行会自动写入）"

if ! curl --silent --fail "http://127.0.0.1:$PORT/api/health" >/dev/null 2>&1; then
  nohup /usr/bin/python3 "$SERVER" --data-root "$COLLECTION_ROOT" --port "$PORT" >"/tmp/audio-v0-data-server.log" 2>&1 &
  for attempt in {1..20}; do
    if curl --silent --fail "http://127.0.0.1:$PORT/api/health" >/dev/null 2>&1; then
      break
    fi
    sleep 0.2
  done
fi

# 避免旧服务仍在读取历史隐藏目录却看似正常启动。
if ! curl --silent --fail "http://127.0.0.1:$PORT/api/health" | /usr/bin/python3 -c 'import json, sys; data = json.load(sys.stdin); sys.exit(0 if data.get("data_root") == sys.argv[1] and data.get("data_root_exists") else 1)' "$DATA_ROOT"; then
  print -u2 "数据服务未就绪或仍使用旧目录，请检查 8091 端口及 /tmp/audio-v0-data-server.log。"
  exit 1
fi

open "$DATA_ROOT"
open "http://127.0.0.1:$PORT/"
