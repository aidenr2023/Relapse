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

    [SerializeField, Range(0, 1)] private float wallRunLookLockLerpAmount = 1f;

    [SerializeField, Min(0)] private float aimAssistRange = 50f;

    #endregion

    #region Private Fields

    private Player _player;

    private Vector2 _lookInput;

    private Vector2 _currentSens;

    // Current rotation around the X and Y axis
    private Quaternion _lookRotation;

    private bool _isHorizontalLookLocked;

    private float _leftLookLockAngle;
    private float _rightLookLockAngle;

    #endregion

    public HashSet<InputData> InputActions { get; } = new();

    private void Awake()
    {
        _player = GetComponent<Player>();

        // Initialize the input actions
        InitializeInput();
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

    private void Start()
    {
        // Get the rotation of the player's transform, apply that rotation to the camera pivot & orientation
        var playerRotation = _player.Rigidbody.rotation;
        _player.Rigidbody.rotation = Quaternion.Euler(0, 0, 0);
        ApplyRotation(playerRotation);

        // Connect to the wall running script
        if (_player.PlayerController is PlayerMovementV2 movementV2)
        {
            movementV2.WallRunning.OnWallRunStart += OnWallRunStart;
            movementV2.WallRunning.OnWallRunEnd += OnWallRunEnd;
        }
    }

    private void OnWallRunStart(PlayerWallRunning obj)
    {
        // Set the look lock to true
        _isHorizontalLookLocked = true;
    }

    private void OnWallRunEnd(PlayerWallRunning obj)
    {
        // Set the look lock to false
        _isHorizontalLookLocked = false;
    }

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
        // TODO: Handle calculations for the wall lock rotations so that I can hard clamp the player's rotation
        // WHILE they are moving the mouse around. This way, I don't have to worry about the player's rotation
        // accidentally going out of bounds while they are moving the mouse around (Especially at high sensitivities)

        var desiredLookRotation = LookUpdate();

        desiredLookRotation = WallRunningUpdate(desiredLookRotation);

        // Set the look rotation to the desired look rotation
        ApplyRotation(desiredLookRotation);
    }

    private Quaternion LookUpdate()
    {
        // Calculate the constant sensitivity multiplier that does not
        // depend on x or y sensitivity / input
        var constantSense = sensitivityMultiplier * Time.deltaTime;

        var desiredLookRotation = _lookRotation;

        var subtractAmount = _lookInput.y * _currentSens.y * constantSense;
        var newXRotation = desiredLookRotation.eulerAngles.x;

        // Get the new X rotation within the 0 - 360 range
        if (newXRotation < 0)
            newXRotation = newXRotation % 360 + 360;
        
        // Clamp the new X rotation to the upper and lower limits
        var upperLimit = 270 + upDownAngleLimit;
        var lowerLimit = 90 - upDownAngleLimit;
        
        if (newXRotation >= 180 && newXRotation - subtractAmount < upperLimit)
            newXRotation = upperLimit;
        else if (newXRotation < 180 && newXRotation - subtractAmount > lowerLimit)
            newXRotation = lowerLimit;
        else
            newXRotation -= subtractAmount;

        // Adjust rotation based on mouse input
        desiredLookRotation = Quaternion.Euler(
            newXRotation,
            desiredLookRotation.eulerAngles.y + _lookInput.x * _currentSens.x * constantSense,
            desiredLookRotation.eulerAngles.z
        );

        return desiredLookRotation;
    }

    private Quaternion WallRunningUpdate(Quaternion desiredLookRotation)
    {
        var movementV2 = _player.PlayerController as PlayerMovementV2;

        if (!_isHorizontalLookLocked ||
            movementV2 == null ||
            !movementV2.WallRunning.IsWallSliding ||
            movementV2.WallRunning.IsCurrentlyJumping
           )
            return desiredLookRotation;

        // Get the current contact info from the wall running script
        var contactInfo = movementV2.WallRunning.ContactInfo;

        // Get the forward of the current wall
        var wallForward = movementV2.WallRunning.IsWallRunningLeft
            ? Vector3.Cross(contactInfo.normal, Vector3.up)
            : Vector3.Cross(Vector3.up, contactInfo.normal);

        // Based on the wall forward, create a float that represents what the player's
        // horizontal look angle would be if they were looking in the same direction as the wall forward
        var wallForwardAngle = -Vector3.SignedAngle(wallForward, Vector3.forward, Vector3.up);

        // Debug.Log($"Wall Forward Angle: {wallForwardAngle}");

        // Create a forward vector that has the same rotation as the desired look rotation
        var desiredForward = Quaternion.Euler(0, desiredLookRotation.eulerAngles.y, 0) * Vector3.forward;

        // Get the angle of the player in relation to the wall forward
        var playerAngle = Vector3.SignedAngle(wallForward, desiredForward, Vector3.up);

        var leftAngleTolerance = movementV2.WallRunning.InwardLookLockAngle;
        var rightAngleTolerance = movementV2.WallRunning.OutwardLookLockAngle;

        if (movementV2.WallRunning.IsWallRunningRight)
            (leftAngleTolerance, rightAngleTolerance) = (rightAngleTolerance, leftAngleTolerance);

        leftAngleTolerance = -leftAngleTolerance + wallForwardAngle;
        rightAngleTolerance += wallForwardAngle;

        playerAngle += wallForwardAngle;

        // Debug.Log(
        //     $"Player Angle: {playerAngle:0.00}, {leftAngleTolerance:0.00}, {rightAngleTolerance:0.00} - {wallForwardAngle:0.00}");

        // Clamp the player angle to the left and right look lock angles
        playerAngle = Mathf.Clamp(
            playerAngle,
            leftAngleTolerance,
            rightAngleTolerance
        );

        // Combine the player angle rotation with the desired look rotation
        var acceptedRotation = Quaternion.Euler(
            desiredLookRotation.eulerAngles.x,
            playerAngle,
            desiredLookRotation.eulerAngles.z
        );

        // Debug.Log($"Player Angle: {acceptedRotation.eulerAngles.y:0.00}, {leftAngleTolerance:0.00}, {rightAngleTolerance:0.00}");

        // Lerp the desired look rotation to the accepted rotation
        // desiredLookRotation = Quaternion.Lerp(desiredLookRotation, acceptedRotation, lerpAmount * frameAmount);
        desiredLookRotation =
            Quaternion.Slerp(
                desiredLookRotation, acceptedRotation,
                CustomFunctions.FrameAmount(wallRunLookLockLerpAmount)
            );
        // desiredLookRotation = acceptedRotation;

        return desiredLookRotation;
    }

    public void ApplyRotation(Quaternion rotation)
    {
        // Set the look rotation to the desired look rotation
        _lookRotation = rotation;

        // Rotate the player's orientation first
        // Then rotate the camera pivot (which is a child of the orientation)
        _player.PlayerController.Orientation.transform.localEulerAngles = new(0f, _lookRotation.eulerAngles.y, 0f);
        _player.PlayerController.CameraPivot.transform.localEulerAngles = new(_lookRotation.eulerAngles.x, 0f, 0f);
    }
}