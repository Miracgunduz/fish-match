using UnityEngine;

namespace FishMatch.Match3
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class AquariumFishController : MonoBehaviour
    {
        [SerializeField] private GameObject coinPickupPrefab;
        [SerializeField] private float wanderSpeed = 1.5f;
        [SerializeField] private float arrivalThreshold = 0.15f;

        private FishSpeciesDefinition species;
        private Bounds wanderBounds;
        private Vector3 wanderTarget;
        private float productionTimer;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(FishSpeciesDefinition speciesDefinition, Bounds bounds)
        {
            species = speciesDefinition;
            wanderBounds = bounds;
            PickNewWanderTarget();
            productionTimer = species.productionIntervalSeconds;
        }

        private void Update()
        {
            if (species == null)
            {
                return;
            }

            MoveTowardTarget();
            TickProduction();
        }

        private void MoveTowardTarget()
        {
            var toTarget = wanderTarget - transform.position;
            if (toTarget.magnitude <= arrivalThreshold)
            {
                PickNewWanderTarget();
                return;
            }

            var direction = toTarget.normalized;
            transform.position += direction * wanderSpeed * Time.deltaTime;
            spriteRenderer.flipX = direction.x < 0f;
        }

        private void PickNewWanderTarget()
        {
            var x = Random.Range(wanderBounds.min.x, wanderBounds.max.x);
            var y = Random.Range(wanderBounds.min.y, wanderBounds.max.y);
            wanderTarget = new Vector3(x, y, transform.position.z);
        }

        private void TickProduction()
        {
            productionTimer -= Time.deltaTime;
            if (productionTimer <= 0f)
            {
                productionTimer = species.productionIntervalSeconds;
                SpawnCoin();
            }
        }

        private void SpawnCoin()
        {
            var coinObject = Instantiate(coinPickupPrefab, transform.position + Vector3.up * 0.3f, Quaternion.identity);
            var coin = coinObject.GetComponent<CoinPickup>();
            coin.Initialize(species.goldPerProduction);
        }
    }
}
