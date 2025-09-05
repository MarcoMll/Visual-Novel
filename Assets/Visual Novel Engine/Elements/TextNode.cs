using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace VisualNovelEngine.Elements
{
    using GameAssets.ScriptableObjects.Core;
    using Utilities;
    using Utilities.Attributes;
    
    [Serializable]
    [NodeMenu("Text Node", "Narrative")]
    public class TextNode : GraphNode
    {
        public CharacterSO Speaker { get; set; }
        public override void Initialize(Vector2 gridPosition, string nodeGuid = null)
        {
            base.Initialize(gridPosition, nodeGuid);

            NodeName = "Text Node";
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

            // set title icon after the title field has been added so the icon stays on the left
            var initialIcon = NodeName == "Dialogue Node" ? EditorConstants.DialogueIcon : EditorConstants.TextIcon;
            SetTitleIcon(GUIUtilities.GetIconByName(initialIcon));

            // input container
            var inputPort = this.CreatePort("Input Link", direction: Direction.Input, capacity: Port.Capacity.Multi);
            inputContainer.Insert(0, inputPort);

            // text field
            var textLabel = new Label("Text");
            textLabel.AddToClassList("gp-node__label");

            var textField = UIElementUtilities.CreateTextArea(Text);
            textField.AddClasses("gp-node__text-field", "gp-node__quote-text-field");
            textField.RegisterValueChangedCallback(evt => Text = evt.newValue);

            // speaker
            var speakerContainer = new VisualElement();
            speakerContainer.AddToClassList("gp-node__custom-data-container");

            var foldout = UIElementUtilities.CreateFoldout("Speaker");

            var speakerField = new ObjectField
            {
                objectType = typeof(CharacterSO),
                allowSceneObjects = false,
                value = Speaker
            };
            speakerField.style.flexGrow = 1;
            speakerField.style.flexShrink = 1;
            speakerField.style.minWidth = 0;
            speakerField.RegisterValueChangedCallback(evt => Speaker = evt.newValue as CharacterSO);
            foldout.Add(speakerField);

            speakerContainer.Add(foldout);

            extensionContainer.Add(textLabel);
            extensionContainer.Add(textField);
            extensionContainer.Add(UIElementUtilities.CreateSpace(6));

            // speaker toggle buttons
            var addSpeakerButton = UIElementUtilities.CreateButton("Add Speaker");
            var removeSpeakerButton = UIElementUtilities.CreateButton("Remove Speaker");

            addSpeakerButton.clicked += () =>
            {
                extensionContainer.Remove(addSpeakerButton);
                extensionContainer.Add(speakerContainer);
                extensionContainer.Add(removeSpeakerButton);

                NodeName = "Dialogue Node";
                var titleField = titleContainer.Q<TextField>();
                titleField?.SetValueWithoutNotify(NodeName);

                var oldIcon = titleContainer.Q<Image>();
                oldIcon?.RemoveFromHierarchy();
                SetTitleIcon(GUIUtilities.GetIconByName(EditorConstants.DialogueIcon));

                RefreshExpandedState();
            };

            removeSpeakerButton.clicked += () =>
            {
                extensionContainer.Remove(speakerContainer);
                extensionContainer.Remove(removeSpeakerButton);
                extensionContainer.Add(addSpeakerButton);

                Speaker = null;
                NodeName = "Text Node";
                var titleField = titleContainer.Q<TextField>();
                titleField?.SetValueWithoutNotify(NodeName);

                var oldIcon = titleContainer.Q<Image>();
                oldIcon?.RemoveFromHierarchy();
                SetTitleIcon(GUIUtilities.GetIconByName(EditorConstants.TextIcon));

                RefreshExpandedState();
            };

            if (Speaker != null || NodeName == "Dialogue Node")
            {
                extensionContainer.Add(speakerContainer);
                extensionContainer.Add(removeSpeakerButton);

                NodeName = "Dialogue Node";
                var titleField = titleContainer.Q<TextField>();
                titleField?.SetValueWithoutNotify(NodeName);

                var oldIcon = titleContainer.Q<Image>();
                oldIcon?.RemoveFromHierarchy();
                SetTitleIcon(GUIUtilities.GetIconByName(EditorConstants.DialogueIcon));
            }
            else
            {
                extensionContainer.Add(addSpeakerButton);
            }

            // output container
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