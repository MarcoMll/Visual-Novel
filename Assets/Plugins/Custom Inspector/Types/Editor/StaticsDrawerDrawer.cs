using CustomInspector.Extensions;
using UnityEditor;
using UnityEngine;
using System.Reflection;


namespace CustomInspector
{
    //Draws all static properties of class. But only editable while playing! (because they cannot get serialized)
    [CustomPropertyDrawer(typeof(StaticsDrawer))]
    public class StaticPropertyDrawerDrawer : PropertyDrawer
    {
        const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUIUtility.singleLineHeight;

            GUIContent header = new("static fields:");
            if (!Application.isPlaying)
                header.tooltip = "You cannot change these values while not playing, because static variables doesnt get serialized";

            object obj = property.GetDirtyOwnerValue();
            FieldInfo[] fields = obj.GetType().GetFields(bindingFlags);

            if (fields.Length <= 0)
            {
                EditorGUI.LabelField(position, header, EditorStyles.boldLabel);
                EditorGUI.LabelField(position, new GUIContent(" "), new GUIContent("(no static fields found)"));
            }
            else
            {
                EditorGUI.LabelField(position, header, EditorStyles.boldLabel);
                using (new EditorGUI.IndentLevelScope(1))
                {
                    using (new EditorGUI.DisabledScope(!Application.isPlaying)) //changes doesnt get serialized and so on not saved anyways
                    {
                        foreach (FieldInfo field in fields)
                        {
                            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                            object v = field.GetValue(obj);

                            GUIContent content = new GUIContent(field.Name);
                            if (!Application.isPlaying)
                                content.tooltip = "You cannot change this value while not playing, because static variables doesnt get serialized";

                            EditorGUI.BeginChangeCheck();
                            object res = DrawProperties.DrawField(position, content, v, field.FieldType);
                            if(EditorGUI.EndChangeCheck())
                                field.SetValue(obj, res);
                        }
                    };
                }
            }
            
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            object obj = property.GetDirtyOwnerValue();
            FieldInfo[] fields = obj.GetType().GetFields(bindingFlags);

            return EditorGUIUtility.singleLineHeight //leadline
                + fields.Length * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
        }
    }
}