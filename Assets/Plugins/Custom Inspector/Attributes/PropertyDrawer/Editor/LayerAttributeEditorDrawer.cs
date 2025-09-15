using Codice.Client.BaseCommands;
using CustomInspector.Extensions;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


namespace CustomInspector
{
    [CustomPropertyDrawer(typeof(LayerAttribute))]
    class LayerAttributeDrawer : PropertyDrawer
    {
        const float fixButtonWidth = 40;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            LayerAttribute l = (LayerAttribute)attribute;

            Rect rect;

            if(!string.IsNullOrEmpty(l.requiredName))
            {
                int req = LayerMask.NameToLayer(l.requiredName);

                if(req == -1)
                {
                    //wrong layer name entered

                    Rect errorRect = new(position)
                    {
                        y = position.y + DrawProperties.errorSpacing,
                        height = DrawProperties.errorHeight,
                    };
                    EditorGUI.HelpBox(errorRect, $"LayerName {l.requiredName} not found"
                                        + "\nTyping error or Layer was removed.", MessageType.Error);
                    rect = new(position)
                    {
                        y = errorRect.y + errorRect.height,
                        height = EditorGUIUtility.singleLineHeight,
                    };
                }
                else if(req == property.intValue)
                {
                    rect = position;
                }
                else
                {
                    Rect errorRect = new(position)
                    {
                        y = position.y + DrawProperties.errorSpacing,
                        width = position.width - fixButtonWidth,
                        height = DrawProperties.errorHeight,
                    };
                    EditorGUI.HelpBox(errorRect, $"{property.name}'s value does not match the code's: {l.requiredName}", MessageType.Warning);
                    Rect buttonRect = new(errorRect)
                    {
                        x = errorRect.x + errorRect.width,
                        width = fixButtonWidth,
                    };
                    if(GUI.Button(buttonRect, new GUIContent("Fix", $"set Layer to {l.requiredName}")))
                    {
                        property.intValue = LayerMask.NameToLayer(l.requiredName);
                    }

                    rect = new(position)
                    {
                        y = errorRect.y + errorRect.height,
                        height = EditorGUIUtility.singleLineHeight,
                    };
                }
            }
            else
            {
                rect = position;
            }

            EditorGUI.BeginChangeCheck();
            int res = EditorGUI.LayerField(rect, label, property.intValue);
            if (EditorGUI.EndChangeCheck())
            {
                property.intValue = res;
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            LayerAttribute l = (LayerAttribute)attribute;
            if (!string.IsNullOrEmpty(l.requiredName))
            {
                int req = LayerMask.NameToLayer(l.requiredName);
                if (req == -1 || req != property.intValue)
                    return DrawProperties.errorSpacing + DrawProperties.errorHeight + EditorGUIUtility.singleLineHeight;
                else
                    return EditorGUIUtility.singleLineHeight;
            }
            else
                return EditorGUIUtility.singleLineHeight;
        }
    }
}