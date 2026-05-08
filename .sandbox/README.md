# `.sandbox/` — mitmproxy ベース ★普段使い

mitmproxy が TLS を復号・検査する透過プロキシ。
初回起動時に自己署名 CA を生成し、claude-dev イメージに組み込む。

## 操作

```bash
./up.sh              # フォアグラウンド起動（Ctrl+C で停止）
./up.sh -d           # バックグラウンド起動
./up.sh -d --build   # 再ビルドしてバックグラウンド起動
./up.sh --rebuild    # キャッシュなし完全再ビルド（ユーザー名変更・CA 更新後）
./attach.sh          # bash で入る
./attach.sh claude   # claude を直接起動
./attach.sh log      # mitmproxy アクセスログを tail
./stop.sh            # コンテナ停止
```

※ スクリプトはリポジトリルートから実行する。

## 初回セットアップ

```bash
cp .sandbox/.env.example .sandbox/.env
# .env を編集: CONTAINER_USER を設定
./up.sh --rebuild    # CA 証明書生成 + イメージビルド
```

> CA 証明書は `certs/` に生成される（git 管理外）。
> `CONTAINER_USER` を変えたり CA を再生成した場合は `--rebuild` が必要。

## ポリシー（`proxy/policy.py`）

ドメインごとに「許可 HTTP メソッド」と「DLP 適用有無」を設定：

```python
POLICIES = {
    "anthropic.com":  DomainPolicy(methods={"GET", "POST", "HEAD"}),  # POST 許可
    "github.com":     DomainPolicy(),                                  # GET/HEAD のみ
    ...
}
```

- リストにないドメインは **全ブロック**
- `methods` 外のメソッドは 405 で返す
- DLP はリクエスト URL・ボディ内の APIキー/トークン等を検出して 400 でブロック

## ファイル構成

```
.sandbox/
├── compose.yml
├── Dockerfile
├── lib/
├── proxy/
│   ├── mitmproxy.Dockerfile
│   ├── filter.py          # mitmproxy addon（ポリシー適用・OTel 計装）
│   ├── policy.py          # ドメインポリシー定義
│   ├── entrypoint.sh      # mitmproxy 起動スクリプト
│   └── tests/             # policy.py のユニットテスト
└── certs/                 # CA 証明書（初回生成・git 管理外）
```
