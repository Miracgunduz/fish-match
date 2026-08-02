using UnityEngine;

namespace FishMatch.Match3
{
    // Pure data/logic describing how levels group into narrative chapters and which specific levels
    // carry special milestone behavior (a "level up" flourish, a harder Challenge variant, or a
    // chapter-end theme unlock). Shared by LevelMapController (node visuals), LevelController
    // (challenge difficulty), and AquariumManager (theme color) so all three stay in sync from one
    // source of truth instead of each hardcoding level numbers separately.
    public static class LevelChapters
    {
        public const int ChapterLength = 10;
        private const int MidChapterPosition = ChapterLength / 2 + 1;

        private static readonly string[] ChapterNames =
        {
            "Sığ Sular", "Atlantis Harabeleri", "Mercan Cenneti"
        };

        private static readonly Color[] ChapterColors =
        {
            new Color(0.10f, 0.35f, 0.55f),
            new Color(0.12f, 0.35f, 0.30f),
            new Color(0.55f, 0.25f, 0.35f),
        };

        public enum MilestoneType
        {
            None,
            LevelUp,
            Challenge,
            ChapterUnlock
        }

        public static int ChapterIndexForLevel(int levelNumber) => (levelNumber - 1) / ChapterLength;

        public static string ChapterName(int chapterIndex)
        {
            if (chapterIndex < 0)
            {
                chapterIndex = 0;
            }

            return chapterIndex < ChapterNames.Length ? ChapterNames[chapterIndex] : $"Bölge {chapterIndex + 1}";
        }

        public static string ChapterNameForLevel(int levelNumber) => ChapterName(ChapterIndexForLevel(levelNumber));

        public static Color ChapterThemeColor(int chapterIndex)
        {
            if (chapterIndex < 0)
            {
                chapterIndex = 0;
            }

            return chapterIndex < ChapterColors.Length ? ChapterColors[chapterIndex] : ChapterColors[ChapterColors.Length - 1];
        }

        // 1-based position within the level's chapter (1..ChapterLength).
        private static int PositionInChapter(int levelNumber) => ((levelNumber - 1) % ChapterLength) + 1;

        // The last level of a chapter unlocks the next chapter's aquarium theme.
        public static bool IsChapterUnlockLevel(int levelNumber) => PositionInChapter(levelNumber) == ChapterLength;

        // The chapter's single mid-point milestone alternates between a Challenge (harder variant)
        // and a cosmetic Level Up flourish so consecutive chapters don't feel identical.
        public static bool IsChallengeLevel(int levelNumber) =>
            PositionInChapter(levelNumber) == MidChapterPosition && ChapterIndexForLevel(levelNumber) % 2 == 1;

        public static bool IsLevelUpLevel(int levelNumber) =>
            PositionInChapter(levelNumber) == MidChapterPosition && ChapterIndexForLevel(levelNumber) % 2 == 0;

        public static MilestoneType GetMilestone(int levelNumber)
        {
            if (IsChapterUnlockLevel(levelNumber))
            {
                return MilestoneType.ChapterUnlock;
            }

            if (IsChallengeLevel(levelNumber))
            {
                return MilestoneType.Challenge;
            }

            if (IsLevelUpLevel(levelNumber))
            {
                return MilestoneType.LevelUp;
            }

            return MilestoneType.None;
        }

        // Fewer moves, higher target - a deliberately harder variant for each chapter's Challenge
        // milestone. Pure and testable on its own, separate from LevelController's MonoBehaviour glue.
        public static (int moveLimit, int targetScore) ApplyChallengeModifierIfNeeded(int levelNumber, int moveLimit, int targetScore)
        {
            if (GetMilestone(levelNumber) != MilestoneType.Challenge)
            {
                return (moveLimit, targetScore);
            }

            return (Mathf.RoundToInt(moveLimit * 0.7f), Mathf.RoundToInt(targetScore * 1.3f));
        }
    }
}
