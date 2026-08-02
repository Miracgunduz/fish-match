using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FishMatch.EditorTools
{
    // Applies Sprite import settings to the free CC0 Kenney.nl art (github.com/KenneyNL, kenney.nl/assets
    // "UI Pack", "Fish Pack", "Physics Assets" - see Assets/Art/Kenney/CREDITS.txt) that replaced this
    // project's procedural flat-shape placeholders for fish/decorations/buttons/coin. The PNGs themselves
    // are committed as-is; this only sets how Unity imports them. Safe to re-run. Must run before any
    // script that loads sprites from Assets/Art/Kenney/. Run via:
    // Unity.exe -batchmode -projectPath <path> -executeMethod FishMatch.EditorTools.KenneyImportSetup.Run -quit
    public static class KenneyImportSetup
    {
        private const string RootFolder = "Assets/Art/Kenney";

        // Kenney's rectangle button PNGs (192x64) have a small ~4-6px corner radius - a modest uniform
        // border keeps Image.Type.Sliced from stretching that corner at any button size used in this
        // project, with no risk of visible artifacts even if the true radius is a couple pixels off.
        private const float ButtonSpriteBorder = 8f;

        private static readonly HashSet<string> SlicedButtonNames = new HashSet<string>
        {
            "Button_Blue", "Button_Green",
        };

        public static void Run()
        {
            if (!Directory.Exists(RootFolder))
            {
                Debug.LogWarning("KenneyImportSetup: Assets/Art/Kenney not found - nothing to import.");
                return;
            }

            var paths = Directory.GetFiles(RootFolder, "*.png", SearchOption.AllDirectories);
            foreach (var path in paths)
            {
                ApplySpriteImportSettings(path.Replace('\\', '/'));
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"KenneyImportSetup: applied sprite import settings to {paths.Length} Kenney asset(s).");
        }

        private static void ApplySpriteImportSettings(string path)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null)
            {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;

            // 1 Unity unit = the sprite's full native width, matching this project's existing convention
            // for procedurally-baked shapes - keeps differently-sized source PNGs at a consistent
            // in-game footprint instead of each rendering at its own raw pixel size.
            var width = GetPngWidth(path);
            importer.spritePixelsPerUnit = width > 0 ? width : 100f;

            var fileName = Path.GetFileNameWithoutExtension(path);
            if (SlicedButtonNames.Contains(fileName))
            {
                importer.spriteBorder = new Vector4(ButtonSpriteBorder, ButtonSpriteBorder, ButtonSpriteBorder, ButtonSpriteBorder);
            }

            importer.SaveAndReimport();
        }

        // Reads width/height straight from the PNG header (bytes 16-23 of any valid PNG) - avoids
        // needing a readable Texture2D just to ask its size.
        private static int GetPngWidth(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var reader = new BinaryReader(stream))
            {
                if (stream.Length < 24)
                {
                    return 0;
                }

                stream.Seek(16, SeekOrigin.Begin);
                var widthBytes = reader.ReadBytes(4);
                if (System.BitConverter.IsLittleEndian)
                {
                    System.Array.Reverse(widthBytes);
                }

                return System.BitConverter.ToInt32(widthBytes, 0);
            }
        }
    }
}
