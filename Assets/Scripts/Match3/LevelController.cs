using System;
using UnityEngine;

namespace FishMatch.Match3
{
    public class LevelController : MonoBehaviour
    {
        private const int PointsPerGem = 10;

        [SerializeField] private LevelDefinition levelDefinition;
        [SerializeField] private Match3GridController gridController;

        public event Action<int, int> OnMovesChanged;
        public event Action<int, int> OnScoreChanged;
        public event Action OnLevelWon;
        public event Action OnLevelLost;

        public int MovesRemaining { get; private set; }
        public int CurrentScore { get; private set; }
        public int CurrentLevelNumber => LevelProgressionState.CurrentLevelNumber;

        private int effectiveTargetScore;
        private bool levelEnded;

        private void Start()
        {
            MovesRemaining = levelDefinition.moveLimit;
            effectiveTargetScore = levelDefinition.targetScore
                + (CurrentLevelNumber - 1) * levelDefinition.targetScoreIncreasePerLevel;
            CurrentScore = 0;
            levelEnded = false;

            gridController.OnValidSwapPerformed += HandleValidSwap;
            gridController.OnGemsCleared += HandleGemsCleared;

            OnMovesChanged?.Invoke(MovesRemaining, levelDefinition.moveLimit);
            OnScoreChanged?.Invoke(CurrentScore, effectiveTargetScore);
        }

        private void OnDestroy()
        {
            gridController.OnValidSwapPerformed -= HandleValidSwap;
            gridController.OnGemsCleared -= HandleGemsCleared;
        }

        private void HandleValidSwap()
        {
            if (levelEnded)
            {
                return;
            }

            MovesRemaining = Mathf.Max(0, MovesRemaining - 1);
            OnMovesChanged?.Invoke(MovesRemaining, levelDefinition.moveLimit);
        }

        private void HandleGemsCleared(int count)
        {
            if (levelEnded)
            {
                return;
            }

            CurrentScore += count * PointsPerGem;
            OnScoreChanged?.Invoke(CurrentScore, effectiveTargetScore);

            CheckForLevelEnd();
        }

        // Win takes priority: a move that both exhausts the limit and reaches the target still counts as a win.
        private void CheckForLevelEnd()
        {
            if (levelEnded)
            {
                return;
            }

            if (CurrentScore >= effectiveTargetScore)
            {
                levelEnded = true;
                gridController.InputEnabled = false;
                LevelProgressionState.AdvanceToNextLevel();
                OnLevelWon?.Invoke();
            }
            else if (MovesRemaining <= 0)
            {
                levelEnded = true;
                gridController.InputEnabled = false;
                OnLevelLost?.Invoke();
            }
        }
    }
}
