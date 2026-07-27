using UnityEngine;

namespace FishMatch.Match3
{
    [CreateAssetMenu(fileName = "Gem_", menuName = "FishMatch/Gem Definition")]
    public class GemDefinition : ScriptableObject
    {
        public GemType type;
        public Sprite shapeSprite;
        public Color tintColor = Color.white;
    }
}
