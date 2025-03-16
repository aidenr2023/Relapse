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
        
        ResetToDefaultValue();

        // Subscribe to the play mode state changed event
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
            return;

        ResetToDefaultValue();
#endif
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        // If the game is currently NOT playing, reset the value to the default value
        if (Application.isPlaying)
            return;

        ResetToDefaultValue();
    }

    private void OnPlayModeStateChanged(PlayModeStateChange obj)
    {
        switch (obj)
        {
            case PlayModeStateChange.EnteredPlayMode:
                // ResetToDefaultValue();
                break;

            case PlayModeStateChange.ExitingPlayMode:
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                ResetToDefaultValue();
                break;
        }
    }

#endif

    private void ResetToDefaultValue()
    {
        value = defaultValue;
    }
}