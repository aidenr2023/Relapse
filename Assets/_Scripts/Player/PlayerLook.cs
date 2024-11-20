using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerLook : MonoBehaviour
{
    #region Serialized Fields

    // Mouse sensitivity
    [Header("Settings")] [SerializeField] [Min(1)]
    private float sensitivityMultiplier = 50;

    [SerializeField] [Range(0.01f, 1)] private float sensX = 1;
    [SerializeField] [Range(0.01f, 1)] private float sensY = 1;

    [SerializeField] [Range(0.01f, 1)] private float controllerSensX = 1;
    [SerializeField] [Range(0.01f, 1)] private float controllerSensY = 1;

    [SerializeField] [Range(0, 90)] private float upDownAngleLimit = 5;

    [Header("References")]

    // Reference to the player's orientation transform
    [SerializeField]
    private Transform orientation;

    #endregion

    #region Private Fields

    // Current rotation around the X axis
    private float _xRotation;

    // Current rotation around the Y axis
    private float _yRotation;

    private Vector2 _lookInput;

    private Vector2 _currentSens;

    #endregion

    private void Start()
    {
        // Initialize the input
        InitializeInput();
    }

    private void InitializeInput()
    {
        // Initialize the input
        InputManager.Instance.PlayerControls.Player.LookMouse.performed += OnLookMousePerformed;
        InputManager.Instance.PlayerControls.Player.LookMouse.canceled += OnLookCanceled;

        InputManager.Instance.PlayerControls.Player.LookController.performed += OnLookControllerPerformed;
        InputManager.Instance.PlayerControls.Player.LookController.canceled += OnLookCanceled;
    }

    private void OnLookMousePerformed(InputAction.CallbackContext obj)
    {
        // Set the current sensitivity to the mouse sensitivity
        _currentSens = UserSettings.Instance.MouseSens;

        // Call the look performed function
        OnLookPerformed(obj);
    }

    private void OnLookControllerPerformed(InputAction.CallbackContext obj)
    {
        // Set the current sensitivity to the controller sensitivity
        _currentSens = UserSettings.Instance.ControllerSens;

        // Call the look performed function
        OnLookPerformed(obj);
    }

    private void OnLookPerformed(InputAction.CallbackContext obj)
    {
        // Get the look input
        _lookInput = obj.ReadValue<Vector2>();
    }

    private void OnLookCanceled(InputAction.CallbackContext obj)
    {
        // Reset the look input
        _lookInput = Vector2.zero;
    }

    private void Update()
    {
        // Run the look update
        LookUpdate();
    }

    private void LookUpdate()
    {
        // Calculate the constant sensitivity multiplier that does not
        // depend on x or y sensitivity / input
        var constantSense = sensitivityMultiplier * Time.deltaTime;

        // Adjust rotation based on mouse input
        _yRotation += _lookInput.x * _currentSens.x * constantSense;
        _xRotation -= _lookInput.y * _currentSens.y * constantSense;

        // Clamp the X rotation to prevent over-rotation
        _xRotation = Mathf.Clamp(_xRotation, -90f + upDownAngleLimit, 90f - upDownAngleLimit);

        // Rotate the camera
        orientation.transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0f);
    }

    private void OnDestroy()
    {
        // Unsubscribe from the input events
        InputManager.Instance.PlayerControls.Player.LookMouse.performed -= OnLookMousePerformed;
        InputManager.Instance.PlayerControls.Player.LookMouse.canceled -= OnLookCanceled;

        InputManager.Instance.PlayerControls.Player.LookController.performed -= OnLookControllerPerformed;
        InputManager.Instance.PlayerControls.Player.LookController.canceled -= OnLookCanceled;
    }
}