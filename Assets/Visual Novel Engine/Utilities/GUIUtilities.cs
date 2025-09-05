using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VisualNovelEngine.Utilities
{
    public static class GUIUtilities
    {
        private static readonly Dictionary<string, string> IconsMap = new Dictionary<string, string>
        {
            {EditorConstants.TextIcon, EditorConstants.TextIconPath},
            {EditorConstants.FlagIcon, EditorConstants.FlagIconPath},
            {EditorConstants.ChoiceIcon, EditorConstants.ChoiceIconPath},
            {EditorConstants.ModifierIcon, EditorConstants.ModifierIconPath},
            {EditorConstants.DialogueIcon, EditorConstants.DialogueIconPath},
            {EditorConstants.AudioIcon, EditorConstants.AudioIconPath},
            {EditorConstants.SceneryIcon, EditorConstants.SceneIconPath},
            {EditorConstants.DelayIcon, EditorConstants.DelayIconPath}
        };

        public static Texture2D GetIconByName(string iconName)
        {
            IconsMap.TryGetValue(iconName, out var path);
            return GetIconByPath(path);
        }

        public static Texture2D GetIconByPath(string pathToIcon)
        {
            if (string.IsNullOrEmpty(pathToIcon) == true)
            {
                Debug.LogError("Path to icon can't be null!");
                return null;
            }
            
            var iconTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(pathToIcon);
            
            if (iconTexture == null)
            {
                Debug.LogWarning($"Icon not found at path: {pathToIcon}");
                return null;
            }

            return iconTexture;
        }
    }
}