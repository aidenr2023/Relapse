using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NewEnemyBehaviorBrain), typeof(NavMeshAgent))]
public class NewEnemyMovement : ComponentScript<Enemy>
{
    private delegate void MovementFunction(bool needsToUpdateDestination);

    private const float STRAFE_DISTANCE = 2f;
    private const float RANDOM_STRAFE_ANGLE = 25f;

    private static readonly int AnimatorIsMovingProperty = Animator.StringToHash("IsMoving");
    private static readonly int AnimatorSpeedProperty = Animator.StringToHash("Speed");
    private static readonly int AnimatorIsRunningProperty = Animator.StringToHash("IsRunning");

    #region Serilized Fields

    [SerializeField, Range(.0001f, 60)] private float updatesPerSecond = 8f;

    [SerializeField, Min(0)] private float movementSpeed = 14;
    [SerializeField, Range(0, 1)] private float strafeMultiplier = 0.5f;
    [SerializeField, Range(0, 1)] private float strafeRotationLerpAmount = 0.075f;

    [Header("Animations")] [SerializeField]
    private Animator animator;

    [SerializeField, Min(0)] private float walkAnimationThreshold;
    [SerializeField, Min(0)] private float runAnimationThreshold;
    [SerializeField, Min(0)] private float animationSpeedCoefficient = 1;

    #endregion

    #region Private Fields

    private Coroutine _coroutineUpdate;

    private NewEnemyBehaviorBrain _brain;
    private NavMeshAgent _navMeshAgent;

    private BehaviorActionMove.MoveAction _currentMoveAction;

    private bool _forceUpdateDestination;
    private Vector3 _targetPosition;
    private float _targetVelocity;

    private readonly HashSet<object> _movementDisableTokens = new();

    private bool _hasStarted;

    #endregion

    #region Getters

    public NavMeshAgent NavMeshAgent => _navMeshAgent;

    public TokenManager<float> MovementSpeedTokens { get; private set; }

    private bool IsStrafing =>
        _currentMoveAction == BehaviorActionMove.MoveAction.StrafeLeft ||
        _currentMoveAction == BehaviorActionMove.MoveAction.StrafeRight ||
        _currentMoveAction == BehaviorActionMove.MoveAction.StrafeForward ||
        _currentMoveAction == BehaviorActionMove.MoveAction.StrafeBackward;

    public float MovementSpeed
    {
        get => movementSpeed;
        set => movementSpeed = value;
    }

    public HashSet<object> RotationDisableTokens { get; } = new();

    #endregion

    protected override void CustomAwake()
    {
        _brain = GetComponent<NewEnemyBehaviorBrain>();
        _navMeshAgent = GetComponent<NavMeshAgent>();

        MovementSpeedTokens = new(false, null, 1);

        // Turn the navmesh agent off
        _navMeshAgent.enabled = false;
    }

    private void Start()
    {
        _hasStarted = true;
    }

    private void OnEnable()
    {
        // Start the coroutine update
        _coroutineUpdate = StartCoroutine(CoroutineUpdate());
    }

    private void OnDisable()
    {
        // Stop the coroutine update
        StopCoroutine(_coroutineUpdate);
        _coroutineUpdate = null;
    }

    private void Update()
    {
        // Update the movement speed tokens
        MovementSpeedTokens.Update(Time.deltaTime);

        // Determine the movement speed
        DetermineMovementSpeed(_brain.CurrentMoveAction.moveAction);

        if (IsStrafing)
            RotateWhileStrafing();

        // Update the animator
        UpdateMovementAnimation();
    }

    private void UpdateMovementAnimation()
    {
        // Return if there is no animator
        if (animator == null)
        {
            Debug.LogError($"{gameObject.name} does not have an animator!", this);
            return;
        }

        // Get the velocity of the NavMeshAgent
        var velocity = _targetVelocity;
        var isMoving = NavMeshAgent.velocity.magnitude > walkAnimationThreshold;
        var isRunning = NavMeshAgent.velocity.magnitude >= runAnimationThreshold;

        var speedValue = velocity * animationSpeedCoefficient;

        // If the navmesh agent is disabled, set the speed value to 0
        if (NavMeshAgent.enabled && NavMeshAgent.isOnNavMesh && NavMeshAgent.isStopped)
            isMoving = false;

        animator.SetBool(AnimatorIsMovingProperty, isMoving);
        animator.SetFloat(AnimatorSpeedProperty, speedValue);
        animator.SetBool(AnimatorIsRunningProperty, isRunning);
    }

    private IEnumerator CoroutineUpdate()
    {
        // Wait until the start method is called
        yield return new WaitUntil(() => _hasStarted);

        // Wait for ANOTHER frame
        yield return null;

        // Make sure the navmesh agent is enabled
        _navMeshAgent.enabled = true;

        while (enabled)
        {
            // Get the current move action
            ChangeMovementState(_brain.CurrentMoveAction.moveAction);

            // Update the movement
            UpdateMovement();

            // Update the variables in the brain
            UpdateBrainVariables();

            yield return new WaitForSeconds(1 / updatesPerSecond);
        }
    }

    private void UpdateBrainVariables()
    {
        var targetPosition = ParentComponent.DetectionBehavior.LastKnownTargetPosition;

        // Update the float variables 
        _brain.DistanceFromTarget = Vector3.Distance(transform.position, targetPosition);
        _brain.DistanceFromDestination = GetRemainingDistance();
        _brain.Speed = _navMeshAgent.velocity.magnitude;
        _brain.HealthPercentage = ParentComponent.EnemyInfo.CurrentHealth / ParentComponent.EnemyInfo.MaxHealth;

        // Update the bool variables
        _brain.IsTargetDetected =
            ParentComponent.DetectionBehavior.CurrentDetectionState != EnemyDetectionState.Unaware;
    }

    private bool DetermineNeedsToUpdateDestination(BehaviorActionMove.MoveAction moveAction)
    {
        const float destinationUpdateDistance = 0.1f;
        const float strafeUpdateDistance = 1.5f;

        return moveAction switch
        {
            BehaviorActionMove.MoveAction.Idle => false,

            BehaviorActionMove.MoveAction.StrafeLeft => GetRemainingDistance() < strafeUpdateDistance,
            BehaviorActionMove.MoveAction.StrafeRight => GetRemainingDistance() < strafeUpdateDistance,
            BehaviorActionMove.MoveAction.StrafeForward => GetRemainingDistance() < strafeUpdateDistance,
            BehaviorActionMove.MoveAction.StrafeBackward => GetRemainingDistance() < strafeUpdateDistance,

            BehaviorActionMove.MoveAction.MoveTowardTarget => true,
            BehaviorActionMove.MoveAction.MoveAwayFromTarget => true,

            BehaviorActionMove.MoveAction.Wander => GetRemainingDistance() < destinationUpdateDistance,
            BehaviorActionMove.MoveAction.MovementScript => true,

            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void DetermineMovementSpeed(BehaviorActionMove.MoveAction moveAction)
    {
        // Based on the current movement state, determine the speed
        var stateSpeed = moveAction switch
        {
            BehaviorActionMove.MoveAction.StrafeLeft => movementSpeed * strafeMultiplier,
            BehaviorActionMove.MoveAction.StrafeRight => movementSpeed * strafeMultiplier,
            BehaviorActionMove.MoveAction.StrafeForward => movementSpeed * strafeMultiplier,
            BehaviorActionMove.MoveAction.StrafeBackward => movementSpeed * strafeMultiplier,
            _ => movementSpeed
        };

        var movementEnabledSpeed = _movementDisableTokens.Count > 0 ? 0 : 1;

        // Calculate the target velocity
        _targetVelocity = stateSpeed * GetMovementSpeedTokenMultiplier() * movementEnabledSpeed;

        // Set the speed of the nav mesh agent
        _navMeshAgent.speed = _targetVelocity;
    }

    private void DetermineRotationMode(BehaviorActionMove.MoveAction moveAction)
    {
        _navMeshAgent.updateRotation = !(
            IsStrafing ||
            moveAction == BehaviorActionMove.MoveAction.Idle ||
            RotationDisableTokens.Count == 0
        );
    }

    private MovementFunction DetermineUpdateFunction(BehaviorActionMove.MoveAction moveAction)
    {
        return moveAction switch
        {
            BehaviorActionMove.MoveAction.Idle => UpdateIdle,
            BehaviorActionMove.MoveAction.StrafeLeft => UpdateStrafeLeft,
            BehaviorActionMove.MoveAction.StrafeRight => UpdateStrafeRight,
            BehaviorActionMove.MoveAction.StrafeForward => UpdateStrafeForward,
            BehaviorActionMove.MoveAction.StrafeBackward => UpdateStrafeBackward,

            BehaviorActionMove.MoveAction.MoveTowardTarget => UpdateMoveTowardTarget,
            BehaviorActionMove.MoveAction.MoveAwayFromTarget => UpdateMoveAwayFromTarget,

            BehaviorActionMove.MoveAction.Wander => UpdateWander,
            BehaviorActionMove.MoveAction.MovementScript => UpdateMovementScript,

            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void UpdateMovement()
    {
        // Determine if the destination needs to be updated
        var needsToUpdateDestination = _forceUpdateDestination || DetermineNeedsToUpdateDestination(_currentMoveAction);

        // Determine the update function
        var updateFunction = DetermineUpdateFunction(_brain.CurrentMoveAction.moveAction);

        // Determine the movement speed
        DetermineMovementSpeed(_brain.CurrentMoveAction.moveAction);

        // Determine the rotation mode
        DetermineRotationMode(_brain.CurrentMoveAction.moveAction);

        // Call the corresponding update function
        updateFunction(needsToUpdateDestination);

        // Reset the force update destination
        _forceUpdateDestination = false;
    }

    #region Update Functions

    private void UpdateMovementScript(bool needsToUpdateDestination)
    {
        if (_brain.MovementBehavior != null)
            _brain.MovementBehavior.StateUpdateMovement(_brain, this, needsToUpdateDestination);
        else
        {
            Debug.LogError("The movement behavior is null!", this);
            UpdateIdle(needsToUpdateDestination);
        }
    }

    private void UpdateWander(bool needsToUpdateDestination)
    {
        if (needsToUpdateDestination)
        {
            const float wanderRadius = 10f;

            const int sampleTries = 5;

            // Keep trying to sample the navmesh for a valid random position
            for (var i = 0; i < sampleTries; i++)
            {
                var randomDirection = UnityEngine.Random.rotation.eulerAngles;
                randomDirection.y = 0;

                var randomPosition = transform.position +
                                     Quaternion.Euler(randomDirection) * Vector3.forward * wanderRadius;

                // Set the destination to the random position

                // Sample the navmesh for a random position
                NavMesh.SamplePosition(randomPosition, out var hitInfo, wanderRadius, NavMesh.AllAreas);

                // Retry if the position is not on the navmesh
                if (!hitInfo.hit)
                    continue;

                // Set the destination to the hit position
                SetDestination(hitInfo.position);

                break;
            }
        }
    }

    private void UpdateMoveAwayFromTarget(bool needsToUpdateDestination)
    {
    }

    private void UpdateMoveTowardTarget(bool needsToUpdateDestination)
    {
        if (needsToUpdateDestination)
        {
            // Set the destination to the forward of the transform
            SetDestination(ParentComponent.DetectionBehavior.LastKnownTargetPosition);
        }
    }

    private void UpdateStrafeBackward(bool needsToUpdateDestination)
    {
        if (needsToUpdateDestination)
        {
            // Get the forward direction of the transform
            var forward = RotateDirectionRandomly(transform.forward);

            // Set the destination to the forward of the transform
            SetDestination(transform.position + -forward * STRAFE_DISTANCE);
        }
    }

    private void UpdateStrafeForward(bool needsToUpdateDestination)
    {
        if (needsToUpdateDestination)
        {
            // Get the forward direction of the transform
            var forward = RotateDirectionRandomly(transform.forward);

            // Set the destination to the forward of the transform
            SetDestination(transform.position + forward * STRAFE_DISTANCE);
        }
    }

    private void UpdateStrafeRight(bool needsToUpdateDestination)
    {
        if (needsToUpdateDestination)
        {
            // Get the right direction of the transform
            var right = RotateDirectionRandomly(transform.right);

            // Set the destination to the right of the transform
            SetDestination(transform.position + right * STRAFE_DISTANCE);
        }
    }

    private void UpdateStrafeLeft(bool needsToUpdateDestination)
    {
        if (needsToUpdateDestination)
        {
            // Get the right direction of the transform
            var right = RotateDirectionRandomly(transform.right);

            // Set the destination to the right of the transform
            SetDestination(transform.position + -right * STRAFE_DISTANCE);
        }
    }

    private void RotateWhileStrafing()
    {
        // Set the forward of the transform to the detection target
        if (!ParentComponent.DetectionBehavior.IsTargetDetected)
            return;

        // Get the current rotation of the forward vector
        var currentRotation = transform.rotation;

        // Get the desired rotation of the forward vector
        var difference = ParentComponent.DetectionBehavior.LastKnownTargetPosition - transform.position;
        var desiredRotation = Quaternion.LookRotation(difference, Vector3.up);

        // Rotate the forward of the transform towards the target forward
        var newRotation = Quaternion.Lerp(currentRotation, desiredRotation,
            CustomFunctions.FrameAmount(strafeRotationLerpAmount)
        );

        // Create a new rotation WITHOUT a rotation around the x or z axis
        var newRotationNoXZ = Quaternion.Euler(0, newRotation.eulerAngles.y, 0);

        // Set the rotation of the transform
        transform.rotation = newRotationNoXZ;
    }

    private void UpdateIdle(bool needsToUpdateDestination)
    {
        // Do nothing
    }

    #endregion

    public void SetDestination(Vector3 destination)
    {
        // Set the target position to the destination
        _targetPosition = destination;

        // Return if the agent is disabled OR
        // return if the agent is not on the navmesh
        if (!NavMeshAgent.enabled || !NavMeshAgent.isOnNavMesh)
            return;

        // Set the destination of the nav mesh agent
        _navMeshAgent.SetDestination(_targetPosition);
    }

    private void ChangeMovementState(BehaviorActionMove.MoveAction moveAction)
    {
        // Return if the move action is the same as the current move action
        if (moveAction == _currentMoveAction)
            return;

        _currentMoveAction = moveAction;

        // Set the force update destination to true
        _forceUpdateDestination = true;
    }

    private float GetMovementSpeedTokenMultiplier()
    {
        var multiplier = 1f;

        foreach (var speedToken in MovementSpeedTokens.Tokens)
            multiplier *= speedToken.Value;

        return multiplier;
    }

    public void AddMovementDisableToken(object token)
    {
        _movementDisableTokens.Add(token);
    }

    public void RemoveMovementDisableToken(object token)
    {
        _movementDisableTokens.Remove(token);
    }

    public void SetPosition(Vector3 pos)
    {
        // Warp the nav mesh agent to the position
        _navMeshAgent.Warp(pos);
    }

    public float GetRemainingDistance()
    {
        // Return if the nav mesh agent is disabled OR
        // return if the agent is not on the navmesh
        if (!NavMeshAgent.enabled || !NavMeshAgent.isOnNavMesh)
            return 0;

        return _navMeshAgent.remainingDistance;
    }

    public static Vector3 RotateDirectionRandomly(Vector3 direction, float angle = RANDOM_STRAFE_ANGLE)
    {
        // Create a random angle from -angle to angle
        var randomAngle = UnityEngine.Random.Range(-angle, angle);

        var rotation = Quaternion.Euler(0, randomAngle, 0);

        return rotation * direction;
    }
}