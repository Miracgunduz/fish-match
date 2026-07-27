using System.Collections.Generic;
using UnityEditor;

namespace FishMatch.EditorTools
{
    // Adds a scene to Build Settings without touching any scenes already registered there -
    // several setup scripts each register their own scene, and can run in any order/re-run individually.
    internal static class BuildSettingsUtil
    {
        public static void EnsureSceneInBuildSettings(string scenePath)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (scenes.Exists(s => s.path == scenePath))
            {
                return;
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
