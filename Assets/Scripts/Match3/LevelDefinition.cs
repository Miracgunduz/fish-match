using UnityEngine;

namespace FishMatch.Match3
{
    [CreateAssetMenu(fileName = "Level_", menuName = "FishMatch/Level Definition")]
    public class LevelDefinition : ScriptableObject
    {
        public int moveLimit = 20;
        public int targetScore = 500;
        public int targetScoreIncreasePerLevel = 100;
    }
}
