using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerSlide : PlayerMovementScript, IUsesInput
{
    private static readonly string UpperSlidingAnimationID = "IsSliding";
    private static readonly string LowerSlidingAnimationID = "Slide";

    #region Serialized Fields

    [SerializeField] private Animator lowerPlayerAnimator;
    [SerializeField] private Animator upperPlayerAnimator;

    [SerializeField] private GameObject _slideMesh;

    [SerializeField] private bool isEnabled = true;

    [SerializeField] private float slideStrength = 10f;

    [SerializeField, Min(1f)] private float slideHeight = 1f;

    [Tooltip("How long before landing the player can still slide")] [SerializeField, Min(0)]
    private float midAirGraceTime = 0.5f;

    [SerializeField, Min(0)] private float slideDeceleration = 1f;
    [SerializeField, Min(0)] private float slideStopVelocityThreshold = 4f;

    [Header("Transfer Velocity when Landing into A Slide")] [SerializeField, Range(0, 1)]
    private float landVelocityTransferMultiplier = 1f;

    [SerializeField, Min(0)] private float landVelocityTransferMax = 20f;

    [Header("Sliding on a Slope")] [SerializeField, Min(0)]
    private float maxSlopeSpeedMultiplier = 2f;

    [SerializeField, Min(0)] private float slopeAcceleration = 16f;
    [SerializeField, Range(0, 90)] private float minSlopeAccelerationAngle = 10f;
    [SerializeField, Range(0, 90)] private float maxSlopeAccelerationAngle = 90f;

    #endregion

    #region Private Fields

    private bool _isSliding;

    private CountdownTimer _midAirGraceTimer;

    private Vector3 _landVelocity;

    private bool _useLandVelocity;

    private bool _slideInputThisFrame;

    private bool _wasSprintingBefore;

    #endregion

    public event Action<PlayerSlide> OnSlideStart;
    public event Action<PlayerSlide> OnSlideEnd;

    #region Getters

    public override InputActionMap InputActionMap => null;

    public HashSet<InputData> InputActions { get; } = new();

    public bool CanSlide
    {
        get => isEnabled;
        set => isEnabled = value;
    }

    public bool IsSetToSlide => _midAirGraceTimer.IsActive && _midAirGraceTimer.IsNotComplete;

    public bool IsSliding => _isSliding;

    #endregion

    #region Initialization Functions

    protected override void CustomAwake()
    {
        // Initialize the input
        InitializeInput();

        // Initialize the midair grace timer
        _midAirGraceTimer = new CountdownTimer(midAirGraceTime, false, true);
        _midAirGraceTimer.OnTimerEnd += () => _midAirGraceTimer.Stop();
        _midAirGraceTimer.Stop();
    }

    public void InitializeInput()
    {
        // Slide button
        InputActions.Add(new InputData(
            InputManager.Instance.PControls.PlayerMovementBasic.Slide, InputType.Performed, OnSlidePerformedStart)
        );
        InputActions.Add(new InputData(
            InputManager.Instance.PControls.PlayerMovementBasic.Slide, InputType.Performed, OnSlidePerformedStop)
        );

        // Jump button
        InputActions.Add(new InputData(
            InputManager.Instance.PControls.PlayerMovementBasic.Jump, InputType.Performed, OnJumpPerformed)
        );

        // Sprint button
        InputActions.Add(new InputData(
            InputManager.Instance.PControls.Player.Sprint, InputType.Performed, OnSprintPerformed)
        );
        InputActions.Add(new InputData(
            InputManager.Instance.PControls.Player.SprintToggle, InputType.Performed, OnSprintTogglePerformed)
        );
    }


    #region Input Functions

    private void OnSprintPerformed(InputAction.CallbackContext obj)
    {
        // Return if not currently sliding
        if (!_isSliding)
            return;

        // Force sprinting to be true
        ParentComponent.ForceSetSprinting(true);

        // Force the slide to end
        EndSlide();
    }

    private void OnSprintTogglePerformed(InputAction.CallbackContext obj)
    {
        // Return if not currently sliding
        if (!_isSliding)
            return;

        // Force sprinting to be true
        // ParentComponent.IsSprintToggled = true;
        ParentComponent.ForceSetSprinting(true);

        // Force the slide to end
        EndSlide();
    }

    private void OnSlidePerformedStart(InputAction.CallbackContext obj)
    {
        // Return if there was a slide input this frame
        if (_slideInputThisFrame)
            return;

        // If the player is already sliding, return
        if (_isSliding)
            return;

        // If this is not enabled, return
        if (!isEnabled)
            return;

        // // Force stop sprinting
        // ForceStopSprintingOnSlide(this);

        // Restart the midAir grace timer
        _midAirGraceTimer.SetMaxTimeAndReset(midAirGraceTime);
        _midAirGraceTimer.Start();

        // Set the flag to true
        _slideInputThisFrame = true;

        // In the basic player movement, reset the jump pre fire
        ParentComponent.BasicPlayerMovement.ResetJumpPreFire();
    }

    private void OnSlidePerformedStop(InputAction.CallbackContext obj)
    {
        // Return if there was a slide input this frame
        if (_slideInputThisFrame)
            return;

        // Debug.Log($"Slide Stopped!");

        // Set the flag to true
        _slideInputThisFrame = true;

        // End the slide
        EndSlide();
    }

    private void OnJumpPerformed(InputAction.CallbackContext obj)
    {
        // Force the slide to end
        if (_isSliding)
            EndSlide();

        // Call the on jump performed from the basic player movement script
        ParentComponent.BasicPlayerMovement.OnJumpPerformed(obj);
    }

    #endregion

    private void OnEnable()
    {
        // Register this script to the InputManager
        InputManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        // Unregister this script from the InputManager
        InputManager.Instance.Unregister(this);
    }

    private void Start()
    {
        // Initialize the events
        InitializeEvents();
    }

    private void InitializeEvents()
    {
        // Store the velocity when the player lands
        ParentComponent.OnLand += StoreVelocityOnLand;

        // Add the slide start events
        OnSlideStart += _ => _isSliding = true;
        OnSlideStart += SetWasSprintingOnSlide;
        OnSlideStart += PushControls;
        OnSlideStart += AddVelocityOnSlideStart;
        OnSlideStart += ChangeHeightOnSlide;
        // OnSlideStart += ForceStopSprintingOnSlide;

        // Add the slide end events
        OnSlideEnd += _ => _isSliding = false;
        OnSlideEnd += RemoveControls;
        OnSlideEnd += ChangeHeightOnSlide;
        OnSlideEnd += SetSprintingOnSlideEnd;
    }

    #region Slide Events

    private void ForceStopSprintingOnSlide(PlayerSlide obj)
    {
        // Force the player to stop sprinting
        ParentComponent.ForceSetSprinting(false);
    }

    private void ChangeHeightOnSlide(PlayerSlide obj)
    {
        ParentComponent.TargetPlayerHeight = _isSliding
            ? slideHeight
            : ParentComponent.DefaultPlayerHeight;
    }

    private void AddVelocityOnSlideStart(PlayerSlide obj)
    {
        // Determine which direction to slide in
        var velocityForward = ParentComponent.Rigidbody.velocity.normalized;
        var forwardMovement = velocityForward;

        if (ParentComponent.IsGrounded)
        {
            // Get the normal of the current surface
            var surfaceNormal = ParentComponent.GroundHit.normal;

            forwardMovement = Vector3.ProjectOnPlane(forwardMovement, surfaceNormal);
        }

        // Transfer some of that landing velocity to the slide if the flag is set
        var currentSlideStrength = slideStrength;
        if (_useLandVelocity)
        {
            var landTransferVelocity = Mathf.Abs(_landVelocity.y) * landVelocityTransferMultiplier;

            // Clamp the transfer velocity
            landTransferVelocity = Mathf.Clamp(landTransferVelocity, 0, landVelocityTransferMax);

            currentSlideStrength = landTransferVelocity + currentSlideStrength;
        }

        // Add the slide velocity to the player
        ParentComponent.Rigidbody.AddForce(forwardMovement * currentSlideStrength, ForceMode.VelocityChange);
    }

    private void StoreVelocityOnLand(Vector3 landVelocity)
    {
        _landVelocity = landVelocity;

        // Debug.Log($"Landed with velocity: {_landVelocity}");

        // Set the flag to use the land velocity
        _useLandVelocity = true;
    }

    private void SetWasSprintingOnSlide(PlayerSlide obj)
    {
        _wasSprintingBefore = ParentComponent.IsSprinting;
    }


    private void SetSprintingOnSlideEnd(PlayerSlide obj)
    {
        // If the player was sprinting before, force sprinting to be true
        if (_wasSprintingBefore)
            ParentComponent.ForceSetSprinting(true);

        // If the player was not sprinting before, but is NOW sprinting, force sprinting to be true
        else if (ParentComponent.IsSprinting)
            ParentComponent.ForceSetSprinting(true);
    }

    #endregion

    #endregion

    #region Update Functions

    private void Update()
    {
        // Update the midAir grace timer
        _midAirGraceTimer.SetMaxTime(midAirGraceTime);
        _midAirGraceTimer.Update(Time.deltaTime);
    }

    private void LateUpdate()
    {
        // Reset the slide input flag
        _slideInputThisFrame = false;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        // Detect if the player should slide
        DetectSlideStart();

        // Detect if the player should stop sliding
        DetectSlideEnd();

        // Reset the flag to use the land velocity
        _useLandVelocity = false;
    }

    private void DetectSlideStart()
    {
        // If this script is not enabled, return
        if (!isEnabled)
            return;

        // Return if the midAir grace timer is not active or is complete
        if (!_midAirGraceTimer.IsActive || !_midAirGraceTimer.IsNotComplete)
            return;

        // If the player is not grounded, return
        if (!ParentComponent.IsGrounded)
            return;

        // Return if the player is already sliding
        if (_isSliding)
            return;

        // Start the slide
        StartSlide();
    }

    private void DetectSlideEnd()
    {
        // Return if the player is not sliding
        if (!_isSliding)
            return;

        var belowThreshold = ParentComponent.Rigidbody.velocity.magnitude < slideStopVelocityThreshold;

        // If the player's current velocity is not less than the threshold, return
        if (ParentComponent.Rigidbody.velocity.magnitude >= slideStopVelocityThreshold)
            return;

        // End the slide
        EndSlide();
    }

    public override void FixedMovementUpdate()
    {
        // Update the slide movement
        UpdateSlideMovement();
    }

    private void UpdateSlideMovement()
    {
        // Get the lateral velocity
        var lateralVelocity = new Vector3(
            ParentComponent.Rigidbody.velocity.x,
            0,
            ParentComponent.Rigidbody.velocity.z
        );

        // Get the camera forward and right vectors
        var cameraForward = ParentComponent.CameraPivot.transform.forward;
        var cameraRight = ParentComponent.CameraPivot.transform.right;

        // Calculate the movement vector
        // var forwardInput = (cameraForward * Mathf.Min(0, ParentComponent.MovementInput.y));
        // var rightInput = (cameraRight * ParentComponent.MovementInput.x);
        // var forwardMovement = forwardInput + rightInput;
        var forwardMovement = lateralVelocity.normalized;

        if (ParentComponent.IsGrounded)
        {
            // Get the normal of the current surface
            var surfaceNormal = ParentComponent.GroundHit.normal;

            // Get the forward and right vectors based on the surface normal
            forwardMovement = Vector3.ProjectOnPlane(forwardMovement, surfaceNormal);
        }

        // Calculate the current movement
        var currentMovement = forwardMovement;

        // Normalize the current movement if the magnitude is greater than 1
        if (currentMovement.magnitude > 1)
            currentMovement.Normalize();

        // Calculate how fast the player should be moving
        // Calculate the velocity the rigidbody should be moving at
        var targetVelocity = new Vector3(0, ParentComponent.Rigidbody.velocity.y, 0);

        // Use the ground hit normal to determine if the player is sliding down a slope
        var slopeDirection = Vector3.Cross(
            ParentComponent.GroundHit.normal,
            Vector3.Cross(Vector3.down, ParentComponent.GroundHit.normal)
        ).normalized;

        var currentAcceleration = 1f;

        // If the cross product is pointing downward, the player is sliding on a slope
        if (slopeDirection.y < 0)
        {
            // Get the angle of the slope
            var slopeAngle = Vector3.Angle(Vector3.up, ParentComponent.GroundHit.normal);

            float slopeInverseLerp;
            float slopeSpeedMult;

            // Determine how the slope affects the player's speed based on the angle
            if (slopeAngle < minSlopeAccelerationAngle)
            {
                slopeInverseLerp = Mathf.InverseLerp(0, minSlopeAccelerationAngle, slopeAngle);
                slopeSpeedMult = Mathf.Lerp(0, 1, slopeInverseLerp);
            }
            else
            {
                slopeInverseLerp = Mathf.InverseLerp(minSlopeAccelerationAngle, maxSlopeAccelerationAngle, slopeAngle);
                slopeSpeedMult = Mathf.Lerp(1, maxSlopeSpeedMultiplier, slopeInverseLerp);
            }

            var targetSlopeSpeed = ParentComponent.MovementSpeed * slopeSpeedMult;

            // Recalculate the target velocity
            targetVelocity = slopeDirection * (targetSlopeSpeed) +
                             new Vector3(0, ParentComponent.Rigidbody.velocity.y, 0);

            currentAcceleration = slopeAcceleration;
        }

        // Determine if the new velocity is a slowdown
        var isSlowDown = Vector3.Dot(targetVelocity - ParentComponent.Rigidbody.velocity,
            ParentComponent.Rigidbody.velocity) < 0;

        if (isSlowDown)
            currentAcceleration = slideDeceleration;

        // Get the dot product between the target velocity and the current velocity
        var directionChangeDot = Vector3.Dot(ParentComponent.Rigidbody.velocity.normalized, targetVelocity.normalized);

        // Evaluate the direction change dot to get the acceleration factor
        var evaluation = ParentComponent.AccelerationFactorFromDot.Evaluate(directionChangeDot);

        // Debug.Log($"DOT: {directionChangeDot:0.00} | EVAL: {evaluation:0.00}");

        // This is the force required to reach the target velocity in EXACTLY one frame
        var targetForce = (targetVelocity - ParentComponent.Rigidbody.velocity) / Time.fixedDeltaTime;

        // Calculate the force that is going to be applied to the rigidbody THIS frame
        // var force = targetForce.normalized * (slideDeceleration * evaluation);
        var force = targetForce.normalized * (currentAcceleration * evaluation);
        // var force = targetForce.normalized * (1 * evaluation);

        // If the player only needs to move a little bit, just set the force to the target force
        if (targetForce.magnitude < slideDeceleration)
            force = targetForce;

        // If the force is greater than the target force, clamp it
        if (force.magnitude > targetForce.magnitude)
            force = targetForce;

        // TODO: Figure out slope movement

        // // Rotate the force based on the movement input (Rotate around the y-axis)
        // const float rotationAngle = 180f;
        // const float rotationSpeedThresholdMin = 3;
        // var rotationSpeedThresholdMax = ParentComponent.MovementSpeed * 2;
        //
        // var rotationDirection = Vector3.Dot(cameraForward, force) < 0 ? -1 : 1;
        // var speedLerpValue = Mathf.InverseLerp(
        //     rotationSpeedThresholdMin, rotationSpeedThresholdMax,
        //     ParentComponent.Rigidbody.velocity.magnitude
        // );
        //
        // force = Quaternion.AngleAxis(rotationAngle * rotationDirection, Vector3.up) * force;

        // Debug.Log(
        //     $"Target Vel: {targetVelocity:0.00} | " +
        //     $"Target Force: {targetForce:0.00} | " +
        //     $"Force: {force:0.00} | " +
        //     $"Direction Dot: {directionChangeDot:0.00} |" +
        //     $"Eval: {evaluation:0.00}");

        // Apply the force to the rigidbody
        ParentComponent.Rigidbody.AddForce(force, ForceMode.Acceleration);
    }

    #endregion

    private void StartSlide()
    {
        // If this script is not enabled, return
        if (!isEnabled)
            return;

        // Slide the player
        // Debug.Log($"Slide Starting! Use Land Velocity?: {_useLandVelocity}");

        // Reset the midAir grace timer
        _midAirGraceTimer.Stop();

        // Invoke the slide event
        OnSlideStart?.Invoke(this);

        //set the slide mesh to active
        _slideMesh.SetActive(true);

        // trigger slide animation
        if (lowerPlayerAnimator != null)
            lowerPlayerAnimator.SetBool(LowerSlidingAnimationID, IsSliding);
        if (upperPlayerAnimator != null)
            upperPlayerAnimator.SetBool(UpperSlidingAnimationID, IsSliding);
    }

    private void EndSlide()
    {
        if (!_isSliding)
            return;
        
        // Invoke the slide end event
        OnSlideEnd?.Invoke(this);

        //_slideMesh.SetActive(false);
        if (lowerPlayerAnimator != null)
            lowerPlayerAnimator.SetBool(LowerSlidingAnimationID, IsSliding);
        if (upperPlayerAnimator != null)
            upperPlayerAnimator.SetBool(UpperSlidingAnimationID, IsSliding);
    }

    public void ResetSlidePreFire()
    {
        // Reset the midAir grace timer
        _midAirGraceTimer.SetMaxTimeAndReset(midAirGraceTime);
        _midAirGraceTimer.Stop();
    }

    #region Debugging

    public override string GetDebugText()
    {
        return "";
    }

    #endregion
}