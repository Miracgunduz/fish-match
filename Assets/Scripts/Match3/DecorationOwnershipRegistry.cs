using System.Collections.Generic;
using UnityEngine;

namespace FishMatch.Match3
{
    // Static so ownership survives scene switches within a session. ID-based (matches
    // FishOwnershipRegistry/FishPieceProgress) so it never depends on which scene's catalog array
    // resolves first.
    public static class DecorationOwnershipRegistry
    {
        private static readonly HashSet<string> ownedDecorationIds = new HashSet<string>();

        public static IReadOnlyCollection<string> OwnedDecorationIds => ownedDecorationIds;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            ownedDecorationIds.Clear();
        }

        public static void MarkOwnedById(string decorationId)
        {
            if (!string.IsNullOrEmpty(decorationId))
            {
                ownedDecorationIds.Add(decorationId);
            }
        }

        public static bool IsOwned(DecorationDefinition decoration)
        {
            return decoration != null && ownedDecorationIds.Contains(decoration.decorationId);
        }

        public static IEnumerable<DecorationDefinition> FilterOwned(IEnumerable<DecorationDefinition> catalog)
        {
            foreach (var decoration in catalog)
            {
                if (decoration == null)
                {
                    Debug.LogWarning("DecorationOwnershipRegistry.FilterOwned: skipping a null catalog entry.");
                    continue;
                }

                if (ownedDecorationIds.Contains(decoration.decorationId))
                {
                    yield return decoration;
                }
            }
        }
    }
}
