using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDash : PlayerMovementScript, IDashScript, IUsesInput
{
    #region Serialized Fields

    [SerializeField] private bool isEnabled = true;

    [SerializeField] [Min(0)] private float dashSpeed = 10f;
    [SerializeField] private CountdownTimer dashDuration = new(.25f, false, true);
    [SerializeField] private CountdownTimer dashCooldown = new(.5f, false, true);

    [SerializeField] [Min(0)] private int maxDashesInAir = 2;

    [Header("Sounds")] [SerializeField] private Sound dashSound;

    #endregion

    private int _remainingDashesInAir;

    private Vector3 _dashDirection;

    public event Action<IDashScript> OnDashStart;
    public event Action<IDashScript> OnDashEnd;

    public HashSet<InputData> InputActions { get; } = new();

    public override InputActionMap InputActionMap => null;

    private bool IsDashing => dashDuration.IsTicking;

    public float DashDuration => dashDuration.MaxTime;


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
            InputManager.Instance.pControls.PlayerMovementBasic.Dash, InputType.Performed, OnDashPerformed)
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
        if (dashCooldown.IsTicking || dashDuration.IsTicking)
            return;

        // Return if the player is in air and has no remaining dashes
        if (
            !(ParentComponent.IsGrounded || ParentComponent.WallRunning.IsWallRunning)
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

        // Kill the player's y velocity
        ParentComponent.Rigidbody.velocity = new Vector3(
            ParentComponent.Rigidbody.velocity.x,
            0,
            ParentComponent.Rigidbody.velocity.z
        );
    }

    #endregion

    private void Update()
    {
        // Check if the player is on the ground to refill the dashes
        if (ParentComponent.IsGrounded || ParentComponent.WallRunning.IsWallRunning)
            _remainingDashesInAir = maxDashesInAir;

        // Update the timers
        dashDuration.Update(Time.deltaTime);
        dashCooldown.Update(Time.deltaTime);
    }

    public override void FixedMovementUpdate()
    {
        // Move the player if they are dashing
        if (IsDashing)
        {
            ParentComponent.Rigidbody.velocity = _dashDirection * dashSpeed;

            // Reset the y velocity
            ParentComponent.Rigidbody.velocity = new Vector3(
                ParentComponent.Rigidbody.velocity.x,
                0,
                ParentComponent.Rigidbody.velocity.z
            );
        }
    }

    public override string GetDebugText()
    {
        return $"Dash Duration: {dashDuration.TimeLeft}\n" +
               $"Dash Cooldown: {dashCooldown.TimeLeft}\n" +
               $"Remaining Dashes: {_remainingDashesInAir}";
    }
}