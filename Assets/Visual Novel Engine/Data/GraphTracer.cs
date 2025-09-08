using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VisualNovelEngine.Data
{
    /// <summary>
    /// Runtime utility used to navigate a saved graph.
    /// Allows selecting a node and querying its connections.
    /// </summary>
    public class GraphTracer
    {
        private readonly Dictionary<string, GraphNodeData> _nodesByGuid;
        private readonly Dictionary<string, List<NodeLinkData>> _linksBySource;

        /// <summary>
        /// Currently selected node within the graph.
        /// </summary>
        public GraphNodeData ActiveNode { get; private set; }

        public GraphTracer(GraphContainer container)
        {
            _nodesByGuid = container.Nodes.ToDictionary(n => n.GUID);
            _linksBySource = container.Links
                .GroupBy(l => l.BaseNodeGUID)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        /// <summary>
        /// Selects a node by GUID and returns its data if found.
        /// </summary>
        public GraphNodeData SelectNode(string nodeGuid)
        {
            _nodesByGuid.TryGetValue(nodeGuid, out var node);
            ActiveNode = node;
            return ActiveNode;
        }

        /// <summary>
        /// Returns all nodes directly connected to the active node.
        /// </summary>
        public List<GraphNodeData> GetConnectedNodes()
        {
            return ActiveNode == null ? new List<GraphNodeData>() : GetConnectedNodes(ActiveNode.GUID);
        }

        /// <summary>
        /// Returns all nodes that the specified node connects to.
        /// </summary>
        public List<GraphNodeData> GetConnectedNodes(string nodeGuid)
        {
            if (!_linksBySource.TryGetValue(nodeGuid, out var links))
                return new List<GraphNodeData>();

            var result = new List<GraphNodeData>();
            foreach (var link in links)
            {
                if (_nodesByGuid.TryGetValue(link.TargetNodeGUID, out var node))
                {
                    result.Add(node);
                }
            }
            return result;
        }

        /// <summary>
        /// Returns all nodes connected to the output ports of the given node.
        /// </summary>
        /// <param name="targetNode">The node whose outputs will be inspected.</param>
        public List<GraphNodeData> GetAllConnectedNodes(GraphNodeData targetNode)
        {
            return targetNode == null ? new List<GraphNodeData>() : GetConnectedNodes(targetNode.GUID);
        }

        /// Finds the special start node and returns all nodes linked from it.
        /// The first connected node becomes the active node.
        /// </summary>
        public List<GraphNodeData> GetAdjacentNodesFromStart()
        {
            var startNode = _nodesByGuid.Values
                .FirstOrDefault(n => n.Type != null && n.Type.Contains("StartNode"));

            if (startNode == null)
                return new List<GraphNodeData>();

            var connectedNodes = GetConnectedNodes(startNode.GUID);
            ActiveNode = connectedNodes.FirstOrDefault();
            return connectedNodes;
        }

        /// <summary>
        /// Attempts to get a node's data by its GUID.
        /// </summary>
        public bool TryGetNode(string nodeGuid, out GraphNodeData node)
        {
            return _nodesByGuid.TryGetValue(nodeGuid, out node);
        }

        /// <summary>
        /// Instantiates the provided node's concrete type and populates its public
        /// properties with the values saved in the graph. This allows accessing
        /// user-set variables directly, e.g. <c>textNode.Text</c> or
        /// <c>sceneNode.Scene</c>.
        /// </summary>
        /// <param name="node">Serialized node data.</param>
        /// <returns>An instance of the node's specific type with its properties filled in.</returns>
        public object GetNodeType(GraphNodeData node)
        {
            if (node == null || string.IsNullOrEmpty(node.Type))
                return null;

            var type = Type.GetType(node.Type);
            if (type == null)
                return null;

            var instance = Activator.CreateInstance(type);

            // Ensure instantiated nodes know their GUID so downstream systems
            // can query graph connections using this identifier.
            var guidProp = type.GetProperty("GUID");
            if (guidProp != null && guidProp.CanWrite)
                guidProp.SetValue(instance, node.GUID);

            foreach (var prop in node.Properties)
            {
                var propInfo = type.GetProperty(prop.Name);
                if (propInfo == null)
                    continue;

                var propType = Type.GetType(prop.Type);
                if (propType == null)
                    continue;

                var value = DeserializeValue(prop.JsonValue, propType);
                propInfo.SetValue(instance, value);
            }

            return instance;
        }

        /// <summary>
        /// Returns a dictionary of property name to value for the active node.
        /// </summary>
        public Dictionary<string, object> GetNodeProperties()
        {
            return ActiveNode == null ? new() : GetNodeProperties(ActiveNode.GUID);
        }

        /// <summary>
        /// Returns a dictionary of property name to value for the specified node.
        /// </summary>
        public Dictionary<string, object> GetNodeProperties(string nodeGuid)
        {
            if (!_nodesByGuid.TryGetValue(nodeGuid, out var node))
                return new();

            var result = new Dictionary<string, object>();
            foreach (var prop in node.Properties)
            {
                var type = Type.GetType(prop.Type);
                if (type == null) continue;
                result[prop.Name] = DeserializeValue(prop.JsonValue, type);
            }
            return result;
        }

        /// <summary>
        /// Attempts to get a typed property value from the active node.
        /// </summary>
        public bool TryGetProperty<T>(string propertyName, out T value)
        {
            value = default;
            return ActiveNode != null && TryGetProperty(ActiveNode.GUID, propertyName, out value);
        }

        /// <summary>
        /// Attempts to get a typed property value from a specific node.
        /// </summary>
        public bool TryGetProperty<T>(string nodeGuid, string propertyName, out T value)
        {
            value = default;

            if (!_nodesByGuid.TryGetValue(nodeGuid, out var node))
                return false;

            var prop = node.Properties.FirstOrDefault(p => p.Name == propertyName);
            if (prop == null)
                return false;

            var type = Type.GetType(prop.Type);
            if (type == null)
                return false;

            var obj = DeserializeValue(prop.JsonValue, type);
            if (obj is T castValue)
            {
                value = castValue;
                return true;
            }

            return false;
        }

        private static object DeserializeValue(string json, Type type)
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
#if UNITY_EDITOR
                if (!string.IsNullOrEmpty(json))
                {
                    var path = AssetDatabase.GUIDToAssetPath(json);
                    return AssetDatabase.LoadAssetAtPath(path, type);
                }
#endif
                return null;
            }

            var wrapperType = typeof(ValueWrapper<>).MakeGenericType(type);
            var wrapper = Activator.CreateInstance(wrapperType);

#if UNITY_EDITOR
            // Use EditorJsonUtility so nested UnityEngine.Object references
            // (e.g., ScriptableObjects within lists) are correctly restored.
            try
            {
                EditorJsonUtility.FromJsonOverwrite(json, wrapper);
            }
            catch
            {
                // Fallback to JsonUtility if EditorJsonUtility fails.
                JsonUtility.FromJsonOverwrite(json, wrapper);
            }
#else
            JsonUtility.FromJsonOverwrite(json, wrapper);
#endif

            return wrapperType.GetField("Value").GetValue(wrapper);
        }

        [Serializable]
        private class ValueWrapper<T>
        {
            public T Value;
        }
    }
}

