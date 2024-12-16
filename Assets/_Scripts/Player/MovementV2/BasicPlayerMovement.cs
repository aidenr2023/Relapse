using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BasicPlayerMovement : PlayerMovementScript, IUsesInput
{
    #region Serialized Fields

    [SerializeField] private bool canSprintWithoutPower = true;
    [SerializeField] private bool canJumpWithoutPower = true;

    [SerializeField] [Min(1)] private float sprintMultiplier = 1.5f;

    [SerializeField] [Min(0)] private float jumpForce = 10f;

    [SerializeField] [Range(0, 1)] private float airMovementMultiplier = 0.5f;

    [Header("Sounds")] [SerializeField] private SoundPool footstepSoundPool;

    [SerializeField] private float walkingFootstepInterval = 0.35f;
    [SerializeField] private float sprintingFootstepInterval = 0.25f;

    [SerializeField] private Sound jumpSound;

    #endregion

    private Vector2 _movementInput;

    private bool _isJumpThisFrame;

    private CountdownTimer _footstepTimer = new(0.5f, true, false);

    #region Getters

    public HashSet<InputData> InputActions { get; } = new();

    public override InputActionMap InputActionMap => InputManager.Instance.PControls.PlayerMovementBasic;

    public Vector2 MovementInput => _movementInput;

    // public bool CanSprint => canSprintWithoutPower && ParentComponent.IsGrounded;
    public bool CanSprint => canSprintWithoutPower;

    public bool IsSprinting => ParentComponent.IsSprinting;

    public float SprintMultiplier => sprintMultiplier;

    #endregion

    #region Initialization Functions

    protected override void CustomAwake()
    {
        // Initialize the controls
        InitializeInput();
    }

    private void Start()
    {
        // Initialize the footstep sounds
        InitializeFootsteps();
    }

    private void OnEnable()
    {
        // Register this script with the input manager
        InputManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        // Unregister this script with the input manager
        InputManager.Instance.Unregister(this);
    }

    public void InitializeInput()
    {
        InputActions.Add(new InputData(
            InputManager.Instance.PControls.PlayerMovementBasic.Move, InputType.Performed, OnMovePerformed)
        );
        InputActions.Add(new InputData(
            InputManager.Instance.PControls.PlayerMovementBasic.Move, InputType.Canceled, OnMoveCanceled)
        );

        InputActions.Add(new InputData(
            InputManager.Instance.PControls.PlayerMovementBasic.Jump, InputType.Performed, OnJumpPerformed)
        );
    }

    private void InitializeFootsteps()
    {
        // Set up the footstep timer
        _footstepTimer.OnTimerEnd += PlayFootstepSound;
    }

    private void PlayFootstepSound()
    {
        // Play the footstep sound
        SoundManager.Instance.PlaySfx(footstepSoundPool.GetRandomSound());

        // Reset the timer
        _footstepTimer.Reset();
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

    private void Update()
    {
        // Update the footstep sounds
        UpdateFootsteps();
    }

    private void UpdateFootsteps()
    {
        // Update the footstep timer
        _footstepTimer.Update(Time.deltaTime);

        // Set the footstep timer's max time based on the player's walking/sprinting state
        _footstepTimer.SetMaxTime(!IsSprinting ? walkingFootstepInterval : sprintingFootstepInterval);

        // If this is NOT the active movement script, disable the footstep timer
        if (ParentComponent.CurrentMovementScript != this)
            _footstepTimer.SetActive(false);
    }

    public override void FixedMovementUpdate()
    {
        // Update the movement
        UpdateLateralMovement();

        // Update the jump
        UpdateJump();
    }

    private void UpdateLateralMovement()
    {
        // Return if the movement input is zero
        if (_movementInput == Vector2.zero)
        {
            // Stop the footstep timer to prevent footstep sounds
            _footstepTimer.SetActive(false);
        }

        var cameraTransform = ParentComponent.Orientation;

        // Get the camera's forward without the y component
        var cameraForward = new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z).normalized;

        // Get the camera's right without the y component
        var cameraRight = new Vector3(cameraTransform.right.x, 0, cameraTransform.right.z).normalized;

        // Get the current move multiplier
        var currentMoveMult = IsSprinting ? sprintMultiplier : 1;

        // Calculate the movement vector
        var forwardMovement = (cameraForward * _movementInput.y);
        var rightMovement = (cameraRight * _movementInput.x);

        if (ParentComponent.IsGrounded)
        {
            // Get the normal of the current surface
            var surfaceNormal = ParentComponent.GroundHit.normal;

            // Get the forward and right vectors based on the surface normal
            forwardMovement = Vector3.ProjectOnPlane(forwardMovement, surfaceNormal);
            rightMovement = Vector3.ProjectOnPlane(rightMovement, surfaceNormal);
        }

        // Calculate the current movement
        var currentMovement = forwardMovement + rightMovement;

        // Normalize the current movement if the magnitude is greater than 1
        if (currentMovement.magnitude > 1)
            currentMovement.Normalize();

        _tmpCurrentMovement = currentMovement.normalized;

        // Calculate how fast the player should be moving
        var currentTargetVelocityMagnitude = ParentComponent.MovementSpeed * currentMoveMult;

        // Calculate the velocity the rigidbody should be moving at
        var targetVelocity = (currentMovement * currentTargetVelocityMagnitude) +
                             new Vector3(0, ParentComponent.Rigidbody.velocity.y, 0);

        // Get the dot product between the target velocity and the current velocity
        var directionChangeDot = Vector3.Dot(ParentComponent.Rigidbody.velocity.normalized, targetVelocity.normalized);

        // Evaluate the direction change dot to get the acceleration factor
        var evaluation = ParentComponent.AccelerationFactorFromDot.Evaluate(directionChangeDot);

        // This is the force required to reach the target velocity in EXACTLY one frame
        var targetForce = (targetVelocity - ParentComponent.Rigidbody.velocity) / Time.fixedDeltaTime;

        // Calculate the force that is going to be applied to the rigidbody THIS frame
        var force = targetForce.normalized * (ParentComponent.Acceleration * evaluation);

        // If the player is midair, apply the air movement multiplier
        if (!ParentComponent.IsGrounded)
            force *= airMovementMultiplier;

        // If the player only needs to move a little bit, just set the force to the target force
        if (targetForce.magnitude < ParentComponent.Acceleration)
            force = targetForce;

        // If the force is greater than the target force, clamp it
        if (force.magnitude > targetForce.magnitude)
            force = targetForce;

        // Apply the force to the rigidbody
        ParentComponent.Rigidbody.AddForce(force, ForceMode.Acceleration);

        // Ensure the player stays on the ground when moving downward along a slope
        // if (currentMovement.y < 0)
        //     force += Vector3.down * (20f * ParentComponent.Rigidbody.velocity.magnitude);
        if (force.y < 0)
        {
            // Debug.Log($"FORCE: {force.normalized} - MOVEMENT {currentMovement.normalized}");
            // force += new Vector3(0, currentMovement.normalized.y, 0) * (currentTargetVelocityMagnitude * 16);
        }

        // Set the footstep timer to active
        _footstepTimer.SetActive(true);
    }

    private Vector3 _tmpCurrentMovement;

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

        var jumpDirection = ParentComponent.transform.up.normalized;

        ParentComponent.Rigidbody.AddForce(jumpDirection * jumpForce, ForceMode.VelocityChange);

        // Play the jump sound
        SoundManager.Instance.PlaySfx(jumpSound);
    }

    #endregion

    public void SetCanSprintWithoutPower(bool canSprint)
    {
        canSprintWithoutPower = canSprint;
    }

    public void SetCanJumpWithoutPower(bool canJump)
    {
        canJumpWithoutPower = canJump;
    }

    public override string GetDebugText()
    {
        return $"\tInput   : {_movementInput}\n";
    }

    #region Debug

    private void OnDrawGizmos()
    {
        const float interval = 0.125f;

        if (ParentComponent == null)
            return;

        var surfaceNormal = ParentComponent.GroundHit.normal;

        var surfaceForward = Vector3.ProjectOnPlane(ParentComponent.Orientation.forward, surfaceNormal).normalized;

        Gizmos.color = Color.red;
        for (var i = 0; i < 20 / interval; i++)
            Gizmos.DrawSphere(ParentComponent.GroundHit.point + surfaceForward * (i * interval), 0.125f / 8);
    }

    #endregion
}