using System;
using UnityEngine;

public class TimeScaleManagerHelper : MonoBehaviour
{
    private bool _addedToDebugManager;

    private void Start()
    {
        // Add the timeScale manager to the debug manager
        DebugManager.Instance.AddDebuggedObject(TimeScaleManager.Instance);

        // Set the added to debug manager flag to true
        _addedToDebugManager = true;
    }

    private void OnDestroy()
    {
        // Remove the timeScale manager from the debug manager
        if (_addedToDebugManager)
            DebugManager.Instance.RemoveDebuggedObject(TimeScaleManager.Instance);
    }

    private void Update()
    {
        // Update the timeScale manager
        TimeScaleManager.Instance.Update();
    }
}