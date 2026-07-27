using System.Collections.Generic;

namespace FishMatch.Match3
{
    // Static so ownership survives the Match3 scene <-> Aquarium scene switch within a Play session.
    // Resets when Play mode stops - save/load isn't in scope yet.
    public static class FishOwnershipRegistry
    {
        private static readonly List<FishSpeciesDefinition> ownedSpecies = new List<FishSpeciesDefinition>();

        public static IReadOnlyList<FishSpeciesDefinition> OwnedSpecies => ownedSpecies;

        public static void MarkOwned(FishSpeciesDefinition species)
        {
            if (!ownedSpecies.Contains(species))
            {
                ownedSpecies.Add(species);
            }
        }

        public static bool IsOwned(FishSpeciesDefinition species)
        {
            return ownedSpecies.Contains(species);
        }
    }
}
