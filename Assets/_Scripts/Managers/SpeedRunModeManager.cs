using System;
using UnityEngine;
using UnityEngine.Events;

public class SpeedRunModeManager : MonoBehaviour
{
    [SerializeField] private FloatVariable speedRunModeBestTime;
    [SerializeField] private FloatVariable speedRunModeCurrentTime;

    [SerializeField] private UnityEvent<float> onNewBestTime;
    [SerializeField] private UnityEvent<float> onNoNewBestTime;

    private bool _isRunning = false;

    private void Update()
    {
        if (!_isRunning)
            return;

        // Increment the current time by the time since the last frame
        speedRunModeCurrentTime.value += Time.deltaTime;
    }

    public void RestartTime()
    {
        // Set the current time's value to 0
        speedRunModeCurrentTime.value = 0;

        // Set the isRunning variable to true
        _isRunning = true;
    }

    public void StopTime()
    {
        // If the clock wasn't running, return
        if (!_isRunning)
            return;
        
        // Set the isRunning variable to false
        _isRunning = false;

        // Check if the current time is less than the best time
        // Set the best time to the current time
        if (speedRunModeCurrentTime.value < speedRunModeBestTime.value || speedRunModeBestTime.value <= 0)
        {
            speedRunModeBestTime.value = speedRunModeCurrentTime.value;

            // Invoke the event
            onNewBestTime.Invoke(speedRunModeBestTime.value);
        }

        // Invoke the event
        else
            onNoNewBestTime.Invoke(speedRunModeBestTime.value);
    }
}