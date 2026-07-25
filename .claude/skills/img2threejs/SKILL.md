---
name: img2threejs
description: 画像（イラスト・アセットシート・コンセプトアート）を解析してThree.jsのプロシージャル3Dモデルに再構築し、GLB書き出しボタン付きの単一HTMLとして納品するスキル。「/img2threejs」「画像を3Dにして」「Three.jsでモデル化」「この絵を3Dモデルに」などで必ずこのスキルを使う。
---

# img2threejs — 画像→Three.js 3Dモデル化スキル

画像1枚を入力に、プロポーション・角度・配色を忠実に再現したThree.jsモデルを
**単一HTMLファイル**として生成する。最後にGLB書き出しボタンを付けて
Unity（glTFast）へ持ち込める状態で納品する。

実績: 天守閣（`Projects/assets/GameProject/02_HomeTown/Buildings/2-01_castle_front_threejs.html`）、
木柵（`Downloads/木柵_threejs.html`）。迷ったらこの2つを参照。

## ワークフロー

1. **画像解析**
   - 構造を部位に分解する（例: 石垣→門→階層→屋根→装飾）
   - 配色は画像からスポイトで拾い、コメント付きのパレットオブジェクト `C = {...}` に定数化
   - プロポーションは画像の比率から実寸（メートル基準）に起こす。
     アセットシートに寸法表記があれば必ずそれに従う（例: 高さ1.1m・スパン2m）
   - アセットシートにカラーバリエーションがあれば切り替えボタンとして実装

2. **HTML生成**
   - 同梱の `template.html` をベースに、`/* ===== build ===== */` 部分へモデル構築コードを書く
   - 保存先は**ソース画像と同じフォルダ**、ファイル名は `<元名>_threejs.html`
   - 完成したら `Start-Process "<パス>"` でブラウザを開いて確認してもらう

3. **GLB書き出しボタン**（template.htmlに実装済み）
   - 書き出すのは**モデル本体のグループのみ**（地面・遠景・ライトは含めない）
   - `toNonIndexed()` + `computeVertexNormals()` でフラット法線を焼き込んでから export
     （flatShadingはマテリアルフラグなのでGLBに乗らない。法線を焼かないと滑らかになってしまう）
   - Canvasテクスチャは自動でGLBに埋め込まれる

## モデリングの型（プロシージャル技法）

- **単位はメートル1:1**。Unityにそのままのスケールで入る
- **反り屋根・曲面**: BoxやConeで妥協せずカスタムBufferGeometryを書く。
  リング状に頂点を積み、`lerp(base, top, pow(t, 1.7))` の凹プロファイル＋
  角の反り上がり `lift * pow(u,3) * pow(1-t,2)` が和風屋根の型
- **風化・手作り感**: 頂点ジッター（±0.008〜0.015）＋ `flatShading:true`
- **木目・石畳・石垣**: Canvasで手続き生成 → `CanvasTexture`。
  `colorSpace = THREE.SRGBColorSpace` を忘れない
- **ロープ・縄**: TorusGeometryを3巻き＋結びこぶのSphere。巻きごとに回転を少しばらす
- **色替え**: テクスチャはグレー基調で描き、`material.color` の乗算で色変えする
  （バリエーション切り替えが1マテリアルで済む）
- **発光部**（提灯・窓・紋）: `emissive` + emissiveIntensity。
  夜シーンなら暖色PointLight＋青系DirectionalLight（月光）＋FogExp2で雰囲気を作る
- **オブジェクト数は控えめに**（格子窓はバー数本で表現など）。ローポリ・軽量設計を守る

## 注意点

- CDNは importmap で `three@0.160.0`（jsdelivr）。ローカルHTMLなので外部CDN可
  （Artifactにする場合はCSPで外部CDN不可なので注意）
- 正面図1枚しかない場合、側面・背面は「それっぽく」補完し、納品時にその旨を伝える
- Unity側はGLB読み込みに **glTFast**（`com.unity.cloud.gltfast`）が必要。
  入っていないプロジェクトにはPackage Managerでの追加を案内する
- カラーバリエーションがある場合「書き出したい色を選んでからGLBボタンを押す」と案内する
- モジュール分割Prefab化（部品ごとの個別GLB）は要望があったときだけ提案する
