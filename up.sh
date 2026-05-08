#!/bin/bash
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_NAME="$(basename "$REPO_ROOT")"
COMPOSE_DIR=.sandbox
COMPOSE_FILE="$REPO_ROOT/$COMPOSE_DIR/compose.yml"
COMPOSE_ENV_FILE="$REPO_ROOT/$COMPOSE_DIR/.env"
CERTS_DIR="$REPO_ROOT/$COMPOSE_DIR/proxy/certs"

# shellcheck source=.sandbox/lib/sandbox_lib.sh
source "$REPO_ROOT/$COMPOSE_DIR/lib/sandbox_lib.sh"

# --- usage ---
usage() {
  cat <<EOF
Usage: ./up.sh [OPTIONS]

Options:
  -d            Detach (run in background)
  --build       Rebuild images (with cache)
  --rebuild     Rebuild images from scratch (no cache)
  --reload      Force-recreate mitmproxy only (e.g. after policy.py change or stale mount)
  -h, --help    Show this help

Examples:
  ./up.sh              Start containers (foreground, Ctrl+C to stop)
  ./up.sh -d           Start containers in background
  ./up.sh -d --build   Rebuild and start in background
  ./up.sh --rebuild    Full rebuild (e.g. after changing CONTAINER_USER or CA cert)
  ./up.sh --reload     Recreate mitmproxy (e.g. after policy.py edit or bind mount error)
EOF
  exit 0
}

for arg in "$@"; do
  case "$arg" in -h|--help) usage ;; esac
done

ensure_env_file "[up]"

# --- CA 証明書生成（初回のみ） ---
if [[ ! -f "$CERTS_DIR/ca.key" ]]; then
  echo "[up] Generating CA certificate..."
  mkdir -p "$CERTS_DIR"
  openssl genrsa -out "$CERTS_DIR/ca.key" 4096
  openssl req -new -x509 -days 3650 \
    -key "$CERTS_DIR/ca.key" \
    -out "$CERTS_DIR/ca.crt" \
    -subj "/CN=Sandbox CA/O=Sandbox/C=JP"
  # mitmproxy は key + cert を結合した PEM を confdir/mitmproxy-ca.pem として読む
  cat "$CERTS_DIR/ca.key" "$CERTS_DIR/ca.crt" > "$CERTS_DIR/mitmproxy-ca.pem"
  echo "[up] CA certificate generated at $CERTS_DIR/"
  echo "[up] Note: --rebuild is required to install the cert into the claude-dev image."
fi

gen_gitconfig
ensure_cache_dirs

# --- 引数解析 ---
DETACH=false
BUILD_FLAG=""
NO_CACHE_FLAG=""
RELOAD=false
PASSTHROUGH_ARGS=()
for arg in "$@"; do
  case "$arg" in
    -d)        DETACH=true ;;
    --build)   BUILD_FLAG="--build --force-recreate" ;;
    --rebuild) BUILD_FLAG="--build --force-recreate"; NO_CACHE_FLAG="--no-cache" ;;
    --reload)  RELOAD=true ;;
    *)         PASSTHROUGH_ARGS+=("$arg") ;;
  esac
done

# --- デタッチモード: 既に起動中なら終了 ---
if $DETACH && is_claude_dev_running; then
  echo "[info] Already up. Run ./attach.sh to enter the container."
  exit 0
fi

# --- テスト実行 ---
echo "[up] Running tests..."
if ! python3 "$REPO_ROOT/$COMPOSE_DIR/proxy/tests/test_policy.py" -v 2>&1; then
  echo "[error] Tests failed. Fix the issues above before starting." >&2
  exit 1
fi

# --- DLP プリフライトチェック ---
echo "[up] DLP pre-flight check..."
_DLP_FILES=(
  "$REPO_ROOT/$COMPOSE_DIR/proxy/policy.py"
  "$REPO_ROOT/$COMPOSE_DIR/proxy/filter.py"
  "$REPO_ROOT/$COMPOSE_DIR/proxy/tests/test_policy.py"
  "$REPO_ROOT/$COMPOSE_DIR/compose.yml"
  "$REPO_ROOT/up.sh"
)
if ! PYTHONPATH="$REPO_ROOT/$COMPOSE_DIR/proxy" python3 - "${_DLP_FILES[@]}" << 'PYEOF'
import sys; from pathlib import Path; from policy import _dlp_scan
ok = True
for f in sys.argv[1:]:
    h = _dlp_scan(Path(f).read_text(errors="replace"))
    print(f"  {'✅' if h is None else f'❌ {h[0]}'} {Path(f).name}")
    if h: ok = False
sys.exit(0 if ok else 1)
PYEOF
then
  echo "[error] DLP check failed. Fix the issues above before starting." >&2
  exit 1
fi

# --- mitmproxy だけ force-recreate（--reload 時） ---
if $RELOAD; then
  echo "[up] Force-recreating mitmproxy..."
  dc up -d --force-recreate mitmproxy
  echo "[up] Done."
  exit 0
fi

# --- キャッシュなしビルド（--rebuild 時のみ） ---
if [[ -n "$NO_CACHE_FLAG" ]]; then
  echo "[up] Building with no cache..."
  dc build --no-cache
fi

# --- コンテナ起動 ---
if $DETACH; then
  echo "[up] Starting containers (detached)..."
  dc up -d $BUILD_FLAG "${PASSTHROUGH_ARGS[@]}"
  wait_claude_dev_running
  echo "[up] Done. Run ./attach.sh to enter the container."
else
  echo "[up] Starting containers (foreground, Ctrl+C to stop)..."
  dc up $BUILD_FLAG "${PASSTHROUGH_ARGS[@]}"
fi
