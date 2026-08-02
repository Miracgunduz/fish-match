using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FishMatch.Match3
{
    // Persists the static runtime state (CurrencyManager, LevelProgressionState, FishOwnershipRegistry,
    // FishPieceProgress) to a JSON file so progress survives across app/Play sessions, not just scene
    // reloads within one. Match-3 board state itself is deliberately never saved - each level scene
    // regenerates a fresh board on load regardless.
    //
    // Ownership/piece-progress are both ID-based (see FishOwnershipRegistry/FishPieceProgress), so
    // loading never depends on which scene's FishSpeciesDefinition roster happens to be available -
    // it can run once, automatically, before any scene loads.
    public static class SaveManager
    {
        private const string SaveFileName = "savegame.json";

        private static bool hasLoadedThisSession;

        private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        // Runs before LoadOnBoot (SubsystemRegistration fires ahead of BeforeSceneLoad) so a session
        // that never gets a domain reload (Enter Play Mode Options with domain reload disabled) still
        // re-reads the save file instead of silently reusing whatever was in memory from last time.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSessionFlag()
        {
            hasLoadedThisSession = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadOnBoot()
        {
            LoadIfNeeded();
        }

        // Idempotent per session - safe to call defensively from anywhere, but no longer necessary to
        // call from individual scenes/components now that LoadOnBoot runs automatically.
        public static void LoadIfNeeded()
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

            SaveData data;
            try
            {
                var json = File.ReadAllText(SavePath);
                data = JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveManager: save file at {SavePath} could not be read ({e.Message}); starting fresh without touching the file.");
                return;
            }

            if (data == null)
            {
                return;
            }

            CurrencyManager.SetGold(data.gold);
            LevelProgressionState.SetHighestUnlockedLevel(data.currentLevelNumber);
            FeedingState.SetLastFedUnixSeconds(data.lastFedUnixSeconds);

            if (data.ownedSpeciesIds != null)
            {
                foreach (var speciesId in data.ownedSpeciesIds)
                {
                    FishOwnershipRegistry.MarkOwnedById(speciesId);
                }
            }

            if (data.ownedDecorationIds != null)
            {
                foreach (var decorationId in data.ownedDecorationIds)
                {
                    DecorationOwnershipRegistry.MarkOwnedById(decorationId);
                }
            }

            if (data.pieceProgress != null)
            {
                foreach (var entry in data.pieceProgress)
                {
                    if (entry != null)
                    {
                        FishPieceProgress.SetPieces(entry.speciesId, entry.pieces);
                    }
                }
            }
        }

        // Writes via a temp file + atomic replace so a mid-write kill (very possible on mobile, and
        // this is called on every coin collect) can never leave the save file truncated/corrupt - the
        // original file is only ever replaced once the new content is fully and successfully written.
        public static void Save()
        {
            var data = new SaveData
            {
                gold = CurrencyManager.Gold,
                currentLevelNumber = LevelProgressionState.HighestUnlockedLevel,
                lastFedUnixSeconds = FeedingState.LastFedUnixSeconds,
                ownedSpeciesIds = new List<string>(FishOwnershipRegistry.OwnedSpeciesIds),
                ownedDecorationIds = new List<string>(DecorationOwnershipRegistry.OwnedDecorationIds),
            };

            foreach (var entry in FishPieceProgress.AllEntries)
            {
                data.pieceProgress.Add(new PieceProgressEntry { speciesId = entry.Key, pieces = entry.Value });
            }

            var json = JsonUtility.ToJson(data, true);
            var tempPath = SavePath + ".tmp";

            try
            {
                File.WriteAllText(tempPath, json);

                if (File.Exists(SavePath))
                {
                    File.Replace(tempPath, SavePath, null);
                }
                else
                {
                    File.Move(tempPath, SavePath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveManager: failed to write save file ({e.Message}).");
            }
        }
    }
}
