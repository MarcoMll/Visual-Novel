using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;


namespace CustomInspector
{

    /// <summary>
    /// Show field, if field (given by path/name) is equal to given value
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class ShowIfIsAttribute : PropertyAttribute
    {
        public readonly string fieldPath;
        public readonly object value;
        public DisabledStyle style = DisabledStyle.Invisible;

        public ShowIfIsAttribute(string fieldPath, object value)
        {
            order = -20;
            this.fieldPath = fieldPath;
            this.value = value;
        }
    }
}