using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace VisualNovelEngine.Windows
{
    using Core;
    using Data;

    public class GraphNodeBlackboard : ScriptableObject
    {
        private NodeGraphView _graphView;
        private Blackboard _blackboard;
        private BlackboardSection _blackboardSection;
        private readonly List<GraphFlag> _flags = new();
        public List<GraphFlag> Flags => _flags;
        public Blackboard Blackboard => _blackboard;

        public void Initialize(NodeGraphView graphView)
        {
            _graphView = graphView;
            _blackboard = new Blackboard(graphView)
            {
                title = "Blackboard"
            };

            _blackboard.addItemRequested += OnAddFlagRequested;
            _blackboard.editTextRequested += OnEditFlagName;

            _blackboardSection = new BlackboardSection { title = "Flags" };
            _blackboard.Add(_blackboardSection);
            _blackboard.SetPosition(new Rect(10, 30, 200, 300));
        }

        private void OnAddFlagRequested(Blackboard blackboard)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Int"), false, () => AddFlag(typeof(int)));
            menu.AddItem(new GUIContent("Bool"), false, () => AddFlag(typeof(bool)));
            menu.ShowAsContext();
        }

        private void OnEditFlagName(Blackboard blackboard, VisualElement element, string newValue)
        {
            if (element is not BlackboardField field) return;
            field.text = newValue;
            if (field.userData is GraphFlag flag)
            {
                flag.Name = newValue;
            }
        }

        public void AddFlag(Type type, string name = "New Flag", object value = null, bool notify = true)
        {
            var flag = new GraphFlag
            {
                Name = name,
                Type = type,
                Value = value ?? GetDefaultValue(type)
            };

            _flags.Add(flag);

            var field = new BlackboardField
            {
                text = flag.Name,
                typeText = type.Name,
                userData = flag
            };

            VisualElement valueField;
            if (type == typeof(int))
            {
                var intField = new IntegerField { value = (int)flag.Value };
                intField.RegisterValueChangedCallback(evt => flag.Value = evt.newValue);
                valueField = intField;
            }
            else if (type == typeof(bool))
            {
                var boolField = new Toggle { value = (bool)flag.Value };
                boolField.RegisterValueChangedCallback(evt => flag.Value = evt.newValue);
                valueField = boolField;
            }
            else
            {
                valueField = new Label("Unsupported");
            }

            var row = new BlackboardRow(field, valueField);
            field.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Delete", _ => RemoveFlag(flag, row));
            }));

            row.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Delete)
                {
                    evt.StopPropagation();
                    RemoveFlag(flag, row);
                }
            });
            
            row.RegisterCallback<DetachFromPanelEvent>(_ => RemoveFlag(flag, null, false), TrickleDown.TrickleDown);
            _blackboardSection.Add(row);

            if (notify)
            {
                _graphView.OnFlagsChanged();
            }
        }

        public void ClearFlags()
        {
            _flags.Clear();
            _blackboardSection?.Clear();
        }

        private void RemoveFlag(GraphFlag flag, BlackboardRow row, bool notify = true)
        {
            _flags.Remove(flag);
            if (row != null)
            {
                _blackboardSection.Remove(row);
                row.RemoveFromHierarchy();
            }

            if (notify)
            {
                _graphView.OnFlagsChanged();
            }
        }

        private static object GetDefaultValue(Type type)
        {
            return type == typeof(int) ? 0 : (object)false;
        }
    }
}

