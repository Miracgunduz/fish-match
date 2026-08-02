using System.Collections.Generic;
using UnityEngine;

namespace FishMatch.Match3
{
    // Static so piece progress survives the scene reload used to advance between levels
    // (Match3Test's FishCollectionManager instance is recreated fresh on every level load).
    // SaveManager reads/restores this across app sessions.
    public static class FishPieceProgress
    {
        private static readonly Dictionary<string, int> unlockedPiecesBySpeciesId = new Dictionary<string, int>();

        public static IReadOnlyDictionary<string, int> AllEntries => unlockedPiecesBySpeciesId;

        // See CurrencyManager.Reset for why this is attributed.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            unlockedPiecesBySpeciesId.Clear();
        }

        public static int GetPieces(string speciesId)
        {
            unlockedPiecesBySpeciesId.TryGetValue(speciesId, out var pieces);
            return pieces;
        }

        public static int AddPiece(string speciesId, int maxPieces)
        {
            var pieces = GetPieces(speciesId) + 1;
            if (pieces > maxPieces)
            {
                pieces = maxPieces;
            }

            unlockedPiecesBySpeciesId[speciesId] = pieces;
            return pieces;
        }

        // For SaveManager to restore a loaded value directly.
        public static void SetPieces(string speciesId, int pieces)
        {
            unlockedPiecesBySpeciesId[speciesId] = pieces;
        }
    }
}
