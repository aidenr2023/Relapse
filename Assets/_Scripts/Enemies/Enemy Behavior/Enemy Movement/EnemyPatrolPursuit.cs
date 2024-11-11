using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrolPursuit : MonoBehaviour, IEnemyMovementBehavior
{
    #region Serialized Fields

    [Header("Checkpoint Traversal")] [SerializeField] [Tooltip("The checkpoints that the enemy will traverse.")]
    private Transform[] patrolCheckpoints;

    [SerializeField] [Min(0)] [Tooltip("How close the enemy needs to be to the checkpoint to consider it reached.")]
    private float checkpointProximityThreshold = 0.5f;

    #endregion

    #region Private Fields

    private int _currentCheckpointIndex;

    #endregion

    #region Getters

    public Enemy Enemy { get; private set; }

    public GameObject GameObject => gameObject;

    public NavMeshAgent NavMeshAgent { get; private set; }

    public IReadOnlyCollection<Transform> PatrolCheckpoints => patrolCheckpoints;

    public Transform CurrentCheckpoint => patrolCheckpoints[_currentCheckpointIndex];

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

        // Set the destination to the closest checkpoint
        NavMeshAgent.SetDestination(patrolCheckpoints[_currentCheckpointIndex].position);
    }

    #endregion

    private void Update()
    {
        // Check if the enemy has reached the checkpoint
        CheckCheckpoint();
    }

    private void CheckCheckpoint()
    {
        // Get the distance to the current checkpoint
        var distanceToCheckpoint = NavMeshAgent.remainingDistance;

        // If the enemy is close enough to the checkpoint, move to the next checkpoint
        if (distanceToCheckpoint > checkpointProximityThreshold)
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

    private void OnDrawGizmos()
    {
        //Make sure node list is not empty
        if (patrolCheckpoints == null || patrolCheckpoints.Length == 0)
            return;

        //Draw gizmos between each node in the sequence
        for (var i = 0; i < patrolCheckpoints.Length; i++)
        {
            var nextIndex = (i + 1) % patrolCheckpoints.Length;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(patrolCheckpoints[i].position, patrolCheckpoints[nextIndex].position);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(patrolCheckpoints[i].position, 0.1f);
        }
    }
}