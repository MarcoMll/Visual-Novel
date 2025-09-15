using CustomInspector.Extensions;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace CustomInspector
{
    //Displays autoproperties e.g. public string s1 { get; private set; }
    [CustomPropertyDrawer(typeof(DisplayAutoPropertyAttribute))]
    public class DisplayAutoPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUIUtility.singleLineHeight;

            DisplayAutoPropertyAttribute ap = (DisplayAutoPropertyAttribute)attribute;

            (string pre, string name) path = SplitInPreAndName(ap.propertyPath);
            string propertyPath = $"{path.pre}<{path.name}>k__BackingField";

            (object owner, _) = property.GetFieldInfos();
            object value;
            Type apSystemType;

            //property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            try
            {
                var res = owner.GetDirtyValueAndField(propertyPath);
                value = res.obj;
                apSystemType = res.fieldInfo.FieldType;
            }
            catch (Exception e) when (e is MissingFieldException || e is Exceptions.WrongTypeException)
            {
                DrawProperties.DrawFieldWithMessage(position, label, property, e.Message, MessageType.Error);
                return;
            }


            Rect apRect = new(position)
            {
                height = EditorGUI.GetPropertyHeight(PropertyConversions.ToPropertyType(apSystemType), label),
            };
            using (new EditorGUI.DisabledScope(!ap.allowChange || !Application.isPlaying)) //property wont be saved, so you shouldnt think you could do
            {
                EditorGUI.BeginChangeCheck();
                var res = DrawProperties.DrawField(apRect, GetGUIContent(path.name), value, apSystemType);
                if (EditorGUI.EndChangeCheck())
                {
                    owner.SetDirtyValue(propertyPath, res);
                }
            }

            //Draw Property below
            Rect propRect = new(position)
            {
                y = position.y + apRect.height + EditorGUIUtility.standardVerticalSpacing,
                height = EditorGUI.GetPropertyHeight(property, label),
            };
            DrawProperties.PropertyField(propRect, label, property);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            DisplayAutoPropertyAttribute ap = (DisplayAutoPropertyAttribute)attribute;
            (string pre, string name) path = SplitInPreAndName(ap.propertyPath);
            string propertyPath = $"{path.pre}<{path.name}>k__BackingField";

            (object owner, _) = property.GetFieldInfos();
            object value;
            Type apSystemType;
            try
            {
                var res = owner.GetDirtyValueAndField(propertyPath);
                value = res.obj;
                apSystemType = res.fieldInfo.FieldType;
            }
            catch (Exception e) when (e is MissingFieldException || e is Exceptions.WrongTypeException)
            {
                return DrawProperties.errorHeight + DrawProperties.errorSpacing
                    + EditorGUI.GetPropertyHeight(property, label) + EditorGUIUtility.standardVerticalSpacing;
            }

            SerializedPropertyType apType = PropertyConversions.ToPropertyType(apSystemType);
            GUIContent apLabel = GetGUIContent(path.name);
            return EditorGUI.GetPropertyHeight(type: apType, label: apLabel)
                + EditorGUIUtility.standardVerticalSpacing
                + EditorGUI.GetPropertyHeight(property, label);
        }
        GUIContent GetGUIContent(string fieldName)
        {
            DisplayAutoPropertyAttribute ap = (DisplayAutoPropertyAttribute)attribute;

            Debug.Assert(fieldName != null, "Field name not found");
            GUIContent content = ap.label == null ? new(fieldName) : new(ap.label);
            if(ap.tooltip != null)
                content.tooltip = ap.tooltip;

            return content;
        }
        (string pre, string name) SplitInPreAndName(string fullPath) //pre contains a dot if has prepath
        {
            int i = fullPath.LastIndexOf('.');
            if(i == -1)
            {
                return ("", fullPath);
            }
            else
            {
                i++;
                return (fullPath[..i], fullPath[i..]);
            }
        }
    }
}