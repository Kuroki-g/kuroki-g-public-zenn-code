# Wasm sample

適当なサンプルです。

## 環境

- mise
- Go 1.24.1

## コマンド

ブラウザで読み込む場合のサンプル

```bash
mise run serve
```

ビルド

```bash
mise run build
```

```bash
GOOS=js GOARCH=wasm go build -o server/wasm/main.wasm main.go 
```
