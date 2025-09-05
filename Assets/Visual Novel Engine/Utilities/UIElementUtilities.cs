using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace VisualNovelEngine.Utilities
{
    using Elements;
    
    public static class UIElementUtilities
    {
        public static Button CreateButton(string text, Action onClick = null)
        {
            var button = new Button(onClick)
            {
                text = text
            };
            return button;
        }
        
        public static Foldout CreateFoldout(string title, bool collapsed = false)
        {
            var foldout = new Foldout()
            {
                text = title,
                value = !collapsed
            };

            return foldout;
        }
        
        public static TextField CreateTextField(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null)
        {
            var textField = new TextField
            {
                value = value,
                label = label
            };

            if (onValueChanged != null)
            {
                textField.RegisterValueChangedCallback(onValueChanged);
            }

            return textField;
        }

        public static TextField CreateTextArea(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null)
        {
            var textArea = CreateTextField(value, label, onValueChanged);
            textArea.multiline = true;
            
            return textArea;
        }

        public static Port CreatePort(this GraphNode node, string portName = "", Orientation orientation = Orientation.Horizontal, Direction direction = Direction.Output, Port.Capacity capacity = Port.Capacity.Single)
        {
            var port = node.InstantiatePort(orientation, direction, capacity, typeof(bool));
            port.portName = portName;
            return port;
        }

        public static VisualElement CreateSpace(float height)
        {
            var spacer = new VisualElement
            {
                style =
                {
                    height = 6 // pixels of space
                }
            };
            return spacer;
        }
    }
}