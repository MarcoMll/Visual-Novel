using CustomInspector.Extensions;
using UnityEditor;
using UnityEngine;

namespace CustomInspector
{
    [CustomPropertyDrawer(typeof(AssetsOnlyAttribute))]
    public class AssetsOnlyAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //position.height = EditorGUI.GetPropertyHeight(property, label);

            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                DrawProperties.DrawFieldWithMessage(position, label, property,
                                "SceneObjectsOnlyAttribute only supports ObjectReferences", MessageType.Error);
                return;
            }

            string infoMessage = "AssetsOnly:\nyou cannot fill sceneObjects in here";
            GUIContent newLabel = new(label.text, string.IsNullOrEmpty(label.tooltip) ? infoMessage : $"{label.tooltip}\n{infoMessage}");

            EditorGUI.BeginChangeCheck();
            var res = EditorGUI.ObjectField(position: position, label: newLabel, obj: property.objectReferenceValue, objType: fieldInfo.FieldType, allowSceneObjects: false);
            if (EditorGUI.EndChangeCheck())
                property.objectReferenceValue = res;
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference)
                return EditorGUI.GetPropertyHeight(property, label) + EditorGUIUtility.standardVerticalSpacing;
            else
                return DrawProperties.errorHeight + DrawProperties.errorSpacing
                    + EditorGUI.GetPropertyHeight(property, label) + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}