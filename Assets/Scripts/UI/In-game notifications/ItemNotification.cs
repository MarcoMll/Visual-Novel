using GameAssets.ScriptableObjects.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VisualNovel.UI.Notifications
{
    using UI;

    public class ItemNotification : AnimatedNotification
    {
        [Header("UI Setup")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TMP_Text itemNameTextField;
        [SerializeField] private TMP_Text itemDescriptionTextField;
        [SerializeField] private ExtendedButton readButton;

        public void Initialize(ItemSO targetItem)
        {
            itemIcon.sprite = targetItem.itemIcon;
            itemNameTextField.text = targetItem.itemName;
            itemDescriptionTextField.text = targetItem.itemDescription;
        }

        public override ExtendedButton ReadButton => readButton;
    }
}
