using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Collections;
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

    [SerializeField, Min(0)] private float desiredWallDistance = 0.25f;
    [SerializeField, Min(0)] private float desiredWallDistanceForce = 1;

    [SerializeField, Range(0, 180)] private float impossibleWallAngle = 150;

    [Header("Wall Jump")] [SerializeField] [Min(0)]
    private float wallJumpForce = 10f;

    [SerializeField] [Range(0, 90)] private float wallJumpAngle = 20f;

    [SerializeField] [Min(0)] private float autoWallJumpForce = .5f;

    [SerializeField] private Sound jumpSound;

    [Header("Looking While on Walls")] [SerializeField]
    private float inwardLookLockAngle = 15;

    [SerializeField] private float outwardLookLockAngle = 60;

    [Header("Wall Climb")] [SerializeField]
    private LayerMask wallClimbLayer;

    [SerializeField, Min(0)] private float wallClimbAngle;

    [SerializeField, Min(0)] private float wallClimbStayAngle;
    [SerializeField, Min(0)] private float wallRunningInputSpeed = 0.5f;
    [SerializeField, Min(0)] private float wallClimbStartVelocity = 4f;
    [SerializeField, Range(0, 1)] private float wallClimbStartVelocityTransfer = 0.75f;
    [SerializeField, Min(0)] private float maxWallClimbStartVelocity = 12f;
    [SerializeField, Min(0)] private float minimumWallClimbGroundedVelocity = 6f;

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
    private List<Ray> _rays;
    private Ray _leftRay, _rightRay;
    private Ray _currentRay;

    private readonly Dictionary<Ray, WallRunHitInfo> _wallRunHitInfos = new();

    private bool _isWallClimbing;
    private bool _previouslyWallClimbing;

    #endregion

    public event Action<PlayerWallRunning> OnWallRunStart;
    public event Action<PlayerWallRunning> OnWallChanged;
    public event Action<PlayerWallRunning> OnWallRunEnd;

    public event Action<PlayerWallRunning> OnWallSlideStart;

    public event Action<PlayerWallRunning> OnWallClimbStart;

    public event Action<PlayerWallRunning> OnWallClimbEnd;

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

    public bool IsWallClimbing => _isWallClimbing;

    #endregion

    #region Initialization Functions

    protected override void CustomAwake()
    {
        // Initialize the input
        InitializeInput();
    }

    private void Start()
    {
        // Initialize the events
        InitializeEvents();

        // Update the rays
        UpdateRays();

        // Add this to the debug manager
        DebugManager.Instance.AddDebuggedObject(this);
    }

    private void OnDestroy()
    {
        // Remove this from the debug manager
        DebugManager.Instance.RemoveDebuggedObject(this);
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

        OnWallRunStart += KillVelocityOnWallRunStart;
        OnWallChanged += KillVelocityOnWallRunStart;

        // Add the on wall run end event
        OnWallRunEnd += AutoWallJump;
        OnWallRunEnd += RemoveControls;
        OnWallRunEnd += GravityOnWallRunEnd;
        OnWallRunEnd += UnregisterOnWallRunEnd;
        OnWallRunEnd += RestartReattachTimer;
        OnWallRunEnd += ResetJumpOnWallRunEnd;

        // Add the on wall climb start event
        OnWallClimbStart += TransferVelocityOnWallClimbStart;
    }

    private void KillVelocityOnWallRunStart(PlayerWallRunning obj)
    {
        // Return if there is no current wall, return
        if (_currentWall == null)
            return;

        // Get the current rigidbody velocity
        var velocity = ParentComponent.Rigidbody.velocity;

        ParentComponent.Rigidbody.velocity = new Vector3(
            velocity.x,
            Mathf.Min(0, velocity.y),
            velocity.z
        );

        // Debug.Log($"Killed Velocity: {ParentComponent.Rigidbody.velocity}");
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
        _reattachTimer.Update(Time.deltaTime);
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
        _rays = new List<Ray>(rayCount);

        var camPivotForward = ParentComponent.CameraPivot.transform.forward;
        var forwardVector = new Vector3(camPivotForward.x, 0, camPivotForward.z).normalized;
        var rayInterval = 360f / rayCount;

        // Populate the array of rays
        for (var i = 0; i < rayCount; i++)
        {
            // Calculate the angle of the ray
            var angle = i * rayInterval;

            // Rotate the forward vector by the angle to get the direction of the ray
            var direction = Quaternion.Euler(0, angle, 0) * forwardVector;

            _rays.Add(new Ray(transform.position, direction));
        }

        var camPivotLeft = -ParentComponent.CameraPivot.transform.right;
        var camPivotRight = ParentComponent.CameraPivot.transform.right;

        // Get the left ray from the array of rays
        // _leftRay = _rays
        //     .OrderByDescending(ray => Vector3.Dot(ray.direction, camPivotLeft))
        //     .First();

        var leftMostRay = _rays[0];
        foreach (var ray in _rays)
        {
            var leftDot = Vector3.Dot(ray.direction, camPivotLeft);

            if (leftDot > Vector3.Dot(leftMostRay.direction, camPivotLeft))
                leftMostRay = ray;
        }

        _leftRay = leftMostRay;

        // Get the right ray from the array of rays
        // _rightRay = _rays
        //     .OrderByDescending(ray => Vector3.Dot(ray.direction, camPivotRight))
        //     .First();

        var rightMostRay = _rays[0];
        foreach (var ray in _rays)
        {
            var rightDot = Vector3.Dot(ray.direction, camPivotRight);

            if (rightDot > Vector3.Dot(rightMostRay.direction, camPivotRight))
                rightMostRay = ray;
        }

        _rightRay = rightMostRay;
    }

    public override void FixedMovementUpdate()
    {
        if (_isWallRunning)
            ParentComponent.ForceSetSprinting(true);

        // Update the movement if the player is wall running
        UpdateMovement();
    }

    private void UpdateHitInfoDictionary()
    {
        // Clear the dictionary of wall run hit infos
        _wallRunHitInfos.Clear();

        var jobHits = new NativeArray<RaycastHit>(_rays.Count, Allocator.TempJob);

        // Populate the commands
        var jobCommands = new NativeArray<RaycastCommand>(_rays.Count, Allocator.TempJob);

        var queryParameters = new QueryParameters
        {
            layerMask = wallLayer, hitBackfaces = false, hitMultipleFaces = false,
            hitTriggers = QueryTriggerInteraction.Ignore
        };

        for (var i = 0; i < _rays.Count; i++)
        {
            jobCommands[i] = new RaycastCommand(
                _rays[i].origin, _rays[i].direction, queryParameters, wallRunningDetectionDistance
            );
        }

        // Schedule the batch of raycast commands
        var jobHandle = RaycastCommand.ScheduleBatch(jobCommands, jobHits, 1, 1, default);

        // Wait for the job to complete
        jobHandle.Complete();

        for (var i = 0; i < _rays.Count; i++)
        {
            var hitInfo = jobHits[i];
            var cRay = _rays[i];

            // If the ray did not hit anything, continue
            if (hitInfo.collider == false)
            {
                _wallRunHitInfos.Add(cRay, new WallRunHitInfo(hitInfo, false, default, default));
                continue;
            }

            // Get the angle between the current ray and the left ray
            var leftRayAngle = Vector3.Angle(_leftRay.direction, cRay.direction);

            // Get the angle between the current ray and the right ray
            var rightRayAngle = Vector3.Angle(_rightRay.direction, cRay.direction);

            // Add the hit info to the dictionary
            _wallRunHitInfos.Add(cRay, new WallRunHitInfo(hitInfo, true, leftRayAngle, rightRayAngle));
        }

        // Dispose of the job hits
        jobCommands.Dispose();

        // Dispose of the results
        jobHits.Dispose();
    }

    private void UpdateDetectWallSliding()
    {
        // Reset the wall sliding flag
        _isWallSliding = false;

        _previouslyWallClimbing = _isWallClimbing;

        // Reset the wall climbing flag
        _isWallClimbing = false;

        // Store the previous wall
        _previousWall = _currentWall;

        // Reset the current wall
        _currentWall = null;

        // Reset the contact info before checking
        _contactInfo = new RaycastHit();

        var closestHitInfo = new RaycastHit();
        var closestHitDistance = float.MaxValue;
        var closestRay = default(Ray);

        var isInBasicMovement = ParentComponent.CurrentMovementScript is BasicPlayerMovement;
        var isInWallRunningMovement = ParentComponent.CurrentMovementScript is PlayerWallRunning;

        var parentForward = ParentComponent.Orientation.forward;
        var parentRight = ParentComponent.Orientation.right;
        var parentVelocity = ParentComponent.Rigidbody.velocity;

        // Check each ray in the dictionary
        foreach (var keyPair in _wallRunHitInfos)
        {
            var cRay = keyPair.Key;
            var hitInfo = keyPair.Value;

            // Get the ray's angle in relation to the player's orientation forward
            var rayAngle = Vector3.Angle(cRay.direction, parentForward);

            // Skip the ray if it did not hit anything
            if (!hitInfo.IsHit)
                continue;

            // Skip if the player is grounded
            if (ParentComponent.IsGrounded && rayAngle > wallClimbAngle)
                continue;

            // If the current hit distance is less than the closest hit distance, update the closest hit info
            if (hitInfo.HitInfo.distance >= closestHitDistance)
                continue;

            // Get the normal of the wall the ray is hitting
            var wallNormal = hitInfo.HitInfo.normal;

            // Get the angle between the wall normal and the player's forward vector
            var wallAngle = Vector3.Angle(wallNormal, parentForward);

            // // TODO: Replace with actual angle checks
            // const float tmpAngle = 180;
            // if (wallAngle > tmpAngle || wallAngle < -tmpAngle)
            //     continue;

            // If the player is in basic movement,
            // check if they are moving fast enough and have been in the air long enough
            if (isInBasicMovement)
            {
                // Get the player's input vector in relation to their camera pivot's orientation
                var inputVector = ParentComponent.MovementInput.y * parentForward +
                                  ParentComponent.MovementInput.x * parentRight;
                inputVector *= ParentComponent.MovementSpeed;

                var totalVector = parentVelocity + inputVector;

                // Get the dot product of the player's velocity and the wall normal
                var dotProduct = Vector3.Dot(-totalVector, wallNormal);

                // If the dot product is less than the wall run start minimum speed, continue
                if (dotProduct < wallRunStartMinimumSpeed)
                    continue;

                // Get the time the player has been in the air
                // If it is less than the wall run start minimum air time, continue
                if (ParentComponent.MidAirTime < wallRunStartMinimumAirTime && rayAngle > wallClimbAngle)
                    continue;

                // Get the lateral velocity of the player
                var lateralVelocity = new Vector3(parentVelocity.x, 0, parentVelocity.z);

                // Get the dot product between the lateral velocity and the wall normal
                var lateralDotProduct = Vector3.Dot(lateralVelocity, -wallNormal);

                // If the player is grounded AND their velocity is less than the minimum wall climb grounded velocity, continue
                if (ParentComponent.IsGrounded && lateralDotProduct < minimumWallClimbGroundedVelocity)
                    continue;
            }

            // If the player is grounded AND they are in wall running movement, continue
            if (ParentComponent.IsGrounded && isInWallRunningMovement && parentVelocity.y <= 0)
                continue;

            // If the ray is OUTSIDE the wall climb angle, but normal of the wall is 
            // damn near the opposite of the player's orientation forward, continue
            var wallForwardVector = Vector3.Cross(wallNormal, Vector3.up).normalized;

            // Orient the wall forward vector to the correct direction
            if (Vector3.Dot(wallForwardVector, parentForward) < 0)
                wallForwardVector *= -1;

            var normalOrientationAngle = Vector3.Angle(wallForwardVector, parentForward);
            if (normalOrientationAngle >= impossibleWallAngle)
            {
                if (rayAngle > wallClimbAngle)
                    continue;

                var isOnWallClimbLayer = Physics.Raycast(
                    cRay,
                    out _,
                    wallRunningDetectionDistance,
                    wallClimbLayer
                );

                if (!isOnWallClimbLayer)
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
            ResetWallSliding();
            return;
        }

        // Set the closet hit info as the contact info
        _contactInfo = closestHitInfo;

        // Set the current wall
        _currentWall = closestHitInfo.collider.gameObject;

        // Set the current ray
        _currentRay = closestRay;

        // Perform a raycast to check if the player is wall climbing
        var wallClimbHit = Physics.Raycast(
            _currentRay,
            out _,
            wallRunningDetectionDistance,
            wallClimbLayer
        );

        // Get the angle of the current ray in relation to the orientation's forward vector
        // If the angle is withing the wall climbing angle, set the flag to true
        var currentRayAngle = Vector3.Angle(_currentRay.direction, ParentComponent.Orientation.forward);

        // If the current ray angle is within the wall climb angle, but there is no wall climb hit, return
        if (currentRayAngle <= wallClimbAngle && !wallClimbHit)
        {
            ResetWallSliding();
            return;
        }

        var wallClimbAngleSatisfied = (_previouslyWallClimbing)
            ? currentRayAngle <= wallClimbStayAngle
            : currentRayAngle <= wallClimbAngle;

        _isWallClimbing = wallClimbAngleSatisfied && !_isWallRunning && wallClimbHit;

        // If the previous wall is null, invoke the on wall slide start event
        if (_previousWall == null)
            OnWallSlideStart?.Invoke(this);

        // If the previous wall is not the current wall, invoke the on wall changed event
        if (_previousWall != _currentWall)
            OnWallChanged?.Invoke(this);

        // If the player is wall climbing, invoke the on wall climb start event
        if (_isWallClimbing && !_previouslyWallClimbing)
            OnWallClimbStart?.Invoke(this);

        // If the player was previously wall climbing, invoke the on wall climb end event
        if (_previouslyWallClimbing && !_isWallClimbing)
            OnWallClimbEnd?.Invoke(this);

        return;

        void ResetWallSliding()
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

            // If the player was previously wall climbing, invoke the on wall climb end event
            if (_previouslyWallClimbing && !_isWallClimbing)
                OnWallClimbEnd?.Invoke(this);
        }
    }

    private void UpdateDetectWallRunning()
    {
        // Get the current wall running hit info
        var containsCurrentRay = _wallRunHitInfos.ContainsKey(_currentRay);
        WallRunHitInfo? currentWallRunHitInfo = containsCurrentRay ? _wallRunHitInfos[_currentRay] : null;
        var currentRayHit = containsCurrentRay && currentWallRunHitInfo.Value.IsHit;

        // Determine if the angle is within the tolerance
        var angleWithinTolerance = false;
        if (currentWallRunHitInfo != null)
        {
            if (currentWallRunHitInfo.Value.LeftRayAngle < currentWallRunHitInfo.Value.RightRayAngle)
                angleWithinTolerance = Mathf.Abs(currentWallRunHitInfo.Value.LeftRayAngle) <= wallAngleTolerance;
            else
                angleWithinTolerance = Mathf.Abs(currentWallRunHitInfo.Value.RightRayAngle) <= wallAngleTolerance;
        }

        // Get the current hit info
        RaycastHit currentHitInfo;

        // Reset both wall running directions
        _isWallRunningLeft = false;
        _isWallRunningRight = false;

        // Determine if the player is wall running.
        // If not, return
        if (currentRayHit && !ParentComponent.IsGrounded && angleWithinTolerance && !_isWallClimbing)
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
        if (_previousWall == null || (_previouslyWallClimbing))
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
        if (!_isWallClimbing)
            UpdateWallRunningMovement();
        else
            UpdateWallClimbingMovement();

        // Move the player closer to the wall
        UpdateWallDistance();
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
            currentMaxFallSpeed =
                Mathf.Lerp(maxFallSpeed, maxFallSpeed * stationaryFallSpeedMultiplier, lerpAmount);

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

    private void UpdateWallClimbingMovement()
    {
        // return if the player is not wall climbing
        if (!_isWallClimbing)
            return;

        // Get the current move multiplier
        var currentMoveMult = ParentComponent.IsSprinting
            ? ParentComponent.BasicPlayerMovement.SprintMultiplier
            : 1;

        // Get the wall running forward direction
        // var rightDirection = _isWallRunningLeft
        //     ? Vector3.Cross(_contactInfo.normal, Vector3.up)
        //     : Vector3.Cross(Vector3.up, _contactInfo.normal);
        var rightDirection = Vector3.Cross(_contactInfo.normal, Vector3.up);

        var rightVelocityDot = Vector3.Dot(ParentComponent.Rigidbody.velocity, rightDirection);

        // Get the wall running upward direction
        // var upwardDirection = _isWallRunningLeft
        //     ? Vector3.Cross(rightDirection, _contactInfo.normal)
        //     : Vector3.Cross(_contactInfo.normal, rightDirection);
        var upwardDirection = Vector3.Cross(rightDirection, _contactInfo.normal);

        var upwardMovement = upwardDirection * (Mathf.Max(0, ParentComponent.MovementInput.y) * wallRunningInputSpeed);

        // upwardMovement *= 0;

        // Get the forward movement based on the input
        var rightMovement = rightDirection * ParentComponent.MovementInput.x;

        var currentMaxFallSpeed = maxFallSpeed;
        var currentFallAcceleration = fallAcceleration;

        // // If the player is not moving fast enough, their max fall speed increases
        // // If the player is not moving fast enough, their fall acceleration increases
        // if (rightVelocityDot < wallRunSpeedThreshold)
        {
            // Reverse interpolate the max fall speed based on the player's forward velocity in relation to the threshold
            var lerpAmount = 1 - Mathf.InverseLerp(0, wallRunSpeedThreshold, rightVelocityDot);

            // Set the max fall speed to the stationary fall speed multiplier
            currentMaxFallSpeed = Mathf.Lerp(
                maxFallSpeed,
                maxFallSpeed * stationaryFallSpeedMultiplier,
                lerpAmount
            );

            // Set the fall acceleration to the stationary fall acceleration multiplier
            currentFallAcceleration = Mathf.Lerp(
                fallAcceleration,
                fallAcceleration * stationaryFallAccelerationMultiplier,
                lerpAmount
            );
        }

        // Get the move vector
        var moveVector = rightMovement + upwardMovement;

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

    private void UpdateWallDistance()
    {
        // Return if the player is not wall sliding / wall climbing
        if (!_isWallSliding && !_isWallClimbing)
            return;

        // Return if there is no current wall
        if (_currentWall == null)
            return;

        // Get the distance between the player and the wall
        var distance = _contactInfo.distance;

        // Apply a force to the player to move them closer to the wall
        var force = -_contactInfo.normal * ((distance - desiredWallDistance) * desiredWallDistanceForce);

        // Apply the force to the player
        ParentComponent.Rigidbody.AddForce(force, ForceMode.Acceleration);
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

        // Reset the y velocity
        ParentComponent.Rigidbody.velocity = new Vector3(
            ParentComponent.Rigidbody.velocity.x,
            0,
            ParentComponent.Rigidbody.velocity.z
        );

        // Add a force to the rigid body
        ParentComponent.Rigidbody.AddForce(wallJumpForceVector, ForceMode.VelocityChange);

        _isCurrentlyJumping = true;

        // Play the jump sound
        SoundManager.Instance?.PlaySfx(jumpSound);
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
        // Return if the player is wall climbing
        if (_isWallClimbing)
            return;

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

        // Close the distance between the player and the wall
        const float stickDistance = .25f;
        ParentComponent.Rigidbody.MovePosition(ParentComponent.transform.position +
                                               _currentRay.direction * stickDistance);
    }

    private void TransferVelocityOnWallClimbStart(PlayerWallRunning obj)
    {
        // Store the player's velocity
        var previousVelocity = ParentComponent.Rigidbody.velocity;

        // Get the sideways vector along the wall
        // Normalize the forward vector
        var rightVector = Vector3.Cross(_contactInfo.normal, Vector3.up).normalized;

        // Get the upward vector along the wall
        var upwardVector = Vector3.Cross(rightVector, _contactInfo.normal).normalized;

        // Get the dot product of the player's velocity and the normal vector
        // This will be the player's speed as they start wall running
        var dotProduct = Vector3.Dot(previousVelocity, -_contactInfo.normal);

        // Transfer the player's velocity
        dotProduct *= wallClimbStartVelocityTransfer;

        // Clamp the dot product
        dotProduct = Mathf.Clamp(dotProduct + wallClimbStartVelocity, 0, maxWallClimbStartVelocity);

        // Kill the player's velocity
        ParentComponent.Rigidbody.velocity = Vector3.zero;

        // Add the player's speed along the wall
        ParentComponent.Rigidbody.AddForce(upwardVector * dotProduct, ForceMode.VelocityChange);

        // Debug.Log($"Transferring Velocity: {upwardVector * dotProduct}");
    }

    #endregion

    #region Debugging

    public override string GetDebugText()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Wall Running: {_isWallRunning}");
        sb.AppendLine($"Wall Climbing: {_isWallClimbing}");

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

        // Draw the rays
        foreach (var ray in _rays)
        {
            Gizmos.color = Color.yellow;

            // Get the angle of the ray in relation to the orientation's forward vector
            var angle = Vector3.Angle(ray.direction, ParentComponent.Orientation.forward);

            // If the angle is withing the wall climbing angle, draw the ray in blue
            if (angle < wallClimbAngle)
                Gizmos.color = Color.blue;

            Gizmos.DrawRay(transform.position, ray.direction * rayLength);
        }

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

    private struct WallRunHitInfo
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