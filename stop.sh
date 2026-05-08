#!/bin/bash
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_NAME="$(basename "$REPO_ROOT")"
COMPOSE_DIR=.sandbox
COMPOSE_FILE="$REPO_ROOT/$COMPOSE_DIR/compose.yml"
COMPOSE_ENV_FILE="$REPO_ROOT/$COMPOSE_DIR/.env"

# shellcheck source=.sandbox/lib/sandbox_lib.sh
source "$REPO_ROOT/$COMPOSE_DIR/lib/sandbox_lib.sh"

ensure_env_file "[stop]"

# --- コンテナ停止 ---
echo "[stop] Stopping containers..."
dc stop "$@"

echo "[stop] Done."
