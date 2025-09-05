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
    public class ItemRow : VisualElement
    {
        private readonly GraphNode _node;
        private readonly IList<ItemData> _items;
        public ItemData Data { get; }

        public ItemRow(GraphNode node, IList<ItemData> items, ItemData data = null, bool drawToggle = true, Action onRemove = null)
        {
            _node = node;
            _items = items;

            Data = data ?? new ItemData();
            if (data == null)
            {
                _items.Add(Data);
            }

            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;

            var itemField = new ObjectField
            {
                objectType = typeof(ItemSO),
                allowSceneObjects = false,
                value = Data.Item
            };
            itemField.style.flexGrow = 1;
            itemField.style.flexShrink = 1;
            itemField.style.minWidth = 0;
            itemField.RegisterValueChangedCallback(evt => Data.Item = evt.newValue as ItemSO);
            Add(itemField);

            if (drawToggle)
            {
                var toggle = new Toggle { value = Data.HasItem };
                toggle.RegisterValueChangedCallback(evt => Data.HasItem = evt.newValue);
                Add(toggle);
            }

            var removeButton = UIElementUtilities.CreateButton("X", () =>
            {
                _items.Remove(Data);
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
