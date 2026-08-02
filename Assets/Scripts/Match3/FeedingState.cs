using UnityEngine;

namespace FishMatch.Match3
{
    // Pure logic (no MonoBehaviour dependency, no direct DateTime.UtcNow calls) - callers pass in
    // "now" as Unix seconds, which keeps this fully unit-testable and matches the LevelRules pattern.
    // Static so the last-fed timestamp survives scene reloads within a session; SaveManager persists
    // it across app restarts the same way it does gold/level progress.
    public static class FeedingState
    {
        private const long CooldownSeconds = 24 * 3600L;
        public const int FeedRewardGold = 20;

        // 0 means "never fed" - always feedable.
        public static long LastFedUnixSeconds { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            LastFedUnixSeconds = 0;
        }

        // For SaveManager to restore a loaded value directly.
        public static void SetLastFedUnixSeconds(long unixSeconds)
        {
            LastFedUnixSeconds = unixSeconds;
        }

        public static bool CanFeedNow(long nowUnixSeconds)
        {
            if (LastFedUnixSeconds == 0)
            {
                return true;
            }

            return nowUnixSeconds - LastFedUnixSeconds >= CooldownSeconds;
        }

        public static void MarkFed(long nowUnixSeconds)
        {
            LastFedUnixSeconds = nowUnixSeconds;
        }

        public static long SecondsUntilNextFeed(long nowUnixSeconds)
        {
            if (CanFeedNow(nowUnixSeconds))
            {
                return 0;
            }

            return (LastFedUnixSeconds + CooldownSeconds) - nowUnixSeconds;
        }
    }
}
