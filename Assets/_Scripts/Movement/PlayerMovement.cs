using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour, IPlayerController
{
    public enum MovementState
    {
        Walking,
        Sprinting,
        WallRunning,
        Air,
        Climbing,
    }

    #region Fields

    // Direction of the player's movement
    private Vector3 _moveDirection;

    // The current movement state of the player
    private MovementState _movementState;

    // Reference to the player's camera pivot
    [SerializeField] private Transform cameraPivot;

    // Reference to the player's orientation transform
    [SerializeField] private Transform orientation;

    // The speed when the player is walking
    [Header("Movement")] [SerializeField] [Range(0, 100)] private float walkSpeed;

    // The speed when the player is sprinting
    [SerializeField] [Range(0, 100)] private float sprintSpeed;

    // The speed when the player is wall running
    [SerializeField] [Range(0, 100)] private float wallrunSpeed;

    // Drag applied to the player when grounded
    [SerializeField] [Range(0, 10)] private float groundDrag;

    // The current speed of the player
    private float _moveSpeed;

    // The force applied when the player jumps
    [SerializeField] [Range(0, 10)] private float jumpForce;

    // The cooldown time between jumps
    [SerializeField] [Range(0, 10)] private float jumpCooldown;

    // Multiplier for movement speed when in the air
    [SerializeField] [Range(0, 10)] private float airMultiplier;

    // Flag to check if the player is ready to jump
    private bool _readyToJump = true;

    [Header("Sound")] [SerializeField] private AudioSource footsteps;
    [SerializeField] private AudioSource wallFootSteps;

    [Header("Movement References")]
    // // Reference to the Climbing script (if applicable)
    // public Climbing cm; 

    // Reference to the WallRunning script
    [SerializeField]
    private WallRunning wallRunning;

    [SerializeField] private Dash dash;

    [Header("Ground Check")] [SerializeField]
    private float playerHeight; // Height of the player for ground checking

    [SerializeField] private LayerMask whatIsGround; // LayerMask to define what is considered ground
    private bool _isGrounded; // Flag to check if the player is grounded

    private Rigidbody _rb; // Reference to the player's Rigidbody component

    // Flag to check if the player is climbing
    private bool _isClimbing;

    // Horizontal input value
    private float _horizontalInput;

    // Vertical input value
    private float _verticalInput;

    // A flag to check if the player is sprinting
    private bool _isSprinting;

    private PlayerLook _playerCam;

    #endregion

    #region Getters

    public GameObject CameraPivot => cameraPivot.gameObject;

    public bool IsGrounded => _isGrounded;

    public bool IsWallRunning => wallRunning.IsWallRunning;

    public bool IsClimbing { get; set; }

    #endregion

    #region Input System Rework

    private void InitializeInput()
    {
        // Connect the input actions to the corresponding functions

        // Initialize the movement input
        InputManager.Instance.PlayerControls.GamePlay.Movement.performed += OnMovePerformed;
        InputManager.Instance.PlayerControls.GamePlay.Movement.canceled += OnMoveCanceled;

        // Initialize the jump input
        InputManager.Instance.PlayerControls.GamePlay.jump.performed += OnJumpPerformed;

        // Initialize the sprint input
        InputManager.Instance.PlayerControls.GamePlay.Sprint.performed += OnSprintPerformed;
        InputManager.Instance.PlayerControls.GamePlay.Sprint.canceled += OnSprintCanceled;
    }

    private void OnJumpPerformed(InputAction.CallbackContext obj)
    {
        // If the player is not ready to jump or not grounded, return
        if (!_readyToJump || !_isGrounded)
            return;

        // Set the ready to jump flag to false
        _readyToJump = false;

        // Perform jump
        Jump();

        // Reset jump after cooldown
        Invoke(nameof(ResetJump), jumpCooldown);
    }

    private void OnMovePerformed(InputAction.CallbackContext obj)
    {
        var moveInput = obj.ReadValue<Vector2>();

        _horizontalInput = moveInput.x;
        _verticalInput = moveInput.y;
    }

    private void OnMoveCanceled(InputAction.CallbackContext obj)
    {
        _horizontalInput = 0;
        _verticalInput = 0;
    }


    private void OnSprintPerformed(InputAction.CallbackContext obj)
    {
        // Set the sprint flag to true
        _isSprinting = true;
    }


    private void OnSprintCanceled(InputAction.CallbackContext obj)
    {
        // Set the sprint flag to false
        _isSprinting = false;
    }

    #endregion

    private void Awake()
    {
        // Initialize the Components
        InitializeComponents();
    }

    private void Start()
    {
        InitializeInput();
    }

    private void InitializeComponents()
    {
        // Get the Rigidbody component
        _rb = GetComponent<Rigidbody>();

        // Prevent the Rigidbody from rotating
        _rb.freezeRotation = true;

        // Get the Dash component
        dash = GetComponent<Dash>();

        // Get the WallRunning component
        wallRunning = GetComponent<WallRunning>();
    }

    private void Update()
    {
        // Check if the player is grounded by casting a ray downward
        _isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        // Control the player's speed
        SpeedControl();

        // Handle the player's movement state
        StateHandler();

        // Handle drag based on whether the player is grounded and not wall running
        if (_isGrounded && !IsWallRunning)
            _rb.drag = groundDrag;
        else
            _rb.drag = 0;
    }

    private void FixedUpdate()
    {
        // Handle player movement in FixedUpdate for physics-based movement
        MovePlayer();
    }

    public void MovePlayer()
    {
        // Calculate the move direction based on input and orientation
        _moveDirection = (orientation.forward * _verticalInput + orientation.right * _horizontalInput).normalized;

        // Remove the y component of the move direction to prevent vertical movement
        _moveDirection = new Vector3(_moveDirection.x, 0f, _moveDirection.z);

        // Move the player on the ground
        if (_isGrounded && !IsWallRunning)
            _rb.AddForce(_moveDirection * (_moveSpeed * 10f), ForceMode.Force);

        // Move the player in the air
        else if (!_isGrounded && !IsWallRunning)
            _rb.AddForce(_moveDirection * (_moveSpeed * 10f * airMultiplier), ForceMode.Force);
    }

    private void SpeedControl()
    {
        // Control the player's speed to prevent exceeding the maximum speed
        var flatVel = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

        if (flatVel.magnitude > _moveSpeed)
        {
            var limitedVel = flatVel.normalized * _moveSpeed;
            _rb.velocity = new Vector3(limitedVel.x, _rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // Reset the vertical velocity and apply jump force
        _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
        _rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        // Allow the player to jump again after cooldown
        _readyToJump = true;
    }

    private void StateHandler()
    {
        // Handle the player's movement state using a switch case
        switch (_movementState)
        {
            case MovementState.Walking:
                _moveSpeed = walkSpeed; // Set move speed to walk speed
                dash.canDash = true;

                SetSoundState(footsteps, false);
                SetSoundState(wallFootSteps, false);
                break;

            case MovementState.Sprinting:
                _moveSpeed = sprintSpeed; // Set move speed to sprint speed
                dash.canDash = true;

                SetSoundState(footsteps, true);
                SetSoundState(wallFootSteps, false);
                break;

            case MovementState.WallRunning:
                _moveSpeed = wallrunSpeed; // Set move speed to wall run speed
                dash.canDash = false; // disable dashing while wallrunning
                _rb.drag = 0; // Disable drag during wall running

                SetSoundState(footsteps, false);
                SetSoundState(wallFootSteps, true);
                break;

            case MovementState.Air:
                // Apply air drag and set move speed
                _moveSpeed = walkSpeed * airMultiplier;
                dash.canDash = true;


                SetSoundState(footsteps, false);
                SetSoundState(wallFootSteps, false);
                break;

            case MovementState.Climbing:
                //moveSpeed = cm.climbSpeed; // Set move speed to climb speed
                break;

            default:
                break;
        }

        // Automatically switch states based on conditions
        if (IsWallRunning)
            _movementState = MovementState.WallRunning;

        else if (_isGrounded)
        {
            if (_isSprinting)
                _movementState = MovementState.Sprinting;

            else
                _movementState = MovementState.Walking;
        }

        else
            _movementState = MovementState.Air;
    }

    private void SetSoundState(AudioSource source, bool active)
    {
        // Return if the source is null
        if (source == null)
            return;

        // Start the sound if active is true
        if (active)
            source.Play();

        // Stop the sound if active is false
        else
            source.Stop();
    }
}