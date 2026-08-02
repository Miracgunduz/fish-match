using FishMatch.Match3;
using NUnit.Framework;
using UnityEngine;

public class MatchFinderTests
{
    [Test]
    public void FindsHorizontalMatchOfThree()
    {
        var cells = new GemType[5, 1];
        cells[0, 0] = GemType.Red;
        cells[1, 0] = GemType.Red;
        cells[2, 0] = GemType.Red;
        cells[3, 0] = GemType.Blue;
        cells[4, 0] = GemType.Green;

        var matches = MatchFinder.FindMatches(cells);

        Assert.AreEqual(3, matches.Count);
        Assert.IsTrue(matches.Contains(new Vector2Int(0, 0)));
        Assert.IsTrue(matches.Contains(new Vector2Int(1, 0)));
        Assert.IsTrue(matches.Contains(new Vector2Int(2, 0)));
    }

    [Test]
    public void FindsVerticalMatchOfFour()
    {
        var cells = new GemType[1, 5];
        cells[0, 0] = GemType.Yellow;
        cells[0, 1] = GemType.Blue;
        cells[0, 2] = GemType.Blue;
        cells[0, 3] = GemType.Blue;
        cells[0, 4] = GemType.Blue;

        var matches = MatchFinder.FindMatches(cells);

        Assert.AreEqual(4, matches.Count);
    }

    [Test]
    public void NoMatchWhenRunIsOnlyTwoLong()
    {
        var cells = new GemType[3, 1];
        cells[0, 0] = GemType.Red;
        cells[1, 0] = GemType.Red;
        cells[2, 0] = GemType.Blue;

        var matches = MatchFinder.FindMatches(cells);

        Assert.AreEqual(0, matches.Count);
    }

    [Test]
    public void FindsLShapedIntersectionAsUnion()
    {
        // Bottom row all Red (horizontal match) plus the rightmost column continuing Red upward
        // (vertical match), sharing the bottom-right corner cell.
        var cells = new GemType[3, 3];
        for (int x = 0; x < 3; x++)
        {
            cells[x, 0] = GemType.Red;
        }

        cells[2, 1] = GemType.Red;
        cells[2, 2] = GemType.Red;
        cells[0, 1] = GemType.Green;
        cells[1, 1] = GemType.Blue;
        cells[0, 2] = GemType.Yellow;
        cells[1, 2] = GemType.Purple;

        var matches = MatchFinder.FindMatches(cells);

        // 3 (bottom row) + 2 (column cells above the shared corner) = 5 unique cells.
        Assert.AreEqual(5, matches.Count);
    }

    [Test]
    public void FindsTwoByTwoSquareMatch()
    {
        var cells = new GemType[3, 2];
        cells[0, 0] = GemType.Purple;
        cells[1, 0] = GemType.Purple;
        cells[0, 1] = GemType.Purple;
        cells[1, 1] = GemType.Purple;
        cells[2, 0] = GemType.Green;
        cells[2, 1] = GemType.Blue;

        var matches = MatchFinder.FindMatches(cells);

        Assert.AreEqual(4, matches.Count);
        Assert.IsTrue(matches.Contains(new Vector2Int(0, 0)));
        Assert.IsTrue(matches.Contains(new Vector2Int(1, 0)));
        Assert.IsTrue(matches.Contains(new Vector2Int(0, 1)));
        Assert.IsTrue(matches.Contains(new Vector2Int(1, 1)));
    }

    [Test]
    public void FindsTwoSeparateRunsInTheSameLine()
    {
        // Exercises the runStart reset in ScanLine - a closed run followed immediately by a new one.
        var cells = new GemType[7, 1];
        cells[0, 0] = GemType.Red;
        cells[1, 0] = GemType.Red;
        cells[2, 0] = GemType.Red;
        cells[3, 0] = GemType.Blue;
        cells[4, 0] = GemType.Green;
        cells[5, 0] = GemType.Green;
        cells[6, 0] = GemType.Green;

        var matches = MatchFinder.FindMatches(cells);

        Assert.AreEqual(6, matches.Count);
        Assert.IsTrue(matches.Contains(new Vector2Int(0, 0)));
        Assert.IsTrue(matches.Contains(new Vector2Int(2, 0)));
        Assert.IsFalse(matches.Contains(new Vector2Int(3, 0)));
        Assert.IsTrue(matches.Contains(new Vector2Int(4, 0)));
        Assert.IsTrue(matches.Contains(new Vector2Int(6, 0)));
    }

    [Test]
    public void RunEndingExactlyAtTheLineBoundaryIsMatched()
    {
        // The run touches the very last index of the line, exercising the terminal (i == length)
        // close path in ScanLine rather than a mismatch-triggered close.
        var cells = new GemType[4, 1];
        cells[0, 0] = GemType.Blue;
        cells[1, 0] = GemType.Red;
        cells[2, 0] = GemType.Red;
        cells[3, 0] = GemType.Red;

        var matches = MatchFinder.FindMatches(cells);

        Assert.AreEqual(3, matches.Count);
        Assert.IsTrue(matches.Contains(new Vector2Int(1, 0)));
        Assert.IsTrue(matches.Contains(new Vector2Int(2, 0)));
        Assert.IsTrue(matches.Contains(new Vector2Int(3, 0)));
    }

    [Test]
    public void SquareOverlappingAThreeRunIsNotDoubleCounted()
    {
        // Bottom-left 2x2 square of Yellow, with the bottom row continuing Yellow into a 3rd column
        // (also a horizontal run). The HashSet union just means the shared cells aren't duplicated.
        var cells = new GemType[3, 2];
        cells[0, 0] = GemType.Yellow;
        cells[1, 0] = GemType.Yellow;
        cells[2, 0] = GemType.Yellow;
        cells[0, 1] = GemType.Yellow;
        cells[1, 1] = GemType.Yellow;
        cells[2, 1] = GemType.Blue;

        var matches = MatchFinder.FindMatches(cells);

        // Union of the horizontal run {(0,0),(1,0),(2,0)} and the square {(0,0),(1,0),(0,1),(1,1)} = 5.
        Assert.AreEqual(5, matches.Count);
    }
}
