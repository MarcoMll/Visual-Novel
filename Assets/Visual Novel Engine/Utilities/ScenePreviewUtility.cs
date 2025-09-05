#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VisualNovelEngine.Utilities;

namespace VisualNovelEngine.Utilities.EditorTools
{
    /// <summary>
    /// Utility for rendering a preview texture of a scene prefab.
    /// </summary>
    public static class ScenePreviewUtility
    {
        /// <summary>
        /// Renders a prefab into an off-screen scene and returns the resulting texture.
        /// The output texture dimensions match the focus area's aspect ratio so the
        /// preview window shows exactly the selected region.
        /// </summary>
        /// <param name="prefabRoot">Root of the prefab to preview.</param>
        /// <param name="height">Desired height of the preview in pixels. Width is calculated
        /// based on the focus area's aspect ratio.</param>
        /// <param name="clearColor">Optional background color.</param>
        public static Texture2D RenderPrefabPreview(GameObject prefabRoot, int height = 180, Color? clearColor = null)
        {
            if (!prefabRoot) return null;

            height = Mathf.Max(1, height);
            var width = height; // width will be recomputed once bounds are known

            var previewScene = EditorSceneManager.NewPreviewScene();
            // Ensure the preview scene renders objects on all layers so 2D
            // prefabs using custom sorting layers are not culled.
            EditorSceneManager.SetSceneCullingMask(previewScene, ulong.MaxValue);
            Texture2D tex = null;
            RenderTexture rt = null;
            Camera cam = null;

            try
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabRoot, previewScene);
                instance.transform.position = Vector3.zero;

                // ----- Determine bounds -----
                Bounds? b = null;

                // 1) explicit focus areas
                var focusAreas = instance.GetComponentsInChildren<PreviewFocusArea>(true);
                if (focusAreas != null && focusAreas.Length > 0)
                {
                    foreach (var f in focusAreas)
                    {
                        var fb = f.GetWorldBounds();
                        b = b.HasValue ? Encapsulate(b.Value, fb) : fb;
                    }
                }
                else
                {
                    // 2) fallback to renderers
                    var rens = instance.GetComponentsInChildren<Renderer>(true)
                                       .Where(r => r.enabled && r.gameObject.activeInHierarchy)
                                       .ToArray();
                    if (rens.Length > 0)
                    {
                        var rb = rens[0].bounds;
                        for (int i = 1; i < rens.Length; i++) rb.Encapsulate(rens[i].bounds);
                        b = rb;
                    }
                    else
                    {
                        // 3) UI fallback: RectTransform bounds
                        var rects = instance.GetComponentsInChildren<RectTransform>(true);
                        if (rects.Length > 0)
                        {
                            var corners = new Vector3[4];
                            Bounds rb = new Bounds();
                            bool init = false;
                            foreach (var rtRect in rects)
                            {
                                rtRect.GetWorldCorners(corners);
                                if (!init)
                                {
                                    rb = new Bounds(corners[0], Vector3.zero);
                                    init = true;
                                }
                                for (int i = 0; i < 4; i++) rb.Encapsulate(corners[i]);
                            }
                            b = rb;
                        }
                    }
                }

                if (!b.HasValue)
                {
                    // nothing visible — return a flat texture
                    tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
                    var fill = Enumerable.Repeat(clearColor ?? new Color(0.16f, 0.16f, 0.16f, 1f), width * height).ToArray();
                    tex.SetPixels(fill);
                    tex.Apply(false, false);
                    return tex;
                }

                var bounds = b.Value;
                // Clamp height to avoid division by zero when calculating aspect ratio
                var aspect = bounds.size.x / Mathf.Max(bounds.size.y, 0.0001f);
                width = Mathf.Max(1, Mathf.RoundToInt(height * aspect));

                // ----- Camera setup -----
                var camGO = new GameObject("PreviewCamera");
                SceneManager.MoveGameObjectToScene(camGO, previewScene);
                cam = camGO.AddComponent<Camera>();
                cam.cullingMask = -1; // render all layers
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = clearColor ?? new Color(0.16f, 0.16f, 0.16f, 1f);
                cam.orthographic = true;

                cam.orthographicSize = Mathf.Max(bounds.extents.y, 0.01f);

                var center = bounds.center;
                cam.transform.position = new Vector3(center.x, center.y, center.z - 10f);
                cam.transform.rotation = Quaternion.identity;
                cam.nearClipPlane = 0.01f;
                cam.farClipPlane = 1000f;

                // ----- Render -----
                rt = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
                cam.targetTexture = rt;
                cam.aspect = (float)width / height;
                cam.Render();

                var prev = RenderTexture.active;
                RenderTexture.active = rt;

                tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply(false, false);

                RenderTexture.active = prev;
            }
            finally
            {
                if (rt) RenderTexture.ReleaseTemporary(rt);
                if (cam) Object.DestroyImmediate(cam.gameObject);
                EditorSceneManager.ClosePreviewScene(previewScene);
            }

            return tex;
        }

        private static Bounds Encapsulate(Bounds a, Bounds b)
        {
            a.Encapsulate(b.min);
            a.Encapsulate(b.max);
            return a;
        }
    }
}
#endif
