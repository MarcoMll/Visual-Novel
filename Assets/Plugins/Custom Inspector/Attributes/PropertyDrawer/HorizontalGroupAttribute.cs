using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CustomInspector
{
    /// <summary>
    /// Define horizontalgroups in the unity inspector
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class HorizontalGroupAttribute : PropertyAttribute
    {
        public readonly bool beginNewGroup = false;
        public float size = 1;

        /// <summary>
        /// Define horizontalgroups in the unity inspector
        /// </summary>
        /// <param name="beginNewGroup">If you have two horizontal groups behind each other you can split these groups with this tag</param>
        /// <param name="size">Defines the proportion to other members of the group. 2 means it will consume double the space of the others</param>
        public HorizontalGroupAttribute(bool beginNewGroup = false)
        {
            order = -100; //has to on top of all

            if (size < 0)
                size = 0;
            this.beginNewGroup = beginNewGroup;
        }
    }
}