using System.Reflection;
using UnityEditor;
using UnityEngine;
using CustomInspector.Extensions;
using System;
using System.Linq;
using System.Collections.Generic;

namespace CustomInspector
{

    [CustomPropertyDrawer(typeof(ShowMethodAttribute))]
    public class ShowMethodAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowMethodAttribute sm = (ShowMethodAttribute)attribute;
            //GUIContent proplabel = new(property.name, property.tooltip);

            if (!PropertyValues.ContainsMethod(property, sm.getmethodPath, out (MethodInfo methodInfo, object owner) getmethod))
            {
                string errorMessage = $"ShowMethod: Method on {sm.getmethodPath} not found";
                DrawProperties.DrawFieldWithMessage(position, label, property, errorMessage, MessageType.Error);
                return;
            }
            
            //Check
            if (getmethod.methodInfo.ReturnType == typeof(void))
            {
                DrawProperties.DrawFieldWithMessage(position, label, property, $"ShowMethod: given method {sm.getmethodPath} doesnt have a return value", MessageType.Error);
                return;
            }

            //Call getter
            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            object value;
            try
            {
                value = getmethod.methodInfo.Invoke(getmethod.owner, null);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                DrawProperties.DrawFieldWithMessage(position, label, property, "error in method. See console for more information", MessageType.Error);
                return;
            }

            //Draw
            Rect methodRect = new(position)
            {
                height = EditorGUIUtility.singleLineHeight
            };
            string infoMessage = $"The result of {property.serializedObject.targetObject.name}.{property.serializedObject.targetObject.GetType().Name}.{property.propertyPath.PrePath(true)}{sm.getmethodPath}()";
            GUIContent methodLabel = new()
            {
                text = sm.label ?? TryGetNameOuttaGetter(getmethod.methodInfo.Name),
                tooltip = string.IsNullOrEmpty(sm.tooltip) ? infoMessage : sm.tooltip + "\n" + infoMessage
            };

            DrawProperties.DrawField(methodRect, methodLabel, value, getmethod.methodInfo.ReturnType, disabled: true);

            //Draw property below
            Rect propRect = new()
            {
                x = position.x,
                y = methodRect.y + methodRect.height + EditorGUIUtility.standardVerticalSpacing,
                height = EditorGUI.GetPropertyHeight(property, label),
                width = position.width,
            };
            DrawProperties.PropertyField(propRect, label, property);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ShowMethodAttribute sm = (ShowMethodAttribute)attribute;

            float height = EditorGUI.GetPropertyHeight(property, label);

            if (PropertyValues.ContainsMethod(property, sm.getmethodPath, out (MethodInfo methodInfo, object owner) getmethod))
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            else
                height += DrawProperties.errorHeight + DrawProperties.errorSpacing;
            
            return height;
        }
        public static string TryGetNameOuttaGetter(string getterName)
        {
            if(getterName.Length >= 3
                && (getterName[0] == 'G' || getterName[0] == 'g')
                && (getterName[1] == 'E' || getterName[1] == 'e')
                && (getterName[2] == 'T' || getterName[2] == 't'))
                    getterName = getterName[3..];

            if(getterName.Length > 1)
            {
                List<char> newName = getterName.ToList();
                for (int i = 1; i < newName.Count; i++)
                {
                    if (newName[i] >= 'A' && newName[i] <= 'Z') //uppercase
                    {
                        newName.Insert(i + 1, (char)(newName[i] + 'a' - 'A'));
                        newName[i] = ' ';
                    }
                }
                return String.Join(null, newName.Select(_ => _.ToString()));
            }
            else return getterName;
        }
    }
}