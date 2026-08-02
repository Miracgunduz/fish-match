using FishMatch.Match3;
using NUnit.Framework;

public class LevelRulesTests
{
    [Test]
    public void FinalMoveCascadeThatReachesTargetIsAWinNotALoss()
    {
        // Regression test: a multi-step cascade on the last move used to be reported as a loss
        // because the move-limit check ran mid-cascade, before later steps could reach the target.
        var rules = new LevelRules(moveLimit: 1, targetScore: 100);

        rules.RecordSwap(); // consumes the last move - MovesRemaining hits 0
        rules.RecordGemsCleared(3); // cascade step 1: 30 points, still below target
        rules.RecordGemsCleared(3); // cascade step 2: 30 more, still below target
        rules.RecordGemsCleared(4); // cascade step 3: crosses the 100 target

        Assert.IsFalse(rules.IsLost, "A move-limit check must not fire before the cascade finishes.");
        Assert.IsTrue(rules.IsWon, "Score crossed the target within the same move's cascade.");

        rules.NotifySwapResolved();

        Assert.IsTrue(rules.IsWon);
        Assert.IsFalse(rules.IsLost);
    }

    [Test]
    public void RunningOutOfMovesWithoutReachingTargetIsALoss()
    {
        var rules = new LevelRules(moveLimit: 1, targetScore: 100);

        rules.RecordSwap();
        rules.RecordGemsCleared(3); // 30 points, below target

        Assert.IsFalse(rules.IsLost, "Loss must only be evaluated once the swap fully resolves.");

        rules.NotifySwapResolved();

        Assert.IsTrue(rules.IsLost);
        Assert.IsFalse(rules.IsWon);
    }

    [Test]
    public void ResultIsLockedOnceDecided()
    {
        var rules = new LevelRules(moveLimit: 5, targetScore: 10);

        rules.RecordSwap();
        rules.RecordGemsCleared(1); // 10 points - wins immediately
        Assert.IsTrue(rules.IsWon);

        var scoreAtWin = rules.CurrentScore;
        var movesAtWin = rules.MovesRemaining;

        rules.RecordSwap();
        rules.RecordGemsCleared(5);
        rules.NotifySwapResolved();

        Assert.AreEqual(scoreAtWin, rules.CurrentScore);
        Assert.AreEqual(movesAtWin, rules.MovesRemaining);
        Assert.IsTrue(rules.IsWon);
        Assert.IsFalse(rules.IsLost);
    }

    [Test]
    public void MovesRemainingNeverGoesBelowZero()
    {
        var rules = new LevelRules(moveLimit: 1, targetScore: 1000);

        rules.RecordSwap();
        rules.RecordGemsCleared(1);
        rules.NotifySwapResolved();

        Assert.AreEqual(0, rules.MovesRemaining);
    }
}
