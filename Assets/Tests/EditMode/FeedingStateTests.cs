using FishMatch.Match3;
using NUnit.Framework;

public class FeedingStateTests
{
    private const long OneDaySeconds = 24 * 3600L;

    [SetUp]
    public void ResetState()
    {
        FeedingState.Reset();
    }

    [Test]
    public void CanFeedNowIsTrueWhenNeverFed()
    {
        Assert.IsTrue(FeedingState.CanFeedNow(1_000_000));
    }

    [Test]
    public void CanFeedNowIsFalseRightAfterFeeding()
    {
        FeedingState.MarkFed(1_000_000);

        Assert.IsFalse(FeedingState.CanFeedNow(1_000_000 + 60));
    }

    [Test]
    public void CanFeedNowIsFalseJustBeforeTheCooldownElapses()
    {
        FeedingState.MarkFed(1_000_000);

        Assert.IsFalse(FeedingState.CanFeedNow(1_000_000 + OneDaySeconds - 1));
    }

    [Test]
    public void CanFeedNowIsTrueOnceTheCooldownElapses()
    {
        FeedingState.MarkFed(1_000_000);

        Assert.IsTrue(FeedingState.CanFeedNow(1_000_000 + OneDaySeconds));
    }

    [Test]
    public void SecondsUntilNextFeedIsZeroWhenFeedable()
    {
        Assert.AreEqual(0, FeedingState.SecondsUntilNextFeed(1_000_000));
    }

    [Test]
    public void SecondsUntilNextFeedCountsDownFromTheCooldown()
    {
        FeedingState.MarkFed(1_000_000);

        Assert.AreEqual(OneDaySeconds - 100, FeedingState.SecondsUntilNextFeed(1_000_000 + 100));
    }

    [Test]
    public void SetLastFedUnixSecondsRestoresATimestampForSaveLoad()
    {
        FeedingState.SetLastFedUnixSeconds(1_000_000);

        Assert.IsFalse(FeedingState.CanFeedNow(1_000_000 + 60));
        Assert.AreEqual(1_000_000, FeedingState.LastFedUnixSeconds);
    }

    [Test]
    public void ResetClearsBackToNeverFed()
    {
        FeedingState.MarkFed(1_000_000);
        FeedingState.Reset();

        Assert.AreEqual(0, FeedingState.LastFedUnixSeconds);
        Assert.IsTrue(FeedingState.CanFeedNow(1_000_000));
    }
}
