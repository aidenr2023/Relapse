using System;
using UnityEngine;

public class SpeedLinesController : MonoBehaviour
{
    [SerializeField] private Vector3Reference playerVelocity;
    
    [SerializeField] private ParticleSystem speedLines;

    [SerializeField, Min(0)] private float multiplier = 10;
    [SerializeField, Min(0)] private float minSpeedThreshold = 16;

    [SerializeField, Min(0.00001f)] private float fillSpeed = 5;
    [SerializeField, Min(0.00001f)] private float depleteSpeed = .5f;

    [SerializeField, Min(1)] private float exceedMultiplier = 1f;
    
    private float _currentValue;

    private void Update()
    {
        var magnitude = playerVelocity.Value.magnitude;
        
        // If the player's velocity is greater than the minimum speed threshold
        // Increase the current value by the fill speed
        if (magnitude >= minSpeedThreshold)
        {
            var exceed = magnitude / minSpeedThreshold * exceedMultiplier;
            
            _currentValue += (1 / fillSpeed) * Time.deltaTime * exceed;
        }
        
        // Decrease the current value by the deplete speed
        else
            _currentValue -= (1 / depleteSpeed) * Time.deltaTime;
        
        // Clamp the current value between 0 and 1
        _currentValue = Mathf.Clamp01(_currentValue);

        // Calculate the desired value for the speed lines
        var desiredValue = _currentValue * multiplier;

        // Get the emission of the speed lines
        // Set the rate over time of the emission to the player's velocity
        var emission = speedLines.emission;
        emission.rateOverTime = desiredValue;
    }
}