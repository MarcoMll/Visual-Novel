using UnityEngine;

namespace VisualNovelEngine.Utilities
{
    /// <summary>
    /// Drop this on the prefab to tell editor previews what to frame.
    /// The box is axis-aligned in world space; size is in world units.
    /// </summary>
    public class PreviewFocusArea : MonoBehaviour
    {
        [Tooltip("Center of the focus area, in local space of this transform.")]
        public Vector3 localCenter = Vector3.zero;

        [Tooltip("Size of the focus area (world units). X/Y for 2D, Z can be small but > 0.")]
        public Vector3 size = new Vector3(10f, 6f, 1f);

        [Tooltip("Extra padding around the bounds (e.g., 0.05 = 5%).")]
        [Range(0f, 0.5f)] public float padding = 0.05f;

        /// <summary>World-space bounds for framing.</summary>
        public Bounds GetWorldBounds()
        {
            var worldCenter = transform.TransformPoint(localCenter);
            // Axis-aligned box sized in world space (ignores rotation intentionally for stable framing)
            var worldSize = new Vector3(
                Mathf.Abs(size.x * transform.lossyScale.x),
                Mathf.Abs(size.y * transform.lossyScale.y),
                Mathf.Max(0.01f, Mathf.Abs(size.z * transform.lossyScale.z))
            );

            var b = new Bounds(worldCenter, worldSize);
            if (padding > 0f)
            {
                var p = 1f + padding;
                b.Expand(new Vector3(b.size.x * (p - 1f), b.size.y * (p - 1f), b.size.z * (p - 1f)));
            }
            return b;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.05f);
            var b = GetWorldBounds();
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.DrawCube(b.center, b.size);
            Gizmos.color = new Color(0.05f, 0.6f, 1f, 1f);
            Gizmos.DrawWireCube(b.center, b.size);
        }
#endif
    }
}