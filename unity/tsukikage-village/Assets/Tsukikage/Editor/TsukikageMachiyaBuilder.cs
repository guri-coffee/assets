using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tsukikage.EditorTools
{
    /// <summary>
    /// 町屋キット第1弾: 町屋1棟（Machiya_A）をプロシージャル生成する。
    /// 壁・屋根・格子窓・縁側をモジュール分割し、プレハブとして保存する。
    /// Tools > Tsukikage > 4 で生成、5 でスクリーンショット保存（レビュー用）。
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
        static readonly Color Plaster = FromHex("#d8cfc0");
        static readonly Color Roof = FromHex("#2f3542");
        static readonly Color WarmLight = FromHex("#ffb347");

        // ---------------------------------------------------------------
        [MenuItem("Tools/Tsukikage/4. Build Machiya A (Prefab)")]
        public static void BuildMachiyaA()
        {
            EnsureDirs();

            var wood = GetOrCreateMat("Machiya_Wood", Wood);
            var woodDark = GetOrCreateMat("Machiya_WoodDark", WoodDark);
            var plaster = GetOrCreateMat("Machiya_Plaster", Plaster);
            var roof = GetOrCreateMat("Machiya_Roof", Roof);
            var window = GetOrCreateEmissiveMat("Machiya_Window", WarmLight, 2.4f);

            var house = new GameObject("Machiya_A");

            // --- 土台（basewall: 石場建て風の低い基礎） ---
            Box(house, woodDark, "Base", new Vector3(0f, 0.1f, 0f), new Vector3(4.2f, 0.2f, 3.2f));

            // --- 1階 漆喰壁 ---
            Box(house, plaster, "Wall_GF", new Vector3(0f, 1.3f, 0f), new Vector3(4.0f, 2.2f, 3.0f));

            // --- 1階 柱（四隅 + 正面中間） ---
            float[] xs = { -1.95f, 1.95f };
            float[] zs = { -1.45f, 1.45f };
            foreach (var x in xs)
                foreach (var z in zs)
                    Box(house, wood, "Pillar", new Vector3(x, 1.3f, z), new Vector3(0.18f, 2.2f, 0.18f));
            Box(house, wood, "Pillar_Mid", new Vector3(0f, 1.3f, -1.48f), new Vector3(0.15f, 2.2f, 0.12f));

            // --- 正面の格子窓（虫籠窓風・発光） ---
            Lattice(house, wood, window, new Vector3(-1.0f, 1.5f, -1.53f), 1.2f, 0.9f);
            Lattice(house, wood, window, new Vector3(1.0f, 1.5f, -1.53f), 1.2f, 0.9f);

            // --- 入口（引き戸 + 暖簾の代わりの庇） ---
            Box(house, woodDark, "Door", new Vector3(0f, 0.95f, -1.52f), new Vector3(0.9f, 1.5f, 0.06f));
            Box(house, roof, "Door_Hisashi", new Vector3(0f, 1.85f, -1.7f), new Vector3(1.3f, 0.08f, 0.5f));

            // --- 縁側（正面右） ---
            Box(house, wood, "Engawa", new Vector3(0f, 0.45f, -1.75f), new Vector3(3.6f, 0.1f, 0.5f));
            Box(house, woodDark, "Engawa_Leg1", new Vector3(-1.6f, 0.2f, -1.75f), new Vector3(0.12f, 0.4f, 0.12f));
            Box(house, woodDark, "Engawa_Leg2", new Vector3(1.6f, 0.2f, -1.75f), new Vector3(0.12f, 0.4f, 0.12f));

            // --- 2階（少し小さく・町屋らしい低い階高） ---
            Box(house, plaster, "Wall_1F", new Vector3(0f, 3.0f, 0.1f), new Vector3(3.6f, 1.2f, 2.8f));
            Lattice(house, wood, window, new Vector3(0f, 3.0f, -1.33f), 2.0f, 0.7f);

            // --- 中間庇（1階と2階の間） ---
            GableRoof(house, roof, "Hisashi_Mid", new Vector3(0f, 2.5f, 0f), 4.6f, 3.8f, 0.35f);

            // --- 大屋根（切妻・軒の出あり） ---
            GableRoof(house, roof, "Roof_Main", new Vector3(0f, 3.7f, 0.1f), 4.4f, 3.6f, 1.1f);

            // --- 屋内の灯り（窓のエミッシブを補強するポイントライト1灯） ---
            var lightGo = new GameObject("Interior_Light");
            lightGo.transform.SetParent(house.transform);
            lightGo.transform.localPosition = new Vector3(0f, 1.6f, -1.2f);
            var li = lightGo.AddComponent<Light>();
            li.type = LightType.Point;
            li.color = WarmLight;
            li.intensity = 1.6f;
            li.range = 5f;

            // --- メッシュをアセット化してプレハブ保存 ---
            SaveMeshes(house);
            var prefabPath = PrefabsDir + "/Machiya_A.prefab";
            PrefabUtility.SaveAsPrefabAssetAndConnect(house, prefabPath, InteractionMode.AutomatedAction);
            AssetDatabase.SaveAssets();

            // ブロックアウトの House_A があれば隣に置いて比較できるようにする
            var blockoutHouse = GameObject.Find("Blockout/House_A");
            if (blockoutHouse != null)
            {
                house.transform.position = blockoutHouse.transform.position - new Vector3(0f, blockoutHouse.transform.localScale.y * 0.5f, 0f);
                blockoutHouse.SetActive(false);
            }

            Selection.activeGameObject = house;
            Debug.Log("[Tsukikage] Machiya_A を生成しました: " + prefabPath);
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

        // 切妻屋根: 軒の出付きの三角プリズムをプロシージャル生成
        static void GableRoof(GameObject parent, Material mat, string name, Vector3 center, float width, float depth, float height)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = center;

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

            var mesh = new Mesh { name = name + "_Mesh" };
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
                    AssetDatabase.CreateAsset(mesh, HousesDir + "/" + rootGo.name + "_" + mesh.name + ".asset");
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
