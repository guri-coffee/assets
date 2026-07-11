using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Tsukikage.EditorTools
{
    /// <summary>
    /// TSUKIKAGE プロジェクトの初期セットアップツール。
    /// Tools > Tsukikage メニューから 1 → 2 → 3 の順に実行する。
    /// </summary>
    public static class TsukikageBootstrap
    {
        const string Root = "Assets/Tsukikage";
        const string SettingsDir = Root + "/Settings";

        static readonly string[] Folders =
        {
            "Models/Terrain", "Models/Castle", "Models/Houses", "Models/Bridges",
            "Models/Props", "Models/Vegetation", "Materials", "Textures",
            "Prefabs", "Scenes", "Shaders", "VFX", "Settings", "Demo",
        };

        // ---------------------------------------------------------------
        // 1. フォルダ構成と URP のセットアップ
        // ---------------------------------------------------------------
        [MenuItem("Tools/Tsukikage/1. Setup URP + Folders")]
        public static void SetupProject()
        {
            foreach (var folder in Folders)
            {
                var path = Path.Combine(Root, folder);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            }
            AssetDatabase.Refresh();

            var rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
            rendererData.postProcessData = AssetDatabase.LoadAssetAtPath<PostProcessData>(
                "Packages/com.unity.render-pipelines.universal/Runtime/Data/PostProcessData.asset");
            AssetDatabase.CreateAsset(rendererData, SettingsDir + "/Tsukikage_Renderer.asset");

            var pipeline = UniversalRenderPipelineAsset.Create(rendererData);
            pipeline.supportsHDR = true;
            AssetDatabase.CreateAsset(pipeline, SettingsDir + "/Tsukikage_URP.asset");

            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
            PlayerSettings.colorSpace = ColorSpace.Linear;

            AssetDatabase.SaveAssets();
            Debug.Log("[Tsukikage] URP とフォルダ構成のセットアップが完了しました。");
        }

        // ---------------------------------------------------------------
        // 2. 夜景デモシーンの生成（ライティング + ポストプロセス）
        // ---------------------------------------------------------------
        [MenuItem("Tools/Tsukikage/2. Create Night Demo Scene")]
        public static void CreateNightScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // 月光（青白い弱めの指向性ライト）
            var moon = Object.FindObjectOfType<Light>();
            moon.name = "Moon Light";
            moon.color = Hex("#8c9ef2");
            moon.intensity = 0.35f;
            moon.transform.rotation = Quaternion.Euler(45f, -140f, 0f);

            // 環境光と霧（夜の藍 + 下層の雲海）
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = Hex("#2b2d5e");
            RenderSettings.ambientEquatorColor = Hex("#3a2f63");
            RenderSettings.ambientGroundColor = Hex("#1b1c38");
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = Hex("#41406e");
            RenderSettings.fogDensity = 0.012f;

            // アイソメ風カメラ
            var cam = Camera.main;
            cam.transform.position = new Vector3(-24f, 26f, -24f);
            cam.transform.rotation = Quaternion.Euler(35f, 45f, 0f);
            cam.fieldOfView = 35f;
            cam.GetUniversalAdditionalCameraData().renderPostProcessing = true;

            // ポストプロセス（この設定が夜景の雰囲気の要）
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, SettingsDir + "/Tsukikage_Night_PostFX.asset");

            var bloom = profile.Add<Bloom>();
            bloom.intensity.Override(1.4f);
            bloom.threshold.Override(0.85f);
            bloom.scatter.Override(0.65f);

            var color = profile.Add<ColorAdjustments>();
            color.postExposure.Override(0.2f);
            color.contrast.Override(12f);
            color.saturation.Override(8f);

            var whiteBalance = profile.Add<WhiteBalance>();
            whiteBalance.temperature.Override(-12f);

            var vignette = profile.Add<Vignette>();
            vignette.intensity.Override(0.28f);

            var volumeGo = new GameObject("Global Volume");
            var volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = profile;

            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(scene, Root + "/Scenes/Tsukikage_Night_Demo.unity");
            Debug.Log("[Tsukikage] 夜景デモシーンを作成しました: " + Root + "/Scenes/Tsukikage_Night_Demo.unity");
        }

        // ---------------------------------------------------------------
        // 3. グレーボックス（ブロックアウト）の生成
        //    参考画像の構図: 3段の台地 + 最上段に天守 + 水面 + 鳥居
        // ---------------------------------------------------------------
        [MenuItem("Tools/Tsukikage/3. Generate Greybox Blockout")]
        public static void GenerateBlockout()
        {
            var root = new GameObject("Blockout");

            var stone = CreateMaterial("Blockout_Stone", "#8a8fa3");
            var roof = CreateMaterial("Blockout_Roof", "#2f3542");
            var wood = CreateMaterial("Blockout_Wood", "#8b4a2f");
            var torii = CreateMaterial("Blockout_Torii", "#c2402a");
            var water = CreateMaterial("Blockout_Water", "#2e6f8e");
            var lantern = CreateEmissiveMaterial("Blockout_Lantern", "#ffb347", 3.0f);

            // 3段の台地（下段ほど広い）
            Box(root, stone, new Vector3(0f, 1f, 0f), new Vector3(34f, 2f, 30f), "Tier_1");
            Box(root, stone, new Vector3(2f, 3f, 2f), new Vector3(24f, 2f, 20f), "Tier_2");
            Box(root, stone, new Vector3(4f, 5f, 4f), new Vector3(14f, 2f, 12f), "Tier_3");

            // 天守（最上段に3層のプレースホルダー）
            Box(root, roof, new Vector3(4f, 7.5f, 4f), new Vector3(6f, 3f, 6f), "Castle_Base");
            Box(root, roof, new Vector3(4f, 10f, 4f), new Vector3(4.5f, 2f, 4.5f), "Castle_Mid");
            Box(root, roof, new Vector3(4f, 12f, 4f), new Vector3(3f, 2f, 3f), "Castle_Top");

            // 町屋（中段・下段に散らす）
            Box(root, wood, new Vector3(-6f, 4.75f, 6f), new Vector3(3f, 1.5f, 3f), "House_A");
            Box(root, wood, new Vector3(-4f, 4.75f, -4f), new Vector3(3.5f, 1.5f, 2.5f), "House_B");
            Box(root, wood, new Vector3(10f, 4.75f, -2f), new Vector3(3f, 1.5f, 3f), "House_C");
            Box(root, wood, new Vector3(-12f, 2.75f, -8f), new Vector3(3f, 1.5f, 3f), "House_D");
            Box(root, wood, new Vector3(-10f, 2.75f, 10f), new Vector3(2.5f, 1.5f, 3.5f), "House_E");
            Box(root, wood, new Vector3(12f, 2.75f, 10f), new Vector3(3f, 1.5f, 3f), "House_F");

            // 大鳥居（下段の入口）
            Box(root, torii, new Vector3(-19f, 3.5f, -13f), new Vector3(0.8f, 5f, 0.8f), "Torii_PillarL");
            Box(root, torii, new Vector3(-15f, 3.5f, -13f), new Vector3(0.8f, 5f, 0.8f), "Torii_PillarR");
            Box(root, torii, new Vector3(-17f, 6.2f, -13f), new Vector3(6.5f, 0.7f, 1f), "Torii_Beam");

            // 水面
            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = "Water";
            plane.transform.SetParent(root.transform);
            plane.transform.position = new Vector3(0f, -0.5f, 0f);
            plane.transform.localScale = new Vector3(12f, 1f, 12f);
            plane.GetComponent<Renderer>().sharedMaterial = water;

            // 提灯（エミッシブ球 + ポイントライトで夜景の暖色を確認）
            PlaceLantern(root, lantern, new Vector3(-17f, 3.2f, -11f));
            PlaceLantern(root, lantern, new Vector3(-6f, 6.2f, 4f));
            PlaceLantern(root, lantern, new Vector3(10f, 6.2f, 0f));
            PlaceLantern(root, lantern, new Vector3(4f, 9.5f, 0.5f));
            PlaceLantern(root, lantern, new Vector3(-10f, 4.2f, 8f));

            AssetDatabase.SaveAssets();
            Debug.Log("[Tsukikage] グレーボックスを生成しました。構図を調整してからモデリングに入ってください。");
        }

        // ---------------------------------------------------------------
        static void Box(GameObject parent, Material mat, Vector3 pos, Vector3 size, string name)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent.transform);
            go.transform.position = pos;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = mat;
        }

        static void PlaceLantern(GameObject parent, Material mat, Vector3 pos)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Lantern";
            go.transform.SetParent(parent.transform);
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 0.5f;
            go.GetComponent<Renderer>().sharedMaterial = mat;

            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = Hex("#ffb347");
            light.intensity = 2.2f;
            light.range = 7f;
        }

        static Material CreateMaterial(string name, string hex)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", Hex(hex));
            SaveMaterial(mat, name);
            return mat;
        }

        static Material CreateEmissiveMaterial(string name, string hex, float intensity)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", Hex(hex));
            mat.EnableKeyword("_EMISSION");
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            mat.SetColor("_EmissionColor", Hex(hex) * intensity);
            SaveMaterial(mat, name);
            return mat;
        }

        static void SaveMaterial(Material mat, string name)
        {
            var dir = Root + "/Materials/Blockout";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            AssetDatabase.CreateAsset(mat, dir + "/" + name + ".mat");
        }

        static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var color);
            return color;
        }
    }
}
