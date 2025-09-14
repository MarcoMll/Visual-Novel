using UnityEngine;

namespace VisualNovel.UI
{
    public class UIPlayerInventoryVisualizer : MonoBehaviour
    {
        [SerializeField] private UIItemVisualizer itemSlotPrefab;
        [SerializeField] private Transform inventoryGrid;
        [SerializeField] private Transform equippedItemsGrid;
    }
}