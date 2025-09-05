using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace VisualNovelEngine.Windows
{
    using Core;
    using Elements;
    using Utilities.Attributes;
    
    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private NodeGraphView _graphView;
        private Texture2D _indentationIcon;

        public void Initialize(NodeGraphView graphView)
        {
            _graphView = graphView;
            
            _indentationIcon = new Texture2D(1,1);
            _indentationIcon.SetPixel(0,0, Color.clear);
            _indentationIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                // search window title
                new SearchTreeGroupEntry(new GUIContent("Create Element"), 0)
            };

            // find all GraphNode subclasses grouped by their NodeMenu group
            var nodeTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(GraphNode)))
                .Select(t => new { Type = t, Attribute = t.GetCustomAttribute<NodeMenuAttribute>() })
                .Where(x => x.Attribute != null)
                .GroupBy(x => x.Attribute.Group)
                .OrderBy(g => g.Key);

            foreach (var group in nodeTypes)
            {
                tree.Add(new SearchTreeGroupEntry(new GUIContent(group.Key), 1));

                foreach (var entry in group.OrderBy(e => e.Attribute.Name))
                {
                    tree.Add(new SearchTreeEntry(new GUIContent(entry.Attribute.Name, _indentationIcon))
                    {
                        level = 2,
                        userData = entry.Type
                    });
                }
            }

            return tree;
        }

        // called when the user picks one of the entries
        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            var chosenType = entry.userData as Type;
            if (chosenType == null) return false;

            // convert screen to local graph coordinates
            var worldMousePos = context.screenMousePosition;
            var localMousePos = _graphView.GetLocalMousePosition(worldMousePos, true);
            
            // instantiating the node
            _graphView.CreateNode(chosenType, localMousePos);
            return true;
        }
    }
}