using CustomInspector.Extensions;
using System;
using UnityEditor;
using UnityEngine;


namespace CustomInspector
{
    [CustomPropertyDrawer(typeof(DynamicSlider))]
    [CustomPropertyDrawer(typeof(DynamicSliderAttribute))]
    public class DynamicSliderDrawer : PropertyDrawer
    {
#if UNITY_EDITOR
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //Check type
            if (fieldInfo.FieldType != typeof(DynamicSlider))
            {
                EditorGUI.HelpBox(position, "DynamicSliderAttribute only valid on DynamicSlider", MessageType.Error);
                return;
            }

            position.height = EditorGUIUtility.singleLineHeight;

            //Get/test min max
            SerializedProperty min = property.FindPropertyRelative("min");
            SerializedProperty max = property.FindPropertyRelative("max");

            //set the fixed side to default value
            FixedSide fixedSide = (FixedSide)property.GetDirtyValue("fixedSide");
            switch (fixedSide)
            {
                case FixedSide.None:
                    break;
                case FixedSide.FixedMin:
                    float defaultMin = (float)property.GetDirtyValue("defaultMin");
                    if (min.floatValue != defaultMin)
                        min.floatValue = defaultMin;
                    break;
                case FixedSide.FixedMax:
                    float defaultMax = (float)property.GetDirtyValue("defaultMax");
                    if (max.floatValue != defaultMax)
                        max.floatValue = defaultMax;
                    break;
                default:
                    throw new NotImplementedException(fixedSide.ToString());
            }

            //test valid range
            if (min.floatValue == max.floatValue)
            {
                DrawProperties.DrawMessageField(position, "Dynamic sliders min and max limit are the same", MessageType.Warning);
                position.y += DrawProperties.errorHeight + DrawProperties.errorSpacing + EditorGUIUtility.standardVerticalSpacing;
            }

            //Draw Label (with foldout)
            GUIContent gc = new(property.name, property.tooltip);
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, gc);
            //Draw Slider
            SerializedProperty valueProperty = property.FindPropertyRelative("value");
            Rect sliderRect = new(position.x + EditorGUIUtility.labelWidth, position.y,
                                  position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.BeginChangeCheck();
            float res = EditorGUI.Slider(sliderRect, valueProperty.floatValue, min.floatValue, max.floatValue);
            if (EditorGUI.EndChangeCheck())
                valueProperty.floatValue = res;


            
            if (property.isExpanded)
            {
                using (new EditorGUI.IndentLevelScope(1))
                {

                    if (fixedSide != FixedSide.FixedMin)
                    {
                        position.y += position.height;
                        EditorGUI.BeginChangeCheck();
                        res = Math.Min(max.floatValue, EditorGUI.FloatField(position, new GUIContent("Custom min", "Change the min value of the slider above"), min.floatValue));
                        if (EditorGUI.EndChangeCheck())
                            min.floatValue = res;
                    }

                    if (fixedSide != FixedSide.FixedMax)
                    {
                        position.y += position.height;
                        EditorGUI.BeginChangeCheck();
                        res = Math.Max(min.floatValue, EditorGUI.FloatField(position, new GUIContent("Custom max", "Change the max value of the slider above"), max.floatValue));
                        if (EditorGUI.EndChangeCheck())
                            max.floatValue = res;
                    }
                }
            }


        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            //Check type
            if (fieldInfo.FieldType != typeof(DynamicSlider))
            {
                return EditorGUIUtility.singleLineHeight;
            }

            SerializedProperty min = property.FindPropertyRelative("min");
            SerializedProperty max = property.FindPropertyRelative("max");

            float errorHeight = 0;
            if(min.floatValue == max.floatValue)
            {
                errorHeight = DrawProperties.errorHeight + DrawProperties.errorSpacing + EditorGUIUtility.standardVerticalSpacing;
            }

            FixedSide fixedSide = (FixedSide)property.GetDirtyValue("fixedSide");

            if (property.isExpanded)
            {
                if (fixedSide == FixedSide.None)
                    return errorHeight + 3 * EditorGUIUtility.singleLineHeight + 3 * EditorGUIUtility.standardVerticalSpacing;
                else //one fixed side
                    return errorHeight + 2 * EditorGUIUtility.singleLineHeight + 2 * EditorGUIUtility.standardVerticalSpacing;
            }
            return errorHeight + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
#endif
    }
}