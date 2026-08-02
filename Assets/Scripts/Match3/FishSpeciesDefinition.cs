using UnityEngine;

namespace FishMatch.Match3
{
    [CreateAssetMenu(fileName = "Fish_", menuName = "FishMatch/Fish Species Definition")]
    public class FishSpeciesDefinition : ScriptableObject
    {
        public string speciesId;
        public string displayName;
        public FishRarity rarity;
        public int totalPieces = 8;
        [TextArea] public string funFact;
        public Sprite icon;

        [Header("Aquarium (passive income)")]
        public float productionIntervalSeconds = 8f;
        public int goldPerProduction = 5;

        // Ownership is tracked by speciesId (see FishOwnershipRegistry), so an unset ID would make
        // every such asset collide as "the same fish". Auto-fill from the asset name as a safety net
        // for species created by hand outside FishCollectionSetup, which always sets it explicitly.
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(speciesId))
            {
                speciesId = name;
            }
        }
#endif
    }
}
