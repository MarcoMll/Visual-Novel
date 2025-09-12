using System;
using UnityEngine;
using VisualNovel.UI;

namespace VisualNovel.Minigames
{
    /// <summary>
    /// Base class for all minigames. Handles UI spawning and
    /// completion notification.
    /// </summary>
    public abstract class MinigameBase : MonoBehaviour
    {
        [SerializeField] protected GameObject uiPrefab;
        private GameObject _uiInstance;
        private Transform _uiRoot;

        /// <summary>Event invoked when the minigame finishes.</summary>
        public event Action<bool> Finished;

        /// <summary>Launches the minigame and spawns its UI.</summary>
        /// <param name="uiParent">Parent transform for the minigame UI.</param>
        public virtual void Launch()
        {
            OnStart();
        }

        /// <summary>
        /// Called when the minigame starts. Derived classes should begin their
        /// game flow here.
        /// </summary>
        protected abstract void OnStart();

        /// <summary>
        /// Finish the minigame and notify listeners.
        /// </summary>
        /// <param name="isSuccess">Whether the player succeeded.</param>
        protected void Finish(bool isSuccess)
        {
            DestroyUI();

            OnFinish(isSuccess);
            Finished?.Invoke(isSuccess);
        }

        /// <summary>
        /// Spawns the configured UI prefab using <see cref="UserInterfaceManager"/>.
        /// </summary>
        /// <returns>The spawned UI instance or null if none was spawned.</returns>
        protected GameObject SpawnUI()
        {
            if (UserInterfaceManager.Instance != null && uiPrefab != null)
            {
                _uiInstance = UserInterfaceManager.Instance.SpawnAdditionalUI(uiPrefab);
            }

            return _uiInstance;
        }

        /// <summary>Destroys the previously spawned UI instance.</summary>
        protected void DestroyUI()
        {
            if (_uiInstance == null)
            {
                return;
            }

            if (UserInterfaceManager.Instance != null && uiPrefab != null)
            {
                UserInterfaceManager.Instance.DeleteAdditionalUI(uiPrefab);
            }
            else
            {
                Destroy(_uiInstance);
            }

            _uiInstance = null;
        }

        /// <summary>
        /// Called before <see cref="Finished"/> is invoked. Allows derived
        /// classes to clean up.
        /// </summary>
        protected virtual void OnFinish(bool isSuccess) {}
    }
}
