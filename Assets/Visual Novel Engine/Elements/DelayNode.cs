using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace VisualNovelEngine.Elements
{
    using Utilities;
    using Utilities.Attributes;

    [Serializable]
    [NodeMenu("Delay Node", "Utilities")]
    public class DelayNode : GraphNode
    {
        public float Delay { get; set; } = 1f;

        public override void Initialize(Vector2 gridPosition, string nodeGuid = null)
        {
            base.Initialize(gridPosition, nodeGuid);

            NodeName = "Delay Node";
            if (!Choices.Contains("Output Link"))
                Choices.Add("Output Link");
        }

        public override void Draw()
        {
            // ----- Title -----
            var nodeNameTextField = UIElementUtilities.CreateTextField(NodeName);
            nodeNameTextField.AddClasses("gp-node__text-field", "gp-node__filename-text-field", "gp-node__text-field__hidden");
            nodeNameTextField.RegisterValueChangedCallback(evt => NodeName = evt.newValue);
            titleContainer.Insert(0, nodeNameTextField);
            titleContainer.Q<TextField>()?.SetEnabled(false);

            // Icon
            SetTitleIcon(GUIUtilities.GetIconByName(EditorConstants.DelayIcon));

            // ----- Ports -----
            var inputPort = this.CreatePort("Input Link", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputContainer.Insert(0, inputPort);

            foreach (var choice in Choices)
            {
                var choicePort = this.CreatePort(choice, capacity: Port.Capacity.Multi);
                choicePort.portName = choice;
                outputContainer.Add(choicePort);
            }

            // ----- Body -----
            var label = new Label("Delay (in seconds)");
            label.AddToClassList("gp-node__label");
            extensionContainer.Add(label);

            var floatField = new FloatField { value = Delay };
            floatField.RegisterValueChangedCallback(evt => Delay = Mathf.Max(0f, evt.newValue));
            floatField.style.flexGrow = 1;
            extensionContainer.Add(floatField);

            RefreshExpandedState();
            RefreshPorts();
        }
    }
}