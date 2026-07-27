using System.IO;
using FishMatch.Match3;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FishMatch.EditorTools
{
    // Builds the Aquarium scene (owned fish swimming + passive gold income + tap-to-collect coins)
    // and wires a nav button into it from Match3Test. Safe to re-run - removes its own previous
    // output first. Run via:
    // Unity.exe -batchmode -projectPath <path> -executeMethod FishMatch.EditorTools.AquariumSetup.Run -quit
    public static class AquariumSetup
    {
        private const int TextureSize = 128;
        private const string ArtFolder = "Assets/Art/Match3";
        private const string PrefabFolder = "Assets/Prefabs/Aquarium";
        private const string ScenePath = "Assets/Scenes/Aquarium.unity";
        private const string Match3ScenePath = "Assets/Scenes/Match3Test.unity";
        private const string CoinSpritePath = ArtFolder + "/Shape_Yellow.png";

        public static void Run()
        {
            Directory.CreateDirectory(PrefabFolder);

            var fishSpritePath = CreateFishShapeSprite();
            var coinPrefabPath = CreateCoinPrefab();
            var fishPrefabPath = CreateFishPrefab(fishSpritePath, coinPrefabPath);

            BuildAquariumScene(fishPrefabPath);
            RegisterScenesInBuildSettings();

            AssetDatabase.SaveAssets();
            Debug.Log("AquariumSetup: Aquarium scene + fish/coin prefabs created, nav buttons wired.");
        }

        private static string CreateFishShapeSprite()
        {
            var path = $"{ArtFolder}/Shape_Fish.png";
            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
            }

            var color = new Color(0.35f, 0.65f, 0.9f);
            var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
            var pixels = new Color[TextureSize * TextureSize];
            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    float u = (x + 0.5f) / TextureSize * 2f - 1f;
                    float v = (y + 0.5f) / TextureSize * 2f - 1f;
                    pixels[y * TextureSize + x] = IsInsideFish(u, v) ? color : new Color(0f, 0f, 0f, 0f);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.spritePixelsPerUnit = TextureSize;
            importer.SaveAndReimport();

            return path;
        }

        // Body: ellipse facing right; Tail: triangle extending left, tapering to a point.
        private static bool IsInsideFish(float u, float v)
        {
            float bu = (u - 0.05f) / 0.65f;
            float bv = v / 0.55f;
            bool inBody = (bu * bu + bv * bv) <= 1f;

            bool inTail = false;
            if (u <= -0.4f && u >= -1f)
            {
                float t = (u + 1f) / 0.6f;
                float halfHeight = Mathf.Lerp(0.05f, 0.5f, t);
                inTail = Mathf.Abs(v) <= halfHeight;
            }

            return inBody || inTail;
        }

        private static string CreateCoinPrefab()
        {
            var path = $"{PrefabFolder}/CoinPickup.prefab";
            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
            }

            var coinSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CoinSpritePath);

            var go = new GameObject("CoinPickup");
            var spriteRenderer = go.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = coinSprite;
            go.transform.localScale = Vector3.one * 0.4f;
            go.AddComponent<CoinPickup>();

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return path;
        }

        private static string CreateFishPrefab(string fishSpritePath, string coinPrefabPath)
        {
            var path = $"{PrefabFolder}/AquariumFish.prefab";
            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
            }

            var fishSprite = AssetDatabase.LoadAssetAtPath<Sprite>(fishSpritePath);
            var coinPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(coinPrefabPath);

            var go = new GameObject("AquariumFish");
            var spriteRenderer = go.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = fishSprite;

            var controller = go.AddComponent<AquariumFishController>();
            var so = new SerializedObject(controller);
            so.FindProperty("coinPickupPrefab").objectReferenceValue = coinPrefab;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return path;
        }

        private static void BuildAquariumScene(string fishPrefabPath)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.05f, 0.12f, 0.18f);

            // Reload by path after NewScene() - Object references fetched before it are invalidated.
            var fishPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fishPrefabPath);
            var fishRoster = new FishSpeciesDefinition[FishCollectionSetup.FishRosterAssetPaths.Length];
            for (int i = 0; i < fishRoster.Length; i++)
            {
                fishRoster[i] = AssetDatabase.LoadAssetAtPath<FishSpeciesDefinition>(FishCollectionSetup.FishRosterAssetPaths[i]);
            }

            var cameraGO = new GameObject("Main Camera");
            cameraGO.tag = "MainCamera";
            var cam = cameraGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.12f, 0.18f);
            cameraGO.transform.position = new Vector3(0f, 0f, -10f);

            var managerGO = new GameObject("AquariumManager");
            var manager = managerGO.AddComponent<AquariumManager>();
            var managerSo = new SerializedObject(manager);
            managerSo.FindProperty("aquariumFishPrefab").objectReferenceValue = fishPrefab;
            managerSo.FindProperty("tankBoundsMin").vector2Value = new Vector2(-4f, -3f);
            managerSo.FindProperty("tankBoundsMax").vector2Value = new Vector2(4f, 3f);

            // Resize then commit before writing elements - see the note in LevelSetup.CreateTestScene.
            var rosterProp = managerSo.FindProperty("fishRoster");
            rosterProp.arraySize = fishRoster.Length;
            managerSo.ApplyModifiedPropertiesWithoutUndo();

            managerSo.Update();
            rosterProp = managerSo.FindProperty("fishRoster");
            for (int i = 0; i < fishRoster.Length; i++)
            {
                rosterProp.GetArrayElementAtIndex(i).objectReferenceValue = fishRoster[i];
            }
            managerSo.ApplyModifiedPropertiesWithoutUndo();

            BuildEventSystem();
            var canvasGO = BuildCanvas();

            var goldText = LevelSetup.CreateText(canvasGO.transform, "GoldText", "Altın: 0",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -30f), new Vector2(320f, 70f));

            var returnButtonGO = CreateReturnButton(canvasGO.transform);

            var hudGO = new GameObject("HUD");
            hudGO.transform.SetParent(canvasGO.transform, false);
            var hud = hudGO.AddComponent<AquariumHUDController>();
            var hudSo = new SerializedObject(hud);
            hudSo.FindProperty("goldText").objectReferenceValue = goldText;
            hudSo.ApplyModifiedPropertiesWithoutUndo();

            var navButton = returnButtonGO.AddComponent<SceneLoadButton>();
            var navSo = new SerializedObject(navButton);
            navSo.FindProperty("sceneName").stringValue = "Match3Test";
            navSo.ApplyModifiedPropertiesWithoutUndo();

            var menuButtonGO = CreateNavButtonVisual(canvasGO.transform, "MainMenuButton", "Ana Menü",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(280f, 90f), new Vector2(260f, 90f));
            var menuNavButton = menuButtonGO.AddComponent<SceneLoadButton>();
            var menuNavSo = new SerializedObject(menuNavButton);
            menuNavSo.FindProperty("sceneName").stringValue = "MainMenu";
            menuNavSo.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static GameObject CreateReturnButton(Transform parent)
        {
            return CreateNavButtonVisual(parent, "ReturnButton", "Seviyeye Dön",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 90f), new Vector2(260f, 90f));
        }

        // Internal (not private) so other one-off setup scripts under Assets/Editor can reuse it.
        internal static GameObject CreateNavButtonVisual(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var image = go.AddComponent<Image>();
            image.color = new Color(0.2f, 0.4f, 0.6f);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            go.AddComponent<Button>();

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(go.transform, false);
            var text = textGO.AddComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 32;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;

            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return go;
        }

        private static void BuildEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        private static GameObject BuildCanvas()
        {
            var go = new GameObject("Canvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        private static void RegisterScenesInBuildSettings()
        {
            BuildSettingsUtil.EnsureSceneInBuildSettings(Match3ScenePath);
            BuildSettingsUtil.EnsureSceneInBuildSettings(ScenePath);
        }
    }
}
