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

    [SerializeField] private float maxFallSpeed;
    [SerializeField] private float fallAcceleration;
    [SerializeField] [Min(0)] private float stationaryFallSpeedMultiplier = 4f;
    [SerializeField] [Min(0)] private float stationaryFallAccelerationMultiplier = 2f;

    [SerializeField] [Min(0)] private float wallRunSpeedThreshold = 1f;

    [SerializeField] [Min(0)] private float wallRunningDetectionDistance = 0.75f;

    [SerializeField] [Min(8)] private int rayCount = 8;

    [SerializeField, Min(0)] private float wallRunStartMinimumSpeed;
    [SerializeField, Min(0)] private float wallRunStartMinimumAirTime = .5f;

    [Header("Wall Jump")] [SerializeField] [Min(0)]
    private float wallJumpForce = 10f;

    [SerializeField] [Range(0, 90)] private float wallJumpAngle = 20f;

    [SerializeField] [Min(0)] private float autoWallJumpForce = .5f;

    [Header("Sounds")] [SerializeField] private SoundPool footstepSoundPool;

    [SerializeField] private float walkingFootstepInterval = 0.35f;
    [SerializeField] private float sprintingFootstepInterval = 0.25f;

    [SerializeField] private Sound jumpSound;

    [Header("Looking While on Walls")] [SerializeField]
    private float inwardLookLockAngle = 15;

    [SerializeField] private float outwardLookLockAngle = 60;

    #endregion

    #region Private Fields

    private bool _isWallSliding;
    private bool _isWallRunning;
    private bool _isWallRunningLeft;
    private bool _isWallRunningRight;

    private bool _isJumpThisFrame;

    private bool _isCurrentlyJumping;

    private readonly CountdownTimer _footstepTimer = new(0.5f, true, false);

    private readonly CountdownTimer _reattachTimer = new(0.125f, true, false);

    // Code for the new wall running system
    private GameObject _currentWall, _previousWall;
    private RaycastHit _contactInfo;

    // Arrays of rays
    private Ray[] _rays;
    private Ray _leftRay, _rightRay;
    private Ray _currentRay;

    private readonly Dictionary<Ray, WallRunHitInfo> _wallRunHitInfos = new();

    #endregion

    public event Action<PlayerWallRunning> OnWallRunStart;
    public event Action<PlayerWallRunning> OnWallChanged;
    public event Action<PlayerWallRunning> OnWallRunEnd;

    public event Action<PlayerWallRunning> OnWallSlideStart;

    #region Getters

    public HashSet<InputData> InputActions { get; } = new();

    public override InputActionMap InputActionMap => InputManager.Instance.PControls.PlayerMovementWallRunning;

    public bool IsWallSliding => _isWallSliding;

    public bool IsWallRunning => _isWallRunning;

    public bool IsWallRunningLeft => _isWallRunningLeft;

    public bool IsWallRunningRight => _isWallRunningRight;

    private float ForwardInput => ParentComponent.MovementInput.y;

    public bool CanWallRun
    {
        get => isEnabled;
        set => isEnabled = value;
    }

    public RaycastHit ContactInfo => _contactInfo;

    public float InwardLookLockAngle => inwardLookLockAngle;

    public float OutwardLookLockAngle => outwardLookLockAngle;

    public bool IsCurrentlyJumping => _isCurrentlyJumping;

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

        // Update the rays
        UpdateRays();

        // Add this to the debug manager
        DebugManager.Instance.AddDebuggedObject(this);
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
        // Add the on wall slide start event
        OnWallSlideStart += RegisterOnWallRunStart;
        OnWallSlideStart += PushControls;
        OnWallSlideStart += GravityOnWallRunStart;
        OnWallSlideStart += TransferVelocityOnWallRunStart;

        // Add the on wall run end event
        OnWallRunEnd += AutoWallJump;
        OnWallRunEnd += RemoveControls;
        OnWallRunEnd += GravityOnWallRunEnd;
        OnWallRunEnd += UnregisterOnWallRunEnd;
        OnWallRunEnd += RestartReattachTimer;
        OnWallRunEnd += ResetJumpOnWallRunEnd;
    }

    private void ResetJumpOnWallRunEnd(PlayerWallRunning obj)
    {
        _isJumpThisFrame = false;
        _isCurrentlyJumping = false;
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
        _footstepTimer.SetMaxTime(walkingFootstepInterval);
        _footstepTimer.Update(Time.deltaTime);

        // If this is NOT the active movement script, disable the footstep timer
        if (ParentComponent.CurrentMovementScript != this)
            _footstepTimer.SetActive(false);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        // Update the rays
        UpdateRays();

        // Update the hit info dictionary
        UpdateHitInfoDictionary();

        // Detect if the player is currently wall sliding
        UpdateDetectWallSliding();

        // Detect if the player is currently wall running
        UpdateDetectWallRunning();
    }

    private void UpdateRays()
    {
        // Create an array of rays that go around the player
        _rays = new Ray[rayCount];

        // Populate the array of rays
        for (var i = 0; i < rayCount; i++)
        {
            // Calculate the angle of the ray
            var angle = i * (360f / rayCount);

            var camPivotForward = ParentComponent.CameraPivot.transform.forward;
            var forwardVector = new Vector3(camPivotForward.x, 0, camPivotForward.z).normalized;

            // Rotate the forward vector by the angle to get the direction of the ray
            var direction = Quaternion.Euler(0, angle, 0) * forwardVector;

            _rays[i] = new Ray(transform.position, direction);
        }

        // Get the left ray from the array of rays
        _leftRay = _rays
            .OrderByDescending(ray => Vector3.Dot(ray.direction, -ParentComponent.CameraPivot.transform.right))
            .First();

        // Get the right ray from the array of rays
        _rightRay = _rays
            .OrderByDescending(ray => Vector3.Dot(ray.direction, ParentComponent.CameraPivot.transform.right))
            .First();
    }

    public override void FixedMovementUpdate()
    {
        // Update the movement if the player is wall running
        UpdateMovement();
    }

    private void UpdateHitInfoDictionary()
    {
        // Clear the dictionary of wall run hit infos
        _wallRunHitInfos.Clear();

        foreach (var cRay in _rays)
        {
            // Get the hit information for the current ray
            var hit = Physics.Raycast(
                cRay,
                out var hitInfo, wallRunningDetectionDistance,
                wallLayer
            );

            // If the ray did not hit anything, continue
            if (!hit)
            {
                // Add the ray to the dictionary
                _wallRunHitInfos.Add(cRay, new WallRunHitInfo(hitInfo, false, default, default));

                continue;
            }

            // Get the angle between the current ray and the left ray
            var leftRayAngle = Vector3.Angle(_leftRay.direction, cRay.direction);

            // Get the angle between the current ray and the right ray
            var rightRayAngle = Vector3.Angle(_rightRay.direction, cRay.direction);

            // Get the forward vector
            var wallForwardVector = Vector3.Cross(hitInfo.normal, Vector3.up).normalized;

            // Add the hit info to the dictionary
            _wallRunHitInfos.Add(cRay, new WallRunHitInfo(hitInfo, true, leftRayAngle, rightRayAngle));
        }
    }

    private void UpdateDetectWallSliding()
    {
        // Reset the wall sliding flag
        _isWallSliding = false;

        // Store the previous wall
        _previousWall = _currentWall;

        // Reset the current wall
        _currentWall = null;

        // Reset the contact info before checking
        _contactInfo = new RaycastHit();

        var closestHitInfo = new RaycastHit();
        var closestHitDistance = float.MaxValue;
        var closestRay = default(Ray);

        // Check each ray in the dictionary
        foreach (var keyPair in _wallRunHitInfos)
        {
            var cRay = keyPair.Key;
            var hitInfo = keyPair.Value;

            // Skip the ray if it did not hit anything
            if (!hitInfo.IsHit)
                continue;

            // Skip if the player is grounded
            if (ParentComponent.IsGrounded)
                continue;

            // If the current hit distance is less than the closest hit distance, update the closest hit info
            if (hitInfo.HitInfo.distance >= closestHitDistance)
                continue;

            // Get the normal of the wall the ray is hitting
            var wallNormal = hitInfo.HitInfo.normal;

            // Get the angle between the wall normal and the player's forward vector
            var wallAngle = Vector3.Angle(wallNormal, ParentComponent.Orientation.forward) - 90;

            // TODO: Replace with actual angle checks
            const float tmpAngle = 180;
            if (wallAngle > tmpAngle || wallAngle < -tmpAngle)
                continue;

            var isInBasicMovement = ParentComponent.CurrentMovementScript is BasicPlayerMovement;

            if (isInBasicMovement)
            {
                // Get the player's velocity vector
                var velocityVector = ParentComponent.Rigidbody.velocity;
                
                // Get the player's input vector in relation to their camera pivot's orientation
                var inputVector = ParentComponent.MovementInput.y * ParentComponent.CameraPivot.transform.forward +
                                  ParentComponent.MovementInput.x * ParentComponent.CameraPivot.transform.right;
                inputVector *= ParentComponent.MovementSpeed;

                var totalVector = velocityVector + inputVector;
                
                // Get the dot product of the player's velocity and the wall normal
                var dotProduct = Vector3.Dot(-totalVector, wallNormal);

                // Log the dot product
                Debug.Log($"Wall dot product: {dotProduct}");

                // If the dot product is less than the wall run start minimum speed, continue
                if (dotProduct < wallRunStartMinimumSpeed)
                    continue;
                
                // Get the time the player has been in the air
                // If it is less than the wall run start minimum air time, continue
                if (ParentComponent.MidAirTime < wallRunStartMinimumAirTime)
                    continue;
            }

            // Set the wall sliding flag to true
            _isWallSliding = true;

            closestHitDistance = hitInfo.HitInfo.distance;
            closestHitInfo = hitInfo.HitInfo;
            closestRay = cRay;
        }

        // If the player is still not wall sliding, return
        if (!_isWallSliding)
        {
            _contactInfo = default;
            _currentWall = null;
            _currentRay = default;

            // If the previous wall is not null, invoke the on wall run end event
            if (_previousWall != null)
                OnWallRunEnd?.Invoke(this);

            // If the previous wall is not the current wall, invoke the on wall changed event
            if (_previousWall != _currentWall)
                OnWallChanged?.Invoke(this);

            return;
        }

        // Set the closet hit info as the contact info
        _contactInfo = closestHitInfo;

        // Set the current wall
        _currentWall = closestHitInfo.collider.gameObject;

        // Set the current ray
        _currentRay = closestRay;

        // If the previous wall is null, invoke the on wall slide start event
        if (_previousWall == null)
            OnWallSlideStart?.Invoke(this);

        // If the previous wall is not the current wall, invoke the on wall changed event
        if (_previousWall != _currentWall)
            OnWallChanged?.Invoke(this);
    }

    private void UpdateDetectWallRunning()
    {
        // Get the current wall running hit info
        var containsCurrentRay = _wallRunHitInfos.ContainsKey(_currentRay);
        var currentWallRunHitInfo = containsCurrentRay ? _wallRunHitInfos[_currentRay] : null;
        var currentRayHit = containsCurrentRay && currentWallRunHitInfo.IsHit;

        // Determine if the angle is within the tolerance
        var angleWithinTolerance = false;
        if (currentWallRunHitInfo != null)
        {
            if (currentWallRunHitInfo.LeftRayAngle < currentWallRunHitInfo.RightRayAngle)
                angleWithinTolerance = currentWallRunHitInfo.LeftRayAngle <= wallAngleTolerance;
            else
                angleWithinTolerance = currentWallRunHitInfo.RightRayAngle <= wallAngleTolerance;
        }

        // Get the current hit info
        RaycastHit currentHitInfo;

        // Reset both wall running directions
        _isWallRunningLeft = false;
        _isWallRunningRight = false;

        // Determine if the player is wall running.
        // If not, return
        if (currentRayHit && !ParentComponent.IsGrounded && angleWithinTolerance)
        {
            currentHitInfo = _wallRunHitInfos[_currentRay].HitInfo;

            _isWallRunningLeft = Vector3.Dot(_leftRay.direction, _currentRay.direction) > 0;
            _isWallRunningRight = !_isWallRunningLeft;
        }
        else
        {
            // Not wall running
            _isWallRunning = false;

            return;
        }

        // Set the wall running flag
        _isWallRunning = true;

        // Set the contact info
        _contactInfo = currentHitInfo;

        // Set the current wall
        _currentWall = currentHitInfo.collider.gameObject;

        // If the previous wall is null, invoke the on wall run start event
        if (_previousWall == null)
            OnWallRunStart?.Invoke(this);
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
        // return if the player is not wall running or wall sliding
        if (!_isWallRunning && !_isWallSliding)
            return;


        // Get the current move multiplier
        var currentMoveMult = ParentComponent.IsSprinting
            ? ParentComponent.BasicPlayerMovement.SprintMultiplier
            : 1;

        // Get the wall running forward direction
        var forwardDirection = _isWallRunningLeft
            ? Vector3.Cross(_contactInfo.normal, Vector3.up)
            : Vector3.Cross(Vector3.up, _contactInfo.normal);

        var forwardVelocityDot = Vector3.Dot(ParentComponent.Rigidbody.velocity, forwardDirection);

        // Get the wall running upward direction
        var upwardDirection = _isWallRunningLeft
            ? Vector3.Cross(forwardDirection, _contactInfo.normal)
            : Vector3.Cross(_contactInfo.normal, forwardDirection);

        // var upwardMovement = upwardDirection * -1;
        var upwardMovement = upwardDirection * 0;

        // Get the forward movement based on the input
        var forwardMovement = forwardDirection * ForwardInput;

        // If the player is wall sliding, but not wall running, reset the forward movement
        if (_isWallSliding && !_isWallRunning)
            forwardMovement = Vector3.zero;

        var currentMaxFallSpeed = maxFallSpeed;
        var currentFallAcceleration = fallAcceleration;

        // If the player is not moving fast enough, their max fall speed increases
        // If the player is not moving fast enough, their fall acceleration increases
        if (forwardVelocityDot < wallRunSpeedThreshold)
        {
            // Reverse interpolate the max fall speed based on the player's forward velocity in relation to the threshold
            var lerpAmount = 1 - Mathf.InverseLerp(0, wallRunSpeedThreshold, forwardVelocityDot);

            // Set the max fall speed to the stationary fall speed multiplier
            currentMaxFallSpeed = Mathf.Lerp(maxFallSpeed, maxFallSpeed * stationaryFallSpeedMultiplier, lerpAmount);

            // Set the fall acceleration to the stationary fall acceleration multiplier
            currentFallAcceleration = Mathf.Lerp(fallAcceleration,
                fallAcceleration * stationaryFallAccelerationMultiplier, lerpAmount);
        }

        // Get the move vector
        // var moveVector = (forwardDirection + upwardMovement).normalized;
        // var moveVector = forwardMovement + upwardMovement + (Vector3.up * updatedYVelocity);
        var moveVector = forwardMovement + upwardMovement;

        // Normalize the move vector
        if (moveVector.magnitude > 1)
            moveVector.Normalize();

        // Create a target velocity vector before fall
        var targetVelocityBeforeFall = moveVector * (ParentComponent.MovementSpeed * currentMoveMult) +
                                       new Vector3(0, ParentComponent.Rigidbody.velocity.y, 0);

        // Create the target velocity after fall
        var targetVelocityAfterFall =
            new Vector3(targetVelocityBeforeFall.x, -currentMaxFallSpeed, targetVelocityBeforeFall.z);

        // Create a target velocity vector
        var targetVelocity = Vector3.MoveTowards(
            targetVelocityBeforeFall,
            targetVelocityAfterFall,
            currentFallAcceleration
        );

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

        // Return if the player is not wall running or wall sliding
        if (!_isWallRunning && !_isWallSliding)
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

        // Set the y velocity to 0
        ParentComponent.Rigidbody.velocity =
            new Vector3(ParentComponent.Rigidbody.velocity.x, 0, ParentComponent.Rigidbody.velocity.z);
    }

    #endregion

    #region Debugging

    public override string GetDebugText()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Wall Running: {_isWallRunning}");

        if (!_isWallRunning)
            return sb.ToString();

        var wallRunningDirection = _isWallRunningLeft
            ? "Left"
            : "Right";

        sb.AppendLine($"\tWall Running Direction: {wallRunningDirection}");

        sb.AppendLine($"\tContact Point: {_contactInfo.point}");

        sb.AppendLine($"\tAngle: {_wallRunHitInfos[_currentRay].LeftRayAngle:0.00}°");
        sb.AppendLine($"\tAngle: {_wallRunHitInfos[_currentRay].RightRayAngle:0.00}°");

        return sb.ToString();
    }

    private void OnDrawGizmos()
    {
        // Return if the rays are null
        if (_rays == null)
            return;

        var rayLength = wallRunningDetectionDistance;

        // // Draw the rays
        // for (var i = 0; i < rayCount; i++)
        // {
        //     Gizmos.color = Color.yellow;
        //     Gizmos.DrawRay(transform.position, _rays[i].direction * rayLength);
        // }

        // Draw the left and right rays
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(transform.position, _leftRay.direction * rayLength);
        Gizmos.DrawRay(transform.position, _rightRay.direction * rayLength);

        // Draw the current ray (if it exists)
        Gizmos.color = Color.red;

        if (_wallRunHitInfos.ContainsKey(_currentRay))
            Gizmos.DrawRay(transform.position, _currentRay.direction * rayLength);

        // If there is a contact point, draw the normal
        if (_contactInfo.collider != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(_contactInfo.point, _contactInfo.point + _contactInfo.normal * 10);

            const float interval = 0.5f;

            // Get the forward vector of the wall
            var forwardVector = _isWallRunningLeft
                ? Vector3.Cross(_contactInfo.normal, Vector3.up)
                : Vector3.Cross(Vector3.up, _contactInfo.normal);

            for (int i = 0; i < 10; i++)
                Gizmos.DrawSphere(_contactInfo.point + forwardVector * interval * i, 0.1f);
        }
    }

    #endregion

    private class WallRunHitInfo
    {
        public RaycastHit HitInfo { get; }
        public bool IsHit { get; }
        public float LeftRayAngle { get; }
        public float RightRayAngle { get; }

        public WallRunHitInfo(
            RaycastHit hitInfo, bool isHit,
            float leftRayAngle, float rightRayAngle
        )
        {
            HitInfo = hitInfo;
            IsHit = isHit;
            LeftRayAngle = leftRayAngle;
            RightRayAngle = rightRayAngle;
        }
    }
}