using UnityEngine;

namespace FishMatch.Match3
{
    // Static so level state survives the scene reload used when starting/advancing a level.
    // SaveManager reads/restores HighestUnlockedLevel across app sessions; SelectedLevelNumber only
    // needs to survive within a session (it's set right before loading Match3Test, from the level
    // map or the win/lose panel's "next level" flow).
    public static class LevelProgressionState
    {
        // The furthest level the player has unlocked by winning - drives the level map's lock state.
        public static int HighestUnlockedLevel { get; private set; } = 1;

        // The level Match3Test will actually load and play - lets the player replay an earlier,
        // already-completed level without disturbing HighestUnlockedLevel.
        public static int SelectedLevelNumber { get; private set; } = 1;

        // See CurrencyManager.Reset for why this is attributed.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            HighestUnlockedLevel = 1;
            SelectedLevelNumber = 1;
        }

        public static void SelectLevel(int levelNumber)
        {
            SelectedLevelNumber = levelNumber;
        }

        // Only advances the frontier if the completed level WAS the frontier - replaying an
        // already-cleared level must never move HighestUnlockedLevel backward or skip it forward.
        public static void NotifyLevelCompleted(int levelNumber)
        {
            if (levelNumber == HighestUnlockedLevel)
            {
                HighestUnlockedLevel++;
            }
        }

        // For SaveManager to restore a loaded value directly.
        public static void SetHighestUnlockedLevel(int levelNumber)
        {
            HighestUnlockedLevel = levelNumber;
        }
    }
}
