using UnityEngine;

namespace GameAssets.ScriptableObjects.Core
{
    [CreateAssetMenu(menuName = "VNE/New item")]
    public class ItemSO : BaseSO
    {
        public string itemName;
        public string itemDescription;
        public Sprite itemIcon;
        public ItemType itemType = ItemType.Trinket;
    }
}