using System.Collections.Generic;
using UnityEngine;

namespace FishMatch.Match3
{
    // Static so ownership survives the Match3 scene <-> Aquarium scene switch within a Play session.
    // Stores species IDs, not FishSpeciesDefinition references, so ownership never depends on which
    // scene's roster happened to resolve it first - matches FishPieceProgress's ID-based approach and
    // avoids silently dropping a species that's missing from whichever roster called into save/load.
    public static class FishOwnershipRegistry
    {
        private static readonly HashSet<string> ownedSpeciesIds = new HashSet<string>();

        public static IReadOnlyCollection<string> OwnedSpeciesIds => ownedSpeciesIds;

        // See CurrencyManager.Reset for why this is attributed.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            ownedSpeciesIds.Clear();
        }

        public static void MarkOwned(FishSpeciesDefinition species)
        {
            if (species == null)
            {
                return;
            }

            ownedSpeciesIds.Add(species.speciesId);
        }

        // For SaveManager to restore ownership from a save file, where only the ID string is known -
        // no FishSpeciesDefinition asset needs to be loaded/resolved to do this.
        public static void MarkOwnedById(string speciesId)
        {
            if (!string.IsNullOrEmpty(speciesId))
            {
                ownedSpeciesIds.Add(speciesId);
            }
        }

        public static bool IsOwned(FishSpeciesDefinition species)
        {
            return species != null && ownedSpeciesIds.Contains(species.speciesId);
        }

        // For code that needs the actual assets (e.g. spawning owned fish in the tank) - filters the
        // caller's own roster rather than the registry trying to hand back objects it never stored.
        public static IEnumerable<FishSpeciesDefinition> FilterOwned(IEnumerable<FishSpeciesDefinition> roster)
        {
            foreach (var species in roster)
            {
                if (species == null)
                {
                    Debug.LogWarning("FishOwnershipRegistry.FilterOwned: skipping a null roster entry.");
                    continue;
                }

                if (ownedSpeciesIds.Contains(species.speciesId))
                {
                    yield return species;
                }
            }
        }
    }
}
