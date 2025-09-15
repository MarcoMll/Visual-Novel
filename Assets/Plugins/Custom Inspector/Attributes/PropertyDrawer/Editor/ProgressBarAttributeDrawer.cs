using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace CustomInspector
{
    [CustomPropertyDrawer(typeof(ProgressBarAttribute))]
    public class ProgressBarAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ProgressBarAttribute pb = (ProgressBarAttribute)attribute;
            position.height = GetSize(pb.size);

            if (property.propertyType == SerializedPropertyType.Float)
            {
                float value = property.floatValue / pb.max;
                EditorGUI.ProgressBar(position, value, property.name + $" ({value * 100}%)");
            }
            else if (property.propertyType == SerializedPropertyType.Integer)
            {
                float value = property.intValue / pb.max;
                EditorGUI.ProgressBar(position, value, property.name + $" ({value * 100}%)");
            }
            else
                EditorGUI.HelpBox(position, $"ProgressBar: type '{property.propertyType}' not supported", MessageType.Error);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ProgressBarAttribute pb = (ProgressBarAttribute)attribute;
            return GetSize(pb.size) + EditorGUIUtility.standardVerticalSpacing;
        }
        float GetSize(Size size)
        {
            return size switch
            {
                Size.small => EditorGUIUtility.singleLineHeight,
                Size.medium => 30,
                Size.big => 40,
                Size.max => 50,
                _ => throw new System.NotImplementedException(size.ToString()),
            };
        }
    }
}
