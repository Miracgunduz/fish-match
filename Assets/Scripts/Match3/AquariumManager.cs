using UnityEngine;

namespace FishMatch.Match3
{
    public class AquariumManager : MonoBehaviour
    {
        [SerializeField] private GameObject aquariumFishPrefab;
        [SerializeField] private FishSpeciesDefinition[] fishRoster;
        [SerializeField] private Vector2 tankBoundsMin = new Vector2(-4f, -3f);
        [SerializeField] private Vector2 tankBoundsMax = new Vector2(4f, 3f);

        [Header("Decorations")]
        [SerializeField] private GameObject decorationPrefab;
        [SerializeField] private DecorationDefinition[] decorationCatalog;

        private void Start()
        {
            ApplyChapterTheme();

            var bounds = new Bounds();
            bounds.SetMinMax(new Vector3(tankBoundsMin.x, tankBoundsMin.y, 0f), new Vector3(tankBoundsMax.x, tankBoundsMax.y, 0f));

            foreach (var species in FishOwnershipRegistry.FilterOwned(fishRoster))
            {
                SpawnFish(species, bounds);
            }

            if (decorationCatalog != null)
            {
                foreach (var decoration in DecorationOwnershipRegistry.FilterOwned(decorationCatalog))
                {
                    SpawnDecoration(decoration);
                }
            }
        }

        // The aquarium's color palette shifts to match the furthest chapter the player has unlocked
        // (see LevelChapters) - a cheap way to make chapter progress feel like a new "theme" without
        // needing hand-painted backgrounds yet.
        private void ApplyChapterTheme()
        {
            int chapterIndex = LevelChapters.ChapterIndexForLevel(LevelProgressionState.HighestUnlockedLevel);
            var themeColor = LevelChapters.ChapterThemeColor(chapterIndex);

            var cam = Camera.main;
            if (cam != null)
            {
                cam.backgroundColor = themeColor;
            }

            RenderSettings.ambientLight = themeColor;
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

        private void SpawnDecoration(DecorationDefinition decoration)
        {
            var decorationObject = Instantiate(decorationPrefab, (Vector3)decoration.tankPosition, Quaternion.identity, transform);
            var spriteRenderer = decorationObject.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = decoration.sprite;
        }
    }
}
