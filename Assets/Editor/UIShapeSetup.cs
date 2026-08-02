using System.IO;
using UnityEditor;
using UnityEngine;

namespace FishMatch.EditorTools
{
    // Bakes a single reusable white, 9-sliced rounded-rectangle sprite that button/panel visuals across
    // the project tint via Image.color - replaces the plain flat-rectangle default Image look with
    // something less generic, without any shading/gradient risk (flat fill only, same safe technique as
    // the original placeholder gem shapes). Safe to re-run - deletes its own previous output first.
    // Must run before any script that loads RoundedRectSpritePath. Run via:
    // Unity.exe -batchmode -projectPath <path> -executeMethod FishMatch.EditorTools.UIShapeSetup.Run -quit
    public static class UIShapeSetup
    {
        private const int TextureSize = 64;
        private const float CornerRadiusPixels = 16f;
        private const string ArtFolder = "Assets/Art/UI";
        public const string RoundedRectSpritePath = ArtFolder + "/Shape_RoundedRect.png";

        public static void Run()
        {
            Directory.CreateDirectory(ArtFolder);

            if (File.Exists(RoundedRectSpritePath))
            {
                AssetDatabase.DeleteAsset(RoundedRectSpritePath);
            }

            var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
            var pixels = new Color[TextureSize * TextureSize];
            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    pixels[y * TextureSize + x] = IsInsideRoundedRect(x, y) ? Color.white : new Color(0f, 0f, 0f, 0f);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(RoundedRectSpritePath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(RoundedRectSpritePath, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(RoundedRectSpritePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.spritePixelsPerUnit = TextureSize;

            // Border big enough that Image.Type.Sliced never stretches the rounded corners themselves,
            // regardless of how wide/short the final button is.
            var border = CornerRadiusPixels + 2f;
            importer.spriteBorder = new Vector4(border, border, border, border);
            importer.SaveAndReimport();

            AssetDatabase.SaveAssets();
            Debug.Log("UIShapeSetup: shared rounded-rect UI sprite baked.");
        }

        private static bool IsInsideRoundedRect(int x, int y)
        {
            float px = x + 0.5f;
            float py = y + 0.5f;
            float minX = CornerRadiusPixels;
            float minY = CornerRadiusPixels;
            float maxX = TextureSize - CornerRadiusPixels;
            float maxY = TextureSize - CornerRadiusPixels;

            float dx = px < minX ? minX - px : (px > maxX ? px - maxX : 0f);
            float dy = py < minY ? minY - py : (py > maxY ? py - maxY : 0f);

            return (dx * dx + dy * dy) <= CornerRadiusPixels * CornerRadiusPixels;
        }
    }
}
