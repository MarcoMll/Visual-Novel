using UnityEditor;
using UnityEngine.UIElements;

namespace VisualNovelEngine.Utilities
{
    public static class StyleSheetUtilities
    {
        public static VisualElement AddClasses(this VisualElement visualElement, params string[] classNames)
        {
            foreach (var className in classNames)
            {
                visualElement.AddToClassList(className);
            }

            return visualElement;
        }
        
        public static VisualElement AddStyleSheets(this VisualElement visualElement, params string[] styleSheetNames)
        {
            foreach (var styleSheetName in styleSheetNames)
            {
                var styleSheet = FindStyleSheet(styleSheetName);
                visualElement.styleSheets.Add(styleSheet);
            }

            return visualElement;
        }
        
        public static StyleSheet FindStyleSheet(string styleSheetName)
        {
            var guid = AssetDatabase.FindAssets($"{styleSheetName} t:StyleSheet"); // looking for all the style sheets with this certain name
            if (guid.Length == 0) return null; // making sure that we actually found anything, if no, then interrupt 
            
            var path = AssetDatabase.GUIDToAssetPath(guid[0]); // in case we found more than one result, we assigning only the first one
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
            return styleSheet;
        }
    }
}