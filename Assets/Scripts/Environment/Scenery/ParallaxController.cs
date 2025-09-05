using UnityEngine;
using System.Collections.Generic;

namespace VisualNovel.Environment
{
    public class ParallaxController : MonoBehaviour
    {
        [Header("Assign each layer in order: Close to Far")]
        [Tooltip("Drag your foreground, midground, and background Transforms here (in order from closest → farthest).")]
        public List<Transform> layers = new List<Transform>();

        [Header("How strong each layer moves. Closer layers get bigger factors.")]
        [Tooltip("Each entry here corresponds to layers[i]. A higher factor = more movement.")]
        public List<float> parallaxFactors = new List<float>();

        [Header("Max offset (in world units) when the mouse is at screen edge.")]
        [Tooltip("If your room is large, you might only want a small offset (e.g. 0.5 units).")]
        public Vector2 maxOffset = new Vector2(1.0f, 0.5f);

    // (Optional) How quickly layers lerp toward their target position. 
    // A value of 1 = instant; lower values = smoother easing.
        [Range(0.01f, 1f)]
        public float smoothing = 0.1f;

    // We’ll store each layer’s original localPosition so we can add offsets to it.
        private Vector3[] originalPositions;

        void Reset()
        {
            // If the script just got added, try auto‐filling children as layers
            layers.Clear();
            parallaxFactors.Clear();
            foreach (Transform child in transform)
            {
                layers.Add(child);
                parallaxFactors.Add(1f);
            }
        }

        void Start()
        {
            // Make sure our lists match
            ClampLists();
            CacheOriginals();
        }

        void OnValidate()
        {
            // Keep things in sync in the Inspector
            ClampLists();
        }

        void ClampLists()
        {
            // Ensure parallaxFactors has same count as layers
            if (parallaxFactors.Count != layers.Count)
            {
                // Resize and fill with 1.0f if needed
                int diff = layers.Count - parallaxFactors.Count;
                if (diff > 0)
                {
                    for (int i = 0; i < diff; i++)
                        parallaxFactors.Add(1f);
                }
                else if (diff < 0)
                {
                    parallaxFactors.RemoveRange(layers.Count, -diff);
                }
            }
        }

        void CacheOriginals()
        {
            originalPositions = new Vector3[layers.Count];
            for (int i = 0; i < layers.Count; i++)
            {
                if (layers[i] != null)
                    originalPositions[i] = layers[i].localPosition;
            }
        }

        void Update()
        {
            if (layers.Count == 0) return;

        // 1) Get mouse position normalized around center
        Vector2 mousePos = Input.mousePosition;
        float nx = (mousePos.x / Screen.width  - 0.5f) * 2f;  // range: [-1..1]
        float ny = (mousePos.y / Screen.height - 0.5f) * 2f;  // range: [-1..1]

        // 2) For each layer, compute its target offset
            for (int i = 0; i < layers.Count; i++)
            {
                if (layers[i] == null) continue;

                float factor = parallaxFactors[i];
                // Desired offset in world‐space
                Vector3 targetOffset = new Vector3(
                    nx * maxOffset.x * factor,
                    ny * maxOffset.y * factor,
                    0f
                );

                Vector3 desiredPos = originalPositions[i] + targetOffset;

                // 3) Smoothly interpolate toward desired position
                layers[i].localPosition = Vector3.Lerp(
                    layers[i].localPosition,
                    desiredPos,
                    Mathf.Clamp01(smoothing) * Time.deltaTime * 60f
                );
            }
        }
    }
}
