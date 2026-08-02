using System.IO;
using FishMatch.Match3;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace FishMatch.EditorTools
{
    // Adds a gold-spending decoration shop to the existing Aquarium scene: a catalog of 3 flat-shape
    // decorations, a shop panel (open/close + buy rows built at runtime by DecorationShopController,
    // since owned/affordable state depends on save data), and wires AquariumManager to spawn owned
    // decorations in the tank. Safe to re-run - removes its own previous output first. Run via:
    // Unity.exe -batchmode -projectPath <path> -executeMethod FishMatch.EditorTools.DecorationShopSetup.Run -quit
    public static class DecorationShopSetup
    {
        private const int TextureSize = 128;
        private const string ArtFolder = "Assets/Art/Match3";
        private const string KenneyFishFolder = "Assets/Art/Kenney/Fish";
        private const string CatalogFolder = "Assets/ScriptableObjects/Decorations";
        private const string PrefabFolder = "Assets/Prefabs/Aquarium";
        private const string AquariumScenePath = "Assets/Scenes/Aquarium.unity";

        public static readonly string[] DecorationAssetPaths =
        {
            CatalogFolder + "/Decoration_Yosun.asset",
            CatalogFolder + "/Decoration_Mercan.asset",
            CatalogFolder + "/Decoration_Inci.asset",
        };

        public static void Run()
        {
            Directory.CreateDirectory(CatalogFolder);
            Directory.CreateDirectory(PrefabFolder);

            CreateCatalog();
            var decorationPrefabPath = CreateDecorationPrefab();

            EditorSceneManager.OpenScene(AquariumScenePath);

            RemoveExisting("ShopOpenButton");
            RemoveExisting("ShopPanel");
            RemoveExisting("DecorationShopController");

            var catalog = new DecorationDefinition[DecorationAssetPaths.Length];
            for (int i = 0; i < catalog.Length; i++)
            {
                catalog[i] = AssetDatabase.LoadAssetAtPath<DecorationDefinition>(DecorationAssetPaths[i]);
            }
            var decorationPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(decorationPrefabPath);

            var managerGO = EditorSetupUtil.RequireGameObject("AquariumManager", "AquariumSetup");
            var managerSo = new SerializedObject(managerGO.GetComponent<AquariumManager>());
            managerSo.FindProperty("decorationPrefab").objectReferenceValue = decorationPrefab;

            var managerCatalogProp = managerSo.FindProperty("decorationCatalog");
            managerCatalogProp.arraySize = catalog.Length;
            managerSo.ApplyModifiedPropertiesWithoutUndo();

            managerSo.Update();
            managerCatalogProp = managerSo.FindProperty("decorationCatalog");
            for (int i = 0; i < catalog.Length; i++)
            {
                managerCatalogProp.GetArrayElementAtIndex(i).objectReferenceValue = catalog[i];
            }
            managerSo.ApplyModifiedPropertiesWithoutUndo();

            var canvasGO = EditorSetupUtil.RequireGameObject("Canvas", "AquariumSetup");
            BuildShopUI(canvasGO.transform, catalog);

            var activeScene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
            AssetDatabase.SaveAssets();
            Debug.Log("DecorationShopSetup: decoration catalog + shop UI wired into Aquarium scene.");
        }

        private static void RemoveExisting(string name)
        {
            var go = GameObject.Find(name);
            if (go != null)
            {
                Object.DestroyImmediate(go);
            }
        }

        private static void CreateCatalog()
        {
            // Yosun/Mercan use real Kenney fish-pack art (seaweed/coral plants exist in that pack);
            // Inci Kabuğu stays a flat procedural shape - the pack has no shell/pearl sprite to match it.
            CreateDecorationFromKenney(DecorationAssetPaths[0], "Yosun", "Yosun", 30, "Decoration_Seaweed", new Vector2(-2.8f, -2.3f));
            CreateDecorationFromKenney(DecorationAssetPaths[1], "Mercan", "Mercan", 60, "Decoration_Coral", new Vector2(0f, -2.5f));
            CreateDecoration(DecorationAssetPaths[2], "Inci", "İnci Kabuğu", 100,
                new Color(0.9f, 0.85f, 0.7f), IsInsideCircle, new Vector2(2.8f, -2.3f));
        }

        private static void CreateDecorationFromKenney(string path, string spriteName, string displayName, int price,
            string kenneySpriteName, Vector2 tankPosition)
        {
            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
            }

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{KenneyFishFolder}/{kenneySpriteName}.png");

            var decoration = ScriptableObject.CreateInstance<DecorationDefinition>();
            decoration.decorationId = spriteName;
            decoration.displayName = displayName;
            decoration.price = price;
            decoration.sprite = sprite;
            decoration.tankPosition = tankPosition;
            AssetDatabase.CreateAsset(decoration, path);
        }

        private static void CreateDecoration(string path, string spriteName, string displayName, int price,
            Color color, System.Func<float, float, bool> insideTest, Vector2 tankPosition)
        {
            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
            }

            var sprite = CreateShapeSprite(spriteName, color, insideTest);

            var decoration = ScriptableObject.CreateInstance<DecorationDefinition>();
            decoration.decorationId = spriteName;
            decoration.displayName = displayName;
            decoration.price = price;
            decoration.sprite = sprite;
            decoration.tankPosition = tankPosition;
            AssetDatabase.CreateAsset(decoration, path);
        }

        // Flat single-color fill, deliberately no shading/facets - matches the original placeholder
        // gem/fish shapes, not the faceted-gem shading attempt that was reverted for looking bad.
        private static Sprite CreateShapeSprite(string name, Color color, System.Func<float, float, bool> insideTest)
        {
            var path = $"{ArtFolder}/Shape_Decoration_{name}.png";
            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
            }

            var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
            var pixels = new Color[TextureSize * TextureSize];
            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    float u = (x + 0.5f) / TextureSize * 2f - 1f;
                    float v = (y + 0.5f) / TextureSize * 2f - 1f;
                    pixels[y * TextureSize + x] = insideTest(u, v) ? color : new Color(0f, 0f, 0f, 0f);
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

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static string CreateDecorationPrefab()
        {
            var path = $"{PrefabFolder}/DecorationPickup.prefab";
            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
            }

            var go = new GameObject("Decoration");
            go.AddComponent<SpriteRenderer>();
            go.transform.localScale = Vector3.one * 0.8f;

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return path;
        }

        private static void BuildShopUI(Transform canvasParent, DecorationDefinition[] catalog)
        {
            var openButtonGO = AquariumSetup.CreateNavButtonVisual(canvasParent, "ShopOpenButton", "Dükkan",
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-30f, 90f), new Vector2(220f, 90f));

            var shopPanel = CreatePanel(canvasParent);

            LevelSetup.CreateText(shopPanel.transform, "ShopTitleText", "Dükkan",
                new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400f, 100f));

            var rowContainerGO = new GameObject("RowContainer");
            rowContainerGO.transform.SetParent(shopPanel.transform, false);
            var rowContainerRect = rowContainerGO.AddComponent<RectTransform>();
            rowContainerRect.anchorMin = new Vector2(0.5f, 0.6f);
            rowContainerRect.anchorMax = new Vector2(0.5f, 0.6f);
            rowContainerRect.pivot = new Vector2(0.5f, 1f);
            rowContainerRect.anchoredPosition = Vector2.zero;
            rowContainerRect.sizeDelta = new Vector2(600f, 400f);

            var closeButtonGO = AquariumSetup.CreateNavButtonVisual(shopPanel.transform, "ShopCloseButton", "Kapat",
                new Vector2(0.5f, 0.12f), new Vector2(0.5f, 0.12f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(260f, 90f));

            var controllerGO = new GameObject("DecorationShopController");
            controllerGO.transform.SetParent(canvasParent, false);
            var controller = controllerGO.AddComponent<DecorationShopController>();
            var controllerSo = new SerializedObject(controller);
            controllerSo.FindProperty("shopPanel").objectReferenceValue = shopPanel;
            controllerSo.FindProperty("openButton").objectReferenceValue = openButtonGO.GetComponent<Button>();
            controllerSo.FindProperty("closeButton").objectReferenceValue = closeButtonGO.GetComponent<Button>();
            controllerSo.FindProperty("rowContainer").objectReferenceValue = rowContainerRect;

            var catalogProp = controllerSo.FindProperty("catalog");
            catalogProp.arraySize = catalog.Length;
            controllerSo.ApplyModifiedPropertiesWithoutUndo();

            controllerSo.Update();
            catalogProp = controllerSo.FindProperty("catalog");
            for (int i = 0; i < catalog.Length; i++)
            {
                catalogProp.GetArrayElementAtIndex(i).objectReferenceValue = catalog[i];
            }
            controllerSo.ApplyModifiedPropertiesWithoutUndo();

            // DecorationShopController.Awake() also hides this at runtime, but that only runs in Play
            // mode - hide it here too so the scene doesn't look broken when just opened in the Editor.
            shopPanel.SetActive(false);
        }

        private static GameObject CreatePanel(Transform parent)
        {
            var go = new GameObject("ShopPanel");
            go.transform.SetParent(parent, false);

            var image = go.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.75f);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            return go;
        }

        // Shape tests operate in a [-1, 1] centered coordinate space.
        private static bool IsInsideCircle(float u, float v) => u * u + v * v <= 0.85f;

        private static bool IsInsideDiamond(float u, float v) => Mathf.Abs(u) + Mathf.Abs(v) <= 0.9f;

        private static bool IsInsideTallEllipse(float u, float v) =>
            (u * u) / (0.32f * 0.32f) + (v * v) / (0.95f * 0.95f) <= 1f;
    }
}
