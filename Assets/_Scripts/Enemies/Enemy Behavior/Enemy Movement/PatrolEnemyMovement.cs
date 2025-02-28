using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class PatrolEnemyMovement : MonoBehaviour, IEnemyMovementBehavior
{
    private static readonly int AnimatorIsMovingProperty = Animator.StringToHash("IsMoving");
    private static readonly int AnimatorSpeedProperty = Animator.StringToHash("Speed");
    private static readonly int AnimatorIsRunningProperty = Animator.StringToHash("IsRunning");

    #region Serialized Fields

    [SerializeField, Min(0)] private float movementSpeed = 8;
    [SerializeField, Min(0)] private float angularSpeed = 1500f;

    [SerializeField, Range(0, 1)] private float unawareMovementMultiplier = .5f;
    [SerializeField, Range(0, 1)] private float curiousMovementMultiplier = .75f;

    [Header("Checkpoint Traversal")] [SerializeField] [Tooltip("The checkpoints that the enemy will traverse.")]
    private Transform[] patrolCheckpoints;

    [SerializeField] [Min(0)] [Tooltip("How close the enemy needs to be to the checkpoint to consider it reached.")]
    private float checkpointProximityThreshold = 0.5f;

    [SerializeField, Min(0)] private float stoppingDistance = 1.5f;

    [Header("Animations")] [SerializeField]
    private Animator animator;

    [SerializeField] [Min(0)] private float walkAnimationThreshold;
    [SerializeField] [Min(0)] private float runAnimationThreshold;

    [SerializeField, Min(0)] private float animationSpeedCoefficient = 1;

    #endregion

    #region Private Fields

    private int _currentCheckpointIndex;

    private TokenManager<float>.ManagedToken _withinStoppingDistanceToken;
    private TokenManager<float>.ManagedToken _detectionStateToken;

    private float _targetVelocity;

    #endregion

    #region Getters

    public Enemy Enemy { get; private set; }

    public GameObject GameObject => gameObject;

    public HashSet<object> MovementDisableTokens { get; } = new();

    public TokenManager<float> MovementSpeedTokens { get; } = new(false, null, 1);

    public NavMeshAgent NavMeshAgent { get; private set; }

    public IReadOnlyCollection<Transform> PatrolCheckpoints => patrolCheckpoints;

    public Transform CurrentCheckpoint
    {
        get
        {
            if (patrolCheckpoints == null || patrolCheckpoints.Length == 0)
                return null;

            return patrolCheckpoints[_currentCheckpointIndex];
        }
    }

    public bool IsMovementEnabled => this.IsMovementEnabled();

    private bool IsWithinStoppingDistance =>
        // NavMeshAgent.remainingDistance <= stoppingDistance &&
        Enemy.DetectionBehavior.CurrentDetectionState == EnemyDetectionState.Aware &&
        Vector3.Distance(transform.position, Enemy.DetectionBehavior.Target.GameObject.transform.position) <=
        stoppingDistance;

    #endregion

    #region Initialization Functions

    private void Awake()
    {
        // Initialize the components
        InitializeComponents();

        // Create the within stopping distance token
        _withinStoppingDistanceToken = MovementSpeedTokens.AddToken(1, -1, true);

        // Create the detection state token
        _detectionStateToken = MovementSpeedTokens.AddToken(1, -1, true);
    }

    private void InitializeComponents()
    {
        // Get the enemy component
        Enemy = GetComponent<Enemy>();

        // Get the NavMeshAgent component
        NavMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        // // Add this to the debug manager
        // DebugManager.Instance.AddDebuggedObject(this);

        // Determine the closest checkpoint
        _currentCheckpointIndex = DetermineClosestCheckpoint();

        // Return if the closest checkpoint is invalid
        if (_currentCheckpointIndex < 0 || _currentCheckpointIndex >= patrolCheckpoints.Length)
            return;

        // Set the destination to the closest checkpoint
        CustomSetDestination(patrolCheckpoints[_currentCheckpointIndex].position);
    }

    #endregion

    private void Update()
    {
        // Set the NavMeshAgent enabled state
        if (NavMeshAgent.enabled && NavMeshAgent.isOnNavMesh)
            NavMeshAgent.isStopped = !IsMovementEnabled;

        // If the navmesh agent is enabled, check if the enemy is within stopping distance
        if (NavMeshAgent.enabled && NavMeshAgent.isOnNavMesh && IsWithinStoppingDistance)
            this.AddMovementDisableToken(this);
        else
            this.RemoveMovementDisableToken(this);

        // Update the detection state token
        _detectionStateToken.Value = Enemy.DetectionBehavior.CurrentDetectionState switch
        {
            EnemyDetectionState.Unaware => unawareMovementMultiplier,
            EnemyDetectionState.Curious => curiousMovementMultiplier,
            EnemyDetectionState.Aware => 1,
            _ => 1
        };

        var movementSpeedTokenMultiplier = this.GetMovementSpeedTokenMultiplier();

        // Set the target velocity
        _targetVelocity = movementSpeed * movementSpeedTokenMultiplier;

        if (NavMeshAgent.enabled && NavMeshAgent.isOnNavMesh)
        {
            // Set the movement speed of the navmesh agent
            NavMeshAgent.speed = _targetVelocity;

            // Set the angular speed of the navmesh agent
            NavMeshAgent.angularSpeed = angularSpeed * movementSpeedTokenMultiplier;
        }

        // Update the movement animation
        UpdateMovementAnimation();

        // Update the movement speed tokens
        MovementSpeedTokens.Update(Time.deltaTime);

        // Update the destination
        UpdateDestination();
    }

    private void UpdateDestination()
    {
        // Return if disabled
        if (!IsMovementEnabled)
            return;

        if (!NavMeshAgent.enabled || !NavMeshAgent.isOnNavMesh)
            return;

        var currentDetectionState = Enemy.DetectionBehavior.CurrentDetectionState;

        switch (currentDetectionState)
        {
            case EnemyDetectionState.Unaware:

                // Check if the enemy has reached the checkpoint
                CheckCheckpoint();

                // Break if the current checkpoint is null
                if (CurrentCheckpoint == null)
                    return;

                // Set the destination to the current checkpoint
                if (NavMeshAgent.destination != CurrentCheckpoint.position)
                    CustomSetDestination(CurrentCheckpoint.position);

                break;

            case EnemyDetectionState.Curious:

                // Set the destination to the last known player position
                if (NavMeshAgent.destination != Enemy.DetectionBehavior.LastKnownTargetPosition)
                    CustomSetDestination(Enemy.DetectionBehavior.LastKnownTargetPosition);

                break;

            case EnemyDetectionState.Aware:

                // Set the destination to the player's current position
                if (Enemy.DetectionBehavior.IsTargetDetected)
                    CustomSetDestination(Enemy.DetectionBehavior.Target.GameObject.transform.position);
                else
                    CustomSetDestination(Enemy.DetectionBehavior.LastKnownTargetPosition);

                break;

            default:
                Debug.LogError($"Case not handled: {currentDetectionState}");
                break;
        }
    }

    private void UpdateMovementAnimation()
    {
        // Return if the animator is null
        if (animator == null)
            return;

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

    private void CheckCheckpoint()
    {
        // Return if the NavMeshAgent is disabled
        if (!NavMeshAgent.enabled || !NavMeshAgent.isOnNavMesh)
            return;

        // Get the distance to the current checkpoint
        var distanceToCheckpoint = NavMeshAgent.remainingDistance;

        // If the enemy is close enough to the checkpoint, move to the next checkpoint
        if (distanceToCheckpoint > checkpointProximityThreshold)
            return;

        // Return if there are no checkpoints
        if (patrolCheckpoints.Length == 0)
            return;

        // Increment the checkpoint index
        _currentCheckpointIndex = (_currentCheckpointIndex + 1) % patrolCheckpoints.Length;

        // Set the destination to the next checkpoint
        if (CurrentCheckpoint != null)
            CustomSetDestination(CurrentCheckpoint.position);
    }

    private int DetermineClosestCheckpoint()
    {
        // return -1 if there are no checkpoints
        if (patrolCheckpoints.Length == 0)
            return -1;

        var closestCheckpointIndex = 0;
        var closestDistance = Vector3.Distance(transform.position, patrolCheckpoints[0].position);

        // Determine the closest checkpoint
        for (var i = 1; i < patrolCheckpoints.Length; i++)
        {
            // Break if the index is out of bounds
            if (i >= patrolCheckpoints.Length)
                break;

            if (patrolCheckpoints[i] == null)
                continue;
            
            var distance = Vector3.Distance(transform.position, patrolCheckpoints[i].position);

            // Update the closest checkpoint if the current checkpoint is closer
            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            closestCheckpointIndex = i;
        }

        return closestCheckpointIndex;
    }

    public void SetPosition(Vector3 pos)
    {
        // If there is a nav mesh agent, set the position
        if (NavMeshAgent != null)
        {
            NavMeshAgent.Warp(pos);
            return;
        }

        // If there is a rigidbody, set the position
        if (TryGetComponent(out Rigidbody rb))
        {
            rb.MovePosition(pos);
            return;
        }

        // Set the position
        transform.position = pos;
    }

    private void CustomSetDestination(Vector3 position)
    {
        const float distanceThreshold = 5f;
        
        // Return if the NavMeshAgent is null or disabled
        if (NavMeshAgent == null || !NavMeshAgent.enabled)
            return;
        
        var currentDestination = NavMeshAgent.destination;
        
        // Return if the distance to the destination is less than the threshold
        if (Vector3.Distance(currentDestination, position) < distanceThreshold)
            return;

        // Set the destination to the position
        NavMeshAgent.SetDestination(position);
        
    }

    #region Debugging

    private void OnDrawGizmosSelected()
    {
        //Make sure node list is not empty
        if (patrolCheckpoints == null || patrolCheckpoints.Length == 0)
            return;

        //Draw gizmos between each node in the sequence
        for (var i = 0; i < patrolCheckpoints.Length; i++)
        {
            var nextIndex = (i + 1) % patrolCheckpoints.Length;

            if (patrolCheckpoints[i] == null)
                continue;

            if (patrolCheckpoints[nextIndex] == null)
                continue;

            // Draw a line between the current node and the next node
            Gizmos.color = Color.red;
            Gizmos.DrawLine(patrolCheckpoints[i].position, patrolCheckpoints[nextIndex].position);

            // Draw a sphere at the current node
            Gizmos.color = Color.green;

            if (i == _currentCheckpointIndex)
                Gizmos.color = Color.blue;

            Gizmos.DrawSphere(patrolCheckpoints[i].position, 0.1f);
        }
    }

    #endregion
}