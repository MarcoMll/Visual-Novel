using System.Collections.Generic;
using GameAssets.ScriptableObjects.Core;
using UnityEngine;
using UnityEngine.UI;

namespace VisualNovel.UI
{
    public class UIItemVisualizer : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private List<ItemType> acceptableTypes = new();

        private ItemSO currentItem;

        public ItemSO CurrentItem => currentItem;

        public bool AcceptsType(ItemType type)
        {
            return acceptableTypes.Count == 0 || acceptableTypes.Contains(type);
        }

        public void SetItem(ItemSO item)
        {
            if (item != null && AcceptsType(item.itemType) == false)
            {
                return;
            }

            currentItem = item;
            if (iconImage != null)
            {
                iconImage.sprite = item != null && item.itemIcon != null ? item.itemIcon : defaultSprite;
            }
        }

        public void Clear()
        {
            currentItem = null;
            if (iconImage != null)
            {
                iconImage.sprite = defaultSprite;
            }
        }
    }
}
