using System;

namespace VisualNovelEngine.Data
{
    [Serializable]
    public class NodeLinkData
    {
        public string BaseNodeGUID;
        public string PortName;
        public int OutputPortIndex;
        public string TargetNodeGUID;
    }
}
