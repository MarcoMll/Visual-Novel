using System;
using UnityEditor;
using UnityEngine;


namespace CustomInspector
{
    [CustomPropertyDrawer(typeof(MaskAttribute))]
    public class MaskAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUIUtility.singleLineHeight;

            if(property.propertyType == SerializedPropertyType.Integer)
            {
                MaskAttribute m = (MaskAttribute)attribute;

                EditorGUI.LabelField(position, label);
                Rect toggleRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight);

                for (int i = 0; i < m.bitsAmount; i++)
                {
                    EditorGUI.BeginChangeCheck();
                    bool res = EditorGUI.Toggle(toggleRect, (property.intValue & (1 << i)) != 0);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if(res)
                            property.intValue |= 1 << i;
                        else
                            property.intValue &= ~(1 << i);
                    }
                    toggleRect.x += EditorGUIUtility.singleLineHeight;
                    //if out of view
                    if (toggleRect.x > position.x + position.width)
                        break;
                }
            }
            else if(property.propertyType == SerializedPropertyType.Enum)
            {
                property.serializedObject.ApplyModifiedProperties();
                EditorGUI.BeginChangeCheck();
                Enum res = EditorGUI.EnumFlagsField(position, (Enum)Enum.ToObject(fieldInfo.FieldType, property.intValue));
                if (EditorGUI.EndChangeCheck())
                    property.intValue = (int)Convert.ChangeType(res, typeof(int));
            }
            else
            {
                EditorGUI.HelpBox(position, "MaskAttribute only supports integers and enums", MessageType.Error);
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}