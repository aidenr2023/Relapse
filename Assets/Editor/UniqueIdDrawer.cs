// https://discussions.unity.com/t/automatically-assigning-gameobjects-a-unique-and-consistent-id-any-ideas/75104/3

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System;
using UnityEditor.SceneManagement;

// Place this file inside Assets/Editor
[CustomPropertyDrawer(typeof(UniqueIdentifierAttribute))]
public class UniqueIdentifierDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
        // Check if the associated game object is a prefab
        // Check if the inspector is prefab mode in context
        var isPrefab = PrefabUtility.IsPartOfPrefabAsset(prop.serializedObject.targetObject) ||
                       PrefabStageUtility.GetCurrentPrefabStage() != null;

        // Generate a unique ID, defaults to an empty string if nothing has been serialized yet
        if (prop.stringValue == "" && !isPrefab)
        {
            var guid = Guid.NewGuid();
            prop.stringValue = guid.ToString();
        }
        else if (isPrefab)
            prop.stringValue = "";

        // Place a label so it can't be edited by accident
        var textFieldPosition = position;
        textFieldPosition.height = 16;
        DrawLabelField(textFieldPosition, prop, label);
    }

    private void DrawLabelField(Rect position, SerializedProperty prop, GUIContent label)
    {
        EditorGUI.LabelField(position, label, new GUIContent(prop.stringValue));
    }
}

#endif