using CustomInspector.Extensions;
using PlasticGui.WorkspaceWindow;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


namespace CustomInspector
{
    [CustomPropertyDrawer(typeof(HookAttribute))]
    public class HookAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            HookAttribute ha = (HookAttribute)attribute;

            if(PropertyValues.ContainsMethod(property, ha.methodPath, out (MethodInfo methodInfo, object owner) nakedMethod)) //contains without parameters
            {
                //method without parameters

                EditorGUI.BeginChangeCheck();
                DrawProperties.PropertyField(position, label, property, true);
                if (EditorGUI.EndChangeCheck())
                {
                    property.serializedObject.ApplyModifiedProperties();
                    try
                    {
                        nakedMethod.methodInfo.Invoke(nakedMethod.owner, new object[] { });
                    }
                    catch (Exception e) { Debug.LogException(e); }
                    property.serializedObject.ApplyModifiedFields(true);
                }
            }
            else
            {
                //method with parameters
                Type fieldType = fieldInfo.FieldType.IsArray ? fieldInfo.FieldType.GetElementType()
                                : fieldInfo.FieldType;
                Type[] pTypes = new Type[] { fieldType, fieldType };
                (MethodInfo methodInfo, object owner) method;
                try
                {
                    method = PropertyValues.GetMethod(property, ha.methodPath, pTypes);
                }
                catch (Exception e)
                {
                    DrawProperties.DrawFieldWithMessage(position, label, property, e.Message + " or without parameters", MessageType.Error);
                    return;
                }

                object oldValue = property.GetValue();
                EditorGUI.BeginChangeCheck();
                DrawProperties.PropertyField(position, label, property, true);
                if (EditorGUI.EndChangeCheck())
                {
                    property.serializedObject.ApplyModifiedProperties();
                    try
                    {
                        method.methodInfo.Invoke(method.owner, new object[] { oldValue, property.GetValue() });
                    }
                    catch (Exception e) { Debug.LogException(e); }
                    property.serializedObject.ApplyModifiedFields(true);
                }
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            HookAttribute ha = (HookAttribute)attribute;
            Type fieldType = fieldInfo.FieldType.IsArray ? fieldInfo.FieldType.GetElementType()
                            : fieldInfo.FieldType;

            if (PropertyValues.ContainsMethod(property, ha.methodPath, out (MethodInfo methodInfo, object owner) _, new Type[] { })
                || PropertyValues.ContainsMethod(property, ha.methodPath, out (MethodInfo methodInfo, object owner) _, new Type[] { fieldType, fieldType }))
            {
                return EditorGUI.GetPropertyHeight(property, label);
            }
            else
            {
                return DrawProperties.errorHeight + DrawProperties.errorSpacing
                    + EditorGUI.GetPropertyHeight(property, label) + EditorGUIUtility.standardVerticalSpacing;
            }
        }
    }
}
