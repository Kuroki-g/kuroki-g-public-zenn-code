---
disableModelInvocation: true
---

以下の手順でコミットを実行する。

## Step 1: 現状確認 (並列実行)

- `git status` — 未追跡ファイル・変更ファイルを確認
- `git diff` — staged/unstaged の差分を確認
- `git log --oneline -5` — 直近のコミットメッセージスタイルを確認

## Step 2: コミットメッセージ作成

変更内容を分析し、以下の形式でコミットメッセージを作成する:

- 形式: `<type>(<scope>): <summary>`
- type: fix / feat / refactor / chore / docs
- 1〜2文で「なぜ」を中心に記述

## Step 3: ステージング & コミット

1. 変更ファイルをファイル名指定で `git add` (`.` や `-A` は使わない)
2. `git commit -m "..."` (HEREDOC 形式)
3. `git status` で成功確認
