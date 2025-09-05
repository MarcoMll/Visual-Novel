using UnityEngine;
using UnityEngine.UI;
using System;

namespace VisualNovel.Decisions
{
    public class SimpleTimer : MonoBehaviour
    {
        [Header("Timer Settings")]
        [Tooltip("The Image whose Fill Amount (1→0) shows remaining time")]
        public Image fillImage;
        [Tooltip("Total seconds before time runs out")]
        public float decisionTime = 5f;

        // Fired the moment time reaches zero
        public event Action OnTimerComplete;

        private float _elapsed;
        private bool  _running = false;

        void Update()
        {
            if (!_running) return;

            _elapsed += Time.deltaTime;
            // update UI
            fillImage.fillAmount = Mathf.Clamp01(1f - _elapsed / decisionTime);

            if (_elapsed >= decisionTime)
            {
                _running = false;
                OnTimerComplete?.Invoke();
            }
        }

        /// <summary>Begin (or restart) the countdown.</summary>
        public void StartTimer()
        {
            _elapsed = 0f;
            fillImage.fillAmount = 1f;
            _running = true;
        }

        /// <summary>Stops the countdown (timer freezes).</summary>
        public void StopTimer()
        {
            _running = false;
        }
    }
}
