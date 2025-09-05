using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace VisualNovelEngine.Elements
{
    using Utilities;
    
    [Serializable]
    public class StartNode : GraphNode
    {
        public override void Initialize(Vector2 gridPosition, string nodeGuid = null)
        {
            base.Initialize(gridPosition, nodeGuid);
            Choices.Add("Output Link");
            NodeName = "START NODE";

            ColorUtility.TryParseHtmlString("#639182", out var titleContainerColor);
            SetTitleContainerColor(titleContainerColor);
            
            capabilities &= ~Capabilities.Deletable; // making the node impossible to remove
        }

        public override void Draw()
        {
            var nodeNameTextField = UIElementUtilities.CreateTextField(NodeName);
            nodeNameTextField.AddClasses("gp-node__text-field", "gp-node__filename-text-field", "gp-node__text-field__hidden");
            nodeNameTextField.RegisterValueChangedCallback(evt => NodeName = evt.newValue);

            titleContainer.Insert(0, nodeNameTextField);
            
            titleContainer.Q<TextField>().SetEnabled(false);
            
            foreach (var choice in Choices)
            {
                var choicePort = this.CreatePort(choice, capacity: Port.Capacity.Multi);

                choicePort.portName = choice;
                outputContainer.Add(choicePort);
            }        
        }
    }

}