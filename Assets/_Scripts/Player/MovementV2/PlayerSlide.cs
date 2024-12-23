using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSlide : PlayerMovementScript, IDebugged, IUsesInput
{
    #region Serialized Fields

    [SerializeField] private bool isEnabled = true;

    [SerializeField] private float slideStrength = 10f;

    [Tooltip("How long before landing the player can still slide")] [SerializeField, Min(0)]
    private float midAirGraceTime = 0.5f;

    [SerializeField, Min(0)] private float slideDeceleration = 1f;
    [SerializeField, Min(0)] private float slideStopVelocityThreshold = 4f;

    [Header("Transfer Velocity when Landing into A Slide")] [SerializeField, Range(0, 1)]
    private float landVelocityTransferMultiplier = 1f;

    [SerializeField, Min(0)] private float landVelocityTransferMax = 20f;

    #endregion

    #region Private Fields

    private bool _isSliding;

    private CountdownTimer _midAirGraceTimer;

    private Vector3 _landVelocity;

    private bool _useLandVelocity;

    private bool _slideInputThisFrame;

    #endregion

    public event Action<PlayerSlide> OnSlideStart;
    public event Action<PlayerSlide> OnSlideEnd;

    #region Getters

    public override InputActionMap InputActionMap => null;

    public HashSet<InputData> InputActions { get; } = new();

    #endregion

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
        InputActions.Add(new InputData(
            InputManager.Instance.PControls.PlayerMovementBasic.Slide, InputType.Performed, OnSlidePerformedStart)
        );
        InputActions.Add(new InputData(
            InputManager.Instance.PControls.PlayerMovementBasic.Slide, InputType.Performed, OnSlidePerformedStop)
        );

        InputActions.Add(new InputData(
            InputManager.Instance.PControls.PlayerMovementBasic.Jump, InputType.Performed, OnJumpPerformed)
        );
    }

    #region Input Functions

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

        // Restart the midAir grace timer
        _midAirGraceTimer.SetMaxTimeAndReset(midAirGraceTime);
        _midAirGraceTimer.Start();

        // Set the flag to true
        _slideInputThisFrame = true;
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

        // Add this to the debug manager
        DebugManager.Instance.AddDebuggedObject(this);
    }

    private void InitializeEvents()
    {
        // Store the velocity when the player lands
        ParentComponent.OnLand += StoreVelocityOnLand;

        // Add the slide start events
        OnSlideStart += _ => _isSliding = true;
        OnSlideStart += PushControls;
        OnSlideStart += AddVelocityOnSlideStart;

        // Add the slide end events
        OnSlideEnd += RemoveControls;
        OnSlideEnd += _ => _isSliding = false;
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

        // Return if the midAir grace timer is not active
        if (!_midAirGraceTimer.IsActive)
            return;

        // Return if the midAir grace timer is complete
        if (!_midAirGraceTimer.IsNotComplete)
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

    private void EndSlide()
    {
        // Invoke the slide end event
        OnSlideEnd?.Invoke(this);
    }

    public override void FixedMovementUpdate()
    {
        // Update the slide movement
        UpdateSlideMovement();
    }

    private void UpdateSlideMovement()
    {
        // var cameraTransform = ParentComponent.Orientation;

        // // Get the camera's forward without the y component
        // var cameraForward = new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z).normalized;
        //
        // // Get the camera's right without the y component
        // var cameraRight = new Vector3(cameraTransform.right.x, 0, cameraTransform.right.z).normalized;

        var velocityForward = ParentComponent.Rigidbody.velocity.normalized;

        // Get the current move multiplier
        // var currentMoveMult = IsSprinting ? sprintMultiplier : 1;
        var currentMoveMult = 0;

        // Calculate the movement vector
        // var forwardMovement = (cameraForward * _movementInput.y);
        // var rightMovement = (cameraRight * _movementInput.x);
        var forwardMovement = velocityForward;

        if (ParentComponent.IsGrounded)
        {
            // Get the normal of the current surface
            var surfaceNormal = ParentComponent.GroundHit.normal;

            // Get the forward and right vectors based on the surface normal
            forwardMovement = Vector3.ProjectOnPlane(forwardMovement, surfaceNormal);
            // rightMovement = Vector3.ProjectOnPlane(rightMovement, surfaceNormal);
        }

        // Calculate the current movement
        var currentMovement = forwardMovement;

        // Normalize the current movement if the magnitude is greater than 1
        if (currentMovement.magnitude > 1)
            currentMovement.Normalize();

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
        var force = targetForce.normalized * (slideDeceleration * evaluation);

        // // If the player is midair, apply the air movement multiplier
        // if (!ParentComponent.IsGrounded)
        //     force *= airMovementMultiplier;

        // If the player only needs to move a little bit, just set the force to the target force
        if (targetForce.magnitude < slideDeceleration)
            force = targetForce;

        // If the force is greater than the target force, clamp it
        if (force.magnitude > targetForce.magnitude)
            force = targetForce;

        // Apply the force to the rigidbody
        ParentComponent.Rigidbody.AddForce(force, ForceMode.Acceleration);
    }

    private void Update()
    {
        // Update the midAir grace timer
        _midAirGraceTimer.SetMaxTime(midAirGraceTime);
        _midAirGraceTimer.Update(Time.deltaTime);
    }


    public override string GetDebugText()
    {
        return "";
    }
}