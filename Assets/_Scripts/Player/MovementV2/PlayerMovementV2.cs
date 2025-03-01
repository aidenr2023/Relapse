using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerMovementV2 : ComponentScript<Player>, IPlayerController, IDebugged, IUsesInput
{
    private const int MAX_SPHERECAST_HITS = 128;

    private static readonly int IsRunningAnimationID = Animator.StringToHash("IsSprinting");

    #region Serialized Fields

    [Header("Important Transforms")]

    // Reference to the player's camera pivot
    [SerializeField]
    private Transform cameraPivot;

    // Reference to the player's orientation transform
    [SerializeField] private Transform orientation;

    //Reference to player animator
    [SerializeField] Animator playerAnimator;

    [Header("Floating Controller")] [SerializeField, Min(0)]
    private float desiredCapsuleHeightOffset = .25f;

    [SerializeField, Min(1), Delayed] private float defaultPlayerHeight = 2;

    [SerializeField] private float rideSpringStrength;
    [SerializeField] private float rideSpringDamper;

    [Space, SerializeField, Min(0.0001f)] private float downwardInterpolation = 1;
    [SerializeField] private float maxForceAdjust = .25f;
    [SerializeField, Min(0.0001f)] private float maxDownwardAngle = 35;
    [SerializeField, Min(0.0001f)] private float velocityInterpolation = 1;
    [SerializeField, Min(0)] private float slideInterpolationMultiplier = 10;
    [SerializeField, Range(0, 1)] private float sphereCastRadius = .9f;
    [SerializeField, Range(0, 90)] private float maxSlopeAngle = 60;
    
    [SerializeField] private PlayerMovementTypeSettings cityMovementSettings;
    [SerializeField] private PlayerMovementTypeSettings apartmentMovementSettings;

    [Header("Locomotion")] [SerializeField]
    private float maxSpeed = 10;

    [SerializeField] private float acceleration = 200;
    [SerializeField] private AnimationCurve accelerationFactorFromDot;

    [SerializeField, Min(0)] private float hardSpeedLimit = 30;
    [SerializeField, Range(0, 1)] private float hardSpeedLimitLerpAmount = .1f;

    [SerializeField] private LayerMask layersToIgnore;

    [Space, SerializeField] private bool staminaDrains = true;
    [SerializeField, Min(0)] private float maxStamina = 100;
    [SerializeField, Min(0)] private float staminaRegenRate = 20f;
    [SerializeField, Min(0)] private float sprintStaminaDrainRate = 10f;
    [SerializeField, Min(0)] private float staminaRegenDelay = .5f;

    [SerializeField, Range(0, 1)] private float relapseSpeedMult = .25f;

    [SerializeField, Range(0, 1)] private float enemyNearbySpeedMult = 1;

    [Header("Bullet Time")] [SerializeField, Range(0, 1)]
    private float bulletTimeSpeedMult = .5f;

    #endregion

    #region Private Fields

    private Rigidbody _rigidbody;

    private CustomStack<PlayerMovementScript> _movementScripts;

    private InputActionMap _currentActionMap;

    private RaycastHit _floatingControllerHit;
    private readonly RaycastHit[] _floatingControllerHits = new RaycastHit[MAX_SPHERECAST_HITS];

    private CapsuleCollider _capsuleCollider;

    private bool _isSprinting;

    private bool _wasPreviouslySprinting;

    private bool _wasPreviouslyGrounded;

    private Vector3 _landVelocity;

    private float _currentPlayerHeight;
    // private float _rideHeight = .5f;

    private float _currentStamina;

    private CountdownTimer _staminaRegenDelayTimer;

    private float _midAirTime;

    private TokenManager<float>.ManagedToken _bulletTimeToken;

    #endregion

    #region Getters

    public GameObject CameraPivot => cameraPivot.gameObject;

    public Transform Orientation => orientation;

    public Rigidbody Rigidbody => _rigidbody;

    public Vector2 MovementInput => BasicPlayerMovement.MovementInput;

    public bool IsGrounded { get; private set; }

    public RaycastHit GroundHit => _floatingControllerHit;

    public AnimationCurve AccelerationFactorFromDot => accelerationFactorFromDot;

    public Animator PlayerAnimator => playerAnimator;

    public float Acceleration => acceleration;

    public bool IsSprintToggled { get; set; }

    public bool IsSprinting => (_isSprinting || IsSprintToggled) && MovementInput.magnitude > 0.25f;

    public float MovementSpeed =>
        maxSpeed *
        (ParentComponent.PlayerInfo.IsRelapsing ? relapseSpeedMult : 1) *
        (ParentComponent.AreEnemiesNearby ? enemyNearbySpeedMult : 1);

    public float HardSpeedLimit => hardSpeedLimit;

    public float HardSpeedLimitLerpAmount => hardSpeedLimitLerpAmount;

    public float DefaultPlayerHeight => defaultPlayerHeight;

    public float TargetPlayerHeight { get; set; }

    public PlayerMovementScript CurrentMovementScript => _movementScripts.Peek();

    public BasicPlayerMovement BasicPlayerMovement { get; private set; }

    public PlayerWallRunning WallRunning { get; private set; }

    public PlayerDash Dash { get; private set; }

    public PlayerSlide PlayerSlide { get; private set; }

    public Vector3 GroundCollisionForward =>
        Vector3.Cross(_floatingControllerHit.normal, -CameraPivot.transform.right).normalized;

    public Vector3 GroundCollisionRight =>
        Vector3.Cross(_floatingControllerHit.normal, CameraPivot.transform.forward).normalized;

    public HashSet<InputData> InputActions { get; } = new();

    public float CurrentStamina => _currentStamina;

    public float MaxStamina => maxStamina;

    public float StaminaRegenRate => staminaRegenRate;

    public float MidAirTime => _midAirTime;

    private bool CurrentlyUsesStamina => staminaDrains && ParentComponent.AreEnemiesNearby;

    #endregion

    public event Action OnSprintStart;
    public event Action OnSprintEnd;

    public event Action<Vector3> OnLand;

    protected override void CustomAwake()
    {
        // Get the components
        InitializeComponents();

        // Initialize the input
        InitializeInput();

        TargetPlayerHeight = defaultPlayerHeight;
        _currentPlayerHeight = defaultPlayerHeight;

        // Set the current stamina to the max stamina
        _currentStamina = maxStamina;

        // Create the stamina regen delay timer
        _staminaRegenDelayTimer = new CountdownTimer(staminaRegenDelay);
        _staminaRegenDelayTimer.Start();
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

    private void InitializeComponents()
    {
        // Get the capsule collider
        _capsuleCollider = GetComponent<CapsuleCollider>();

        // Get the rigid body
        _rigidbody = GetComponent<Rigidbody>();

        // Get the basic player movement
        BasicPlayerMovement = GetComponent<BasicPlayerMovement>();

        // Get the wall running component
        WallRunning = GetComponent<PlayerWallRunning>();

        // Get the dash component
        Dash = GetComponent<PlayerDash>();

        // Get the slide component
        PlayerSlide = GetComponent<PlayerSlide>();
    }

    private void Start()
    {
        // Add this object to the debug manager
        DebugManager.Instance.AddDebuggedObject(this);

        // Create the bullet time token
        _bulletTimeToken = TimeScaleManager.Instance.TimeScaleTokenManager.AddToken(1, -1, true);

        // Create the movement scripts stack
        _movementScripts = new CustomStack<PlayerMovementScript>();

        // Push the basic player movement onto the stack
        PushMovementScript(BasicPlayerMovement);

        // Enable the top most input map
        EnableTopMostInputMap();
    }

    private void OnDestroy()
    {
        // Remove this object from the debug manager
        DebugManager.Instance.RemoveDebuggedObject(this);
    }

    public void InitializeInput()
    {
        // InputActions.Add(new InputData(
        //     InputManager.Instance.PControls.Player.Sprint, InputType.Performed, OnSprintPerformed)
        // );
        // InputActions.Add(new InputData(
        //     InputManager.Instance.PControls.Player.Sprint, InputType.Canceled, OnSprintCanceled)
        // );
        InputActions.Add(new InputData(
            InputManager.Instance.PControls.Player.Sprint, InputType.Performed, OnSprintTogglePerformed)
        );

        InputActions.Add(new InputData(
            InputManager.Instance.PControls.Player.SprintToggle, InputType.Performed,
            OnSprintTogglePerformed)
        );
    }

    #region Update Functions

    private void Update()
    {
        // Update the stamina
        UpdateStamina();

        if (IsSprinting &&
            (
                CurrentStamina <= 0 ||
                MovementInput.magnitude < .125f ||
                !BasicPlayerMovement.CanSprint ||
                Rigidbody.velocity.magnitude < 1f
            ) &&
            CurrentMovementScript == BasicPlayerMovement &&
            IsGrounded
           )
        {
            _isSprinting = false;
            IsSprintToggled = false;
        }

        // Sprint events
        if (IsSprinting && !_wasPreviouslySprinting)
        {
            OnSprintStart?.Invoke();

            playerAnimator.SetBool(IsRunningAnimationID, true);
        }
        else if (!IsSprinting && _wasPreviouslySprinting)
        {
            OnSprintEnd?.Invoke();
            playerAnimator.SetBool(IsRunningAnimationID, false);
        }
    }

    private void LateUpdate()
    {
        // Update the previous sprinting flag
        _wasPreviouslySprinting = IsSprinting;
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

        // Get the y velocity of the player
        _landVelocity = _rigidbody.velocity;
        // _landYVelocity = _rigidbody.velocity.y;

        if (!IsGrounded)
            _midAirTime += Time.fixedDeltaTime;
        else
            _midAirTime = 0;

        // Update the bullet time
        UpdateBulletTime();
    }

    private void UpdateBulletTime()
    {
        // Determine the desired amount of bullet time
        float desiredAmount = 1;

        if (ParentComponent.AreEnemiesNearby && !IsGrounded)
            desiredAmount = bulletTimeSpeedMult;

        // Set the bullet time token value
        _bulletTimeToken.Value =
            Mathf.Lerp(_bulletTimeToken.Value, desiredAmount, CustomFunctions.FrameAmount(.1f, true, true));

        if (Mathf.Abs(_bulletTimeToken.Value - desiredAmount) < .001f)
            _bulletTimeToken.Value = desiredAmount;
    }

    private void UpdateGroundCheck()
    {
        // Create a layer mask that includes everything but the actor layer and NonPhysical
        var layerMask = ~layersToIgnore;

        // // Perform a sphere cast to check if the player is grounded
        // var sphereRadius = _capsuleCollider.radius * sphereCastRadius;
        //
        // var hit = Physics.SphereCast(
        //     transform.position + ((_capsuleCollider.height / 2 + sphereRadius) * Vector3.up),
        //     // transform.position + (Vector3.up * sphereRadius),
        //     // transform.position + (_capsuleCollider.height / 2 * Vector3.up), 
        //     sphereRadius,
        //     Vector3.down,
        //     out _floatingControllerHit,
        //     // _rideHeight,
        //     // _rideHeight + (_capsuleCollider.height / 2 * Vector3.up).y,
        //     TargetPlayerHeight - sphereRadius,
        //     layerMask
        // );

        var hit = GroundCast(layerMask);

        // Do a backup raycast if the sphere cast fails. just to make sure
        var backupHit = Physics.Raycast(
            transform.position + ((_capsuleCollider.height / 2) * Vector3.up),
            Vector3.down,
            out var backupHitInfo,
            // _rideHeight + (_capsuleCollider.height / 2 * Vector3.up).y,
            TargetPlayerHeight,
            layerMask
        );

        var cHit = hit;

        // Reset the capsule collider height if the player is not grounded
        if (!hit && !backupHit)
            _floatingControllerHit = new RaycastHit();

        // If the sphere cast fails, but the raycast succeeds, set the floating controller hit to the raycast hit
        if (!hit && backupHit)
        {
            _floatingControllerHit = backupHitInfo;
            cHit = true;
        }

        // Set the grounded state to the hit state
        IsGrounded = cHit;

        // Handle the code for when the player lands
        // Invoke the on land event
        if (IsGrounded && !_wasPreviouslyGrounded)
            OnLand?.Invoke(_landVelocity);

        // Set the was previously grounded state
        _wasPreviouslyGrounded = IsGrounded;
    }

    private bool IsValidHit(RaycastHit hit)
    {
        var normal = hit.normal;
        var hitAngle = Vector3.Angle(Vector3.up, normal);

        var hitStepHeight = _currentPlayerHeight - hit.distance;

        // Debug.Log($"Hit Step Height: {hitStepHeight:0.00} ({hit.distance:0.00})");

        // if (hitStepHeight > 0.25f)
        //     return false;

        // Return if the hit's collider is a trigger
        if (hit.collider.isTrigger)
            return false;

        // Return if the angle between the normal and the ground is more than the max slope angle
        if (hitAngle > maxSlopeAngle)
            return false;

        return true;
    }

    private bool GroundCast(LayerMask layerMask)
    {
        // Perform a sphere cast to check if the player is grounded
        var sphereRadius = _capsuleCollider.radius * sphereCastRadius;

        // var hit = Physics.SphereCast(
        //     transform.position + ((_capsuleCollider.height / 2 + sphereRadius) * Vector3.up),
        //     // transform.position + (Vector3.up * sphereRadius),
        //     // transform.position + (_capsuleCollider.height / 2 * Vector3.up), 
        //     sphereRadius,
        //     Vector3.down,
        //     out _floatingControllerHit,
        //     // _rideHeight,
        //     // _rideHeight + (_capsuleCollider.height / 2 * Vector3.up).y,
        //     TargetPlayerHeight - sphereRadius,
        //     layerMask
        // );

        // Physics.SphereCastAll(
        //     transform.position + ((_capsuleCollider.height / 2 + sphereRadius) * Vector3.up),
        //     sphereRadius,
        //     Vector3.down,
        //     TargetPlayerHeight - sphereRadius,
        //     layerMask
        // );

        var hitCount = Physics.SphereCastNonAlloc(
            transform.position + ((_capsuleCollider.height / 2 + sphereRadius) * Vector3.up),
            sphereRadius,
            Vector3.down,
            _floatingControllerHits,
            TargetPlayerHeight - sphereRadius + .01f,
            // TargetPlayerHeight,
            layerMask
        );

        // Debug.Log($"Hit Count: {hitCount}");

        // Find the first valid hit
        for (var i = 0; i < hitCount; i++)
        {
            var hit = _floatingControllerHits[i];

            if (!IsValidHit(hit))
                continue;

            // var normal = hit.normal;
            // var hitAngle = Vector3.Angle(Vector3.up, normal);
            //
            // // // If the angle between the normal and the ground is more than the max slope angle, continue
            // // if (hitAngle > maxSlopeAngle)
            // // {
            // //     // continue;
            // //     return false;
            // // }

            // Debug.Log($"Hit Angle: {hitAngle:0.00}");

            _floatingControllerHit = hit;
            return true;
        }

        return false;
    }

    private void UpdateSpringForce()
    {
        // Return if the floating controller hit is not set
        if (!_floatingControllerHit.collider)
            return;

        // Return if the player is trying to jump
        if (BasicPlayerMovement.IsTryingToJump)
            return;

        // Return if the player is set to jump
        if (BasicPlayerMovement.IsSetToJump)
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

        var distance = _floatingControllerHit.distance - _capsuleCollider.height / 2;

        // Distance of the ray hit vs the ride height
        // var rideHeightPenetration = distance - _rideHeight;
        var rideHeightPenetration = distance - (TargetPlayerHeight / 2);

        // var remainingHeight = _currentPlayerHeight - _capsuleCollider.height;
        // var capsuleHeightOffset = _rideHeight - remainingHeight;
        // var hitOffset = distance - capsuleHeightOffset;
        // var actualPenetration = remainingHeight - hitOffset;

        // // Create a target velocity with the same x and z,
        // // but enough velocity to go up by the actual penetration
        // var targetVelocity = new Vector3(
        //     currentVelocity.x,
        //     actualPenetration,
        //     currentVelocity.z
        // );

        // // This is the force required to reach the target velocity in EXACTLY one frame
        // var targetForce = (targetVelocity - _rigidbody.velocity) / Time.fixedDeltaTime;

        // Get the spring force
        var springForce = (rideHeightPenetration * rideSpringStrength) - (relativeVelocity * rideSpringDamper);

        // Debug.Log($"RHP: [{hitOffset:0.000}] [{actualPenetration:0.000}] [{targetForce.y:0.000}]");

        var downwardSlopeForceAdjust = 1f;

        // var groundVelocityForward = GroundCollisionForward;
        var groundVelocityForward = (GroundCollisionForward + GroundCollisionRight).normalized;

        if (Vector3.Dot(currentVelocity, groundVelocityForward) < 0)
            groundVelocityForward *= -1;

        // If the player is on a slope, and they are going down, reduce the force
        if (groundVelocityForward.y < 0)
        {
            // Get the downward angle of the slope
            // var downwardAngle = Vector3.Angle(
            //     new Vector3(groundVelocityForward.x, 0, groundVelocityForward.z),
            //     groundVelocityForward
            // );

            // var downwardAngle = 90 - Vector3.Angle(
            //     new Vector3(GroundHit.normal.x, 0, GroundHit.normal.z).normalized,
            //     new Vector3(GroundHit.normal.x, -GroundHit.normal.y, GroundHit.normal.z).normalized
            // );

            var downwardAngle = 90 - Quaternion.LookRotation(
                new Vector3(GroundHit.normal.x, -GroundHit.normal.y, GroundHit.normal.z)
            ).eulerAngles.x;

            // Get the player's velocity in relation to the slope
            var playerSlopeVelocity = Vector3.Dot(currentVelocity, groundVelocityForward.normalized);

            const float maxVelocity = 8;

            var velocityFactor = playerSlopeVelocity / maxVelocity;

            var interpolation = LogarithmicInterpolation(downwardAngle / maxDownwardAngle, downwardInterpolation);

            // TODO:
            if (PlayerSlide.IsSliding)
                interpolation *= slideInterpolationMultiplier;

            velocityFactor = LogarithmicInterpolation(velocityFactor, velocityInterpolation);

            downwardSlopeForceAdjust = Mathf.Lerp(1, maxForceAdjust, interpolation * velocityFactor);

            // Debug.Log(
            //     $"DOWNWARD ANGLE: " +
            //     $"{downwardAngle:0.00} => " +
            //     $"{interpolation:0.00} => " +
            //     $"{velocityFactor:0.00} => " +
            //     $"{downwardSlopeForceAdjust:0.00}"
            // );
        }

        // The amount of force applied to the player this frame
        var force = springForce * downwardSlopeForceAdjust;

        // // Ensure the force + the upward velocity is not more than ground hit's penetration
        // if (force + currentVelocity.y > actualPenetration)
        //     force = actualPenetration - currentVelocity.y;

        // Add force to the player
        _rigidbody.AddForce(downDirection * force, ForceMode.Acceleration);

        // Debug.Log($"FORCE: {downDirection * springForce}");

        // Add force to the other object
        if (otherRigidbody)
            otherRigidbody.AddForceAtPosition(-downDirection * springForce, _floatingControllerHit.point);
        return;

        float LogarithmicInterpolation(float x, float constA) => Mathf.Log(constA * Mathf.Clamp01(x) + 1, constA + 1);
    }

    private void UpdateCapsuleColliderHeight()
    {
        var oldPlayerHeight = _currentPlayerHeight;

        // _currentPlayerHeight = Mathf.Lerp(_currentPlayerHeight, TargetPlayerHeight, frameAmount * .25f);
        _currentPlayerHeight =
            Mathf.Lerp(_currentPlayerHeight, TargetPlayerHeight, CustomFunctions.FrameAmount(.15f, true));

        if (Mathf.Abs(_currentPlayerHeight - TargetPlayerHeight) < .001f)
            _currentPlayerHeight = TargetPlayerHeight;

        // _rideHeight = _currentPlayerHeight / 2;
        var desiredCapsuleHeight = _currentPlayerHeight - desiredCapsuleHeightOffset;

        // Debug.Log($"_currentPlayerHeight: {_currentPlayerHeight:0.00} => {TargetPlayerHeight:0.00}");

        // Change the y of the transform based on the difference between the old and new player height
        var yDifference = _currentPlayerHeight - oldPlayerHeight;

        if (yDifference > 0)
            yDifference = 0;

        Rigidbody.MovePosition(Rigidbody.position + new Vector3(0, yDifference, 0));

        // Get the distance from the player to the floating controller hit
        var distance = _floatingControllerHit.distance;

        // TODO: Does this line work?
        distance -= _capsuleCollider.height / 2;

        var heightAdjust = desiredCapsuleHeight - ((desiredCapsuleHeight / 2) - distance);

        // Set the capsule collider height based on the distance
        var newCapsuleHeight = Mathf.Min(desiredCapsuleHeight, heightAdjust);
        _capsuleCollider.height = Mathf.Max(newCapsuleHeight, 1);

        // Set the capsule collider y position based on the distance
        var newYPosition = (_currentPlayerHeight - newCapsuleHeight) / 2;
        _capsuleCollider.center = new Vector3(0, newYPosition, 0);
    }

    private void UpdateStamina()
    {
        // Update the stamina regen delay timer
        _staminaRegenDelayTimer.Update(Time.deltaTime);

        // // If this is the active movement script, the player is on the ground, and they are sprinting,
        // // drain the stamina
        // if (CurrentMovementScript != BasicPlayerMovement)
        //     return;

        if (IsSprinting && ((IsGrounded && CurrentMovementScript == BasicPlayerMovement) || WallRunning.IsWallRunning))
            ChangeStamina(-sprintStaminaDrainRate * Time.deltaTime);

        // if (IsGrounded)
        else
        {
            // If the player is not sprinting, regenerate the stamina
            if (_staminaRegenDelayTimer.IsComplete)
                ChangeStamina(StaminaRegenRate * Time.deltaTime);
        }
        // else if (IsSprinting)
        //     ChangeStamina(-sprintStaminaDrainRate / 2 * Time.deltaTime);
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
        sb.AppendLine($"\tStamina: {_currentStamina:0.00} / {maxStamina:0.00}");

        sb.AppendLine($"\tAll Movement Scripts: {string.Join(", ", _movementScripts.Select(n => n.GetType().Name))}");

        return sb.ToString();
    }

    private void OnDrawGizmos()
    {
        // Draw the forward vector of the orientation
        Gizmos.color = Color.red;
        Gizmos.DrawRay(orientation.position, orientation.forward * 3);

        // // Draw the floating controller ray
        // Gizmos.color = Color.red;
        // Gizmos.DrawRay(
        //     transform.position,
        //     Vector3.down * _rideHeight
        // );

        if (_floatingControllerHit.collider)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(_floatingControllerHit.point, .125f);
        }

        if (_capsuleCollider != null)
        {
            Gizmos.color = Color.blue;

            // Draw a sphere at the top of the capsule
            Gizmos.DrawSphere(
                transform.position + _capsuleCollider.center + (_capsuleCollider.height / 2 * Vector3.up),
                _capsuleCollider.radius
            );

            // Draw a ray for the cast
            Gizmos.DrawRay(
                transform.position + _capsuleCollider.center + (_capsuleCollider.height / 2 * Vector3.up),
                // Vector3.down * (_rideHeight + (_capsuleCollider.height / 2 * Vector3.up).y)
                Vector3.down * TargetPlayerHeight
            );
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
        //set animator isMoving to true

        // if (playerAnimator != null)
    }

    private void OnSprintCanceled(InputAction.CallbackContext obj)
    {
        // Set the sprinting flag to false
        _isSprinting = false;

        //if (playerAnimator != null)
    }


    private void OnSprintTogglePerformed(InputAction.CallbackContext obj)
    {
        // Return if the player cannot sprint
        if (!BasicPlayerMovement.CanSprint)
        {
            IsSprintToggled = false;
            return;
        }

        // If the sprint toggle flag is already on, return
        if (IsSprintToggled && !IsSprinting)
        {
            IsSprintToggled = false;
            return;
        }

        // Set the sprinting flag to true
        IsSprintToggled = true;

        Debug.Log($"Sprint Toggle Performed: {IsSprintToggled}");
    }

    #endregion

    public void ForceSetSprinting(bool sprinting)
    {
        _isSprinting = sprinting;
    }

    public void ChangeStamina(float amount)
    {
        // Don't drain the stamina if the stamina drains flag is false
        if (!staminaDrains && amount < 0)
        {
            _currentStamina = maxStamina;
            return;
        }

        if (amount < 0 && !CurrentlyUsesStamina)
            amount = 0;

        if (amount < 0)
        {
            // Reset the stamina regen delay timer
            _staminaRegenDelayTimer.SetMaxTimeAndReset(staminaRegenDelay);
        }

        _currentStamina = Mathf.Clamp(_currentStamina + amount, 0, maxStamina);
    }

    public void SetUpStamina(float cStamina, float mStamina)
    {
        _currentStamina = cStamina;
        maxStamina = mStamina;
    }

    public void DisablePlayerControls()
    {
        InputManager.Instance.IsExternallyDisabled = true;
        Debug.Log("Player Controls Disabled");
    }

    public void EnablePlayerControls()
    {
        InputManager.Instance.IsExternallyDisabled = false;
        Debug.Log("Player Controls Enabled");
    }

    public void ApplyMovementTypeSettings(PlayerMovementType movementType)
    {
        Debug.Log($"Applying settings for {movementType}");
        
        // Determine which settings to use
        var cSettings = movementType switch
        {
            PlayerMovementType.City => cityMovementSettings,
            PlayerMovementType.Apartment => apartmentMovementSettings,
            _ => throw new ArgumentOutOfRangeException(nameof(movementType), movementType, null)
        };
        
        // Apply the settings
        desiredCapsuleHeightOffset = cSettings.desiredCapsuleHeightOffset;
        sphereCastRadius = cSettings.sphereCastRadius;
    }

    [Serializable]
    private struct PlayerMovementTypeSettings
    {
        public float desiredCapsuleHeightOffset;
        [Range(0, 1)] public float sphereCastRadius;
    }
}