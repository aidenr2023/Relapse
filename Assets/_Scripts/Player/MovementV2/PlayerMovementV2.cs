using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerMovementV2 : ComponentScript<Player>, IPlayerController, IDebugged
{
    private const float GROUND_VELOCITY_THRESHOLD = 0.05f;

    #region Serialized Fields

    [Header("Important Transforms")]

    // Reference to the player's camera pivot
    [SerializeField]
    private Transform cameraPivot;

    // Reference to the player's orientation transform
    [SerializeField] private Transform orientation;

    [Header("Ground Checking")]
    // Reference to a transform used to check if the player is grounded
    [SerializeField]
    private Transform groundChecker;

    // How far the ground checker should check for ground
    [SerializeField] [Min(0)] private float groundCheckDistance = 0.2f;

    [SerializeField] [Min(0)] private float groundCheckBoxAdjust = .25f;

    [Header("Stats")] [SerializeField] [Min(0)]
    private float movementSpeed = 10f;

    #endregion

    #region Private Fields

    private Rigidbody _rigidbody;

    private CustomStack<PlayerMovementScript> _movementScripts;

    private InputActionMap _currentActionMap;

    private bool _groundCollide;

    private RaycastHit _groundCollideHitInfo;

    #endregion

    #region Getters

    public GameObject CameraPivot => cameraPivot.gameObject;

    public Transform Orientation => orientation;

    public Rigidbody Rigidbody => _rigidbody;

    public Vector2 MovementInput => BasicPlayerMovement.MovementInput;

    public bool IsGrounded { get; private set; }

    public bool IsSprinting { get; }

    public float MovementSpeed => movementSpeed;

    public PlayerMovementScript CurrentMovementScript => _movementScripts.Peek();

    public PlayerWallRunning WallRunning { get; private set; }

    public Vector3 GroundCollisionForward =>
        Vector3.Cross(_groundCollideHitInfo.normal, -CameraPivot.transform.right).normalized;

    public Vector3 GroundCollisionRight =>
        Vector3.Cross(_groundCollideHitInfo.normal, CameraPivot.transform.forward).normalized;

    public BasicPlayerMovement BasicPlayerMovement { get; private set; }

    #endregion

    protected override void CustomAwake()
    {
        // Get the components
        GetComponents();
    }

    private void GetComponents()
    {
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

    #region Update Functions

    private void FixedUpdate()
    {
        // Update the on ground
        UpdateOnGround();

        // Update the current movement script
        CurrentMovementScript?.FixedMovementUpdate();
    }

    private void UpdateOnGround()
    {
        // Create a mask that includes everything but the 'Actor' and 'Wall' layers
        // var mask = ~LayerMask.GetMask("Actor", "Wall");
        var mask = ~LayerMask.GetMask("Actor");

        // // Check if there is a collider below the player
        // _groundCollide = Physics.Raycast(
        //     groundChecker.position,
        //     Vector3.down,
        //     out _groundCollideHitInfo,
        //     groundCheckDistance,
        //     mask
        // );

        // Check if there is a collider below the player using a box cast
        _groundCollide = Physics.BoxCast(
            groundChecker.position + new Vector3(0, groundCheckBoxAdjust / 2, 0),
            new Vector3(0.5f, groundCheckBoxAdjust / 2, 0.5f),
            Vector3.down,
            out _groundCollideHitInfo,
            Quaternion.identity,
            groundCheckDistance,
            mask
        );

        // var groundCollisions = new List<RaycastHit>();
        //
        // var collides = false;
        //
        //
        // _groundCollide = collides;
        //
        // // Set the ground hit info to the item w/ the highest y value
        // if (groundCollisions.Count > 0)
        // {
        //     _groundCollideHitInfo = groundCollisions[0];
        //
        //     foreach (var hitInfo in groundCollisions)
        //     {
        //         if (hitInfo.point.y > _groundCollideHitInfo.point.y)
        //             _groundCollideHitInfo = hitInfo;
        //     }
        // }

        // if (_groundCollide)
        //     Debug.Log($"Collided with {hitInfo.collider.name}");

        // Check if the player's vertical velocity is less than the threshold
        var verticalVelocityCheck = Mathf.Abs(_rigidbody.velocity.y) < GROUND_VELOCITY_THRESHOLD;

        // TODO: Delete this maybe?
        verticalVelocityCheck = true;

        // Set the on ground to true if the player is on the ground
        // IsGrounded = _groundCollide && verticalVelocityCheck;
        IsGrounded = _groundCollide;
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

        // Add the current movement script's debug text
        var movementScriptDebugText = CurrentMovementScript?.GetDebugText();
        if (movementScriptDebugText != null)
        {
            sb.AppendLine($"\n");
            sb.AppendLine($"\tCurrent Movement Script: {CurrentMovementScript.GetType().Name}");

            // Split the debug text by new lines
            var splitDebugText = movementScriptDebugText.Split('\n');

            // Append an extra tab to each line
            foreach (var text in splitDebugText)
                sb.AppendLine($"\t{text}");
        }

        sb.AppendLine($"Movement input maps:");
        foreach (var movementScript in GetComponents<PlayerMovementScript>())
        {
            string inputString = "NONE";
            if (movementScript.InputActionMap != null)
                inputString = movementScript.InputActionMap.enabled ? "ENABLED" : "DISABLED";

            sb.AppendLine($"\t{movementScript.GetType().Name}: {inputString}");
        }

        return sb.ToString();
    }

    private void OnDrawGizmos()
    {
        // Draw the forward vector of the orientation
        Gizmos.color = Color.red;
        Gizmos.DrawRay(orientation.position, orientation.forward * 3);

        // // Draw the ground check vector
        // Gizmos.color = Color.green;
        // Gizmos.DrawRay(groundChecker.position, Vector3.down * groundCheckDistance);

        // Draw the ground check box
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            groundChecker.position + new Vector3(0, groundCheckBoxAdjust / 2 - groundCheckDistance, 0),
            new Vector3(1, groundCheckBoxAdjust, 1)
        );

        // Draw the ground collision normal's forward direction
        if (_groundCollide)
        {
            var groundColor = new Color(1, .4f, 0, 1);

            Gizmos.color = groundColor;
            Gizmos.DrawRay(_groundCollideHitInfo.point, GroundCollisionForward * 10);
            Gizmos.DrawRay(_groundCollideHitInfo.point, GroundCollisionRight * 10);
        }
    }

    #endregion
}