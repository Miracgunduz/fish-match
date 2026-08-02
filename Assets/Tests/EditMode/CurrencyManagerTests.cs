using FishMatch.Match3;
using NUnit.Framework;

public class CurrencyManagerTests
{
    [SetUp]
    public void ResetState()
    {
        CurrencyManager.Reset();
    }

    [Test]
    public void TrySpendGoldSucceedsAndDeductsWhenAffordable()
    {
        CurrencyManager.AddGold(100);

        var result = CurrencyManager.TrySpendGold(60);

        Assert.IsTrue(result);
        Assert.AreEqual(40, CurrencyManager.Gold);
    }

    [Test]
    public void TrySpendGoldFailsAndLeavesGoldUnchangedWhenNotAffordable()
    {
        CurrencyManager.AddGold(50);

        var result = CurrencyManager.TrySpendGold(60);

        Assert.IsFalse(result);
        Assert.AreEqual(50, CurrencyManager.Gold);
    }

    [Test]
    public void TrySpendGoldSucceedsWhenExactlyAffordable()
    {
        CurrencyManager.AddGold(60);

        var result = CurrencyManager.TrySpendGold(60);

        Assert.IsTrue(result);
        Assert.AreEqual(0, CurrencyManager.Gold);
    }
}
