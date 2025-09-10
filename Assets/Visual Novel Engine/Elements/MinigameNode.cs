using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace VisualNovelEngine.Elements
{
    using Utilities;
    using Utilities.Attributes;
    using VisualNovel.Minigames;

    [Serializable]
    [NodeMenu("Minigame Node", "Logic & Flow")]
    public class MinigameNode : GraphNode
    {
        /// <summary>Prefab containing the minigame to launch.</summary>
        public MinigameBase MinigamePrefab { get; set; }

        public override void Initialize(Vector2 gridPosition, string nodeGuid = null)
        {
            base.Initialize(gridPosition, nodeGuid);

            NodeName = "Minigame Node";
            Text = string.Empty;
            Choices.Clear();
            Choices.Add("onSuccess");
            Choices.Add("onFail");
        }

        public override void Draw()
        {
            // ----- Title -----
            var nodeNameTextField = UIElementUtilities.CreateTextField(NodeName);
            nodeNameTextField.AddClasses("gp-node__text-field", "gp-node__filename-text-field", "gp-node__text-field__hidden");
            nodeNameTextField.RegisterValueChangedCallback(evt => NodeName = evt.newValue);
            titleContainer.Insert(0, nodeNameTextField);
            titleContainer.Q<TextField>()?.SetEnabled(false);

            // ----- Ports -----
            var inputPort = this.CreatePort("Input Link", direction: Direction.Input, capacity: Port.Capacity.Multi);
            inputContainer.Insert(0, inputPort);

            foreach (var choice in Choices)
            {
                var choicePort = this.CreatePort(choice, capacity: Port.Capacity.Multi);
                choicePort.portName = choice;
                outputContainer.Add(choicePort);
            }

            // ----- Body -----
            var field = new ObjectField("Minigame Prefab")
            {
                objectType = typeof(MinigameBase),
                allowSceneObjects = false,
                value = MinigamePrefab
            };
            field.RegisterValueChangedCallback(evt => MinigamePrefab = evt.newValue as MinigameBase);
            extensionContainer.Add(field);

            RefreshExpandedState();
            RefreshPorts();
        }
    }
}

