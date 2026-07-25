using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tsukikage.EditorTools
{
    /// <summary>
    /// 町屋キット: Machiya A〜E の5バリエーションをプロシージャル生成する。
    /// 生成後は対応するブロックアウト町屋（House_A〜E）と自動で差し替わる。
    /// Tools > Tsukikage > Machiya > Build X で1棟ずつ、5 でスクショ保存。
    /// </summary>
    public static class TsukikageMachiyaBuilder
    {
        const string Root = "Assets/Tsukikage";
        const string HousesDir = Root + "/Models/Houses";
        const string PrefabsDir = Root + "/Prefabs";
        const string MatDir = Root + "/Materials/Machiya";
        const string ShotDir = Root + "/Demo/Screenshots";

        // 仕様書のパレット
        static readonly Color Wood = FromHex("#8b4a2f");
        static readonly Color WoodDark = FromHex("#6e3a24");
        static readonly Color Roof = FromHex("#2f3542");
        static readonly Color WarmLight = FromHex("#ffb347");

        struct MachiyaConfig
        {
            public string id;
            public float w, d;        // 1階の間口・奥行
            public float h1;          // 1階の壁高
            public bool twoStory;
            public float h2;          // 2階の壁高
            public bool ridgeAlongX;  // 大屋根の棟の向き（true=X方向）
            public bool engawa;
            public string plasterHex;
            public float roofSteep;   // 屋根の勾配係数（幅×この値=高さ）
            public int gfWindows;     // 1階正面の格子窓の数（1 or 2）
        }

        static MachiyaConfig Config(string id)
        {
            switch (id)
            {
                case "B": return new MachiyaConfig { id = "B", w = 5.2f, d = 3.4f, h1 = 2.6f, twoStory = false, ridgeAlongX = true, engawa = true, plasterHex = "#ded4c2", roofSteep = 0.22f, gfWindows = 2 };
                case "C": return new MachiyaConfig { id = "C", w = 3.0f, d = 2.6f, h1 = 2.4f, twoStory = true, h2 = 1.4f, engawa = false, plasterHex = "#cfc8bd", roofSteep = 0.28f, gfWindows = 1 };
                case "D": return new MachiyaConfig { id = "D", w = 3.4f, d = 3.0f, h1 = 2.0f, twoStory = false, engawa = false, plasterHex = "#d0bfa8", roofSteep = 0.32f, gfWindows = 1 };
                case "E": return new MachiyaConfig { id = "E", w = 4.8f, d = 3.6f, h1 = 2.2f, twoStory = true, h2 = 1.3f, ridgeAlongX = true, engawa = true, plasterHex = "#c9c4bb", roofSteep = 0.24f, gfWindows = 2 };
                default:  return new MachiyaConfig { id = "A", w = 4.0f, d = 3.0f, h1 = 2.2f, twoStory = true, h2 = 1.2f, engawa = true, plasterHex = "#d8cfc0", roofSteep = 0.25f, gfWindows = 2 };
            }
        }

        [MenuItem("Tools/Tsukikage/Machiya/Build A")] public static void BuildA() { Build("A"); }
        [MenuItem("Tools/Tsukikage/Machiya/Build B")] public static void BuildB() { Build("B"); }
        [MenuItem("Tools/Tsukikage/Machiya/Build C")] public static void BuildC() { Build("C"); }
        [MenuItem("Tools/Tsukikage/Machiya/Build D")] public static void BuildD() { Build("D"); }
        [MenuItem("Tools/Tsukikage/Machiya/Build E")] public static void BuildE() { Build("E"); }

        // ---------------------------------------------------------------
        static void Build(string id)
        {
            EnsureDirs();
            Cleanup("Machiya_" + id);

            var cfg = Config(id);
            var wood = GetOrCreateMat("Machiya_Wood", Wood);
            var woodDark = GetOrCreateMat("Machiya_WoodDark", WoodDark);
            var plaster = GetOrCreateMat("Machiya_Plaster_" + cfg.id, FromHex(cfg.plasterHex));
            var roof = GetOrCreateMat("Machiya_Roof", Roof);
            var window = GetOrCreateEmissiveMat("Machiya_Window", WarmLight, 2.4f);

            var house = new GameObject("Machiya_" + cfg.id);
            float w = cfg.w, d = cfg.d, h1 = cfg.h1;
            float gfTop = 0.2f + h1;
            float zf = -(d * 0.5f + 0.03f);

            // 土台
            Box(house, woodDark, "Base", new Vector3(0f, 0.1f, 0f), new Vector3(w + 0.2f, 0.2f, d + 0.2f));

            // 1階 漆喰壁と柱
            Box(house, plaster, "Wall_GF", new Vector3(0f, 0.2f + h1 * 0.5f, 0f), new Vector3(w, h1, d));
            foreach (var sx in new[] { -1f, 1f })
                foreach (var sz in new[] { -1f, 1f })
                    Box(house, wood, "Pillar", new Vector3(sx * (w * 0.5f - 0.05f), 0.2f + h1 * 0.5f, sz * (d * 0.5f - 0.05f)), new Vector3(0.18f, h1, 0.18f));

            // 正面: 引き戸 + 庇 + 格子窓（虫籠窓風・発光）
            Box(house, woodDark, "Door", new Vector3(0f, 0.95f, zf), new Vector3(0.9f, 1.5f, 0.06f));
            Box(house, roof, "Door_Hisashi", new Vector3(0f, 1.85f, -(d * 0.5f + 0.2f)), new Vector3(1.3f, 0.08f, 0.5f));
            float winW = Mathf.Min(w * 0.27f, 1.2f);
            float winY = 0.2f + h1 * 0.59f;
            Lattice(house, wood, window, new Vector3(-w * 0.3f, winY, zf), winW, 0.9f);
            if (cfg.gfWindows >= 2)
                Lattice(house, wood, window, new Vector3(w * 0.3f, winY, zf), winW, 0.9f);

            // 縁側
            if (cfg.engawa)
            {
                Box(house, wood, "Engawa", new Vector3(0f, 0.45f, -(d * 0.5f + 0.25f)), new Vector3(w - 0.4f, 0.1f, 0.5f));
                Box(house, woodDark, "Engawa_Leg1", new Vector3(-(w * 0.5f - 0.4f), 0.2f, -(d * 0.5f + 0.25f)), new Vector3(0.12f, 0.4f, 0.12f));
                Box(house, woodDark, "Engawa_Leg2", new Vector3(w * 0.5f - 0.4f, 0.2f, -(d * 0.5f + 0.25f)), new Vector3(0.12f, 0.4f, 0.12f));
            }

            float roofBaseY, roofW, roofD;
            if (cfg.twoStory)
            {
                // 2階（少し小さく・町屋らしい低い階高）
                float w2 = w - 0.4f, d2 = d - 0.2f, h2 = cfg.h2;
                Box(house, plaster, "Wall_1F", new Vector3(0f, gfTop + h2 * 0.5f, 0.1f), new Vector3(w2, h2, d2));
                float zf2 = 0.1f - d2 * 0.5f - 0.03f;
                Lattice(house, wood, window, new Vector3(0f, gfTop + h2 * 0.5f, zf2), Mathf.Min(w2 * 0.55f, 2.0f), h2 * 0.58f);

                // 中間庇
                GableRoof(house, roof, "Hisashi_Mid", new Vector3(0f, gfTop + 0.1f, 0f), w + 0.6f, d + 0.8f, 0.35f, false);

                roofBaseY = gfTop + h2 + 0.1f;
                roofW = w2 + 0.8f; roofD = d2 + 0.8f;
            }
            else
            {
                roofBaseY = gfTop + 0.1f;
                roofW = w + 0.8f; roofD = d + 0.8f;
            }

            // 大屋根（切妻・軒の出あり）
            GableRoof(house, roof, "Roof_Main", new Vector3(0f, roofBaseY, cfg.twoStory ? 0.1f : 0f), roofW, roofD, roofW * cfg.roofSteep, cfg.ridgeAlongX);

            // 屋内の灯り
            var lightGo = new GameObject("Interior_Light");
            lightGo.transform.SetParent(house.transform);
            lightGo.transform.localPosition = new Vector3(0f, 0.2f + h1 * 0.64f, -(d * 0.5f - 0.3f));
            var li = lightGo.AddComponent<Light>();
            li.type = LightType.Point;
            li.color = WarmLight;
            li.intensity = 1.6f;
            li.range = 5f;

            // メッシュ・プレハブ保存
            SaveMeshes(house);
            var prefabPath = PrefabsDir + "/Machiya_" + cfg.id + ".prefab";
            PrefabUtility.SaveAsPrefabAssetAndConnect(house, prefabPath, InteractionMode.AutomatedAction);
            AssetDatabase.SaveAssets();

            PlaceAtBlockoutHouse(house, "House_" + cfg.id);
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

            Selection.activeGameObject = house;
            Debug.Log("[Tsukikage] Machiya_" + cfg.id + " を生成しました: " + prefabPath);
        }

        // 既存の同名インスタンスとアセットを削除してから作り直す
        static void Cleanup(string name)
        {
            var inst = GameObject.Find(name);
            if (inst != null) Object.DestroyImmediate(inst);
            foreach (var guid in AssetDatabase.FindAssets(name, new[] { PrefabsDir, HousesDir }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path).StartsWith(name))
                    AssetDatabase.DeleteAsset(path);
            }
        }

        static void PlaceAtBlockoutHouse(GameObject house, string houseName)
        {
            var blockRoot = GameObject.Find("Blockout");
            if (blockRoot == null) return;
            var t = blockRoot.transform.Find(houseName);
            if (t == null) return;
            float groundY = t.position.y - t.localScale.y * 0.5f;
            house.transform.position = new Vector3(t.position.x, groundY, t.position.z);
            t.gameObject.SetActive(false);
        }

        // ---------------------------------------------------------------
        [MenuItem("Tools/Tsukikage/5. Capture Game View Screenshot")]
        public static void CaptureScreenshot()
        {
            EnsureDirs();
            var cam = Camera.main;
            if (cam == null) { Debug.LogError("[Tsukikage] Main Camera が見つかりません"); return; }

            const int w = 1280, h = 720;
            var rt = new RenderTexture(w, h, 24);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            cam.targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);

            var path = ShotDir + "/shot_" + System.DateTime.Now.ToString("HHmmss") + ".png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.Refresh();
            Debug.Log("[Tsukikage] スクリーンショット保存: " + path);
        }

        // ---------------------------------------------------------------
        // 格子窓: 発光パネル + 縦格子
        static void Lattice(GameObject parent, Material frame, Material glow, Vector3 center, float width, float height)
        {
            Box(parent, glow, "Window_Glow", center, new Vector3(width, height, 0.04f));
            int bars = Mathf.Max(3, Mathf.RoundToInt(width / 0.18f));
            for (int i = 0; i <= bars; i++)
            {
                float x = -width * 0.5f + width * i / bars;
                Box(parent, frame, "Window_Bar",
                    center + new Vector3(x, 0f, -0.03f), new Vector3(0.05f, height, 0.05f));
            }
            Box(parent, frame, "Window_FrameT", center + new Vector3(0f, height * 0.5f, -0.02f), new Vector3(width + 0.1f, 0.08f, 0.08f));
            Box(parent, frame, "Window_FrameB", center + new Vector3(0f, -height * 0.5f, -0.02f), new Vector3(width + 0.1f, 0.08f, 0.08f));
        }

        // 切妻屋根: 軒の出付きの三角プリズム（フラットシェーディング）
        static void GableRoof(GameObject parent, Material mat, string name, Vector3 center, float width, float depth, float height, bool ridgeAlongX)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = center;
            if (ridgeAlongX)
            {
                // 棟をX方向に回す（幅と奥行を入れ替えて90度回転）
                float tmp = width; width = depth; depth = tmp;
                go.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            }

            float hw = width * 0.5f, hd = depth * 0.5f;
            var fl = new Vector3(-hw, 0, -hd); var fr = new Vector3(hw, 0, -hd); var ft = new Vector3(0, height, -hd);
            var bl = new Vector3(-hw, 0, hd); var br = new Vector3(hw, 0, hd); var bt = new Vector3(0, height, hd);

            // フラットシェーディングにするため面ごとに頂点を複製する
            var v = new List<Vector3>();
            var tris = new List<int>();
            void Tri(Vector3 a, Vector3 b, Vector3 c)
            {
                int i = v.Count;
                v.Add(a); v.Add(b); v.Add(c);
                tris.Add(i); tris.Add(i + 1); tris.Add(i + 2);
            }
            Tri(fl, ft, fr);            // 前面
            Tri(bl, br, bt);            // 背面
            Tri(fl, bl, ft); Tri(bl, bt, ft);   // 左屋根面
            Tri(fr, ft, bt); Tri(fr, bt, br);   // 右屋根面
            Tri(fl, fr, br); Tri(fl, br, bl);   // 底面

            var mesh = new Mesh { name = parent.name + "_" + name + "_Mesh" };
            mesh.SetVertices(v);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
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

        static void SaveMeshes(GameObject rootGo)
        {
            foreach (var mf in rootGo.GetComponentsInChildren<MeshFilter>())
            {
                var mesh = mf.sharedMesh;
                if (mesh != null && !AssetDatabase.Contains(mesh))
                {
                    AssetDatabase.CreateAsset(mesh, HousesDir + "/" + mesh.name + ".asset");
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
            foreach (var d in new[] { HousesDir, PrefabsDir, MatDir, ShotDir })
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
