using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace VisualNovelEngine.Core
{
    using Elements;
    using Windows;
    using Utilities;
    using Utilities.Attributes;
    using Data;
    using Data.Utilities;
    
    public class NodeGraphView : GraphView
    {
        private NodeEditorWindow _editorWindow;
        private NodeSearchWindow _searchWindowProvider;
        private GraphNodeBlackboard _blackboardProvider;
        private GraphEditUtility _editUtility;
        private MiniMap _miniMap;
        public List<GraphFlag> Flags => _blackboardProvider.Flags;

        public NodeGraphView(NodeEditorWindow nodeEditorWindow)
        {
            _editorWindow = nodeEditorWindow;

            DrawGridBackground();
            AddManipulators();
            AddMiniMap();
            SetupBlackboard();
            SetupSearchWindow();
            _editUtility = new GraphEditUtility(this);
            _editUtility.RegisterCallbacks();
            LoadStyles();
            InitializeStartNode();
        }

        private void InitializeStartNode()
        {
            CreateNode(typeof(StartNode), Vector2.zero);
        }
        
        private void LoadStyles()
        {
            this.AddStyleSheets( EditorConstants.GraphViewStyleSheet, EditorConstants.GraphNodeStyleSheet );
        }

        private void SetupSearchWindow()
        {
            // create the provider instance and give it a reference back to this view
            _searchWindowProvider = ScriptableObject.CreateInstance<NodeSearchWindow>();
            _searchWindowProvider.Initialize(this);

            nodeCreationRequest = ctx =>
                SearchWindow.Open(new SearchWindowContext(ctx.screenMousePosition),
                    _searchWindowProvider);
        }
        
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort == port)
                {
                    return;
                }

                if (startPort.node == port.node)
                {
                    return;
                }

                if (startPort.direction == port.direction)
                {
                    return;
                }

                // Determine which port is output and which is input
                Port outputPort = startPort.direction == Direction.Output ? startPort : port;
                Port inputPort  = startPort.direction == Direction.Output ? port      : startPort;

                // Apply connection rules; skip port if not allowed
                if (!NodeConnectionRules.CanConnect(outputPort, inputPort))
                {
                    return;
                }

                compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }
        
        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.AddManipulator(new ContextualMenuManipulator(CreateNodeContextualMenu));
            this.AddManipulator(CreateGroupContextualMenu());
        }

        private void AddMiniMap()
        {
            _miniMap = new MiniMap { anchored = true };
            _miniMap.SetPosition(new Rect(1670, 50, 200, 140));
            Add(_miniMap);
        }

        private IManipulator CreateGroupContextualMenu()
        {
            var contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Create Group", actionEvent =>
                {
                    var group = CreateGroup("Nodes Group", GetLocalMousePosition(actionEvent.eventInfo.localMousePosition));
                    AddElement(group);
                    _editUtility.PushUndo(() => RemoveElement(group));
                }));
            return contextualMenuManipulator;
        }

        private void CreateNodeContextualMenu(ContextualMenuPopulateEvent populateEvent)
        {
            // find all non-abstract subclasses of GraphNode
            var nodeTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(GraphNode)));

            foreach (var nodeType in nodeTypes)
            {
                // check if nodeType has the NodeMenu attribute
                var attribute = nodeType.GetCustomAttribute<NodeMenuAttribute>();
                if (attribute == null)
                    continue;
                
                populateEvent.menu.AppendAction(attribute.MenuPath, action => CreateNode(nodeType, GetLocalMousePosition(action.eventInfo.localMousePosition)));
            }
        }

        private void SetupBlackboard()
        {
            _blackboardProvider = ScriptableObject.CreateInstance<GraphNodeBlackboard>();
            _blackboardProvider.Initialize(this);
            Add(_blackboardProvider.Blackboard);
        }

        internal Group CreateGroup(string groupTitle, Vector2 gridPosition)
        {
            var group = new Group()
            {
                title = groupTitle
            };
            
            group.SetPosition(new Rect(gridPosition, Vector2.zero));
            return group;
        }

        #region Blackboard

        public void AddFlag(Type type, string name = "New Flag", object value = null, bool notify = true)
        {
            _blackboardProvider.AddFlag(type, name, value, notify);
        }

        public void ClearFlags()
        {
            _blackboardProvider.ClearFlags();
        }

        internal void OnFlagsChanged()
        {
            var path = _editorWindow.CurrentAssetPath;
            if (!string.IsNullOrEmpty(path))
            {
                GraphSaveUtility.SaveGraph(this, path);
            }
        }

        #endregion
        
        internal GraphNode CreateNode(Type nodeClass, Vector2 gridPosition, List<GraphNodePropertyData> properties = null, string nodeGuid = null)
        {
            var node = (GraphNode)Activator.CreateInstance(nodeClass);
            node.Initialize(gridPosition, nodeGuid);

            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    var memberType = Type.GetType(prop.Type);
                    if (memberType == null) continue;

                    var value = GraphEditUtility.DeserializeValue(prop.JsonValue, memberType);

                    var propertyInfo = nodeClass.GetProperty(prop.Name,
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

                    if (propertyInfo != null && propertyInfo.CanWrite &&
                        typeof(GraphNode).IsAssignableFrom(propertyInfo.DeclaringType))
                    {
                        propertyInfo.SetValue(node, value);
                    }
                    else
                    {
                        var fieldInfo = nodeClass.GetField(prop.Name,
                            BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

                        if (fieldInfo != null &&
                            typeof(GraphNode).IsAssignableFrom(fieldInfo.DeclaringType))
                        {
                            fieldInfo.SetValue(node, value);
                        }
                    }
                }
            }

            node.Draw();
            AddElement(node);
            _editUtility.PushUndo(() => RemoveElement(node));
            return node;
        }

        private void DrawGridBackground()
        {
            var gridBackground = new GridBackground();
            gridBackground.StretchToParentSize();
            
            Insert(0, gridBackground);
        }

        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
        {
            var worldMousePosition = mousePosition;

            if (isSearchWindow == true)
            {
                worldMousePosition -= _editorWindow.position.position; // _editorWindow.position is a rect so it's required to type .pos twice to get the Vector2
            }
            
            var localMousePosition = contentViewContainer.WorldToLocal(worldMousePosition);
            return localMousePosition;
        }

        internal GraphNode GetNodeByGuid(string guid)
        {
            return nodes.OfType<GraphNode>().FirstOrDefault(n => n.GUID == guid);
        }
    }
}