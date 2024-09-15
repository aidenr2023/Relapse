using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;  // Include the new Input System namespace

public class Climbing : MonoBehaviour
{
    public static Climbing instance { get; private set; }  // Singleton instance of Climbing script

    [Header("References")]
    public Transform orientation;  // Reference to the player's orientation transform
    public Rigidbody rb;           // Reference to the player's Rigidbody
    [HideInInspector] public PlayerMovement pm;  // Reference to PlayerMovement script
    public LayerMask whatIsWall;   // LayerMask to detect climbable walls

    [Header("Climbing")]
    public float climbSpeed;       // Speed of climbing
    public float maxClimbTime;     // Maximum time allowed for climbing
    private float climbTimer;      // Timer to track climbing time

    private bool climbing;         // Flag to check if the player is climbing

    [Header("Detection")]
    public float detectionLength;  // Length of the ray used to detect walls
    public float sphereCastRadius; // Radius of the sphere cast for wall detection
    public float maxWallLookAngle; // Maximum angle between the player's look direction and the wall
    private float wallLookAngle;   // Current angle between the player's look direction and the wall

    private RaycastHit frontWallHit;  // Information about the wall hit by the sphere cast
    private bool wallFront;           // Flag to check if there is a wall in front

    [Header("Input Actions")]
    public InputActionReference moveAction;  // Input action for movement

    private void Start()
    {
        instance = this;  // Set the singleton instance
        rb = GetComponent<Rigidbody>();  // Get the Rigidbody component
        pm = GetComponent<PlayerMovement>();  // Get the PlayerMovement script

        moveAction.action.Enable();  // Enable the input action for movement
    }

    private void OnDestroy()
    {
        // Disable the input action when the script is destroyed
        moveAction.action.Disable();  
    }

    private void Update()
    {
        WallCheck();      // Check for walls in front of the player
        StateMachine();   // Manage the climbing state
        if (climbing) 
            ClimbingMovement();  // Move the player if they are climbing
    }

    private void StateMachine()
    {
        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();

        // Check if the player is in a state where they can start climbing
        if (wallFront && moveInput.y > 0 && wallLookAngle < maxWallLookAngle)
        {
            if (!climbing && climbTimer > 0) 
                StartClimbing();  // Start climbing if conditions are met

            if (climbTimer > 0) 
                climbTimer -= Time.deltaTime;  // Decrease the climbing timer
            if (climbTimer < 0) 
                StopClimbing();  // Stop climbing if the timer runs out
        }
        else
        {
            if (climbing) 
                StopClimbing();  // Stop climbing if conditions are no longer met
        }
    }

    private void WallCheck()
    {
        // Cast a sphere in front of the player to detect walls
        wallFront = Physics.SphereCast(transform.position, sphereCastRadius, orientation.forward, out frontWallHit, detectionLength, whatIsWall);
        wallLookAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);  // Calculate the angle between the player's forward direction and the wall

        if (pm.grounded)
        {
            climbTimer = maxClimbTime;  // Reset the climbing timer if the player is grounded
        }
    }

    private void StartClimbing()
    {
        pm.climbing = true;  // Set the climbing flag in PlayerMovement script
        ClimbingMovement();  // Move the player up the wall
    }

    private void ClimbingMovement()
    {
        // Move the player upward at the climbing speed
        rb.velocity = new Vector3(rb.velocity.x, climbSpeed, rb.velocity.y);
    }

    private void StopClimbing()
    {
        climbing = false;  // Stop climbing
    }
}
