using System.IO;
using FishMatch.Match3;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace FishMatch.EditorTools
{
    // Adds move-limit/score level logic + a basic HUD to the existing Match3Test scene.
    // Safe to re-run - removes its own previous output first. Run via:
    // Unity.exe -batchmode -projectPath <path> -executeMethod FishMatch.EditorTools.LevelSetup.Run -quit
    public static class LevelSetup
    {
        private const string ScenePath = "Assets/Scenes/Match3Test.unity";
        private const string LevelFolder = "Assets/ScriptableObjects/Levels";
        private const string LevelAssetPath = LevelFolder + "/Level_01.asset";

        public static void Run()
        {
            Directory.CreateDirectory(LevelFolder);

            // Create the level asset BEFORE opening the scene, then reload it by path afterward -
            // OpenScene (like NewScene) invalidates Object references fetched before it runs.
            var levelDefinitionPath = CreateLevelDefinition();

            EditorSceneManager.OpenScene(ScenePath);

            RemoveExisting("LevelManager");
            RemoveExisting("Canvas");
            RemoveExisting("EventSystem");

            var levelDefinition = AssetDatabase.LoadAssetAtPath<LevelDefinition>(levelDefinitionPath);
            var gridController = EditorSetupUtil.RequireGameObject("Match3Grid", "FishMatchSetup").GetComponent<Match3GridController>();

            var levelManagerGO = new GameObject("LevelManager");
            var levelController = levelManagerGO.AddComponent<LevelController>();
            var lcSo = new SerializedObject(levelController);
            lcSo.FindProperty("levelDefinition").objectReferenceValue = levelDefinition;
            lcSo.FindProperty("gridController").objectReferenceValue = gridController;
            lcSo.ApplyModifiedPropertiesWithoutUndo();

            BuildEventSystem();
            var canvasGO = BuildCanvas();

            var levelText = CreateText(canvasGO.transform, "LevelText", "Seviye 1",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(320f, 70f));
            var movesText = CreateText(canvasGO.transform, "MovesText", "Hamle: 0/0",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -30f), new Vector2(320f, 70f));
            var scoreText = CreateText(canvasGO.transform, "ScoreText", "Puan: 0/0",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30f, -110f), new Vector2(320f, 70f));

            var resultPanel = CreatePanel("ResultPanel", canvasGO.transform);
            var resultText = CreateText(resultPanel.transform, "ResultText", "",
                new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600f, 120f));
            var restartButton = CreateButton(resultPanel.transform, "RestartButton", "Tekrar Dene",
                new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(320f, 100f));

            var menuButtonGO = AquariumSetup.CreateNavButtonVisual(resultPanel.transform, "MainMenuButton", "Ana Menü",
                new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.5f), new Vector2(0f, -130f), new Vector2(280f, 80f));
            var menuNavButton = menuButtonGO.AddComponent<SceneLoadButton>();
            var menuNavSo = new SerializedObject(menuNavButton);
            menuNavSo.FindProperty("sceneName").stringValue = "MainMenu";
            menuNavSo.ApplyModifiedPropertiesWithoutUndo();

            BuildPauseUI(canvasGO.transform, gridController);

            var hudGO = new GameObject("HUD");
            hudGO.transform.SetParent(canvasGO.transform, false);
            var hud = hudGO.AddComponent<LevelHUDController>();
            var hudSo = new SerializedObject(hud);
            hudSo.FindProperty("levelController").objectReferenceValue = levelController;
            hudSo.FindProperty("levelText").objectReferenceValue = levelText;
            hudSo.FindProperty("movesText").objectReferenceValue = movesText;
            hudSo.FindProperty("scoreText").objectReferenceValue = scoreText;
            hudSo.FindProperty("resultPanel").objectReferenceValue = resultPanel;
            hudSo.FindProperty("resultText").objectReferenceValue = resultText;
            hudSo.FindProperty("restartButton").objectReferenceValue = restartButton;
            hudSo.ApplyModifiedPropertiesWithoutUndo();

            // LevelHUDController.Awake() also hides this at runtime, but that only runs in Play mode -
            // hide it here too so the scene doesn't look broken when just opened in the Editor.
            resultPanel.SetActive(false);

            var activeScene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
            AssetDatabase.SaveAssets();
            Debug.Log("LevelSetup: LevelController + HUD wired into Match3Test scene.");
        }

        private static void RemoveExisting(string name)
        {
            var go = GameObject.Find(name);
            if (go != null)
            {
                Object.DestroyImmediate(go);
            }
        }

        private static string CreateLevelDefinition()
        {
            if (File.Exists(LevelAssetPath))
            {
                AssetDatabase.DeleteAsset(LevelAssetPath);
            }

            var level = ScriptableObject.CreateInstance<LevelDefinition>();
            level.moveLimit = 20;
            level.targetScore = 500;
            AssetDatabase.CreateAsset(level, LevelAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return LevelAssetPath;
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

        // Internal (not private) so other one-off setup scripts under Assets/Editor can reuse it.
        internal static Text CreateText(Transform parent, string name, string content, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var text = go.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 36;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            return text;
        }

        private static void BuildPauseUI(Transform canvasParent, Match3GridController gridController)
        {
            var pauseButtonGO = AquariumSetup.CreateNavButtonVisual(canvasParent, "PauseButton", "II",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30f, -30f), new Vector2(90f, 70f));

            var pausePanel = CreatePanel("PausePanel", canvasParent);
            CreateText(pausePanel.transform, "PauseTitleText", "Duraklatıldı",
                new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600f, 120f));

            var resumeButton = CreateButton(pausePanel.transform, "ResumeButton", "Devam Et",
                new Vector2(0.5f, 0.58f), new Vector2(0.5f, 0.58f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(320f, 100f));
            var restartButton = CreateButton(pausePanel.transform, "PauseRestartButton", "Tekrar Başlat",
                new Vector2(0.5f, 0.46f), new Vector2(0.5f, 0.46f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(320f, 100f));

            var aquariumButtonGO = AquariumSetup.CreateNavButtonVisual(pausePanel.transform, "PauseAquariumButton", "Akvaryum",
                new Vector2(0.5f, 0.34f), new Vector2(0.5f, 0.34f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(320f, 90f));
            var aquariumNavButton = aquariumButtonGO.AddComponent<SceneLoadButton>();
            var aquariumNavSo = new SerializedObject(aquariumNavButton);
            aquariumNavSo.FindProperty("sceneName").stringValue = "Aquarium";
            aquariumNavSo.ApplyModifiedPropertiesWithoutUndo();

            var mainMenuButtonGO = AquariumSetup.CreateNavButtonVisual(pausePanel.transform, "PauseMainMenuButton", "Ana Menü",
                new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(320f, 90f));
            var mainMenuNavButton = mainMenuButtonGO.AddComponent<SceneLoadButton>();
            var mainMenuNavSo = new SerializedObject(mainMenuNavButton);
            mainMenuNavSo.FindProperty("sceneName").stringValue = "MainMenu";
            mainMenuNavSo.ApplyModifiedPropertiesWithoutUndo();

            var pauseControllerGO = new GameObject("PauseController");
            pauseControllerGO.transform.SetParent(canvasParent, false);
            var pauseController = pauseControllerGO.AddComponent<PauseController>();
            var pcSo = new SerializedObject(pauseController);
            pcSo.FindProperty("gridController").objectReferenceValue = gridController;
            pcSo.FindProperty("pausePanel").objectReferenceValue = pausePanel;
            pcSo.FindProperty("pauseButton").objectReferenceValue = pauseButtonGO.GetComponent<Button>();
            pcSo.FindProperty("resumeButton").objectReferenceValue = resumeButton;
            pcSo.FindProperty("restartButton").objectReferenceValue = restartButton;
            pcSo.ApplyModifiedPropertiesWithoutUndo();

            // PauseController.Awake() also hides this at runtime, but that only runs in Play mode -
            // hide it here too so the scene doesn't look broken when just opened in the Editor.
            pausePanel.SetActive(false);
        }

        private static GameObject CreatePanel(string name, Transform parent)
        {
            var go = new GameObject(name);
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

        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var image = go.AddComponent<Image>();
            image.sprite = EditorSetupUtil.LoadKenneyButtonSprite("Green");
            image.type = Image.Type.Sliced;
            image.color = Color.white;

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            var button = go.AddComponent<Button>();

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(go.transform, false);
            var text = textGO.AddComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 28;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;

            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return button;
        }
    }
}
