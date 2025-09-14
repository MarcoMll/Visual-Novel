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
        [SerializeField] private List<string> equippedGuids = new();
        [SerializeField] private int maxEquippedTrinkets = 1;
        private readonly List<ItemSO> items = new();
        private readonly List<ItemSO> equippedItems = new();

        protected override string SaveKey => "INVENTORY_DATA";

        /// <summary>
        /// Exposes the current list of items.
        /// </summary>
        public IReadOnlyList<ItemSO> Items => items;
        public IReadOnlyList<ItemSO> EquippedItems => equippedItems;
        public int MaxEquippedTrinkets
        {
            get => maxEquippedTrinkets;
            set => maxEquippedTrinkets = value;
        }

        public void AddItem(ItemSO item)
        {
            if (item == null || items.Contains(item)) return;
            items.Add(item);
        }

        public void RemoveItem(ItemSO item)
        {
            if (item == null) return;
            if (items.Contains(item) == false)
            {
                Debug.Log($"Item {item.itemName} is not present in the player's inventory.");
                return;
            }

            items.Remove(item);
            equippedItems.Remove(item);
        }

        public bool HasItem(ItemSO item) => items.Contains(item);

        public void EquipItem(ItemSO item)
        {
            if (item == null || items.Contains(item) == false) return;
            if (equippedItems.Contains(item)) return;
            equippedItems.Add(item);
        }

        public void UnequipItem(ItemSO item)
        {
            if (item == null) return;
            equippedItems.Remove(item);
        }

        public bool IsEquipped(ItemSO item) => equippedItems.Contains(item);

        public bool HasEquippedOfType(ItemType type)
        {
            foreach (var equipped in equippedItems)
            {
                if (equipped.itemType == type)
                    return true;
            }

            return false;
        }

        public int EquippedCount(ItemType type)
        {
            var count = 0;
            foreach (var equipped in equippedItems)
            {
                if (equipped.itemType == type)
                    count++;
            }

            return count;
        }

        public override void Save()
        {
            itemGuids.Clear();
            equippedGuids.Clear();
            foreach (var item in items)
            {
                itemGuids.Add(item.Guid);
                if (equippedItems.Contains(item))
                {
                    equippedGuids.Add(item.Guid);
                }
            }
            base.Save();
        }

        public override void Load()
        {
            base.Load();
            items.Clear();
            equippedItems.Clear();
            foreach (var guid in itemGuids)
            {
                var item = BaseSO.GetByGuid<ItemSO>(guid);
                if (item != null)
                {
                    items.Add(item);
                }
            }

            foreach (var guid in equippedGuids)
            {
                var item = BaseSO.GetByGuid<ItemSO>(guid);
                if (item != null)
                {
                    if (items.Contains(item) == false)
                    {
                        items.Add(item);
                    }
                    equippedItems.Add(item);
                }
            }
        }
    }
}

