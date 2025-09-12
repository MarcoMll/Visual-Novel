using System.Collections.Generic;
using UnityEngine;
using VisualNovel.GameFlow;
using GameAssets.ScriptableObjects.Core;

namespace VisualNovel.UI
{
    using Notifications;

    public class UINotificationManager : MonoBehaviour, IInitializeOnAwake
    {
        [SerializeField] private GameObject traitNotificationPrefab;

        private readonly Queue<TraitSO> traitQueue = new();
        private TraitNotification currentNotification;

        public static UINotificationManager Instance { get; private set; }

        public void Initialize()
        {
            if (Instance != null)
            {
                return;
            }

            Instance = this;
        }

        public void ShowTraitNotification(TraitSO trait)
        {
            if (trait == null)
            {
                return;
            }

            traitQueue.Enqueue(trait);
            if (currentNotification == null)
            {
                ShowNextTrait();
            }
        }

        private void ShowNextTrait()
        {
            if (traitQueue.Count == 0)
            {
                return;
            }

            var controller = UserInterfaceController.Instance;
            if (controller == null || traitNotificationPrefab == null)
            {
                traitQueue.Clear();
                return;
            }

            var go = controller.SpawnAdditionalUI(traitNotificationPrefab);
            currentNotification = go.GetComponent<TraitNotification>();
            if (currentNotification == null)
            {
                Debug.LogError("TraitNotification component missing on prefab");
                return;
            }

            currentNotification.Initialize(traitQueue.Dequeue());
            if (currentNotification.ReadButton != null)
            {
                currentNotification.ReadButton.OnLeftClick.AddListener(HandleNotificationRead);
            }
            currentNotification.Show();
        }

        private void HandleNotificationRead()
        {
            if (currentNotification != null)
            {
                if (currentNotification.ReadButton != null)
                {
                    currentNotification.ReadButton.OnLeftClick.RemoveListener(HandleNotificationRead);
                }

                currentNotification.Hide();
                UserInterfaceController.Instance.DeleteAdditionalUI(traitNotificationPrefab);
                currentNotification = null;
            }

            ShowNextTrait();
        }
    }
}
