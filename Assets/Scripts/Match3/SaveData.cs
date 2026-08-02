using System;
using System.Collections.Generic;

namespace FishMatch.Match3
{
    [Serializable]
    public class SaveData
    {
        public int gold;
        public int currentLevelNumber = 1;
        public long lastFedUnixSeconds;
        public List<string> ownedSpeciesIds = new List<string>();
        public List<string> ownedDecorationIds = new List<string>();
        public List<PieceProgressEntry> pieceProgress = new List<PieceProgressEntry>();
    }

    [Serializable]
    public class PieceProgressEntry
    {
        public string speciesId;
        public int pieces;
    }
}
