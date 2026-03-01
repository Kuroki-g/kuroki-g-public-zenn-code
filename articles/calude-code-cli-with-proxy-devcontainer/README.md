# devcontainer for Claude Code (Squid)

Squidプロキシによるアウトバウンド制限付きDevContainer設定です。

## 構成

- `Dockerfile` — 開発コンテナ本体
- `compose.yml` — Squidプロキシコンテナとのネットワーク構成
- `squid.conf` — ドメインホワイトリスト設定
- `devcontainer.json` — VS Code DevContainer設定
- `change-network.sh` — `claude login` のOAuth認証用ポートリバインドスクリプト

## 使い方

1. `.devcontainer/` をリポジトリに配置
2. `squid.conf` の `allowed_domains` にプロジェクトに必要なドメインを追加
3. VS Code でDevContainerを起動
4. 初回ログイン時は `change-network.sh` を別タブで実行してからclaudeでログイン

## License

Apache 2.0 — See [LICENSE](../LICENSE)
