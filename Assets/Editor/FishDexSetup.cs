using FishMatch.Match3;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace FishMatch.EditorTools
{
    // Builds the Fish-dex catalog scene (all species, collected status, completion %) and wires a
    // nav button into it from Aquarium. Safe to re-run - removes its own previous output first. Run via:
    // Unity.exe -batchmode -projectPath <path> -executeMethod FishMatch.EditorTools.FishDexSetup.Run -quit
    public static class FishDexSetup
    {
        private const string ScenePath = "Assets/Scenes/FishDex.unity";
        private const string AquariumScenePath = "Assets/Scenes/Aquarium.unity";

        public static void Run()
        {
            BuildFishDexScene();
            AddFishDexNavButton();
            BuildSettingsUtil.EnsureSceneInBuildSettings(ScenePath);

            AssetDatabase.SaveAssets();
            Debug.Log("FishDexSetup: FishDex scene created, nav button wired into Aquarium.");
        }

        private static void BuildFishDexScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.1f, 0.08f, 0.15f);

            // Reload by path after NewScene() - Object references fetched before it are invalidated.
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
            cam.backgroundColor = new Color(0.1f, 0.08f, 0.15f);
            cameraGO.transform.position = new Vector3(0f, 0f, -10f);

            BuildEventSystem();
            var canvasGO = BuildCanvas();

            var completionText = LevelSetup.CreateText(canvasGO.transform, "CompletionText", "%0 Tamamlandı",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(600f, 70f));

            var entriesText = LevelSetup.CreateText(canvasGO.transform, "EntriesText", "",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(900f, 1400f));
            entriesText.alignment = TextAnchor.UpperLeft;
            entriesText.fontSize = 28;

            var returnButtonGO = AquariumSetup.CreateNavButtonVisual(canvasGO.transform, "ReturnButton", "Akvaryuma Dön",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 90f), new Vector2(260f, 90f));
            var navButton = returnButtonGO.AddComponent<SceneLoadButton>();
            var navSo = new SerializedObject(navButton);
            navSo.FindProperty("sceneName").stringValue = "Aquarium";
            navSo.ApplyModifiedPropertiesWithoutUndo();

            var menuButtonGO = AquariumSetup.CreateNavButtonVisual(canvasGO.transform, "MainMenuButton", "Ana Menü",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(280f, 90f), new Vector2(260f, 90f));
            var menuNavButton = menuButtonGO.AddComponent<SceneLoadButton>();
            var menuNavSo = new SerializedObject(menuNavButton);
            menuNavSo.FindProperty("sceneName").stringValue = "MainMenu";
            menuNavSo.ApplyModifiedPropertiesWithoutUndo();

            var controllerGO = new GameObject("FishDexController");
            var controller = controllerGO.AddComponent<FishDexController>();
            var controllerSo = new SerializedObject(controller);

            var rosterProp = controllerSo.FindProperty("fishRoster");
            rosterProp.arraySize = fishRoster.Length;
            controllerSo.ApplyModifiedPropertiesWithoutUndo();

            controllerSo.Update();
            rosterProp = controllerSo.FindProperty("fishRoster");
            for (int i = 0; i < fishRoster.Length; i++)
            {
                rosterProp.GetArrayElementAtIndex(i).objectReferenceValue = fishRoster[i];
            }
            controllerSo.FindProperty("completionText").objectReferenceValue = completionText;
            controllerSo.FindProperty("entriesText").objectReferenceValue = entriesText;
            controllerSo.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void AddFishDexNavButton()
        {
            EditorSceneManager.OpenScene(AquariumScenePath);

            var canvasGO = EditorSetupUtil.RequireGameObject("Canvas", "AquariumSetup");

            var existing = canvasGO.transform.Find("FishDexNavButton");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            var buttonGO = AquariumSetup.CreateNavButtonVisual(canvasGO.transform, "FishDexNavButton", "Rehber",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-280f, 90f), new Vector2(260f, 90f));

            var navButton = buttonGO.AddComponent<SceneLoadButton>();
            var navSo = new SerializedObject(navButton);
            navSo.FindProperty("sceneName").stringValue = "FishDex";
            navSo.ApplyModifiedPropertiesWithoutUndo();

            var activeScene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
        }

        private static void BuildEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
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
    }
}
