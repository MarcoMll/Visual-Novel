using System;
using System.Collections.Generic;
using UnityEngine;

namespace VisualNovelEngine.Data
{
    [Serializable]
    public class GraphNodePropertyData
    {
        // Stores serialized information about a node member (field or property).
        public string Name;
        public string Type;
        public string JsonValue;
    }

    [Serializable]
    public class GraphNodeData
    {
        public string GUID;
        public string Type;
        public Vector2 Position;
        public List<GraphNodePropertyData> Properties = new();
    }
}
