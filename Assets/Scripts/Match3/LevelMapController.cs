using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FishMatch.Match3
{
    // Populates the scrollable level path at runtime (not editor-baked like the other scenes' setup
    // scripts) because node state - locked/current/completed/milestone - depends on save data that
    // only exists once the game is actually running.
    public class LevelMapController : MonoBehaviour
    {
        [SerializeField] private RectTransform content;
        [SerializeField] private int totalLevels = 30;

        private const float RowSpacing = 190f;
        private const float ZigzagAmplitude = 190f;
        private const float TopPadding = 170f;
        private const float NormalNodeSize = 110f;
        private const float MilestoneNodeSize = 150f;

        private static readonly Color CompletedColor = new Color(0.25f, 0.65f, 0.35f);
        private static readonly Color CurrentColor = new Color(0.85f, 0.7f, 0.2f);
        private static readonly Color LockedColor = new Color(0.35f, 0.35f, 0.4f);
        private static readonly Color LevelUpColor = new Color(0.3f, 0.55f, 0.8f);
        private static readonly Color ChallengeColor = new Color(0.75f, 0.25f, 0.2f);
        private static readonly Color ChapterUnlockColor = new Color(0.75f, 0.55f, 0.15f);

        private Sprite nodeSprite;

        private void Start()
        {
            nodeSprite = CreateRoundedRectSprite();
            content.sizeDelta = new Vector2(content.sizeDelta.x, TopPadding + totalLevels * RowSpacing);

            BuildChapterBands();

            for (int i = 0; i < totalLevels; i++)
            {
                CreateLevelNode(i + 1, i);
            }
        }

        private void BuildChapterBands()
        {
            int chapterCount = Mathf.CeilToInt((float)totalLevels / LevelChapters.ChapterLength);
            float cumulativeOffset = 0f;

            for (int c = 0; c < chapterCount; c++)
            {
                // The last chapter can be partial if totalLevels isn't an exact multiple of
                // ChapterLength - size its band to the levels it actually contains, not a fixed height,
                // so the band never extends past the last real node.
                int levelsInThisChapter = Mathf.Min(LevelChapters.ChapterLength, totalLevels - c * LevelChapters.ChapterLength);
                float bandHeight = levelsInThisChapter * RowSpacing;

                var bandGO = new GameObject($"ChapterBand_{c}");
                bandGO.transform.SetParent(content, false);

                var image = bandGO.AddComponent<Image>();
                var themeColor = LevelChapters.ChapterThemeColor(c);
                image.color = new Color(themeColor.r, themeColor.g, themeColor.b, 0.35f);

                var rect = bandGO.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 1f);
                rect.anchorMax = new Vector2(0.5f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(0f, -(TopPadding - RowSpacing * 0.5f + cumulativeOffset));
                rect.sizeDelta = new Vector2(1000f, bandHeight);

                // -50: just below the band's own top edge (this rect's local space is also top-anchored).
                CreateFloatingLabel(bandGO.transform, LevelChapters.ChapterName(c), 0f, -50f, 36, 700f);

                cumulativeOffset += bandHeight;
            }
        }

        private void CreateLevelNode(int levelNumber, int rowIndex)
        {
            bool isCompleted = levelNumber < LevelProgressionState.HighestUnlockedLevel;
            bool isCurrent = levelNumber == LevelProgressionState.HighestUnlockedLevel;
            bool isUnlocked = levelNumber <= LevelProgressionState.HighestUnlockedLevel;
            var milestone = LevelChapters.GetMilestone(levelNumber);
            float size = milestone == LevelChapters.MilestoneType.None ? NormalNodeSize : MilestoneNodeSize;
            float xOffset = Mathf.Sin(rowIndex * 0.9f) * ZigzagAmplitude;
            float yPos = -TopPadding - rowIndex * RowSpacing;

            var go = new GameObject($"LevelNode_{levelNumber}");
            go.transform.SetParent(content, false);

            var image = go.AddComponent<Image>();
            image.sprite = nodeSprite;
            image.type = Image.Type.Sliced;
            image.color = NodeColor(isCompleted, isCurrent, milestone);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(xOffset, yPos);
            rect.sizeDelta = new Vector2(size, size);

            var button = go.AddComponent<Button>();
            button.interactable = isUnlocked;
            if (isUnlocked)
            {
                button.onClick.AddListener(() =>
                {
                    LevelProgressionState.SelectLevel(levelNumber);
                    SceneManager.LoadScene("Match3Test");
                });
            }

            CreateCenteredLabel(go.transform, levelNumber.ToString(), 38);

            if (milestone != LevelChapters.MilestoneType.None)
            {
                CreateFloatingLabel(content, MilestoneCaption(levelNumber, milestone), xOffset, yPos - size * 0.5f - 30f, 20, 260f);
            }

            if (isCurrent)
            {
                CreateFloatingLabel(content, "▼ Buradasın", xOffset, yPos + size * 0.5f + 30f, 22, 260f);
            }
        }

        // Generated once per scene load (not baked at edit-time like the shared editor sprite in
        // UIShapeSetup) since this controller only ever runs at Play-time - no need for it to persist
        // as a project asset.
        private static Sprite CreateRoundedRectSprite()
        {
            const int size = 64;
            const float cornerRadius = 16f;

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    pixels[y * size + x] = IsInsideRoundedRect(x, y, size, cornerRadius) ? Color.white : new Color(0f, 0f, 0f, 0f);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            var border = cornerRadius + 2f;
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size, 0,
                SpriteMeshType.FullRect, new Vector4(border, border, border, border));
        }

        private static bool IsInsideRoundedRect(int x, int y, int size, float cornerRadius)
        {
            float px = x + 0.5f;
            float py = y + 0.5f;
            float minX = cornerRadius;
            float minY = cornerRadius;
            float maxX = size - cornerRadius;
            float maxY = size - cornerRadius;

            float dx = px < minX ? minX - px : (px > maxX ? px - maxX : 0f);
            float dy = py < minY ? minY - py : (py > maxY ? py - maxY : 0f);

            return (dx * dx + dy * dy) <= cornerRadius * cornerRadius;
        }

        private static Color NodeColor(bool isCompleted, bool isCurrent, LevelChapters.MilestoneType milestone)
        {
            if (!isCompleted && !isCurrent)
            {
                return LockedColor;
            }

            return milestone switch
            {
                LevelChapters.MilestoneType.ChapterUnlock => ChapterUnlockColor,
                LevelChapters.MilestoneType.Challenge => ChallengeColor,
                LevelChapters.MilestoneType.LevelUp => LevelUpColor,
                _ => isCompleted ? CompletedColor : CurrentColor,
            };
        }

        private static string MilestoneCaption(int levelNumber, LevelChapters.MilestoneType milestone)
        {
            switch (milestone)
            {
                case LevelChapters.MilestoneType.LevelUp:
                    return "Seviye Atla!";
                case LevelChapters.MilestoneType.Challenge:
                    return "Meydan Okuma";
                case LevelChapters.MilestoneType.ChapterUnlock:
                    return LevelChapters.ChapterName(LevelChapters.ChapterIndexForLevel(levelNumber) + 1);
                default:
                    return string.Empty;
            }
        }

        // Fills its parent entirely (stretch + center) - used for the number inside a node, regardless
        // of that node's size.
        private static void CreateCenteredLabel(Transform parent, string content, int fontSize)
        {
            var textGO = new GameObject("Label");
            textGO.transform.SetParent(parent, false);

            var text = textGO.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;

            var rect = textGO.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        // Explicitly positioned in the given parent's own top-anchored local space (anchor at top-
        // center, Y measured downward as negative) - used for captions/titles/markers that aren't
        // meant to fill a parent rect.
        private static void CreateFloatingLabel(Transform parent, string content, float x, float y, int fontSize, float width)
        {
            var textGO = new GameObject("Label");
            textGO.transform.SetParent(parent, false);

            var text = textGO.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;

            var rect = textGO.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, 70f);
        }
    }
}
