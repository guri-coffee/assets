# TSUKIKAGE – Stylized Japanese Night Village

Unity Asset Store 販売用の和風ナイトビレッジ環境アセットのプロジェクト。
仕様は [`docs/field-spec-ninja-village.md`](../../docs/field-spec-ninja-village.md)、販売計画は [`docs/unity-asset-store-sales-plan.md`](../../docs/unity-asset-store-sales-plan.md) を参照。

## セットアップ手順

1. **Unity Hub で Unity 2022.3 LTS をインストール**（パッチバージョンは最新でOK。プロジェクトは 2022.3.55f1 を指定しているが、アップグレードダイアログが出たらそのまま進めてよい）
2. Unity Hub の「Add」からこのフォルダ（`unity/tsukikage-village`）を開く。初回はパッケージ解決に数分かかる
3. メニューの **Tools > Tsukikage** から以下を順に実行:
   1. **Setup URP + Folders** — URP パイプラインアセットの生成・適用、Linear カラースペース設定、フォルダ構成の作成
   2. **Create Night Demo Scene** — 月光・藍色の環境光・霧・アイソメ風カメラ・ポストプロセス（Bloom / Color Adjustments / White Balance / Vignette）を設定済みの夜景シーンを生成
   3. **Generate Greybox Blockout** — 参考構図（3段の台地＋天守＋町屋＋鳥居＋水面＋提灯ライト）のグレーボックスを生成

3つ実行して Game ビューを見ると、夜景ライティングの方向性がその場で確認できる。

## フォルダ構成（Setup 実行後）

```
Assets/Tsukikage/
├── Models/          # Terrain / Castle / Houses / Bridges / Props / Vegetation
├── Materials/       # 共通マテリアル（Blockout/ はグレーボックス用）
├── Textures/        # テクスチャアトラス
├── Prefabs/         # 販売パッケージに含めるプレハブ
├── Scenes/          # デモシーン
├── Shaders/         # Shader Graph（水面 / 滝 / ホログラム）
├── VFX/             # パーティクル（滝しぶき / 霧 / 桜 / ホタル）
├── Settings/        # URP・ポストプロセス設定
└── Demo/            # ストア用デモシーン素材
```

## 制作の進め方

1. ブロックアウトの構図を調整して確定（カメラから見た絵で判断する）
2. `docs/field-spec-ninja-village.md` のキット構成に沿って、地形・岩 → 町屋 → 城 の順にモデルを差し替えていく
3. グレーボックスの `Blockout_*` マテリアルは本番マテリアル完成後に破棄

## ブランド・権利面のルール

- 看板・紋章・文言はすべてオリジナル（ブランド名 **TSUKIKAGE / 月影の里**、紋章は三日月＋桜の「月桜紋」）
- NFT / DAO / 実在ブランドの文言・ロゴは**一切使用しない**（Asset Store 審査で差し戻し対象）
- ホログラム看板はテクスチャ差し替え可能なスロットとして実装する

## トラブルシューティング

- **画面がピンク色**: URP セットアップ前にシーンを開いた状態。`Tools > Tsukikage > 1. Setup URP + Folders` を実行する
- **Setup スクリプトでエラーが出る場合**: 手動で `Assets > Create > Rendering > URP Asset (with Universal Renderer)` を作成し、`Project Settings > Graphics` に割り当ててもよい
