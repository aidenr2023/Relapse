using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CinemachineShake : MonoBehaviour
{
    // Singleton instance of the CinemachineShake class to allow global access
    public static CinemachineShake Instance { get; private set; }

    // Reference to the Cinemachine virtual camera
    private CinemachineVirtualCamera CinemachineVirtualCamera;

    private CinemachineBasicMultiChannelPerlin perlinNoise;

    // Timer to track the remaining shake duration
    private float shakeTimer;

    // Stores the total shake duration
    private float shakeTimerTotal = 1;

    // The initial intensity of the camera shake
    private float startingIntensity;

    // Called when the script instance is being loaded
    private void Awake()
    {
        // Assign this instance to the static Instance property
        Instance = this;

        // Get the CinemachineVirtualCamera component attached to the same GameObject
        CinemachineVirtualCamera = GetComponent<CinemachineVirtualCamera>();

        // Get the perlin noise component
        perlinNoise = CinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    // Method to trigger the camera shake with specified intensity and duration
    public void ShakeCamera(float intensity, float time)
    {
        // Store the initial intensity and set the total shake time
        startingIntensity = intensity;
        shakeTimerTotal = time;

        // Initialize the shake timer to start the countdown
        shakeTimer = time;
    }

    // Called once per frame to update the shake effect
    private void Update()
    {
        // Decrease the shake timer by the time passed since the last frame
        shakeTimer = Mathf.Clamp(shakeTimer - Time.deltaTime, 0, shakeTimerTotal);

        // Gradually decrease the shake intensity from the starting value to 0 over time
        perlinNoise.m_AmplitudeGain = Mathf.Lerp(startingIntensity, 0f, 1 - (shakeTimer / shakeTimerTotal));
    }
}