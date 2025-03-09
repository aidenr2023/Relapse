using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class PatrolEnemyMovement : ComponentScript<Enemy>, INewEnemyMovementBehavior
{
    #region Serialized Fields

    [Header("Checkpoint Traversal")] [SerializeField] [Tooltip("The checkpoints that the enemy will traverse.")]
    private Transform[] patrolCheckpoints;

    [SerializeField] [Min(0)] [Tooltip("How close the enemy needs to be to the checkpoint to consider it reached.")]
    private float checkpointProximityThreshold = 0.5f;

    [SerializeField, Range(0, 1)] private float unawareMovementMultiplier = .5f;

    #endregion

    #region Private Fields

    private int _currentCheckpointIndex = -1;

    private TokenManager<float>.ManagedToken _detectionStateToken;

    #endregion

    #region Getters

    public NewEnemyBehaviorBrain Brain { get; private set; }
    public NewEnemyMovement NewMovement { get; private set; }

    public GameObject GameObject => gameObject;
    public Enemy Enemy => gameObject.GetComponent<Enemy>();

    #endregion

    protected override void CustomAwake()
    {
        Brain = GetComponent<NewEnemyBehaviorBrain>();
        NewMovement = GetComponent<NewEnemyMovement>();

        Brain.onPlayerMovementStateChange += OnPlayerMovementStateChange;
    }

    private void OnPlayerMovementStateChange(BehaviorActionMove.MoveAction prevState, NewEnemyBehaviorBrain brain)
    {
        // If the brain's current state is script,
        // set the destination to the current checkpoint
        if (
            brain.CurrentMoveActionType == BehaviorActionMove.MoveAction.MovementScript &&
            (PatrolEnemyMovement)brain.MovementBehavior == this
        )
        {
            SetDestinationToCheckpoint(_currentCheckpointIndex);
        }

        else
        {
            // Reset the movement speed token
            _detectionStateToken.Value = 1;
        }
    }

    private void Start()
    {
        _currentCheckpointIndex = 0;

        // Create the detection state token
        _detectionStateToken = NewMovement.MovementSpeedTokens.AddToken(1, -1, true);
    }

    public void StateUpdateMovement(
        NewEnemyBehaviorBrain brain, NewEnemyMovement newMovement, bool needsToUpdateDestination
    )
    {
        // Update the detection state token
        UpdateDetectionStateToken();

        // Check if the enemy has reached the current checkpoint
        if (CheckForNewCheckpoint())
        {
            // Increment the checkpoint index
            if (patrolCheckpoints.Length > 0)
                _currentCheckpointIndex = (_currentCheckpointIndex + 1) % patrolCheckpoints.Length;

            SetDestinationToCheckpoint(_currentCheckpointIndex);
        }
    }

    private void UpdateDetectionStateToken()
    {
        _detectionStateToken.Value = Enemy.DetectionBehavior.CurrentDetectionState switch
        {
            EnemyDetectionState.Unaware => unawareMovementMultiplier,
            _ => 1
        };
    }

    private bool CheckForNewCheckpoint()
    {
        return (NewMovement.NavMeshAgent.remainingDistance < checkpointProximityThreshold);
    }

    private void SetDestinationToCheckpoint(int index)
    {
        // Skip if there are no checkpoints
        if (patrolCheckpoints.Length == 0)
        {
            // Debug.LogError("No patrol checkpoints have been set for this enemy.", this);
            return;
        }

        // Ensure that the index is within the bounds of the array
        index %= patrolCheckpoints.Length;

        // Skip if the checkpoint at the index is null
        if (patrolCheckpoints[index] == null)
        {
            Debug.LogError($"The checkpoint at index {index} is null.", this);
            return;
        }

        // Set the destination to the checkpoint
        NewMovement.SetDestination(patrolCheckpoints[index].position);

        Debug.Log($"Setting destination to checkpoint {index}.");
    }

    private void OnDrawGizmos()
    {
        // Draw spheres at the patrol checkpoints
        for (var i = 0; i < patrolCheckpoints.Length; i++)
        {
            if (patrolCheckpoints[i] == null)
                continue;

            if (i == _currentCheckpointIndex)
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.red;
        }
    }
}