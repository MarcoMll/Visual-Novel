using System;
using System.Collections.Generic;

namespace VisualNovel.GameFlow.SaveSystem
{
    /// <summary>
    /// Stores player-specific information such as items, traits and relationships.
    /// </summary>
    [Serializable]
    public class PlayerGameData : BaseGameData
    {
        public List<string> Items = new();
        public List<string> Traits = new();
        public Dictionary<string, int> Relationships = new();

        protected override string SaveKey => "PLAYER_GAME_DATA";
    }
}
