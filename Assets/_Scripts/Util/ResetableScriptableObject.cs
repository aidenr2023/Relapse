using UnityEditor;
using UnityEngine;

public class ResetableScriptableObject : ScriptableObject
{
    protected void OnEnable()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
            return;

        Reset();

        // Subscribe to the play mode state changed event
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
    }

    protected void OnDisable()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
            return;

        Reset();
#endif
    }

#if UNITY_EDITOR

    protected void OnValidate()
    {
        // If the game is currently NOT playing, reset the value to the default value
        if (Application.isPlaying)
            return;

        Reset();
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
                Reset();
                break;
        }
    }

#endif

    public void Reset()
    {
        CustomReset();
        
        Debug.Log($"Resetting {name}");
    }

    protected virtual void CustomReset()
    {
    }
}