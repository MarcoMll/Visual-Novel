using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VisualNovel.UI.Dynamic
{
    public class UIHealthBar : MonoBehaviour
    {
        [SerializeField] private Slider healthSliderIndicator;
        [SerializeField] private TMP_Text healthTextIndicator;

        private int _currentHealth = 0;
        private int _maxHealth = 0;

        private Tween _healthTween;

        public void Initialize(int maxHealth, int currentHealth = -1)
        {
            if (currentHealth == -1) currentHealth = maxHealth;

            _maxHealth = Mathf.Max(0, maxHealth);
            _currentHealth = Mathf.Clamp(currentHealth, 0, _maxHealth);

            if (healthSliderIndicator)
            {
                healthSliderIndicator.minValue = 0f;
                healthSliderIndicator.maxValue = _maxHealth;
                healthSliderIndicator.wholeNumbers = true; // health is int
                healthSliderIndicator.value = _currentHealth;
            }

            UpdateTextIndicator();
            UpdateSliderIndicator();
        }

        private void UpdateTextIndicator()
        {
            if (healthTextIndicator)
            {
                healthTextIndicator.text = $"{_currentHealth}/{_maxHealth}";
            }
        }

        private void UpdateSliderIndicator()
        {
            if (healthSliderIndicator)
            {
                healthSliderIndicator.value = _currentHealth;
            }
        }

        /// <summary>
        /// Adds (or subtracts) health and animates the change smoothly.
        /// Example: amount = -10 to take damage, +15 to heal.
        /// </summary>
        public void ModifyCurrentHealthValue(int amount)
        {
            if (_maxHealth <= 0) return;

            var target = Mathf.Clamp(_currentHealth + amount, 0, _maxHealth);
            if (target == _currentHealth) return;

            // Kill any ongoing tween to avoid conflicts
            if (_healthTween != null && _healthTween.IsActive())
                _healthTween.Kill();

            // Duration scales with delta so big hits/heals take a bit longer
            var delta = Mathf.Abs(target - _currentHealth);
            var secondsPerPoint = 0.02f;                 // ~20ms per point
            var duration = Mathf.Clamp(delta * secondsPerPoint, 0.15f, 0.9f);

            _healthTween = DOTween
                .To(() => _currentHealth, v => _currentHealth = v, target, duration)
                .SetEase(Ease.OutCubic)
                .OnUpdate(() =>
                {
                    // reflect the in-between value in UI every tick
                    UpdateSliderIndicator();
                    UpdateTextIndicator();
                })
                .OnComplete(() =>
                {
                    // ensure final clamped value & UI are in sync
                    _currentHealth = target;
                    UpdateSliderIndicator();
                    UpdateTextIndicator();
                })
                .SetTarget(this); // allows DOTween.Kill(this)
        }

        private void OnDestroy()
        {
            if (_healthTween != null && _healthTween.IsActive())
                _healthTween.Kill();
        }
    }
}