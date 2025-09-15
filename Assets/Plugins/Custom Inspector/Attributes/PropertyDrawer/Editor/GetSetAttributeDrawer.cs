using System.Reflection;
using UnityEditor;
using UnityEngine;
using CustomInspector.Extensions;
using System;

namespace CustomInspector
{

    [CustomPropertyDrawer(typeof(GetSetAttribute))]
    public class GetSetAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GetSetAttribute sm = (GetSetAttribute)attribute;

            //Get getter
            if (!PropertyValues.ContainsMethod(property, sm.getmethodPath, out (MethodInfo methodInfo, object owner) getmethod))
            {
                string errorMessage = $"Get-Method on {sm.getmethodPath} not found";
                DrawProperties.DrawFieldWithMessage(position, label, property, errorMessage, MessageType.Error);
                return;
            }
            //check getters return type
            if (getmethod.methodInfo.ReturnType == typeof(void))
            {
                string errorMessage = $"Get-Method {sm.getmethodPath} doesnt have a return value";
                DrawProperties.DrawFieldWithMessage(position, label, property, errorMessage, MessageType.Error);
                return;
            }
            //get setter with getters return type
            if (!PropertyValues.ContainsMethod(property, sm.setmethodPath, out (MethodInfo methodInfo, object owner) setmethod,
                                                parameterTypes: new Type[] { getmethod.methodInfo.ReturnType }))
            {
                string errorMessage = $"No set-Method on {sm.getmethodPath} with parameter '{getmethod.methodInfo.ReturnType}' found";
                DrawProperties.DrawFieldWithMessage(position, label, property, errorMessage, MessageType.Error);
                return;
            }

            //call getter
            object value;
            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            try
            {
                value = getmethod.methodInfo.Invoke(getmethod.owner, null);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                DrawProperties.DrawFieldWithMessage(position, label, property, "error in get-function. See console for more information", MessageType.Error);
                return;
            }
            //Draw Value
            EditorGUI.BeginChangeCheck();
            Rect getRect = new(position)
            {
                height = EditorGUI.GetPropertyHeight(PropertyConversions.ToPropertyType(getmethod.methodInfo.ReturnType), label),
            };
            GUIContent getSetLabel;
            string tooltip = sm.tooltip;
            if (sm.label is null)
                getSetLabel = new(ShowMethodAttributeDrawer.TryGetNameOuttaGetter(getmethod.methodInfo.Name), tooltip);
            else
                getSetLabel = new(sm.label, tooltip);

            var res = DrawProperties.DrawField(position: getRect, label: getSetLabel, value: value, getmethod.methodInfo.ReturnType);
            if (EditorGUI.EndChangeCheck())
            {
                //call setter
                try
                {
                    setmethod.methodInfo.Invoke(setmethod.owner, new object[] { res });
                }
                catch (Exception e) { Debug.LogException(e); }
                property.serializedObject.ApplyModifiedFields(true);
            }

            //Draw Property below
            Rect propRect = new(position)
            {
                y = position.y + getRect.height + EditorGUIUtility.standardVerticalSpacing,
                height = EditorGUI.GetPropertyHeight(property, label),
            };
            DrawProperties.PropertyField(propRect, label, property);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            GetSetAttribute sm = (GetSetAttribute)attribute;

            //Get getter
            if (!PropertyValues.ContainsMethod(property, sm.getmethodPath, out (MethodInfo methodInfo, object owner) getmethod))
                return DrawProperties.errorHeight + DrawProperties.errorSpacing + EditorGUI.GetPropertyHeight(property, label);

            //check getters return type
            if (getmethod.methodInfo.ReturnType == typeof(void))
                return DrawProperties.errorHeight + DrawProperties.errorSpacing + EditorGUI.GetPropertyHeight(property, label);

            //get setter with getters return type
            if (!PropertyValues.ContainsMethod(property, sm.setmethodPath, out (MethodInfo methodInfo, object owner) _,
                                                parameterTypes: new Type[] { getmethod.methodInfo.ReturnType }))
                return DrawProperties.errorHeight + DrawProperties.errorSpacing + EditorGUI.GetPropertyHeight(property, label);

            //Draw
            return EditorGUI.GetPropertyHeight(PropertyConversions.ToPropertyType(getmethod.methodInfo.ReturnType), label) + EditorGUIUtility.standardVerticalSpacing
                        + EditorGUI.GetPropertyHeight(property, label);
        }
    }
}