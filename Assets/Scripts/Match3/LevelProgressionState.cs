namespace FishMatch.Match3
{
    // Static so the current level number survives the scene reload used to advance to the next level.
    // SaveManager reads/restores this across app sessions.
    public static class LevelProgressionState
    {
        public static int CurrentLevelNumber { get; private set; } = 1;

        public static void AdvanceToNextLevel()
        {
            CurrentLevelNumber++;
        }

        // For SaveManager to restore a loaded value directly.
        public static void SetCurrentLevelNumber(int levelNumber)
        {
            CurrentLevelNumber = levelNumber;
        }
    }
}
