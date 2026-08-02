using System;
using System.Collections.Generic;
using UnityEngine;

namespace FishMatch.Match3
{
    // Pure logic - owns grid state only, no MonoBehaviour/scene dependency.
    // Match3GridController drives this and keeps the visual Gem objects in sync.
    public class Match3Grid
    {
        // Bounds the initial-fill reroll loop - with very few gem types the "no pre-formed match"
        // constraints can become unsatisfiable, and this stops that from hanging the game/Editor.
        private const int MaxFillRerolls = 30;

        public int Width { get; }
        public int Height { get; }

        private readonly GemType?[,] cells;
        private readonly Func<GemType> randomGemProvider;

        public Match3Grid(int width, int height, Func<GemType> randomGemProvider)
        {
            Width = width;
            Height = height;
            this.randomGemProvider = randomGemProvider;
            cells = new GemType?[width, height];
        }

        public bool InBounds(Vector2Int position)
        {
            return position.x >= 0 && position.x < Width && position.y >= 0 && position.y < Height;
        }

        public GemType GetCell(Vector2Int position)
        {
            return cells[position.x, position.y].Value;
        }

        public void Swap(Vector2Int a, Vector2Int b)
        {
            (cells[a.x, a.y], cells[b.x, b.y]) = (cells[b.x, b.y], cells[a.x, a.y]);
        }

        // Fills every cell randomly, re-rolling any placement that would create an already-matched run.
        public void FillRandom()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    GemType type;
                    int attempts = 0;
                    do
                    {
                        type = randomGemProvider();
                        attempts++;
                    } while (CausesInitialMatch(x, y, type) && attempts < MaxFillRerolls);

                    cells[x, y] = type;
                }
            }
        }

        private bool CausesInitialMatch(int x, int y, GemType type)
        {
            if (x >= 2 && cells[x - 1, y] == type && cells[x - 2, y] == type)
            {
                return true;
            }

            if (y >= 2 && cells[x, y - 1] == type && cells[x, y - 2] == type)
            {
                return true;
            }

            // Fill order is column-major (x outer, y inner), so these three neighbors are always
            // already placed by the time (x, y) is - completing them would form a 2x2 square match.
            if (x >= 1 && y >= 1 && cells[x - 1, y - 1] == type && cells[x - 1, y] == type && cells[x, y - 1] == type)
            {
                return true;
            }

            return false;
        }

        // Only valid to call when every cell is populated (i.e. before any ClearCells this cascade pass).
        public HashSet<Vector2Int> FindMatches()
        {
            var snapshot = new GemType[Width, Height];
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    snapshot[x, y] = cells[x, y].Value;
                }
            }

            return MatchFinder.FindMatches(snapshot);
        }

        public void ClearCells(IEnumerable<Vector2Int> cellsToClear)
        {
            foreach (var cell in cellsToClear)
            {
                cells[cell.x, cell.y] = null;
            }
        }

        // Checks whether any adjacent swap anywhere on the board would produce a match. Only checking
        // each cell's right/up neighbor (not left/down too) still covers every adjacent pair exactly
        // once. Used to detect and recover from a board with no legal moves left.
        public bool HasAnyValidMove()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var a = new Vector2Int(x, y);

                    var right = a + Vector2Int.right;
                    if (InBounds(right) && SwapProducesMatch(a, right))
                    {
                        return true;
                    }

                    var up = a + Vector2Int.up;
                    if (InBounds(up) && SwapProducesMatch(a, up))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool SwapProducesMatch(Vector2Int a, Vector2Int b)
        {
            Swap(a, b);
            var hasMatch = FindMatches().Count > 0;
            Swap(a, b);
            return hasMatch;
        }

        // Row 0 is the bottom row: existing gems fall toward y=0, empty slots at the top get freshly rolled gems.
        public List<ColumnCollapseResult> CollapseColumns()
        {
            var results = new List<ColumnCollapseResult>();

            for (int x = 0; x < Width; x++)
            {
                var columnResult = new ColumnCollapseResult(x);
                int writeY = 0;

                for (int y = 0; y < Height; y++)
                {
                    if (cells[x, y].HasValue)
                    {
                        if (writeY != y)
                        {
                            cells[x, writeY] = cells[x, y];
                            cells[x, y] = null;
                            columnResult.Falls.Add(new GemFall(new Vector2Int(x, y), new Vector2Int(x, writeY)));
                        }

                        writeY++;
                    }
                }

                for (int y = writeY; y < Height; y++)
                {
                    var newType = randomGemProvider();
                    cells[x, y] = newType;
                    columnResult.Spawns.Add(new GemSpawn(new Vector2Int(x, y), newType));
                }

                results.Add(columnResult);
            }

            return results;
        }
    }
}
