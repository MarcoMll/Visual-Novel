using NUnit.Framework;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static CustomInspector.Extensions.PropertyValues;
using static UnityEngine.UI.GridLayoutGroup;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace CustomInspector.Extensions
{
    public static class DrawProperties
    {
        public static void PropertyField(Rect position, GUIContent label, SerializedProperty property, bool includeChildren = true)
        {
            if (label.text == null) //even GUIContent.none has text = "" instead of null
                label = new(PropertyConversions.NameFormat(property.name), property.tooltip);
            EditorGUI.PropertyField(position: position, label: label, property: property, includeChildren: includeChildren);
        }
        /// <summary>
        /// Generic show label in the inspector and returns user input
        /// </summary>
        public static System.Object DrawField(Rect position, object value, Type fieldType, bool disabled = false)
            => DrawField(position, GUIContent.none, value, fieldType, disabled);
        public static System.Object DrawField<T>(Rect position, GUIContent label, T value, Type fieldType, bool disabled = false)
        {
            UnityEngine.Debug.Assert(value is not SerializedProperty, "Your drawing the serialization instead of the actual object. Use DrawProperties.PropertyField instead");

            if (value is null)
            {
                return EditorGUI.ObjectField(position, label, obj: null, objType: fieldType, true);
            }

            //we have to top align the label for wide-mode=true
            Rect labelRect = new(position)
            {
                height = EditorGUIUtility.singleLineHeight,
            };
            EditorGUI.LabelField(labelRect, label);

            GUIContent g = new(" ");
            using (new EditorGUI.DisabledScope(disabled))
            {
                switch (value)
                {
                    case int i:
                        return EditorGUI.IntField(position, g, i);
                    case bool b:
                        return EditorGUI.Toggle(position, g, b);
                    case float f:
                        return EditorGUI.FloatField(position, g, f);
                    case string s:
                        return EditorGUI.TextField(position, g, s);
                    case Color c:
                        return EditorGUI.ColorField(position, g, c);
                    case Enum e:
                        return EditorGUI.EnumFlagsField(position, g, e);
                    case Vector2Int v2i:
                        return EditorGUI.Vector2IntField(position, g, v2i);
                    case Vector2 v2:
                        return EditorGUI.Vector2Field(position, g, v2);
                    case Vector3Int v3i:
                        return EditorGUI.Vector3IntField(position, g, v3i);
                    case Vector3 v3:
                        return EditorGUI.Vector3Field(position, g, v3);
                    case Vector4 v4:
                        return EditorGUI.Vector4Field(position, g, v4);
                    case RectInt ri:
                        return EditorGUI.RectIntField(position, g, ri);
                    case Rect r:
                        return EditorGUI.RectField(position, g, r);
                    case AnimationCurve ac:
                        return EditorGUI.CurveField(position, g, ac);
                    case BoundsInt bi:
                        return EditorGUI.BoundsIntField(position, g, bi);
                    case Bounds b:
                        return EditorGUI.BoundsField(position, g, b);
                    case Quaternion q:
                        return EditorGUI.Vector4Field(position, g, new Vector4(q.x, q.y, q.z, q.w)).ToQuaternion();
                    case Object o:
                        return EditorGUI.ObjectField(position, label: g, obj: o, objType: fieldType, true);
                    default:
                        EditorGUI.LabelField(position, g, new GUIContent(value.ToString(), label.tooltip));
                        return value;
                };
            }
        }
        public static void DrawLabelSettings(Rect position, SerializedProperty property, GUIContent label, LabelStyle style)
        {
            switch (style)
            {
                case LabelStyle.NoLabel:
                    DrawProperties.PropertyField(position, GUIContent.none, property, true);
                    break;

                case LabelStyle.EmptyLabel:
                    DrawProperties.PropertyField(position, new GUIContent(" "), property, true);
                    break;

                case LabelStyle.NoSpacing:
                    if(property.propertyType == SerializedPropertyType.Generic)
                        DrawProperties.PropertyField(position, label, property, true);
                    else
                    {
                        float labelWidth = GUI.skin.label.CalcSize(label).x + 5; //some additional distance
                        EditorGUI.LabelField(position, label);
                        Rect propRect = new(position)
                        {
                            x = position.x + labelWidth,
                            width = position.width - labelWidth,
                        };
                        DrawProperties.PropertyField(propRect, GUIContent.none, property, true);
                    }
                    break;

                case LabelStyle.FullSpacing:
                    DrawProperties.PropertyField(position, label, property, true);
                    break;
                default:
                    throw new NotImplementedException(style.ToString());
            }
        }
        /// <summary>
        /// Additional spacing to before field. So that you know where the error belongs to
        /// </summary>
        public const float errorSpacing = 5;
        public const float errorHeight = 40;
        /// <summary>
        /// Draws the field with position, but inserts an error before
        /// </summary>
        /// <param name="position"></param>
        /// <param name="label"></param>
        /// <param name="property"></param>
        /// <param name="errorMessage"></param>
        /// <param name="type"></param>
        /// <param name="includeChildren"></param>
        public static void DrawFieldWithMessage(Rect position, GUIContent label, SerializedProperty property, string errorMessage, MessageType type, bool includeChildren = true, bool disabled = false)
        {
            position.y += errorSpacing;
            Rect rect = new (position)
            {
                height = errorHeight
            };
            EditorGUI.HelpBox(rect, errorMessage, type);
            position.y += errorHeight;
            position.height = EditorGUI.GetPropertyHeight(property, label);

            if (string.IsNullOrEmpty(label.text)) //was a fix for a unity bug, where label was empty
                label = new(property.name, property.tooltip);

            if(disabled)
                DisabledPropertyField(position, label, property, includeChildren);
            else
                DrawProperties.PropertyField(position, label: label, property: property, includeChildren: includeChildren);
        }
        public static void DisabledPropertyField(Rect position, GUIContent label, SerializedProperty property, bool includeChildren = true)
        {
            Rect labelRect = new(position)
            {
                height = EditorGUIUtility.singleLineHeight,
            };
            EditorGUI.LabelField(labelRect, label);
            using (new EditorGUI.DisabledScope(true))
                DrawProperties.PropertyField(position: position, label: new GUIContent(" "), property: property, includeChildren: includeChildren);
        }
        /// <summary>
        /// Draws a helpbox with specific height
        /// </summary>
        /// <param name="position"></param>
        /// <param name="errorMessage"></param>
        /// <param name="type"></param>
        public static void DrawMessageField(Rect position, string errorMessage, MessageType type)
        {
            position.y += errorSpacing;
            Rect rect = new (position)
            {
                height = errorHeight
            };
            EditorGUI.HelpBox(rect, errorMessage, type);
            position.y += errorHeight;
        }

        /// <summary> Draws a border around given rect </summary>
        /// <param name="extendOutside">If the thickness makes the border bigger(true) or the space in the middle smaller</param>
        public static void DrawBorder(Rect position, bool extendOutside, float thickness = 1) => DrawBorder(position, new Color(0.5f, 0.5f, 0.5f, 1f), extendOutside, thickness);
        /// <summary> Draws a border around given rect </summary>
        public static void DrawBorder(Rect position, Color color, bool extendOutside, float thickness = 1)
        {
            if(extendOutside)
            {
                //Up down
                Rect up = new(position.x - thickness, position.y - thickness,
                                    position.width + 2 * thickness, thickness);
                EditorGUI.DrawRect(up, color);
                up.y += position.height + thickness;
                EditorGUI.DrawRect(up, color);

                //right left
                Rect left = new(position.x - thickness, position.y - thickness,
                                    thickness, position.height + 2 * thickness);
                EditorGUI.DrawRect(left, color);
                left.x += position.width + thickness;
                EditorGUI.DrawRect(left, color);
            }
            else
            {
                //Up down
                Rect up = new(position.x, position.y,
                                    position.width, thickness);
                EditorGUI.DrawRect(up, color);
                up.y += position.height - thickness;
                EditorGUI.DrawRect(up, color);

                //right left
                Rect left = new(position.x, position.y,
                                    thickness, position.height);
                EditorGUI.DrawRect(left, color);
                left.x += position.width - thickness;
                EditorGUI.DrawRect(left, color);
            }
        }

        public static class ReadOnlyList
        {
            /// <summary>
            /// Draws a readonly list with EditorGUILayout in the inspector
            /// </summary>
            [System.Diagnostics.Conditional("UNITY_EDITOR")]
            public static void DrawList(Rect position, GUIContent label, SerializedProperty property)
            {
                TestIfList(property);

                DrawHeader(position, label, property);
                position.y += HeaderHeight + EditorGUIUtility.standardVerticalSpacing;

                if (property.isExpanded)
                    using (new EditorGUI.IndentLevelScope(1))
                    {
                        DrawBody(position, property);
                    }

            }
            [System.Diagnostics.Conditional("UNITY_EDITOR")]
            public static void DrawList<T>(Rect position, SerializedProperty owner, IEnumerable<T> list, bool withLabels = true)
            {
                if (owner == null)
                    throw new NullReferenceException("SerializedProperty cannot be null");

                owner.isExpanded = EditorGUILayout.Foldout(owner.isExpanded, new GUIContent(owner.name, owner.tooltip));
                if (owner.isExpanded)
                {
                    using (new EditorGUI.IndentLevelScope(1))
                    {
                        position = EditorGUI.IndentedRect(position);
                    }
                    DrawBody(position, list, withLabels);
                }
            }
            public static float HeaderHeight => EditorGUIUtility.singleLineHeight;
            [System.Diagnostics.Conditional("UNITY_EDITOR")]
            public static void DrawHeader(Rect position, GUIContent label, SerializedProperty property)
            {
                TestIfList(property);

                position.height = HeaderHeight;
                label.text += $" (Count: {property.arraySize})";
                property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
            }
            /// <summary>
            /// Draws a readonly list without top label or foldout
            /// </summary>
            [System.Diagnostics.Conditional("UNITY_EDITOR")]
            public static void DrawBody(Rect position, SerializedProperty property, bool withLabels = true, bool includeChildren = true)
                => DrawBody(isReadonly: true, position: position, property: property, withLabels: withLabels, includeChildren: includeChildren);
            /// <summary>
            /// Draws the list readonly if condition true, without top label or foldout
            /// </summary>
            [System.Diagnostics.Conditional("UNITY_EDITOR")]
            public static void DrawBody(bool isReadonly, Rect position, SerializedProperty property, bool withLabels = true, bool includeChildren = true)
            {
                TestIfList(property);

                if (property.arraySize > 0)
                {
                    if (withLabels)
                    {
                        for (int i = 0; i < property.arraySize; i++)
                        {
                            SerializedProperty prop = property.GetArrayElementAtIndex(i);
                            GUIContent label = new($"Element: {i}");
                            position.height = EditorGUI.GetPropertyHeight(prop, label , includeChildren);
                            EditorGUI.LabelField(position, label);

                            if(prop.propertyType != SerializedPropertyType.Generic)
                            {
                                using (new EditorGUI.DisabledScope(isReadonly))
                                {
                                    DrawProperties.PropertyField(position, new GUIContent(" "), prop, includeChildren);
                                }
                            }
                            else
                                EditorGUI.LabelField(position, " ", prop.GetValue()?.ToString() ?? "(null)");

                            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                        }
                    }
                    else // no labels
                    {
                        using (new EditorGUI.DisabledScope(isReadonly))
                        {
                            for (int i = 0; i < property.arraySize; i++)
                            {
                                SerializedProperty prop = property.GetArrayElementAtIndex(i);
                                position.height = EditorGUI.GetPropertyHeight(prop, includeChildren);
                                if (prop.propertyType != SerializedPropertyType.Generic)
                                    DrawProperties.PropertyField(position, GUIContent.none, prop, includeChildren);
                                else
                                    EditorGUI.LabelField(position, prop.GetValue()?.ToString() ?? "(null)");
                                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                            }
                        }
                    }
                }
                else // arraysize 0
                {
                    GUIContent content = new("(empty)");
                    //center align
                    float width = GUI.skin.label.CalcSize(content).x + 20;
                    Rect infoRect = new(position)
                    {
                        x = position.x + (position.width - width) / 2f,
                        width = width,
                        height = EditorGUIUtility.singleLineHeight,
                    };
                    EditorGUI.LabelField(infoRect, content);
                }
            }
            public static void DrawBody<T>(Rect position, IEnumerable<T> list, bool withLabels = true)
            {
                var e = list.GetEnumerator();
                position.height = EditorGUIUtility.singleLineHeight;
                if (!e.MoveNext())
                {
                    EditorGUI.LabelField(position, "(empty)");
                }
                else
                {
                    if(withLabels)
                    {
                        int i = 0;
                        do
                        {
                            EditorGUI.LabelField(position, new GUIContent($"Element: {i++}"),
                                                           new GUIContent($"{e.Current}"));
                            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                        }
                        while (e.MoveNext());
                    }
                    else
                    {
                        do
                        {
                            EditorGUI.LabelField(position, new GUIContent($"{e.Current}"));
                            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                        }
                        while (e.MoveNext());
                    }
                }
            }
            public static float GetBodyHeight(SerializedProperty list)
            {
                TestIfList(list);
                return Math.Max(list.arraySize, 1)
                        * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            }

            [System.Diagnostics.Conditional("UNITY_EDITOR")]
            static void TestIfList(SerializedProperty property)
            {
                if (property == null)
                    throw new NullReferenceException("SerializedProperty cannot be null");

                if (!property.isArray)
                    throw new Exceptions.WrongTypeException($"Given SerializedProperty({property.propertyPath}) is not a list/array." +
                    $"\nUse DrawList<T>(SerializedProperty property, List<T> list) instead to pass custom list");
            }
        }
}
    public static class PropertyValues
    {
        public const BindingFlags defaultBindingFlags = BindingFlags.Instance
                                                      | BindingFlags.Static
                                                      | BindingFlags.Public
                                                      | BindingFlags.NonPublic
                                                      | BindingFlags.FlattenHierarchy;

        //Get values
        /// <summary>
        /// A generic way to get the serialized value of an existing serialized property
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public static object GetValue(this SerializedProperty property)
        {
            Debug.Assert(property != null, "property is null");
            return property.propertyType switch
            {
                SerializedPropertyType.AnimationCurve => property.animationCurveValue,
                SerializedPropertyType.Boolean => property.boolValue,
                SerializedPropertyType.BoundsInt => property.boundsIntValue,
                SerializedPropertyType.Bounds => property.boundsValue,
                SerializedPropertyType.Color => property.colorValue,
                //SerializedPropertyType.Double => serializedProperty.doubleValue,
                SerializedPropertyType.ExposedReference => property.exposedReferenceValue,
                SerializedPropertyType.Float /* or double */ => GetSingleOrDouble(),
                SerializedPropertyType.Integer => property.intValue,
                //SerializedPropertyType.Long => property.longValue,
                SerializedPropertyType.ManagedReference => property.managedReferenceValue,
                //SerializedPropertyType.ObjectReferenceInstanceID => property.objectReferenceInstanceIDValue,
                SerializedPropertyType.ObjectReference => property.objectReferenceValue,
                SerializedPropertyType.LayerMask => property.intValue,
                SerializedPropertyType.ArraySize => property.arraySize,
                //SerializedPropertyType.Character => property.characterValue,
                //SerializedPropertyType.Gradient => property.gradientValue,
                SerializedPropertyType.Quaternion => property.quaternionValue,
                SerializedPropertyType.FixedBufferSize => property.fixedBufferSize,
                SerializedPropertyType.RectInt => property.rectIntValue,
                SerializedPropertyType.Rect => property.rectValue,
                SerializedPropertyType.String => property.stringValue,
                SerializedPropertyType.Vector2Int => property.vector2IntValue,
                SerializedPropertyType.Vector2 => property.vector2Value,
                SerializedPropertyType.Vector3Int => property.vector3IntValue,
                SerializedPropertyType.Vector3 => property.vector3Value,
                SerializedPropertyType.Vector4 => property.vector4Value,
                SerializedPropertyType.Hash128 => property.hash128Value,


                SerializedPropertyType.Enum => Enum.ToObject(property.GetFieldType(), property.intValue),
                SerializedPropertyType.Generic => GetGeneric(),

                _ => throw new NotSupportedException($"({property.propertyType} not supported)")
            };

            object GetSingleOrDouble()
            {
                Type type = property.GetFieldType();
                if (type == typeof(double))
                    return property.doubleValue;
                else
                    return property.floatValue;
            }
            object GetGeneric()
            {
                if (property.isArray)
                {
                    try
                    {
                        Type type = property.GetFieldType();
                        IList res = (IList)Activator.CreateInstance(type);
                        foreach (object item in property.GetAllPropertys().Select(_ => _.GetValue()))
                        {
                            res.Add(item);
                        }
                        return res;
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        return null;
                    }
                }
                else
                {
                    Type type = property.GetFieldType();
                    object res = Activator.CreateInstance(type);
                    var props = property.GetAllPropertys();

                    foreach (var prop in props)
                    {
                        FieldInfo field = type.GetField(prop.name, defaultBindingFlags);
                        Debug.Assert(field != null, $"Field '{prop.name}' not found in '{type}'");
                        field.SetValue(res, prop.GetValue());
                    }

                    return res;
                }
            }
        }

        /// <summary>
        /// A generic way to get the serialized value of an existing serialized property
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public static object GetValue(this SerializedProperty property, string propertyPath)
        {
            SerializedProperty prop = property.FindPropertyRelative(propertyPath);
#if UNITY_EDITOR
            if(prop == null)
            {
                if(property.GetDirtyValue(propertyPath) != null)
                    throw new InvalidOperationException($"Due to '{property.propertyPath}'+'{propertyPath}' not being serialized, it cannot be found." +
                                                        $"\nMaybe use SerializedProperty.GetDirtyValue(propertyPath) instead.");
                else
                    throw new KeyNotFoundException($"No serialized field on '{property.propertyPath}'+'{propertyPath}' found.");
            }
#endif
            return prop.GetValue();
        }


        /// <summary>
        /// Get the value through reflection from current unity instantiation
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="MissingFieldException"></exception>
        /// <exception cref="Exceptions.WrongTypeException"></exception>
        public static object GetDirtyValue(this SerializedProperty property, BindingFlags bindingAttr = defaultBindingFlags)
        => GetDirtyValue(property.serializedObject.targetObject, property.propertyPath, bindingAttr);
        /// <summary>
        /// Get the value through reflection from current unity instantiation
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="MissingFieldException"></exception>
        /// <exception cref="Exceptions.WrongTypeException"></exception>
        public static object GetDirtyValue(this SerializedProperty property, string propertyPath, BindingFlags bindingAttr = defaultBindingFlags)
        => property.GetDirtyValue().GetDirtyValue(propertyPath, bindingAttr);
        /// <summary>
        /// Get the value through reflection from current unity instantiation
        /// </summary>
        /// <exception cref="ArgumentException">path begins with '.' </exception>
        /// <exception cref="MissingFieldException">A field name on the path is not found</exception>
        /// <exception cref="Exceptions.WrongTypeException">If path has wrong format</exception>
        public static object GetDirtyValue(this object targetObject, string propertyPath, BindingFlags bindingAttr = defaultBindingFlags)
            => GetDirtyValueAndField(targetObject: targetObject, propertyPath: propertyPath, bindingAttr).obj;
        /// <summary>
        /// Get the value through reflection from current unity instantiation
        /// </summary>
        /// <exception cref="ArgumentException">path begins with '.' </exception>
        /// <exception cref="MissingFieldException">A field name on the path is not found</exception>
        /// <exception cref="Exceptions.WrongTypeException">If path has wrong format</exception>
        public static (object obj, FieldInfo fieldInfo) GetDirtyValueAndField(this object targetObject, string propertyPath, BindingFlags bindingAttr = defaultBindingFlags)
        {
            UnityEngine.Debug.Assert(targetObject != null, "TargetObject is empty");
            UnityEngine.Debug.Assert(!string.IsNullOrEmpty(propertyPath), "propertyPath is empty");

            if (propertyPath[0] == '.')
                throw new ArgumentException("Path cannot begin with '.'");

            string[] propertys = propertyPath.Split('.');

            FieldInfo field = null;
            if (propertys.Length > 0)
            {
                for (int i = 0; i < propertys.Length; i++)
                {
                    if (propertys[i] != "Array")
                    {
                        field = targetObject.GetType().GetField(propertys[i], bindingAttr);
                        if (field != null)
                        {
                            targetObject = field.GetValue(targetObject);
                        }
                        else
                        {
                            throw new MissingFieldException($"Field '{propertys[i]}' not found in '{targetObject}'");
                        }
                    }
                    else
                    {
                        if (++i < propertys.Length) //wenn es ein element der liste ist : MyClassName.myArrayName.Array.data[i]
                        {
                            int.TryParse(propertys[i][5..^1], out int dataIndex); //Its naming is always "data[i]"

                            if (targetObject is IList list)
                            {
                                targetObject = list[dataIndex];
                            }
                            else
                            {
                                throw new Exceptions.WrongTypeException($"Expected collection instead of '{targetObject.GetType()}'");
                            }
                            continue;
                        }
                    }
                }
            }
            Debug.Assert(field != null, "unknown error");
            return (targetObject, field);
        }
        public static object GetDirtyOwnerValue(this SerializedProperty property)
        {
            if (property == null)
                throw new NullReferenceException("serializedProperty is null");

            //Get owner path
            string prePath = property.propertyPath.PrePath(false);
            if (string.IsNullOrEmpty(prePath)) //no owner
            {
                return property.serializedObject.targetObject;
            }
            else
            {
                return GetDirtyValue(property.serializedObject.targetObject, prePath);
            }
        }
        public static IFindProperties GetOwnerAsFinder(this SerializedProperty property)
        {
            string prePath = property.propertyPath.PrePath(false);

            if (string.IsNullOrEmpty(prePath))
                return new IFindProperties.PropertyRoot(property.serializedObject);
            else
                return new IFindProperties.ChildProp(property.serializedObject.FindProperty(prePath));
        }
        public interface IFindProperties
        {
            public abstract SerializedProperty FindPropertyRelative(string name);
            public abstract Type GetPropertyType();
            public abstract bool IsArray();
            public abstract int GetArraySize();
            public abstract SerializedProperty GetArrayElementAtIndex(int index);

            public class PropertyRoot : IFindProperties
            {
                public readonly SerializedObject obj;
                public PropertyRoot(SerializedObject obj)
                => this.obj = obj;
                public SerializedProperty FindPropertyRelative(string propertyPath)
                => obj.FindProperty(propertyPath: propertyPath);
                public Type GetPropertyType()
                => obj.targetObject.GetType();
                public int GetArraySize()
                => throw new ArgumentException("PropertyRoot cannot be an array");
                public SerializedProperty GetArrayElementAtIndex(int index)
                => throw new ArgumentException("PropertyRoot cannot be an array");
                public bool IsArray()
                => false;
            }
            public class ChildProp : IFindProperties
            {
                public readonly SerializedProperty property;
                public ChildProp(SerializedProperty property)
                {
                    if (property.propertyType != SerializedPropertyType.Generic)
                        throw new NotSupportedException($"{property.propertyType} not supported");
                    this.property = property;
                }
                public SerializedProperty FindPropertyRelative(string relativePropertyPath)
                    => property.FindPropertyRelative(relativePropertyPath: relativePropertyPath);
                public Type GetPropertyType()
                    => property.GetFieldType();
                public int GetArraySize()
                    => property.arraySize;
                public SerializedProperty GetArrayElementAtIndex(int index)
                    => property.GetArrayElementAtIndex(index);
                public bool IsArray()
                    => property.isArray;
            }
        }

        public static Type GetFieldType(this SerializedProperty property, BindingFlags bindingAttr = defaultBindingFlags)
        {
            if (property?.serializedObject?.targetObject == null)
                throw new NullReferenceException("property is not valid");

            (_, FieldInfo fieldInfo) = GetFieldInfos(property, bindingAttr);

            if(typeof(IEnumerable).IsAssignableFrom(fieldInfo.FieldType))
            {
                //Check how deep nested in list
                string[] parts = property.propertyPath.Split('.');
                int nested = 0;
                for (int i = parts.Length - 1; i >= 0; i -= 2)
                {
                    // one times nestes in .Array.data[i] in the propertyPath
                    if (parts[i][^1] == ']')
                        nested++;
                    else
                        break;
                }
                Type res = fieldInfo.FieldType;
                for (int i = 0; i < nested; i++)
                {
                    if(fieldInfo.FieldType.IsArray)
                        res = res.GetElementType();
                    else
                        res = res.GetGenericArguments()[0];
                }
                Debug.Assert(res != null, "Unexpected Type value: null");
                if (res == null)
                    Debug.Log("fdb");
                return res;
            }
            else
            {
                return fieldInfo.FieldType;
            }
        }
        /// <summary>
        /// In most cases you want to use PropertyDrawer.fieldInfo instead
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static (object owner, FieldInfo fieldInfo) GetFieldInfos(this SerializedProperty property, BindingFlags bindingAttr = defaultBindingFlags)
        {
            if (property == null)
                throw new NullReferenceException("serializedProperty is null");

            string prePath = property.propertyPath.PrePath(false);

            object owner;
            string name; 
            if(string.IsNullOrEmpty(prePath))
            {
                owner = property.serializedObject.targetObject;
                name = BeforeFirstDot(property.propertyPath);
            }
            else
            {
                owner = property.serializedObject.targetObject.GetDirtyValue(prePath);
                Debug.Assert(owner != null, "owner not found");
                name = property.propertyPath[(prePath.Length + 1)..]; //on arrays its still arrayName.Array.data[i] left
                name = BeforeFirstDot(name);
            }

            FieldInfo fieldInfo = owner.GetType().GetField(name, bindingAttr);
            if(fieldInfo == null)
                fieldInfo = GetFieldFromHierarchy(owner.GetType(), name);


            return (owner, fieldInfo);

            static string BeforeFirstDot(string s)
            {
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] == '.')
                        return s[..i];
                }
                return s;
            }
        }
        static FieldInfo GetFieldFromHierarchy(Type type, string name, BindingFlags bindingFlags = defaultBindingFlags)
        {
            FieldInfo fieldInfo;
            Type upperType = type.BaseType;
            while (upperType != null)
            {
                fieldInfo = upperType.GetField(name, bindingFlags);
                if (fieldInfo != null)
                    return fieldInfo;

                upperType = upperType.BaseType;
            }
            throw new ArgumentException($"Field '{name}' not found in '{type}' or its hierarchy");
        }
        public static bool IsArrayElement(this SerializedProperty property, out int index)
        {
            if (property.propertyPath[^1] != ']')
            {
                //property is not an array element
                index = -1;
                return false;
            }

            index = int.Parse(property.propertyPath[(property.propertyPath.LastIndexOf('[') + 1)..^1]);
            return true;
        }
        public static bool IsArrayElement(this SerializedProperty property)
        => property.IsArrayElement(out var _);

        /// <summary>
        /// Get the loacl scope classes SerializedProperty where the property is inside
        /// </summary>
        /// <param name="serializedProperty"></param>
        /// <returns></returns>
        public static (SerializedProperty list, int elementIndex) GetListFromElement(SerializedProperty element)
        {
            Debug.Assert(element.IsArrayElement(), "property is not an element of a list");

            int pathElementIndexStart = element.propertyPath.LastIndexOf('[') + 1;
            int arrayIndex = int.Parse(element.propertyPath[pathElementIndexStart..^1]);

            //Get owner path
            return (element.GetOwnerAsFinder().FindPropertyRelative(element.GetFieldInfos().fieldInfo.Name), arrayIndex);
        }

        public static IEnumerable<SerializedProperty> GetAllPropertys(this IFindProperties finder)
        {
            if (finder is null)
                throw new NullReferenceException("property is null");

            if (finder.IsArray())
            {
                return Enumerable.Range(0, finder.GetArraySize()).Select(_ => finder.GetArrayElementAtIndex(_));
            }
            else
            {
                Type type = finder.GetPropertyType(); //returned field can be owner array of current element (because element has no own field)

                FieldInfo[] fields = type.GetFields(defaultBindingFlags);
                return fields.Where(_ => !Attribute.IsDefined(_, typeof(NonSerializedAttribute))).Select(_ => finder.FindPropertyRelative(_.Name)).Where(_ => _ != null);
            }
        }
        /// <summary>
        /// Returns all propertys in given object
        /// </summary>
        public static IEnumerable<SerializedProperty> GetAllPropertys(this SerializedObject property)
        {
            FieldInfo[] fields = property.targetObject.GetType().GetFields(defaultBindingFlags);
            return fields.Where(_ => !Attribute.IsDefined(_, typeof(NonSerializedAttribute))).Select(_ => property.FindProperty(_.Name)).Where(_ => _ != null);
        }
        /// <summary>
        /// Returns all propertys in given property
        /// </summary>
        public static IEnumerable<SerializedProperty> GetAllPropertys(this SerializedProperty property)
        => GetAllPropertys(new IFindProperties.ChildProp(property));
        /// <summary>
        /// Returns all propertys in given property that dont have HideInInspector defined
        /// </summary>
        /// <exception cref="NotSupportedException">Only on generic props</exception>
        public static IEnumerable<SerializedProperty> GetAllVisiblePropertys(this SerializedProperty property)
        => property.GetAllPropertys().Where(_ => !Attribute.IsDefined(_.GetFieldInfos().fieldInfo, typeof(HideInInspector)));
        //infos
        public static bool ContainsMethod(this SerializedProperty property, string methodPath, out (MethodInfo methodInfo, object owner) method, Type[] parameterTypes = null, BindingFlags bindingAttr = defaultBindingFlags)
        {
            try
            {
                method = GetMethod(property, methodPath, parameterTypes, bindingAttr);
                return true;
            }
            catch
            {
                method = (null, null);
                return false;
            }
        }
        public static bool ContainsMethod(this object obj, string methodPath, out (MethodInfo methodInfo, object owner) method, Type[] parameterTypes = null, BindingFlags bindingAttr = defaultBindingFlags)
        {
            UnityEngine.Debug.Assert(obj is not SerializedProperty, "Your calling a method on the serialization instead of the actual object.");
            try
            {
                method = GetMethod(obj, methodPath, parameterTypes, bindingAttr);
                return true;
            }
            catch
            {
                method = (null, null);
                return false;
            }
        }
        //get method
        /// <summary>
        /// 
        /// </summary>
        /// <param name="object"></param>
        /// <param name="methodPath"></param>
        /// <param name="bindingAttr"></param>
        /// <returns></returns>
        /// <exception cref="MissingFieldException"></exception>
        /// <exception cref="WrongTypeException"></exception>
        /// <exception cref="MissingMethodException"></exception>
        public static (MethodInfo methodInfo, object owner) GetMethod(this object @object, string methodPath, Type[] parameterTypes = null, BindingFlags bindingAttr = defaultBindingFlags)
        {
            if (methodPath is null)
                throw new NullReferenceException(nameof(methodPath));

            int splitIndex = methodPath.LastIndexOf('.');
            //if has prepath
            if (splitIndex != -1)
            {
                @object = @object.GetDirtyValue(methodPath[0..splitIndex]);
                Debug.Assert(splitIndex + 1 < methodPath.Length, "Methodpath must not end on a '.' - Name is missing");
                methodPath = methodPath[(splitIndex + 1)..];
            }

            (MethodInfo methodInfo, object obj) method;
            if (parameterTypes == null)
            {
                method = (@object.GetType().GetMethod(name: methodPath, bindingAttr: bindingAttr, null, CallingConventions.Any, Type.EmptyTypes, null), @object);
                if (method.methodInfo == null)
                    throw new MissingMethodException($"No method without parameters on \"{methodPath}\" found in \"{@object.GetType()}\"");
            }
            else
            {
                method = (@object.GetType().GetMethod(name: methodPath, bindingAttr: bindingAttr, null, CallingConventions.Any, parameterTypes, null), @object);
                if (method.methodInfo == null)
                    throw new MissingMethodException($"No method on \"{methodPath}\" with parameters ({string.Join(", ", parameterTypes.Select(_ => _.Name))}) found in \"{@object.GetType()}\"");
            }
            return method;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <param name="methodPath"></param>
        /// <param name="parameterTypes"></param>
        /// <param name="bindingAttr"></param>
        /// <returns></returns>
        /// <exception cref="MissingFieldException"></exception>
        /// <exception cref="WrongTypeException"></exception>
        /// <exception cref="MissingMethodException"></exception>
        public static (MethodInfo methodInfo, object owner) GetMethod(this SerializedProperty property, string methodPath, Type[] parameterTypes = null, BindingFlags bindingAttr = defaultBindingFlags)
            => property.GetDirtyOwnerValue().GetMethod(methodPath, parameterTypes, bindingAttr);

    }
    public static class EditProperties
    {
        const BindingFlags defaultBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        //Set
        /// <summary>
        /// Sets value of Property ASSUMING value has same type as serializedProperty.propertyType
        /// </summary>
        public static void SetValue(this SerializedProperty property, object value)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer: property.intValue = (int)value; break;
                case SerializedPropertyType.Boolean: property.boolValue = (bool)value; break;
                case SerializedPropertyType.Float: //and double value
                    if(value is float f) property.floatValue = f;
                    else property.doubleValue = (double)value; break;
                case SerializedPropertyType.String: property.stringValue = (string)value; break;
                case SerializedPropertyType.Color: property.colorValue = (Color)value; break;
                case SerializedPropertyType.ExposedReference: property.exposedReferenceValue = (UnityEngine.Object)value; break;
                case SerializedPropertyType.ObjectReference: property.objectReferenceValue = (UnityEngine.Object)value; break;
                case SerializedPropertyType.Enum: property.enumValueIndex = (int)value; break;
                case SerializedPropertyType.Vector2Int: property.vector2IntValue = (Vector2Int)value; break;
                case SerializedPropertyType.Vector2: property.vector2Value = (Vector2)value; break;
                case SerializedPropertyType.Vector3Int: property.vector3IntValue = (Vector3Int)value; break;
                case SerializedPropertyType.Vector3: property.vector3Value = (Vector3)value; break;
                case SerializedPropertyType.Vector4: property.vector4Value = (Vector4)value; break;
                case SerializedPropertyType.RectInt: property.rectIntValue = (RectInt)value; break;
                case SerializedPropertyType.Rect: property.rectValue = (Rect)value; break;
                case SerializedPropertyType.AnimationCurve: property.animationCurveValue = (AnimationCurve)value; break;
                case SerializedPropertyType.BoundsInt: property.boundsIntValue = (BoundsInt)value; break;
                case SerializedPropertyType.Bounds: property.boundsValue = (Bounds)value; break;
                case SerializedPropertyType.Quaternion: property.quaternionValue = (Quaternion)value; break;
                case SerializedPropertyType.Generic: SetGeneric(value); break;
                default: UnityEngine.Debug.LogError($"{property.propertyType} not supported"); break;
            };

            void SetGeneric(object value)
            {
                if (!property.isArray)
                {
                    Type type = property.GetFieldType();

                    foreach (var prop in property.GetAllPropertys())
                    {
                        FieldInfo field = type.GetField(prop.name, defaultBindingFlags);
                        Debug.Assert(field != null, $"Field {prop.name} not found in {type}");
                        prop.SetValue( field.GetValue(value) );
                    }
                }
                else
                {
                    IList newValues;
                    try
                    {
                        newValues = (IList)value;
                    }
                    catch (InvalidCastException)
                    {
                        throw new ArgumentException($"{property.propertyPath} was expecting a list");
                    }
                    //Make same size as new values:
                    property.arraySize = newValues.Count;

                    for (int i = 0; i < property.arraySize; i++)
                    {
                        property.GetArrayElementAtIndex(i).SetValue(newValues[i]);
                    }
                }
            }
        }
        public static void SetDirtyValue(this SerializedProperty property, string propertyPath, object value, BindingFlags bindingAttr = defaultBindingFlags)
        {
            (object owner, FieldInfo fieldInfo) = property.GetFieldInfos(bindingAttr);
            fieldInfo.SetValue(owner, value);
        }
        public static void SetDirtyValue(this object obj, string propertyPath, object value, BindingFlags bindingAttr = defaultBindingFlags)
        {
            Debug.Assert(string.IsNullOrEmpty(propertyPath), "propertyPath cannot be empty");

            string prePath = propertyPath.PrePath(false);
            if(!string.IsNullOrEmpty(prePath))
            {
                obj = obj.GetDirtyValue(prePath);
            }

            var field = obj.GetType().GetField(propertyPath[(prePath.Length+1)..], defaultBindingFlags);
            field.SetValue(obj, value);
        }

        //Call method
        /// <summary>
        /// Call Method inside the serializedProperty
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="methodPath"></param>
        /// <param name="methodPath">If true, you call the method on the actual property inseatead of the current monobehaviour</param>
        /// <param name="bindingAttr"></param>
        /// <returns></returns>
        /// <exception cref="MissingMethodException"></exception>
        /// <exception cref="MissingFieldException"></exception>
        /// <exception cref="WrongTypeException"></exception>
        public static object CallMethod(this SerializedProperty property, string methodPath, object[] parameters = null, BindingFlags bindingAttr = defaultBindingFlags)
        {
            if (property.propertyType == SerializedPropertyType.Generic || property.propertyType == SerializedPropertyType.ObjectReference)
            {
                return CallMethod(property.serializedObject.targetObject, property.propertyPath + "." + methodPath, parameters, bindingAttr);
            }
            else
                throw new NotSupportedException($"Calling methods on {property.propertyType} is not supported");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="methodPath"></param>
        /// <param name="bindingAttr"></param>
        /// <returns></returns>
        /// <exception cref="MissingMethodException"></exception>
        /// <exception cref="MissingFieldException"></exception>
        /// <exception cref="WrongTypeException"></exception>
        public static object CallMethod(this object obj, string methodPath, object[] parameters = null, BindingFlags bindingAttr = defaultBindingFlags)
        {
            UnityEngine.Debug.Assert(obj is not SerializedProperty, "Your calling the serialization instead of the actual object");

            (MethodInfo methodInfo, object targetObject) = PropertyValues.GetMethod(obj, methodPath, parameters?.Select(_ => _.GetType()).ToArray(), bindingAttr);

            if (methodInfo != null)
            {
                return methodInfo.Invoke(targetObject, parameters);
            }
            else
            {
                throw new MissingMethodException($"Method {methodPath} not found in {obj.GetType()}");
            }
        }
    }
    public static class PropertyConversions
    {
        /// <summary>
        /// Adds spaces to name and removes underscores
        /// </summary>
        /// <returns></returns>
        public static string NameFormat(string name)
        {
            if (string.IsNullOrEmpty(name))
                return " ";

            //remove underscores
            for (int start = 0; start < name.Length; start++)
            {
                if (name[start] != '_')
                {
                    name = name[start..];
                    break;
                }
            }
            string res = name[0].ToString();
            //replace uppercase to lowercase and space
            for (int i = 1; i < name.Length; i++)
            {
                if ((int)name[i] < (int)'a') //if uppercase
                    res += " " + (char)(name[i] + ('a' - 'A'));
                else
                    res += name[i];
            }
            return res;
        }
        public static System.Type ToSystemType(this SerializedPropertyType propertyType)
        {
            return propertyType switch
            {
                SerializedPropertyType.Integer          => typeof(int),
                SerializedPropertyType.Boolean          => typeof(bool),
                SerializedPropertyType.Float            => typeof(float),
                SerializedPropertyType.String           => typeof(string),
                SerializedPropertyType.Color            => typeof(Color),
                SerializedPropertyType.Enum             => typeof(Enum),
                SerializedPropertyType.Vector2Int       => typeof(Vector2Int),
                SerializedPropertyType.Vector2          => typeof(Vector2),
                SerializedPropertyType.Vector3Int       => typeof(Vector3Int),
                SerializedPropertyType.Vector3          => typeof(Vector3),
                SerializedPropertyType.Vector4          => typeof(Vector4),
                SerializedPropertyType.RectInt          => typeof(RectInt),
                SerializedPropertyType.Rect             => typeof(Rect),
                SerializedPropertyType.AnimationCurve   => typeof(AnimationCurve),
                SerializedPropertyType.BoundsInt        => typeof(BoundsInt),
                SerializedPropertyType.Bounds           => typeof(Bounds),
                SerializedPropertyType.Quaternion       => typeof(Quaternion),
                SerializedPropertyType.ExposedReference => typeof(UnityEngine.Object),
                SerializedPropertyType.ObjectReference  => typeof(UnityEngine.Object),
                _                                       => null
            };
        }
        public static SerializedPropertyType ToPropertyType(this System.Type type)
        {
            if (type == null)
                throw new NullReferenceException($"Type conversion failed");
            else if (type == typeof(int))         return SerializedPropertyType.Integer;
            else if (type == typeof(bool))        return SerializedPropertyType.Boolean;
            else if (type == typeof(float))       return SerializedPropertyType.Float;
            else if (type == typeof(double))      return SerializedPropertyType.Float;
            else if (type == typeof(string))      return SerializedPropertyType.String;
            else if (type == typeof(Color))       return SerializedPropertyType.Color;
            else if (type == typeof(Enum))        return SerializedPropertyType.Enum;
            else if (type == typeof(Vector2Int))  return SerializedPropertyType.Vector2Int;
            else if (type == typeof(Vector2))     return SerializedPropertyType.Vector2;
            else if (type == typeof(Vector3Int))  return SerializedPropertyType.Vector3Int;
            else if (type == typeof(Vector4))     return SerializedPropertyType.Vector4;
            else if (type == typeof(RectInt))     return SerializedPropertyType.RectInt;
            else if (type == typeof(AnimationCurve)) return SerializedPropertyType.AnimationCurve;
            else if (type == typeof(BoundsInt))   return SerializedPropertyType.BoundsInt;
            else if (type == typeof(Bounds))      return SerializedPropertyType.Bounds;
            else if (type == typeof(Quaternion))  return SerializedPropertyType.Quaternion;
            else if (type == typeof(UnityEngine.Object)) return SerializedPropertyType.ObjectReference;
            else                                    return SerializedPropertyType.Generic;
        }

        /// <summary>
        /// Parses string to type of given property. Reverse of ToString()
        /// </summary>
        public static object ParseString(this SerializedProperty property, string value)
        {
            return property.propertyType switch
            {
                SerializedPropertyType.Integer => int.Parse(value),
                SerializedPropertyType.Boolean => bool.Parse(value),
                SerializedPropertyType.Float => ParseSingleOrDouble(),
                SerializedPropertyType.String => value,
                SerializedPropertyType.Color => UnityParsing.ParseColor(value),
                SerializedPropertyType.Enum => (int)Enum.Parse(property.GetValue().GetType(), value),
                SerializedPropertyType.Vector2Int => UnityParsing.ParseVector2Int(value),
                SerializedPropertyType.Vector2 => UnityParsing.ParseVector2(value),
                SerializedPropertyType.Vector3Int => UnityParsing.ParseVector3Int(value),
                SerializedPropertyType.Vector3 => UnityParsing.ParseVector3(value),
                SerializedPropertyType.Vector4 => UnityParsing.ParseVector4(value),
                SerializedPropertyType.RectInt => UnityParsing.ParseRectInt(value),
                SerializedPropertyType.Rect => UnityParsing.ParseRect(value),
                SerializedPropertyType.BoundsInt => UnityParsing.ParseBoundsInt(value),
                SerializedPropertyType.Bounds => UnityParsing.ParseBounds(value),
                SerializedPropertyType.Quaternion => UnityParsing.ParseQuaternion(value),
                _ => throw new ArgumentException($"Parse for '{property.propertyType}' not supported")

                /*  case SerializedPropertyType.ExposedReference:
                    case SerializedPropertyType.ObjectReference:*/
            };

            object ParseSingleOrDouble()
            {
                if (property.GetFieldType() == typeof(double))
                    return double.Parse(value);
                else
                    return float.Parse(value);
            }
        }
        //get path
        /// <summary>
        /// The path minus the name on end
        /// </summary>
        /// <param name="path"></param>
        /// <param name="includeDot">if the path contains dots, it will end on a dot</param>
        /// <returns></returns>
        public static string PrePath(this string path, bool includeDot)
        {
            // Bei array elementen wollen wir nicht das array sondern den besitzer von dem array (because unitys thing was it that attributes on arrays get applied to the elements)
            // array elemente: MyClassName.myArrayName.Array.data[i]

            if (path[^1] == ']') //its an array element
            {
                int dotsToFind = 2;

                for (int i = path.Length - 1; i >= 0; i--)
                {
                    if (path[i] == '.')
                    {
                        dotsToFind--;
                    }
                    if (dotsToFind <= 0)
                    {
                        path = path[..i];
                        break;
                    }
                }
            }

            for (int i = path.Length - 1; i >= 0; i--)
            {
                if (path[i] == '.')
                {
                    if (includeDot)
                        return path[..(i + 1)];
                    else
                        return path[..i];
                }
            }
            return "";
        }
        /// <summary>
        /// Removes everything until (including) the last dot
        /// </summary>
        /// <param name="path"></param>
        public static string NameOfPath(this string path)
        {
            if (path is null)
                return null;
            int indexlastDot = path.LastIndexOf('.');
            if (indexlastDot == -1)
                return path;
            else if (indexlastDot + 1 >= path.Length)
                throw new ArgumentException($"Path '{path}' doesnt contain a name");
            else
                return path[(indexlastDot + 1)..];
        }


        /// <summary>
        /// Updates the changes made in reflection on specific object
        /// </summary>
        /// <returns>Returns true if any pending changes were applied to the SerializedProperty</returns>
        public static bool ApplyModifiedFields(this SerializedObject serializedObject, bool withUndoOperation)
        {
            bool hadChanges = false;
            foreach (var prop in PropertyValues.GetAllPropertys(serializedObject))
            {
                hadChanges |= ObjectToPropChanges(prop);
            }
            if(withUndoOperation)
                serializedObject.ApplyModifiedProperties();
            else
                serializedObject.ApplyModifiedPropertiesWithoutUndo();

            return hadChanges;
        }
        /// <summary>
        /// Updates the changes made in reflection on specific field
        /// </summary>
        /// <returns>Returns true if any pending changes were applied to the SerializedProperty</returns>
        public static bool ApplyModifiedField(this SerializedProperty property, bool withUndoOperation)
        {
            bool hadChanges = ObjectToPropChanges(property);
            if (withUndoOperation)
                property.serializedObject.ApplyModifiedProperties();
            else
                property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return hadChanges;
        }
        /// <summary>
        /// transforms object changes to prop changes
        /// </summary>
        /// <returns>Returns true if there were a pending change</returns>
        static bool ObjectToPropChanges(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.Generic)
            {
                if(!property.isArray)
                {
                    bool hadChanges = false;
                    foreach (var prop in PropertyValues.GetAllPropertys(property))
                    {
                        hadChanges |= ObjectToPropChanges(prop);
                    }
                    return hadChanges;
                }
                else //prop is array
                {
                    (object owner, FieldInfo fieldInfo) = property.GetFieldInfos();
                    IList dirtyList = (IList)PropertyValues.GetDirtyValue(owner, fieldInfo.Name);

                    if(dirtyList.Count != property.arraySize)
                    {
                        IList cleanList = (IList)property.GetValue();
                        property.SetValue(dirtyList);
                        fieldInfo.SetValue(owner, cleanList);
                        return true;
                    }
                    else
                    {
                        bool hadChanges = true;
                        var props = PropertyValues.GetAllPropertys(property).ToList();
                        for (int i = 0; i < Math.Min(props.Count, dirtyList.Count); i++)
                        {
                            hadChanges |= ObjectToPropChanges(props[i]);
                        }
                        return hadChanges;
                    }
                }
            }
            else //normal field
            {
                (object owner, FieldInfo fieldInfo) = property.GetFieldInfos();

                if(property.IsArrayElement(out int index))
                {
                    //Get values
                    IList dirtyList = (IList)fieldInfo.GetValue(owner);
                    object dirtyValue;//dirty value
                    try
                    {
                        dirtyValue = dirtyList[index];
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        throw new IndexOutOfRangeException("Could not switch single value. Loaded array and propertyArray had different sizes."); //how should i insert at index 9 if length is 2.
                    }
                    object propValue = property.GetValue(); //serialized value

                    //Null checks
                    if (propValue == null)
                    {
                        if (dirtyValue == null)
                            return false;
                    }
                    //Check if they are the same
                    else if (propValue.Equals(dirtyValue))
                        return false;

                    //transform object changes to prop changes
                    dirtyList[index] = propValue;
                    fieldInfo.SetValue(owner, dirtyList);

                    property.SetValue(dirtyValue);

                    //return
                    return true;
                }
                else
                {
                    object dirtyValue = fieldInfo.GetValue(owner);
                    object propValue = property.GetValue();

                    if (propValue == null)
                    {
                        if (dirtyValue == null)
                            return false;
                    }
                    else if (propValue.Equals(dirtyValue))
                        return false;

                    //transform object changes to prop changes
                    fieldInfo.SetValue(owner, propValue);
                    property.SetValue(dirtyValue);

                    return true;
                }

            }
        }
    }
    /// <summary> Sets indent level </summary>
    public class FixedIndentLevel : IDisposable
    {
        int prevIndentLevel;
        public FixedIndentLevel(int indentLevel)
        {
            prevIndentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = indentLevel;
        }

        public void Dispose()
        {
            EditorGUI.indentLevel = prevIndentLevel;
        }
    }
public static class Exceptions
    {
        /// <summary>
        /// If system.Reflections members have unexpected types
        /// </summary>
        public class WrongTypeException : Exception
        {
            public WrongTypeException() : base() { }
            public WrongTypeException(string message) : base(message) { }
            public WrongTypeException(string message, Exception inner) : base(message, inner) { }
        }
        ///<summary>if my custom unity parsing fails</summary>
        public class ParseException : Exception
        {
            public ParseException() : base() { }
            public ParseException(string message) : base(message) { }
            public ParseException(string message, Exception inner) : base(message, inner) { }
        }
    }
}
