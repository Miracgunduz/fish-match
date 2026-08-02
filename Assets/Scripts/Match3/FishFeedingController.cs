using System;
using UnityEngine;
using UnityEngine.UI;

namespace FishMatch.Match3
{
    // Once-a-day "feed your fish" interaction in the Aquarium scene - a small guaranteed gold reward
    // to give players a reason to open the aquarium daily even between match-3 sessions.
    public class FishFeedingController : MonoBehaviour
    {
        [SerializeField] private Button feedButton;
        [SerializeField] private Text statusText;

        private void Awake()
        {
            feedButton.onClick.AddListener(HandleFeedClicked);
        }

        private void OnEnable()
        {
            RefreshUI();
        }

        private void HandleFeedClicked()
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (!FeedingState.CanFeedNow(now))
            {
                RefreshUI();
                return;
            }

            FeedingState.MarkFed(now);
            CurrencyManager.AddGold(FeedingState.FeedRewardGold);
            SaveManager.Save();
            RefreshUI();
        }

        private void RefreshUI()
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (FeedingState.CanFeedNow(now))
            {
                feedButton.interactable = true;
                statusText.text = $"Balıkları Besle (+{FeedingState.FeedRewardGold} Altın)";
            }
            else
            {
                feedButton.interactable = false;
                var remaining = FeedingState.SecondsUntilNextFeed(now);
                var hours = remaining / 3600;
                var minutes = (remaining % 3600) / 60;
                statusText.text = $"Sonraki besleme: {hours}s {minutes}dk";
            }
        }
    }
}
