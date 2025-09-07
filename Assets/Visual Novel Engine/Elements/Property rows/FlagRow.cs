using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VisualNovelEngine.Core;
using VisualNovelEngine.Data;
using VisualNovelEngine.Utilities;

namespace VisualNovelEngine.Elements
{
    public class FlagRow : VisualElement
    {
        private readonly GraphNode _node;
        private readonly IList<FlagData> _flagsList;
        public FlagData Data { get; }

        public FlagRow(GraphNode node, IList<FlagData> flagsList, FlagData data = null, bool drawValue = true, System.Type requiredType = null, Action onRemove = null)
        {
            _node = node;
            _flagsList = flagsList;

            var graphView = node.GetFirstAncestorOfType<NodeGraphView>();
            if (graphView == null)
            {
                Add(new Label("No graph view"));
                return;
            }

            var flags = requiredType == null ? graphView.Flags : graphView.Flags.Where(f => f.Type == requiredType).ToList();
            var flagNames = flags.Select(f => f.Name).ToList();

            if (flagNames.Count == 0)
            {
                Add(new Label("No flags defined"));
                return;
            }

            Data = data ?? new FlagData();
            if (data == null)
            {
                _flagsList.Add(Data);
            }

            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;

            var valueContainer = new VisualElement();

            var defaultIndex = Mathf.Max(0, flagNames.IndexOf(Data.FlagName));
            if (string.IsNullOrEmpty(Data.FlagName))
            {
                Data.FlagName = flagNames[defaultIndex];
            }
            var dropdown = new PopupField<string>(flagNames, defaultIndex);
            dropdown.style.flexGrow = 1;
            dropdown.style.flexShrink = 1;
            dropdown.style.minWidth = 0;
            dropdown.RegisterValueChangedCallback(evt =>
            {
                Data.FlagName = evt.newValue;
                var flag = flags.FirstOrDefault(f => f.Name == evt.newValue);
                Data.Type = flag?.Type.AssemblyQualifiedName;
                if (requiredType == null)
                {
                    valueContainer.Clear();
                    if (drawValue)
                    {
                        valueContainer.Add(CreateValueField(flag));
                    }
                    else if (flag != null && flag.Type == typeof(bool))
                    {
                        Data.BoolValue = true;
                    }
                }
            });
            Add(dropdown);

            if (drawValue)
            {
                if (requiredType == typeof(int))
                {
                    var intField = new IntegerField { value = Data.IntValue };
                    intField.style.width = 50;
                    intField.RegisterValueChangedCallback(evt => Data.IntValue = evt.newValue);
                    valueContainer.Add(intField);
                }
                else if (requiredType == typeof(bool))
                {
                    var toggle = new Toggle { value = Data.BoolValue };
                    toggle.RegisterValueChangedCallback(evt => Data.BoolValue = evt.newValue);
                    valueContainer.Add(toggle);
                }
            }

            Add(valueContainer);

            var removeButton = UIElementUtilities.CreateButton("X", () =>
            {
                _flagsList.Remove(Data);
                if (onRemove != null)
                {
                    onRemove();
                }
                else
                {
                    RemoveFromHierarchy();
                    _node.RefreshExpandedState();
                }
            });
            removeButton.AddToClassList("gp-node__button");
            Add(removeButton);

            if (!string.IsNullOrEmpty(Data.FlagName))
            {
                var flag = flags.FirstOrDefault(f => f.Name == Data.FlagName);
                if (flag != null)
                {
                    dropdown.SetValueWithoutNotify(Data.FlagName);
                    Data.Type = flag.Type.AssemblyQualifiedName;
                    if (drawValue && requiredType == null)
                    {
                        valueContainer.Add(CreateValueField(flag));
                    }
                    else if (requiredType == null && !drawValue && flag.Type == typeof(bool))
                    {
                        Data.BoolValue = true;
                    }
                }
            }

            _node.RefreshExpandedState();
        }

        private VisualElement CreateValueField(GraphFlag flag)
        {
            if (flag.Type == typeof(int))
            {
                var intField = new IntegerField { value = Data.IntValue };
                intField.RegisterValueChangedCallback(evt => Data.IntValue = evt.newValue);
                return intField;
            }

            if (flag.Type == typeof(bool))
            {
                var toggle = new Toggle { value = Data.BoolValue };
                toggle.RegisterValueChangedCallback(evt => Data.BoolValue = evt.newValue);
                return toggle;
            }

            return new Label("Unsupported");
        }
    }
}
