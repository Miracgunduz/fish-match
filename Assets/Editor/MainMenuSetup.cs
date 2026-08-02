using System.Collections.Generic;
using FishMatch.Match3;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace FishMatch.EditorTools
{
    // Builds the MainMenu scene (title + Oyna/Akvaryum/Rehber buttons) as the game's true entry
    // point, and makes it build index 0. Safe to re-run - rebuilds the scene from scratch. Run via:
    // Unity.exe -batchmode -projectPath <path> -executeMethod FishMatch.EditorTools.MainMenuSetup.Run -quit
    public static class MainMenuSetup
    {
        private const string ScenePath = "Assets/Scenes/MainMenu.unity";

        public static void Run()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.06f, 0.09f, 0.14f);

            var cameraGO = new GameObject("Main Camera");
            cameraGO.tag = "MainCamera";
            var cam = cameraGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.09f, 0.14f);
            cameraGO.transform.position = new Vector3(0f, 0f, -10f);

            BuildEventSystem();
            var canvasGO = BuildCanvas();

            var titleText = LevelSetup.CreateText(canvasGO.transform, "TitleText", "Fish Match",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -220f), new Vector2(800f, 160f));
            titleText.fontSize = 90;

            var playButton = AquariumSetup.CreateNavButtonVisual(canvasGO.transform, "PlayButton", "Oyna",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 130f), new Vector2(300f, 90f));
            WireNavButton(playButton, "LevelMap");

            var aquariumButton = AquariumSetup.CreateNavButtonVisual(canvasGO.transform, "AquariumButton", "Akvaryum",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(300f, 90f));
            WireNavButton(aquariumButton, "Aquarium");

            var fishDexButton = AquariumSetup.CreateNavButtonVisual(canvasGO.transform, "FishDexButton", "Rehber",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -130f), new Vector2(300f, 90f));
            WireNavButton(fishDexButton, "FishDex");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);

            PrependSceneToBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log("MainMenuSetup: MainMenu scene created and set as build index 0.");
        }

        private static void WireNavButton(GameObject buttonGO, string sceneName)
        {
            var navButton = buttonGO.AddComponent<SceneLoadButton>();
            var navSo = new SerializedObject(navButton);
            navSo.FindProperty("sceneName").stringValue = sceneName;
            navSo.ApplyModifiedPropertiesWithoutUndo();
        }

        // MainMenu must be build index 0 - unlike other scenes, which just append themselves.
        private static void PrependSceneToBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.RemoveAll(s => s.path == ScenePath);
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
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
