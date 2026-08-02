using FishMatch.Match3;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace FishMatch.EditorTools
{
    // Builds the LevelMap scene (scrollable, zigzag path of level nodes - Candy Crush style level
    // select). The nodes themselves are generated at runtime by LevelMapController since their
    // locked/current/completed state depends on save data; this script only wires the static scene
    // shell (camera, canvas, scroll view, nav button). Safe to re-run - rebuilds from scratch. Run via:
    // Unity.exe -batchmode -projectPath <path> -executeMethod FishMatch.EditorTools.LevelMapSetup.Run -quit
    public static class LevelMapSetup
    {
        private const string ScenePath = "Assets/Scenes/LevelMap.unity";

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

            LevelSetup.CreateText(canvasGO.transform, "TitleText", "Seviyeler",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(600f, 100f));

            var contentRect = BuildScrollView(canvasGO.transform);

            var mapControllerGO = new GameObject("LevelMapController");
            var mapController = mapControllerGO.AddComponent<LevelMapController>();
            var mcSo = new SerializedObject(mapController);
            mcSo.FindProperty("content").objectReferenceValue = contentRect;
            mcSo.ApplyModifiedPropertiesWithoutUndo();

            var menuButtonGO = AquariumSetup.CreateNavButtonVisual(canvasGO.transform, "MainMenuButton", "Ana Menü",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 90f), new Vector2(320f, 90f));
            var menuNavButton = menuButtonGO.AddComponent<SceneLoadButton>();
            var menuNavSo = new SerializedObject(menuNavButton);
            menuNavSo.FindProperty("sceneName").stringValue = "MainMenu";
            menuNavSo.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);

            BuildSettingsUtil.EnsureSceneInBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("LevelMapSetup: LevelMap scene created (scrollable level path, nodes generated at runtime).");
        }

        private static RectTransform BuildScrollView(Transform canvasParent)
        {
            var scrollGO = new GameObject("ScrollView");
            scrollGO.transform.SetParent(canvasParent, false);
            var scrollRect = scrollGO.AddComponent<ScrollRect>();

            var scrollRT = scrollGO.GetComponent<RectTransform>();
            scrollRT.anchorMin = Vector2.zero;
            scrollRT.anchorMax = Vector2.one;
            scrollRT.offsetMin = new Vector2(0f, 160f); // leave room for the "Ana Menü" footer button
            scrollRT.offsetMax = new Vector2(0f, -180f); // leave room for the title

            var viewportGO = new GameObject("Viewport");
            viewportGO.transform.SetParent(scrollGO.transform, false);
            var viewportRT = viewportGO.AddComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.offsetMin = Vector2.zero;
            viewportRT.offsetMax = Vector2.zero;
            viewportGO.AddComponent<RectMask2D>();

            var contentGO = new GameObject("Content");
            contentGO.transform.SetParent(viewportGO.transform, false);
            var contentRT = contentGO.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0.5f, 1f);
            contentRT.anchorMax = new Vector2(0.5f, 1f);
            contentRT.pivot = new Vector2(0.5f, 1f);
            contentRT.anchoredPosition = Vector2.zero;
            contentRT.sizeDelta = new Vector2(1080f, 2000f); // resized by LevelMapController at runtime

            scrollRect.viewport = viewportRT;
            scrollRect.content = contentRT;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            return contentRT;
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
