﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BasicPlayerMovement : PlayerMovementScript
{
    #region Serialized Fields

    [SerializeField] private bool canSprintWithoutPower = true;
    [SerializeField] private bool canJumpWithoutPower = true;

    [SerializeField] [Min(1)] private float sprintMultiplier = 1.5f;

    [SerializeField] [Min(0)] private float jumpForce = 10f;

    #endregion

    private Vector2 _movementInput;

    private bool _isSprinting;
    private bool _isJumpThisFrame;

    #region Getters

    public override InputActionMap InputActionMap => InputManager.Instance.PlayerControls.PlayerMovementBasic;

    public Vector2 MovementInput => _movementInput;

    private bool CanSprint => canSprintWithoutPower && ParentComponent.IsGrounded;

    public bool IsSprinting => _isSprinting;

    #endregion

    #region Initialization Functions

    private void Start()
    {
        // Initialize the controls
        InitializeControls();
    }

    private void InitializeControls()
    {
        // Subscribe to the movement input
        InputManager.Instance.PlayerControls.PlayerMovementBasic.Move.performed += OnMovePerformed;
        InputManager.Instance.PlayerControls.PlayerMovementBasic.Move.canceled += OnMoveCanceled;

        InputManager.Instance.PlayerControls.PlayerMovementBasic.Sprint.performed += OnSprintPerformed;
        InputManager.Instance.PlayerControls.PlayerMovementBasic.Sprint.canceled += OnSprintCanceled;

        InputManager.Instance.PlayerControls.PlayerMovementBasic.Jump.performed += OnJumpPerformed;
    }

    #endregion

    #region Input Methods

    private void OnMovePerformed(InputAction.CallbackContext obj)
    {
        // Get the movement input
        _movementInput = obj.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext obj)
    {
        // Reset the movement input
        _movementInput = Vector2.zero;
    }

    private void OnSprintPerformed(InputAction.CallbackContext obj)
    {
        // Return if the player cannot sprint
        if (!CanSprint)
            return;

        // Set the sprinting flag to true
        _isSprinting = true;
    }

    private void OnSprintCanceled(InputAction.CallbackContext obj)
    {
        // Set the sprinting flag to false
        _isSprinting = false;
    }

    private void OnJumpPerformed(InputAction.CallbackContext obj)
    {
        // Return if the player cannot jump
        if (!canJumpWithoutPower)
            return;

        // If the player is not grounded, return
        if (!ParentComponent.IsGrounded)
        {
            _isJumpThisFrame = false;
            return;
        }

        // Set the jump flag to true
        _isJumpThisFrame = true;
    }

    #endregion

    #region Update Functions

    public override void FixedMovementUpdate()
    {
        // Update the movement
        if (ParentComponent.IsGrounded)
            UpdateGroundedLateralMovement();
        else
            UpdateAirborneLateralMovement();

        // Update the jump
        UpdateJump();

        // Adjust the player's rotation
        CameraRotationAdjust();

        // Apply the lateral speed
        // if (true)
        {
            var sprintMod = _isSprinting ? sprintMultiplier : 1;
            ApplyLateralSpeedLimit(ParentComponent.MovementSpeed * sprintMod);
        }
        // else
        //     ApplyLateralSpeedLimit(ParentComponent.MovementSpeed);

        Debug.Log($"SPRINTING: {_isSprinting}!");
    }

    private void UpdateGroundedLateralMovement()
    {
        // Return if the movement input is zero
        if (_movementInput == Vector2.zero)
        {
            // Kill the lateral velocity
            ParentComponent.Rigidbody.velocity = new Vector3(0, ParentComponent.Rigidbody.velocity.y, 0);
            return;
        }

        // Get the camera's transform
        // var cameraTransform = ParentComponent.ParentComponent.PlayerCameraController.CurrentCamera.transform;

        var cameraTransform = ParentComponent.Orientation;

        // Get the camera's forward without the y component
        var cameraForward = new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z).normalized;

        // Get the camera's right without the y component
        var cameraRight = new Vector3(cameraTransform.right.x, 0, cameraTransform.right.z).normalized;

        // Calculate the movement direction relative to the player's transform
        var movementDirection =
            cameraRight * _movementInput.x +
            cameraForward * _movementInput.y;

        var sprintMult = _isSprinting ? sprintMultiplier : 1;

        // Calculate the move vector
        var move = movementDirection * (ParentComponent.MovementSpeed * sprintMult);

        // Set the velocity of the rigid body
        // ParentComponent.Rigidbody.velocity =
        //     new Vector3(move.x, ParentComponent.Rigidbody.velocity.y, move.z);

        ParentComponent.Rigidbody.CustomAddForce(new Vector3(move.x, 0, move.z), ForceMode.VelocityChange);
    }

    private void UpdateAirborneLateralMovement()
    {
        // Return if the movement input is zero
        if (_movementInput == Vector2.zero)
            return;

        // Get the camera's transform
        // var cameraTransform = ParentComponent.ParentComponent.PlayerCameraController.CurrentCamera.transform;
        var cameraTransform = ParentComponent.Orientation;

        // Get the camera's forward without the y component
        var cameraForward = new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z).normalized;

        // Get the camera's right without the y component
        var cameraRight = new Vector3(cameraTransform.right.x, 0, cameraTransform.right.z).normalized;

        // Calculate the movement direction relative to the player's transform
        var movementDirection =
            cameraRight * _movementInput.x +
            cameraForward * _movementInput.y;

        var sprintMult = _isSprinting ? sprintMultiplier : 1;

        // Calculate the move vector
        var move = movementDirection * (ParentComponent.MovementSpeed * sprintMult);

        // Set the velocity of the rigid body
        ParentComponent.Rigidbody.CustomAddForce(
            new Vector3(move.x, 0, move.z), ForceMode.Impulse
        );
    }

    private void UpdateJump()
    {
        // Return if the jump this frame flag is false
        if (!_isJumpThisFrame)
            return;

        // Set the jump this frame flag to false
        _isJumpThisFrame = false;

        // Return if the player is not grounded
        if (!ParentComponent.IsGrounded)
            return;

        // Jump in the direction of the movement input
        Jump(_movementInput);
    }

    private void Jump(Vector2 movementInput)
    {
        // Normalize the movement input
        movementInput = movementInput.normalized;

        // Get the direction of the jump relative to the transform
        var jumpDirection =
            (ParentComponent.Orientation.forward * movementInput.y +
             ParentComponent.Orientation.right * movementInput.x +
             ParentComponent.transform.up).normalized;

        // Add a force to the rigid body
        ParentComponent.Rigidbody.CustomAddForce(jumpDirection * jumpForce, ForceMode.VelocityChange);
    }

    private void CameraRotationAdjust()
    {
    }

    #endregion


    public override string GetDebugText()
    {
        return $"\tInput   : {_movementInput}\n";
    }
}