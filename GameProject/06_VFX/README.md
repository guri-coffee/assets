# 06_VFX — 忍術VFX（月影流・雷鳴陣）

`ninjutsu_vfx_editor.html` は **依存ライブラリなしの単一HTML**。
ブラウザで開くだけで動く VFX 制作環境で、構成は参考動画と同じ2段構え。

- **Animation Editor**（下部タイムライン）: 8トラックのクリップを時間軸に並べて演出を組む
- **Composer**（右パネル）: Bloom → Grade → CA → Vignette → Grain のポストチェーン

実装は WebGL2 の生API（`GameProject/06_VFX/ninjutsu_vfx_editor.html` 内に全部入り）。
シーンを HDR バッファに描き、明部抽出 → 1/2・1/4・1/8 の3段ガウスぼかし → 合成、
という Unity の Post-processing / URP Volume と同じ構造にしてある。

## プレビュー

| 集束 (0.9s) | 発破 (1.27s) | 斬撃 (1.87s) |
|---|---|---|
| ![](preview/01_charge.jpg) | ![](preview/02_burst.jpg) | ![](preview/03_slash.jpg) |

通しの動画: [`preview/tsukikage_vfx.webm`](preview/tsukikage_vfx.webm)（1280x720 / 30fps / 3.4s）

## 使い方

1. HTML をブラウザで開く（ローカルファイルのままで可）
2. `Space` または ▶ で再生。タイムラインをドラッグでスクラブ
3. トラック名をクリックで **ソロ切り/オフ**、隣のスライダーで **そのレイヤーの強度**
4. 右パネルで Bloom・グレード・カメラを追い込む
5. `PNG を保存` / `WebM 録画（1ループ）` で書き出し

## タイムライン構成（3.40s / ループ）

| トラック | 時間 | 内容 | Unity での実装先 |
|---|---|---|---|
| 火の粉 / Embers | 0.00–3.40 | 常時漂う霊気の粒 | ParticleSystem（ループ・低密度） |
| 集束 / Charge | 0.00–1.18 | 内側へ巻き込む収束粒子 | ParticleSystem + Velocity over Lifetime（Orbital） |
| 術式陣 / Sigil | 0.12–2.95 | 月桜紋の魔法陣。角度スイープで描き起こし | Shader Graph（極座標＋リング＋回転刻み）を Quad に |
| 発破 / Burst | 1.05–1.42 | 画面フラッシュ＋カメラシェイク | Volume の一時的な露出上げ＋Cinemachine Impulse |
| 衝撃波 / Shockwave | 1.06–2.05 | 地面を走る輪 | Shader Graph（半径アニメ＋Fresnel状の帯） |
| 光柱 / Pillar | 1.02–2.55 | 天へ抜ける光の柱 | 円柱メッシュ＋スクロールノイズ・Fresnel |
| 斬撃 / Slash | 1.52–2.02 | 袈裟斬りの三日月トレイル | ビルボードのアーチメッシュ＋UV スイープ |
| 火花 / Sparks | 1.06–1.85 | 放射状の線状火花 | ParticleSystem（Stretched Billboard） |

各レイヤーは「クリップ内 0→1 の正規化時間」だけを受け取って描く作りなので、
クリップの時間を動かすだけでタイミングを作り直せる。

## プリセット

| | ベース | 用途 |
|---|---|---|
| 雷 Raiden | シアン＋青白 | 本編の基準色（ArtDirection のエフェクトカラーに準拠） |
| 炎 Enbu | 橙＋深紅 | 火遁系 |
| 桜 Sakura | 桃＋金 | 演出・イベント用 |

パレットは `PRESETS` 配列（core / mid / rim / amb の4色）を差し替えるだけで増やせる。

## Unity へ持っていく手順

1. 各トラックを 1 ParticleSystem / 1 Shader Graph に分解（上表の対応）
2. タイミングは Timeline（または Animator）に同じ秒数でクリップを置く
3. ポストは URP Volume に Bloom / Color Adjustments / Chromatic Aberration /
   Vignette / Film Grain を作り、右パネルの数値をそのまま初期値にする
4. 発破のカメラシェイクは Cinemachine Impulse Source を 1.05s に配置

## 既知の制約

- `EXT_color_buffer_float` があれば RGBA16F、無ければ RGBA8 に自動フォールバック
- 録画は `MediaRecorder`（WebM/VP9）。mp4 が必要なら書き出し後に変換する
