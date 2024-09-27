using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class DynamicFOVController : MonoBehaviour
{
    // Reference to the Cinemachine Virtual Camera
    public CinemachineVirtualCamera cinemachineCamera;

    // FOV values for sprint and dash
    public float normalFOV = 60f;
    public float sprintFOV = 80f;
    public float dashFOV = 90f;

    // Sprint and Dash controls
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode dashKey = KeyCode.Space;

    // Speed of the FOV transitions
    public float fovTransitionSpeed = 5f;
    public float dashFOVDuration = 0.5f;  // How long the dash FOV effect lasts

    private bool isSprinting;
    private bool isDashing;
    private bool isInDashFOVTransition;

    void Update()
    {
        // Handle Sprinting
        HandleSprint();

        // Handle Dashing
        if (Input.GetKeyDown(dashKey) && !isInDashFOVTransition)
        {
            StartCoroutine(HandleDash());
        }
    }

    void HandleSprint()
    {
        // Check if the player is holding down the sprint key
        isSprinting = Input.GetKey(sprintKey);

        // Get the current FOV of the camera
        float currentFOV = cinemachineCamera.m_Lens.FieldOfView;

        // Only adjust sprint FOV if not currently dashing
        if (!isDashing)
        {
            // Determine the target FOV based on whether the player is sprinting
            float targetFOV = isSprinting ? sprintFOV : normalFOV;

            // Smoothly transition the FOV
            cinemachineCamera.m_Lens.FieldOfView = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime * fovTransitionSpeed);
        }
    }

    // Coroutine to handle the Dash FOV effect
    IEnumerator HandleDash()
    {
        isDashing = true;
        isInDashFOVTransition = true;

        // Quickly transition to dash FOV
        float currentFOV = cinemachineCamera.m_Lens.FieldOfView;
        float elapsedTime = 0f;

        while (elapsedTime < dashFOVDuration)
        {
            cinemachineCamera.m_Lens.FieldOfView = Mathf.Lerp(currentFOV, dashFOV, (elapsedTime / dashFOVDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the FOV reaches the dashFOV value exactly
        cinemachineCamera.m_Lens.FieldOfView = dashFOV;

        // Wait a brief moment at the peak of the dash
        yield return new WaitForSeconds(0.1f);

        // Return smoothly to normal FOV
        elapsedTime = 0f;
        currentFOV = cinemachineCamera.m_Lens.FieldOfView;

        while (elapsedTime < dashFOVDuration)
        {
            cinemachineCamera.m_Lens.FieldOfView = Mathf.Lerp(currentFOV, normalFOV, (elapsedTime / dashFOVDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the FOV returns to normalFOV exactly
        cinemachineCamera.m_Lens.FieldOfView = normalFOV;

        // Dash transition is over
        isInDashFOVTransition = false;
        isDashing = false;
    }
}
