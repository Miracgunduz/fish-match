using System.Collections.Generic;
using UnityEngine;

namespace FishMatch.Match3
{
    // Pure logic, no scene/MonoBehaviour dependency - fully unit-testable with hand-built grids.
    public static class MatchFinder
    {
        private const int MinMatchLength = 3;

        public static HashSet<Vector2Int> FindMatches(GemType[,] cells)
        {
            int width = cells.GetLength(0);
            int height = cells.GetLength(1);
            var matched = new HashSet<Vector2Int>();

            for (int y = 0; y < height; y++)
            {
                ScanLine(matched, width, runStart => new Vector2Int(runStart, y), (a, b) => cells[a, y] == cells[b, y]);
            }

            for (int x = 0; x < width; x++)
            {
                ScanLine(matched, height, runStart => new Vector2Int(x, runStart), (a, b) => cells[x, a] == cells[x, b]);
            }

            FindSquares(matched, cells, width, height);

            return matched;
        }

        // A 2x2 block of the same gem type matches too, independent of any 3-in-a-row.
        private static void FindSquares(HashSet<Vector2Int> matched, GemType[,] cells, int width, int height)
        {
            for (int x = 0; x < width - 1; x++)
            {
                for (int y = 0; y < height - 1; y++)
                {
                    var type = cells[x, y];
                    if (cells[x + 1, y] == type && cells[x, y + 1] == type && cells[x + 1, y + 1] == type)
                    {
                        matched.Add(new Vector2Int(x, y));
                        matched.Add(new Vector2Int(x + 1, y));
                        matched.Add(new Vector2Int(x, y + 1));
                        matched.Add(new Vector2Int(x + 1, y + 1));
                    }
                }
            }
        }

        // Walks one row/column, closing a run whenever the value changes and recording it if long enough.
        // sameAs(index, runStart) compares the gem at `index` against the gem at the run's start.
        private static void ScanLine(HashSet<Vector2Int> matched, int length, System.Func<int, Vector2Int> toCell, System.Func<int, int, bool> sameAs)
        {
            int runStart = 0;
            for (int i = 1; i <= length; i++)
            {
                bool continuesRun = i < length && sameAs(i, runStart);
                if (!continuesRun)
                {
                    int runLength = i - runStart;
                    if (runLength >= MinMatchLength)
                    {
                        for (int j = runStart; j < i; j++)
                        {
                            matched.Add(toCell(j));
                        }
                    }
                    runStart = i;
                }
            }
        }
    }
}
