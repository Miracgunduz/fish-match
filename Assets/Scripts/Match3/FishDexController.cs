using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace FishMatch.Match3
{
    public class FishDexController : MonoBehaviour
    {
        [SerializeField] private FishSpeciesDefinition[] fishRoster;
        [SerializeField] private Text completionText;
        [SerializeField] private Text entriesText;

        private void Start()
        {
            // Also loads here so opening this scene directly in the Editor still shows correct status.
            SaveManager.LoadIfNeeded(fishRoster);

            var unlockedCount = 0;
            var sb = new StringBuilder();

            foreach (var species in fishRoster)
            {
                var owned = FishOwnershipRegistry.IsOwned(species);
                if (owned)
                {
                    unlockedCount++;
                }

                var pieces = FishPieceProgress.GetPieces(species.speciesId);
                var status = owned ? "Açık ✓" : $"Kilitli ({pieces}/{species.totalPieces} parça)";
                var fact = owned ? species.funFact : "???";

                sb.AppendLine($"{species.displayName} [{species.rarity}] - {status}");
                sb.AppendLine(fact);
                sb.AppendLine();
            }

            entriesText.text = sb.ToString();

            var percent = fishRoster.Length > 0 ? unlockedCount * 100 / fishRoster.Length : 0;
            completionText.text = $"%{percent} Tamamlandı ({unlockedCount}/{fishRoster.Length})";
        }
    }
}
