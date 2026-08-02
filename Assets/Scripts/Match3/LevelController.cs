using System;
using UnityEngine;

namespace FishMatch.Match3
{
    public class LevelController : MonoBehaviour
    {
        [SerializeField] private LevelDefinition levelDefinition;
        [SerializeField] private Match3GridController gridController;

        public event Action<int, int> OnMovesChanged;
        public event Action<int, int> OnScoreChanged;
        public event Action OnLevelWon;
        public event Action OnLevelLost;

        public int MovesRemaining => rules.MovesRemaining;
        public int CurrentScore => rules.CurrentScore;
        public int CurrentLevelNumber => LevelProgressionState.SelectedLevelNumber;

        private LevelRules rules;
        private int moveLimit;
        private bool resultAnnounced;

        public bool IsChallengeLevel { get; private set; }

        private void Start()
        {
            moveLimit = levelDefinition.moveLimit;
            var targetScore = levelDefinition.targetScore
                + (CurrentLevelNumber - 1) * levelDefinition.targetScoreIncreasePerLevel;

            IsChallengeLevel = LevelChapters.GetMilestone(CurrentLevelNumber) == LevelChapters.MilestoneType.Challenge;
            (moveLimit, targetScore) = LevelChapters.ApplyChallengeModifierIfNeeded(CurrentLevelNumber, moveLimit, targetScore);

            rules = new LevelRules(moveLimit, targetScore);
            resultAnnounced = false;

            gridController.OnValidSwapPerformed += HandleValidSwap;
            gridController.OnGemsCleared += HandleGemsCleared;
            gridController.OnSwapResolved += HandleSwapResolved;

            OnMovesChanged?.Invoke(rules.MovesRemaining, moveLimit);
            OnScoreChanged?.Invoke(rules.CurrentScore, rules.TargetScore);
        }

        private void OnDestroy()
        {
            if (gridController == null)
            {
                return;
            }

            gridController.OnValidSwapPerformed -= HandleValidSwap;
            gridController.OnGemsCleared -= HandleGemsCleared;
            gridController.OnSwapResolved -= HandleSwapResolved;
        }

        private void HandleValidSwap()
        {
            rules.RecordSwap();
            OnMovesChanged?.Invoke(rules.MovesRemaining, moveLimit);
        }

        private void HandleGemsCleared(int count)
        {
            rules.RecordGemsCleared(count);
            OnScoreChanged?.Invoke(rules.CurrentScore, rules.TargetScore);
            AnnounceResultIfNeeded();
        }

        // Only checks the loss condition - by the time a swap fully resolves, every cascade step's
        // score has already been recorded, so a final move that wins via a later cascade step is
        // never mistakenly reported as a loss.
        private void HandleSwapResolved()
        {
            rules.NotifySwapResolved();
            AnnounceResultIfNeeded();
        }

        private void AnnounceResultIfNeeded()
        {
            if (resultAnnounced)
            {
                return;
            }

            if (rules.IsWon)
            {
                resultAnnounced = true;
                gridController.InputEnabled = false;
                LevelProgressionState.NotifyLevelCompleted(CurrentLevelNumber);
                OnLevelWon?.Invoke();
            }
            else if (rules.IsLost)
            {
                resultAnnounced = true;
                gridController.InputEnabled = false;
                OnLevelLost?.Invoke();
            }
        }
    }
}
