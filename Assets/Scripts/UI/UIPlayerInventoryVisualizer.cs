using GameAssets.ScriptableObjects.Core;
using UnityEngine;
using VisualNovel.Data;

namespace VisualNovel.UI
{
    public class UIPlayerInventoryVisualizer : MonoBehaviour
    {
        [SerializeField] private UIItemVisualizer itemSlotPrefab;
        [SerializeField] private Transform inventoryGrid;
        [SerializeField] private Transform equippedTrinketsGrid;

        private Inventory inventory;

        public void Initialize(Inventory playerInventory)
        {
            inventory = playerInventory;
            VisualizeEquippedTrinkets();
            VisualizeInventory();
        }

        private void VisualizeEquippedTrinkets()
        {
            ClearGrid(equippedTrinketsGrid);
            if (inventory == null) return;
            foreach (var item in inventory.EquippedItems)
            {
                if (item.itemType != ItemType.Trinket) continue;
                SpawnItem(item, equippedTrinketsGrid);
            }
        }

        private void VisualizeInventory()
        {
            ClearGrid(inventoryGrid);
            if (inventory == null) return;
            foreach (var item in inventory.Items)
            {
                if (inventory.IsEquipped(item)) continue;
                SpawnItem(item, inventoryGrid);
            }
        }

        private void SpawnItem(ItemSO item, Transform parent)
        {
            if (itemSlotPrefab == null || parent == null) return;
            var slot = Instantiate(itemSlotPrefab, parent);
            slot.SetItem(item);
        }

        private void ClearGrid(Transform grid)
        {
            if (grid == null) return;
            for (int i = grid.childCount - 1; i >= 0; i--)
            {
                Destroy(grid.GetChild(i).gameObject);
            }
        }
    }
}
