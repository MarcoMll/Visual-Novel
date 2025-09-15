using CustomInspector.Extensions;
using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomInspector
{

    [CustomPropertyDrawer(typeof(ShowIfIsAttribute))]
    public class ShowIfIsAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowIfIsAttribute sa = (ShowIfIsAttribute)attribute;

            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();

            //Get value
            object refValue;
            try
            {
                refValue = property.GetDirtyOwnerValue().GetDirtyValue(sa.fieldPath);
            }
            catch(Exception e)
            {
                //error - could not get value
                DrawProperties.DrawFieldWithMessage(position, label, property, e.Message, MessageType.Error);
                return;
            }

            if((refValue == null && sa.value == null)
                || (refValue != null && refValue.Equals(sa.value)))
            {
                //Show
                position.height = EditorGUI.GetPropertyHeight(label: label, property: property);
                using (new EditorGUI.IndentLevelScope(1))
                {
                    DrawProperties.PropertyField(position, label: label, property: property);
                }
                return;
            }
            else
            {
                //Hide
                return;
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ShowIfIsAttribute sa = (ShowIfIsAttribute)attribute;

            //Get value
            object refValue;
            try
            {
                refValue = property.GetDirtyOwnerValue().GetDirtyValue(sa.fieldPath);
            }
            catch
            {
                //error display
                return EditorGUI.GetPropertyHeight(label: label, property: property)
                        + DrawProperties.errorSpacing + DrawProperties.errorHeight;
            }

            if ((refValue == null && sa.value == null)
                || (refValue != null && refValue.Equals(sa.value)))
            {
                //Show
                return EditorGUI.GetPropertyHeight(label: label, property: property);
            }
            else
            {
                //Hide
                return -EditorGUIUtility.standardVerticalSpacing;
            }
        }
    }
}

