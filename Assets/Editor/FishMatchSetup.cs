using System.IO;
using FishMatch.Match3;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FishMatch.EditorTools
{
    // One-time bootstrap: generates placeholder gem sprites/definitions/prefab and a Match3Test scene.
    // Safe to re-run - deletes its own previous output first. Run via:
    // Unity.exe -batchmode -projectPath <path> -executeMethod FishMatch.EditorTools.FishMatchSetup.Run -quit
    public static class FishMatchSetup
    {
        private const int TextureSize = 128;
        private const string ArtFolder = "Assets/Art/Match3";
        private const string GemFolder = "Assets/ScriptableObjects/Gems";
        private const string PrefabFolder = "Assets/Prefabs/Match3";
        private const string ScenePath = "Assets/Scenes/Match3Test.unity";

        private struct GemSpec
        {
            public GemType Type;
            public Color Color;
            public System.Func<float, float, bool> InsideTest;
        }

        public static void Run()
        {
            Directory.CreateDirectory(ArtFolder);
            Directory.CreateDirectory(GemFolder);
            Directory.CreateDirectory(PrefabFolder);
            Directory.CreateDirectory("Assets/Scenes");

            var gemSpecs = new[]
            {
                new GemSpec { Type = GemType.Yellow, Color = new Color(1f, 0.82f, 0.1f), InsideTest = IsInsideCircle },
                new GemSpec { Type = GemType.Blue, Color = new Color(0.15f, 0.55f, 0.95f), InsideTest = IsInsideDrop },
                new GemSpec { Type = GemType.Red, Color = new Color(0.9f, 0.15f, 0.2f), InsideTest = IsInsideDiamond },
                new GemSpec { Type = GemType.Green, Color = new Color(0.2f, 0.75f, 0.35f), InsideTest = IsInsideRoundedSquare },
                new GemSpec { Type = GemType.Purple, Color = new Color(0.6f, 0.25f, 0.85f), InsideTest = IsInsideHexagon },
            };

            var definitionPaths = new string[gemSpecs.Length];
            for (int i = 0; i < gemSpecs.Length; i++)
            {
                var spec = gemSpecs[i];
                var sprite = CreateShapeSprite(spec.Type.ToString(), spec.Color, spec.InsideTest);
                definitionPaths[i] = CreateGemDefinition(spec.Type, sprite);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var prefabPath = CreateGemPrefab();
            CreateTestScene(prefabPath, definitionPaths);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("FishMatchSetup: placeholder gem sprites/definitions/prefab + Match3Test scene created.");
        }

        private static Sprite CreateShapeSprite(string name, Color color, System.Func<float, float, bool> insideTest)
        {
            var path = $"{ArtFolder}/Shape_{name}.png";
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

        private static string CreateGemDefinition(GemType type, Sprite sprite)
        {
            var path = $"{GemFolder}/Gem_{type}.asset";
            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
            }

            var definition = ScriptableObject.CreateInstance<GemDefinition>();
            definition.type = type;
            definition.shapeSprite = sprite;
            definition.tintColor = Color.white; // color is already baked into the generated sprite pixels
            AssetDatabase.CreateAsset(definition, path);
            return path;
        }

        private static string CreateGemPrefab()
        {
            var path = $"{PrefabFolder}/Gem.prefab";
            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
            }

            var go = new GameObject("Gem");
            go.AddComponent<SpriteRenderer>();
            go.AddComponent<Gem>();
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return path;
        }

        private static void CreateTestScene(string gemPrefabPath, string[] definitionPaths)
        {
            // EditorSceneManager.NewScene() below triggers asset-database housekeeping that invalidates
            // any Object reference fetched before it (they read back as fake-null afterward) - so every
            // asset reference used here is deliberately (re)loaded from its path after NewScene(), not before.
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var definitions = new GemDefinition[definitionPaths.Length];
            for (int i = 0; i < definitionPaths.Length; i++)
            {
                definitions[i] = AssetDatabase.LoadAssetAtPath<GemDefinition>(definitionPaths[i]);
            }
            var gemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(gemPrefabPath);

            // Clear the default 3D skybox so the Scene view also reads as flat/2D, not just the Game view.
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.08f, 0.1f, 0.15f);

            var cameraGO = new GameObject("Main Camera");
            cameraGO.tag = "MainCamera";
            var cam = cameraGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 4.5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.1f, 0.15f);
            cameraGO.transform.position = new Vector3(3f, 3f, -10f);

            var gridGO = new GameObject("Match3Grid");
            var controller = gridGO.AddComponent<Match3GridController>();

            var so = new SerializedObject(controller);
            so.FindProperty("width").intValue = 7;
            so.FindProperty("height").intValue = 7;
            so.FindProperty("cellSize").floatValue = 1f;
            so.FindProperty("gemPrefab").objectReferenceValue = gemPrefab;

            // Resizing and populating an array via SerializedProperty must be two separate commits -
            // the resize isn't backed by real storage until applied, so element writes made before
            // that point silently get discarded.
            var definitionsProp = so.FindProperty("gemDefinitions");
            definitionsProp.arraySize = definitions.Length;
            so.ApplyModifiedPropertiesWithoutUndo();

            so.Update();
            definitionsProp = so.FindProperty("gemDefinitions");
            for (int i = 0; i < definitions.Length; i++)
            {
                definitionsProp.GetArrayElementAtIndex(i).objectReferenceValue = definitions[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        // Shape tests operate in a [-1, 1] centered coordinate space.
        private static bool IsInsideCircle(float u, float v) => u * u + v * v <= 0.92f;

        private static bool IsInsideDiamond(float u, float v) => Mathf.Abs(u) + Mathf.Abs(v) <= 1f;

        private static bool IsInsideRoundedSquare(float u, float v)
        {
            float au = Mathf.Abs(u);
            float av = Mathf.Abs(v);
            return Mathf.Pow(au, 4f) + Mathf.Pow(av, 4f) <= 0.85f;
        }

        private static bool IsInsideHexagon(float u, float v)
        {
            float au = Mathf.Abs(u);
            float av = Mathf.Abs(v);
            return av <= 0.9f && (au * 0.8660254f + av * 0.5f) <= 0.9f;
        }

        // Narrow, pointed top; wide, rounded bottom (widens downward, per the design doc).
        private static bool IsInsideDrop(float u, float v)
        {
            float t = Mathf.Clamp01((v + 1f) / 2f); // 0 at bottom, 1 at top
            float radius = Mathf.Lerp(0.95f, 0.05f, t * t);

            if (v < -0.3f)
            {
                float dy = v + 0.3f;
                return u * u + dy * dy <= 0.6f;
            }

            return Mathf.Abs(u) <= radius;
        }
    }
}
