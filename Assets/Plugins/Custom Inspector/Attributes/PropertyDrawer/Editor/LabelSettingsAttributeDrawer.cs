using CustomInspector.Extensions;
using System;
using UnityEditor;
using UnityEngine;

namespace CustomInspector
{
    [CustomPropertyDrawer(typeof(LabelSettingsAttribute))]
    public class LabelSettingsAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            LabelSettingsAttribute la = (LabelSettingsAttribute)attribute;
            if(la.newName != null)
                label.text = la.newName;

            float savedLabelWidth = EditorGUIUtility.labelWidth;
            LabelGUI(position, property, label, la.labelStyle);
            EditorGUIUtility.labelWidth = savedLabelWidth;
        }

        /// <summary>
        /// warning: It changes EditorGUIUtility.labelWidth
        /// </summary>
        /// <returns>used Labelwidth</returns>
        static void LabelGUI(Rect position, SerializedProperty property, GUIContent label, LabelStyle style)
        {
            DrawProperties.DrawLabelSettings(position, property, label, style);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true) + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}