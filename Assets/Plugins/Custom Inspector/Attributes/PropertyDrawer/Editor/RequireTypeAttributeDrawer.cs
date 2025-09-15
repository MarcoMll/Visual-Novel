using UnityEngine;
using UnityEditor;
using System.Linq;



namespace CustomInspector
{
    /// <summary>
    /// Draws an ObjectField constrained to given type like some interface
    /// </summary>
    [CustomPropertyDrawer(typeof(RequireTypeAttribute))]
    public class RequireTypeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //position.height = EditorGUI.GetPropertyHeight(property, label);
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                RequireTypeAttribute requiredAttribute = (RequireTypeAttribute)attribute;
                EditorGUI.BeginChangeCheck();
                var res = EditorGUI.ObjectField(position, label, property.objectReferenceValue, requiredAttribute.requiredType, true);
                if (EditorGUI.EndChangeCheck())
                    property.objectReferenceValue = res;
            }
            else
            {
                EditorGUI.HelpBox(position, $"RequireTypeAttribute is only valid for references", MessageType.Error);
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label) + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}