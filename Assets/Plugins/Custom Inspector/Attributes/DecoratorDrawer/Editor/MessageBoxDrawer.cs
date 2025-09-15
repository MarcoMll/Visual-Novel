using UnityEngine;
using UnityEditor;
using CustomInspector.Extensions;

namespace CustomInspector
{
    /// <summary>
    /// Draws the field name and behind a custom message
    /// </summary>
    [CustomPropertyDrawer(typeof(MessageBoxAttribute))]
    public class MessageBoxDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            position.y += DrawProperties.errorSpacing;
            MessageBoxAttribute hv = (MessageBoxAttribute)attribute;
            position.size = new Vector2(position.width, hv.height);
            EditorGUI.HelpBox(position, hv.content, MessageBoxConvert.ToUnityMessageType(hv.type));
                
        }

        public override float GetHeight()
        {
            MessageBoxAttribute hv = (MessageBoxAttribute)attribute;
            return DrawProperties.errorSpacing + hv.height + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}