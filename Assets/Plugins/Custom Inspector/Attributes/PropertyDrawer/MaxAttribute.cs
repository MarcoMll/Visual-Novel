using System;
using System.Diagnostics;
using UnityEngine;

namespace CustomInspector
{
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class MaxAttribute : PropertyAttribute
    {
        /// <summary>
        /// The maximum allowed value.
        /// </summary>
        public readonly float max;

        /// <summary>
        /// Attribute used to make a float or int variable in a script be restricted to a specific maximum  value.
        /// </summary>
        /// <param name="max">The maximum  allowed value.</param>
        public MaxAttribute(float max)
        {
            this.max = max;
            //Just before the MinAttribute
            order = -1;
        }
    }
}