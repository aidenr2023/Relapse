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

    // Timer to track the remaining shake duration
    private float shakeTimer;

    // Stores the total shake duration
    private float shakeTimerTotal;

    // The initial intensity of the camera shake
    private float startingIntensity;

    // Called when the script instance is being loaded
    private void Awake() {
        // Assign this instance to the static Instance property
        Instance = this;

        // Get the CinemachineVirtualCamera component attached to the same GameObject
        CinemachineVirtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    // Method to trigger the camera shake with specified intensity and duration
    public void ShakeCamera(float intensity, float time) {
        // Get the noise component (CinemachineBasicMultiChannelPerlin) used for camera shake
        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin =    
            CinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        // Set the amplitude of the noise, which determines the shake intensity
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = intensity;

        // Store the initial intensity and set the total shake time
        startingIntensity = intensity;
        shakeTimerTotal = time;

        // Initialize the shake timer to start the countdown
        shakeTimer = time;
    }

    // Called once per frame to update the shake effect
    private void Update() {
        // Check if the shake effect is still active (i.e., the timer is not yet finished)
        if (shakeTimer > 0) {
            // Decrease the shake timer by the time passed since the last frame
            shakeTimer -= Time.deltaTime;

            // Get the noise component again to adjust the shake intensity
            CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = 
                CinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

            // Gradually decrease the shake intensity from the starting value to 0 over time
            cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 
                Mathf.Lerp(startingIntensity, 0f, shakeTimer / shakeTimerTotal);
        }
    }
}
