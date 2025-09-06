using System;
using System.Collections.Generic;

namespace VisualNovel.GameFlow.SaveSystem
{
    /// <summary>
    /// Stores graph progression data such as current node and flags.
    /// </summary>
    [Serializable]
    public class GraphGameData : BaseGameData
    {
        public string CurrentNodeGuid;
        public Dictionary<string, bool> Flags = new();

        protected override string SaveKey => "GRAPH_GAME_DATA";
    }
}
