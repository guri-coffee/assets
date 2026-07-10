# Unity Asset Store 環境アセット販売計画

参考アセット: [FANTASTIC - Highlands Castle](https://assetstore.unity.com/packages/3d/environments/fantastic-highlands-castle-381282)（Tidal Flask Studios / $59.99）のような**スタイライズド・ファンタジー環境アセット**を制作し、Unity Asset Store で販売するための実行計画。

作成日: 2026-07-10（提出前に必ず最新の [Submission Guidelines](https://assetstore.unity.com/publishing/submission-guidelines) を確認すること）

---

## 1. 有効なカテゴリ

Asset Store のカテゴリは出品時に Publisher Portal で選択する。今回のようなアセットに関係するのは **3D > Environments** 系統。

### 3D > Environments のサブカテゴリ（2026年7月時点で確認済み）

| カテゴリ | URL | 今回の適合度 |
|---|---|---|
| **Fantasy** | `/3d/environments/fantasy` | ◎ 第一候補。城・ハイランド系の競合が集中するカテゴリ |
| Historic | `/3d/environments/historic` | ○ リアル調の中世城ならこちら |
| Landscapes | `/3d/environments/landscapes` | ○ 地形・自然が主体の場合 |
| Dungeons | `/3d/environments/dungeons` | △ 城内部・地下が主体なら |
| Sci-Fi | `/3d/environments/sci-fi` | × |
| Industrial | `/3d/environments/industrial` | × |
| Urban | `/3d/environments/urban` | × |
| Roadways | `/3d/environments/roadways` | × |

- 参考アセット自体は `3d/environments` 直下（サブカテゴリなし）に配置されているが、類似の競合の多くは **3D > Environments > Fantasy** に出品されている。
- カテゴリ選択のルール: 1パッケージにつき主カテゴリは1つ。審査時に内容とカテゴリの不一致は差し戻し要因になるため、パッケージの「主体」が何か（建物群か、地形か、内装か）で決める。
- 補足: プロップ単体を切り出して売る場合は 3D > Props、植生は 3D > Vegetation など別系統になる。

**推奨: 3D > Environments > Fantasy**（スタイライズド調の場合）。リアル調に振るなら Historic。

## 2. 市場状況（ファンタジー城・環境アセット）

確認できた競合と価格帯:

| アセット | パブリッシャー | 価格帯 |
|---|---|---|
| FANTASTIC - Highlands Castle（参考） | Tidal Flask Studios | $59.99 |
| Modular Medieval Castle - Stylized Fantasy | JustCreate | 約€28 |
| KINGDOM: Stylized Modular Castle Environment | Polyart Studio | 定価約€137（セール時€69） |
| Fantasy Castle Environment (Soulslike) | Leartes Studios | 中〜高価格帯 |
| Medieval Kingdom | Hivemind | 約€129 |
| The Medieval Ultimate Bundle | Hivemind | 約€368（バンドル） |

読み取れる傾向:

- **価格帯はおおむね $25〜$140**。単品の環境パックは $30〜$70 が中心、大型・メガパックが $100 超。
- 売れ筋の共通点: **モジュラー設計**（組み替えて独自の城が作れる）、**デモシーン同梱**、**プロップ点数の多さ**（KINGDOM は約1,600点を訴求）、統一されたアートスタイル。
- 競合はレッドオーシャン気味。差別化軸は「アートスタイルの独自性」「URP/HDRP/Built-in 全対応」「セットアップの容易さ」「拡張パックによるシリーズ展開」。

## 3. 販売開始までの手順（公式ワークフロー・検証済み）

### Phase 0: 登録（1日）
1. Unity ID を作成し、[Publisher Portal](https://publisher.unity.com) でパブリッシャーアカウントを登録。登録・出品は**無料**。
2. [Asset Store Provider Agreement](https://unity.com/legal/provider)（法的契約）と [Submission Guidelines](https://assetstore.unity.com/publishing/submission-guidelines)（審査基準）を読む。
3. 支払い情報・税務情報を設定。

### Phase 1: 制作（規模により4〜12週間）
1. スコープ定義: 城本体（モジュラーパーツ）、地形・岩・植生、プロップ、デモシーン。
2. **Unity 2021.3 LTS 以降**（新規提出はガイドライン上 2022.3+ が要求される場合あり。提出前に最新要件を確認）でプロジェクトを構築。
3. レンダーパイプライン対応を決める（最低でも URP。Built-in / HDRP 対応は訴求点になる）。
4. デモシーンを必ず同梱。主要顧客層は「中級スキルのソロインディー開発者・ホビイスト」（Unity 公式）なので、ドラッグ&ドロップで動く状態にする。

### Phase 2: パッケージング（1週間)
1. Unity 公式の [Asset Store Publishing Tools](https://github.com/Unity-Technologies/com.unity.asset-store-tools)（最新 v12.0.0、最低対応エディタ 2021.3）を導入。
   - Unity 2020.1 以降: ストアで「Add to my Assets」→ Package Manager の My Assets からインポート。
2. ツールの**バリデータ**で事前検証（命名規則、フォルダ構成、不要ファイル、エラーの有無）。
3. `.unitypackage` としてアップロード。

### Phase 3: 出品・審査（審査は数日〜数週間）
1. Publisher Portal でパッケージドラフト作成: タイトル、説明文、カテゴリ（3D > Environments > Fantasy）、タグ、価格。
2. 画像アセットをアップロード: キービジュアル、スクリーンショット、（推奨）デモ動画。競合はビジュアル品質が高いため、ここは手を抜かない。
3. 審査に提出 → Asset Store キュレーションチームがガイドラインに照らして審査。差し戻しがあれば修正して再提出。

## 4. 価格設定・収益

- **収益分配: 販売価格の 70% がパブリッシャー、30% が Unity**（[Provider Agreement](https://unity.com/legal/provider)）。返金・銀行手数料・税金は控除される。
- 最低販売価格は **$4.99**（無料公開も可）。
- Unity 公式の価格ガイド「[Finding the Right Price](https://assetstore.unity.com/publishing/finding-the-right-price)」の要点（検証済み）:
  - **安売りしない**。パブリッシャーは過小価格をつけがちで、収益機会の損失と知覚価値の低下を招くと公式が明言。
  - **セール余地を織り込んだ定価**にする。定価が高いほどセール時に魅力的な割引率を出せ、購入数と収益を伸ばせる。Asset Store の定期セールへの参加を前提に設計する。
  - 最適価格の探索には **Van Westendorp 価格感度モデル**（4質問方式）が公式ガイドで紹介されている。
- **本計画の推奨価格**: 競合分布と参考アセット（$59.99）を踏まえ、同等ボリュームなら **定価 $49.99〜$69.99**、セール時 30〜50% オフ。小規模な最初のパックなら $29.99〜$39.99 から。

## 5. 長期戦略（公式推奨・検証済み）

1. **継続的アップデートとサポート** — 経常収益と評価の維持につながる。
2. **エコシステム構築** — 同一アートスタイルでシリーズ展開し、顧客ロイヤルティを活用。
3. **補完プロダクト** — 城 → 村 → ダンジョン → キャラクター、のような拡張パック展開（Tidal Flask の「FANTASTIC」シリーズや Hivemind のバンドル戦略がまさにこの形）。

## 6. マイルストーン案

| 時期 | マイルストーン |
|---|---|
| Week 1 | パブリッシャー登録、規約・ガイドライン確認、競合3件の購入・分析 |
| Week 2 | スコープ・アートスタイル決定、モジュラー設計 |
| Week 3–8 | モデリング / テクスチャ / プレハブ化 / デモシーン制作 |
| Week 9 | URP 対応確認、バリデータ検証、ドキュメント作成 |
| Week 10 | ストアページ素材（画像・動画）制作、価格決定、審査提出 |
| 審査通過後 | SNS / Unity フォーラムで告知、初回セール参加、レビュー対応 |

## 7. 留意事項

- 価格に関する助言は Unity 自身（収益分配の当事者）の一次情報であり、「高めが良い」という指導には商業的インセンティブが介在しうる。競合実売価格と突き合わせて判断する。
- Asset Store Tools のバージョンや提出可能な最低 Unity バージョンは変化するため、**提出直前に Submission Guidelines を再確認**する。
- 他ストア（Fab/Unreal 等）との併売は Provider Agreement 上、非独占なので可能。ただし各ストアの規約を個別確認。

### 主な出典

- [Publish and sell assets（公式）](https://assetstore.unity.com/publishing/publish-and-sell-assets)
- [Submission Guidelines（公式）](https://assetstore.unity.com/publishing/submission-guidelines)
- [Asset Store Provider Agreement（公式）](https://unity.com/legal/provider)
- [Finding the Right Price（公式価格ガイド）](https://assetstore.unity.com/publishing/finding-the-right-price)
- [Asset Store Publishing Tools（公式 GitHub）](https://github.com/Unity-Technologies/com.unity.asset-store-tools)
- [Asset Store workflow（公式マニュアル）](https://docs.unity3d.com/Manual/asset-store-workflow.html)
- [3D Environments カテゴリ](https://assetstore.unity.com/3d/environments) / [Fantasy サブカテゴリ](https://assetstore.unity.com/3d/environments/fantasy)
