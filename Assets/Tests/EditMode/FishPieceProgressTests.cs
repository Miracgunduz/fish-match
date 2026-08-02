using FishMatch.Match3;
using NUnit.Framework;

public class FishPieceProgressTests
{
    [SetUp]
    public void ResetState()
    {
        FishPieceProgress.Reset();
    }

    [Test]
    public void AddPieceIncrementsFromZero()
    {
        var pieces = FishPieceProgress.AddPiece("TestFish", maxPieces: 8);
        Assert.AreEqual(1, pieces);
    }

    [Test]
    public void AddPieceCapsAtMaxPieces()
    {
        for (var i = 0; i < 10; i++)
        {
            FishPieceProgress.AddPiece("TestFish", maxPieces: 3);
        }

        Assert.AreEqual(3, FishPieceProgress.GetPieces("TestFish"));
    }

    [Test]
    public void GetPiecesReturnsZeroForUnknownSpecies()
    {
        Assert.AreEqual(0, FishPieceProgress.GetPieces("NeverAwarded"));
    }

    [Test]
    public void SetPiecesOverwritesDirectly()
    {
        FishPieceProgress.AddPiece("TestFish", maxPieces: 8);
        FishPieceProgress.SetPieces("TestFish", 5);

        Assert.AreEqual(5, FishPieceProgress.GetPieces("TestFish"));
    }

    [Test]
    public void AllEntriesReflectsSetPieces()
    {
        FishPieceProgress.SetPieces("A", 2);
        FishPieceProgress.SetPieces("B", 4);

        Assert.AreEqual(2, FishPieceProgress.AllEntries["A"]);
        Assert.AreEqual(4, FishPieceProgress.AllEntries["B"]);
    }
}
