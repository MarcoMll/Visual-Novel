using Codice.Client.BaseCommands;
using CustomInspector.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


namespace CustomInspector
{
    [CustomPropertyDrawer(typeof(ForceFillAttribute))]
    public class ForceFillAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ForceFillAttribute ffa = (ForceFillAttribute)attribute;

            //if parsing failed
            object invalid;
            try
            {
                invalid = GetInvalid(property, ffa.notAllowed);
            }
            catch (Exceptions.ParseException e)
            {
                DrawProperties.DrawFieldWithMessage(position, label, property, e.Message, MessageType.Warning);
                return;
            }

            //If we should even test
            if (ffa.onlyTestInPlayMode && !Application.isPlaying)
            {
                DrawProperties.PropertyField(position, label, property, true);
                return;
            }

            //We should test
            if (invalid is not null)
            {
                string errorMessage;
                if (ffa.errorMessage is null)
                {
                    errorMessage = (ffa.notAllowed == null) ?
                        $"Value of \"{invalid}\" on {property.name} is not valid"
                        : $"Value of \"{invalid}\" on {property.name} is not valid.\nForbidden values are: {string.Join(", ", ffa.notAllowed)} & null";
                }
                else
                {
                    errorMessage = $"ForceFill: {ffa.errorMessage}";
                }

                DrawProperties.DrawFieldWithMessage(position, label, property, errorMessage, MessageType.Error);
            }
            else //Property
                DrawProperties.PropertyField(position, label, property, true);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ForceFillAttribute ffa = (ForceFillAttribute)attribute;

            //if parsing failed
            bool isValid;
            try
            {
                isValid = GetInvalid(property, ffa.notAllowed) != null;
            }
            catch (Exceptions.ParseException)
            {
                return DrawProperties.errorSpacing + DrawProperties.errorHeight
                    + EditorGUI.GetPropertyHeight(property, label) + EditorGUIUtility.standardVerticalSpacing;
            }

            //If we should even test
            if(ffa.onlyTestInPlayMode && !Application.isPlaying)
                return EditorGUI.GetPropertyHeight(property, true);

            //from actual test
            if (GetInvalid(property, ffa.notAllowed) == null)
                return EditorGUI.GetPropertyHeight(property, true);
            else
                return DrawProperties.errorSpacing + DrawProperties.errorHeight
                + EditorGUI.GetPropertyHeight(property, label) + EditorGUIUtility.standardVerticalSpacing;
        }

        string GetInvalid(SerializedProperty property, string[] notAllowed)
        {
            //Check for empty string
            if (property.propertyType == SerializedPropertyType.String && string.IsNullOrEmpty(property.stringValue)) //bei strings soll leerer string auch null sein, da unity automatisch null zu leerstrings macht
                return "null";

            //Check for empty values
            object value = property.GetValue();

            if (value is null)
                return "null";

            if (notAllowed != null)
            {
                foreach (string item in notAllowed)
                {
                    object parsed;
                    try
                    {
                        parsed = property.ParseString(item);
                    }
                    catch
                    {
                        throw new Exceptions.ParseException($"ForceFill: Failed to parse '{item}' as '{property.propertyType}'");
                    }

                    if (value.Equals(parsed))
                        return parsed.ToString();
                }
            }
            return null;
        }
    }
}