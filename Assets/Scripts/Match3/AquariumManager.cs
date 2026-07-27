using UnityEngine;

namespace FishMatch.Match3
{
    public class AquariumManager : MonoBehaviour
    {
        [SerializeField] private GameObject aquariumFishPrefab;
        [SerializeField] private FishSpeciesDefinition[] fishRoster;
        [SerializeField] private Vector2 tankBoundsMin = new Vector2(-4f, -3f);
        [SerializeField] private Vector2 tankBoundsMax = new Vector2(4f, 3f);

        private void Start()
        {
            // Also loads here (not just in Match3Test's FishCollectionManager) so opening this scene
            // directly in the Editor still restores saved fish ownership before spawning.
            SaveManager.LoadIfNeeded(fishRoster);

            var bounds = new Bounds();
            bounds.SetMinMax(new Vector3(tankBoundsMin.x, tankBoundsMin.y, 0f), new Vector3(tankBoundsMax.x, tankBoundsMax.y, 0f));

            foreach (var species in FishOwnershipRegistry.OwnedSpecies)
            {
                SpawnFish(species, bounds);
            }
        }

        private void SpawnFish(FishSpeciesDefinition species, Bounds bounds)
        {
            var spawnPosition = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                0f);

            var fishObject = Instantiate(aquariumFishPrefab, spawnPosition, Quaternion.identity, transform);
            var fishController = fishObject.GetComponent<AquariumFishController>();
            fishController.Initialize(species, bounds);
        }
    }
}
