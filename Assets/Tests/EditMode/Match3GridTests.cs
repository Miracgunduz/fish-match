using System.Collections.Generic;
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

    [Test]
    public void CollapseColumnsHandlesFullyClearedColumn()
    {
        var grid = new Match3Grid(2, 3, NextType);
        grid.FillRandom();

        grid.ClearCells(new[] { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2) });
        var results = grid.CollapseColumns();

        var column0 = results[0];
        Assert.AreEqual(0, column0.Falls.Count);
        Assert.AreEqual(3, column0.Spawns.Count);
        Assert.AreEqual(new Vector2Int(0, 0), column0.Spawns[0].Position);
        Assert.AreEqual(new Vector2Int(0, 1), column0.Spawns[1].Position);
        Assert.AreEqual(new Vector2Int(0, 2), column0.Spawns[2].Position);
    }

    [Test]
    public void CollapseColumnsHandlesNonContiguousHoles()
    {
        // Clear the bottom (y=0) and top (y=2) of a 3-tall column, leaving only the middle (y=1) gem,
        // which must fall all the way to y=0 while two new gems spawn above it.
        var grid = new Match3Grid(1, 3, NextType);
        grid.FillRandom();

        var survivorValue = grid.GetCell(new Vector2Int(0, 1));

        grid.ClearCells(new[] { new Vector2Int(0, 0), new Vector2Int(0, 2) });
        var results = grid.CollapseColumns();

        var column0 = results[0];
        Assert.AreEqual(1, column0.Falls.Count);
        Assert.AreEqual(new Vector2Int(0, 1), column0.Falls[0].From);
        Assert.AreEqual(new Vector2Int(0, 0), column0.Falls[0].To);
        Assert.AreEqual(survivorValue, grid.GetCell(new Vector2Int(0, 0)));

        Assert.AreEqual(2, column0.Spawns.Count);
    }

    [Test]
    public void CollapseColumnsHandlesClearingTopmostCell()
    {
        var grid = new Match3Grid(1, 3, NextType);
        grid.FillRandom();

        grid.ClearCells(new[] { new Vector2Int(0, 2) });
        var results = grid.CollapseColumns();

        var column0 = results[0];
        Assert.AreEqual(0, column0.Falls.Count);
        Assert.AreEqual(1, column0.Spawns.Count);
        Assert.AreEqual(new Vector2Int(0, 2), column0.Spawns[0].Position);
    }

    [Test]
    public void HasAnyValidMoveIsFalseOnADeadlockedTwoByTwoBoard()
    {
        // A 2x2 board can never have a legal swap: matches require 3-in-a-row (impossible with only
        // 2 cells per line) or a full 2x2 square, which a single swap of two distinct-valued cells
        // among four already-distinct values can never complete.
        var sequence = new Queue<GemType>(new[]
        {
            GemType.Yellow, GemType.Blue,
            GemType.Red, GemType.Green,
        });
        var grid = new Match3Grid(2, 2, () => sequence.Dequeue());
        grid.FillRandom();

        Assert.IsFalse(grid.HasAnyValidMove());
    }

    [Test]
    public void HasAnyValidMoveFindsAKnownLegalSwap()
    {
        // Red, Red, Blue, Red - swapping the last two cells produces a 3-in-a-row of Red.
        var sequence = new Queue<GemType>(new[]
        {
            GemType.Red, GemType.Red, GemType.Blue, GemType.Red,
        });
        var grid = new Match3Grid(4, 1, () => sequence.Dequeue());
        grid.FillRandom();

        Assert.IsTrue(grid.HasAnyValidMove());
    }
}
