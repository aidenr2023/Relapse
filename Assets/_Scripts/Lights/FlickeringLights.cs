using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickeringLights : MonoBehaviour
{
    [SerializeField, Min(0)] private float minTime;
    [SerializeField, Min(0)] private float maxTime;
    [SerializeField, Min(0)] private float maxIntensity = 1;
    [SerializeField, Min(0)] private float minIntensity = 0;

    private Light _light;
    private bool _isEnabled;
    private float _timer;

    private void Awake()
    {
        _light = GetComponent<Light>();
    }


    private void Start()
    {
        SetRandomTimerTime();
    }

    // Update is called once per frame
    private void Update()
    {
        LightsFlickering();
    }

    private void LightsFlickering()
    {
        // If the timer is still active,
        // tick the timer and return
        if (_timer > 0)
        {
            _timer -= Time.deltaTime;
            return;
        }

        // If the timer is done,

        // flip the active state
        _isEnabled = !_isEnabled;

        // If the light is enabled,
        // Set the intensity to the max intensity
        // Otherwise, set it to the min intensity
        _light.intensity = _isEnabled ? maxIntensity : minIntensity;

        SetRandomTimerTime();
    }

    private void SetRandomTimerTime()
    {
        // Set a new time for the timer
        _timer = Random.Range(minTime, maxTime);
    }
}