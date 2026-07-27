using UnityEngine;

namespace FishMatch.Match3
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Gem : MonoBehaviour
    {
        private const float MoveSpeed = 12f;

        public GemType Type { get; private set; }
        public Vector2Int GridPosition { get; private set; }

        private SpriteRenderer spriteRenderer;
        private Vector3 targetLocalPosition;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(GemDefinition definition, Vector2Int gridPosition)
        {
            Type = definition.type;
            GridPosition = gridPosition;
            spriteRenderer.sprite = definition.shapeSprite;
            spriteRenderer.color = definition.tintColor;
        }

        // snap = true places the gem instantly; snap = false lets Update() lerp it in (used for falls/spawns).
        public void SetGridPosition(Vector2Int gridPosition, Vector3 localPosition, bool snap = false)
        {
            GridPosition = gridPosition;
            targetLocalPosition = localPosition;
            if (snap)
            {
                transform.localPosition = localPosition;
            }
        }

        private void Update()
        {
            if (transform.localPosition != targetLocalPosition)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetLocalPosition, MoveSpeed * Time.deltaTime);
            }
        }
    }
}
