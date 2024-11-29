using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrolPursuit : MonoBehaviour, IEnemyMovementBehavior
{
    //Adding this ref here dont fight me
    //public NPCmovement npcMovement;

    #region Serialized Fields

    [Header("Checkpoint Traversal")] [SerializeField] [Tooltip("The checkpoints that the enemy will traverse.")]
    private Transform[] patrolCheckpoints;

    [SerializeField] [Min(0)] [Tooltip("How close the enemy needs to be to the checkpoint to consider it reached.")]
    private float checkpointProximityThreshold = 0.5f;

    #endregion

    #region Private Fields

    private int _currentCheckpointIndex;

    private bool _externallyEnabled = true;

    #endregion

    #region Getters

    public Enemy Enemy { get; private set; }

    public GameObject GameObject => gameObject;

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

    private bool NavMeshEnabled => _externallyEnabled;

    #endregion

    #region Initialization Functions

    private void Awake()
    {
        // Initialize the components
        InitializeComponents();
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
        // Determine the closest checkpoint
        _currentCheckpointIndex = DetermineClosestCheckpoint();

        // Return if the closest checkpoint is invalid
        if (_currentCheckpointIndex < 0 || _currentCheckpointIndex >= patrolCheckpoints.Length)
            return;

        // Set the destination to the closest checkpoint
        NavMeshAgent.SetDestination(patrolCheckpoints[_currentCheckpointIndex].position);
    }

    #endregion

    private void Update()
    {
        // Update the destination
        UpdateDestination();

        // Set the NavMeshAgent enabled state
        NavMeshAgent.enabled = NavMeshEnabled;
    }

    private void UpdateDestination()
    {
        var currentDetectionState = Enemy.EnemyDetectionBehavior.CurrentDetectionState;

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
                    NavMeshAgent.SetDestination(CurrentCheckpoint.position);

                break;

            case EnemyDetectionState.Curious:

                // Set the destination to the last known player position
                if (NavMeshAgent.destination != Enemy.EnemyDetectionBehavior.LastKnownTargetPosition)
                    NavMeshAgent.SetDestination(Enemy.EnemyDetectionBehavior.LastKnownTargetPosition);

                break;

            case EnemyDetectionState.Aware:

                // Set the destination to the player's current position
                NavMeshAgent.SetDestination(Enemy.EnemyDetectionBehavior.Target.GameObject.transform.position);
                break;

            default:
                Debug.LogError($"Case not handled: {currentDetectionState}");
                break;
        }
    }

    private void CheckCheckpoint()
    {
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
        NavMeshAgent.SetDestination(CurrentCheckpoint.position);
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

            var currentCheckpoint = patrolCheckpoints[i];
            var distance = Vector3.Distance(transform.position, currentCheckpoint.position);

            // Update the closest checkpoint if the current checkpoint is closer
            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            closestCheckpointIndex = i;
        }

        return closestCheckpointIndex;
    }

    public void SetMovementEnabled(bool on)
    {
        NavMeshAgent.enabled = on;
    }

    #region Debugging

    private void OnDrawGizmos()
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

            Gizmos.color = Color.red;
            Gizmos.DrawLine(patrolCheckpoints[i].position, patrolCheckpoints[nextIndex].position);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(patrolCheckpoints[i].position, 0.1f);
        }
    }

    #endregion

    // public enum EnemyMovementState
    // {
    //     /// <summary>
    //     /// The player is nowhere in sight.
    //     /// The enemy is going along their route
    //     /// </summary>
    //     Patrol,
    //
    //     /// <summary>
    //     /// The player is in sight, but the enemy hasn't had sight of them for long.
    //     /// </summary>
    //     Searching,
    //
    //     /// <summary>
    //     /// The player is in sight and the enemy is actively pursuing them.
    //     /// </summary>
    //     Pursuit,
    // }
}