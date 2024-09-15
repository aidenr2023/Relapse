using UnityEngine;
using UnityEngine.InputSystem;

public class WallRunning : MonoBehaviour
{
    public float wallRunForce = 10f;
    public float maxWallRunTime = 2f;
    public float wallCheckDistance = 1f;
    public LayerMask wallLayer;
    public float wallJumpForce = 10f;
    public float wallJumpUpwardsForce = 5f;
    public float verticalWallRunSpeed = 5f;
    public float wallJumpCooldown = .5f;
    public PlayerMovement pm;
    //public Dash dash;

    //protected bool isWallRunning = false;
    private float wallRunTimer = 0f;
    private float wallJumpCooldownTimer = 0f;
    private Rigidbody rb;
    private Vector3 lastWallNormal;
    private bool isGrounded;
    private RaycastHit[] hits = new RaycastHit[1]; // Array to store the results of the raycast
    private PlayerControls playerInputActions;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInputActions = new PlayerControls();
    }

    void OnEnable()
    {
        playerInputActions.GamePlay.jump.performed += OnJumpPerformed;
        playerInputActions.Enable();
    }

    void OnDisable()
    {
        playerInputActions.GamePlay.jump.performed -= OnJumpPerformed;
        playerInputActions.Disable();
    }

    void FixedUpdate()
    {
        // Update the wall jump cooldown timer
        if (wallJumpCooldownTimer > 0)
        {
            wallJumpCooldownTimer -= Time.deltaTime;
        }
        
        CheckForWall();
        if (pm.isWallRunning)
        {
            PerformWallRun();
        }
        else
        {
            wallRunTimer = 0f;
        }
    }

    private void CheckForWall()
    {
        if (wallJumpCooldownTimer > 0)
        {
            StopWallRun();
            return;
        }
        
        // Check for walls on the right
        bool wallRight = Physics.RaycastNonAlloc(new Ray(transform.position, transform.right), hits, wallCheckDistance, wallLayer) > 0;

        // Check for walls on the left
        bool wallLeft = Physics.RaycastNonAlloc(new Ray(transform.position, -transform.right), hits, wallCheckDistance, wallLayer) > 0;

        if (wallRight || wallLeft)
        {
            lastWallNormal = hits[0].normal; // Update wall normal based on the first hit
            StartWallRun();
        }
        else
        {
            StopWallRun();
        }
    }

    private void StartWallRun()
    {
        if (!pm.isWallRunning)
        {
            pm.isWallRunning = true;
            wallRunTimer = 0f;
            rb.useGravity = false; // Disable gravity while wall running
        }
    }

    private void PerformWallRun()
    {
        // pm.isWallRunning = true;
        wallRunTimer += Time.deltaTime;

        // Stop wall running if the timer exceeds the max duration
        if (wallRunTimer > maxWallRunTime)
        {
            StopWallRun();
            return;
        }

        // Calculate the direction along the wall
        Vector3 wallRunDirection = Vector3.Cross(lastWallNormal, Vector3.up);

        // Ensure the player moves forward relative to their input direction
        if (Vector3.Dot(wallRunDirection, rb.velocity) < 0)
        {
            wallRunDirection = -wallRunDirection; // Flip direction if necessary
        }

        // Apply force to move the player along the wall
        rb.velocity = new Vector3(wallRunDirection.x * wallRunForce, rb.velocity.y, wallRunDirection.z * wallRunForce);

        // Get vertical movement input (e.g., W/S or joystick vertical axis)
        float verticalInput = playerInputActions.GamePlay.Movement.ReadValue<Vector2>().y;

        // Adjust the player's velocity for vertical movement on the wall
        rb.velocity = new Vector3(rb.velocity.x, verticalInput * verticalWallRunSpeed, rb.velocity.z);

        // Apply a small force towards the wall to keep the player attached
        rb.AddForce(-lastWallNormal * wallRunForce, ForceMode.Force);
    }

    private void StopWallRun()
    {
        pm.isWallRunning = false;
        rb.useGravity = true; // Re-enable gravity
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (pm.isWallRunning)
        {
            WallJump();
        }
    }

    private void WallJump()
    {
        // Calculate the jump direction away from the wall
        Vector3 wallJumpDirection = lastWallNormal + Vector3.up * wallJumpUpwardsForce;

        // Apply the jump force
        rb.velocity = new Vector3(wallJumpDirection.x * wallJumpForce, wallJumpDirection.y, wallJumpDirection.z * wallJumpForce);

        // Start the wall jump cooldown timer
        wallJumpCooldownTimer = wallJumpCooldown;
        
        // Stop wall running after the jump
        StopWallRun();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = false;
        }
    }
}
