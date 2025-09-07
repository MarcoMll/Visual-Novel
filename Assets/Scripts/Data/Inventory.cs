using System;
using System.Collections.Generic;
using GameAssets.ScriptableObjects.Core;
using UnityEngine;

namespace VisualNovel.Data
{
    /// <summary>
    /// Runtime container tracking the player's items.
    /// Save and load is handled through the BaseGameData system.
    /// </summary>
    [Serializable]
    public class Inventory : BaseGameData
    {
        [SerializeField] private List<string> itemGuids = new();
        private readonly List<ItemSO> items = new();

        protected override string SaveKey => "INVENTORY_DATA";

        /// <summary>
        /// Exposes the current list of items.
        /// </summary>
        public IReadOnlyList<ItemSO> Items => items;

        public void AddItem(ItemSO item)
        {
            if (item == null || items.Contains(item)) return;
            items.Add(item);
        }

        public void RemoveItem(ItemSO item)
        {
            if (item == null) return;
            items.Remove(item);
        }

        public bool HasItem(ItemSO item) => items.Contains(item);

        public override void Save()
        {
            itemGuids.Clear();
            foreach (var item in items)
            {
                itemGuids.Add(item.Guid);
            }
            base.Save();
        }

        public override void Load()
        {
            base.Load();
            items.Clear();
            foreach (var guid in itemGuids)
            {
                var item = BaseSO.GetByGuid<ItemSO>(guid);
                if (item != null)
                {
                    items.Add(item);
                }
            }
        }
    }
}

