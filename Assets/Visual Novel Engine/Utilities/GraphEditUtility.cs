using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace VisualNovelEngine.Utilities
{
    using Core;
    using Elements;
    using Data;
    using Data.Utilities;

    public class GraphEditUtility
    {
        private readonly NodeGraphView _graphView;
        private readonly Stack<Action> _undoStack = new();
        private readonly List<GraphNodeData> _copiedNodes = new();
        private bool _isUndoOperation = false;

        public GraphEditUtility(NodeGraphView graphView)
        {
            _graphView = graphView;
        }

        public void RegisterCallbacks()
        {
            _graphView.RegisterCallback<KeyDownEvent>(OnKeyDown);
            _graphView.graphViewChanged = OnGraphViewChanged;
        }

        public void PushUndo(Action action)
        {
            if (_isUndoOperation) return;
            _undoStack.Push(action);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (!evt.ctrlKey && !evt.commandKey)
                return;

            switch (evt.keyCode)
            {
                case KeyCode.C:
                    CopySelection();
                    evt.StopImmediatePropagation();
                    break;
                case KeyCode.V:
                    PasteSelection();
                    evt.StopImmediatePropagation();
                    break;
                case KeyCode.Z:
                    UndoLastAction();
                    evt.StopImmediatePropagation();
                    break;
            }
        }

        private void CopySelection()
        {
            _copiedNodes.Clear();
            foreach (var node in _graphView.selection.OfType<GraphNode>())
            {
                _copiedNodes.Add(SerializeNode(node));
            }
        }

        private void PasteSelection()
        {
            if (_copiedNodes.Count == 0) return;

            foreach (var data in _copiedNodes)
            {
                var type = Type.GetType(data.Type);
                var position = data.Position + new Vector2(30f, 30f);
                _graphView.CreateNode(type, position, data.Properties);
            }
        }

        private void UndoLastAction()
        {
            if (_undoStack.Count == 0) return;
            _isUndoOperation = true;
            var action = _undoStack.Pop();
            action.Invoke();
            _isUndoOperation = false;
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (_isUndoOperation)
                return change;

            if (change.edgesToCreate != null && change.edgesToCreate.Count > 0)
            {
                var edges = change.edgesToCreate.ToList();
                PushUndo(() =>
                {
                    foreach (var edge in edges)
                    {
                        edge.output.Disconnect(edge);
                        edge.input.Disconnect(edge);
                        _graphView.RemoveElement(edge);
                    }
                });
            }

            if (change.elementsToRemove != null && change.elementsToRemove.Count > 0)
            {
                var removedNodes = change.elementsToRemove.OfType<GraphNode>().ToList();
                var removedEdges = change.elementsToRemove.OfType<Edge>().ToList();
                var removedGroups = change.elementsToRemove.OfType<Group>().ToList();

                var nodeData = removedNodes.Select(n => SerializeNode(n)).ToList();
                var edgeData = removedEdges.Select(e =>
                {
                    var outputNode = e.output.node as GraphNode;
                    var ports = outputNode.outputContainer.Children().OfType<Port>().ToList();
                    return new NodeLinkData
                    {
                        BaseNodeGUID = outputNode.GUID,
                        TargetNodeGUID = (e.input.node as GraphNode).GUID,
                        OutputPortIndex = ports.IndexOf(e.output),
                        PortName = e.output.portName
                    };
                }).ToList();

                var groupData = removedGroups.Select(g => new
                {
                    g.title,
                    Rect = g.GetPosition(),
                    Nodes = g.containedElements.OfType<GraphNode>().Select(n => n.GUID).ToList()
                }).ToList();

                PushUndo(() =>
                {
                    var guidToNode = new Dictionary<string, GraphNode>();
                    foreach (var n in nodeData)
                    {
                        var type = Type.GetType(n.Type);
                        var node = _graphView.CreateNode(type, n.Position, n.Properties, n.GUID);
                        guidToNode[n.GUID] = node;
                    }

                    foreach (var g in groupData)
                    {
                        var group = _graphView.CreateGroup(g.title, g.Rect.position);
                        group.SetPosition(g.Rect);
                        _graphView.AddElement(group);
                        foreach (var guid in g.Nodes)
                        {
                            var node = _graphView.GetNodeByGuid(guid);
                            if (node != null)
                                group.AddElement(node);
                        }
                    }

                    foreach (var e in edgeData)
                    {
                        var baseNode = _graphView.GetNodeByGuid(e.BaseNodeGUID);
                        var targetNode = _graphView.GetNodeByGuid(e.TargetNodeGUID);
                        if (baseNode == null || targetNode == null)
                            continue;

                        var outputs = baseNode.outputContainer.Children().OfType<Port>().ToList();
                        Port outputPort = null;
                        if (e.OutputPortIndex >= 0 && e.OutputPortIndex < outputs.Count)
                            outputPort = outputs[e.OutputPortIndex];
                        else
                            outputPort = outputs.FirstOrDefault(p => p.portName == e.PortName);
                        var inputPort = (Port)targetNode.inputContainer.Children().FirstOrDefault();
                        if (outputPort != null && inputPort != null)
                        {
                            var edge = outputPort.ConnectTo(inputPort);
                            _graphView.AddElement(edge);
                        }
                    }
                });
            }

            return change;
        }

        private GraphNodeData SerializeNode(GraphNode node)
        {
            var data = new GraphNodeData
            {
                GUID = node.GUID,
                Type = node.GetType().AssemblyQualifiedName,
                Position = node.GetPosition().position,
                Properties = new List<GraphNodePropertyData>()
            };

            var nodeType = node.GetType();
            foreach (var prop in nodeType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!prop.CanRead || !prop.CanWrite || prop.Name == nameof(GraphNode.GUID))
                    continue;
                if (!typeof(GraphNode).IsAssignableFrom(prop.DeclaringType))
                    continue;
                var value = prop.GetValue(node);
                var json = SerializeValue(value, prop.PropertyType);
                data.Properties.Add(new GraphNodePropertyData
                {
                    Name = prop.Name,
                    Type = prop.PropertyType.AssemblyQualifiedName,
                    JsonValue = json
                });
            }

            foreach (var field in nodeType.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!typeof(GraphNode).IsAssignableFrom(field.DeclaringType))
                    continue;
                var value = field.GetValue(node);
                var json = SerializeValue(value, field.FieldType);
                data.Properties.Add(new GraphNodePropertyData
                {
                    Name = field.Name,
                    Type = field.FieldType.AssemblyQualifiedName,
                    JsonValue = json
                });
            }

            return data;
        }

        public static object DeserializeValue(string json, Type type)
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

        public static string SerializeValue(object value, Type type)
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

        [Serializable]
        private class ValueWrapper<T>
        {
            public T Value;
        }
    }
}
