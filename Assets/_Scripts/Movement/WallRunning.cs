using Cinemachine;
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
    public bool canWallRun = false;
    public PlayerMovement pm;
    //public Dash dash;

    
    //protected bool isWallRunning = false;
    private float wallRunTimer = 0f;
    private float wallJumpCooldownTimer = 0f;
    private Rigidbody rb;
    private Vector3 lastWallNormal;
    private bool isGrounded;
    //[SerializeField] private float boxCastSize = 0.5f;
    private RaycastHit[] hits = new RaycastHit[1]; // Array to store the results of the raycast
    private PlayerControls playerInputActions;
    

    
    
    //wall tilt reference
    [SerializeField] private CinemachineVirtualCamera vcam;

    // Box cast 
    public Vector3 boxCastSize = new Vector3(0.2f, 1.8f, 0.2f);
    [SerializeField] private float boxOffset = 0.5f; 
    public float wallBoxCastDistance = 1f;
    public Transform orientation;
    
    
    //Camera Tilt 
    private float targetTilt = 0f;  // Target tilt angle
    [SerializeField] [Range(-45, 45)]private float wallRunTiltAngle = 15f; // Max tilt angle during wall run
    [SerializeField] [Range(1,15)]private float tiltSpeed = 5f;         // Speed at which the tilt happens
    private float tiltVelocity = 0f;

    
    [SerializeField] [Range(-45,45)]private float currentTilt = 0f; // Current tilt value for Lerp
    
    #region Properties
    
    public bool IsWallRunning { get; private set; }
    
    #endregion
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        //vcam = GetComponent<CinemachineVirtualCamera>();
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
        UpdateCameraTilt();       
        // Update the wall jump cooldown timer
        if (wallJumpCooldownTimer > 0)
        {
            wallJumpCooldownTimer -= Time.deltaTime;
        }
        
        CheckForWall();
        if (pm.IsWallRunning)
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
        
        //checks for walls on the forward 
        bool wallForward = Physics.RaycastNonAlloc(new Ray(transform.position, transform.forward), hits, wallCheckDistance, wallLayer) > 0;

        //checks for walls on the backward 
        bool wallBackward = Physics.RaycastNonAlloc(new Ray(transform.position, -transform.forward), hits, wallCheckDistance, wallLayer) > 0;

        // if (wallRight || wallForward)
        // {
        //     targetTilt = wallRunTiltAngle; // Tilt to the right
        //     //UpdateCameraTilt();
        // }
        // else if (wallLeft || wallBackward)
        // {
        //     targetTilt = -wallRunTiltAngle; // Tilt to the left
        //     //UpdateCameraTilt();
        // }
        // else
        // {
        //     targetTilt = 0f; // Reset tilt if not on a wall
        // }

        
        //checks for walls for z and x axis
        if (wallRight || wallLeft || wallForward || wallBackward )
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
        if (!pm.IsWallRunning && canWallRun)
        {
            IsWallRunning = true;
            wallRunTimer = 0f;
            rb.useGravity = false; // Disable gravity while wall running
        }
    }

    private void PerformWallRun()
    {
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
        canWallRun = false;
        IsWallRunning = false;
        rb.useGravity = true; // Re-enable gravity
        //targetTilt = 0;
    }
    private void UpdateCameraTilt()
    {
        //box placements
        Vector3 leftBoxOrigin = orientation.position + (-orientation.right * boxOffset); 
        Vector3 rightBoxOrigin = orientation.position + (orientation.right * boxOffset); 


        //check for walls on the lefy 
        RaycastHit[] leftBoxHits = Physics.BoxCastAll(leftBoxOrigin, boxCastSize / 2, -orientation.right, orientation.rotation, wallBoxCastDistance, wallLayer);
        RaycastHit[] rightBoxHits = Physics.BoxCastAll(rightBoxOrigin, boxCastSize / 2, orientation.right, orientation.rotation, wallBoxCastDistance, wallLayer);

        bool isLeftWall = leftBoxHits.Length > 0;
        bool isRightWall = rightBoxHits.Length > 0;

        //targetTilt = 0;
        if (isLeftWall)
        {
            targetTilt = -wallRunTiltAngle; //tilt left
        }
        else if (isRightWall)
        {
            targetTilt = wallRunTiltAngle;// tilt right
        }
        else
        {
            targetTilt = 0;
        }
        
        
        // Smoothly interpolate the current tilt to the target tilt
        currentTilt = Mathf.SmoothDamp(currentTilt, targetTilt, ref tiltVelocity, tiltSpeed * Time.deltaTime);
        
        vcam.m_Lens.Dutch = currentTilt;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (pm.IsWallRunning)
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
    void OnDrawGizmos()
    {
        // if (pm == null)
        //     return;
        //
        // if (pm.IsWallRunning)
        // {
        //     Gizmos.color = Color.red;
        //     Gizmos.DrawLine(transform.position, transform.position + lastWallNormal);
        // }
        {
            // Gizmos.color = Color.red;
            //
            // Gizmos.color = Color.red;
            //
            // // Calculate positions for left and right BoxCasts based on the player's orientation
            // Vector3 leftBoxOrigin = orientation.position + (-orientation.right * boxOffset); // Left box position
            // Vector3 rightBoxOrigin = orientation.position + (orientation.right * boxOffset); // Right box position
            //
            // // Draw left BoxCast with the player's rotation
            // Gizmos.matrix = Matrix4x4.TRS(leftBoxOrigin, orientation.rotation, Vector3.one);
            // Gizmos.DrawWireCube(Vector3.zero, boxCastSize);
            //
            // // Draw right BoxCast with the player's rotation
            // Gizmos.color = Color.blue;
            // Gizmos.matrix = Matrix4x4.TRS(rightBoxOrigin, orientation.rotation, Vector3.one);
            // Gizmos.DrawWireCube(Vector3.zero, boxCastSize);
        }
    }
    }
    

