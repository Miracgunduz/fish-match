using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FishMatch.Match3
{
    // Persists the static runtime state (CurrencyManager, LevelProgressionState, FishOwnershipRegistry,
    // FishPieceProgress) to a JSON file so progress survives across app/Play sessions, not just scene
    // reloads within one. Match-3 board state itself is deliberately never saved - each level scene
    // regenerates a fresh board on load regardless.
    public static class SaveManager
    {
        private const string SaveFileName = "savegame.json";

        private static bool hasLoadedThisSession;

        private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        // Safe to call from multiple scenes' Awake/Start - only actually loads once per session.
        public static void LoadIfNeeded(IReadOnlyList<FishSpeciesDefinition> knownRoster)
        {
            if (hasLoadedThisSession)
            {
                return;
            }

            hasLoadedThisSession = true;

            if (!File.Exists(SavePath))
            {
                return;
            }

            var json = File.ReadAllText(SavePath);
            var data = JsonUtility.FromJson<SaveData>(json);
            if (data == null)
            {
                return;
            }

            CurrencyManager.SetGold(data.gold);
            LevelProgressionState.SetCurrentLevelNumber(data.currentLevelNumber);

            foreach (var speciesId in data.ownedSpeciesIds)
            {
                var species = FindSpecies(knownRoster, speciesId);
                if (species != null)
                {
                    FishOwnershipRegistry.MarkOwned(species);
                }
            }

            foreach (var entry in data.pieceProgress)
            {
                FishPieceProgress.SetPieces(entry.speciesId, entry.pieces);
            }
        }

        public static void Save()
        {
            var data = new SaveData
            {
                gold = CurrencyManager.Gold,
                currentLevelNumber = LevelProgressionState.CurrentLevelNumber,
            };

            foreach (var species in FishOwnershipRegistry.OwnedSpecies)
            {
                data.ownedSpeciesIds.Add(species.speciesId);
            }

            foreach (var entry in FishPieceProgress.AllEntries)
            {
                data.pieceProgress.Add(new PieceProgressEntry { speciesId = entry.Key, pieces = entry.Value });
            }

            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
        }

        private static FishSpeciesDefinition FindSpecies(IReadOnlyList<FishSpeciesDefinition> roster, string speciesId)
        {
            foreach (var species in roster)
            {
                if (species != null && species.speciesId == speciesId)
                {
                    return species;
                }
            }

            return null;
        }
    }
}
