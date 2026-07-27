using FishMatch.Match3;
using NUnit.Framework;
using UnityEngine;

public class Match3GridTests
{
    private static readonly GemType[] Cycle =
    {
        GemType.Yellow, GemType.Blue, GemType.Red, GemType.Green, GemType.Purple
    };

    private int cycleIndex;

    private GemType NextType()
    {
        var type = Cycle[cycleIndex % Cycle.Length];
        cycleIndex++;
        return type;
    }

    [SetUp]
    public void ResetCycle()
    {
        cycleIndex = 0;
    }

    [Test]
    public void FillRandomProducesNoInitialMatches()
    {
        var grid = new Match3Grid(7, 7, NextType);
        grid.FillRandom();

        var matches = grid.FindMatches();

        Assert.AreEqual(0, matches.Count);
    }

    [Test]
    public void SwapIsReversible()
    {
        var grid = new Match3Grid(4, 4, NextType);
        grid.FillRandom();

        var a = new Vector2Int(0, 0);
        var b = new Vector2Int(1, 0);
        var originalA = grid.GetCell(a);
        var originalB = grid.GetCell(b);

        grid.Swap(a, b);
        Assert.AreEqual(originalB, grid.GetCell(a));
        Assert.AreEqual(originalA, grid.GetCell(b));

        grid.Swap(a, b);
        Assert.AreEqual(originalA, grid.GetCell(a));
        Assert.AreEqual(originalB, grid.GetCell(b));
    }

    [Test]
    public void CollapseColumnsMakesGemsFallAndRefillsTop()
    {
        var grid = new Match3Grid(3, 4, NextType);
        grid.FillRandom();

        var valueAt2Before = grid.GetCell(new Vector2Int(1, 2));
        var valueAt3Before = grid.GetCell(new Vector2Int(1, 3));

        grid.ClearCells(new[] { new Vector2Int(1, 1) });
        var results = grid.CollapseColumns();

        var column1 = results[1];
        Assert.AreEqual(2, column1.Falls.Count);
        Assert.AreEqual(new Vector2Int(1, 2), column1.Falls[0].From);
        Assert.AreEqual(new Vector2Int(1, 1), column1.Falls[0].To);
        Assert.AreEqual(new Vector2Int(1, 3), column1.Falls[1].From);
        Assert.AreEqual(new Vector2Int(1, 2), column1.Falls[1].To);

        Assert.AreEqual(1, column1.Spawns.Count);
        Assert.AreEqual(new Vector2Int(1, 3), column1.Spawns[0].Position);

        Assert.AreEqual(valueAt2Before, grid.GetCell(new Vector2Int(1, 1)));
        Assert.AreEqual(valueAt3Before, grid.GetCell(new Vector2Int(1, 2)));
    }
}
