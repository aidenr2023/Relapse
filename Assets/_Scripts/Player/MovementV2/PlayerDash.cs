using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDash : PlayerMovementScript, IDashScript, IUsesInput
{
    private const float DEFAULT_FIXED_DELTA_TIME = 0.02f;

    #region Serialized Fields

    [SerializeField] private bool isEnabled = true;

    [SerializeField] [Min(0)] private float dashSpeed = 10f;

    // [SerializeField] [Min(0)] private float dashExitVelocity;

    [SerializeField] private CountdownTimer dashDuration = new(.25f, false, true);
    [SerializeField] private CountdownTimer dashCooldown = new(.5f, false, true);

    [SerializeField] [Min(0)] private int maxDashesInAir = 2;

    [Header("Sounds")] [SerializeField] private Sound dashSound;

    #endregion

    #region Private Fields

    private int _remainingDashesInAir;

    private Vector3 _dashDirection;

    private Vector3 _previousVelocity;

    public HashSet<InputData> InputActions { get; } = new();

    #endregion

    #region Getters

    public override InputActionMap InputActionMap => null;

    public bool IsDashing => dashDuration.IsNotComplete;

    public float DashDuration => dashDuration.MaxTime;

    #endregion

    public event Action<IDashScript> OnDashStart;
    public event Action<IDashScript> OnDashEnd;


    protected override void CustomAwake()
    {
        // Initialize the input
        InitializeInput();
    }

    private void Start()
    {
        // Initialize the events
        InitializeEvents();
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

    public void InitializeInput()
    {
        // Initialize the event that is called when the button is pressed
        InputActions.Add(new InputData(
            InputManager.Instance.PControls.PlayerMovementBasic.Dash, InputType.Performed, OnDashPerformed)
        );
    }

    private void InitializeEvents()
    {
        OnDashStart += _ => PushControls(this);
        OnDashStart += StartDash;
        OnDashStart += _ => SoundManager.Instance.PlaySfx(dashSound);

        OnDashEnd += _ => RemoveControls(this);
        OnDashEnd += EndDash;

        // Set up the countdown events
        dashDuration.OnTimerEnd += () => OnDashEnd?.Invoke(this);
    }

    #region Event Functions

    private void OnDashPerformed(InputAction.CallbackContext obj)
    {
        // Return if the dash is not enabled
        if (!isEnabled)
            return;

        // If the cooldown is ticking,
        // Or the dash duration is ticking,
        // return
        if (dashCooldown.IsNotComplete || dashDuration.IsNotComplete)
            return;

        // Return if the player is in air and has no remaining dashes
        if (
            !(ParentComponent.IsGrounded)
            && _remainingDashesInAir <= 0
        )
            return;

        OnDashStart?.Invoke(this);
    }

    private void StartDash(IDashScript obj)
    {
        // Reset & start the dash duration
        dashDuration.Reset();
        dashDuration.SetActive(true);

        // Store the player's velocity
        _previousVelocity = ParentComponent.Rigidbody.velocity;

        // Kill the player's velocity
        ParentComponent.Rigidbody.velocity = Vector3.zero;

        // Get the dash direction (ignoring the y)
        var dashInput = ParentComponent.MovementInput;
        if (dashInput == Vector2.zero)
            dashInput = Vector2.up;

        var forwardMovement =
            ParentComponent.CameraPivot.transform.forward *
            dashInput.y;

        var rightMovement =
            ParentComponent.CameraPivot.transform.right *
            dashInput.x;

        _dashDirection = forwardMovement + rightMovement;
        _dashDirection.y = 0;
        _dashDirection = _dashDirection.normalized;


        // If the player is not grounded, decrement the remaining dashes in air
        if (!ParentComponent.IsGrounded)
            _remainingDashesInAir--;
    }

    private void EndDash(IDashScript obj)
    {
        // Reset & start the dash cooldown
        dashCooldown.Reset();
        dashCooldown.SetActive(true);

        // // Kill the player's y velocity
        // ParentComponent.Rigidbody.velocity = new Vector3(
        //     ParentComponent.Rigidbody.velocity.x,
        //     0,
        //     ParentComponent.Rigidbody.velocity.z
        // );

        // ParentComponent.Rigidbody.velocity = _dashDirection * dashExitVelocity;
        ParentComponent.Rigidbody.velocity = _dashDirection * _previousVelocity.magnitude;

    }

    #endregion

    private void Update()
    {
        // Check if the player is on the ground / wall running to refill the dashes
        if (ParentComponent.IsGrounded || ParentComponent.WallRunning.IsWallRunning || ParentComponent.WallRunning.IsWallSliding)
            _remainingDashesInAir = maxDashesInAir;

        // Update the timers
        dashDuration.Update(Time.deltaTime);
        dashCooldown.Update(Time.deltaTime);

        // Make the physics more accurate when dashing to prevent clipping
        if (IsDashing)
            Time.fixedDeltaTime = DEFAULT_FIXED_DELTA_TIME / 8F;
        else
            Time.fixedDeltaTime = DEFAULT_FIXED_DELTA_TIME;

        // Debug.Log($"Fixed Delta Time: {Time.fixedDeltaTime}");
    }

    public override void FixedMovementUpdate()
    {
        if (!IsDashing)
            return;

        // Calculate the target velocity
        Vector3 targetVelocity = default;

        // If the player is not grounded, set the target velocity to the dash direction
        if (!ParentComponent.IsGrounded)
            targetVelocity = _dashDirection * dashSpeed;

        // If the player IS grounded, calculate the target velocity based on the dash direction's relation to the surface normal
        else
        {
            // Get the surface normal
            var surfaceNormal = ParentComponent.GroundHit.normal;

            // Calculate the target velocity based on the surface normal
            targetVelocity = Vector3.ProjectOnPlane(_dashDirection, surfaceNormal).normalized * dashSpeed;
        }

        _tmpDashVelocity = targetVelocity;

        // This is the force required to reach the target velocity in EXACTLY one frame
        var targetForce = (targetVelocity - ParentComponent.Rigidbody.velocity) / Time.fixedDeltaTime;

        // Apply the force
        ParentComponent.Rigidbody.AddForce(targetForce, ForceMode.Acceleration);
    }

    private Vector3 _tmpDashVelocity;

    public override string GetDebugText()
    {
        return $"Dash Duration: {dashDuration.TimeLeft}\n" +
               $"Dash Cooldown: {dashCooldown.TimeLeft}\n" +
               $"Remaining Dashes: {_remainingDashesInAir}";
    }

    private void OnDrawGizmos()
    {
        const float interval = 0.5f;

        Gizmos.color = Color.red;
        for (int i = 0; i < 20; i++)
            Gizmos.DrawSphere(transform.position + _tmpDashVelocity.normalized * (i * interval), 0.125f);
    }
}