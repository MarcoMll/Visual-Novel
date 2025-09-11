using System;
using UnityEngine;

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
            if (_uiInstance != null)
            {
                Destroy(_uiInstance);
            }

            OnFinish(isSuccess);
            Finished?.Invoke(isSuccess);
        }

        /// <summary>
        /// Called before <see cref="Finished"/> is invoked. Allows derived
        /// classes to clean up.
        /// </summary>
        protected virtual void OnFinish(bool isSuccess) {}
    }
}
