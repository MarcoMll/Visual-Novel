using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using CustomInspector.Extensions;
using System.Reflection;
using System;
using System.Linq;
using System.Collections;

namespace CustomInspector
{
    [CustomPropertyDrawer(typeof(HorizontalGroupAttribute))]
    public class HorizontalGroupAttributeDrawer : PropertyDrawer
    {
        /// <summary>
        /// Range: [0, 1]
        /// 1 means label takes whole width and field is zero width
        /// </summary>
        const float labelFieldProportion = 0.4f;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float propertysSpacing = position.width / 25f; // Distance between two properties in same horizontal group

            //Check if not list element
            if (property.IsArrayElement()) //is element in a list
            {
                EditorGUI.HelpBox(position, "HorizontalGroup not valid on lists", MessageType.Error); //Pack lists in classes and show classes parallel
                return;
            }

            //Get values
            HorizontalGroupAttribute hg = (HorizontalGroupAttribute)attribute;
            var props = GetAllPropsInGroup(property);

            //Check again
            if (props.Count == 1) //alone in group
            {
                if (hg.beginNewGroup)
                    DrawProperties.DrawFieldWithMessage(position, label, property, $"{property.name} is alone in a horizontal group. Maybe set \"beginNewGroup\"=false to let him join the previous group", MessageType.Warning);
                else
                    DrawProperties.DrawFieldWithMessage(position, label, property, $"unnecessary assignment of HorizontalGroupAttribute ({property.name} is alone in a horizontal group). All members of the group must have the attribute and stand behind each other in the code", MessageType.Warning);
                return;
            }

            //Draw
            float customTotalWidth = props.Select(_ => _.size).Sum();
            float realWidth = (position.width - (props.Count - 1) * propertysSpacing);
            float labelAndfieldWidth = realWidth / customTotalWidth * hg.size; //withPer = (width - spacing) / amount * myAmount
            int myIndex = props.FindIndex(_ => _.prop.name.Equals(property.name));
            float startX = position.x;
            for (int i = 0; i < myIndex; i++)
            {
                startX += realWidth / customTotalWidth * props[i].size + propertysSpacing;
            }

            Rect rect = new Rect(startX, position.y,
                                    labelAndfieldWidth, EditorGUIUtility.singleLineHeight);

            float oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = labelAndfieldWidth * labelFieldProportion;
            DrawProperties.PropertyField(rect, label, property, true);
            EditorGUIUtility.labelWidth = oldLabelWidth;
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            //Check if valid
            if (property.IsArrayElement()) //is element in a list
            {
                return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            var props = GetAllPropsInGroup(property);
            //Check again
            if(props.Count == 1)
            {
                return DrawProperties.errorHeight + DrawProperties.errorSpacing
                        + EditorGUI.GetPropertyHeight(property, label) + EditorGUIUtility.standardVerticalSpacing;
            }

            // Get height
            if (props[^1].prop.name.Equals(property.name)) //is last
            {
                heights.Push((property.name, EditorGUI.GetPropertyHeight(property, label)));
                for (int i = 0; i < props.Count - 1; i++)
                {
                    //let themselves enter in heights dict if they are bigger
                    EditorGUI.GetPropertyHeight(props[i].prop);
                }
                (_, float maxHeight) = heights.Pop();
                return maxHeight;
            }
            else
            {
                //wenn der letzte gerade sucht, dann meine dazu
                string lastName = props[^1].prop.name;
                if (heights.Count > 0 && heights.Peek().name == lastName)
                {
                    float currentMax = heights.Pop().maxHeight;
                    float myHeight = EditorGUI.GetPropertyHeight(property, label);
                    heights.Push((lastName, Mathf.Max(currentMax, myHeight)));
                }
                
                return -EditorGUIUtility.standardVerticalSpacing;
            }
        }
        static Stack<(string name, float maxHeight)> heights = new();

        /// <summary>
        /// Get all propertys in the same group as the given property
        /// </summary>
        List<(SerializedProperty prop, float size)> GetAllPropsInGroup(SerializedProperty property)
        {
            var owner = property.GetOwnerAsFinder();

            List<(SerializedProperty prop, float size)> props = new();

            bool foundMyGroup = false; //we are looking for the group the property is in

            foreach (SerializedProperty prop in owner.GetAllPropertys())
            {
                //Check is in group
                HorizontalGroupAttribute hg = (HorizontalGroupAttribute)Attribute.GetCustomAttribute(prop.GetFieldInfos().fieldInfo, typeof(HorizontalGroupAttribute));
                if (hg is null)
                {
                    if (foundMyGroup)
                        break;
                    else
                    {
                        props.Clear();
                        continue;
                    }
                }
                if(hg.beginNewGroup == true)
                {
                    if (foundMyGroup)
                        break;
                    else
                        props.Clear();
                }
                //Add
                props.Add((prop, hg.size));
                if (prop.name == property.name)
                    foundMyGroup = true;
            }
            //Test
            Debug.Assert(props.Count > 0, "Property group is empty");
            //Return
            return props;
        }
    }
}