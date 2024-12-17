using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


public class PlayerWallRunning : PlayerMovementScript, IDebugged, IUsesInput
{
    #region Serialized Fields

    [SerializeField] private bool isEnabled = true;

    [SerializeField] private LayerMask wallLayer;

    [SerializeField] [Range(0, 90)] private float wallAngleTolerance = 45f;

    [SerializeField] [Min(0)] private float maxFallSpeed;
    [SerializeField] [Min(0)] private float fallAcceleration;

    [SerializeField] [Min(0)] private float wallRunSpeedThreshold = 1f;

    [SerializeField] [Min(0)] private float wallRunningDetectionDistance = 0.75f;

    [Header("Wall Jump")] [SerializeField] [Min(0)]
    private float wallJumpForce = 10f;

    [SerializeField] [Range(0, 90)] private float wallJumpAngle = 20f;

    [SerializeField] [Min(0)] private float autoWallJumpForce = .5f;

    [Header("Sounds")] [SerializeField] private SoundPool footstepSoundPool;

    [SerializeField] private float walkingFootstepInterval = 0.35f;
    [SerializeField] private float sprintingFootstepInterval = 0.25f;

    [SerializeField] private Sound jumpSound;

    #endregion

    #region Private Fields

    private bool _isWallRunning;
    private bool _isWallRunningLeft;
    private bool _isWallRunningRight;

    private bool _isJumpThisFrame;

    private bool _isCurrentlyJumping;
    private GameObject _jumpObject;

    private readonly CountdownTimer _footstepTimer = new(0.5f, true, false);

    private readonly CountdownTimer _reattachTimer = new(0.125f, true, false);

    // Code for the new wall running system
    private GameObject _currentWall;
    private RaycastHit _contactInfo;

    #endregion

    public event Action<PlayerWallRunning> OnWallRunStart;
    public event Action<PlayerWallRunning> OnWallRunEnd;

    #region Getters

    public HashSet<InputData> InputActions { get; } = new();

    public override InputActionMap InputActionMap => InputManager.Instance.PControls.PlayerMovementWallRunning;

    public bool IsWallRunning => _isWallRunning;

    public bool IsWallRunningLeft => _isWallRunningLeft;

    public bool IsWallRunningRight => _isWallRunningRight;

    private float ForwardInput => ParentComponent.MovementInput.y;

    #endregion

    #region Initialization Functions

    protected override void CustomAwake()
    {
        // // Initialize the wall running objects
        // _wallRunningObjects = new HashSet<GameObject>();

        // Initialize the input
        InitializeInput();
    }

    private void Start()
    {
        // Initialize the events
        InitializeEvents();

        // Initialize the footstep sounds
        InitializeFootsteps();
    }

    private void RegisterOnWallRunStart(PlayerMovementScript obj)
    {
        InputManager.Instance.Register(this);
    }

    private void UnregisterOnWallRunEnd(PlayerWallRunning obj)
    {
        InputManager.Instance.Unregister(this);
    }

    private void InitializeEvents()
    {
        // Add the on wall run start event
        OnWallRunStart += RegisterOnWallRunStart;
        OnWallRunStart += PushControls;
        OnWallRunStart += GravityOnWallRunStart;
        OnWallRunStart += TransferVelocityOnWallRunStart;

        // Add the on wall run end event
        OnWallRunEnd += AutoWallJump;
        OnWallRunEnd += RemoveControls;
        OnWallRunEnd += GravityOnWallRunEnd;
        OnWallRunEnd += UnregisterOnWallRunEnd;
        OnWallRunEnd += RestartReattachTimer;
    }

    private void RestartReattachTimer(PlayerWallRunning obj)
    {
        _reattachTimer.Reset();
        _reattachTimer.SetActive(true);
    }

    public void InitializeInput()
    {
        // Add the input action to the input actions hashset
        // Jumping
        InputActions.Add(
            new InputData(InputManager.Instance.PControls.PlayerMovementWallRunning.Jump, InputType.Performed,
                OnJumpPerformed)
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

    #region Input Functions

    private void OnJumpPerformed(InputAction.CallbackContext obj)
    {
        // Set the is jump this frame flag to true
        _isJumpThisFrame = true;
    }

    #endregion

    private void Update()
    {
        // Update the footstep sounds
        UpdateFootsteps();

        _reattachTimer.Update(Time.deltaTime);
    }

    private void UpdateFootsteps()
    {
        // Update the footstep timer
        _footstepTimer.Update(Time.deltaTime);

        // Set the footstep timer's max time based on the player's walking/sprinting state
        _footstepTimer.SetMaxTime(walkingFootstepInterval);

        // If this is NOT the active movement script, disable the footstep timer
        if (ParentComponent.CurrentMovementScript != this)
            _footstepTimer.SetActive(false);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        // Detect if the player is currently wall running
        UpdateDetectWallRunning();
    }

    public override void FixedMovementUpdate()
    {
        // Update the movement if the player is wall running
        UpdateMovement();
    }

    private void UpdateDetectWallRunning()
    {
        // Store the previous wall
        var previousWall = _currentWall;

        // Get the hit information for the left and right sides
        var leftHit = Physics.Raycast(
            transform.position, -ParentComponent.CameraPivot.transform.right,
            out var leftHitInfo, wallRunningDetectionDistance,
            wallLayer
        );

        // Get the angle between the player's left ray and the wall's normal
        var leftHitAngle = Vector3.Angle(-ParentComponent.CameraPivot.transform.right, -leftHitInfo.normal);

        // Check if the angle is within the tolerance
        var leftAngleWithinTolerance = leftHitAngle < wallAngleTolerance;

        // Get forward vector
        var leftForwardVector = Vector3.Cross(leftHitInfo.normal, Vector3.up).normalized;

        // Get the dot product of the player's velocity and the forward vector
        // This will be the player's speed as they start wall running
        var leftWallVelocity = Vector3.Dot(ParentComponent.Rigidbody.velocity, leftForwardVector);

        var leftVelocityAboveThreshold = leftWallVelocity >= wallRunSpeedThreshold;

        var rightHit = Physics.Raycast(
            transform.position, ParentComponent.CameraPivot.transform.right,
            out var rightHitInfo, wallRunningDetectionDistance,
            wallLayer
        );

        // Get the angle between the player's right ray and the wall's normal
        var rightHitAngle = Vector3.Angle(ParentComponent.CameraPivot.transform.right, -rightHitInfo.normal);

        var rightAngleWithinTolerance = rightHitAngle < wallAngleTolerance;

        // get the forward vector
        var rightForwardVector = Vector3.Cross(Vector3.up, rightHitInfo.normal).normalized;

        // Get the dot product of the player's velocity and the forward vector
        // This will be the player's speed as they start wall running
        var rightWallVelocity = Vector3.Dot(ParentComponent.Rigidbody.velocity, rightForwardVector);

        var rightVelocityAboveThreshold = rightWallVelocity >= wallRunSpeedThreshold;

        // Get the current hit info
        RaycastHit currentHitInfo;

        // Reset both wall running directions
        _isWallRunningLeft = false;
        _isWallRunningRight = false;

        if (leftHit && rightHit && !ParentComponent.IsGrounded && leftAngleWithinTolerance &&
            rightAngleWithinTolerance && leftVelocityAboveThreshold && rightVelocityAboveThreshold)
        {
            currentHitInfo =
                leftHitInfo.distance < rightHitInfo.distance
                    ? leftHitInfo
                    : rightHitInfo;

            _isWallRunningLeft = leftHitInfo.distance < rightHitInfo.distance;
            _isWallRunningRight = !_isWallRunningLeft;
        }
        else if (leftHit && !ParentComponent.IsGrounded && leftAngleWithinTolerance && leftVelocityAboveThreshold)
        {
            currentHitInfo = leftHitInfo;

            _isWallRunningLeft = true;
        }
        else if (rightHit && !ParentComponent.IsGrounded && rightAngleWithinTolerance && rightVelocityAboveThreshold)
        {
            currentHitInfo = rightHitInfo;

            _isWallRunningRight = true;
        }

        else
        {
            // Not wall running
            _isWallRunning = false;

            // Reset the contact info
            _contactInfo = new RaycastHit();

            // Reset the current wall
            _currentWall = null;

            // If the previous wall is not null, invoke the on wall run end event
            if (previousWall != null)
                OnWallRunEnd?.Invoke(this);

            return;
        }

        // Set the wall running flag
        _isWallRunning = true;

        // Set the contact info
        _contactInfo = currentHitInfo;

        // Set the current wall
        _currentWall = currentHitInfo.collider.gameObject;

        // If the previous wall is null, invoke the on wall run start event
        if (previousWall == null)
            OnWallRunStart?.Invoke(this);

        // var currentAngle = _isWallRunningLeft
        //     ? leftHitAngle
        //     : rightHitAngle;

        // Debug.Log($"Currently Colliding with {_currentWall.name} ({currentAngle:0.00}°)");
    }

    private void UpdateMovement()
    {
        // If the player jumps this frame, jump
        if (_isJumpThisFrame)
        {
            WallJump();

            // Reset the jump this frame flag
            _isJumpThisFrame = false;

            return;
        }

        // If the player has jumped this frame, return
        if (_isCurrentlyJumping)
        {
            // Reset the is currently jumping flag
            _isCurrentlyJumping = false;

            return;
        }

        // Update the wall running movement
        UpdateWallRunningMovement();
    }

    private void UpdateWallRunningMovement()
    {
        // return if the player is not wall running
        if (!_isWallRunning)
        {
            Debug.Log($"Returning because the player is not wall running!");
            return;
        }

        // Get the current move multiplier
        var currentMoveMult = ParentComponent.IsSprinting
            ? ParentComponent.BasicPlayerMovement.SprintMultiplier
            : 1;

        // Get the wall running forward direction
        var forwardDirection = _isWallRunningLeft
            ? Vector3.Cross(_contactInfo.normal, Vector3.up)
            : Vector3.Cross(Vector3.up, _contactInfo.normal);

        // Get the wall running upward direction
        var upwardDirection = _isWallRunningLeft
            ? Vector3.Cross(forwardDirection, _contactInfo.normal)
            : Vector3.Cross(_contactInfo.normal, forwardDirection);

        // var upwardMovement = upwardDirection * -1;
        var upwardMovement = upwardDirection * 0;

        // Get the forward movement based on the input
        var forwardMovement = forwardDirection * ForwardInput;

        // Get the velocity vector
        var currentYVelocity = ParentComponent.Rigidbody.velocity.y;
        var updatedYVelocity = Mathf.Clamp(currentYVelocity - fallAcceleration, -maxFallSpeed, float.MaxValue);

        // Get the move vector
        // var moveVector = (forwardDirection + upwardMovement).normalized;
        var moveVector = forwardMovement + upwardMovement + (Vector3.up * updatedYVelocity);

        // Normalize the move vector
        if (moveVector.magnitude > 1)
            moveVector.Normalize();

        // Create a target velocity vector
        var targetVelocity = moveVector * (ParentComponent.MovementSpeed * currentMoveMult);

        // This is the force required to reach the target velocity in EXACTLY one frame
        var targetForce = (targetVelocity - ParentComponent.Rigidbody.velocity) / Time.fixedDeltaTime;

        // Calculate the force that is going to be applied to the player THIS frame
        var force = targetForce.normalized * ParentComponent.Acceleration;

        // If the player only needs to move a little bit, just set the force to the target force
        if (targetForce.magnitude < ParentComponent.Acceleration)
            force = targetForce;

        // If the force is greater than the target force, clamp it
        if (force.magnitude > targetForce.magnitude)
            force = targetForce;

        // Apply the force to the rigidbody
        ParentComponent.Rigidbody.AddForce(force, ForceMode.Acceleration);

        // Activate the footstep timer
        _footstepTimer.SetActive(true);
    }

    private void WallJump()
    {
        // Set the is jump this frame flag to true
        _isJumpThisFrame = false;

        // Return if the player is not wall running
        if (!_isWallRunning)
            return;

        // Get the normal of the contact point
        var normal = _contactInfo.normal;

        var forwardLine = _isWallRunningLeft
            ? Vector3.Cross(normal, Vector3.up)
            : Vector3.Cross(Vector3.up, normal);

        var upwardLine = _isWallRunningLeft
            ? Vector3.Cross(forwardLine, normal)
            : Vector3.Cross(normal, forwardLine);

        var anglePercent = wallJumpAngle / (90 - 0);

        // Get a vector that combines the normal and the world's up vector
        var wallJumpDirection = Vector3.Lerp(normal, upwardLine, anglePercent).normalized;

        // Get the wall jump force
        var wallJumpForceVector = wallJumpDirection * wallJumpForce;

        // Get the wall running object that corresponds to the current contact point
        _jumpObject = _currentWall;

        // Force all the booleans to false
        _isWallRunning = false;
        _isWallRunningLeft = false;
        _isWallRunningRight = false;

        // Add a force to the rigid body
        ParentComponent.Rigidbody.AddForce(wallJumpForceVector, ForceMode.VelocityChange);

        _isCurrentlyJumping = true;

        // Play the jump sound
        SoundManager.Instance.PlaySfx(jumpSound);
    }

    #region Event Functions

    private void GravityOnWallRunStart(PlayerWallRunning playerWallRunning)
    {
        // Disable the player's gravity
        ParentComponent.Rigidbody.useGravity = false;
    }

    private void GravityOnWallRunEnd(PlayerWallRunning playerWallRunning)
    {
        // Enable the player's gravity
        ParentComponent.Rigidbody.useGravity = true;
    }

    private void AutoWallJump(PlayerWallRunning obj)
    {
        var contactPoint = _contactInfo.point;

        // Get the normal of the contact point
        var normal = _contactInfo.normal;

        var forwardLine = _isWallRunningLeft
            ? Vector3.Cross(normal, Vector3.up)
            : Vector3.Cross(Vector3.up, normal);

        var upwardLine = _isWallRunningLeft
            ? Vector3.Cross(forwardLine, normal)
            : Vector3.Cross(normal, forwardLine);

        var anglePercent = 10f / 90;
        var wallJumpDirection = Vector3.Lerp(normal, upwardLine, anglePercent).normalized;

        var wallJumpForceVector = wallJumpDirection * autoWallJumpForce;

        // ParentComponent.Rigidbody.CustomAddForce(wallJumpForceVector, ForceMode.VelocityChange);
        ParentComponent.Rigidbody.AddForce(wallJumpForceVector, ForceMode.VelocityChange);
    }

    private void TransferVelocityOnWallRunStart(PlayerWallRunning obj)
    {
        // Store the player's velocity
        var previousVelocity = ParentComponent.Rigidbody.velocity;

        // // Get the forward vector along the wall
        var forwardVector = _isWallRunningLeft
            ? Vector3.Cross(_contactInfo.normal, Vector3.up)
            : Vector3.Cross(Vector3.up, _contactInfo.normal);

        // Normalize the forward vector
        forwardVector.Normalize();

        // Get the dot product of the player's velocity and the forward vector
        // This will be the player's speed as they start wall running
        var dotProduct = Vector3.Dot(previousVelocity, forwardVector);

        // Kill the player's velocity
        ParentComponent.Rigidbody.velocity = Vector3.zero;

        // Add the player's speed along the wall
        ParentComponent.Rigidbody.AddForce(forwardVector * dotProduct, ForceMode.VelocityChange);
    }

    #endregion

    #region Debugging

    public override string GetDebugText()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Wall Running: {_isWallRunning}");

        if (_isWallRunning)
        {
            var wallRunningDirection = _isWallRunningLeft
                ? "Left"
                : "Right";
            sb.AppendLine($"\tWall Running Direction: {wallRunningDirection}");

            sb.AppendLine($"\tContact Point: {_contactInfo.point}");
        }

        sb.AppendLine($"Jump this frame: {_isJumpThisFrame}");

        return sb.ToString();
    }

    private void OnDrawGizmos()
    {
        // Draw the rays that extend from the player's sides
        Gizmos.color = Color.yellow;

        // Draw the left and right rays
        if (ParentComponent != null)
        {
            Gizmos.DrawRay(transform.position,
                -ParentComponent.CameraPivot.transform.right * wallRunningDetectionDistance);
            Gizmos.DrawRay(transform.position,
                ParentComponent.CameraPivot.transform.right * wallRunningDetectionDistance);
        }

        // // Return if the player is not wall running
        // if (!_isWallRunning)
        //     return;
        //
        // // Return if the contact point index is out of range
        // if (_contactPointIndex < 0 || _contactPointIndex >= _contactPoints.Length)
        //     return;
        //
        // const float sphereSize = 0.1f;
        //
        // var contactPoint = _contactPoints[_contactPointIndex];
        //
        // // Draw a sphere at the contact point
        // Gizmos.color = Color.red;
        // Gizmos.DrawSphere(contactPoint.point, sphereSize);
        //
        // var forwardLine = _isWallRunningLeft
        //     ? Vector3.Cross(contactPoint.normal, Vector3.up)
        //     : Vector3.Cross(Vector3.up, contactPoint.normal);
        //
        // var upwardLine = _isWallRunningLeft
        //     ? Vector3.Cross(forwardLine, contactPoint.normal)
        //     : Vector3.Cross(contactPoint.normal, forwardLine);
        //
        // var anglePercent = wallJumpAngle / (90 - 0);
        //
        // var jumpLine = Vector3.Lerp(contactPoint.normal, upwardLine, anglePercent);
        //
        // const float lineInterval = 0.25f;
        // const float lineDistance = 10f;
        //
        // for (float i = 0; i < lineDistance; i += lineInterval)
        // {
        //     // Draw a sphere at the new point
        //     var forwardPoint = contactPoint.point + (forwardLine * i);
        //     Gizmos.color = Color.magenta;
        //     Gizmos.DrawSphere(forwardPoint, sphereSize);
        //
        //
        //     // Draw a sphere at the new point
        //     var upwardPoint = contactPoint.point + (upwardLine * i);
        //     Gizmos.color = Color.blue;
        //     Gizmos.DrawSphere(upwardPoint, sphereSize);
        //
        //
        //     // Draw a sphere at the new point
        //     var jumpPoint = contactPoint.point + (jumpLine * i);
        //     Gizmos.color = Color.green;
        //     Gizmos.DrawSphere(jumpPoint, sphereSize);
        // }
    }

    #endregion
}