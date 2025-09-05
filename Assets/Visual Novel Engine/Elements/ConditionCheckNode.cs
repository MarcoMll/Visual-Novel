using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace VisualNovelEngine.Elements
{
    using Core;
    using Data;
    using Utilities;
    using Utilities.Attributes;

    [Serializable]
    [NodeMenu("Condition Node", "Logic & Flow")]
    public class ConditionCheckNode : GraphNode
    {
        public List<ItemData> Items { get; set; } = new();
        public List<TraitData> Traits { get; set; } = new();
        public List<FlagData> Flags { get; set; } = new();

        public override void Initialize(Vector2 gridPosition, string nodeGuid = null)
        {
            base.Initialize(gridPosition, nodeGuid);
            Choices.Add("Output Link");
            NodeName = "Condition Node";
        }

        public override void Draw()
        {
            // title container
            var nodeNameTextField = UIElementUtilities.CreateTextField(NodeName);
            nodeNameTextField.AddClasses("gp-node__text-field", "gp-node__filename-text-field", "gp-node__text-field__hidden");
            nodeNameTextField.RegisterValueChangedCallback(evt => NodeName = evt.newValue);

            titleContainer.Insert(0, nodeNameTextField);

            titleContainer.Q<TextField>().SetEnabled(false);

            // title icon
            SetTitleIcon(GUIUtilities.GetIconByName(EditorConstants.FlagIcon));

            // input container
            var inputPort = this.CreatePort("Input Link", direction: Direction.Input, capacity: Port.Capacity.Multi);
            inputContainer.Insert(0, inputPort);

            // item conditions
            var itemsFoldout = new Foldout { text = "Items" };
            var itemsContainer = new VisualElement();
            itemsFoldout.Add(itemsContainer);
            var addItemConditionButton = UIElementUtilities.CreateButton("Add Item", () =>
            {
                itemsContainer.Add(new ItemRow(this, Items));
            });
            addItemConditionButton.AddToClassList("gp-node__button");
            itemsFoldout.Add(addItemConditionButton);
            extensionContainer.Add(itemsFoldout);

            // characteristic conditions
            var characteristicsFoldout = new Foldout { text = "Characteristics" };
            var traitsContainer = new VisualElement();
            characteristicsFoldout.Add(traitsContainer);
            var addCharacteristicConditionButton = UIElementUtilities.CreateButton("Add Characteristic", () =>
            {
                traitsContainer.Add(new TraitRow(this, Traits));
            });
            addCharacteristicConditionButton.AddToClassList("gp-node__button");
            characteristicsFoldout.Add(addCharacteristicConditionButton);
            extensionContainer.Add(characteristicsFoldout);

            // flag conditions
            var flagsFoldout = new Foldout { text = "Flags" };
            var flagsContainer = new VisualElement();
            flagsFoldout.Add(flagsContainer);
            var addFlagConditionButton = UIElementUtilities.CreateButton("Add Flag", () =>
            {
                flagsContainer.Add(new FlagRow(this, Flags));
            });
            addFlagConditionButton.AddToClassList("gp-node__button");
            flagsFoldout.Add(addFlagConditionButton);
            extensionContainer.Add(flagsFoldout);

            // populate existing conditions after node is attached to the graph view
            schedule.Execute(() =>
            {
                foreach (var condition in Items.ToList())
                {
                    itemsContainer.Add(new ItemRow(this, Items, condition));
                }
                foreach (var condition in Traits.ToList())
                {
                    traitsContainer.Add(new TraitRow(this, Traits, condition));
                }
                foreach (var condition in Flags.ToList())
                {
                    flagsContainer.Add(new FlagRow(this, Flags, condition));
                }
                RefreshExpandedState();
            });

            // output container
            foreach (var choice in Choices)
            {
                var choicePort = this.CreatePort(choice, capacity: Port.Capacity.Single);

                choicePort.portName = choice;
                outputContainer.Add(choicePort);
            }
        }
    }
}
