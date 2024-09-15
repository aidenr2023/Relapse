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

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;  // The current speed of the player
    public float walkSpeed;   // The speed when the player is walking
    public float sprintSpeed; // The speed when the player is sprinting
    public float wallrunSpeed; // The speed when the player is wall running
    public float groundDrag;  // Drag applied to the player when grounded
    public float jumpForce;   // The force applied when the player jumps
    public float jumpCooldown; // The cooldown time between jumps
    public float airMultiplier; // Multiplier for movement speed when in the air
    public bool readyToJump = true; // Flag to check if the player is ready to jump
    public AudioSource footsteps;
    public AudioSource wallFootSteps;
    
    [Header("References")]
    //public Climbing cm; // Reference to the Climbing script (if applicable)
    public WallRunning wallRunning; // Reference to the WallRunning script
    public Dash dash;
    
    [Header("Input Actions")]
    public InputActionReference moveAction;   // Input action for movement
    public InputActionReference jumpAction;   // Input action for jumping
    public InputActionReference sprintAction; // Input action for sprinting

    [Header("Ground Check")]
    public float playerHeight;  // Height of the player for ground checking
    public LayerMask whatIsGround; // LayerMask to define what is considered ground
    [HideInInspector] public bool grounded; // Flag to check if the player is grounded

    private Rigidbody rb; // Reference to the player's Rigidbody component
    public Transform orientation; // Reference to the player's orientation transform
    private float horizontalInput; // Horizontal input value
    private float verticalInput; // Vertical input value
    [HideInInspector] public Vector3 moveDirection; // Direction of the player's movement
    public MovementState state; // The current movement state of the player
    [HideInInspector] public bool isWallRunning; // Flag to check if the player is wall running
    public bool climbing; // Flag to check if the player is climbing

    private void Awake()
    {
        rb = GetComponent<Rigidbody>(); // Get the Rigidbody component
        rb.freezeRotation = true; // Prevent the Rigidbody from rotating
    }

    private void Start()
    {
        // Enable input actions
        moveAction.action.Enable();
        jumpAction.action.Enable();
        sprintAction.action.Enable();
    }

    private void Update()
    {
        // Check if the player is grounded by casting a ray downward
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput(); // Handle player input
        SpeedControl(); // Control the player's speed
        StateHandler(); // Handle the player's movement state

        // Handle drag based on whether the player is grounded and not wall running
        if (grounded && !isWallRunning)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer(); // Handle player movement in FixedUpdate for physics-based movement
    }

    private void MyInput()
    {
        // Read movement input from the new Input System
        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
        horizontalInput = moveInput.x;
        verticalInput = moveInput.y;

        // Handle jumping
        if (jumpAction.action.triggered && readyToJump && grounded)
        {
            readyToJump = false;
            Jump(); // Perform jump
            Invoke(nameof(ResetJump), jumpCooldown); // Reset jump after cooldown
        }
        // else if (jumpAction.action.triggered && isWallRunning)
        // {
        //     wallRunning.JumpOffWall(); // Perform wall jump
        //     Invoke(nameof(ResetJump), jumpCooldown); // Reset jump after cooldown
        // }
    }

    public void MovePlayer()
    {
        // Calculate the move direction based on input and orientation
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Move the player on the ground
        if (grounded && !isWallRunning)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        // Move the player in the air
        else if (!grounded && !isWallRunning)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        // Control the player's speed to prevent exceeding the maximum speed
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // Reset the vertical velocity and apply jump force
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true; // Allow the player to jump again after cooldown
    }

    private void StateHandler()
    {
        // Handle the player's movement state using a switch case
        switch (state)
        {
            case MovementState.walking:
                moveSpeed = walkSpeed; // Set move speed to walk speed
                dash.canDash = true;
                if (footsteps.isPlaying) footsteps.Stop();
                if (wallFootSteps.isPlaying) wallFootSteps.Stop();
                break;

            case MovementState.sprinting:
                moveSpeed = sprintSpeed; // Set move speed to sprint speed
                dash.canDash = true;
                if (!footsteps.isPlaying) footsteps.Play();
                if (wallFootSteps.isPlaying) wallFootSteps.Stop();
                break;

            case MovementState.wallrunning:
                moveSpeed = wallrunSpeed; // Set move speed to wall run speed
                dash.canDash = false; // disable dashing while wallrunning
                rb.drag = 0; // Disable drag during wall running
                if (footsteps.isPlaying) footsteps.Stop();
                if (!wallFootSteps.isPlaying) wallFootSteps.Play();
                break;

            case MovementState.air:
                // Apply air drag and set move speed
                moveSpeed = walkSpeed * airMultiplier;
                dash.canDash = true;
                if (footsteps.isPlaying) footsteps.Stop();
                if (wallFootSteps.isPlaying) wallFootSteps.Stop();
                break;

            case MovementState.climbing:
                //moveSpeed = cm.climbSpeed; // Set move speed to climb speed
                break;

            default:
                break;
        }
        // Automatically switch states based on conditions
        if (isWallRunning)
        {
            state = MovementState.wallrunning;
            // footsteps.Stop();
            // wallFootSteps.Play();
        }
        else if (grounded)
        {
            // wallFootSteps.Stop();
            if (sprintAction.action.IsPressed())
            {
                state = MovementState.sprinting;
            }
            else
            {
                state = MovementState.walking;
                // footsteps.Stop();
            }
        }
        else
        {
            state = MovementState.air;
            // footsteps.Stop();
        }
    }
}
