using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using VisualNovel.UI.Animations;

/// <summary>
/// Custom drawer for <see cref="UIAnimSequence"/> that exposes the list of
/// <see cref="UIAnimStep"/> objects in the inspector. Allows adding new
/// concrete step types and reordering them.
/// </summary>
[CustomPropertyDrawer(typeof(UIAnimSequence))]
public class UIAnimSequenceDrawer : PropertyDrawer
{
    readonly Dictionary<string, ReorderableList> _lists = new();

    ReorderableList GetList(SerializedProperty property)
    {
        var steps = property.FindPropertyRelative("steps");
        if (_lists.TryGetValue(property.propertyPath, out var list))
        {
            try
            {
                var serializedProp = list.serializedProperty;
                if (serializedProp != null &&
                    serializedProp.serializedObject.targetObject != null &&
                    serializedProp == steps)
                {
                    return list;
                }
            }
            catch (Exception)
            {
                // The cached ReorderableList references a destroyed object.
                // Fall through so it can be rebuilt below.
            }

            _lists.Remove(property.propertyPath);
        }

        list = new ReorderableList(property.serializedObject, steps, true, true, true, true);
        list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Steps");
        list.elementHeightCallback = index =>
        {
            var element = steps.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(element, true) + 2f;
        };
        list.drawElementCallback = (rect, index, active, focused) =>
        {
            var element = steps.GetArrayElementAtIndex(index);
            rect.height = EditorGUI.GetPropertyHeight(element, true);
            // Display the step's type name so each element is clearly labeled.
            var typeName = "";
            var fullName = element.managedReferenceFullTypename;
            if (!string.IsNullOrEmpty(fullName))
            {
                var lastDot = fullName.LastIndexOf('.') + 1;
                var comma = fullName.IndexOf(',');
                if (comma < 0) comma = fullName.Length;
                typeName = fullName.Substring(lastDot, comma - lastDot);
            }
            EditorGUI.PropertyField(rect, element, new GUIContent(typeName), true);
        };
        list.onAddDropdownCallback = (rect, l) =>
        {
            var menu = new GenericMenu();
            foreach (var type in TypeCache.GetTypesDerivedFrom<UIAnimStep>())
            {
                if (type.IsAbstract) continue;
                menu.AddItem(new GUIContent(type.Name), false, () =>
                {
                    steps.arraySize++;
                    var element = steps.GetArrayElementAtIndex(steps.arraySize - 1);
                    element.managedReferenceValue = Activator.CreateInstance(type);
                    property.serializedObject.ApplyModifiedProperties();
                });
            }
            menu.ShowAsContext();
        };
        _lists[property.propertyPath] = list;
        return list;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var list = GetList(property);
        return EditorGUIUtility.singleLineHeight + 2f + list.GetHeight();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        var idProp = property.FindPropertyRelative("id");
        var list = GetList(property);

        var idRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(idRect, idProp);

        var listRect = new Rect(position.x, idRect.yMax + 2f, position.width, list.GetHeight());
        list.DoList(listRect);
        EditorGUI.EndProperty();
    }
}