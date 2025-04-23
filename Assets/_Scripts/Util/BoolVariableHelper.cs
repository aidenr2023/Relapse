using System;
using UnityEngine;
using UnityEngine.Events;

public class BoolVariableHelper : MonoBehaviour
{
    [SerializeField] private BoolReference variable;

    [SerializeField] private UnityEvent<bool> onTrue;
    [SerializeField] private UnityEvent<bool> onFalse;

    [SerializeField] private bool activateOnAwake = false;
    [SerializeField] private bool activateOnStart = false;

    private void Awake()
    {
        // If activateOnAwake is true, call Activate method
        if (activateOnAwake)
            Activate();
    }

    private void Start()
    {
        // If activateOnStart is true, call Activate method
        if (activateOnStart)
            Activate();
    }

    [ContextMenu("Activate")]
    public void Activate()
    {
        // If there is no variable, log an error and return
        if (variable == null)
        {
            Debug.LogError("BoolVariableHelper: variable is not set.");
            return;
        }

        // If the variable is true, invoke the onTrue event
        if (variable.Value)
            onTrue?.Invoke(variable.Value);

        // If the variable is false, invoke the onFalse event
        if (!variable.Value)
            onFalse?.Invoke(variable.Value);
    }
}