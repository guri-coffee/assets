using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tsukikage.EditorTools
{
    /// <summary>
    /// 城キット: 天守（石垣土台+階層+反り屋根+鯱）・門・櫓を生成する。
    /// Tools > Tsukikage > Castle > 1〜3 の順に実行する。
    /// </summary>
    public static class TsukikageCastleBuilder
    {
        const string Root = "Assets/Tsukikage";
        const string CastleDir = Root + "/Models/Castle";
        const string MatDir = Root + "/Materials/Castle";

        static readonly Color Plaster = FromHex("#dcd6c9");
        static readonly Color RoofCol = FromHex("#2f3542");
        static readonly Color Stone = FromHex("#7d8296");
        static readonly Color Gold = FromHex("#e8c56a");
        static readonly Color WarmLight = FromHex("#ffb347");

        static bool GuardEditMode()
        {
            if (!Application.isPlaying) return true;
            Debug.LogError("[Tsukikage] 再生モード中は実行できません。Playを停止してから実行してください。");
            return false;
        }

        // ---------------------------------------------------------------
        // 1. 天守
        // ---------------------------------------------------------------
        [MenuItem("Tools/Tsukikage/Castle/1. Build Tenshu")]
        public static void BuildTenshu()
        {
            if (!GuardEditMode()) return;
            EnsureDirs();
            Cleanup("Castle_Tenshu");

            var root = new GameObject("Castle_Tenshu");
            root.transform.position = new Vector3(4f, 6f, 4f); // 天守台(Tier3)上面

            var stone = GetOrCreateMat("Castle_Stone", Stone);
            var plaster = GetOrCreateMat("Castle_Plaster", Plaster);
            var roof = GetOrCreateMat("Castle_Roof", RoofCol);
            var trim = GetOrCreateMat("Castle_Trim", FromHex("#4a3524"));
            var gold = GetOrCreateEmissiveMat("Castle_Gold", Gold, 1.2f);
            var glow = GetOrCreateEmissiveMat("Castle_Window", WarmLight, 2.2f);

            // 石垣土台（テーパー付き）
            TaperBox(root, stone, "Base_Ishigaki", 0f, 0f, 1.3f, 8.0f, 7.0f, 6.6f, 5.6f);

            // 階層1
            Box(root, plaster, "L1_Wall", new Vector3(0f, 2.25f, 0f), new Vector3(5.8f, 1.9f, 4.8f));
            Box(root, trim, "L1_Band", new Vector3(0f, 1.45f, 0f), new Vector3(5.9f, 0.25f, 4.9f));
            GlowWindow(root, trim, glow, new Vector3(0f, 2.3f, -2.44f), 1.4f, 0.55f);
            HipRoof(root, roof, "L1_Roof", new Vector3(0f, 3.2f, 0f), 6.8f, 5.8f, 1.0f, 2.8f, 0.16f);

            // 階層2
            Box(root, plaster, "L2_Wall", new Vector3(0f, 4.7f, 0f), new Vector3(4.6f, 1.6f, 3.7f));
            GlowWindow(root, trim, glow, new Vector3(0f, 4.75f, -1.9f), 1.1f, 0.5f);
            HipRoof(root, roof, "L2_Roof", new Vector3(0f, 5.5f, 0f), 5.4f, 4.4f, 0.9f, 2.2f, 0.14f);

            // 階層3（最上層）
            Box(root, plaster, "L3_Wall", new Vector3(0f, 6.85f, 0f), new Vector3(3.4f, 1.5f, 2.6f));
            GlowWindow(root, trim, glow, new Vector3(0f, 6.9f, -1.34f), 0.9f, 0.45f);
            Box(root, trim, "L3_Rail", new Vector3(0f, 6.2f, 0f), new Vector3(3.7f, 0.15f, 2.9f));
            HipRoof(root, roof, "L3_Roof", new Vector3(0f, 7.6f, 0f), 4.2f, 3.3f, 1.3f, 1.7f, 0.2f);

            // 鯱（金の飾り・棟の両端）
            Shachi(root, gold, new Vector3(-0.85f, 8.95f, 0f), 12f);
            Shachi(root, gold, new Vector3(0.85f, 8.95f, 0f), -12f);

            // 最上層の灯り
            var lightGo = new GameObject("Tenshu_Light");
            lightGo.transform.SetParent(root.transform);
            lightGo.transform.localPosition = new Vector3(0f, 6.9f, -1.0f);
            var li = lightGo.AddComponent<Light>();
            li.type = LightType.Point;
            li.color = WarmLight;
            li.intensity = 1.8f;
            li.range = 6f;

            // ブロックアウトの天守を無効化
            var blockout = GameObject.Find("Blockout");
            if (blockout != null)
                foreach (var n in new[] { "Castle_Base", "Castle_Mid", "Castle_Top" })
                {
                    var t = blockout.transform.Find(n);
                    if (t != null) t.gameObject.SetActive(false);
                }

            SaveMeshes(root);
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            Selection.activeGameObject = root;
            Debug.Log("[Tsukikage] 天守を生成しました (castle builder v1)");
        }

        // ---------------------------------------------------------------
        // 2. 門（石段の到着点・南の石垣の出入口）
        // ---------------------------------------------------------------
        [MenuItem("Tools/Tsukikage/Castle/2. Build Gate")]
        public static void BuildGate()
        {
            if (!GuardEditMode()) return;
            EnsureDirs();
            Cleanup("Castle_Gate");

            var root = new GameObject("Castle_Gate");
            root.transform.position = new Vector3(4f, 6f, -1.9f);

            var wood = GetOrCreateMat("Castle_GateWood", FromHex("#6e3a24"));
            var trim = GetOrCreateMat("Castle_Trim", FromHex("#4a3524"));
            var roof = GetOrCreateMat("Castle_Roof", RoofCol);

            Box(root, wood, "Pillar_L", new Vector3(-1.3f, 1.1f, 0f), new Vector3(0.35f, 2.2f, 0.35f));
            Box(root, wood, "Pillar_R", new Vector3(1.3f, 1.1f, 0f), new Vector3(0.35f, 2.2f, 0.35f));
            Box(root, trim, "Beam", new Vector3(0f, 2.25f, 0f), new Vector3(3.4f, 0.3f, 0.5f));
            Box(root, trim, "Door_L", new Vector3(-0.55f, 1.0f, 0.05f), new Vector3(1.0f, 2.0f, 0.08f));
            Box(root, trim, "Door_R", new Vector3(0.55f, 1.0f, 0.05f), new Vector3(1.0f, 2.0f, 0.08f));
            HipRoof(root, roof, "Gate_Roof", new Vector3(0f, 2.5f, 0f), 4.0f, 1.8f, 0.65f, 2.4f, 0.12f);

            SaveMeshes(root);
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Tsukikage] 門を生成しました");
        }

        // ---------------------------------------------------------------
        // 3. 櫓（天守台の北東隅）
        // ---------------------------------------------------------------
        [MenuItem("Tools/Tsukikage/Castle/3. Build Yagura")]
        public static void BuildYagura()
        {
            if (!GuardEditMode()) return;
            EnsureDirs();
            Cleanup("Castle_Yagura");

            var root = new GameObject("Castle_Yagura");
            root.transform.position = new Vector3(9.4f, 6f, 8.4f);

            var stone = GetOrCreateMat("Castle_Stone", Stone);
            var plaster = GetOrCreateMat("Castle_Plaster", Plaster);
            var roof = GetOrCreateMat("Castle_Roof", RoofCol);
            var trim = GetOrCreateMat("Castle_Trim", FromHex("#4a3524"));
            var glow = GetOrCreateEmissiveMat("Castle_Window", WarmLight, 2.2f);

            TaperBox(root, stone, "Yagura_Base", 0f, 0f, 0.6f, 2.8f, 2.8f, 2.3f, 2.3f);
            Box(root, plaster, "Yagura_Wall", new Vector3(0f, 1.4f, 0f), new Vector3(2.0f, 1.6f, 2.0f));
            GlowWindow(root, trim, glow, new Vector3(0f, 1.45f, -1.04f), 0.7f, 0.4f);
            HipRoof(root, roof, "Yagura_Roof", new Vector3(0f, 2.25f, 0f), 2.8f, 2.8f, 0.75f, 0.7f, 0.12f);

            SaveMeshes(root);
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Tsukikage] 櫓を生成しました");
        }

        // ---------------------------------------------------------------
        // 反り付き寄棟屋根: 8点リング×3段のロフト。軒中央を下げて隅を持ち上げる
        static void HipRoof(GameObject parent, Material mat, string name, Vector3 center,
            float width, float depth, float height, float ridgeLen, float sori)
        {
            Vector3[] Ring(float hw, float hd, float y, float cornerLift)
            {
                return new[]
                {
                    new Vector3(-hw, y + cornerLift, -hd), new Vector3(0f, y, -hd), new Vector3(hw, y + cornerLift, -hd),
                    new Vector3(hw, y, 0f), new Vector3(hw, y + cornerLift, hd), new Vector3(0f, y, hd),
                    new Vector3(-hw, y + cornerLift, hd), new Vector3(-hw, y, 0f),
                };
            }
            float rx = ridgeLen * 0.5f;
            var r0 = Ring(width * 0.5f, depth * 0.5f, 0f, sori);                                      // 軒
            var r1 = Ring(Mathf.Lerp(width * 0.5f, rx, 0.55f), depth * 0.5f * 0.45f, height * 0.38f, 0f); // 中腹（反り）
            var r2 = Ring(rx, 0f, height, 0f);                                                        // 棟（奥行き0）

            var rings = new[] { r0, r1, r2 };
            var v = new List<Vector3>();
            var tris = new List<int>();
            void Tri(Vector3 a, Vector3 b, Vector3 c)
            {
                if ((b - a).sqrMagnitude < 1e-8f || (c - a).sqrMagnitude < 1e-8f || (c - b).sqrMagnitude < 1e-8f) return;
                int i = v.Count;
                v.Add(a); v.Add(b); v.Add(c);
                tris.Add(i); tris.Add(i + 1); tris.Add(i + 2);
            }
            // 崖で実証済みの巻き順: 上リングu・下リングlに対し (u_i, u_j, l_j), (u_i, l_j, l_i)
            for (int ring = 0; ring < 2; ring++)
            {
                var lower = rings[ring];
                var upper = rings[ring + 1];
                for (int i = 0; i < 8; i++)
                {
                    int j = (i + 1) % 8;
                    Tri(upper[i], upper[j], lower[j]);
                    Tri(upper[i], lower[j], lower[i]);
                }
            }
            var mesh = new Mesh { name = parent.name + "_" + name + "_Mesh" };
            mesh.SetVertices(v);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = center;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
        }

        // テーパー付きの台（石垣土台用）。上面キャップ付き
        static void TaperBox(GameObject parent, Material mat, string name,
            float cx, float cz, float h, float wBot, float dBot, float wTop, float dTop)
        {
            Vector3[] Ring(float hw, float hd, float y)
            {
                return new[]
                {
                    new Vector3(-hw, y, -hd), new Vector3(hw, y, -hd),
                    new Vector3(hw, y, hd), new Vector3(-hw, y, hd),
                };
            }
            var bot = Ring(wBot * 0.5f, dBot * 0.5f, 0f);
            var top = Ring(wTop * 0.5f, dTop * 0.5f, h);

            var v = new List<Vector3>();
            var tris = new List<int>();
            void Tri(Vector3 a, Vector3 b, Vector3 c)
            {
                int i = v.Count;
                v.Add(a); v.Add(b); v.Add(c);
                tris.Add(i); tris.Add(i + 1); tris.Add(i + 2);
            }
            for (int i = 0; i < 4; i++)
            {
                int j = (i + 1) % 4;
                Tri(top[i], top[j], bot[j]);
                Tri(top[i], bot[j], bot[i]);
            }
            // 上面キャップ（実証済みの巻き順: center, j, i）
            var c = new Vector3(0f, h, 0f);
            for (int i = 0; i < 4; i++)
            {
                int j = (i + 1) % 4;
                Tri(c, top[j], top[i]);
            }
            var mesh = new Mesh { name = parent.name + "_" + name + "_Mesh" };
            mesh.SetVertices(v);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = new Vector3(cx, 0f, cz);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
        }

        // 鯱: 金の飾り（少し傾けた縦長ボックス+尾）
        static void Shachi(GameObject parent, Material gold, Vector3 pos, float tilt)
        {
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Shachi";
            body.transform.SetParent(parent.transform);
            body.transform.localPosition = pos;
            body.transform.localScale = new Vector3(0.22f, 0.55f, 0.16f);
            body.transform.localRotation = Quaternion.Euler(0f, 0f, tilt);
            body.GetComponent<Renderer>().sharedMaterial = gold;
            Object.DestroyImmediate(body.GetComponent<Collider>());

            var tail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tail.name = "Shachi_Tail";
            tail.transform.SetParent(parent.transform);
            tail.transform.localPosition = pos + new Vector3(tilt > 0 ? -0.12f : 0.12f, 0.28f, 0f);
            tail.transform.localScale = new Vector3(0.18f, 0.18f, 0.1f);
            tail.transform.localRotation = Quaternion.Euler(0f, 0f, tilt * 2.5f);
            tail.GetComponent<Renderer>().sharedMaterial = gold;
            Object.DestroyImmediate(tail.GetComponent<Collider>());
        }

        // 発光窓（横長の連子窓風）
        static void GlowWindow(GameObject parent, Material frame, Material glow, Vector3 pos, float w, float h)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name = "Window_Glow";
            g.transform.SetParent(parent.transform);
            g.transform.localPosition = pos;
            g.transform.localScale = new Vector3(w, h, 0.05f);
            g.GetComponent<Renderer>().sharedMaterial = glow;
            Object.DestroyImmediate(g.GetComponent<Collider>());

            int bars = Mathf.Max(3, Mathf.RoundToInt(w / 0.16f));
            for (int i = 0; i <= bars; i++)
            {
                float x = -w * 0.5f + w * i / bars;
                var b = GameObject.CreatePrimitive(PrimitiveType.Cube);
                b.name = "Window_Bar";
                b.transform.SetParent(parent.transform);
                b.transform.localPosition = pos + new Vector3(x, 0f, -0.03f);
                b.transform.localScale = new Vector3(0.045f, h, 0.05f);
                b.GetComponent<Renderer>().sharedMaterial = frame;
                Object.DestroyImmediate(b.GetComponent<Collider>());
            }
        }

        static void Box(GameObject parent, Material mat, string name, Vector3 localPos, Vector3 size)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = localPos;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            Object.DestroyImmediate(go.GetComponent<Collider>());
        }

        static void Cleanup(string name)
        {
            var inst = GameObject.Find(name);
            if (inst != null) Object.DestroyImmediate(inst);
            foreach (var guid in AssetDatabase.FindAssets(name, new[] { CastleDir }))
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
                    var path = CastleDir + "/" + mesh.name + ".asset";
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
            foreach (var d in new[] { CastleDir, MatDir })
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
