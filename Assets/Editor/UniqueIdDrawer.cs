// https://discussions.unity.com/t/automatically-assigning-gameobjects-a-unique-and-consistent-id-any-ideas/75104/3

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

// Place this file inside Assets/Editor
[CustomPropertyDrawer(typeof(UniqueIdentifierAttribute))]
public class UniqueIdentifierDrawer : PropertyDrawer
{
    private const int BUTTON_HEIGHT = 18;

    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
        // Check if the property is being multi-edited
        var isMultiEditing = prop.serializedObject.isEditingMultipleObjects;

        // Check if the associated game object is a prefab
        // Check if the inspector is prefab mode in context
        var isPrefab = PrefabUtility.IsPartOfPrefabAsset(prop.serializedObject.targetObject) ||
                       PrefabStageUtility.GetCurrentPrefabStage() != null;

        // Generate a unique ID, defaults to an empty string if nothing has been serialized yet
        if (prop.stringValue == "" && !isPrefab && !isMultiEditing)
            GenerateNewId(prop);
        else if (isPrefab)
            prop.stringValue = "";

        // If the property is not being multi-edited, draw the label field
        // Place a label so it can't be edited by accident
        if (!isMultiEditing)
        {
            var textFieldPosition = position;
            textFieldPosition.height = 16;

            DrawLabelField(textFieldPosition, prop, label);
        }

        // If it is being multi-edited
        else if (!isPrefab)
        {
            // EditorGUI.LabelField(textFieldPosition, label, new GUIContent("MULTI-EDITING MODE"));

            var isMultipleValuesTheSame = false;

            var uniqueValues = new HashSet<string>();

            // Check if any of the targets have the same value
            foreach (var target in prop.serializedObject.targetObjects)
            {
                var so = new SerializedObject(target);
                var sp = so.FindProperty(prop.propertyPath);

                // If the value is unique, add it to the hash set
                if (uniqueValues.Add(sp.stringValue))
                    continue;

                // Otherwise, set the flag to true and break
                isMultipleValuesTheSame = true;
                break;
            }

            var warningRect = new Rect(position.x, position.y, position.width, BUTTON_HEIGHT);

            // If any values are the same, tell the user to generate new IDs
            if (isMultipleValuesTheSame)
            {
                EditorGUI.HelpBox(
                    warningRect,
                    "Multiple objects have the same ID. Press Generate new IDs!",
                    MessageType.Error
                );
            }
            else
            {
                EditorGUI.HelpBox(
                    warningRect,
                    "All objects have unique IDs.",
                    MessageType.Info
                );
            }
        }

        // Make a new button that says "Generate New ID"
        var buttonRect = new Rect(position.x, position.y + 18, position.width, BUTTON_HEIGHT);

        // Get the height of the button and add it to the property height
        position.height += BUTTON_HEIGHT;

        if (GUI.Button(buttonRect, "Generate New ID") && !isPrefab)
            GenerateNewId(prop);
    }

    private void DrawLabelField(Rect position, SerializedProperty prop, GUIContent label)
    {
        EditorGUI.LabelField(position, label, new GUIContent(prop.stringValue));
    }

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
        return base.GetPropertyHeight(prop, label) + BUTTON_HEIGHT;
    }

    private void GenerateNewId(SerializedProperty prop)
    {
        // Check if multiple objects are being edited
        var isMultiEditing = prop.serializedObject.isEditingMultipleObjects;

        // For each object being edited, generate a new GUID
        if (isMultiEditing)
        {
            foreach (var target in prop.serializedObject.targetObjects)
            {
                var serializedObject = new SerializedObject(target);
                var serializedProperty = serializedObject.FindProperty(prop.propertyPath);

                GenerateNewId(serializedProperty);
                serializedObject.ApplyModifiedProperties();
            }

            return;
        }

        var guid = Guid.NewGuid();
        prop.stringValue = guid.ToString();
    }
}

#endif