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
    public class TraitRow : VisualElement
    {
        private readonly GraphNode _node;
        private readonly IList<TraitData> _traits;
        public TraitData Data { get; }

        public TraitRow(GraphNode node, IList<TraitData> traits, TraitData data = null, bool drawToggle = true, Action onRemove = null)
        {
            _node = node;
            _traits = traits;

            Data = data ?? new TraitData();
            if (data == null)
            {
                _traits.Add(Data);
            }

            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;

            var traitField = new ObjectField
            {
                objectType = typeof(TraitSO),
                allowSceneObjects = false,
                value = Data.Trait
            };
            traitField.style.flexGrow = 1;
            traitField.style.flexShrink = 1;
            traitField.style.minWidth = 0;
            traitField.RegisterValueChangedCallback(evt => Data.Trait = evt.newValue as TraitSO);
            Add(traitField);

            if (drawToggle)
            {
                var toggle = new Toggle { value = Data.HasTrait };
                toggle.RegisterValueChangedCallback(evt => Data.HasTrait = evt.newValue);
                Add(toggle);
            }

            var removeButton = UIElementUtilities.CreateButton("X", () =>
            {
                _traits.Remove(Data);
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
