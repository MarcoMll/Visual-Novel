using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace VisualNovelEngine.Elements
{
    using Utilities;
    using Utilities.Attributes;
    
    [Serializable]
    [NodeMenu("Choice Node", "Narrative")]
    public class ChoiceNode : GraphNode
    {
        public override void Initialize(Vector2 gridPosition, string nodeGuid = null)
        {
            base.Initialize(gridPosition, nodeGuid);

            NodeName = "Choice Node";
            Text = "Input text.";
            Choices.Add("Output Link");
        }

        public override void Draw()
        {
            // title container
            var nodeNameTextField = UIElementUtilities.CreateTextField(NodeName);
            nodeNameTextField.AddClasses("gp-node__text-field", "gp-node__filename-text-field", "gp-node__text-field__hidden");
            nodeNameTextField.RegisterValueChangedCallback(evt => NodeName = evt.newValue);
            titleContainer.Insert(0, nodeNameTextField);
            titleContainer.Q<TextField>().SetEnabled(false);
            
            SetTitleIcon(GUIUtilities.GetIconByName(EditorConstants.ChoiceIcon));
            
            // input container
            var inputPort = this.CreatePort("Input Link", direction: Direction.Input, capacity: Port.Capacity.Multi);
            inputContainer.Insert(0, inputPort);
            
            // text field
            var textLabel = new Label("Choice text");
            textLabel.AddToClassList("gp-node__label");

            var textField = UIElementUtilities.CreateTextArea(Text);
            textField.AddClasses("gp-node__text-field", "gp-node__quote-text-field");
            textField.RegisterValueChangedCallback(evt => Text = evt.newValue);
            
            extensionContainer.Add(textLabel);
            extensionContainer.Add(textField);
            
            foreach (var choice in Choices)
            {
                var choicePort = this.CreatePort(choice, capacity: Port.Capacity.Multi);

                choicePort.portName = choice;
                outputContainer.Add(choicePort);
            }
            
            RefreshExpandedState();
        }
    }
}