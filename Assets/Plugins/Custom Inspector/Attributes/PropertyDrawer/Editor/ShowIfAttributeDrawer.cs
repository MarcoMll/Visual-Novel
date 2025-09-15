using CustomInspector.Extensions;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CustomInspector
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ConditionalDrawer.OnGUI(position, property, label, attribute);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return ConditionalDrawer.GetPropertyHeight(property, label, attribute);
        }
    }

    [CustomPropertyDrawer(typeof(ShowIfNotAttribute))]
    public class ShowIfNotAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ConditionalDrawer.OnGUI(position, property, label, attribute);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return ConditionalDrawer.GetPropertyHeight(property, label, attribute);
        }
    }

    internal static class ConditionalDrawer
    {
        public static void OnGUI(Rect position, SerializedProperty property, GUIContent label, PropertyAttribute attribute)
        {
            //Check if not list element
            if (property.IsArrayElement()) //is element in a list
            {
                EditorGUI.HelpBox(position, "conditional-show not valid on list elements." +
                "(Hint: Put your list in a class, give it this attribute and UnwrapAttribute to hide and unhide the whole list)", MessageType.Error); //Pack lists in classes and show classes parallel
                return;
            }

            ShowIfAttribute conditionalAttribute = (ShowIfAttribute)attribute;
            bool condition;
            try
            {
                condition = ValueOfAllConditions(property, conditionalAttribute);
            }
            catch (Exception e) when (e is MissingMemberException || e is Exceptions.WrongTypeException)
            {
                DrawProperties.DrawFieldWithMessage(position, label, property, e.Message, MessageType.Error);
                return;
            }

            //Display
            using (new EditorGUI.IndentLevelScope(1))
            {
                switch (conditionalAttribute.style)
                {
                    case DisabledStyle.Invisible:
                        if (condition)
                        {
                            DrawProperties.PropertyField(position, label, property, true);
                        }
                        break;
                    case DisabledStyle.GreyedOut:
                        using (new EditorGUI.DisabledScope(!condition))
                        {
                            DrawProperties.PropertyField(position, label, property, true);
                        }
                        break;
                    default:
                        throw new System.NotImplementedException($"{conditionalAttribute.style}");
                }
            }
        }

        public static float GetPropertyHeight(SerializedProperty property, GUIContent label, PropertyAttribute attribute)
        {
            //Check if valid
            if (property.IsArrayElement()) //is element in a list
            {
                return 2 * EditorGUIUtility.singleLineHeight;
            }

            ShowIfAttribute conditionalAttribute = (ShowIfAttribute)attribute;
            bool condition;
            try
            {
                condition = ValueOfAllConditions(property, conditionalAttribute);
            }
            catch (Exception e) when (e is MissingMemberException || e is Exceptions.WrongTypeException)
            {
                return DrawProperties.errorHeight + DrawProperties.errorSpacing
                    + EditorGUI.GetPropertyHeight(property, label);
            }

            if (condition)
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }
            else
            {
                switch (conditionalAttribute.style)
                {
                    case DisabledStyle.Invisible:
                        (object owner, FieldInfo field) = property.GetFieldInfos();

                        Debug.Assert(field != null, $"{property.name} not found in {owner.GetType()}");
                        float space = 0;
                        foreach (Attribute attr in Attribute.GetCustomAttributes(field))
                        {
                            if (attr is SpaceAttribute s)
                                space += s.height;
                        }
                        return -(space + EditorGUIUtility.standardVerticalSpacing);

                    case DisabledStyle.GreyedOut:
                        return EditorGUI.GetPropertyHeight(property, label, true);

                    default:
                        throw new System.NotImplementedException($"{conditionalAttribute.style}");
                };
            }
        }
        static bool ValueOfAllConditions(SerializedProperty property, ShowIfAttribute conditionalAttribute)
        {
            string[] conditionPaths = conditionalAttribute.conditionPaths;

            bool condition;
            if(conditionalAttribute.op.HasValue)
            {
                switch (conditionalAttribute.op)
                {
                    case BoolOperator.And:
                        condition = conditionPaths.All(_ => GetBoolValue(property, _));
                        break;
                    case BoolOperator.Or:
                        condition = conditionPaths.Any(_ => GetBoolValue(property, _));
                        break;
                    default:
                        throw new System.NotImplementedException($"{conditionalAttribute.op}");
                };
            }
            else if(conditionalAttribute.comOp.HasValue)
            {
                switch (conditionalAttribute.comOp)
                {
                    case ComparisonOp.Equals:
                        var firstValue = property.GetOwnerAsFinder().FindPropertyRelative(conditionPaths[0]).GetValue();
                        condition = conditionPaths.All(_ => property.GetOwnerAsFinder().FindPropertyRelative(_).GetValue().Equals(firstValue));
                        break;
                    case ComparisonOp.NotNull:
                        condition = conditionPaths.All(_ => property.GetOwnerAsFinder().FindPropertyRelative(_).GetValue() != null);
                        break;
                    case ComparisonOp.Null:
                        condition = conditionPaths.All(_ => property.GetOwnerAsFinder().FindPropertyRelative(_).GetValue() == null);
                        break;
                    default:
                        throw new System.NotImplementedException($"{conditionalAttribute.comOp}");
                };
            }
            else
            {
                if(conditionPaths.Length > 1)
                    throw new ArgumentException("No operator or comparison provided to evalute multiple paths/names");
                condition = GetBoolValue(property, conditionPaths[0]);
            }
            return condition ^ conditionalAttribute.invert;


            /// <exception cref="MissingFieldException">If condition is not found</exception>
            /// <exception cref="WrongTypeException">If condition is not a boolean</exception>
            static bool GetBoolValue(SerializedProperty property, string conditionPath)
            {
                bool condition;

                //check if inverted
                bool inverted = false;
                if (conditionPath[0] == '!')
                {
                    inverted = true;
                    conditionPath = conditionPath.Remove(0, 1);
                }

                //Get value
                switch (conditionPath)
                {
                    case "True":
                    case "true":
                        condition = true;
                        break;

                    case "False":
                    case "false":
                        condition = false;
                        break;

                    case "IsPlaying":
                    case "isPlaying":
                        condition = Application.isPlaying;
                        break;

                    default:
                        SerializedProperty boolean = property.GetOwnerAsFinder().FindPropertyRelative(conditionPath);
                        if(boolean != null)
                        {
                            if (boolean.propertyType == SerializedPropertyType.Boolean)
                                condition = boolean.boolValue;
                            else
                                throw new Exceptions.WrongTypeException($"'{conditionPath}' is not type of boolean");
                        }
                        else
                        {
                            if (property.ContainsMethod(conditionPath, out (MethodInfo methodInfo, object owner) method))
                            {
                                if (method.methodInfo.ReturnType != typeof(bool))
                                    throw new Exceptions.WrongTypeException($"'{conditionPath}' returns not type of boolean");

                                property.serializedObject.ApplyModifiedPropertiesWithoutUndo(); //so the method gets right variables
                                try
                                {
                                    condition = (bool)method.methodInfo.Invoke(method.owner, null);
                                }
                                catch (Exception e)
                                {
                                    Debug.LogException(e);
                                    condition = true;
                                }
                            }
                            else
                            {
                                throw new MissingMemberException($"No method or field '{conditionPath}' on '{property.propertyPath.PrePath(false)}' found");
                            }
                        }

                        break;
                }
                return condition ^ inverted;
            }
        }

    }
}