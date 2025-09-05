using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;
using GameAssets.ScriptableObjects.Core;
using VisualNovelEngine.Data;
using VisualNovelEngine.Utilities;

namespace VisualNovelEngine.Elements
{
    public class RelationshipModifierRow : VisualElement
    {
        private readonly GraphNode _node;
        private readonly IList<RelationshipModifier> _modifiers;
        public RelationshipModifier Data { get; }

        public RelationshipModifierRow(GraphNode node, IList<RelationshipModifier> modifiers, RelationshipModifier data = null, Action onRemove = null)
        {
            _node = node;
            _modifiers = modifiers;

            Data = data ?? new RelationshipModifier();
            // Ensure the modifier is tracked by the parent list. When an existing
            // modifier is passed in it will already be contained in the list, but
            // menu actions construct a new instance with prefilled values. Those
            // instances are not in the list yet, which previously caused them to
            // disappear after saving. Always add the instance if it's missing.
            if (!_modifiers.Contains(Data))
            {
                _modifiers.Add(Data);
            }

            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;

            var characterField = new ObjectField
            {
                objectType = typeof(CharacterSO),
                allowSceneObjects = false,
                value = Data.Character
            };
            characterField.style.flexGrow = 1;
            characterField.style.flexShrink = 1;
            characterField.style.minWidth = 0;
            characterField.RegisterValueChangedCallback(evt => Data.Character = evt.newValue as CharacterSO);
            Add(characterField);

            var intField = new IntegerField { value = Data.Amount };
            intField.style.width = 50;
            
            intField.RegisterValueChangedCallback(evt => Data.Amount = evt.newValue);
            Add(intField);

            var removeButton = UIElementUtilities.CreateButton("X", () =>
            {
                _modifiers.Remove(Data);
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

            _node.RefreshExpandedState();
        }
    }
}
