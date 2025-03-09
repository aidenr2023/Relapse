using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NewEnemyBehaviorBrain), typeof(NavMeshAgent))]
public class NewEnemyMovement : ComponentScript<Enemy>
{
    private const float STRAFE_DISTANCE = 2f;
    private const float RANDOM_STRAFE_ANGLE = 25f;

    public delegate void MovementFunction(bool needsToUpdateDestination);


    #region Serilized Fields

    [SerializeField, Range(.0001f, 60)] private float updatesPerSecond = 8f;

    [SerializeField, Min(0)] private float movementSpeed = 14;
    [SerializeField, Range(0, 1)] private float strafeMultiplier = 0.5f;

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

    #endregion

    #region Getters
    
    public NavMeshAgent NavMeshAgent => _navMeshAgent;

    private bool IsStrafing =>
        _currentMoveAction == BehaviorActionMove.MoveAction.StrafeLeft ||
        _currentMoveAction == BehaviorActionMove.MoveAction.StrafeRight ||
        _currentMoveAction == BehaviorActionMove.MoveAction.StrafeForward ||
        _currentMoveAction == BehaviorActionMove.MoveAction.StrafeBackward;

    #endregion

    protected override void CustomAwake()
    {
        _brain = GetComponent<NewEnemyBehaviorBrain>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
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
        if (IsStrafing)
            Strafe();
    }

    private IEnumerator CoroutineUpdate()
    {
        // Wait a frame
        yield return null;

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
        _brain.DistanceFromDestination = _navMeshAgent.remainingDistance;
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

            BehaviorActionMove.MoveAction.StrafeLeft => _navMeshAgent.remainingDistance < strafeUpdateDistance,
            BehaviorActionMove.MoveAction.StrafeRight => _navMeshAgent.remainingDistance < strafeUpdateDistance,
            BehaviorActionMove.MoveAction.StrafeForward => _navMeshAgent.remainingDistance < strafeUpdateDistance,
            BehaviorActionMove.MoveAction.StrafeBackward => _navMeshAgent.remainingDistance < strafeUpdateDistance,

            BehaviorActionMove.MoveAction.MoveTowardTarget => true,
            BehaviorActionMove.MoveAction.MoveAwayFromTarget => true,

            BehaviorActionMove.MoveAction.Wander => _navMeshAgent.remainingDistance < destinationUpdateDistance,
            BehaviorActionMove.MoveAction.MovementScript => true,

            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void DetermineMovementSpeed(BehaviorActionMove.MoveAction moveAction)
    {
        var newSpeed = moveAction switch
        {
            BehaviorActionMove.MoveAction.StrafeLeft => movementSpeed * strafeMultiplier,
            BehaviorActionMove.MoveAction.StrafeRight => movementSpeed * strafeMultiplier,
            BehaviorActionMove.MoveAction.StrafeForward => movementSpeed * strafeMultiplier,
            BehaviorActionMove.MoveAction.StrafeBackward => movementSpeed * strafeMultiplier,
            _ => movementSpeed
        };

        // Set the speed of the nav mesh agent
        _navMeshAgent.speed = newSpeed;
    }

    private void DetermineRotationMode(BehaviorActionMove.MoveAction moveAction)
    {
        _navMeshAgent.updateRotation = (!IsStrafing && moveAction != BehaviorActionMove.MoveAction.Idle);
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
        _brain.MovementBehavior?.StateUpdateMovement(_brain, this, needsToUpdateDestination);
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

    private void Strafe()
    {
        // Set the forward of the transform to the detection target
        if (!ParentComponent.DetectionBehavior.IsTargetDetected)
            return;

        var targetPosition = ParentComponent.DetectionBehavior.Target.GameObject.transform.position;
        var direction = targetPosition - transform.position;
        direction.y = 0;
        transform.forward = direction.normalized;
    }

    private void UpdateIdle(bool needsToUpdateDestination)
    {
    }

    #endregion

    public void SetDestination(Vector3 destination)
    {
        // Set the target position to the destination
        _targetPosition = destination;

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

    public static Vector3 RotateDirectionRandomly(Vector3 direction, float angle = RANDOM_STRAFE_ANGLE)
    {
        // Create a random angle from -angle to angle
        var randomAngle = UnityEngine.Random.Range(-angle, angle);

        var rotation = Quaternion.Euler(0, randomAngle, 0);

        return rotation * direction;
    }
}