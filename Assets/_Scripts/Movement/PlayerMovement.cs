using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum MovementState
{
    walking,
    sprinting,
    wallrunning,
    air,
    climbing,
}

public class PlayerMovement : MonoBehaviour, IPlayerController
{
    #region Fields

    [Header("Movement")] private float moveSpeed; // The current speed of the player
    public float walkSpeed; // The speed when the player is walking
    public float sprintSpeed; // The speed when the player is sprinting
    public float wallrunSpeed; // The speed when the player is wall running
    public float groundDrag; // Drag applied to the player when grounded
    public float jumpForce; // The force applied when the player jumps
    public float jumpCooldown; // The cooldown time between jumps
    public float airMultiplier; // Multiplier for movement speed when in the air
    public bool readyToJump = true; // Flag to check if the player is ready to jump
    public AudioSource footsteps;
    public AudioSource wallFootSteps;

    [Header("References")]
    //public Climbing cm; // Reference to the Climbing script (if applicable)
    public WallRunning wallRunning; // Reference to the WallRunning script

    public Dash dash;

    [Header("Ground Check")] public float playerHeight; // Height of the player for ground checking
    public LayerMask whatIsGround; // LayerMask to define what is considered ground
    [HideInInspector] public bool grounded; // Flag to check if the player is grounded

    private Rigidbody _rb; // Reference to the player's Rigidbody component
    public Transform orientation; // Reference to the player's orientation transform

    [HideInInspector] public Vector3 moveDirection; // Direction of the player's movement
    public MovementState state; // The current movement state of the player
    [HideInInspector] public bool isWallRunning; // Flag to check if the player is wall running
    public bool climbing; // Flag to check if the player is climbing

    // Horizontal input value
    private float horizontalInput;

    // Vertical input value
    private float verticalInput;

    // A flag to check if the player is sprinting
    private bool _isSprinting;

    private PlayerCam _playerCam;
    
    #endregion

    public GameObject CameraPivot { get; private set; }

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
        if (!readyToJump || !grounded)
            return;

        // Set the ready to jump flag to false
        readyToJump = false;

        // Perform jump
        Jump();

        // Reset jump after cooldown
        Invoke(nameof(ResetJump), jumpCooldown);
    }

    private void OnMovePerformed(InputAction.CallbackContext obj)
    {
        var moveInput = obj.ReadValue<Vector2>();

        horizontalInput = moveInput.x;
        verticalInput = moveInput.y;
    }

    private void OnMoveCanceled(InputAction.CallbackContext obj)
    {
        horizontalInput = 0;
        verticalInput = 0;
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
        _rb = GetComponent<Rigidbody>(); // Get the Rigidbody component
        _rb.freezeRotation = true; // Prevent the Rigidbody from rotating
        
        // Find the player cam component in the children
        CameraPivot = GetComponentInChildren<PlayerCam>().gameObject;
    }

    private void Start()
    {
        InitializeInput();
    }

    private void Update()
    {
        // Check if the player is grounded by casting a ray downward
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        // Control the player's speed
        SpeedControl();
        
        // Handle the player's movement state
        StateHandler(); 

        // Handle drag based on whether the player is grounded and not wall running
        if (grounded && !isWallRunning)
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
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Move the player on the ground
        if (grounded && !isWallRunning)
            _rb.AddForce(moveDirection.normalized * (moveSpeed * 10f), ForceMode.Force);

        // Move the player in the air
        else if (!grounded && !isWallRunning)
            _rb.AddForce(moveDirection.normalized * (moveSpeed * 10f * airMultiplier), ForceMode.Force);
    }

    private void SpeedControl()
    {
        // Control the player's speed to prevent exceeding the maximum speed
        Vector3 flatVel = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
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
        readyToJump = true; 
    }

    private void StateHandler()
    {
        // Handle the player's movement state using a switch case
        switch (state)
        {
            case MovementState.walking:
                moveSpeed = walkSpeed; // Set move speed to walk speed
                dash.canDash = true;

                if (footsteps.isPlaying)
                    footsteps.Stop();
                if (wallFootSteps.isPlaying)
                    wallFootSteps.Stop();

                break;

            case MovementState.sprinting:
                moveSpeed = sprintSpeed; // Set move speed to sprint speed
                dash.canDash = true;

                if (!footsteps.isPlaying)
                    footsteps.Play();
                if (wallFootSteps.isPlaying)
                    wallFootSteps.Stop();

                break;

            case MovementState.wallrunning:
                moveSpeed = wallrunSpeed; // Set move speed to wall run speed
                dash.canDash = false; // disable dashing while wallrunning
                _rb.drag = 0; // Disable drag during wall running

                if (footsteps.isPlaying)
                    footsteps.Stop();
                if (!wallFootSteps.isPlaying)
                    wallFootSteps.Play();

                break;

            case MovementState.air:
                // Apply air drag and set move speed
                moveSpeed = walkSpeed * airMultiplier;
                dash.canDash = true;

                if (footsteps.isPlaying)
                    footsteps.Stop();
                if (wallFootSteps.isPlaying)
                    wallFootSteps.Stop();

                break;

            case MovementState.climbing:
                //moveSpeed = cm.climbSpeed; // Set move speed to climb speed
                break;

            default:
                break;
        }

        // Automatically switch states based on conditions
        if (isWallRunning)
            state = MovementState.wallrunning;
        
        else if (grounded)
        {
            if (_isSprinting)
                state = MovementState.sprinting;

            else
                state = MovementState.walking;
        }

        else
            state = MovementState.air;
    }
}