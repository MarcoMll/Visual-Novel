using System;
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
        [SerializeField] private GameObject itemNotificationPrefab;

        private readonly Queue<Action> notificationQueue = new();
        private GlobalNotification currentNotification;
        private GameObject currentNotificationPrefab;

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

            notificationQueue.Enqueue(() => SpawnTraitNotification(trait));
            if (currentNotification == null)
            {
                ShowNext();
            }
        }

        public void ShowItemNotification(ItemSO item)
        {
            if (item == null)
            {
                return;
            }

            notificationQueue.Enqueue(() => SpawnItemNotification(item));
            if (currentNotification == null)
            {
                ShowNext();
            }
        }

        private void SpawnTraitNotification(TraitSO trait)
        {
            var controller = UserInterfaceController.Instance;
            if (controller == null || traitNotificationPrefab == null)
            {
                notificationQueue.Clear();
                return;
            }

            var go = controller.SpawnAdditionalUI(traitNotificationPrefab);
            var notification = go.GetComponent<TraitNotification>();
            if (notification == null)
            {
                Debug.LogError("TraitNotification component missing on prefab");
                return;
            }

            currentNotification = notification;
            currentNotificationPrefab = traitNotificationPrefab;
            notification.Initialize(trait);
            if (notification.ReadButton != null)
            {
                notification.ReadButton.OnLeftClick.AddListener(HandleNotificationRead);
            }
            currentNotification.Show();
        }

        private void SpawnItemNotification(ItemSO item)
        {
            var controller = UserInterfaceController.Instance;
            if (controller == null || itemNotificationPrefab == null)
            {
                notificationQueue.Clear();
                return;
            }

            var go = controller.SpawnAdditionalUI(itemNotificationPrefab);
            var notification = go.GetComponent<ItemNotification>();
            if (notification == null)
            {
                Debug.LogError("ItemNotification component missing on prefab");
                return;
            }

            currentNotification = notification;
            currentNotificationPrefab = itemNotificationPrefab;
            notification.Initialize(item);
            if (notification.ReadButton != null)
            {
                notification.ReadButton.OnLeftClick.AddListener(HandleNotificationRead);
            }
            currentNotification.Show();
        }

        private void ShowNext()
        {
            if (notificationQueue.Count == 0)
            {
                return;
            }

            var action = notificationQueue.Dequeue();
            action?.Invoke();
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
                if (currentNotificationPrefab != null)
                {
                    UserInterfaceController.Instance.DeleteAdditionalUI(currentNotificationPrefab);
                }
                currentNotification = null;
                currentNotificationPrefab = null;
            }

            ShowNext();
        }
    }
}
