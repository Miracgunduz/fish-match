using System.Collections.Generic;
using UnityEngine;

namespace FishMatch.Match3
{
    public readonly struct GemFall
    {
        public readonly Vector2Int From;
        public readonly Vector2Int To;

        public GemFall(Vector2Int from, Vector2Int to)
        {
            From = from;
            To = to;
        }
    }

    public readonly struct GemSpawn
    {
        public readonly Vector2Int Position;
        public readonly GemType Type;

        public GemSpawn(Vector2Int position, GemType type)
        {
            Position = position;
            Type = type;
        }
    }

    public class ColumnCollapseResult
    {
        public int Column { get; }
        public List<GemFall> Falls { get; } = new List<GemFall>();
        public List<GemSpawn> Spawns { get; } = new List<GemSpawn>();

        public ColumnCollapseResult(int column)
        {
            Column = column;
        }
    }
}
