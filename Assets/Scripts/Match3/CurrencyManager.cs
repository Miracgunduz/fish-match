using System;
using UnityEngine;

namespace FishMatch.Match3
{
    // Static so gold persists across scene loads within a Play session without any
    // DontDestroyOnLoad singleton ceremony. SaveManager reads/restores this across app sessions.
    public static class CurrencyManager
    {
        public static int Gold { get; private set; }

        public static event Action<int> OnGoldChanged;

        // Attributed so this also resets correctly if "Enter Play Mode Options" ever disables domain
        // reload (statics would otherwise silently carry over from the previous Play session); doubles
        // as an explicit reset EditMode tests can call in [SetUp] for isolation.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            Gold = 0;
            OnGoldChanged = null;
        }

        public static void AddGold(int amount)
        {
            Gold += amount;
            OnGoldChanged?.Invoke(Gold);
        }

        // Fails (returns false, no state change) rather than allowing Gold to go negative.
        public static bool TrySpendGold(int amount)
        {
            if (amount > Gold)
            {
                return false;
            }

            Gold -= amount;
            OnGoldChanged?.Invoke(Gold);
            return true;
        }

        // For SaveManager to restore a loaded value directly, without treating it as newly earned gold.
        public static void SetGold(int amount)
        {
            Gold = amount;
            OnGoldChanged?.Invoke(Gold);
        }
    }
}
