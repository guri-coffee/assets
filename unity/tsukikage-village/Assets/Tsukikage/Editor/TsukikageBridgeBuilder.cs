using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tsukikage.EditorTools
{
    /// <summary>
    /// 橋・鳥居キット: 赤い太鼓橋（アーチ+欄干）と本番鳥居（笠木反り+貫）。
    /// Tools > Tsukikage > Bridge > 1〜2 の順に実行する。
    /// </summary>
    public static class TsukikageBridgeBuilder
    {
        const string Root = "Assets/Tsukikage";
        const string BridgeDir = Root + "/Models/Bridges";
        const string MatDir = Root + "/Materials/Bridge";

        static readonly Color Vermilion = FromHex("#c2402a"); // 朱
        static readonly Color DeckWood = FromHex("#7a4630");
        static readonly Color KasagiCol = FromHex("#2f3542");
        static readonly Color Stone = FromHex("#6b7089");
        static readonly Color Gold = FromHex("#e8c56a");

        static bool GuardEditMode()
        {
            if (!Application.isPlaying) return true;
            Debug.LogError("[Tsukikage] 再生モード中は実行できません。Playを停止してから実行してください。");
            return false;
        }

        // ---------------------------------------------------------------
        // 1. 太鼓橋（小島2つ + アーチ橋 + 岸への石段）
        // ---------------------------------------------------------------
        [MenuItem("Tools/Tsukikage/Bridge/1. Build Taiko Bridge")]
        public static void BuildTaikoBridge()
        {
            if (!GuardEditMode()) return;
            EnsureDirs();
            Cleanup("Bridge_Taiko");

            var root = new GameObject("Bridge_Taiko");
            var red = GetOrCreateMat("Bridge_Red", Vermilion);
            var deckMat = GetOrCreateMat("Bridge_Deck", DeckWood);
            var stone = GetOrCreateMat("Bridge_Stone", Stone);
            var gold = GetOrCreateEmissiveMat("Bridge_Gold", Gold, 1.0f);

            // 小島2つ（岩の足場）
            RockIslet(root, stone, "Islet_A", new Vector3(-11f, 0f, -19.5f), 4.2f, 3.4f, 0.9f, 11);
            RockIslet(root, stone, "Islet_B", new Vector3(-11f, 0f, -25.5f), 4.4f, 3.6f, 0.9f, 22);

            // 太鼓橋アーチ（弦5.0m・ライズ0.9m・8分割）
            float deckY = 0.9f, chord = 5.0f, rise = 0.9f, width = 1.6f;
            float R = (chord * chord * 0.25f + rise * rise) / (2f * rise);
            float alpha = Mathf.Asin(chord * 0.5f / R);
            int segs = 8;
            float zCenter = -22.5f;
            var joints = new Vector3[segs + 1];
            for (int i = 0; i <= segs; i++)
            {
                float phi = Mathf.Lerp(-alpha, alpha, (float)i / segs);
                joints[i] = new Vector3(-11f, deckY + R * Mathf.Cos(phi) - (R - rise), zCenter + R * Mathf.Sin(phi));
            }
            for (int i = 0; i < segs; i++)
            {
                var a = joints[i]; var b = joints[i + 1];
                var mid = (a + b) * 0.5f;
                float len = Vector3.Distance(a, b);
                float slope = Mathf.Atan2(b.y - a.y, b.z - a.z) * Mathf.Rad2Deg;

                var seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                seg.name = "Deck_Seg";
                seg.transform.SetParent(root.transform);
                seg.transform.position = mid;
                seg.transform.rotation = Quaternion.Euler(-slope, 0f, 0f);
                seg.transform.localScale = new Vector3(width, 0.12f, len + 0.04f);
                seg.GetComponent<Renderer>().sharedMaterial = deckMat;
                Object.DestroyImmediate(seg.GetComponent<Collider>());

                // 欄干の上桟（デッキと平行・両側）
                foreach (var sx in new[] { -1f, 1f })
                {
                    var rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    rail.name = "Rail";
                    rail.transform.SetParent(root.transform);
                    rail.transform.position = mid + new Vector3(sx * (width * 0.5f - 0.05f), 0.55f, 0f);
                    rail.transform.rotation = Quaternion.Euler(-slope, 0f, 0f);
                    rail.transform.localScale = new Vector3(0.08f, 0.08f, len + 0.06f);
                    rail.GetComponent<Renderer>().sharedMaterial = red;
                    Object.DestroyImmediate(rail.GetComponent<Collider>());
                }
            }
            // 欄干の柱（1つおきのジョイント + 両端は親柱）
            for (int i = 0; i <= segs; i += 2)
            {
                bool isEnd = (i == 0 || i == segs);
                foreach (var sx in new[] { -1f, 1f })
                {
                    float h = isEnd ? 0.7f : 0.55f;
                    var post = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    post.name = isEnd ? "Post_Oya" : "Post";
                    post.transform.SetParent(root.transform);
                    post.transform.position = joints[i] + new Vector3(sx * (width * 0.5f - 0.05f), h * 0.5f, 0f);
                    post.transform.localScale = new Vector3(isEnd ? 0.14f : 0.09f, h, isEnd ? 0.14f : 0.09f);
                    post.GetComponent<Renderer>().sharedMaterial = red;
                    Object.DestroyImmediate(post.GetComponent<Collider>());
                    if (isEnd)
                    {
                        var cap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        cap.name = "Post_Cap";
                        cap.transform.SetParent(root.transform);
                        cap.transform.position = joints[i] + new Vector3(sx * (width * 0.5f - 0.05f), h + 0.07f, 0f);
                        cap.transform.localScale = Vector3.one * 0.16f;
                        cap.GetComponent<Renderer>().sharedMaterial = gold;
                        Object.DestroyImmediate(cap.GetComponent<Collider>());
                    }
                }
            }

            // 岸（崖）への石段: 小島Aから台地1上面(y=2)へ
            for (int i = 0; i < 5; i++)
            {
                float y = 0.9f + (i + 1) * 0.22f;
                var step = GameObject.CreatePrimitive(PrimitiveType.Cube);
                step.name = "Shore_Step";
                step.transform.SetParent(root.transform);
                step.transform.position = new Vector3(-11f, y - 0.11f, -18.2f + i * 0.55f);
                step.transform.localScale = new Vector3(1.8f, 0.22f, 0.62f);
                step.GetComponent<Renderer>().sharedMaterial = stone;
                Object.DestroyImmediate(step.GetComponent<Collider>());
            }

            SaveMeshes(root);
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            Selection.activeGameObject = root;
            Debug.Log("[Tsukikage] 太鼓橋を生成しました (bridge builder v1)");
        }

        // ---------------------------------------------------------------
        // 2. 本番鳥居（笠木の反り+島木+貫+額束）
        // ---------------------------------------------------------------
        [MenuItem("Tools/Tsukikage/Bridge/2. Build Torii")]
        public static void BuildTorii()
        {
            if (!GuardEditMode()) return;
            EnsureDirs();
            Cleanup("Torii_Main");

            var root = new GameObject("Torii_Main");
            root.transform.position = new Vector3(-13.3f, 2f, -8.75f);
            root.transform.rotation = Quaternion.Euler(0f, 45f, 0f);

            var red = GetOrCreateMat("Bridge_Red", Vermilion);
            var kasagiMat = GetOrCreateMat("Bridge_Kasagi", KasagiCol);

            float pillarH = 3.1f, spacing = 2.6f;

            // 柱（内転び: わずかに内側へ傾ける）
            foreach (var sx in new[] { -1f, 1f })
            {
                var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.name = "Pillar";
                pillar.transform.SetParent(root.transform);
                pillar.transform.localPosition = new Vector3(sx * spacing * 0.5f, pillarH * 0.5f, 0f);
                pillar.transform.localRotation = Quaternion.Euler(0f, 0f, sx * 2.5f);
                pillar.transform.localScale = new Vector3(0.34f, pillarH * 0.5f, 0.34f);
                pillar.GetComponent<Renderer>().sharedMaterial = red;
                Object.DestroyImmediate(pillar.GetComponent<Collider>());

                // 台石
                var dai = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                dai.name = "Daiseki";
                dai.transform.SetParent(root.transform);
                dai.transform.localPosition = new Vector3(sx * spacing * 0.5f, 0.12f, 0f);
                dai.transform.localScale = new Vector3(0.5f, 0.12f, 0.5f);
                dai.GetComponent<Renderer>().sharedMaterial = kasagiMat;
                Object.DestroyImmediate(dai.GetComponent<Collider>());
            }

            // 貫（柱を貫通して両端が突き出る）
            var nuki = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nuki.name = "Nuki";
            nuki.transform.SetParent(root.transform);
            nuki.transform.localPosition = new Vector3(0f, pillarH * 0.72f, 0f);
            nuki.transform.localScale = new Vector3(spacing + 1.1f, 0.22f, 0.18f);
            nuki.GetComponent<Renderer>().sharedMaterial = red;
            Object.DestroyImmediate(nuki.GetComponent<Collider>());

            // 島木（笠木の下の直材）
            var shimagi = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shimagi.name = "Shimagi";
            shimagi.transform.SetParent(root.transform);
            shimagi.transform.localPosition = new Vector3(0f, pillarH + 0.11f, 0f);
            shimagi.transform.localScale = new Vector3(spacing + 1.3f, 0.24f, 0.26f);
            shimagi.GetComponent<Renderer>().sharedMaterial = red;
            Object.DestroyImmediate(shimagi.GetComponent<Collider>());

            // 笠木（5分割の反り: 端ほど持ち上がる）
            float kasagiY = pillarH + 0.34f;
            float kasagiLen = spacing + 1.7f;
            int kSegs = 5;
            for (int i = 0; i < kSegs; i++)
            {
                float t0 = (float)i / kSegs - 0.5f;
                float t1 = (float)(i + 1) / kSegs - 0.5f;
                float x0 = t0 * kasagiLen, x1 = t1 * kasagiLen;
                float y0 = 1.1f * t0 * t0, y1 = 1.1f * t1 * t1; // 放物線の反り
                var a = new Vector3(x0, kasagiY + y0, 0f);
                var b = new Vector3(x1, kasagiY + y1, 0f);
                var mid = (a + b) * 0.5f;
                float len = Vector3.Distance(a, b);
                float ang = Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg;

                var seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                seg.name = "Kasagi_Seg";
                seg.transform.SetParent(root.transform);
                seg.transform.localPosition = mid;
                seg.transform.localRotation = Quaternion.Euler(0f, 0f, ang);
                seg.transform.localScale = new Vector3(len + 0.05f, 0.2f, 0.3f);
                seg.GetComponent<Renderer>().sharedMaterial = kasagiMat;
                Object.DestroyImmediate(seg.GetComponent<Collider>());
            }

            // 額束（島木と貫の間の中央の束）
            var gaku = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gaku.name = "Gakuzuka";
            gaku.transform.SetParent(root.transform);
            gaku.transform.localPosition = new Vector3(0f, pillarH * 0.86f + 0.08f, 0f);
            gaku.transform.localScale = new Vector3(0.3f, 0.55f, 0.14f);
            gaku.GetComponent<Renderer>().sharedMaterial = red;
            Object.DestroyImmediate(gaku.GetComponent<Collider>());

            // 仮鳥居を無効化
            var blockout = GameObject.Find("Blockout");
            if (blockout != null)
                foreach (var n in new[] { "Torii_PillarL", "Torii_PillarR", "Torii_Beam" })
                {
                    var t = blockout.transform.Find(n);
                    if (t != null) t.gameObject.SetActive(false);
                }

            SaveMeshes(root);
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            Selection.activeGameObject = root;
            Debug.Log("[Tsukikage] 鳥居を生成しました");
        }

        // ---------------------------------------------------------------
        // 岩の小島: 8点リングを2段ロフト+上面キャップ（ジッター付き）
        static void RockIslet(GameObject parent, Material mat, string name, Vector3 center, float w, float d, float topY, int seed)
        {
            var rand = new System.Random(seed);
            float Jit(float a, float b) => a + (float)rand.NextDouble() * (b - a);

            var dirs = new Vector2[8];
            for (int i = 0; i < 8; i++)
            {
                float ang = Mathf.PI * 2f * i / 8f;
                dirs[i] = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
            }
            var top = new Vector3[8];
            var bot = new Vector3[8];
            for (int i = 0; i < 8; i++)
            {
                float rw = w * 0.5f * Jit(0.85f, 1.1f);
                float rd = d * 0.5f * Jit(0.85f, 1.1f);
                top[i] = new Vector3(dirs[i].x * rw, topY, dirs[i].y * rd);
                bot[i] = new Vector3(dirs[i].x * (rw + Jit(0.4f, 0.9f)), -1.0f, dirs[i].y * (rd + Jit(0.4f, 0.9f)));
            }

            var v = new List<Vector3>();
            var tris = new List<int>();
            void Tri(Vector3 a, Vector3 b, Vector3 c)
            {
                int i = v.Count;
                v.Add(a); v.Add(b); v.Add(c);
                tris.Add(i); tris.Add(i + 1); tris.Add(i + 2);
            }
            // 8点リングは反時計回り(上から見て)なので崖と同じ巻き順
            for (int i = 0; i < 8; i++)
            {
                int j = (i + 1) % 8;
                Tri(top[i], top[j], bot[j]);
                Tri(top[i], bot[j], bot[i]);
            }
            var c0 = new Vector3(0f, topY, 0f);
            for (int i = 0; i < 8; i++)
            {
                int j = (i + 1) % 8;
                Tri(c0, top[j], top[i]);
            }
            var mesh = new Mesh { name = parent.name + "_" + name + "_Mesh" };
            mesh.SetVertices(v);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            go.transform.position = center;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
        }

        static void Cleanup(string name)
        {
            var inst = GameObject.Find(name);
            if (inst != null) Object.DestroyImmediate(inst);
            foreach (var guid in AssetDatabase.FindAssets(name, new[] { BridgeDir }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path).StartsWith(name))
                    AssetDatabase.DeleteAsset(path);
            }
        }

        static void SaveMeshes(GameObject rootGo)
        {
            foreach (var mf in rootGo.GetComponentsInChildren<MeshFilter>())
            {
                var mesh = mf.sharedMesh;
                if (mesh != null && !AssetDatabase.Contains(mesh))
                {
                    var path = BridgeDir + "/" + mesh.name + ".asset";
                    if (AssetDatabase.LoadAssetAtPath<Mesh>(path) != null) AssetDatabase.DeleteAsset(path);
                    AssetDatabase.CreateAsset(mesh, path);
                }
            }
            AssetDatabase.SaveAssets();
        }

        static Material GetOrCreateMat(string name, Color color)
        {
            var path = MatDir + "/" + name + ".mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", color);
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        static Material GetOrCreateEmissiveMat(string name, Color color, float intensity)
        {
            var path = MatDir + "/" + name + ".mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", color);
            mat.EnableKeyword("_EMISSION");
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            mat.SetColor("_EmissionColor", color * intensity);
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        static void EnsureDirs()
        {
            foreach (var d in new[] { BridgeDir, MatDir })
                if (!Directory.Exists(d)) Directory.CreateDirectory(d);
            AssetDatabase.Refresh();
        }

        static Color FromHex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }
    }
}
