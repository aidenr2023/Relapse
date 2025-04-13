using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PhotoModeController : MonoBehaviour
{
    private const int MAX_PRIORITY = 10000;
    private const int MIN_PRIORITY = -1000;

    public static Option<PhotoModeController> Instance { get; private set; } = Option<PhotoModeController>.None;

    #region Serialized Fields

    [SerializeField] private CinemachineVirtualCamera mainVCam;
    [SerializeField] private CinemachineVirtualCamera photoModeVCam;
    [SerializeField] private Camera handsCamera;
    
    [SerializeField, Min(0)] private float moveSpeed = 8;
    [SerializeField, Min(0)] private float lookSense = 15;

    [SerializeField] private LayerMask layersToHide;

    #endregion

    #region Private Fields

    private PlayerControls _playerControls;

    private bool _isActive;
    private TokenManager<float>.ManagedToken _timeScaleToken;
    private Vector2 _moveInput;
    private Vector2 _lookInput;

    #endregion

    #region Initialization Functions

    private void Awake()
    {
        // Destroy if there is already another instance
        if (Instance.HasValue)
        {
            Destroy(gameObject);
            return;
        }

        // Set the singleton instance
        Instance = Option<PhotoModeController>.Some(this);

        // Create an instance of the player controls
        _playerControls = new PlayerControls();

        // Initialize the controls
        InitializeControls();
    }

    private void InitializeControls()
    {
        _playerControls.Debug.PhotoMode.performed += TogglePhotoModeOnInput;

        // Movement input
        _playerControls.PlayerMovementBasic.Move.performed += OnMove;
        _playerControls.PlayerMovementBasic.Move.canceled += OnMove;

        // Look input
        _playerControls.Player.LookMouse.performed += OnLookMouse;
        _playerControls.Player.LookMouse.canceled += OnLookMouse;
    }

    #region Input Functions

    private void OnLookMouse(InputAction.CallbackContext obj)
    {
        _lookInput = _isActive
            ? obj.ReadValue<Vector2>()
            : Vector2.zero;
    }

    private void OnMove(InputAction.CallbackContext obj)
    {
        // Set the movement input
        _moveInput = _isActive
            ? obj.ReadValue<Vector2>()
            : Vector2.zero;
    }

    private void TogglePhotoModeOnInput(InputAction.CallbackContext obj)
    {
        // If the photo mode is not currently active,
        // Perform some checks first to ensure it's safe to toggle
        if (!_isActive)
        {
            // Return if the game is not in debug mode
            if (!DebugManager.Instance.IsDebugMode)
                return;

            // Return if the game is paused
            if (MenuManager.Instance.IsGamePausedInMenus)
                return;

            // Force debug mode to be turned off for the sake of the photo mode
            DebugManager.Instance.IsDebugMode = false;
        }

        // Toggle the photo mode when the input is received
        TogglePhotoMode();
    }

    #endregion

    private void OnEnable()
    {
        // Enable the player controls
        _playerControls.Enable();
    }

    private void OnDisable()
    {
        // Disable the player controls
        _playerControls.Disable();
    }

    private void OnDestroy()
    {
        if (Instance.HasValue && Instance.Value == this)
            Instance = Option<PhotoModeController>.None;
    }

    #endregion

    #region Update Functions

    private void Update()
    {
        // Update the movement and look inputs
        UpdateLook();
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        // Return if not active
        if (!_isActive)
            return;

        // Calculate the movement direction
        // Get the forward and right vectors of the camera
        var forward = photoModeVCam.transform.forward;
        var right = photoModeVCam.transform.right;

        var forwardMovement = forward * (moveSpeed * _moveInput.y);
        var rightMovement = right * (moveSpeed * _moveInput.x);

        // Calculate the new position
        var newPosition = photoModeVCam.transform.position +
                          ((forwardMovement + rightMovement) * Time.unscaledDeltaTime);
        photoModeVCam.transform.position = newPosition;
    }

    private void UpdateLook()
    {
        // Return if not active
        if (!_isActive)
            return;

        // Calculate the look direction
        var lookDelta = new Vector3(-_lookInput.y, _lookInput.x, 0) * lookSense;

        // Get the euler angles of the camera
        var eulerAngles = photoModeVCam.transform.eulerAngles;

        // Calculate the new rotation
        var newRotation = eulerAngles + lookDelta;
        newRotation.z = 0;
        
        // Set the new rotation
        photoModeVCam.transform.eulerAngles = newRotation;
    }

    #endregion


    private void StartPhotoMode()
    {
        // Return if already active
        if (_isActive)
            return;

        // Set the active state
        _isActive = true;

        // Move the photo mode camera to the main camera position
        photoModeVCam.transform.position = mainVCam.transform.position;
        photoModeVCam.transform.rotation = mainVCam.transform.rotation;

        // Create a new token to manage time scale
        _timeScaleToken = TimeScaleManager.Instance.TimeScaleTokenManager.AddToken(0, -1, true);

        // Disable the player controls
        (Player.Instance.PlayerController as PlayerMovementV2)?.DisablePlayerControls(this);

        // Hide the UI
        GameUIHelper.Instance.AddUIHider(this);
        
        // Set the priority of the camera
        photoModeVCam.Priority = MAX_PRIORITY;
        
        // Remove the layers to hide from the hands camera
        handsCamera.cullingMask &= ~layersToHide;

        Debug.Log($"Started photo mode!", this);
    }

    private void EndPhotoMode()
    {
        // Return if already inactive
        if (!_isActive)
            return;

        // Set the active state
        _isActive = false;

        // Move the photo mode camera to the main camera position
        photoModeVCam.transform.position = mainVCam.transform.position;
        photoModeVCam.transform.rotation = mainVCam.transform.rotation;

        // Remove the time scale token
        TimeScaleManager.Instance.TimeScaleTokenManager.RemoveToken(_timeScaleToken);
        _timeScaleToken = null;

        // Re-enable the player controls
        (Player.Instance.PlayerController as PlayerMovementV2)?.EnablePlayerControls(this);

        // Set the priority of the camera
        photoModeVCam.Priority = MIN_PRIORITY;
        
        // Show the UI
        GameUIHelper.Instance.RemoveUIHider(this);
        
        // Add the layers to hide back to the hands camera
        handsCamera.cullingMask |= layersToHide;

        Debug.Log($"Ended photo mode!", this);
    }

    private void TogglePhotoMode()
    {
        if (_isActive)
            EndPhotoMode();
        else
            StartPhotoMode();
    }
}