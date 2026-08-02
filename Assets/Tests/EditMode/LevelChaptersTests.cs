using FishMatch.Match3;
using NUnit.Framework;

public class LevelChaptersTests
{
    [TestCase(1, 0)]
    [TestCase(10, 0)]
    [TestCase(11, 1)]
    [TestCase(20, 1)]
    [TestCase(21, 2)]
    [TestCase(30, 2)]
    public void ChapterIndexForLevelGroupsLevelsIntoTensBlocks(int levelNumber, int expectedChapterIndex)
    {
        Assert.AreEqual(expectedChapterIndex, LevelChapters.ChapterIndexForLevel(levelNumber));
    }

    [Test]
    public void ChapterNameFallsBackToAGenericLabelBeyondTheNamedChapters()
    {
        Assert.AreEqual("Sığ Sular", LevelChapters.ChapterName(0));
        Assert.AreEqual("Atlantis Harabeleri", LevelChapters.ChapterName(1));
        Assert.AreEqual("Mercan Cenneti", LevelChapters.ChapterName(2));
        Assert.AreEqual("Bölge 4", LevelChapters.ChapterName(3));
    }

    [TestCase(9, false)]
    [TestCase(10, true)]
    [TestCase(11, false)]
    [TestCase(20, true)]
    [TestCase(30, true)]
    public void IsChapterUnlockLevelIsOnlyTrueOnTheLastLevelOfEachChapter(int levelNumber, bool expected)
    {
        Assert.AreEqual(expected, LevelChapters.IsChapterUnlockLevel(levelNumber));
    }

    [Test]
    public void MidChapterMilestoneAlternatesBetweenLevelUpAndChallengePerChapter()
    {
        // Chapter 0 (levels 1-10) is even-indexed - its mid-point is a cosmetic Level Up.
        Assert.IsTrue(LevelChapters.IsLevelUpLevel(6));
        Assert.IsFalse(LevelChapters.IsChallengeLevel(6));

        // Chapter 1 (levels 11-20) is odd-indexed - its mid-point is the harder Challenge variant.
        Assert.IsTrue(LevelChapters.IsChallengeLevel(16));
        Assert.IsFalse(LevelChapters.IsLevelUpLevel(16));
    }

    [TestCase(3, LevelChapters.MilestoneType.None)]
    [TestCase(6, LevelChapters.MilestoneType.LevelUp)]
    [TestCase(10, LevelChapters.MilestoneType.ChapterUnlock)]
    [TestCase(16, LevelChapters.MilestoneType.Challenge)]
    [TestCase(20, LevelChapters.MilestoneType.ChapterUnlock)]
    public void GetMilestoneReturnsTheExpectedTypePerLevel(int levelNumber, LevelChapters.MilestoneType expected)
    {
        Assert.AreEqual(expected, LevelChapters.GetMilestone(levelNumber));
    }

    [Test]
    public void ApplyChallengeModifierLeavesNonChallengeLevelsUnchanged()
    {
        var (moveLimit, targetScore) = LevelChapters.ApplyChallengeModifierIfNeeded(6, 20, 500);

        Assert.AreEqual(20, moveLimit);
        Assert.AreEqual(500, targetScore);
    }

    [Test]
    public void ApplyChallengeModifierReducesMovesAndRaisesTargetOnChallengeLevels()
    {
        var (moveLimit, targetScore) = LevelChapters.ApplyChallengeModifierIfNeeded(16, 20, 500);

        Assert.AreEqual(14, moveLimit); // 20 * 0.7
        Assert.AreEqual(650, targetScore); // 500 * 1.3
    }
}
