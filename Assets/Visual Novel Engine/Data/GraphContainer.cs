using System.Collections.Generic;
using UnityEngine;

namespace VisualNovelEngine.Data
{
    public class GraphContainer : ScriptableObject
    {
        public List<GraphNodeData> Nodes = new();
        public List<NodeLinkData> Links = new();
        public List<GraphGroupData> Groups = new();
        public List<GraphFlagData> Flags = new();
    }
}
