using System;
using System.Diagnostics;
using UnityEngine;


namespace CustomInspector
{
    /// <summary>
    /// Displays a progress bar instead of your number. Progressbar is full of you reached given max
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class ProgressBarAttribute : PropertyAttribute
    {
        public readonly Size size;
        public readonly float max;
        public ProgressBarAttribute(float max, Size size = Size.medium)
        {
            this.max = max;
            this.size = size;
        }
    }
}
