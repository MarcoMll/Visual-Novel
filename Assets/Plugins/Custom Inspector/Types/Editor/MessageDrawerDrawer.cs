using CustomInspector.Extensions;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace CustomInspector
{
    /// <summary>
    /// Draws a message if there are some in MessageDrawer
    /// </summary>
    [CustomPropertyDrawer(typeof(MessageDrawer))]
    public class MessageDrawerDrawer : PropertyDrawer
    {
#if UNITY_EDITOR
        public const int messageSize = 35;

        const float minSize = 350; //size at what the spacing disappears
        const float spacing = 0.2f; //proportion of helpbox start

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            MessageDrawer md = (MessageDrawer)property.GetDirtyValue();

            if (md != null)
            {
                List<(string content, MessageBoxType type)> messages = md.messages;
                if (messages.Count > 0)
                {
                    int savedIndentLevel = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 0;
                    {
                        Rect messageRect = new (position);
                        float space = Mathf.Min(Mathf.Max(position.width - minSize, 0), position.width * spacing);
                        messageRect.x += space;
                        messageRect.width -= space;
                        messageRect.height = messageSize;
                        for (int i = 0; i < messages.Count; i++)
                        {
                            EditorGUI.HelpBox(messageRect, messages[i].content, MessageBoxConvert.ToUnityMessageType(messages[i].type));

                            messageRect.y += messageSize;
                            messageRect.y += EditorGUIUtility.standardVerticalSpacing;
                        }

                    }
                    EditorGUI.indentLevel = savedIndentLevel;
                }
            }
            else
            {
                position.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.HelpBox(position, $"MessageDrawer is null", MessageType.Error);
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            MessageDrawer md = (MessageDrawer)property.GetValue();

            if (md == null)
            {
                return EditorGUIUtility.singleLineHeight; //error that messagedrawer is null
            }
            else
            {
                return (messageSize + EditorGUIUtility.standardVerticalSpacing) * md.messages.Count;
            }
        }
#endif
    }
}