using System;
using UnityEngine;

public class TimeScaleManagerHelper : MonoBehaviour
{
    private void Start()
    {
        // Add the timeScale manager to the debug manager
        DebugManager.Instance.AddDebuggedObject(TimeScaleManager.Instance);
    }

    private void OnDestroy()
    {
        // Remove the timeScale manager from the debug manager
        DebugManager.Instance.RemoveDebuggedObject(TimeScaleManager.Instance);
    }

    private void Update()
    {
        // Update the timeScale manager
        TimeScaleManager.Instance.Update();
    }

}