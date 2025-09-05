using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering.Universal;

namespace VisualNovel.Environment
{
    [RequireComponent(typeof(Light2D))]
    public class LightIntensityController : MonoBehaviour
    {
        [Header("Light & Limits")]
        [SerializeField] private Light2D targetLight;
        [SerializeField, Min(0f)] private float minIntensity = 0f;
        [SerializeField, Min(0f)] private float maxIntensity = 1f;

        [Header("Timing & Curve")]
        [Tooltip("Duration in seconds for one full transition (min → max → min).")]
        [SerializeField, Min(0.01f)] private float cycleDuration = 2f;
        [Tooltip("Use this curve (0–1 on X) to shape how intensity interpolates between min and max.")]
        [SerializeField] private AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        // Internal timer (0 ≤ t < cycleDuration)
        private float timer = 0f;

        private void Reset()
        {
            // Auto‐assign the Light2D component if not set
            targetLight = GetComponent<Light2D>();
        }

        private void OnValidate()
        {
            // Ensure limits make sense
            if (maxIntensity < minIntensity)
            {
                maxIntensity = minIntensity;
            }
            if (cycleDuration <= 0f)
            {
                cycleDuration = 0.01f;
            }
        }

        private void Update()
        {
            if (targetLight == null) return;

            // Advance timer
            timer += Time.deltaTime;
            if (timer > cycleDuration)
            {
                timer -= cycleDuration; // loop back
            }

            // Normalize t to [0,1], but we want a ping-pong effect:
            // first half of cycle: 0→1, second half: 1→0
            float half = cycleDuration * 0.5f;
            float tNormalized;
            if (timer <= half)
            {
                tNormalized = timer / half;       // 0 → 1
            }
            else
            {
                tNormalized = (cycleDuration - timer) / half; // 1 → 0
            }

            // Evaluate curve (expects input 0–1) to shape interpolation
            float curveValue = Mathf.Clamp01(intensityCurve.Evaluate(tNormalized));

            // Lerp between min and max based on curve
            float newIntensity = Mathf.Lerp(minIntensity, maxIntensity, curveValue);
            targetLight.intensity = newIntensity;
        }
    }
}
