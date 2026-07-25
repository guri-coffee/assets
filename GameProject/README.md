# GameProject — TSUKIKAGE ゲーム版 アセット制作管理

Claude Code × Unity MCP × Blender MCP でアセット制作を回すための管理フォルダ。

## 3つの基本ドキュメント

| ファイル | 役割 |
|---|---|
| [`ROADMAP.md`](ROADMAP.md) | Phase構成・依存関係・画像生成順・制作フロー |
| [`ASSET_MASTER_LIST.md`](ASSET_MASTER_LIST.md) | 全アセットの一覧とステータス（**進捗の正はここ**） |
| `00_Direction/` | アートディレクション（世界観・パレット・設定） |

## フォルダ構成

```
GameProject/
├── 00_Direction/        # ArtDirection / ColorPalette / WorldSetting
├── 01_UI/               # TapScreen / Logo / Loading / Icons
├── 02_HomeTown/         # Terrain / Buildings / Nature / Props / Lighting
├── 03_Characters/       # Jin / NPC / Enemy
├── 04_Battle/
├── 05_Audio/
└── (Unity本体は ../unity/tsukikage-village/)
```

各フォルダには「画像リファレンス」「Blender作業ファイル(.blend)」「書き出しglTF」を
アセット番号付きで置く（例: `02_HomeTown/Buildings/2-05_torii_ref.png`）。

## 運用ルール

1. アセットは必ず ASSET_MASTER_LIST の番号で呼ぶ（「2-05を作成して」）
2. 工程が1つ進むたびに ASSET_MASTER_LIST のステータスを更新する
3. 画像生成は必ず `00_Direction/ColorPalette.md` を参照してプロンプトを組む
4. Blender→Unity の受け渡しは glTF (.glb)。スケールは実寸ベース（Jin=約1.7m基準）
5. Unity配置後はGameビュー（夜景ライティング）で色味を確認してから「完了」にする
