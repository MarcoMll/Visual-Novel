using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace VisualNovelEngine.Elements
{
    using Utilities;

    [Serializable]
    public class GraphNode : Node
    {
        public string GUID { get; set; }
        public string NodeName { get; set; }
        public string Text { get; set; }
        public List<string> Choices { get; set; }

        public virtual void Initialize(Vector2 gridPosition, string nodeGuid = null)
        {
            NodeName = "NodeName";
            Choices = new List<string>();
            Text = "Node text.";

            GUID = nodeGuid ?? System.Guid.NewGuid().ToString();
            viewDataKey = GUID;

            SetPosition(new Rect(gridPosition, Vector2.zero));

            mainContainer.AddToClassList("gp-node__main-container");
            extensionContainer.AddToClassList("gp-node__extension-container");
        }

        public virtual void Draw()
        {
            // title container
            var nodeNameTextField = UIElementUtilities.CreateTextField(NodeName);
            nodeNameTextField.AddClasses("gp-node__text-field", "gp-node__filename-text-field", "gp-node__text-field__hidden");
            nodeNameTextField.RegisterValueChangedCallback(evt => NodeName = evt.newValue);

            titleContainer.Insert(0, nodeNameTextField);

            // input container
            var inputPort = this.CreatePort("Node connection", direction: Direction.Input, capacity: Port.Capacity.Multi);

            inputContainer.Insert(0, inputPort);

            // extension container
            var customDataContainer = new VisualElement();

            customDataContainer.AddToClassList("gp-node__custom-data-container");

            var textFoldout = UIElementUtilities.CreateFoldout("Choices container");

            var textField = UIElementUtilities.CreateTextArea(Text);
            textField.AddClasses("gp-node__text-field", "gp-node__quote-text-field");
            textField.RegisterValueChangedCallback(evt => Text = evt.newValue);

            textFoldout.Add(textField);

            customDataContainer.Add(textFoldout);
            extensionContainer.Add(customDataContainer);
        }

        protected void DrawOutputPortsForAllChoices()
        {
            foreach (var choice in Choices)
            {
                var choicePort = this.CreatePort(choice, capacity: Port.Capacity.Multi);

                choicePort.portName = choice;
                outputContainer.Add(choicePort);
            }
        }
        
        protected void SetTitleContainerColor(Color color)
        {
            // Ensure the titleContainer has a background style
            titleContainer.style.backgroundColor = new StyleColor(color);
        }
        
        /// <summary>
        /// Inserts an icon into the node's title container, positioning it before the title text.
        /// </summary>
        protected void SetTitleIcon(Texture2D iconTexture, int size = 16)
        {
            if (iconTexture == null) return;

            // Ensure row layout & proper vertical alignment
            titleContainer.style.flexDirection = FlexDirection.Row;
            titleContainer.style.alignItems    = Align.Center;

            var icon = new Image
            {
                image     = iconTexture,
                scaleMode = ScaleMode.ScaleToFit,
                style =
                {
                    width = size,
                    height = size,
                    marginRight = 0,
                    marginLeft = 6,
                    flexShrink = 0,
                    alignSelf = Align.Center
                }
            };

            // Find the thing you're actually showing as the title (your TextField),
            // otherwise fall back to the built-in title label.
            var titleElement =
                titleContainer.Q<TextField>() as VisualElement ??
                titleContainer.Q<Label>("title-label");

            var insertIndex = titleElement != null
                ? titleContainer.IndexOf(titleElement)
                : 0;

            // Insert directly before the title element -> [ICON] Title
            titleContainer.Insert(insertIndex, icon);
        }

    }
}