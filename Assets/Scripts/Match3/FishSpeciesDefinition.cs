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

        [Header("Aquarium (passive income)")]
        public float productionIntervalSeconds = 8f;
        public int goldPerProduction = 5;
    }
}
