namespace FishMatch.Match3
{
    // Pure logic, no MonoBehaviour dependency - fully unit-testable.
    // Tracks a single level attempt's moves/score and determines win/loss.
    //
    // Win is evaluated as soon as the score crosses the target (RecordGemsCleared), but loss is only
    // evaluated once the whole swap - including every cascade step - has fully settled
    // (NotifySwapResolved). Checking the move limit mid-cascade would incorrectly call a winning move
    // a loss the instant moves hit zero, before the rest of that same cascade has scored.
    public class LevelRules
    {
        private const int PointsPerGem = 10;

        public int MovesRemaining { get; private set; }
        public int CurrentScore { get; private set; }
        public int TargetScore { get; }
        public bool IsWon { get; private set; }
        public bool IsLost { get; private set; }

        public LevelRules(int moveLimit, int targetScore)
        {
            MovesRemaining = moveLimit;
            TargetScore = targetScore;
        }

        // Call once per player swap that actually produces a match (i.e. "costs a move").
        public void RecordSwap()
        {
            if (IsWon || IsLost)
            {
                return;
            }

            MovesRemaining = MovesRemaining > 0 ? MovesRemaining - 1 : 0;
        }

        // Call once per cascade step within a swap's resolution.
        public void RecordGemsCleared(int count)
        {
            if (IsWon || IsLost)
            {
                return;
            }

            CurrentScore += count * PointsPerGem;
            if (CurrentScore >= TargetScore)
            {
                IsWon = true;
            }
        }

        // Call once the whole swap (every cascade step) has fully resolved.
        public void NotifySwapResolved()
        {
            if (IsWon || IsLost)
            {
                return;
            }

            if (MovesRemaining <= 0)
            {
                IsLost = true;
            }
        }
    }
}
