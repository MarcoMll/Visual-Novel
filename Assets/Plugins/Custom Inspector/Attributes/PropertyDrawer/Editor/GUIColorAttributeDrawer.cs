using CustomInspector.Extensions;
using UnityEditor;
using UnityEngine;


namespace CustomInspector
{
    [CustomPropertyDrawer(typeof(GUIColorAttribute))]
    public class GUIColorAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUIColorAttribute c = (GUIColorAttribute)attribute;

            if (c.fixedColor.HasValue)
            {
                Color savedColor = GUI.color;
                GUI.color = FixedColorConvert.ToColor(c.fixedColor.Value);
                DrawProperties.PropertyField(position, label, property);
                if (!c.colorWholeUI)
                    GUI.color = savedColor;
            }
            else
            {
                if (UnityParsing.IsColor(c.colorString, out Color color))
                {
                    Color savedColor = GUI.color;
                    GUI.color = color;
                    EditorGUI.PropertyField(position, property, label);
                    if (!c.colorWholeUI)
                        GUI.color = savedColor;
                }
                else
                {
                    DrawProperties.DrawFieldWithMessage(position, label, property, $"Wrong color format: \"{c.colorString}\". Example format: \"(1, 0.2, 0.2, 1)\"", MessageType.Error);
                }
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            GUIColorAttribute c = (GUIColorAttribute)attribute;

            if (c.fixedColor.HasValue
                || UnityParsing.IsColor(c.colorString, out _))
            {
                return EditorGUI.GetPropertyHeight(property, label) + EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {
                return DrawProperties.errorHeight + DrawProperties.errorSpacing
                + EditorGUI.GetPropertyHeight(property, label) + EditorGUIUtility.standardVerticalSpacing;
            }

        }

    }
}