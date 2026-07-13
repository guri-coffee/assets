using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tsukikage.EditorTools
{
    /// <summary>
    /// 地形・岩キット: 崖付き台地・石垣・石段・水面と滝を生成する。
    /// Tools > Tsukikage > Terrain > 1〜4 の順に実行する。
    /// </summary>
    public static class TsukikageTerrainBuilder
    {
        const string Root = "Assets/Tsukikage";
        const string TerrainDir = Root + "/Models/Terrain";
        const string MatDir = Root + "/Materials/Terrain";

        static readonly Color Rock = FromHex("#5c6180");      // 夜の藍に馴染む岩
        static readonly Color RockDark = FromHex("#474c66");
        static readonly Color StoneWall = FromHex("#8a8fa3"); // 石垣
        static readonly Color WaterCol = FromHex("#2e6f8e");
        static readonly Color FoamCol = FromHex("#9fd8e8");

        // ---------------------------------------------------------------
        // 1. 崖付き台地: ブロックアウトのTier箱をギザギザの崖メッシュに置換
        // ---------------------------------------------------------------
        // 再生モード中に生成すると停止時に全部消えるため必ず拒否する
        static bool GuardEditMode()
        {
            if (!Application.isPlaying) return true;
            Debug.LogError("[Tsukikage] 再生モード中は実行できません。Playを停止してから実行してください。");
            return false;
        }

        [MenuItem("Tools/Tsukikage/Terrain/1. Build Cliff Tiers")]
        public static void BuildCliffTiers()
        {
            if (!GuardEditMode()) return;
            EnsureDirs();
            var old = GameObject.Find("Terrain_Tiers");
            if (old != null) Object.DestroyImmediate(old);
            var root = new GameObject("Terrain_Tiers");

            var rock = GetOrCreateMat("Terrain_Rock", Rock);

            // (中心x, 中心z, 上面y, 底y, 半幅, 半奥行, シード)
            Plateau(root, rock, "Cliff_Tier1", 0f, 0f, 2f, -1.0f, 17f, 15f, 101);
            Plateau(root, rock, "Cliff_Tier2", 2f, 2f, 4f, 1.6f, 12f, 10f, 202);
            Plateau(root, rock, "Cliff_Tier3", 4f, 4f, 6f, 3.6f, 7f, 6f, 303);

            // 元のTier箱を無効化
            var blockout = GameObject.Find("Blockout");
            if (blockout != null)
                foreach (var n in new[] { "Tier_1", "Tier_2", "Tier_3" })
                {
                    var t = blockout.transform.Find(n);
                    if (t != null) t.gameObject.SetActive(false);
                }

            SaveMeshes(root);
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Tsukikage] 崖付き台地3段を生成しました (builder v2)");
        }

        // 上面は元の矩形を確実に覆い、外周は外向きジッターでギザギザにした台地
        static void Plateau(GameObject parent, Material mat, string name,
            float cx, float cz, float topY, float baseY, float hw, float hd, int seed)
        {
            var rand = new System.Random(seed);
            float Jit(float a, float b) => a + (float)rand.NextDouble() * (b - a);

            // 外周点を矩形に沿って約2m間隔でサンプリング
            var pts = new List<Vector2>();
            void Edge(Vector2 from, Vector2 to)
            {
                float len = Vector2.Distance(from, to);
                int n = Mathf.Max(2, Mathf.RoundToInt(len / 2f));
                for (int i = 0; i < n; i++) pts.Add(Vector2.Lerp(from, to, (float)i / n));
            }
            Edge(new Vector2(-hw, -hd), new Vector2(hw, -hd));
            Edge(new Vector2(hw, -hd), new Vector2(hw, hd));
            Edge(new Vector2(hw, hd), new Vector2(-hw, hd));
            Edge(new Vector2(-hw, hd), new Vector2(-hw, -hd));

            int count = pts.Count;
            var top = new Vector3[count];
            var bottom = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                var p = pts[i];
                var dir = p.normalized;
                // 角の点はコーナー方向へ、辺の点は外法線方向へ押し出す
                float outTop = Jit(0.15f, 0.7f);
                float outBase = outTop + Jit(0.5f, 1.3f);
                float midYJit = Jit(-0.15f, 0.15f);
                top[i] = new Vector3(cx + p.x + dir.x * outTop, topY + midYJit * 0f, cz + p.y + dir.y * outTop);
                bottom[i] = new Vector3(cx + p.x + dir.x * outBase, baseY, cz + p.y + dir.y * outBase);
            }

            var v = new List<Vector3>();
            var tris = new List<int>();
            void Tri(Vector3 a, Vector3 b, Vector3 c)
            {
                int i = v.Count;
                v.Add(a); v.Add(b); v.Add(c);
                tris.Add(i); tris.Add(i + 1); tris.Add(i + 2);
            }

            // 崖側面（各セグメントに中間の折れ目を入れて岩らしく）
            for (int i = 0; i < count; i++)
            {
                int j = (i + 1) % count;
                var t0 = top[i]; var t1 = top[j];
                var b0 = bottom[i]; var b1 = bottom[j];
                // 中間点を少し外へ張り出させる
                float bulge = Jit(0.1f, 0.5f);
                var mid0 = Vector3.Lerp(t0, b0, 0.5f); var d0 = new Vector3(mid0.x - cx, 0, mid0.z - cz).normalized;
                var mid1 = Vector3.Lerp(t1, b1, 0.5f); var d1 = new Vector3(mid1.x - cx, 0, mid1.z - cz).normalized;
                mid0 += d0 * bulge; mid1 += d1 * bulge;

                Tri(t0, t1, mid1); Tri(t0, mid1, mid0);
                Tri(mid0, mid1, b1); Tri(mid0, b1, b0);
            }

            // 上面（中心からのファン。外周ジッターは外向きのみなので元の矩形を覆う）
            var center = new Vector3(cx, topY, cz);
            for (int i = 0; i < count; i++)
            {
                int j = (i + 1) % count;
                Tri(center, new Vector3(top[j].x, topY, top[j].z), new Vector3(top[i].x, topY, top[i].z));
            }

            var mesh = new Mesh { name = name + "_Mesh" };
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(v);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
        }

        // ---------------------------------------------------------------
        // 2. 石垣: 天守台（Tier3）の外周に石ブロックを積む
        // ---------------------------------------------------------------
        [MenuItem("Tools/Tsukikage/Terrain/2. Build Stone Walls")]
        public static void BuildStoneWalls()
        {
            if (!GuardEditMode()) return;
            EnsureDirs();
            var old = GameObject.Find("Terrain_StoneWalls");
            if (old != null) Object.DestroyImmediate(old);
            var root = new GameObject("Terrain_StoneWalls");
            var mat = GetOrCreateMat("Terrain_StoneWall", StoneWall);
            var matD = GetOrCreateMat("Terrain_StoneWallDark", RockDark);

            // Tier3 上面(y=6) の外周: x -3..11, z -2..10（中心4,4 半幅7 半奥6）
            var rand = new System.Random(77);
            float Jit(float a, float b) => a + (float)rand.NextDouble() * (b - a);

            void WallRun(Vector3 from, Vector3 to, float gapT0 = -1f, float gapT1 = -1f)
            {
                float len = Vector3.Distance(from, to);
                var dir = (to - from).normalized;
                var side = Vector3.Cross(Vector3.up, dir);
                int blocks = Mathf.FloorToInt(len / 1.25f);
                for (int i = 0; i < blocks; i++)
                {
                    for (int row = 0; row < 2; row++)
                    {
                        float offset = (row == 0) ? 0f : 0.6f; // 上段は半ブロックずらす
                        float t = (i * 1.25f + offset + 0.55f) / len;
                        if (t > 1f) continue;
                        if (t > gapT0 && t < gapT1) continue; // 石段用の出入口
                        var pos = Vector3.Lerp(from, to, t)
                                  + Vector3.up * (0.19f + row * 0.36f)
                                  + side * Jit(-0.03f, 0.03f);
                        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        go.name = "Ishigaki_Block";
                        go.transform.SetParent(root.transform);
                        go.transform.position = pos;
                        go.transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(0f, 90f, 0f);
                        go.transform.localScale = new Vector3(Jit(1.0f, 1.2f), Jit(0.32f, 0.38f), Jit(0.35f, 0.45f));
                        go.GetComponent<Renderer>().sharedMaterial = (i + row) % 3 == 0 ? matD : mat;
                        Object.DestroyImmediate(go.GetComponent<Collider>());
                    }
                }
            }

            float y = 6f;
            var c00 = new Vector3(-2.6f, y, -1.6f);
            var c10 = new Vector3(10.6f, y, -1.6f);
            var c11 = new Vector3(10.6f, y, 9.6f);
            var c01 = new Vector3(-2.6f, y, 9.6f);
            // 南面は石段(x=2.6〜5.4)の分だけ開けておく
            WallRun(c00, c10, 0.35f, 0.65f); WallRun(c10, c11); WallRun(c11, c01); WallRun(c01, c00);

            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Tsukikage] 石垣を生成しました");
        }

        // ---------------------------------------------------------------
        // 3. 石段: 台地1→2、2→3 をつなぐ階段
        // ---------------------------------------------------------------
        [MenuItem("Tools/Tsukikage/Terrain/3. Build Stone Steps")]
        public static void BuildStoneSteps()
        {
            if (!GuardEditMode()) return;
            EnsureDirs();
            var old = GameObject.Find("Terrain_Steps");
            if (old != null) Object.DestroyImmediate(old);
            var root = new GameObject("Terrain_Steps");
            var mat = GetOrCreateMat("Terrain_StoneWall", StoneWall);

            // (x, 上面yの高い側, 低い側, 出発z=台地の前縁, 下る方向-z)
            Stairs(root, mat, "Steps_T1_T2", -2f, 4f, 2f, -8f);
            Stairs(root, mat, "Steps_T2_T3", 4f, 6f, 4f, -2f);

            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Tsukikage] 石段を生成しました");
        }

        static void Stairs(GameObject parent, Material mat, string name, float x, float topY, float bottomY, float edgeZ)
        {
            var group = new GameObject(name);
            group.transform.SetParent(parent.transform);
            float rise = 0.25f, run = 0.4f, width = 2.6f;
            int steps = Mathf.CeilToInt((topY - bottomY) / rise);
            for (int i = 0; i < steps; i++)
            {
                float y = topY - rise * (i + 1);           // 段の上面
                float z = edgeZ - run * i - run * 0.5f;    // 前縁から外へ
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "Step";
                go.transform.SetParent(group.transform);
                float h = 0.12f;
                go.transform.position = new Vector3(x, y - h * 0.5f + rise * 0f, z);
                go.transform.localScale = new Vector3(width, rise, run + 0.12f);
                go.GetComponent<Renderer>().sharedMaterial = mat;
                Object.DestroyImmediate(go.GetComponent<Collider>());
                // 位置補正: 段の上面が y になるように
                go.transform.position = new Vector3(x, y - rise * 0.5f, z);
            }
        }

        // ---------------------------------------------------------------
        // 4. 水面と滝
        // ---------------------------------------------------------------
        [MenuItem("Tools/Tsukikage/Terrain/4. Build Water And Falls")]
        public static void BuildWaterAndFalls()
        {
            if (!GuardEditMode()) return;
            EnsureDirs();
            var old = GameObject.Find("Terrain_Water");
            if (old != null) Object.DestroyImmediate(old);
            var root = new GameObject("Terrain_Water");

            var waterMat = GetOrCreateEmissiveMat("Terrain_Water", WaterCol, 0.25f);
            var foamMat = GetOrCreateEmissiveMat("Terrain_Foam", FoamCol, 0.6f);

            // 既存のブロックアウト水面を差し替え
            var blockout = GameObject.Find("Blockout");
            if (blockout != null)
            {
            var w = blockout.transform.Find("Water");
                if (w != null) w.gameObject.SetActive(false);
            }
            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = "Water_Surface";
            plane.transform.SetParent(root.transform);
            plane.transform.position = new Vector3(0f, -0.45f, 0f);
            plane.transform.localScale = new Vector3(14f, 1f, 14f);
            plane.GetComponent<Renderer>().sharedMaterial = waterMat;
            Object.DestroyImmediate(plane.GetComponent<Collider>());

            // 滝: 台地1の東縁(上面y=2)から水面へ。垂直板を崖の張り出しに貫通させて密着感を出す
            Waterfall(root, waterMat, foamMat, new Vector3(18.0f, 2f, 2f), 2.2f, 2.7f);
            // 滝: 台地2の東縁(上面y=4)から台地1へ（小滝）
            Waterfall(root, waterMat, foamMat, new Vector3(15.1f, 4f, 6f), 1.4f, 2.2f);

            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Tsukikage] 水面と滝を生成しました");
        }

        static void Waterfall(GameObject parent, Material water, Material foam, Vector3 topEdge, float width, float drop)
        {
            var group = new GameObject("Waterfall");
            group.transform.SetParent(parent.transform);
            group.transform.position = topEdge;

            // 落水（崖の勾配約25°に沿わせた薄い板。岩に少しめり込ませて密着させる）
            var fall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fall.name = "Fall";
            fall.transform.SetParent(group.transform);
            fall.transform.localPosition = new Vector3(0f, -drop * 0.5f + 0.05f, 0f);
            fall.transform.localScale = new Vector3(0.16f, drop * 1.05f, width);
            fall.GetComponent<Renderer>().sharedMaterial = water;
            Object.DestroyImmediate(fall.GetComponent<Collider>());

            // 落ち口のリップ（台地の上面から滝板まで橋渡しする）
            var lip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lip.name = "Lip";
            lip.transform.SetParent(group.transform);
            lip.transform.localPosition = new Vector3(-0.55f, 0.04f, 0f);
            lip.transform.localScale = new Vector3(1.5f, 0.1f, width);
            lip.GetComponent<Renderer>().sharedMaterial = water;
            Object.DestroyImmediate(lip.GetComponent<Collider>());

            // 水面の泡（扁平な円柱）
            var splash = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            splash.name = "Splash";
            splash.transform.SetParent(group.transform);
            splash.transform.localPosition = new Vector3(0.35f, -drop + 0.06f, 0f);
            splash.transform.localScale = new Vector3(1.6f, 0.04f, width + 0.7f);
            splash.GetComponent<Renderer>().sharedMaterial = foam;
            Object.DestroyImmediate(splash.GetComponent<Collider>());
        }

        // ---------------------------------------------------------------
        static void SaveMeshes(GameObject rootGo)
        {
            foreach (var mf in rootGo.GetComponentsInChildren<MeshFilter>())
            {
                var mesh = mf.sharedMesh;
                if (mesh != null && !AssetDatabase.Contains(mesh))
                {
                    // 既存パスへのCreateAsset上書きはレンダラーの境界を壊すため先に削除する
                    var path = TerrainDir + "/" + mesh.name + ".asset";
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
            foreach (var d in new[] { TerrainDir, MatDir })
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
