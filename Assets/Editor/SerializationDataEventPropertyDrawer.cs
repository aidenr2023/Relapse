#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SerializationDataEvent))]
public class SerializationDataEventPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // If the property is null, return
        if (property == null)
            return;

        // Set the property height
        property.serializedObject.Update();

        // Get the data property
        var dataProperty = property.FindPropertyRelative("dataInfo");

        // Get the boolConditionValue property
        var boolConditionalValueProperty = property.FindPropertyRelative("boolConditionalValue");

        // Get the numberConditionValue property
        var numberConditionalValueProperty = property.FindPropertyRelative("numberConditionalValue");

        // Get the stringConditionValue property
        var stringConditionalValueProperty = property.FindPropertyRelative("stringConditionalValue");

        // Get the onFalse property
        var onFalseProperty = property.FindPropertyRelative("onFalse");

        // Get the onTrue property
        var onTrueProperty = property.FindPropertyRelative("onTrue");

        var currentY = position.y;

        // Make this entire section collapsible
        property.isExpanded = EditorGUI.Foldout(
            new Rect(position.x, currentY, position.width,
                EditorGUIUtility.singleLineHeight
            ),
            property.isExpanded, new GUIContent(property.displayName)
        );

        currentY += EditorGUIUtility.singleLineHeight;

        // If the property is not expanded, return
        if (!property.isExpanded)
            return;

        currentY += EditorGUIUtility.singleLineHeight;

        // Get the height of the data field property
        var dataPropertyHeight = EditorGUI.GetPropertyHeight(dataProperty);

        // Draw the data field
        EditorGUI.PropertyField(
            new Rect(position.x, currentY, position.width, dataPropertyHeight),
            dataProperty
        );

        currentY += dataPropertyHeight;

        // If the data property is not null or none, draw the condition value field
        if (dataProperty.objectReferenceValue != null)
        {
            // Create a copy of the data property
            var dataPropertyCopy = new SerializedObject(dataProperty.objectReferenceValue);

            var enumValue = (SerializationDataType)dataPropertyCopy.FindProperty("dataType").enumValueIndex;

            // Depending on the data type, draw the appropriate condition value field
            switch (enumValue)
            {
                case SerializationDataType.Boolean:
                    EditorGUI.PropertyField(
                        new Rect(position.x, currentY, position.width,
                            EditorGUIUtility.singleLineHeight),
                        boolConditionalValueProperty
                    );
                    break;

                case SerializationDataType.Number:
                    EditorGUI.PropertyField(
                        new Rect(position.x, currentY, position.width,
                            EditorGUIUtility.singleLineHeight),
                        numberConditionalValueProperty
                    );
                    break;

                case SerializationDataType.String:
                    EditorGUI.PropertyField(
                        new Rect(position.x, currentY, position.width,
                            EditorGUIUtility.singleLineHeight),
                        stringConditionalValueProperty
                    );
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        currentY += EditorGUIUtility.singleLineHeight;

        // Put a single line space between the condition value field and the onTrue property
        currentY += EditorGUIUtility.singleLineHeight;

        // get the height of the onTrue property
        var onTruePropertyHeight = EditorGUI.GetPropertyHeight(onTrueProperty);

        // Draw the onTrue property
        EditorGUI.PropertyField(
            new Rect(position.x, currentY, position.width,
                onTruePropertyHeight),
            onTrueProperty
        );

        currentY += onTruePropertyHeight;

        // get the height of the onFalse property
        var onFalsePropertyHeight = EditorGUI.GetPropertyHeight(onFalseProperty);

        // Draw the onFalse property
        EditorGUI.PropertyField(
            new Rect(position.x, currentY, position.width,
                onFalsePropertyHeight),
            onFalseProperty
        );

        currentY += onFalsePropertyHeight;

        // Apply the changes
        property.serializedObject.ApplyModifiedProperties();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // If the property is null, return a single line height
        if (property == null)
            return EditorGUIUtility.singleLineHeight;

        // If the property is not expanded, return a single line height
        if (!property.isExpanded)
            return EditorGUIUtility.singleLineHeight;

        var dataProperty = property.FindPropertyRelative("dataInfo");
        var onTrueProperty = property.FindPropertyRelative("onTrue");
        var onFalseProperty = property.FindPropertyRelative("onFalse");

        var dataPropertyHeight = EditorGUI.GetPropertyHeight(dataProperty);
        var onTruePropertyHeight = EditorGUI.GetPropertyHeight(onTrueProperty);
        var onFalsePropertyHeight = EditorGUI.GetPropertyHeight(onFalseProperty);

        return EditorGUIUtility.singleLineHeight * 4 + dataPropertyHeight + onTruePropertyHeight +
               onFalsePropertyHeight;
    }
}

#endif