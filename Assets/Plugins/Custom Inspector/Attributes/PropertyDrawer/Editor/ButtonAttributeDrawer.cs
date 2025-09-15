using UnityEngine;
using UnityEditor;
using System.Reflection;
using CustomInspector.Extensions;
using System;
using static Codice.CM.WorkspaceServer.WorkspaceTreeDataStore;
using NUnit.Framework;
using System.Linq;
using UnityEngine.Windows;

namespace CustomInspector
{
    [CustomPropertyDrawer(typeof(ButtonAttribute))]
    public class ButtonAttributeDrawer : PropertyDrawer
    {
        const float maxInputWidth = 100;
        const float horizontalSpacing = 5;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ButtonAttribute ib = (ButtonAttribute)attribute;

            GUIContent buttonLabel;
            if (ib.label == null)
                buttonLabel = new(PropertyConversions.NameFormat(PropertyConversions.NameOfPath(ib.methodPath)), ib.tooltip);
            else
                buttonLabel = new(ib.label, ib.tooltip);
            float buttonWidth = GetWidth(position, buttonLabel, ib.size);

            float buttonHeight = GetButtonHeight(ib.size);

            if (ib.usePropertyAsParameter) //use field as input
            {
                Type fieldType = fieldInfo.FieldType.IsArray ? fieldInfo.FieldType.GetElementType()
                                : fieldInfo.FieldType;
                Type[] pTypes = new Type[] { fieldType };

                if (PropertyValues.ContainsMethod(property, ib.methodPath, out (MethodInfo methodInfo, object owner) method, pTypes))
                {
                    float savedLabelWidth = EditorGUIUtility.labelWidth;
                    GUIContent inputLabel = new(PropertyConversions.NameFormat(label.text), label.tooltip + "\n-This will be passed in the function as paramter");
                    EditorGUIUtility.labelWidth = Mathf.Min( GUI.skin.label.CalcSize(inputLabel).x, position.width/4f);

                    Rect buttonRect = new(x: position.x, y: position.y,
                                        width: Math.Min(position.width - EditorGUIUtility.fieldWidth - horizontalSpacing, buttonWidth), //zumindest platz für ein field
                                        height: buttonHeight);

                    position.y = buttonRect.y + (buttonRect.height - EditorGUIUtility.singleLineHeight) / 2;
                    float inputSpace = position.width - buttonRect.width - horizontalSpacing;
                    float input_width = Math.Min(inputSpace, maxInputWidth);
                    input_width = Math.Max(input_width, inputSpace - (GUI.skin.label.CalcSize(inputLabel).x + 20));
                    Rect inputRect = new(x: position.x + position.width - input_width,
                                              y: position.y,
                                              width: input_width,
                                              height: EditorGUIUtility.singleLineHeight);

                    float inputLabel_width = position.width - buttonRect.width - horizontalSpacing - inputRect.width;
                    if(inputLabel_width > 0)
                    {
                        Rect inputLabelRect = new(x: buttonRect.x + buttonRect.width + horizontalSpacing,
                                             y: position.y,
                                             width: inputLabel_width,
                                             height: EditorGUIUtility.singleLineHeight);

                        using (new FixedIndentLevel(0))
                        {
                            EditorGUI.LabelField(inputLabelRect, inputLabel);
                        }
                    }

                    using (new FixedIndentLevel(0))
                    {
                        DrawProperties.PropertyField(inputRect, GUIContent.none, property, false);
                    }

                    if (GUI.Button(buttonRect, buttonLabel))
                    {
                        object inputValue = property.GetValue();
                        Debug.Assert(inputValue.GetType() == fieldType, $"Mismatched type: {inputValue.GetType()} not same type as {fieldType}");
                        var input = new object[] { inputValue };

                        if (Selection.count <= 1)
                        {
                            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                            try
                            {
                                method.methodInfo.Invoke(method.owner, input);
                            }
                            catch(Exception e) { Debug.LogException(e); }
                            property.serializedObject.ApplyModifiedFields(true);
                        }
                        else //multiediting
                        {
                            var serializedObjects = property.serializedObject.targetObjects.Select(_ => new SerializedObject(_)).ToList();

                            foreach (var so in serializedObjects)
                            {
                                so.ApplyModifiedPropertiesWithoutUndo();
                            }
                            foreach (var so in serializedObjects)
                            {
                                (MethodInfo methodInfo, object owner) = so.targetObject.GetMethod(property.propertyPath.PrePath(true) + ib.methodPath);
                                try
                                {
                                    methodInfo.Invoke(owner, input);
                                }
                                catch (Exception e) { Debug.LogException(e); }
                            }
                            foreach (var so in serializedObjects)
                            {
                                so.ApplyModifiedFields(true);
                                so.Dispose();
                            }
                        }
                    }

                    EditorGUIUtility.labelWidth = savedLabelWidth;
                }
                else
                {
                    DrawProperties.DrawFieldWithMessage(position, label, property,
                                $"No method \"{ib.methodPath}\" with parameter {pTypes[0]} on \"{property.GetDirtyOwnerValue().GetType()}\" found", MessageType.Error);
                }
            }
            else //just the default: only the button
            {
                Rect buttonRect = new(position.x + (position.width - buttonWidth) / 2, position.y,
                                    buttonWidth, buttonHeight);

                if (PropertyValues.ContainsMethod(property, ib.methodPath, out (MethodInfo methodInfo, object owner) method))
                {
                    if (GUI.Button(buttonRect, buttonLabel))
                    {
                        if (Selection.count <= 1)
                        {
                            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                            try
                            {
                                method.methodInfo.Invoke(method.owner, null);
                            }
                            catch (Exception e)
                            {
                                Debug.LogException(e);
                            }
                            property.serializedObject.ApplyModifiedFields(true);
                        }
                        else //multiediting
                        {
                            var serializedObjects = property.serializedObject.targetObjects.Select(_ => new SerializedObject(_)).ToList();

                            foreach (var so in serializedObjects)
                            {
                                so.ApplyModifiedPropertiesWithoutUndo();
                            }
                            foreach (var so in serializedObjects)
                            {
                                (MethodInfo methodInfo, object owner) = so.targetObject.GetMethod(property.propertyPath.PrePath(true) + ib.methodPath);
                                try
                                {
                                    methodInfo.Invoke(owner, null);
                                }
                                catch (Exception e) { Debug.LogException(e); }
                            }
                            foreach (var so in serializedObjects)
                            {
                                so.ApplyModifiedFields(true);
                                so.Dispose();
                            }
                        }
                        
                    }
                    
                    position.y += buttonRect.height + EditorGUIUtility.standardVerticalSpacing;
                }
                else
                {
                    position.height = buttonRect.x + buttonRect.height;
                    EditorGUI.HelpBox(position, $"No method \"{ib.methodPath}\" without parameters on \"{property.GetDirtyOwnerValue().GetType()}\" found", MessageType.Error);
                    position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                }

                //And the field below
                position.height = EditorGUI.GetPropertyHeight(property, label, true);
                EditorGUI.PropertyField(position, property, new GUIContent(property.name, property.tooltip), true);
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ButtonAttribute ib = (ButtonAttribute)attribute;
            if (ib.usePropertyAsParameter)
            {
                Type fieldType = fieldInfo.FieldType.IsArray ? fieldInfo.FieldType.GetElementType()
                    : fieldInfo.FieldType;
                Type[] pTypes = new Type[] { fieldType };

                if (PropertyValues.ContainsMethod(property, ib.methodPath, out var _, pTypes))
                    return GetButtonHeight(ib.size) + EditorGUIUtility.standardVerticalSpacing;
                else
                    return EditorGUI.GetPropertyHeight(property, label) + DrawProperties.errorHeight + DrawProperties.errorSpacing;
            }
            else
                return GetButtonHeight(ib.size) + EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(property, label, true);
        }
        public float GetWidth(Rect position, GUIContent buttonLabel, Size size)
        {
            return Mathf.Min(position.width, Math.Max(GUI.skin.label.CalcSize(buttonLabel).x + 20, size switch
            {
                Size.small => 100,
                Size.medium => 50 + position.width / 4,
                Size.big => 50 + position.width / 2,
                Size.max => float.MaxValue,
                _ => throw new System.NotImplementedException(size.ToString()),
            }));
        }
        public float GetButtonHeight(Size size)
        {
            return size switch
            {
                Size.small => EditorGUIUtility.singleLineHeight,
                Size.medium => 30,
                Size.big => 40,
                Size.max => 60,

                _ => throw new System.NotImplementedException(size.ToString()),
            };
        }
    }
}