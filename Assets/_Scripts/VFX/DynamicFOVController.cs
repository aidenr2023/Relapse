using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class DynamicFOVController : MonoBehaviour
{
    // Reference to the Cinemachine Virtual Camera
    [SerializeField] private CinemachineVirtualCamera cinemachineCamera;

    // FOV values for sprint and dash
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float sprintFOV = 80f;
    [SerializeField] private float dashFOV = 90f;

    // Speed of the FOV transitions
    [SerializeField] private float fovTransitionSpeed = 5f;
    [SerializeField] private float dashFOVDuration = 0.5f; // How long the dash FOV effect lasts

    private bool _isDashing;
    private bool _isInDashFOVTransition;

    private IPlayerController _playerMovement;
    private IDashScript _dashScript;

    private float _dashStartTime = float.MinValue;
    private float _dashEndTime = float.MinValue;

    private bool IsSprinting => _playerMovement.IsSprinting;

    #region Initialization

    private void Awake()
    {
        // Initialize the components
        InitializeComponents();
    }

    private void Start()
    {
        InitializeEvents();
    }

    private void InitializeEvents()
    {
        // Add the OnDash method to the event
        // _dashScript.OnDashStart += OnDashStart;
        _dashScript.OnDashStart += OnDashStart2;
        _dashScript.OnDashEnd += OnDashEnd;
    }

    private void InitializeComponents()
    {
        // Get the PlayerMovement script
        _playerMovement = GetComponent<IPlayerController>();

        // Get the Dash script
        _dashScript = GetComponent<IDashScript>();
    }

    #endregion

    private void OnDashStart(IDashScript dash)
    {
        // Return if in the FOV transition
        if (_isInDashFOVTransition)
            return;

        StartCoroutine(HandleDash());
    }


    private void OnDashStart2(IDashScript obj)
    {
        // Return if the dash is dash
        // This script does not have the ability to handle the new dash effect
        if (_dashScript is Dash)
        {
            OnDashStart(_dashScript);
            return;
        }

        // Set the isDashing bool to true
        _isDashing = true;

        // Set the dash start time
        _dashStartTime = Time.time;
    }

    private void OnDashEnd(IDashScript obj)
    {
        // Set the isDashing bool to false
        _isDashing = false;

        // Set the dash end time
        _dashEndTime = Time.time;
    }

    private void Update()
    {
        // // Handle Sprinting
        // HandleSprint();

        var setFov = normalFOV;

        var setSprintFov = setFov;
        var setDashFov = setFov;

        if (IsSprinting && sprintFOV > normalFOV)
            setSprintFov = sprintFOV;

        setDashFov = _isDashing
            ? Mathf.Lerp(normalFOV, dashFOV, (Time.time - _dashStartTime) / _dashScript.DashDuration)
            : Mathf.Lerp(dashFOV, normalFOV, (Time.time - _dashEndTime) / dashFOVDuration);

        setFov = Mathf.Max(setSprintFov, setDashFov, normalFOV);

        // Debug.Log($"Set FOV: {setFov} - {normalFOV} - {setSprintFov} - {setDashFov}");

        cinemachineCamera.m_Lens.FieldOfView = setFov;
    }

    private void HandleSprint()
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
    private IEnumerator HandleDash()
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