using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace CustomInspector
{
    public enum LabelStyle
    {
        /// <summary>
        /// No label. draws the value over full width
        /// </summary>
        NoLabel,
        /// <summary>
        /// No label, but draws the value only in the value Area
        /// </summary>
        EmptyLabel,
        /// <summary>
        /// Makes the label as small as the label text
        /// </summary>
        NoSpacing,
        /// <summary>
        /// The common: label has default labelwith and value field starts in next column
        /// </summary>
        FullSpacing,
    }

    /// <summary>
    /// Change the variable label in the unity inspector
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class LabelSettingsAttribute : PropertyAttribute
    {
        public readonly LabelStyle labelStyle;
        public readonly string newName = null;

        public LabelSettingsAttribute(LabelStyle style)
        {
            order = -5;
            this.labelStyle = style;
        }
        public LabelSettingsAttribute(string newName, LabelStyle style = LabelStyle.FullSpacing)
        {
            order = -5;
            this.newName = newName;
            this.labelStyle = style;
        }
    }
}