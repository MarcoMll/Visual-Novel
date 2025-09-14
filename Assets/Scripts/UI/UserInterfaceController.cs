using System.Collections.Generic;
using UnityEngine;
using VisualNovel.GameFlow;
using VisualNovel.Data;

namespace VisualNovel.UI
{
    public class UserInterfaceController : MonoBehaviour, IInitializeOnAwake
    {
        [SerializeField] private Canvas gameCanvas;
        public static UserInterfaceController Instance { get; private set; }

        private readonly Dictionary<GameObject, GameObject> additionalUIs = new();
        
        public void Initialize()
        {
            if (Instance != null)
            {
                return;
            }

            Instance = this;
        }

        public GameObject SpawnAdditionalUI(GameObject uiPrefab)
        {
            if (uiPrefab == null)
            {
                return null;
            }

            if (additionalUIs.TryGetValue(uiPrefab, out var existingUi))
            {
                return existingUi;
            }

            var spawnedUi = Instantiate(uiPrefab, gameCanvas.transform);
            additionalUIs[uiPrefab] = spawnedUi;
            return spawnedUi;
        }
        
        public void DeleteAdditionalUI(GameObject uiPrefab)
        {
            if (uiPrefab == null)
            {
                return;
            }

            if (!additionalUIs.TryGetValue(uiPrefab, out var spawnedUi))
            {
                return;
            }

            Destroy(spawnedUi);
            additionalUIs.Remove(uiPrefab);
        }

        public void ShowInventory(GameObject inventoryPrefab, Inventory inventory)
        {
            if (inventoryPrefab == null)
            {
                return;
            }

            var inventoryUi = SpawnAdditionalUI(inventoryPrefab);
            if (inventoryUi == null)
            {
                return;
            }

            var initializer = inventoryUi.GetComponent<UIInventoryInitializer>();
            initializer?.Initialize(inventory, inventoryPrefab);
        }
    }
}