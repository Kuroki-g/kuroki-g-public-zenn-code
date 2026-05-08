#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_NAME="$(basename "$REPO_ROOT")"
COMPOSE_DIR=.sandbox
COMPOSE_FILE="$REPO_ROOT/$COMPOSE_DIR/compose.yml"
COMPOSE_ENV_FILE="$REPO_ROOT/$COMPOSE_DIR/.env"

# shellcheck source=.sandbox/lib/sandbox_lib.sh
source "$REPO_ROOT/$COMPOSE_DIR/lib/sandbox_lib.sh"

ensure_env_file "[attach]"

# ---------- usage ----------
usage() {
  cat <<EOF
Usage: ./attach.sh [COMMAND]

Commands:
  (none)      Interactive bash shell  [default]
  vim         Launch vim
  nvim        Launch nvim
  claude, cl  Launch claude
  log         Tail mitmproxy access log
  -h, --help  Show this help

Examples:
  ./attach.sh
  ./attach.sh claude
  ./attach.sh log
EOF
  exit 0
}

# ---------- parse args ----------
CMD="${1:-}"
case "$CMD" in
  ""|bash)     EXEC_CMD=(bash) ;;
  vim)         EXEC_CMD=(vim) ;;
  nvim)        EXEC_CMD=(nvim) ;;
  claude|cl)   EXEC_CMD=(claude) ;;
  log)         EXEC_CMD=(tail -F /var/log/mitmproxy/access.log) ;;
  -h|--help)   usage ;;
  *)
    echo "[error] Unknown command: '$CMD'" >&2
    echo "        Run ./attach.sh --help for usage." >&2
    exit 1
    ;;
esac

# ---------- guard: container must be running ----------
if ! is_claude_dev_running; then
  echo "[error] claude-dev container is not running." >&2
  echo "        Run ./up.sh first." >&2
  exit 1
fi

# ---------- attach ----------
echo "[attach] Attaching to claude-dev (${EXEC_CMD[*]})..."
dc exec -u "1000:1000" claude-dev "${EXEC_CMD[@]}"

# ---------- teardown prompt (bash のみ) ----------
if [[ "${EXEC_CMD[0]}" == "bash" ]]; then
  echo ""
  read -r -p "[attach] Stop containers? [y/N]: " answer
  case "$answer" in
    [yY]*)
      echo "[attach] Stopping containers..."
      dc stop
      echo "[attach] Containers stopped."
      ;;
    *)
      echo "[attach] Containers left running. Run ./attach.sh to reconnect."
      ;;
  esac
fi
