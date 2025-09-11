using System;
using UnityEngine;
using VisualNovel.GameFlow;

namespace VisualNovel.Minigames
{
    /// <summary>
    /// Handles instantiation and tracking of active minigames.
    /// </summary>
    public class MinigameManager : MonoBehaviour, IInitializeOnAwake
    {
        [SerializeField] private Transform uiRoot;

        private MinigameBase _current;

        public static MinigameManager Instance { get; private set; }

        public void Initialize()
        {
            if (Instance != null)
                return;

            Instance = this;
        }

        /// <summary>
        /// Launches a new minigame based on the given prefab.
        /// </summary>
        /// <param name="minigamePrefab">Prefab that contains a <see cref="MinigameBase"/>.</param>
        /// <param name="setup">Optional callback to configure the minigame before launch.</param>
        /// <param name="callback">Invoked with the success state once the minigame finishes.</param>
        public void StartMinigame(MinigameBase minigamePrefab, Action<MinigameBase> setup = null, Action<bool> callback = null)
        {
            if (_current != null)
            {
                Debug.LogWarning("A minigame is already running.");
                return;
            }

            _current = Instantiate(minigamePrefab);
            setup?.Invoke(_current);
            if (callback != null)
                _current.Finished += callback;
            _current.Finished += HandleMinigameFinished;
            _current.Launch();
        }

        private void HandleMinigameFinished(bool success)
        {
            if (_current == null)
                return;

            _current.Finished -= HandleMinigameFinished;
            Destroy(_current.gameObject);
            _current = null;
        }
    }
}
