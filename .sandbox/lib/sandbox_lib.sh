# sandbox_lib.sh — v1 共通ライブラリ
# source "$REPO_ROOT/.sandbox/lib/sandbox_lib.sh" で読み込む
#
# 前提: 呼び出し元スクリプトで以下の変数が設定済みであること:
#   REPO_ROOT, PROJECT_NAME, COMPOSE_DIR, COMPOSE_FILE, COMPOSE_ENV_FILE
#
# export は使わない。dc() 内で env コマンドで必要な変数だけ子プロセスへ渡す。
# COMPOSE_PROJECT_NAME は --project-name フラグで渡す（export 不要）。

# docker compose のコモンフラグラッパー（v1 専用プロジェクト名を常に付与）
dc() {
  env \
    REPO_ROOT="$REPO_ROOT" \
    PROJECT_NAME="$PROJECT_NAME" \
    COMPOSE_DIR="$COMPOSE_DIR" \
    docker compose \
    --project-name "${PROJECT_NAME}-v1" \
    --env-file "$COMPOSE_ENV_FILE" \
    -f "$COMPOSE_FILE" \
    "$@"
}

# .env がなければ .env.example からコピー
# 引数: [LOG_PREFIX]  例: "[up]"
ensure_env_file() {
  local prefix="${1:-[setup]}"
  if [[ ! -f "$COMPOSE_ENV_FILE" ]]; then
    echo "$prefix $COMPOSE_ENV_FILE not found. Copying from .env.example..."
    cp "$REPO_ROOT/$COMPOSE_DIR/.env.example" "$COMPOSE_ENV_FILE"
    echo "$prefix Created. Edit $COMPOSE_ENV_FILE if needed."
  fi
}

# .gitconfig を $COMPOSE_DIR 下に生成
gen_gitconfig() {
  local dest="$REPO_ROOT/$COMPOSE_DIR/.gitconfig"
  local git_name git_email
  git_name="$(git config --global user.name 2>/dev/null || true)"
  git_email="$(git config --global user.email 2>/dev/null || true)"
  cat > "$dest" <<EOF
[user]
	name = ${git_name}
	email = ${git_email}
EOF
  echo "[up] Generated $dest (name=${git_name:-<unset>}, email=${git_email:-<unset>})"
}

# ビルドキャッシュディレクトリを事前作成
ensure_cache_dirs() {
  mkdir -p "$HOME/.cache/uv"
  mkdir -p "$HOME/.cache/go"
  mkdir -p "$HOME/go/pkg/mod"
}

# claude-dev が running かどうかを判定（終了コードで返す）
is_claude_dev_running() {
  local cid
  cid="$(dc ps -q claude-dev 2>/dev/null || true)"
  [[ -n "$cid" ]] && docker inspect -f '{{.State.Running}}' "$cid" 2>/dev/null | grep -q "true"
}

# claude-dev が running になるまで待機（最大 60 秒）
wait_claude_dev_running() {
  local cid timeout=60 elapsed=0
  cid="$(dc ps -q claude-dev 2>/dev/null || true)"
  if [[ -z "$cid" ]]; then
    echo "[error] claude-dev service not found." >&2
    return 1
  fi
  echo "[up] Waiting for claude-dev (${cid:0:12}) to be running..."
  until docker inspect -f '{{.State.Running}}' "$cid" 2>/dev/null | grep -q "true"; do
    sleep 1
    (( elapsed++ ))
    if (( elapsed >= timeout )); then
      echo "[error] Timed out waiting for claude-dev." >&2
      return 1
    fi
  done
}
