using CustomInspector.Extensions;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CustomInspector
{
    [CustomPropertyDrawer(typeof(DictionaryAttribute))]
#pragma warning disable CS0618 // Type or member is obsolete
    [CustomPropertyDrawer(typeof(SerializableDictionaryAttribute))]
    [CustomPropertyDrawer(typeof(SerializableSortedDictionaryAttribute))]
#pragma warning restore CS0618 // Type or member is obsolete
    public class DictionaryDrawer : PropertyDrawer
    {
        const float elementLabelWidth = 70;

        const float buttonWidth = 100;
        const float horizontalSpacing = 5;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //Check type
            if (!ValidType())
            {
                DrawProperties.DrawFieldWithMessage(position, label, property,
                    "SerializableDictionaryAttribute only valid on SerializableDictionary's", MessageType.Error);
                return;
            }

            //draw dict
            SerializedProperty keys = property.FindPropertyRelative("keys.values");
            SerializedProperty values = property.FindPropertyRelative("values");

            DrawProperties.ReadOnlyList.DrawHeader(position, label, keys);
            if (!keys.isExpanded)
                return;

            using (new EditorGUI.IndentLevelScope(1))
            {
                position = EditorGUI.IndentedRect(position);
            }
            //draw dict body
            Rect labelsRect = new()
            {
                x = position.x,
                y = position.y + DrawProperties.ReadOnlyList.HeaderHeight
                                + EditorGUIUtility.standardVerticalSpacing,
                width = elementLabelWidth,
                height = EditorGUIUtility.singleLineHeight,
            };
            Rect keysRect = new(labelsRect)
            {
                x = labelsRect.x + labelsRect.width + horizontalSpacing,
                width = position.width / 2f - elementLabelWidth,
            };
            Rect valuesRect = new(keysRect)
            {
                x = keysRect.x + keysRect.width + horizontalSpacing,
                width = position.x + position.width - (keysRect.x + keysRect.width) - horizontalSpacing,
            };

            //Draw types of columns
            EditorGUI.LabelField(keysRect,
                new GUIContent($"key: {keys.GetFieldType().GetGenericArguments()[0].Name}"));
            EditorGUI.LabelField(valuesRect,
                new GUIContent($"value: {values.GetFieldType().GetGenericArguments()[0].Name}"));

            //Draw lists
            labelsRect.y += keysRect.height + EditorGUIUtility.standardVerticalSpacing;
            keysRect.y = labelsRect.y;
            valuesRect.y = labelsRect.y;

            DrawProperties.ReadOnlyList.DrawBody(labelsRect,
                                    Enumerable.Range(0, keys.arraySize).Select(_ => $"Element: {_}"), 
                                    withLabels: false);
            DrawProperties.ReadOnlyList.DrawBody(keysRect, keys, includeChildren: false, withLabels: false);
            DrawProperties.ReadOnlyList.DrawBody(false, valuesRect, values, includeChildren: false, withLabels: false);

            //Edit
            SerializedProperty editor_keyInput = property.FindPropertyRelative("editor_keyInput");
            SerializedProperty editor_valueInput = property.FindPropertyRelative("editor_valueInput");
            Rect foldoutRect = new(position)
            {
                y = keysRect.y + DrawProperties.ReadOnlyList.GetBodyHeight(keys)
                    + EditorGUIUtility.standardVerticalSpacing,
                height = EditorGUIUtility.singleLineHeight,
            };
            if (editor_keyInput.propertyType == SerializedPropertyType.Generic
                || editor_valueInput.propertyType == SerializedPropertyType.Generic)
            {
                values.isExpanded = false;
                EditorGUI.LabelField(foldoutRect, "Generics have to be edited by script");
                return;
            }
            values.isExpanded = EditorGUI.Foldout(foldoutRect, values.isExpanded,
                                                new GUIContent("Edit", "add or remove elements"));

            if (!values.isExpanded)
                return;


            using (new EditorGUI.IndentLevelScope(1))
            {
                position = EditorGUI.IndentedRect(position);
            }

            Rect buttonRect = new()
            {
                x = position.x,
                y = foldoutRect.y + foldoutRect.height + EditorGUIUtility.standardVerticalSpacing,
                width = buttonWidth,
                height = EditorGUIUtility.singleLineHeight,
            };
            Rect input1Rect = new()
            {
                x = buttonRect.x + buttonRect.width + horizontalSpacing,
                y = buttonRect.y,
                width = (position.width - (buttonWidth + horizontalSpacing)) / 2f,
                height = EditorGUIUtility.singleLineHeight,
            };
            Rect input2Rect = new(input1Rect)
            {
                x = input1Rect.x + input1Rect.width + horizontalSpacing,
            };

            if (GUI.Button(buttonRect, new GUIContent("TryAdd", "Adds given key/value pair to the dictionary")))
            {
                property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                bool wasAdded = (bool)property.CallMethod("TryAdd", new object[] { editor_keyInput.GetValue(), editor_valueInput.GetValue() });
                if(wasAdded)
                {
                    if (!property.ApplyModifiedField(true))
                    {
                        Debug.LogWarning("TryAdd could not be saved");
                    }
                    EditorGUIUtility.keyboardControl = 0;
                }
                else Debug.Log("Dictionary already contains key");
            }

            DrawProperties.PropertyField(input1Rect, label: GUIContent.none, property: editor_keyInput);
            DrawProperties.PropertyField(input2Rect, label: GUIContent.none, property: editor_valueInput);

            buttonRect.y += buttonRect.height + EditorGUIUtility.standardVerticalSpacing;
            input1Rect.y = buttonRect.y;

            if (GUI.Button(buttonRect, new GUIContent("Remove", "Removes entry with given key from the dictionary")))
            {
                property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                bool wasRemoved = (bool)property.CallMethod("Remove", new object[] { editor_keyInput.GetValue() });
                if (wasRemoved)
                {
                    if (!property.ApplyModifiedField(true))
                    {
                        Debug.LogWarning("Remove could not be saved");
                    }
                }
            }
            DrawProperties.PropertyField(input1Rect, label: GUIContent.none, property: editor_keyInput);

            buttonRect.y += buttonRect.height + EditorGUIUtility.standardVerticalSpacing;

            using (new EditorGUI.DisabledScope(keys.arraySize <= 0))
                if (GUI.Button(buttonRect, new GUIContent("Clear", "Removes all key/value pairs from the dictionary")))
                {
                    property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    property.CallMethod("Clear", new object[] {  });
                    if (!property.ApplyModifiedField(true))
                    {
                        Debug.LogWarning("Clear could not be saved");
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

            SerializedProperty keys = property.FindPropertyRelative("keys.values");
            if (keys.isExpanded)
            {
                float listHeight = 2 * EditorGUIUtility.singleLineHeight
                                    + DrawProperties.ReadOnlyList.GetBodyHeight(keys)
                                    + 3 * EditorGUIUtility.standardVerticalSpacing;

                SerializedProperty values = property.FindPropertyRelative("values");

                if (!values.isExpanded)
                    return listHeight + EditorGUIUtility.singleLineHeight;
                else
                    return listHeight + 4 * EditorGUIUtility.singleLineHeight + 3 * EditorGUIUtility.standardVerticalSpacing;
            }
            else
                return EditorGUIUtility.singleLineHeight;
        }
        bool ValidType()
        {
            return fieldInfo.FieldType.IsGenericType &&
            (fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(SerializableDictionary<,>)
               || fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(SerializableSortedDictionary<,>));
        }
    }
}