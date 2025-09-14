using GameAssets.ScriptableObjects.Core;
using VisualNovel.Data;
using UnityEngine;

namespace VisualNovel.UI
{
    public class UICharacterEquipment : MonoBehaviour
    {
        [SerializeField] private UIItemVisualizer helmet;
        [SerializeField] private UIItemVisualizer chest;
        [SerializeField] private UIItemVisualizer pants;
        [SerializeField] private UIItemVisualizer weapon;
        [SerializeField] private UIItemVisualizer shield;

        public void Initialize(Inventory inventory)
        {
            if (inventory == null) return;
            ClearSlots();

            foreach (var item in inventory.EquippedItems)
            {
                switch (item.itemType)
                {
                    case ItemType.Helmet:
                        helmet?.SetItem(item);
                        break;
                    case ItemType.Chest:
                        chest?.SetItem(item);
                        break;
                    case ItemType.Pants:
                        pants?.SetItem(item);
                        break;
                    case ItemType.Weapon:
                        weapon?.SetItem(item);
                        break;
                    case ItemType.Shield:
                        shield?.SetItem(item);
                        break;
                }
            }
        }

        private void ClearSlots()
        {
            helmet?.Clear();
            chest?.Clear();
            pants?.Clear();
            weapon?.Clear();
            shield?.Clear();
        }
    }
}
