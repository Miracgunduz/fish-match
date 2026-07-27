using UnityEngine;
using UnityEngine.UI;

namespace FishMatch.Match3
{
    public class AquariumHUDController : MonoBehaviour
    {
        [SerializeField] private Text goldText;

        private void OnEnable()
        {
            CurrencyManager.OnGoldChanged += HandleGoldChanged;
            HandleGoldChanged(CurrencyManager.Gold);
        }

        private void OnDisable()
        {
            CurrencyManager.OnGoldChanged -= HandleGoldChanged;
        }

        private void HandleGoldChanged(int gold)
        {
            goldText.text = $"Altın: {gold}";
        }
    }
}
