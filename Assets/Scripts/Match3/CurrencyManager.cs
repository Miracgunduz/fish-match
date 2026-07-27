using System;

namespace FishMatch.Match3
{
    // Static so gold persists across scene loads within a Play session without any
    // DontDestroyOnLoad singleton ceremony. SaveManager reads/restores this across app sessions.
    public static class CurrencyManager
    {
        public static int Gold { get; private set; }

        public static event Action<int> OnGoldChanged;

        public static void AddGold(int amount)
        {
            Gold += amount;
            OnGoldChanged?.Invoke(Gold);
        }

        // For SaveManager to restore a loaded value directly, without treating it as newly earned gold.
        public static void SetGold(int amount)
        {
            Gold = amount;
            OnGoldChanged?.Invoke(Gold);
        }
    }
}
