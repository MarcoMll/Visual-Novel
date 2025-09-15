using CustomInspector.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomInspector
{
    [CustomPropertyDrawer(typeof(DecimalsAttribute))]
    public class DecimalsAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DecimalsAttribute d = (DecimalsAttribute)attribute;

            if(property.propertyType == SerializedPropertyType.Float
                || property.propertyType == SerializedPropertyType.Integer)
            {
                double prevNumber = Convert.ToDouble(property.GetValue());
                EditorGUI.BeginChangeCheck();
                DrawProperties.PropertyField(position, label, property);
                if (EditorGUI.EndChangeCheck())
                {
                    double enteredNumber = Convert.ToDouble(property.GetValue());
                    double shift = Math.Pow(10, d.amount);
                    double number = ToNextInt(enteredNumber * shift, (int)(prevNumber * shift)) / shift;
                    property.SetValue(Convert.ChangeType(number, fieldInfo.FieldType));

                    static int ToNextInt(double value, int lastValue) //we want to round, but we also want to change the value
                    {
                        int newValue = value > 0 ? (int)(value + .5f) : (int)(value - .5f);
                        if (newValue == lastValue)
                        {
                            if (value > lastValue)
                                newValue = lastValue + 1;
                            else if (value < lastValue)
                                newValue = lastValue - 1;
                        }
                        return newValue;
                    }
                }
            }
            else
            {
                DrawProperties.DrawFieldWithMessage(position, label, property,
                                                    $"DecimalsAttribute only valid on number-types.\nMaybe use float instead.", MessageType.Error);
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.Float
                || property.propertyType == SerializedPropertyType.Integer)
            {
                return EditorGUI.GetPropertyHeight(property, label);
            }
            else
            {
                return DrawProperties.errorHeight + DrawProperties.errorSpacing + EditorGUI.GetPropertyHeight(property, label);
            }
        }
    }
}

