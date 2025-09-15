using CustomInspector.Extensions;
using System;
using UnityEditor;
using UnityEngine;


namespace CustomInspector
{
    [CustomPropertyDrawer(typeof(HorizontalLineAttribute))]
    public class HorizontalLineAttributeDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            HorizontalLineAttribute hl = (HorizontalLineAttribute)attribute;

            if (!string.IsNullOrEmpty(hl.message))
            {
                float height = Math.Max(hl.thickness, EditorGUIUtility.singleLineHeight);
                position.y += hl.spacing;
                //Message
                GUIContent label = new(hl.message);
                Vector2 labelWidth = GUI.skin.label.CalcSize(label);
                Rect messageRect = new(position.x + (position.width - labelWidth.x) / 2, position.y + height / 2 - labelWidth.y / 2,
                                            labelWidth.x, EditorGUIUtility.singleLineHeight);
                using (new FixedIndentLevel(0))
                {
                    EditorGUI.LabelField(messageRect, label);
                }
                //Line
                //up and down
                if (hl.thickness > labelWidth.y)
                {
                    Rect lineRect = new(messageRect.x, position.y,
                                             messageRect.width, (hl.thickness - labelWidth.y) / 2);
                    EditorGUI.DrawRect(lineRect, ToColor(hl.color));
                    lineRect.y += lineRect.height + labelWidth.y;
                    EditorGUI.DrawRect(lineRect, ToColor(hl.color));
                }
                //right and left
                if (messageRect.x > hl.gapSize) //enough space
                {
                    Rect lineRect = new(position.x + hl.gapSize, position.y + height / 2 - hl.thickness / 2,
                                    messageRect.x - position.x - hl.gapSize, hl.thickness);
                    EditorGUI.DrawRect(lineRect, ToColor(hl.color));
                    lineRect.x = messageRect.x + messageRect.width;
                    EditorGUI.DrawRect(lineRect, ToColor(hl.color));
                }
            }
            else //If there is ONLY the line (no string)
            {
                //Line
                Rect lineRect = new(position.x + hl.gapSize, position.y + hl.spacing,
                            position.width - 2 * hl.gapSize, hl.thickness);
                EditorGUI.DrawRect(lineRect, ToColor(hl.color));
            }
        }
        public override float GetHeight()
        {
            HorizontalLineAttribute hl = (HorizontalLineAttribute)attribute;
            if (!string.IsNullOrEmpty(hl.message))
            {
                return Math.Max(hl.thickness, EditorGUIUtility.singleLineHeight) + 2 * hl.spacing + EditorGUIUtility.standardVerticalSpacing;
            }
            else
                return hl.thickness + 2 * hl.spacing + EditorGUIUtility.standardVerticalSpacing;
        }
        private Color ToColor(FixedColor c) => FixedColorConvert.ToColor(c);
    }
}