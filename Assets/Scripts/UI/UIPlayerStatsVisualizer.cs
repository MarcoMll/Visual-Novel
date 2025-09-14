using UnityEngine;
using VisualNovel.Data;

namespace VisualNovel.UI
{
    public class UIPlayerStatsVisualizer : MonoBehaviour
    {
        [SerializeField] private UICharacterEquipment characterEquipment;
        [SerializeField] private ExtendedButton hideButton;

        private GameObject inventoryPrefab;

        public void Initialize(Inventory inventory, GameObject uiReference)
        {
            inventoryPrefab = uiReference;

            if (characterEquipment != null)
            {
                characterEquipment.Initialize(inventory);
            }

            if (hideButton != null)
            {
                hideButton.OnLeftClick.AddListener(Hide);
            }
        }

        private void Hide()
        {
            if (inventoryPrefab != null)
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
