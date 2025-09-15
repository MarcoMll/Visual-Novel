using CustomInspector.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CustomInspector
{
    [CustomPropertyDrawer(typeof(UnwrapAttribute))]
    public class UnwrapAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(property.IsArrayElement()) //is list element
            {
                DrawProperties.PropertyField(position, label, property, true);
                EditorGUI.DrawRect(new Rect(position.x - 11, position.y + 5, 9, 9), Color.grey);
                property.isExpanded = true;
                return;
            }

            if(property.propertyType != SerializedPropertyType.Generic)
            {
                EditorGUI.HelpBox(position, "Unwrap Attribute only valid on Generic's (a serialized class)", MessageType.Error);
                return;
            }
            var props = property.GetAllVisiblePropertys();


            foreach (var prop in props)
            {
                position.height = EditorGUI.GetPropertyHeight(prop);
                DrawProperties.PropertyField(position, property: prop, label: new GUIContent(prop.name, prop.tooltip));
                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.IsArrayElement()) //is list element
            {
                return EditorGUI.GetPropertyHeight(property, true);
            }

            return property.GetAllVisiblePropertys().Select(_ => EditorGUI.GetPropertyHeight(_) + EditorGUIUtility.standardVerticalSpacing).Sum();
        }
    }
}

