using UnityEngine;
using UnityEngine.UI;

namespace FishMatch.Match3
{
    public class FishProgressHUDController : MonoBehaviour
    {
        [SerializeField] private FishCollectionManager collectionManager;
        [SerializeField] private Text pieceProgressText;

        private void Awake()
        {
            pieceProgressText.text = string.Empty;
        }

        private void OnEnable()
        {
            collectionManager.OnPieceAwarded += HandlePieceAwarded;
            collectionManager.OnFishUnlocked += HandleFishUnlocked;
            collectionManager.OnAllFishUnlocked += HandleAllFishUnlocked;
        }

        private void OnDisable()
        {
            collectionManager.OnPieceAwarded -= HandlePieceAwarded;
            collectionManager.OnFishUnlocked -= HandleFishUnlocked;
            collectionManager.OnAllFishUnlocked -= HandleAllFishUnlocked;
        }

        private void HandlePieceAwarded(FishSpeciesDefinition species, int unlockedPieces, int totalPieces)
        {
            pieceProgressText.text = $"{species.displayName}: {unlockedPieces}/{totalPieces} parça";
        }

        private void HandleFishUnlocked(FishSpeciesDefinition species)
        {
            pieceProgressText.text = $"Yeni Balık Açıldı: {species.displayName}!";
        }

        private void HandleAllFishUnlocked()
        {
            pieceProgressText.text = "Tüm balıklar toplandı!";
        }
    }
}
