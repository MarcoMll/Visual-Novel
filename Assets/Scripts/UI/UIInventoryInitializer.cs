using UnityEngine;
using VisualNovel.Data;

namespace VisualNovel.UI
{
    public class UIInventoryInitializer : MonoBehaviour
    {
        [SerializeField] private UICharacterEquipment characterEquipment;
        [SerializeField] private ExtendedButton hideButton;

        private GameObject inventoryPrefab;

        public void Initialize(Inventory inventory, GameObject prefab)
        {
            inventoryPrefab = prefab;

            if (characterEquipment != null)
            {
                characterEquipment.Initialize(inventory);
            }

            if (hideButton != null)
            {
                hideButton.OnLeftClick.RemoveListener(Hide);
                hideButton.OnLeftClick.AddListener(Hide);
            }
        }

        private void Hide()
        {
            if (UserInterfaceController.Instance != null && inventoryPrefab != null)
            {
                UserInterfaceController.Instance.DeleteAdditionalUI(inventoryPrefab);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
