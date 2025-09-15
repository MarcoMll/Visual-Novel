using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace CustomInspector
{
    /// <summary>
    /// Calls the given function, when variable got changed in the unity inspector
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public class HookAttribute : PropertyAttribute
    {
        public readonly string methodPath;

        public HookAttribute(string methodPath)
        {
            this.methodPath = methodPath;
            order = -10;
        }
    }
}