using UnityEditor;
using UnityEngine;


namespace CustomInspector
{
    [CustomPropertyDrawer(typeof(TagAttribute))]
    public class TagAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            string res = EditorGUI.TagField(position, label, property.stringValue);
            if (EditorGUI.EndChangeCheck())
            {
                property.stringValue = res;
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}