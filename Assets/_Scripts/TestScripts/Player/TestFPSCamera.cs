using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This script SHOULD NOT touch the camera's position.
/// This script SHOULD NOT read input from the player.
/// This script is only responsible for changing the camera's look rotation.
/// </summary>
[RequireComponent(typeof(CinemachineVirtualCamera))]
public class TestFPSCamera : MonoBehaviour
{
    // Reference to the player controller on the player object
    [SerializeField] private TestPlayerController _playerController;

    // Reference to the Cinemachine Virtual Camera
    private CinemachineVirtualCamera _vcam;

    #region Initialization Functions

    private void Awake()
    {
        // Initialize the components
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // Get the Cinemachine Virtual Camera component
        _vcam = GetComponent<CinemachineVirtualCamera>();
    }

    #endregion

    // Update is called once per frame
    void Update()
    {
    }

    private void UpdateRotation()
    {
        
    }
}