using System;
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

    // Speed of the FOV transitions
    public float fovTransitionSpeed = 5f;
    public float dashFOVDuration = 0.5f; // How long the dash FOV effect lasts

    private bool _isDashing;
    private bool _isInDashFOVTransition;

    private IPlayerController _playerMovement;
    private Dash _dashScript;

    private bool IsSprinting => _playerMovement.IsSprinting;

    #region Initialization

    private void Awake()
    {
        // Initialize the components
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // Get the PlayerMovement script
        _playerMovement = GetComponent<PlayerMovement>();

        // Get the Dash script
        _dashScript = GetComponent<Dash>();

        // Add the OnDash method to the event
        _dashScript.OnDash += OnDash;
    }

    #endregion

    private void OnDash(Dash dash)
    {
        // Return if in the FOV transition
        if (_isInDashFOVTransition)
            return;

        StartCoroutine(HandleDash());
    }

    void Update()
    {
        // Handle Sprinting
        HandleSprint();
    }

    void HandleSprint()
    {
        // Get the current FOV of the camera
        var currentFOV = cinemachineCamera.m_Lens.FieldOfView;

        // Only adjust sprint FOV if not currently dashing
        if (!_isDashing)
        {
            // Determine the target FOV based on whether the player is sprinting
            var targetFOV = IsSprinting ? sprintFOV : normalFOV;

            // Smoothly transition the FOV
            cinemachineCamera.m_Lens.FieldOfView =
                Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime * fovTransitionSpeed);
        }
    }

    // Coroutine to handle the Dash FOV effect
    IEnumerator HandleDash()
    {
        _isDashing = true;
        _isInDashFOVTransition = true;

        // Quickly transition to dash FOV
        var currentFOV = cinemachineCamera.m_Lens.FieldOfView;
        var elapsedTime = 0f;

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
        _isInDashFOVTransition = false;
        _isDashing = false;
    }
}