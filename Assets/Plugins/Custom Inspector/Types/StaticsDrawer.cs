using System.Diagnostics;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CustomInspector
{
    /// <summary>
    /// This will display in the inspector all your static values
    /// </summary>
    [System.Serializable]
    public class StaticsDrawer
    {
#if UNITY_EDITOR
        [MessageBox("You are overriding the default PropertyDrawer of StaticsDrawer", MessageBoxType.Error)]
        bool b;
#endif
    }
}
