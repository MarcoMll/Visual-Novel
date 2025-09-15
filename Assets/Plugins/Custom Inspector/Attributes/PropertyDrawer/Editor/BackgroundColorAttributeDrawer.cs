using CustomInspector.Extensions;
using UnityEditor;
using UnityEngine;

namespace CustomInspector
{
    [CustomPropertyDrawer(typeof(BackgroundColorAttribute))]
    public class BackgroundColorAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            BackgroundColorAttribute bc = (BackgroundColorAttribute)attribute;
            position.height = EditorGUI.GetPropertyHeight(property, label) + 2 * bc.borderSize;
            Rect coloredRect = EditorGUI.IndentedRect(position);
            EditorGUI.DrawRect(coloredRect, FixedColorConvert.ToColor(bc.color));

            Rect shrinked = new(position.x + bc.borderSize, position.y + bc.borderSize,
                                    position.width - 2 * bc.borderSize, position.height - 2 * bc.borderSize);

            var savedLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = (EditorGUIUtility.labelWidth / position.width) * shrinked.width;
            DrawProperties.PropertyField(shrinked, label, property);
            EditorGUIUtility.labelWidth = savedLabelWidth;
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            BackgroundColorAttribute bc = (BackgroundColorAttribute)attribute;
            return EditorGUI.GetPropertyHeight(property, label) + 2 * bc.borderSize + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}