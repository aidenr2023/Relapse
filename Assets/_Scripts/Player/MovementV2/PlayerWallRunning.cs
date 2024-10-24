using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerWallRunning : PlayerMovementScript, IDebugged
{
    [SerializeField] private bool isEnabled = true;

    [SerializeField] private LayerMask wallLayer;

    [SerializeField] [Range(0, 1)] private float wallRunningSensitivity = 1f;

    [Header("Wall Jump")] [SerializeField] [Min(0)]
    private float wallJumpForce = 10f;

    [SerializeField] [Range(0, 90)] private float wallJumpAngle = 20f;

    [SerializeField] [Min(0)] private float autoWallJumpForce = .5f;

    #region Private Fields

    private ContactPoint[] _contactPoints;
    private int _contactPointIndex = -1;

    private bool _isWallRunning;
    private bool _isWallRunningLeft;
    private bool _isWallRunningRight;

    // The objects the player is wall running on
    private HashSet<GameObject> _wallRunningObjects;

    private Vector2 _movementInput;

    private bool _isJumpThisFrame;

    private bool _isCurrentlyJumping;
    private GameObject _jumpObject;

    #endregion

    public event Action<PlayerWallRunning> OnWallRunStart;
    public event Action<PlayerWallRunning> OnWallRunEnd;

    #region Getters

    public override InputActionMap InputActionMap => InputManager.Instance.PlayerControls.PlayerMovementWallRunning;

    public bool IsWallRunning => _isWallRunning;

    public bool IsWallRunningLeft => _isWallRunningLeft;

    public bool IsWallRunningRight => _isWallRunningRight;

    #endregion

    #region Initialization Functions

    protected override void CustomAwake()
    {
        // Initialize the wall running objects
        _wallRunningObjects = new HashSet<GameObject>();
    }

    private void Start()
    {
        // Initialize the events
        InitializeEvents();

        // Initialize the input
        InitializeInput();
    }

    private void InitializeEvents()
    {
        // Add the on wall run start event
        OnWallRunStart += PushControls;
        OnWallRunStart += KinematicOnWallRunStart;

        // Add the on wall run end event
        OnWallRunEnd += AutoWallJump;
        OnWallRunEnd += RemoveControls;
        OnWallRunEnd += KinematicOnWallRunEnd;
    }

    private void InitializeInput()
    {
        // Subscribe to the movement input
        InputManager.Instance.PlayerControls.PlayerMovementWallRunning.Move.performed += OnMovePerformed;
        InputManager.Instance.PlayerControls.PlayerMovementWallRunning.Move.canceled += OnMoveCanceled;

        InputManager.Instance.PlayerControls.PlayerMovementWallRunning.Jump.performed += OnJumpPerformed;
    }

    #endregion

    #region Input Functions

    private void OnMovePerformed(InputAction.CallbackContext obj)
    {
        // Get the movement input
        _movementInput = obj.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext obj)
    {
        // Reset the movement input
        _movementInput = Vector2.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext obj)
    {
        // Set the is jump this frame flag to true
        _isJumpThisFrame = true;
    }

    #endregion

    private void Update()
    {
        // reset the wall running if the player is not wall running
        if (_wallRunningObjects.Count <= 0)
            UpdateResetWallRunning();
    }

    public override void FixedMovementUpdate()
    {
        // Update the movement if the player is wall running
        UpdateMovement();
    }

    private void UpdateResetWallRunning()
    {
        // Clear the wall running booleans
        _isWallRunning = false;
        _isWallRunningLeft = false;
        _isWallRunningRight = false;

        // Clear the contact points
        _contactPoints = null;

        // Clear the contact point index
        _contactPointIndex = -1;
    }

    private void UpdateMovement()
    {
        // If the player jumps this frame, jump
        if (_isJumpThisFrame)
        {
            WallJump();
            return;
        }

        // If the player has jumped this frame, return
        if (_isCurrentlyJumping)
            return;

        // Update the wall running movement
        UpdateWallRunningMovement();
    }

    private void UpdateWallRunningMovement()
    {
        // return if the player is not wall running
        if (!_isWallRunning)
            return;

        // Return if the contact point index is out of range
        if (_contactPointIndex < 0 || _contactPointIndex >= _contactPoints.Length)
            return;

        // Get the current contact point
        var contactPoint = _contactPoints[_contactPointIndex];

        // Get the wall running forward direction
        var forwardDirection = _isWallRunningLeft
            ? Vector3.Cross(contactPoint.normal, Vector3.up)
            : Vector3.Cross(Vector3.up, contactPoint.normal);

        // Get the wall running upward direction
        var upwardDirection = _isWallRunningLeft
            ? Vector3.Cross(forwardDirection, _contactPoints[_contactPointIndex].normal)
            : Vector3.Cross(_contactPoints[_contactPointIndex].normal, forwardDirection);

        var upwardMovement = upwardDirection * _movementInput.y;

        // Move the player in the direction of the wall a little bit to prevent them from falling off
        forwardDirection -= contactPoint.normal * 0.05f;

        // Remove the y component of the forward direction
        forwardDirection = new Vector3(forwardDirection.x, 0, forwardDirection.z);

        // Get the move vector
        var moveVector = (forwardDirection + upwardMovement).normalized;

        // Set the player's rotation forward direction
        // ParentComponent.transform.forward = forwardDirection;

        // Get the velocity vector
        var velocityVector = moveVector * ParentComponent.MovementSpeed;

        // Move the player in the forward direction
        ParentComponent.Rigidbody.velocity = velocityVector;

        // // Set the velocity of the rigid body
        // ParentComponent.Rigidbody.linearVelocity = new Vector3(
        //     ParentComponent.Rigidbody.linearVelocity.x,
        //     0,
        //     ParentComponent.Rigidbody.linearVelocity.z
        // );
        // ParentComponent.Rigidbody.CustomAddForce(velocityVector, ForceMode.VelocityChange);
        ApplyLateralSpeedLimit();
    }

    private void OnCollisionStay(Collision other)
    {
        // Return if the collision is not with a wall
        if ((wallLayer.value & (1 << other.gameObject.layer)) == 0)
            return;

        // Get all the contact points
        _contactPoints = new ContactPoint[other.contactCount];
        other.GetContacts(_contactPoints);

        // Get all the contact normals
        var contactNormals = new Vector3[_contactPoints.Length];
        for (var i = 0; i < _contactPoints.Length; i++)
            contactNormals[i] = _contactPoints[i].normal;

        // Get the player's left and right direction vectors
        var playerLeft = -ParentComponent.CameraPivot.transform.right;
        var playerRight = ParentComponent.CameraPivot.transform.right;

        // Get the minimum dot product required to wall run
        // (This is negative on purpose)
        var minimumDot = -1 + wallRunningSensitivity;

        // Create a collection to store the left dot products
        var leftDotProducts = contactNormals.Select(
            normal => Vector3.Dot(playerLeft, normal)
        ).ToArray();

        // Create a collection to store the right dot products
        var rightDotProducts = contactNormals.Select(
            normal => Vector3.Dot(playerRight, normal)
        ).ToArray();

        // Get the index of the smallest left dot product
        var smallestLeftDotIndex = 0;
        for (var i = 0; i < leftDotProducts.Length; i++)
            if (leftDotProducts[i] < leftDotProducts[smallestLeftDotIndex])
                smallestLeftDotIndex = i;

        // Get the index of the smallest right dot product
        var smallestRightDotIndex = 0;
        for (var i = 0; i < rightDotProducts.Length; i++)
            if (rightDotProducts[i] < rightDotProducts[smallestRightDotIndex])
                smallestRightDotIndex = i;

        // Whichever side with the most negative dot product is the side that is wall running
        var leftDotProduct = leftDotProducts[smallestLeftDotIndex];
        var rightDotProduct = rightDotProducts[smallestRightDotIndex];

        // Set the booleans for wall running
        _isWallRunningLeft = leftDotProduct <= minimumDot;
        _isWallRunningRight = rightDotProduct <= minimumDot && !_isWallRunningLeft;
        _isWallRunning = _isWallRunningLeft || _isWallRunningRight;

        if (_isWallRunning && !ParentComponent.IsGrounded && isEnabled)
        {
            // Set the contact point index
            _contactPointIndex = _isWallRunningLeft
                ? smallestLeftDotIndex
                : smallestRightDotIndex;

            // Add the current object to the wall running objects
            AddWallRunningObject(other.gameObject);
        }

        // Otherwise, remove the current object from the wall running objects
        else if (_wallRunningObjects.Contains(other.gameObject))
        {
            Debug.Log("Pop from the wall!");
            RemoveWallRunningObject(other.gameObject);
        }
    }

    private void OnCollisionExit(Collision other)
    {
        // Reset the has jumped this frame flag
        if (other.gameObject == _jumpObject)
        {
            _jumpObject = null;
            _isCurrentlyJumping = false;
        }

        // Remove the current object from the wall running objects
        RemoveWallRunningObject(other.gameObject);
    }

    private void WallJump()
    {
        // Set the is jump this frame flag to true
        _isJumpThisFrame = false;

        // Return if the player is not wall running
        if (!_isWallRunning)
            return;

        // Return if the current contact point index is out of range
        if (_contactPointIndex < 0 || _contactPointIndex >= _contactPoints.Length)
            return;

        // Get the current contact point
        var contactPoint = _contactPoints[_contactPointIndex];

        // Get the normal of the contact point
        var normal = contactPoint.normal;

        var forwardLine = _isWallRunningLeft
            ? Vector3.Cross(contactPoint.normal, Vector3.up)
            : Vector3.Cross(Vector3.up, contactPoint.normal);

        var upwardLine = _isWallRunningLeft
            ? Vector3.Cross(forwardLine, contactPoint.normal)
            : Vector3.Cross(contactPoint.normal, forwardLine);

        var anglePercent = wallJumpAngle / (90 - 0);

        // Get a vector that combines the normal and the world's up vector
        var wallJumpDirection = Vector3.Lerp(normal, upwardLine, anglePercent).normalized;

        // Get the wall jump force
        var wallJumpForceVector = wallJumpDirection * wallJumpForce;

        // Get the wall running object that corresponds to the current contact point
        _jumpObject = contactPoint.otherCollider.gameObject;

        // Remove the current object from the wall running objects
        RemoveWallRunningObject(_jumpObject);

        // Force all the booleans to false
        _isWallRunning = false;
        _isWallRunningLeft = false;
        _isWallRunningRight = false;

        // Add a force to the rigid body
        ParentComponent.Rigidbody.CustomAddForce(wallJumpForceVector, ForceMode.VelocityChange);
        // ApplyLateralSpeedLimit();

        _isCurrentlyJumping = true;
    }

    private void AddWallRunningObject(GameObject wallRunningObject)
    {
        var objectsEmpty = _wallRunningObjects.Count <= 0;

        // Add the wall running object to the wall running objects
        _wallRunningObjects.Add(wallRunningObject);

        // If the wall running objects were empty, invoke the on wall run start event
        if (objectsEmpty)
            OnWallRunStart?.Invoke(this);
    }

    private void RemoveWallRunningObject(GameObject wallRunningObject)
    {
        // Remove the wall running object from the wall running objects
        _wallRunningObjects.Remove(wallRunningObject);

        // If the wall running objects are empty, invoke the on wall run end event
        if (_wallRunningObjects.Count <= 0)
            OnWallRunEnd?.Invoke(this);
    }

    #region Event Functions

    private void KinematicOnWallRunStart(PlayerWallRunning playerWallRunning)
    {
        // // Set the rigid body to kinematic
        // ParentComponent.Rigidbody.isKinematic = true;

        // Disable the player's gravity
        ParentComponent.Rigidbody.useGravity = false;
    }

    private void KinematicOnWallRunEnd(PlayerWallRunning playerWallRunning)
    {
        // // Set the rigid body to non-kinematic
        // ParentComponent.Rigidbody.isKinematic = false;

        // Enable the player's gravity
        ParentComponent.Rigidbody.useGravity = true;
    }

    private void AutoWallJump(PlayerWallRunning obj)
    {
        if (_contactPoints == null)
            return;

        // Get the current contact point
        var contactPoint = _contactPoints[_contactPointIndex];

        // Get the normal of the contact point
        var normal = contactPoint.normal;

        var forwardLine = _isWallRunningLeft
            ? Vector3.Cross(contactPoint.normal, Vector3.up)
            : Vector3.Cross(Vector3.up, contactPoint.normal);

        var upwardLine = _isWallRunningLeft
            ? Vector3.Cross(forwardLine, contactPoint.normal)
            : Vector3.Cross(contactPoint.normal, forwardLine);

        var anglePercent = 10f / 90;
        var wallJumpDirection = Vector3.Lerp(normal, upwardLine, anglePercent).normalized;

        var wallJumpForceVector = wallJumpDirection * autoWallJumpForce;

        ParentComponent.Rigidbody.CustomAddForce(wallJumpForceVector, ForceMode.VelocityChange);
    }

    #endregion

    #region Debugging

    public override string GetDebugText()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Wall Running: {_isWallRunning}");

        if (_isWallRunning)
        {
            var wallRunningDirection = _isWallRunningLeft
                ? "Left"
                : "Right";
            sb.AppendLine($"\tWall Running Direction: {wallRunningDirection}");

            if (_contactPointIndex >= 0 && _contactPointIndex < _contactPoints.Length && _contactPoints != null)
                sb.AppendLine($"\tContact Point: {_contactPoints?[_contactPointIndex].point}");

            sb.AppendLine($"Movement Input: {_movementInput}");
        }

        sb.AppendLine($"Jump this frame: {_isJumpThisFrame}");

        return sb.ToString();
    }

    private void OnDrawGizmos()
    {
        // Return if the player is not wall running
        if (!_isWallRunning)
            return;

        // Return if the contact point index is out of range
        if (_contactPointIndex < 0 || _contactPointIndex >= _contactPoints.Length)
            return;

        const float sphereSize = 0.1f;

        var contactPoint = _contactPoints[_contactPointIndex];

        // Draw a sphere at the contact point
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(contactPoint.point, sphereSize);

        var forwardLine = _isWallRunningLeft
            ? Vector3.Cross(contactPoint.normal, Vector3.up)
            : Vector3.Cross(Vector3.up, contactPoint.normal);

        var upwardLine = _isWallRunningLeft
            ? Vector3.Cross(forwardLine, contactPoint.normal)
            : Vector3.Cross(contactPoint.normal, forwardLine);

        var anglePercent = wallJumpAngle / (90 - 0);

        var jumpLine = Vector3.Lerp(contactPoint.normal, upwardLine, anglePercent);

        const float lineInterval = 0.25f;
        const float lineDistance = 10f;

        for (float i = 0; i < lineDistance; i += lineInterval)
        {
            // Draw a sphere at the new point
            var forwardPoint = contactPoint.point + (forwardLine * i);
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(forwardPoint, sphereSize);


            // Draw a sphere at the new point
            var upwardPoint = contactPoint.point + (upwardLine * i);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(upwardPoint, sphereSize);


            // Draw a sphere at the new point
            var jumpPoint = contactPoint.point + (jumpLine * i);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(jumpPoint, sphereSize);
        }
    }

    #endregion
}