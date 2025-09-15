using CustomInspector.Extensions;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace CustomInspector
{
    [CustomPropertyDrawer(typeof(SetAttribute))]
#pragma warning disable CS0618 // Type or member is obsolete
    [CustomPropertyDrawer(typeof(SerializableSetAttribute))]
    [CustomPropertyDrawer(typeof(SerializableSortedSetAttribute))]
#pragma warning restore CS0618 // Type or member is obsolete
    public class SetDrawer : PropertyDrawer
    {
        const float buttonWidth = 100;
        const float horizontalSpacing = 5;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //Check type
            if (!ValidType())
            {
                DrawProperties.DrawFieldWithMessage(position, label, property,
                    "SerializableSetAttribute only valid on SerializableSet's", MessageType.Error);
                return;
            }

            //draw list
            SerializedProperty list = property.FindPropertyRelative("values");
            DrawProperties.ReadOnlyList.DrawList(position, label, list);

            if (!list.isExpanded)
                return;
            
            using (new EditorGUI.IndentLevelScope(1))
                position = EditorGUI.IndentedRect(position);
            
            position.y += EditorGUIUtility.singleLineHeight
                            + DrawProperties.ReadOnlyList.GetBodyHeight(list)
                            + 2 * EditorGUIUtility.standardVerticalSpacing;
            position.height = EditorGUIUtility.singleLineHeight;

            //Edit
            SerializedProperty editor_Input = GetInputProperty(property);
            if (editor_Input.propertyType == SerializedPropertyType.Generic)
            {
                property.isExpanded = false;
                EditorGUI.LabelField(position, "Generics have to be edited by script");
                return;
            }
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, new GUIContent("Edit"));
            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

            using (new EditorGUI.IndentLevelScope(1))
                position = EditorGUI.IndentedRect(position);

            if (property.isExpanded)
            {
                //float inputFieldHeight = EditorGUI.GetPropertyHeight(editor_Input);
                //Make add possible
                Rect buttonRect = new(position)
                {
                    width = buttonWidth,
                    height = EditorGUIUtility.singleLineHeight,
                };
                Rect inputRect = new(position)
                {   
                    x = buttonRect.x + buttonRect.width + horizontalSpacing,
                    width = position.width - (buttonWidth + horizontalSpacing),
                    height = EditorGUIUtility.singleLineHeight,
                };

                if(GUI.Button(buttonRect, new GUIContent("TryAdd", "Adds given element to the set")))
                {
                    property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    bool wasAdded = (bool)property.CallMethod("TryAdd", new object[] { editor_Input.GetValue() });
                    if (wasAdded)
                    {
                        if (!list.ApplyModifiedField(true))
                        {
                            Debug.LogWarning("TryAdd could not be saved");
                        }
                        EditorGUIUtility.keyboardControl = 0;
                    }
                    else Debug.Log("Set already contains key");
                }
                DrawProperties.PropertyField(inputRect, label: GUIContent.none, property: editor_Input);

                buttonRect.y += buttonRect.height + EditorGUIUtility.standardVerticalSpacing;
                inputRect.y = buttonRect.y;

                if (GUI.Button(buttonRect, new GUIContent("Remove", "Removes given element from the set")))
                {
                    property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    bool wasRemoved = (bool)property.CallMethod("Remove", new object[] { editor_Input.GetValue() });
                    if (wasRemoved)
                    {
                        if (!list.ApplyModifiedField(true))
                        {
                            Debug.LogWarning("Remove could not be saved");
                        }
                    }
                }
                DrawProperties.PropertyField(inputRect, label: GUIContent.none, property: editor_Input);

                buttonRect.y += buttonRect.height + EditorGUIUtility.standardVerticalSpacing;

                using (new EditorGUI.DisabledScope(list.arraySize <= 0))
                    if (GUI.Button(buttonRect, new GUIContent("Clear", "Removes all elements from the set")))
                    {
                        property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                        property.CallMethod("Clear");
                        if (!list.ApplyModifiedField(true))
                        {
                            Debug.LogWarning("Clear could not be saved");
                        }
                    }
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            //Check type
            if (!ValidType())
            {
                return EditorGUI.GetPropertyHeight(property, label, true) + DrawProperties.errorHeight + DrawProperties.errorSpacing;
            }

            //List
            SerializedProperty list = property.FindPropertyRelative("values");

            if (!list.isExpanded)
                return EditorGUIUtility.singleLineHeight;
            else
            {
                float listOpenSize = DrawProperties.ReadOnlyList.GetBodyHeight(list)
                + 2 * EditorGUIUtility.singleLineHeight + 2 * EditorGUIUtility.standardVerticalSpacing;

                if(property.isExpanded)
                    return listOpenSize + 3 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
                else
                    return listOpenSize;

            }
        }
        bool ValidType()
        {
            return fieldInfo.FieldType.IsGenericType &&
            (fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(SerializableSet<>)
               || fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(SerializableSortedSet<>));
        }
        SerializedProperty GetInputProperty(SerializedProperty set) => set.FindPropertyRelative("editor_input");
    }
}