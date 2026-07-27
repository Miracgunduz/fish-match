using System.IO;
using FishMatch.Match3;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FishMatch.EditorTools
{
    // Adds the puzzle-piece fish-unlock loop (a small starter roster) to the existing Match3Test scene.
    // Safe to re-run - removes its own previous output first. Run via:
    // Unity.exe -batchmode -projectPath <path> -executeMethod FishMatch.EditorTools.FishCollectionSetup.Run -quit
    public static class FishCollectionSetup
    {
        private const string ScenePath = "Assets/Scenes/Match3Test.unity";
        private const string FishFolder = "Assets/ScriptableObjects/Fish";

        // Exposed so FishDexSetup can build the same roster without re-deriving these paths.
        public static readonly string[] FishRosterAssetPaths =
        {
            FishFolder + "/Fish_JaponBaligi.asset",
            FishFolder + "/Fish_MelekBaligi.asset",
            FishFolder + "/Fish_EjderhaBaligi.asset",
        };

        public static void Run()
        {
            Directory.CreateDirectory(FishFolder);

            // Create the assets BEFORE opening the scene, then reload them by path afterward -
            // OpenScene invalidates Object references fetched before it runs (see LevelSetup).
            CreateFishRoster();

            EditorSceneManager.OpenScene(ScenePath);

            RemoveExisting("FishCollectionManager");

            var fishRoster = new FishSpeciesDefinition[FishRosterAssetPaths.Length];
            for (int i = 0; i < FishRosterAssetPaths.Length; i++)
            {
                fishRoster[i] = AssetDatabase.LoadAssetAtPath<FishSpeciesDefinition>(FishRosterAssetPaths[i]);
            }

            var levelController = GameObject.Find("LevelManager").GetComponent<LevelController>();

            var collectionGO = new GameObject("FishCollectionManager");
            var collectionManager = collectionGO.AddComponent<FishCollectionManager>();
            var cmSo = new SerializedObject(collectionManager);
            cmSo.FindProperty("levelController").objectReferenceValue = levelController;

            // Resize then commit before writing elements - see the note in LevelSetup.CreateTestScene.
            var rosterProp = cmSo.FindProperty("fishRoster");
            rosterProp.arraySize = fishRoster.Length;
            cmSo.ApplyModifiedPropertiesWithoutUndo();

            cmSo.Update();
            rosterProp = cmSo.FindProperty("fishRoster");
            for (int i = 0; i < fishRoster.Length; i++)
            {
                rosterProp.GetArrayElementAtIndex(i).objectReferenceValue = fishRoster[i];
            }
            cmSo.ApplyModifiedPropertiesWithoutUndo();

            // ResultPanel is inactive by default, and GameObject.Find only searches active objects -
            // find it via its (active) Canvas parent's Transform.Find instead, which does see inactive children.
            var resultPanelTransform = GameObject.Find("Canvas").transform.Find("ResultPanel");
            var pieceProgressText = LevelSetup.CreateText(resultPanelTransform, "PieceProgressText", "",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(560f, 70f));

            var hudGO = GameObject.Find("HUD");
            var fishHud = hudGO.AddComponent<FishProgressHUDController>();
            var fishHudSo = new SerializedObject(fishHud);
            fishHudSo.FindProperty("collectionManager").objectReferenceValue = collectionManager;
            fishHudSo.FindProperty("pieceProgressText").objectReferenceValue = pieceProgressText;
            fishHudSo.ApplyModifiedPropertiesWithoutUndo();

            var activeScene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
            AssetDatabase.SaveAssets();
            Debug.Log("FishCollectionSetup: FishCollectionManager (3-species roster) + piece-progress HUD wired into Match3Test scene.");
        }

        private static void RemoveExisting(string name)
        {
            var go = GameObject.Find(name);
            if (go != null)
            {
                Object.DestroyImmediate(go);
            }
        }

        private static void CreateFishRoster()
        {
            CreateFishSpecies(FishRosterAssetPaths[0], "JaponBaligi", "Japon Balığı", FishRarity.Common, 8,
                "Japon balıkları uygun koşullarda 20 yıldan fazla yaşayabilir!");
            CreateFishSpecies(FishRosterAssetPaths[1], "MelekBaligi", "Melek Balığı", FishRarity.Rare, 16,
                "Melek balıkları çift eşleşir ve ömür boyu aynı eşle kalabilir!");
            CreateFishSpecies(FishRosterAssetPaths[2], "EjderhaBaligi", "Ejderha Balığı", FishRarity.Legendary, 30,
                "Ejderha balığı adını, yüzgeçlerinin efsanevi ejderhaları andırmasından alır!");
        }

        private static void CreateFishSpecies(string path, string speciesId, string displayName, FishRarity rarity, int totalPieces, string funFact)
        {
            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
            }

            var fish = ScriptableObject.CreateInstance<FishSpeciesDefinition>();
            fish.speciesId = speciesId;
            fish.displayName = displayName;
            fish.rarity = rarity;
            fish.totalPieces = totalPieces;
            fish.funFact = funFact;
            AssetDatabase.CreateAsset(fish, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
