using CustomInspector.Extensions;
using UnityEditor;
using UnityEngine;


namespace CustomInspector
{
    [CustomPropertyDrawer(typeof(SelfFillAttribute))]
    public class SelfFillAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent _)
        {
            //unity fucks up
            GUIContent label = new(property.name, property.tooltip);
            //position.height = EditorGUI.GetPropertyHeight(property, label, true);

            //start
            label.text += " (auto-filled)";
            string tooltipMessage = "SelfFill: This field will be automatically filled with the first matching component on this gameObject";
            label.tooltip = (string.IsNullOrEmpty(label.tooltip)) ? tooltipMessage : $"{label.tooltip}\n{tooltipMessage}";

            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                DrawProperties.DrawFieldWithMessage(position, label, property, $"SelfFill: type {property.propertyType} not supported", MessageType.Error, disabled: true);
            }
            else
            {
                Component component = property.serializedObject.targetObject as Component;
                if (component == null)
                {
                    if(property.serializedObject.targetObject is ScriptableObject)
                        DrawProperties.DrawFieldWithMessage(position, label, property, $"SelfFillAttribute for ScriptableObjects not supported", MessageType.Error, disabled: true);
                    else
                        DrawProperties.DrawFieldWithMessage(position, label, property, $"SelfFillAttribute for {property.serializedObject.targetObject.GetType()} not supported", MessageType.Error, disabled: true);
                    return;
                }
                GameObject gob = component.gameObject;

                //Check if empty
                if (property.objectReferenceValue == null)
                {
                    if (fieldInfo.FieldType == typeof(GameObject))
                        property.objectReferenceValue = gob;
                    else if (fieldInfo.FieldType == typeof(Component) || fieldInfo.FieldType.IsSubclassOf(typeof(Component)))
                        property.objectReferenceValue = gob.GetComponent(fieldInfo.FieldType);
                    else
                    {
                        DrawProperties.DrawFieldWithMessage(position, label, property, $"SelfFill: type {fieldInfo.FieldType} not supported. Use Component types (no Assets)", MessageType.Error, disabled: true);
                        return;
                    }

                    //Check if not found
                    if (property.objectReferenceValue == null)
                    {
                        DrawProperties.DrawFieldWithMessage(position, label, property, $"SelfFill: no {fieldInfo.FieldType} component on this gameObject", MessageType.Error, disabled: true);
                        return;
                    }
                }
                else //property.objectReferenceValue != null
                {
                    //Check if valid (invalid fills when for example you copy the script to other objects)
                    if(property.objectReferenceValue is GameObject g)
                    {
                        if (!object.ReferenceEquals(g, gob)) //c.gameObject != gob
                        {
                            if (Application.isPlaying)
                                Debug.LogError($"gameObject reference value on {property.name} deleted. SelfFillAttribute only valid for own gameObject. Location: {property.serializedObject.targetObject}");
                            else
                                Debug.LogWarning($"gameObject reference value on {property.name} discarded. SelfFillAttribute only valid for own gameObject. Location: {property.serializedObject.targetObject}");
                            property.objectReferenceValue = null;
                        }
                    }
                    else if(property.objectReferenceValue is Component c)
                    {
                        if (!object.ReferenceEquals(c.gameObject, gob)) //c.gameObject != gob
                        {
                            if (Application.isPlaying)
                                Debug.LogError($"objectReferenceValue on {property.name} deleted. SelfFillAttribute only valid for components on same gameObject as the script, holding them. Location: {property.serializedObject.targetObject}");
                            else
                                Debug.LogWarning($"objectReferenceValue on {property.name} discarded. SelfFillAttribute only valid for components on same gameObject as the script, holding them. Location: {property.serializedObject.targetObject}\"");
                            property.objectReferenceValue = null;
                        }
                    }
                    else //like an asset
                    {
                        if (!Application.isPlaying)
                            Debug.LogWarning($"Value on {property.name} discarded, because selffillattribute only supports components");
                        else
                            Debug.LogError($"Reference on {property.name} deleted, because selffillattribute only supports components. Location: {property.serializedObject.targetObject}");
                        property.objectReferenceValue = null;
                    }
                }
                //Display
                SelfFillAttribute sa = (SelfFillAttribute)attribute;

                if (!sa.hideIfFilled)
                {
                    DrawProperties.DisabledPropertyField(position, label, property);
                }
            }
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference
                || property.objectReferenceValue == null)
            {
                return DrawProperties.errorHeight + DrawProperties.errorSpacing
                        + EditorGUI.GetPropertyHeight(property, label, true) + EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {
                SelfFillAttribute sa = (SelfFillAttribute)attribute;
                if (!sa.hideIfFilled)
                    return EditorGUI.GetPropertyHeight(property, label, true) + EditorGUIUtility.standardVerticalSpacing;
                else
                    return 0;
            }
        }
    }
}