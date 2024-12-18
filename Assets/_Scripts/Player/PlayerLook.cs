using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerLook : MonoBehaviour, IUsesInput
{
    #region Serialized Fields

    // Mouse sensitivity
    [Header("Settings")] [SerializeField] [Min(1)]
    private float sensitivityMultiplier = 50;

    [SerializeField] [Range(0, 90)] private float upDownAngleLimit = 5;

    #endregion

    #region Private Fields

    private Player _player;

    // Current rotation around the X axis
    private float _xRotation;

    // Current rotation around the Y axis
    private float _yRotation;

    private Vector2 _lookInput;

    private Vector2 _currentSens;

    #endregion

    public HashSet<InputData> InputActions { get; } = new();

    private void Awake()
    {
        _player = GetComponent<Player>();

        // Initialize the input actions
        InitializeInput();
    }

    private void Start()
    {
    }

    public void InitializeInput()
    {
        // Initialize the input
        InputActions.Add(
            new InputData(InputManager.Instance.PControls.Player.LookMouse, InputType.Performed, OnLookMousePerformed)
        );
        InputActions.Add(
            new InputData(InputManager.Instance.PControls.Player.LookMouse, InputType.Canceled, OnLookCanceled)
        );

        InputActions.Add(
            new InputData(InputManager.Instance.PControls.Player.LookController, InputType.Performed,
                OnLookControllerPerformed)
        );
        InputActions.Add(
            new InputData(InputManager.Instance.PControls.Player.LookController, InputType.Canceled, OnLookCanceled)
        );
    }

    #region Input Functions

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

    #endregion

    private void OnEnable()
    {
        // Register the input
        InputManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        // Unregister the input
        InputManager.Instance.Unregister(this);
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


        // Rotate the player's orientation first
        _player.PlayerController.Orientation.rotation = Quaternion.Euler(0f, _yRotation, 0f);

        // Then rotate the camera pivot (which is a child of the orientation)
        _player.PlayerController.CameraPivot.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

    }
}