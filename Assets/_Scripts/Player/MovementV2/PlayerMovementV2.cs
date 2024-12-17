using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerMovementV2 : ComponentScript<Player>, IPlayerController, IDebugged, IUsesInput
{
    private const float GROUND_VELOCITY_THRESHOLD = 0.05f;

    #region Serialized Fields

    [Header("Important Transforms")]

    // Reference to the player's camera pivot
    [SerializeField]
    private Transform cameraPivot;

    // Reference to the player's orientation transform
    [SerializeField] private Transform orientation;

    [Header("Floating Controller")] [SerializeField]
    private float desiredCapsuleHeight = 2;

    [SerializeField] private float totalPlayerHeight = 2;

    [SerializeField] private float rideHeightOffset = 1f;
    [SerializeField] [Min(0)] private float rideHeight = .5f;
    [SerializeField] private float rideSpringStrength;
    [SerializeField] private float rideSpringDamper;

    [Header("Locomotion")] [SerializeField]
    private float maxSpeed = 10;

    [SerializeField] private float acceleration = 200;
    [SerializeField] private AnimationCurve accelerationFactorFromDot;

    #endregion

    #region Private Fields

    private Rigidbody _rigidbody;

    private CustomStack<PlayerMovementScript> _movementScripts;

    private InputActionMap _currentActionMap;

    private bool _groundCollide;

    private RaycastHit _floatingControllerHit;

    private CapsuleCollider _capsuleCollider;

    private bool _isSprinting;

    #endregion

    #region Getters

    public GameObject CameraPivot => cameraPivot.gameObject;

    public Transform Orientation => orientation;

    public Rigidbody Rigidbody => _rigidbody;

    public Vector2 MovementInput => BasicPlayerMovement.MovementInput;

    public bool IsGrounded { get; private set; }

    public RaycastHit GroundHit => _floatingControllerHit;

    public AnimationCurve AccelerationFactorFromDot => accelerationFactorFromDot;

    public float Acceleration => acceleration;

    public bool IsSprintToggled { get; set; }

    public bool IsSprinting => _isSprinting || IsSprintToggled;

    public float MovementSpeed => maxSpeed;

    public PlayerMovementScript CurrentMovementScript => _movementScripts.Peek();

    public PlayerWallRunning WallRunning { get; private set; }

    public Vector3 GroundCollisionForward =>
        Vector3.Cross(_floatingControllerHit.normal, -CameraPivot.transform.right).normalized;

    public Vector3 GroundCollisionRight =>
        Vector3.Cross(_floatingControllerHit.normal, CameraPivot.transform.forward).normalized;

    public BasicPlayerMovement BasicPlayerMovement { get; private set; }

    public HashSet<InputData> InputActions { get; } = new();

    #endregion

    protected override void CustomAwake()
    {
        // Get the components
        GetComponents();

        // Initialize the input
        InitializeInput();
    }

    private void OnEnable()
    {
        // Register this to the input manager
        InputManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        // Unregister this from the input manager
        InputManager.Instance.Unregister(this);
    }


    private void GetComponents()
    {
        // Get the capsule collider
        _capsuleCollider = GetComponent<CapsuleCollider>();

        // Get the rigid body
        _rigidbody = GetComponent<Rigidbody>();

        // Get the basic player movement
        BasicPlayerMovement = GetComponent<BasicPlayerMovement>();

        // Get the wall running component
        WallRunning = GetComponent<PlayerWallRunning>();
    }

    private void Start()
    {
        // Add this object to the debug manager
        DebugManager.Instance.AddDebuggedObject(this);

        // Create the movement scripts stack
        _movementScripts = new CustomStack<PlayerMovementScript>();

        // Push the basic player movement onto the stack
        PushMovementScript(BasicPlayerMovement);

        // Enable the top most input map
        EnableTopMostInputMap();
    }


    public void InitializeInput()
    {
        InputActions.Add(new InputData(
            InputManager.Instance.PControls.Player.Sprint, InputType.Performed, OnSprintPerformed)
        );
        InputActions.Add(new InputData(
            InputManager.Instance.PControls.Player.Sprint, InputType.Canceled, OnSprintCanceled)
        );
        InputActions.Add(new InputData(
            InputManager.Instance.PControls.Player.SprintToggle, InputType.Performed,
            OnSprintTogglePerformed)
        );
    }

    #region Update Functions

    private void Update()
    {
        Debug.Log($"Sprint Toggled!: {IsSprintToggled} -> ({IsSprinting})");
    }

    private void FixedUpdate()
    {
        // Update the ground check
        UpdateGroundCheck();

        // Update the spring force
        UpdateSpringForce();

        // Update the capsule collider height
        UpdateCapsuleColliderHeight();

        // Update the current movement script
        CurrentMovementScript?.FixedMovementUpdate();
    }

    private void UpdateGroundCheck()
    {
        // // Create a mask that includes everything but the 'Actor' layer
        // var mask = ~LayerMask.GetMask("Actor");
        //
        // // Perform a ray cast using the ride height
        // _groundCollide = Physics.Raycast(
        //     groundChecker.position,
        //     Vector3.down,
        //     out _groundCollideHitInfo,
        //     rideHeight,
        //     mask
        // );
        //

        // Create a layer mask that includes everything but the actor layer and NonPhysical
        var layerMask = ~LayerMask.GetMask("Actor", "NonPhysical");

        // Perform a raycast to check if the player is grounded
        var hit = Physics.Raycast(
            transform.position,
            Vector3.down,
            out _floatingControllerHit,
            rideHeight,
            layerMask
        );

        // Reset the capsule collider height if the player is not grounded
        if (!hit)
            _floatingControllerHit = new RaycastHit();

        // Set the grounded state to the hit state
        IsGrounded = hit;
    }


    private void UpdateSpringForce()
    {
        // Return if the floating controller hit is not set
        if (!_floatingControllerHit.collider)
            return;

        // Get the current velocity of the player
        var currentVelocity = _rigidbody.velocity;

        // Get the direction of the ray
        // var downDirection = transform.TransformDirection(Vector3.down);
        var downDirection = Vector3.down;

        // Get the velocity of the other object
        var otherVelocity = Vector3.zero;

        // Get the other rigidbody
        var otherRigidbody = _floatingControllerHit.rigidbody;

        // If there is no other rigidbody, set the other velocity to zero
        if (otherRigidbody)
            otherVelocity = otherRigidbody.velocity;

        // Get the relative velocity
        var rayDirectionVelocity = Vector3.Dot(downDirection, currentVelocity);
        var otherDirectionVelocity = Vector3.Dot(downDirection, otherVelocity);

        // Get the relative velocity
        var relativeVelocity = rayDirectionVelocity - otherDirectionVelocity;

        // Distance of the ray hit vs the ride height
        var rideHeightPenetration = _floatingControllerHit.distance - rideHeight;

        var remainingHeight = totalPlayerHeight - _capsuleCollider.height;
        var capsuleHeightOffset = rideHeight - remainingHeight;
        var hitOffset = _floatingControllerHit.distance - capsuleHeightOffset;
        var actualPenetration = remainingHeight - hitOffset;


        // Create a target velocity with the same x and z,
        // but enough velocity to go up by the actual penetration
        var targetVelocity = new Vector3(
            currentVelocity.x,
            actualPenetration,
            currentVelocity.z
        );

        // This is the force required to reach the target velocity in EXACTLY one frame
        var targetForce = (targetVelocity - _rigidbody.velocity) / Time.fixedDeltaTime;

        // Get the spring force
        var springForce = (rideHeightPenetration * rideSpringStrength) - (relativeVelocity * rideSpringDamper);

        // Debug.Log($"RHP: [{hitOffset:0.000}] [{actualPenetration:0.000}] [{targetForce.y:0.000}]");

        // Add force to the player
        _rigidbody.AddForce(downDirection * springForce, ForceMode.Acceleration);

        // Add force to the other object
        if (otherRigidbody)
            otherRigidbody.AddForceAtPosition(-downDirection * springForce, _floatingControllerHit.point);
    }

    private void UpdateCapsuleColliderHeight()
    {
        // Return if the floating controller hit is not set
        if (!_floatingControllerHit.collider)
        {
            // Reset the capsule collider height
            _capsuleCollider.height = desiredCapsuleHeight;

            // Reset the capsule collider center
            _capsuleCollider.center = Vector3.zero;

            return;
        }

        // Get the distance from the player to the floating controller hit
        var distance = _floatingControllerHit.distance;

        var heightAdjust = desiredCapsuleHeight - ((desiredCapsuleHeight / 2) - distance);

        // Set the capsule collider height based on the distance
        var newCapsuleHeight = Mathf.Min(desiredCapsuleHeight, heightAdjust);
        _capsuleCollider.height = Mathf.Max(newCapsuleHeight, 1);

        // Set the capsule collider y position based on the distance
        var newYPosition = (totalPlayerHeight - newCapsuleHeight) / 2;
        _capsuleCollider.center = new Vector3(0, newYPosition, 0);
    }

    #endregion

    #region Movement Script Management

    public void PushMovementScript(PlayerMovementScript movementScript)
    {
        // Push the movement script onto the stack
        _movementScripts.Push(movementScript);

        // Enable the top most input map
        EnableTopMostInputMap();
    }

    public void RemoveMovementScript(PlayerMovementScript movementScript)
    {
        // Remove the movement script from the stack
        _movementScripts.Remove(movementScript);

        // Enable the top most input map
        EnableTopMostInputMap();
    }

    private void EnableTopMostInputMap()
    {
        var previousInputMap = _currentActionMap;

        PlayerMovementScript topMostMovementScript = BasicPlayerMovement;

        var allMovementScripts = GetComponents<PlayerMovementScript>();

        // If the top most input map is not null, set the top most input map to the top most input map
        foreach (var movementScript in _movementScripts)
        {
            if (movementScript.InputActionMap != null)
                topMostMovementScript = movementScript;
        }

        // Disable all the input maps except for the top most input map
        foreach (var movementScript in allMovementScripts)
        {
            if (
                movementScript != topMostMovementScript &&
                movementScript.InputActionMap is { enabled: true }
            )
                movementScript.InputActionMap.Disable();
        }

        // If the top most input map is the same as the previous input map, return
        if (topMostMovementScript?.InputActionMap == _currentActionMap)
            return;

        // Enable the top most input map
        topMostMovementScript?.InputActionMap.Enable();

        // Set the current action map to the top most input map
        _currentActionMap = topMostMovementScript?.InputActionMap;

        // Debug.Log($"Enabled {topMostMovementScript?.GetType().Name}");
    }

    #endregion

    #region Debugging

    public string GetDebugText()
    {
        var sb = new StringBuilder();

        var lateralVelocity = new Vector2(_rigidbody.velocity.x, _rigidbody.velocity.z);

        sb.AppendLine($"Player:");
        sb.AppendLine($"\tPosition: {transform.position}");
        sb.AppendLine($"\tVelocity: {_rigidbody.velocity} ({lateralVelocity.magnitude:0.0000})");
        sb.AppendLine($"\tGrounded: {IsGrounded}");
        sb.AppendLine($"\tGCollide: {_groundCollide}");

        sb.AppendLine($"\tAll Movement Scripts: {string.Join(", ", _movementScripts.Select(n => n.GetType().Name))}");

        return sb.ToString();
    }

    private void OnDrawGizmos()
    {
        // Draw the forward vector of the orientation
        Gizmos.color = Color.red;
        Gizmos.DrawRay(orientation.position, orientation.forward * 3);

        // Draw the ground collision normal's forward direction
        if (_groundCollide)
        {
            var groundColor = new Color(1, .4f, 0, 1);

            Gizmos.color = groundColor;
            Gizmos.DrawRay(_floatingControllerHit.point, GroundCollisionForward * 10);
            Gizmos.DrawRay(_floatingControllerHit.point, GroundCollisionRight * 10);
        }

        // Draw the floating controller ray
        Gizmos.color = Color.red;
        Gizmos.DrawRay(
            transform.position,
            Vector3.down * rideHeight
        );

        if (_floatingControllerHit.collider)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(_floatingControllerHit.point, .125f);
        }
    }

    #endregion

    #region Input Functions

    private void OnSprintPerformed(InputAction.CallbackContext obj)
    {
        // Return if the player cannot sprint
        if (!BasicPlayerMovement.CanSprint)
            return;

        // Set the sprinting flag to true
        _isSprinting = true;

        Debug.Log($"Is Sprinting? {_isSprinting}");
    }

    private void OnSprintCanceled(InputAction.CallbackContext obj)
    {
        // Set the sprinting flag to false
        _isSprinting = false;
    }


    private void OnSprintTogglePerformed(InputAction.CallbackContext obj)
    {
        // Return if the player cannot sprint
        if (!BasicPlayerMovement.CanSprint)
        {
            IsSprintToggled = false;
            return;
        }

        // Set the sprinting flag to true
        IsSprintToggled = !IsSprintToggled;
    }

    #endregion
}