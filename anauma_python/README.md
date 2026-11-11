# anauma_python_refactored

競馬穴馬AIのPython側を役割別に分割した構成です。

## 構成

- `main.py` : 全処理の入口。順番だけ管理
- `config.py` : DB・期間・公開条件・出力先
- `database.py` : MySQLからデータ取得
- `features.py` : 特徴量作成
- `modeling.py` : LightGBM学習・予測
- `evaluation.py` : バックテスト・評価表示
- `exporter.py` : WordPress用JSON出力
- `output/` : JSON出力先

## 今回追加した特徴量

- `HorseTrackTop3Rate`
  - 馬自身の同競馬場における「今回より前」の3着内率
- `HorseTrackStarts`
  - 上記率の根拠になった過去出走数

## 重要

`config.py` の `DATABASE_URL` のパスワードを自分の環境に合わせて変更してください。

## 実行

```bash
python main.py
```

## JSON

### output/anauma_predictions.json
現在のWordPressショートコードと互換のフラット形式です。

### output/anauma_predictions_v2.json
将来の血統・タイム・適性・試行回数などを追加しやすい構造化JSONです。

## 次の実装候補

`main.py` 実行時に `TrackType` の実値を表示します。
その値を確認後、`馬自身 × 芝/ダート` を `features.py` に追加します。
父情報がDBに入った後は `父 × 競馬場`、`父 × 距離` を同じ場所に追加します。
