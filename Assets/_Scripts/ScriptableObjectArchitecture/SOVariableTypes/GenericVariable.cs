using System;
using UnityEditor;
using UnityEngine;

public abstract class GenericVariable<T> : ScriptableObject
{
    [Header("AFTER PLAY MODE, THIS RESETS TO ITS ORIGINAL VALUE")] [SerializeField]
    public T value;

    [SerializeField] public T defaultValue;

    private void OnEnable()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
            return;

        // Subscribe to the play mode state changed event
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        // If the game is currently NOT playing, reset the value to the default value
        if (!Application.isPlaying)
            ResetToDefaultValue();
    }

    private void OnPlayModeStateChanged(PlayModeStateChange obj)
    {
        switch (obj)
        {
            case PlayModeStateChange.EnteredPlayMode:
                ResetToDefaultValue();
                break;

            case PlayModeStateChange.ExitingPlayMode:
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                ResetToDefaultValue();
                break;
        }
    }

#endif

    public void ResetToDefaultValue()
    {
        value = defaultValue;
        
        Debug.Log($"Resetting {name} to default value: {defaultValue}");
    }
}