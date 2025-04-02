using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BasicPlayerMovement : PlayerMovementScript, IUsesInput, IDebugged
{
    #region Serialized Fields

    [SerializeField] private bool canSprintWithoutPower = true;
    [SerializeField] private bool canJumpWithoutPower = true;

    [SerializeField, Min(1)] private float sprintMultiplier = 1.5f;

    [Space, SerializeField, Min(0)] private float jumpForce = 10f;
    [SerializeField, Min(0)] private float jumpGraceTime = 0.5f;
    [SerializeField, Range(0, 1)] private float airMovementMultiplier = 0.5f;
    [SerializeField] private float variableJumpForce = 1f;
    [SerializeField, Min(0)] private float variableJumpTime = 1f;

    // variables to store the ground floor and jump height for calculating distance to ground
    [SerializeField] private float groundFloorDistance = 100f;
    [SerializeField] private float JumpHeightThreshold = 2f;

    [SerializeField, Min(0)] private float coyoteJumpTime = .5f; 
    
    [Header("Sounds")] [SerializeField] private SoundPool footstepSoundPool;

    [SerializeField] private float walkingFootstepInterval = 0.35f;
    [SerializeField] private float sprintingFootstepInterval = 0.25f;

    [SerializeField] private Sound jumpSound;

    #endregion

    #region Private Fields

    private static readonly int HasJumped = Animator.StringToHash("hasJumped");

    private Vector2 _movementInput;

    private readonly CountdownTimer _footstepTimer = new(0.5f, true, false);
    private float _timeSinceLastFootstep;

    private CountdownTimer _jumpGraceTimer;

    private bool _isJumpHeld;
    private CountdownTimer _variableJumpTimer;
    private bool _canJumpWhileReloading;

    private CountdownTimer _coyoteJumpTimer;

    #endregion

    #region Getters

    public HashSet<InputData> InputActions { get; } = new();

    public override InputActionMap InputActionMap => InputManager.Instance.PControls.PlayerMovementBasic;

    public Vector2 MovementInput => _movementInput;

    // public bool CanSprint => canSprintWithoutPower && ParentComponent.IsGrounded;
    public bool CanSprint
    {
        get => canSprintWithoutPower;
        set => canSprintWithoutPower = value;
    }

    public bool IsSprinting => ParentComponent.IsSprinting;

    public float SprintMultiplier => sprintMultiplier;

    public bool IsTryingToJump { get; set; }

    public bool IsSetToJump => _jumpGraceTimer.IsActive && !_jumpGraceTimer.IsComplete;

    public bool CanJump
    {
        get => canJumpWithoutPower;
        set => canJumpWithoutPower = value;
    }

    private Animator PlayerAnimator => ParentComponent.PlayerAnimator;

    #endregion

    #region Initialization Functions

    protected override void CustomAwake()
    {
        // Initialize the controls
        InitializeInput();

        // Initialize the jump grace timer
        _jumpGraceTimer = new CountdownTimer(jumpGraceTime, false, true);
        _jumpGraceTimer.OnTimerEnd += () => _jumpGraceTimer.Stop();
        _jumpGraceTimer.Stop();

        // Initialize the variable jump timer
        _variableJumpTimer = new CountdownTimer(variableJumpTime, false, true);
        _variableJumpTimer.Start();
        
        // Initialize the coyote jump timer
        _coyoteJumpTimer = new CountdownTimer(coyoteJumpTime, false, true);
        _coyoteJumpTimer.Start();
    }

    private void Start()
    {
        // Initialize the footstep sounds
        InitializeFootsteps();

        // Add this script to the debug manager
        DebugManager.Instance.AddDebuggedObject(this);
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

        _movementInput = Vector2.zero;
    }

    private void OnDestroy()
    {
        // Remove this script from the debug manager
        DebugManager.Instance.RemoveDebuggedObject(this);
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
        InputActions.Add(new InputData(
            InputManager.Instance.PControls.PlayerMovementBasic.Jump, InputType.Canceled, SetJumpOnJumpCanceled)
        );
    }

    private void SetJumpOnJumpCanceled(InputAction.CallbackContext obj)
    {
        // Set the is jump held flag to false
        _isJumpHeld = false;
    }

    private void InitializeFootsteps()
    {
        // Set up the footstep timer
        _footstepTimer.OnTimerEnd += PlayFootstepSound;
    }

    private void PlayFootstepSound()
    {
        // Return if the sound manager's instance is null
        if (SoundManager.Instance == null)
            return;
        
        // Play the footstep sound
        SoundManager.Instance.PlaySfx(footstepSoundPool.GetRandomSound());

        // Reset the timer
        _footstepTimer.Reset();

        // Reset the time since the last footstep
        _timeSinceLastFootstep = 0;
    }

    #endregion

    #region Input Methods

    private void OnMovePerformed(InputAction.CallbackContext obj)
    {
        // Get the movement input
        _movementInput = obj.ReadValue<Vector2>();

        // // set walking true in animator
        // if (PlayerAnimator != null)
        //     PlayerAnimator?.SetBool("Walking", true);

        // Reset the parent component's isSprintToggled flag if there is no forward input
        if (_movementInput == Vector2.zero)
            ParentComponent.ForceSetSprinting(false);
    }

    private void OnMoveCanceled(InputAction.CallbackContext obj)
    {
        // Reset the movement input
        _movementInput = Vector2.zero;

        if (PlayerAnimator != null)
            PlayerAnimator?.SetBool("Walking", false);
    }


    public void OnJumpPerformed(InputAction.CallbackContext obj)
    {
        // Return if the player cannot jump
        if (!canJumpWithoutPower)
            return;

        // Return if this is not the active movement script
        if (ParentComponent.CurrentMovementScript != this)
            return;

        // Restart the jump grace timer
        _jumpGraceTimer.SetMaxTimeAndReset(jumpGraceTime);
        _jumpGraceTimer.Start();

        // Reset the slide pre fire of the slide script
        ParentComponent.PlayerSlide.ResetSlidePreFire();
    }

    #endregion

    #region Update Functions

    private void Update()
    {
        // Update the jump grace timer
        _jumpGraceTimer.SetMaxTime(jumpGraceTime);
        _jumpGraceTimer.Update(Time.deltaTime);

        // Update the variable jump timer
        _variableJumpTimer.SetMaxTime(variableJumpTime);
        _variableJumpTimer.Update(Time.deltaTime);

        // Update the coyote jump timer
        _coyoteJumpTimer.SetMaxTime(coyoteJumpTime);
        if (ParentComponent.IsGrounded)
            _coyoteJumpTimer.Reset();
        else
            _coyoteJumpTimer.Update(Time.deltaTime);
        
        // Set the is trying to jump flag to false if the player is falling
        if (ParentComponent.Rigidbody.velocity.y < 0)
            IsTryingToJump = false;

        // Update the footstep sounds
        UpdateFootsteps();

        //check if the player is on the ground
        var heightAboveGround = GetHeightAboveGround();
        PlayerAnimator?.SetFloat("JumpHeight", heightAboveGround);

        // TODO: REMOVE LATER - FLIGHT DEBUG
        if (Input.GetKey(KeyCode.F))
            Flight();
    }

    private void Flight()
    {
        // Get the camera's forward without the y component
        var cameraForward = ParentComponent.CameraPivot.transform.forward;

        // Stop the velocity
        ParentComponent.Rigidbody.velocity = Vector3.zero;

        const float flySpeed = 50;
        
        // Move the player forward
        ParentComponent.Rigidbody.position += cameraForward * (Time.deltaTime * flySpeed);
    }

    private float GetHeightAboveGround()
    {
        //raycast to the ground
        RaycastHit hit;
        if (Physics.Raycast(ParentComponent.transform.position, Vector3.down, out hit, groundFloorDistance))
        {
            //get the distance to the ground
            float distanceToGround = hit.distance;
            return distanceToGround;
        }

        return 0;
    }

    private void UpdateFootsteps()
    {
        // Set the footstep timer's max time based on the player's walking/sprinting state
        _footstepTimer.SetMaxTime(walkingFootstepInterval);

        // Update the time since the last footstep
        _timeSinceLastFootstep += Time.deltaTime;

        // If this is NOT the active movement script, disable the footstep timer
        if (ParentComponent.CurrentMovementScript != this)
            _footstepTimer.SetActive(false);

        // If the player is not grounded, disable the footstep timer
        if (!ParentComponent.IsGrounded)
            _footstepTimer.SetActive(false);

        var sprintingMultiplier = IsSprinting ? walkingFootstepInterval / sprintingFootstepInterval : 1;
        var moveMagnitude = _movementInput.magnitude;

        // Update the footstep timer
        _footstepTimer.Update(Time.deltaTime * sprintingMultiplier * moveMagnitude);
    }

    public override void FixedMovementUpdate()
    {
        // Update the sprinting state for toggled sprinting
        if (MovementInput == Vector2.zero || !CanSprint)
            ParentComponent.IsSprintToggled = false;

        // Update the movement
        UpdateLateralMovement();

        // Apply the lateral speed limit
        ApplyLateralSpeedLimit();

        // Update the jump
        UpdateJump();

        if (InputManager.Instance.PControls.PlayerMovementBasic.Jump.ReadValue<float>() < 0)
            _isJumpHeld = false;

        // Apply the variable jump force
        if (_isJumpHeld && !_variableJumpTimer.IsComplete)
        {
            var gravity = -Physics.gravity;
            var gravityMagnitude = gravity.magnitude;

            ParentComponent.Rigidbody.AddForce(
                gravity.normalized * (gravityMagnitude + variableJumpForce),
                ForceMode.Acceleration
            );

            // ParentComponent.Rigidbody.AddForce(
            //     Vector3.up * variableJumpForce,
            //     ForceMode.VelocityChange
            // );
        }
    }

    private void UpdateLateralMovement()
    {
        // Return if the movement input is zero
        // Stop the footstep timer to prevent footstep sounds
        if (_movementInput.magnitude <= 0)
            _footstepTimer.SetActive(false);

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

            // var rightNormal = Vector3.Cross(surfaceNormal, cameraForward);
            // var forwardNormal = Vector3.Cross(surfaceNormal, cameraRight);
            //
            // forwardMovement = forwardNormal.normalized * forwardMovement.magnitude;
            // rightMovement = rightNormal.normalized * rightMovement.magnitude;
            //
            // if (_movementInput.y > 0)
            //     forwardMovement = -forwardMovement;
            // if (_movementInput.x < 0)
            //     rightMovement = -rightMovement;

            // Debug.Log(
            //     $"DOT: " +
            //     $"{Vector3.Dot(rightNormal.normalized, rightMovement.normalized):0.00000000} - " +
            //     $"{rightNormal.normalized} " +
            //     $"{rightMovement.normalized}");
        }

        // Get the angle of the forward movement
        var angle = Vector3.Angle(Vector3.up, forwardMovement);


        // Calculate the current movement
        var currentMovement = forwardMovement + rightMovement;

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
        if (!_footstepTimer.IsActive && _movementInput.magnitude > 0)
        {
            _footstepTimer.SetActive(true);

            // If it has been a while since the last footstep, force the footstep timer to complete
            if (_timeSinceLastFootstep > 0.5f)
                _footstepTimer.ForcePercent(1);
        }
    }

    private void ApplyLateralSpeedLimit()
    {
        var vel = ParentComponent.Rigidbody.velocity;

        // Get the lateral velocity of the player
        var lateralVelocity = new Vector3(vel.x, 0, vel.z);

        // Get the speed limit
        var speedLimit = ParentComponent.HardSpeedLimit;

        // If the lateral velocity is less than the speed limit, return
        if (lateralVelocity.magnitude <= speedLimit)
            return;

        // Get the normalized lateral velocity
        var normalizedLateralVelocity = lateralVelocity.normalized;

        var lerpAmount = ParentComponent.HardSpeedLimitLerpAmount;

        // Calculate the new velocity
        var newVelocity =
            Vector3.Lerp(lateralVelocity, normalizedLateralVelocity * speedLimit,
                CustomFunctions.FrameAmount(lerpAmount, true));

        // Apply the new velocity
        ParentComponent.Rigidbody.velocity = new Vector3(newVelocity.x, vel.y, newVelocity.z);
    }

    private void UpdateJump()
    {
        if (!canJumpWithoutPower)
            return;

        // Return if the player is not grounded
        // AND if the player is NOT (not grounded and the coyote jump timer is still running)
        if (!ParentComponent.IsGrounded && !(!ParentComponent.IsGrounded && _coyoteJumpTimer.IsNotComplete))
            return;

        // Return if the jump grace timer is not active or is complete
        if (!_jumpGraceTimer.IsActive || _jumpGraceTimer.IsComplete)
            return;

        if (!ParentComponent.IsGrounded && _coyoteJumpTimer.IsNotComplete)
            Debug.Log($"COYOTE JUMP: {_coyoteJumpTimer.Percentage:0.00}");

        // Jump in the direction of the movement input
        PlayerAnimator?.SetBool(HasJumped, false);
        Jump(_movementInput);
    }

    private void Jump(Vector2 movementInput)
    {
        // Normalize the movement input
        movementInput = movementInput.normalized;

        var jumpDirection = ParentComponent.transform.up.normalized;

        // Kill the vertical velocity
        ParentComponent.Rigidbody.velocity =
            new Vector3(ParentComponent.Rigidbody.velocity.x, 0, ParentComponent.Rigidbody.velocity.z);

        ParentComponent.Rigidbody.AddForce(jumpDirection * jumpForce, ForceMode.VelocityChange);

        // Play the jump sound
        SoundManager.Instance?.PlaySfx(jumpSound);
        // player_Animator?.SetBool(HasJumped, true);

        // Set the is trying to jump flag to true
        IsTryingToJump = true;

        // Reset the jump grace timer
        _jumpGraceTimer.SetMaxTimeAndReset(jumpGraceTime);
        _jumpGraceTimer.Stop();

        // Set the is jump held flag to true
        _isJumpHeld = true;

        // Reset the variable jump timer
        _variableJumpTimer.SetMaxTimeAndReset(variableJumpTime);
        
        // Force the coyote jump timer to be complete 
        _coyoteJumpTimer.ForcePercent(1);
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

    public void ResetJumpPreFire()
    {
        // Reset the jump grace timer
        _jumpGraceTimer.SetMaxTimeAndReset(jumpGraceTime);
        _jumpGraceTimer.Stop();
    }

    #region Debug

    public override string GetDebugText()
    {
        return
            $"\tInput   : {_movementInput}\n" +
            $"\tJump Timer: {_jumpGraceTimer.Percentage:0.00}\n" +
            $"\tTrying to Jump: {IsTryingToJump}";
    }

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