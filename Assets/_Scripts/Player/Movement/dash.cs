using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Dash : MonoBehaviour, IDashScript
{
    [SerializeField] [Range(0, 2000)] private float dashForce = 100f; // The strength of the dash impulse
    [SerializeField] [Range(0, 10)] private float dashCooldown = 1f; // Cooldown time before the player can dash again

    // Reference to the player
    private Player _player;

    private WallRunning _wallRunning;

    private Rigidbody _rb; // Reference to the player's Rigidbody
    private PlayerControls _playerInputActions; // Reference to the Input System controls

    [SerializeField] [Min(0)] private int maxDashesInAir = 1;
    private int _remainingDashesInAir = 0;

    /// <summary>
    /// An external flag to determine if the player can dash.
    /// This flag is supposed to be controlled by the movement script, not this.
    /// NOT to be used by itself for the sake of dashing.
    /// Use the CanDash property instead.
    /// </summary>
    private bool _externalDashFlag = true;

    /// <summary>
    /// A flag to determine if the player is currently in the dash cooldown period.
    /// </summary>
    private bool _isDashCooldown;

    /// <summary>
    /// A flag to determine if the player is currently dashing.
    /// </summary>
    private bool _isDashing;

    /// <summary>
    /// The player is allowed to dash if:
    /// The external dash flag is set AND
    /// the dash cooldown flag is NOT active AND
    /// the player is either on the ground OR is in the air with remaining dashes
    /// </summary>
    private bool CanDash =>
        _externalDashFlag &&
        !_isDashCooldown &&
        (
            (_remainingDashesInAir > 0 && !_player.PlayerController.IsGrounded) ||
            _player.PlayerController.IsGrounded
        );

    public bool IsDashing => _isDashing;

    public float DashDuration => .05f;

    #region Events

    public event Action<IDashScript> OnDashStart;
    public event Action<IDashScript> OnDashEnd;

    #endregion

    private void Awake()
    {
        // Initialize the components
        InitializeComponents();

        // Initialize the Input System controls
        _playerInputActions = new PlayerControls();
    }

    private void InitializeComponents()
    {
        // Get the player component
        _player = GetComponent<Player>();

        // Get the Rigidbody component
        _rb = GetComponent<Rigidbody>();

        // Get the WallRunning component
        _wallRunning = GetComponent<WallRunning>();
    }

    private void OnEnable()
    {
        _playerInputActions.GamePlay.dash.performed += OnDashPerformed;
        _playerInputActions.Enable();
    }

    private void OnDisable()
    {
        _playerInputActions.GamePlay.dash.performed -= OnDashPerformed;
        _playerInputActions.Disable();
    }

    private void Update()
    {
        UpdateAirDashCount();
    }

    private void FixedUpdate()
    {
        ClampVerticalVelocity();
    }

    private void UpdateAirDashCount()
    {
        // Detect if the player is grounded using the controller

        // If the player is grounded, reset the dash count
        // If the player is wall running, reset the dash count
        if (
            _player.PlayerController.IsGrounded ||
            _wallRunning != null && _wallRunning.IsWallRunning
        )
            _remainingDashesInAir = maxDashesInAir;
    }

    private void OnDashPerformed(InputAction.CallbackContext context)
    {
        // Return if the player cannot dash
        if (!CanDash)
            return;

        var dashDirection = GetDashDirection();

        // Set the velocity on the Y-axis to 0
        _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
        ClampVerticalVelocity();

        // Apply dash impulse
        _rb.AddForce(dashDirection * dashForce, ForceMode.Impulse);

        // Start the dash cooldown coroutine
        StartCoroutine(DashCooldownCoroutine());

        // If the player is in the air, decrement the remaining dashes
        if (!_player.PlayerController.IsGrounded)
            _remainingDashesInAir--;

        // Run the OnDash event
        OnDashStart?.Invoke(this);
    }

    private void ClampVerticalVelocity()
    {
        // Limit how much vertical velocity (Y-axis) the player can have
        if (_rb.velocity.y > 5f) // Adjust this value to control vertical speed when dashing
            _rb.velocity = new Vector3(_rb.velocity.x, 5f, _rb.velocity.z);
    }

    private Vector3 GetDashDirection()
    {
        // var currentVelocity = _rb.velocity;
        // var horizontalVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
        //
        // // If the player is stationary, default to forward
        // if (horizontalVelocity.magnitude == 0)
        //     return transform.forward;
        //
        // return horizontalVelocity.normalized;

        // Get the movement input from the controller
        var movementInput = _player.PlayerController.MovementInput;

        // Get the camera pivot's transform
        var playerTransform = _player.PlayerController.CameraPivot.transform;

        Vector3 newForward;

        // If the movement input is not zero,
        // return a new Vector3 with the movement input
        // relative to the player's transform
        if (movementInput != Vector2.zero)
        {
            newForward =
            (
                playerTransform.right * movementInput.x +
                playerTransform.forward * movementInput.y
            ).normalized;
        }
        else
        {
            // If the movement input is zero, return the player's forward direction
            newForward = playerTransform.forward;
        }

        // Eliminate the Y-axis component
        return new Vector3(newForward.x, 0, newForward.z);
    }


    private IEnumerator DashCooldownCoroutine()
    {
        // Set the dash cooldown flag to true
        _isDashCooldown = true;

        yield return new WaitForSeconds(dashCooldown);

        // Set the dash cooldown flag to false
        _isDashCooldown = false;
    }

    public void SetExternalDashFlag(bool canDash)
    {
        _externalDashFlag = canDash;
    }
}