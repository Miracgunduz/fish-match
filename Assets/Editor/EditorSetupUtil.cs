using System;
using UnityEditor;
using UnityEngine;

namespace FishMatch.EditorTools
{
    internal static class EditorSetupUtil
    {
        // Turns a confusing NullReferenceException deep in a setup script into an actionable message
        // pointing at which prerequisite script needs to run first - these scripts build on each
        // other's scene output in a fixed order (see ProjectSetupRunner).
        public static GameObject RequireGameObject(string name, string prerequisiteScriptHint)
        {
            var go = GameObject.Find(name);
            if (go == null)
            {
                throw new InvalidOperationException(
                    $"Could not find GameObject '{name}' in the active scene - run {prerequisiteScriptHint} first.");
            }

            return go;
        }

        // Shared rounded-rect UI sprite baked by UIShapeSetup - white and Image.color-tintable, so it's
        // kept for the one place that still needs dynamic per-instance tinting: LevelMapController's
        // node state colors (locked/current/completed/milestone), generated at runtime, not by an
        // editor script. Must run before any script calling this.
        public static Sprite LoadUIRoundedRectSprite()
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UIShapeSetup.RoundedRectSpritePath);
            if (sprite == null)
            {
                throw new InvalidOperationException("Could not load the shared rounded-rect UI sprite - run UIShapeSetup first.");
            }

            return sprite;
        }

        // Real Kenney UI-pack button art (see KenneyImportSetup) - used at full color, not tinted, for
        // every fixed-color button helper (nav buttons, action buttons). Must run after KenneyImportSetup.
        public static Sprite LoadKenneyButtonSprite(string colorName)
        {
            var path = $"Assets/Art/Kenney/UI/Button_{colorName}.png";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                throw new InvalidOperationException($"Could not load Kenney button sprite '{path}' - run KenneyImportSetup first.");
            }

            return sprite;
        }
    }
}
