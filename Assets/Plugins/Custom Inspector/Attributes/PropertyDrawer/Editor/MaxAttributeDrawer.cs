using CustomInspector.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;


namespace CustomInspector
{
    [CustomPropertyDrawer(typeof(MaxAttribute))]
    public class MaxAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            DrawProperties.PropertyField(position, label, property);
            if (EditorGUI.EndChangeCheck())
            {
                MaxAttribute ma = (MaxAttribute)attribute;
                switch (property.propertyType)
                {
                    case SerializedPropertyType.Integer:
                        property.intValue = CapInt(property.intValue);
                        break;
                    case SerializedPropertyType.Float:
                        property.floatValue = Cap(property.floatValue);
                        break;

                    case SerializedPropertyType.Vector2Int:
                        Vector2Int v2i = property.vector2IntValue;
                        property.vector2IntValue = new Vector2Int(CapInt(v2i.x), CapInt(v2i.y));
                        break;
                    case SerializedPropertyType.Vector2:
                        Vector2 v2 = property.vector2Value;
                        property.vector2Value = new Vector2(Cap(v2.x), Cap(v2.y));
                        break;

                    case SerializedPropertyType.Vector3Int:
                        Vector3Int v3i = property.vector3IntValue;
                        property.vector3IntValue = new Vector3Int(CapInt(v3i.x), CapInt(v3i.y), CapInt(v3i.z));
                        break;
                    case SerializedPropertyType.Vector3:
                        Vector3 v3 = property.vector3Value;
                        property.vector3Value = new Vector3(Cap(v3.x), Cap(v3.y), Cap(v3.z));
                        break;

                    case SerializedPropertyType.Vector4:
                        Vector4 v4 = property.vector4Value;
                        property.vector4Value = new Vector4(Cap(v4.x), Cap(v4.y), Cap(v4.z), Cap(v4.w));
                        break;
                }
                int CapInt(int i) => Math.Min(i, (int)ma.max);
                float Cap(float f) => Mathf.Min(f, ma.max);
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label) + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}

