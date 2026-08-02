using UnityEngine;

namespace FishMatch.Match3
{
    [CreateAssetMenu(fileName = "Decoration_", menuName = "FishMatch/Decoration Definition")]
    public class DecorationDefinition : ScriptableObject
    {
        public string decorationId;
        public string displayName;
        public int price = 50;
        public Sprite sprite;

        // Fixed spot in the tank - a small hand-placed catalog doesn't need randomized/collision-avoided
        // placement like the swimming fish do.
        public Vector2 tankPosition;

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Same reasoning as FishSpeciesDefinition.speciesId - ownership is ID-based, so an unset ID
            // would make every such asset collide.
            if (string.IsNullOrEmpty(decorationId))
            {
                decorationId = name;
            }
        }
#endif
    }
}
