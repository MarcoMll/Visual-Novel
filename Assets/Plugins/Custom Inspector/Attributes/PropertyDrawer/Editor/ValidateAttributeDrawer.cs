using UnityEditor;
using UnityEngine;
using CustomInspector.Extensions;
using System;
using System.Reflection;
using UnityEngine.UIElements;

namespace CustomInspector
{
    [CustomPropertyDrawer(typeof(ValidateAttribute))]
    public class ValidateAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            bool isValid = IsValid(property, out string message);
            property.serializedObject.ApplyModifiedFields(false); //this function is not made for changes, but why not preventing wierd behaviour
            if (isValid)
                DrawProperties.PropertyField(position, label, property);
            else
                DrawProperties.DrawFieldWithMessage(position, label, property, $"{property.name}: " + message, MessageBoxConvert.ToUnityMessageType(((ValidateAttribute)attribute).errorType));
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (IsValid(property, out _))
                return EditorGUI.GetPropertyHeight(property, label) + EditorGUIUtility.standardVerticalSpacing;
            else
                return DrawProperties.errorSpacing + DrawProperties.errorHeight
                    + EditorGUI.GetPropertyHeight(property, label, true) + EditorGUIUtility.standardVerticalSpacing;
        }

        bool IsValid(SerializedProperty property, out string errorMessage)
        {
            ValidateAttribute va = (ValidateAttribute)attribute;

            string methodPath = va.methodPath;

            Type fieldType = fieldInfo.FieldType.IsArray ? fieldInfo.FieldType.GetElementType()
                            : fieldInfo.FieldType;
            Type[] pTypes = new Type[] { fieldType };
            (MethodInfo methodInfo, object owner) method;

            bool hasParams;
            try
            {
                method = property.GetMethod(methodPath);
                hasParams = false;
            }
            catch(MissingMethodException)
            {
                try
                {
                    method = property.GetMethod(methodPath, pTypes);
                    hasParams = true;
                }
                catch (MissingMethodException e)
                {
                    errorMessage = e.Message + " or without parameters";
                    return false;
                }
                catch (Exception e)
                {
                    errorMessage = e.Message;
                    return false;
                }
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                return false;
            }


            if (method.methodInfo.ReturnType == typeof(bool))
            {
                var @params = hasParams ? new object[] { property.GetValue() } : new object[0];
                bool res;
                try
                {
                    res = (bool)method.methodInfo.Invoke(method.owner, @params);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    errorMessage = "error in validate-method. See console for more information";
                    return false;
                }

                if (res)
                {
                    errorMessage = null;
                    return true;
                }
                else
                {
                    errorMessage = va.errorMessage;
                    return false;
                }

            }
            else
            {
                errorMessage = $"{methodPath}'s return type is not typeof bool";
                return false;
            }
        }
    }
}