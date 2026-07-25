# Asset Production Roadmap v1.0

**プロジェクト**: TSUKIKAGE ゲーム版（tsukikage-village をゲーム化）
**開発方式**: Claude Code × Unity MCP × Blender MCP
**Unityプロジェクト**: `Projects/assets/unity/tsukikage-village`（Unity 6000.5.3f1 / URP）

依存関係順にPhaseを並べる。前のPhaseの土台が固まってから次に進む。
個別アセットの進捗は [`ASSET_MASTER_LIST.md`](ASSET_MASTER_LIST.md) で管理する。

---

## Phase 0：アートディレクション ✅完了

- 世界観 / カラーパレット / ライティング / 世界設定
- 成果物は `00_Direction/` を参照（ArtDirection.md / ColorPalette.md / WorldSetting.md）
- ベース: TSUKIKAGE「月影の里」— 夜景・月光・藍色環境光・霧・アイソメ風カメラ

## Phase 1：UI・背景（ゲーム起動まで）

**3D不要。** 画像生成のみで完結する。

デザイン方針（2026-07-23 ぐり指示）: **UIは「忍者の里」のイメージを強く出す**。
手裏剣・巻物・忍者シルエット・幟・忍具などのモチーフをボタン・アイコン・背景に効かせる。
明るさは「暗いが見やすい」より明るめに寄せる。

作成順:
1. Tap Screen背景（1-01）
2. Game Logo（1-02）
3. Loading背景（1-03）
4. UIパーツ：ボタン素材・アイコン（1-04, 1-05）

格納先: `01_UI/`

## Phase 2：ホーム「月影里（忍者の里）」

**拠点は城下町ではなく忍者の里「月影里」**（2026-07-24 ぐり決定）。
城はあくまで遠景ランドマーク — 見えるだけで、プレイヤーが直接アクセスする場所ではない。
プレイヤーが歩き回るのは里（石畳・鳥居・鍛冶屋・忍術研究所などの生活圏）。

サブフェーズは依存順。

| サブ | 内容 | 格納先 |
|---|---|---|
| 2-1 地形 | 石畳・地面・段差・階段 | `02_HomeTown/Terrain/` |
| 2-2 建築 | 城★5・出撃鳥居★5 → 鍛冶屋★4・忍術研究所★4 → 武器屋★3 | `02_HomeTown/Buildings/` |
| 2-3 自然 | 桜・松・竹・岩・草・池 | `02_HomeTown/Nature/` |
| 2-4 小物 | 提灯・木箱・樽・巻物・ベンチ・看板・柵・木桶（**超重要**） | `02_HomeTown/Props/` |

## Phase 3：プレイヤー

**Jinのみ。** モデル＋アニメーション4種（Idle / Walk / Run / Interact）。
格納先: `03_Characters/Jin/`

## Phase 4：NPC（後回し）

Lili / Narukami / Shiranui / Yama など。
格納先: `03_Characters/NPC/`

## Phase 5：敵

雑魚1体 → ボス1体。
格納先: `03_Characters/Enemy/`

## Phase 6：バトル

バトル地面・オブジェクト・エフェクト・スキル。
格納先: `04_Battle/`

---

## 画像生成順（重要）

キャラを先に作らない。**背景を固めてからキャラを置く**ことで、スケール感・色味のズレを防ぐ。

```
① 世界観ボード
② TapScreen
③ 城
④ 城下町全体
⑤ 地面
⑥ 建物
⑦ 小物
⑧ Jin
⑨ NPC
⑩ 敵
```

## 画像生成ツール（2026-07-23 確定）

**正式ツールは OpenAI gpt-image-1（`tools/gen_image.py`、Claude Codeが直接実行）。**
APIキーはUser環境変数 `OPENAI_API_KEY`。ChatGPT手動生成は予備ルート。

```
Claude Code が gen_image.py でシーン説明から生成
  （CryptoNinjaアートディレクションガイドv0.1のSTYLE_BASE焼き込み済み → トンマナ自動統一）
  → アセット番号付きファイル名で所定フォルダに保存（例: 2-05_torii_front.png）
  → チャットでプレビュー提示 → ぐりの目視OK → 次工程（Blender）へ
```

- 3D化用リファレンス画像の鉄則: **セル塗り・フラット色面・白背景・均一光**
  （3Dモデルの画風は入力画像に引っ張られる。shinobi-rotation `prompts/uka_character_sheet.md` 準拠）
- 建物・小物は front / side / back の3枚渡しが精度が出る（seed・スタイル固定でview違い）
- UI・背景系（TapScreen等）は白背景不要。`00_Direction/` の世界観ボードをトーンの正とする

## アセット1個あたりの制作フロー（2026-07-24 分担確定版）

```
① 画像生成（Claude・gen_image.py/gpt-image-1 → 三面図をアセット番号フォルダに保存）
② 3Dモデリング（ぐり・Tripo AI ※ローポリ指定 → GLBを同じ番号フォルダに納品）
③ リギング（Claude・Blender MCP ※動くキャラのみ。建物・小物はスキップ）
④ Unity配置（Claude・Unity CLI/MCP・glTFast）→ 調整 → 完了
```

- ぐりの納品は「GLBをフォルダに入れて『2-05できた』と言うだけ」でOK
- 石畳・地面などのタイル系は例外: トップダウン画像→テクスチャ貼りプレーン方式（Blender直行・Tripo不要）

各工程が終わるたびに ASSET_MASTER_LIST.md のステータスを更新する。

## Claude Codeへの指示方法

アセット番号で指示できる:
- 「**2-05を作成して**」→ 出撃鳥居を画像→Blender→Unityまで進める
- 「**2-09をUnityに配置して**」→ 提灯をUnityへインポート・配置する
- 「**次は何？**」→ ASSET_MASTER_LISTの依存順で次の未着手を提案する
