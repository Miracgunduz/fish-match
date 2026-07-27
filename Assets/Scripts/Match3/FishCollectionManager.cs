using System;
using UnityEngine;

namespace FishMatch.Match3
{
    // Awards one puzzle piece toward the current target fish each time a level is won.
    // The target is always the first not-yet-owned species in the roster, in order - once one
    // is fully collected, the next level win starts contributing to the next species automatically.
    public class FishCollectionManager : MonoBehaviour
    {
        [SerializeField] private LevelController levelController;
        [SerializeField] private FishSpeciesDefinition[] fishRoster;

        public event Action<FishSpeciesDefinition, int, int> OnPieceAwarded;
        public event Action<FishSpeciesDefinition> OnFishUnlocked;
        public event Action OnAllFishUnlocked;

        private void Awake()
        {
            SaveManager.LoadIfNeeded(fishRoster);
        }

        private void OnEnable()
        {
            levelController.OnLevelWon += HandleLevelWon;
        }

        private void OnDisable()
        {
            levelController.OnLevelWon -= HandleLevelWon;
        }

        private void HandleLevelWon()
        {
            AwardPieceToCurrentTarget();
        }

        private FishSpeciesDefinition GetCurrentTarget()
        {
            foreach (var species in fishRoster)
            {
                if (!FishOwnershipRegistry.IsOwned(species))
                {
                    return species;
                }
            }

            return null;
        }

        private void AwardPieceToCurrentTarget()
        {
            var target = GetCurrentTarget();
            if (target == null)
            {
                OnAllFishUnlocked?.Invoke();
                return;
            }

            var pieces = FishPieceProgress.AddPiece(target.speciesId, target.totalPieces);
            OnPieceAwarded?.Invoke(target, pieces, target.totalPieces);

            if (pieces >= target.totalPieces)
            {
                FishOwnershipRegistry.MarkOwned(target);
                OnFishUnlocked?.Invoke(target);
            }

            SaveManager.Save();
        }
    }
}
