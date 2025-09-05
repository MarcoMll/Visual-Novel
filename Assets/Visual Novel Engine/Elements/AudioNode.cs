using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VisualNovelEngine.Elements
{
    using Utilities;
    using Utilities.Attributes;

    [Serializable]
    [NodeMenu("Audio Node", "Utilities")]
    public class AudioNode : GraphNode
    {
        public enum AudioKind { None, Ambience, Music, Audio }

        public AudioKind Kind { get; set; } = AudioKind.None;
        public AudioClip AudioClip { get; set; }

        private DropdownField _kindDropdown;
        private VisualElement _clipRow;

        public override void Initialize(Vector2 gridPosition, string nodeGuid = null)
        {
            base.Initialize(gridPosition, nodeGuid);

            NodeName = "Audio Node";
            Choices.Clear(); // no output ports
        }

        public override void Draw()
        {
            // ----- Title -----
            var nodeNameTextField = UIElementUtilities.CreateTextField(NodeName);
            nodeNameTextField.AddClasses("gp-node__text-field", "gp-node__filename-text-field", "gp-node__text-field__hidden");
            nodeNameTextField.RegisterValueChangedCallback(evt => NodeName = evt.newValue);
            titleContainer.Insert(0, nodeNameTextField);
            titleContainer.Q<TextField>()?.SetEnabled(false);

            SetTitleIcon(GUIUtilities.GetIconByName(EditorConstants.AudioIcon));

            // ----- Ports -----
            var inputPort = this.CreatePort("Input Link", direction: Direction.Input, capacity: Port.Capacity.Multi);
            inputContainer.Insert(0, inputPort);

            // ----- Body -----
            var setAudioLabel = new Label("Set audio");
            setAudioLabel.AddToClassList("gp-node__label");
            extensionContainer.Add(setAudioLabel);

            // Dropdown with "None" default
            var options = new List<string> { "None", "Set ambience", "Set Music", "Play Audio" };
            _kindDropdown = new DropdownField(options, 0) { style = { flexGrow = 1 } };
            _kindDropdown.RegisterValueChangedCallback(OnKindChanged);
            extensionContainer.Add(_kindDropdown);

            if (Kind != AudioKind.None)
            {
                _kindDropdown.SetValueWithoutNotify(
                    Kind == AudioKind.Ambience ? "Set ambience" : "Set Music");
                BuildClipRow();
            }

            RefreshExpandedState();
            RefreshPorts();
        }

        private void OnKindChanged(ChangeEvent<string> evt)
        {
            Kind = evt.newValue switch
            {
                "Set ambience" => AudioKind.Ambience,
                "Set Music"    => AudioKind.Music,
                "Play Audio"   => AudioKind.Audio,
                _              => AudioKind.None
            };

            if (Kind == AudioKind.None)
            {
                _clipRow?.RemoveFromHierarchy();
                _clipRow = null;
            }
            else
            {
                BuildClipRow();
            }

            RefreshExpandedState();
        }

        private void BuildClipRow()
        {
            _clipRow?.RemoveFromHierarchy();
            _clipRow = null;

            var clipField = new ObjectField("Audio Clip")
            {
                objectType = typeof(AudioClip),
                allowSceneObjects = false,
                value = AudioClip,
                style =
                {
                    flexGrow = 1,
                    marginLeft = 0,
                    marginRight = 0,
                    marginTop = 2,
                    marginBottom = 0,
                    maxWidth = 160 // keep node narrow
                }
            };

            // Compact label
            var labelEl = clipField.Q<Label>();
            if (labelEl != null)
            {
                labelEl.style.marginRight = 4;
                labelEl.style.minWidth = 0;
                labelEl.style.whiteSpace = WhiteSpace.NoWrap;
                labelEl.style.overflow = Overflow.Hidden;
                labelEl.style.textOverflow = TextOverflow.Ellipsis;
            }

            // Truncate long clip names in the text part
            var textInput = clipField.Q<TextElement>();
            if (textInput != null)
            {
                textInput.style.whiteSpace = WhiteSpace.NoWrap;
                textInput.style.overflow = Overflow.Hidden;
                textInput.style.textOverflow = TextOverflow.Ellipsis;
            }

            clipField.RegisterValueChangedCallback(e => AudioClip = e.newValue as AudioClip);

            extensionContainer.Add(clipField);
            _clipRow = clipField;
        }
    }
}