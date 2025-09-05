using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using VisualNovelEngine.Core;
using VisualNovelEngine.Elements;

namespace VisualNovelEngine.Data.Utilities
{
    public static class GraphSaveUtility
    {
        public static void SaveGraph(NodeGraphView graphView, string path)
        {
            var container = ScriptableObject.CreateInstance<GraphContainer>();

            foreach (var edge in graphView.edges.ToList())
            {
                if (edge.input == null || edge.output == null) continue;

                var outputNode = edge.output.node as GraphNode;
                var inputNode = edge.input.node as GraphNode;

                var outputPorts = outputNode.outputContainer.Children().OfType<Port>().ToList();
                var outputIndex = outputPorts.IndexOf(edge.output);

                container.Links.Add(new NodeLinkData
                {
                    BaseNodeGUID = outputNode.GUID,
                    PortName = edge.output.portName,
                    OutputPortIndex = outputIndex,
                    TargetNodeGUID = inputNode.GUID
                });
            }

            foreach (var node in graphView.nodes.ToList().OfType<GraphNode>())
            {
                var nodeData = new GraphNodeData
                {
                    GUID = node.GUID,
                    Type = node.GetType().AssemblyQualifiedName,
                    Position = node.GetPosition().position
                };

                foreach (var prop in node.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (!prop.CanRead || !prop.CanWrite || prop.Name == nameof(GraphNode.GUID))
                        continue;

                    if (!typeof(GraphNode).IsAssignableFrom(prop.DeclaringType))
                        continue;

                    var value = prop.GetValue(node);
                    var json = SerializeValue(value, prop.PropertyType);
                    nodeData.Properties.Add(new GraphNodePropertyData
                    {
                        Name = prop.Name,
                        Type = prop.PropertyType.AssemblyQualifiedName,
                        JsonValue = json
                    });
                }

                container.Nodes.Add(nodeData);
            }

            foreach (var group in graphView.graphElements.ToList().OfType<Group>())
            {
                var rect = group.GetPosition();
                var groupData = new GraphGroupData
                {
                    Title = group.title,
                    Position = rect.position,
                    Size = rect.size,
                    Nodes = group.containedElements
                        .OfType<GraphNode>()
                        .Select(n => n.GUID)
                        .ToList()
                };

                container.Groups.Add(groupData);
            }

            foreach (var flag in graphView.Flags)
            {
                container.Flags.Add(new GraphFlagData
                {
                    Name = flag.Name,
                    Type = flag.Type.AssemblyQualifiedName,
                    JsonValue = SerializeValue(flag.Value, flag.Type)
                });
            }

            var existingContainer = AssetDatabase.LoadAssetAtPath<GraphContainer>(path);

            if (existingContainer == null)
            {
                AssetDatabase.CreateAsset(container, path);
            }
            else
            {
                existingContainer.Nodes = container.Nodes;
                existingContainer.Links = container.Links;
                existingContainer.Groups = container.Groups;
                existingContainer.Flags = container.Flags;

                EditorUtility.SetDirty(existingContainer);
            }

            AssetDatabase.SaveAssets();
        }

        public static void LoadGraph(NodeGraphView graphView, string path)
        {
            var container = AssetDatabase.LoadAssetAtPath<GraphContainer>(path);
            if (container == null) return;

            foreach (var edge in graphView.edges.ToList())
            {
                graphView.RemoveElement(edge);
            }

            foreach (var group in graphView.graphElements.ToList().OfType<Group>())
            {
                graphView.RemoveElement(group);
            }

            foreach (var node in graphView.nodes.ToList())
            {
                graphView.RemoveElement(node);
            }

            graphView.ClearFlags();

            // Load flags before nodes so that nodes depending on them
            // can properly initialize their fields during creation.
            foreach (var flagData in container.Flags)
            {
                var type = Type.GetType(flagData.Type);
                var value = DeserializeValue(flagData.JsonValue, type);
                graphView.AddFlag(type, flagData.Name, value, false);
            }

            var nodeLookup = new Dictionary<string, GraphNode>();

            foreach (var nodeData in container.Nodes)
            {
                var nodeType = Type.GetType(nodeData.Type);
                var node = graphView.CreateNode(nodeType, nodeData.Position, nodeData.Properties, nodeData.GUID);
                nodeLookup.Add(nodeData.GUID, node);
            }

            foreach (var groupData in container.Groups)
            {
                var group = new Group { title = groupData.Title };
                group.SetPosition(new Rect(groupData.Position, groupData.Size));

                graphView.AddElement(group);

                foreach (var nodeGuid in groupData.Nodes)
                {
                    if (nodeLookup.TryGetValue(nodeGuid, out var node))
                    {
                        group.AddElement(node);
                    }
                }
            }

            foreach (var link in container.Links)
            {
                if (!nodeLookup.ContainsKey(link.BaseNodeGUID) || !nodeLookup.ContainsKey(link.TargetNodeGUID))
                    continue;

                var baseNode = nodeLookup[link.BaseNodeGUID];
                var targetNode = nodeLookup[link.TargetNodeGUID];

                var outputPorts = baseNode.outputContainer.Children().OfType<Port>().ToList();

                Port outputPort;
                if (link.OutputPortIndex >= 0 && link.OutputPortIndex < outputPorts.Count)
                {
                    outputPort = outputPorts[link.OutputPortIndex];
                }
                else
                {
                    outputPort = outputPorts.First(p => p.portName == link.PortName);
                }

                var inputPort = (Port)targetNode.inputContainer.Children().First();

                var edge = outputPort.ConnectTo(inputPort);
                graphView.AddElement(edge);
            }
        }

        private static string SerializeValue(object value, Type type)
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                var obj = value as UnityEngine.Object;
                if (obj == null) return string.Empty;

                var path = AssetDatabase.GetAssetPath(obj);
                return AssetDatabase.AssetPathToGUID(path);
            }

            var wrapperType = typeof(ValueWrapper<>).MakeGenericType(type);
            var wrapper = Activator.CreateInstance(wrapperType);
            wrapperType.GetField("Value").SetValue(wrapper, value);
            return JsonUtility.ToJson(wrapper);
        }

        private static object DeserializeValue(string json, Type type)
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                if (string.IsNullOrEmpty(json)) return null;

                var path = AssetDatabase.GUIDToAssetPath(json);
                return AssetDatabase.LoadAssetAtPath(path, type);
            }

            var wrapperType = typeof(ValueWrapper<>).MakeGenericType(type);
            var wrapper = Activator.CreateInstance(wrapperType);

            try
            {
                JsonUtility.FromJsonOverwrite(json, wrapper);
            }
            catch
            {
                // ignore - wrapper will retain default value
            }

            return wrapperType.GetField("Value").GetValue(wrapper);
        }

        [Serializable]
        private class ValueWrapper<T>
        {
            public T Value;
        }
    }
}
