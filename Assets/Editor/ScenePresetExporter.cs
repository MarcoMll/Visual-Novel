using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using VisualNovel.Environment;

namespace VisualNovel.Editor
{
    public static class ScenePresetExporter
    {
        [MenuItem("Tools/Export Scene Presets Info")]
        public static void ExportScenePresets()
        {
            var guids = AssetDatabase.FindAssets("t:Prefab");
            var sb = new StringBuilder();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                    continue;

                var controller = prefab.GetComponent<SceneController>();
                if (controller == null)
                    continue;

                sb.AppendLine($"Scene Prefab: {prefab.name}");

                var presets = controller.ScenePresets;
                if (presets == null || presets.Length == 0)
                {
                    sb.AppendLine("  (No presets found)");
                    continue;
                }

                foreach (var preset in presets)
                {
                    sb.AppendLine($"  Preset: {preset.PresetName}");
                    AppendLightSource(sb, "    Global Light", preset.GlobalLight);
                    sb.AppendLine($"    Sky Color: {ColorToString(preset.SkyColor)}");
                    sb.AppendLine($"    Background Color: {ColorToString(preset.BackgroundColor)}");

                    sb.AppendLine("    Scene Light Sources:");
                    AppendLightSources(sb, preset.SceneLightSources);

                    sb.AppendLine("    Scene Particles:");
                    AppendGameObjects(sb, preset.SceneParticles);

                    sb.AppendLine("    Additional Scene Objects:");
                    AppendGameObjects(sb, preset.AdditionalSceneObjects);

                    sb.AppendLine("    Ambience Clips:");
                    AppendAmbienceClips(sb, preset.AmbienceClips);
                }

                sb.AppendLine();
            }

            var filePath = Path.Combine(Application.dataPath, "ScenePresetsInfo.txt");
            File.WriteAllText(filePath, sb.ToString());
            AssetDatabase.Refresh();
            Debug.Log($"Scene preset info exported to {filePath}");
        }

        private static void AppendLightSource(StringBuilder sb, string label, SceneController.LightSource source)
        {
            if (source == null)
            {
                sb.AppendLine($"{label}: None");
                return;
            }

            var lightName = source.Light != null ? source.Light.name : "None";
            sb.AppendLine($"{label}: {lightName}, Color: {ColorToString(source.Color)}, Intensity: {source.Intensity}");
        }

        private static void AppendLightSources(StringBuilder sb, SceneController.LightSource[] sources)
        {
            if (sources == null || sources.Length == 0)
            {
                sb.AppendLine("      (none)");
                return;
            }

            foreach (var source in sources)
            {
                AppendLightSource(sb, "      -", source);
            }
        }

        private static void AppendGameObjects(StringBuilder sb, Object[] objects)
        {
            if (objects == null || objects.Length == 0)
            {
                sb.AppendLine("      (none)");
                return;
            }

            foreach (var obj in objects)
            {
                var name = obj != null ? obj.name : "None";
                sb.AppendLine($"      - {name}");
            }
        }

        private static void AppendAmbienceClips(StringBuilder sb, SceneController.AmbienceClip[] clips)
        {
            if (clips == null || clips.Length == 0)
            {
                sb.AppendLine("      (none)");
                return;
            }

            foreach (var clip in clips)
            {
                var name = clip.Clip != null ? clip.Clip.name : "None";
                sb.AppendLine($"      - {name} (volume: {clip.Volume})");
            }
        }

        private static string ColorToString(Color color)
        {
            return $"RGBA({color.r:F3},{color.g:F3},{color.b:F3},{color.a:F3})";
        }
    }
}

