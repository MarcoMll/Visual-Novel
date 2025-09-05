using System;
using System.Collections.Generic;
using UnityEngine;

namespace VisualNovelEngine.Data
{
    [Serializable]
    public class GraphGroupData
    {
        public string Title;
        public Vector2 Position;
        public Vector2 Size;
        public List<string> Nodes = new();
    }
}
