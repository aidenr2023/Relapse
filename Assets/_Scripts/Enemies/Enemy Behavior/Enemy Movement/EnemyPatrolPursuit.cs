using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class EnemyPatrolPursuit : MonoBehaviour, IEnemyMovementBehavior, IDebugged
{
    #region Serialized Fields

    [Header("Checkpoint Traversal")] [SerializeField] [Tooltip("The checkpoints that the enemy will traverse.")]
    private Transform[] patrolCheckpoints;

    [SerializeField] [Min(0)] [Tooltip("How close the enemy needs to be to the checkpoint to consider it reached.")]
    private float checkpointProximityThreshold = 0.5f;

    [Header("Detection")] [SerializeField] [Min(0)]
    private float visionDistance = 10f;

    [SerializeField] [Min(0)] private float visionAngle = 45f;

    [SerializeField] private CountdownTimer patrolDetectionTimer;
    [SerializeField] private CountdownTimer searchDetectionTimer;
    [SerializeField] private CountdownTimer pursuitDetectionTimer;

    #endregion

    #region Private Fields

    private int _currentCheckpointIndex;

    private EnemyMovementState _currentMovementState = EnemyMovementState.Patrol;

    /// <summary>
    /// The player is within the vision cone of the enemy & within the vision distance.
    /// </summary>
    private bool _playerDetected;

    private Vector3 _lastKnownPlayerPosition;

    #endregion

    #region Events

    public event Action<EnemyPatrolPursuit, EnemyMovementState, EnemyMovementState> OnMovementStateChanged;

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

        // Reset and enable all timers
        patrolDetectionTimer.Reset();
        searchDetectionTimer.Reset();
        pursuitDetectionTimer.Reset();

        patrolDetectionTimer.SetActive(true);
        searchDetectionTimer.SetActive(true);
        pursuitDetectionTimer.SetActive(true);
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

        OnMovementStateChanged += (enemy, oldState, newState) =>
            Debug.Log($"{enemy.name} changed movement state to {newState} from {oldState}");

        OnMovementStateChanged += ResetTimersOnStateChange;

        // Add this to the debug manager
        DebugManager.Instance.AddDebuggedObject(this);
    }

    private void ResetTimersOnStateChange(EnemyPatrolPursuit enemy,
        EnemyMovementState oldState, EnemyMovementState newState)
    {
        // Reset the timers based on the new state
        switch (newState)
        {
            case EnemyMovementState.Patrol:
                // patrolDetectionTimer.Reset();
                searchDetectionTimer.Reset();
                pursuitDetectionTimer.Reset();
                break;

            case EnemyMovementState.Searching:
                patrolDetectionTimer.Reset();
                // searchDetectionTimer.Reset();
                pursuitDetectionTimer.Reset();
                break;

            case EnemyMovementState.Pursuit:
                patrolDetectionTimer.Reset();
                searchDetectionTimer.Reset();
                // pursuitDetectionTimer.Reset();

                // Set the pursuit destination timer to the max value
                pursuitDetectionTimer.ForcePercent(1);

                break;

            default:
                Debug.LogError($"Case not handled: {newState}");
                break;
        }
    }

    #endregion

    private void Update()
    {
        // Detect the player
        _playerDetected = PlayerDetected();

        // Update the last known player position
        UpdateLastKnowPlayerPosition();

        // Determine the current movement state
        UpdateMovementState();

        // Update the destination
        UpdateDestination();
    }

    private void UpdateLastKnowPlayerPosition()
    {
        // Update the last known player position
        if (_playerDetected)
            _lastKnownPlayerPosition = Player.Instance.transform.position;
    }

    private void UpdateDestination()
    {
        switch (_currentMovementState)
        {
            case EnemyMovementState.Patrol:

                // Check if the enemy has reached the checkpoint
                CheckCheckpoint();

                // Set the destination to the current checkpoint
                if (NavMeshAgent.destination != CurrentCheckpoint.position)
                    NavMeshAgent.SetDestination(CurrentCheckpoint.position);

                break;

            case EnemyMovementState.Searching:

                // Set the destination to the last known player position
                if (NavMeshAgent.destination != _lastKnownPlayerPosition)
                    NavMeshAgent.SetDestination(_lastKnownPlayerPosition);

                break;

            case EnemyMovementState.Pursuit:

                // Set the destination to the player's current position
                NavMeshAgent.SetDestination(Player.Instance.transform.position);

                break;

            default:
                Debug.LogError($"Case not handled: {_currentMovementState}");
                break;
        }
    }

    /// <summary>
    /// Based on the enemy's current movement state,
    /// determine if the enemy needs to change state
    /// </summary>
    private void UpdateMovementState()
    {
        var previousMovementState = _currentMovementState;

        switch (_currentMovementState)
        {
            case EnemyMovementState.Patrol:
                // Update the player detection timer
                if (_playerDetected)
                    patrolDetectionTimer.Update(Time.deltaTime);
                else
                    patrolDetectionTimer.Update(-Time.deltaTime);

                // If the player is detected for long enough,
                // change the movement state to pursuit
                if (patrolDetectionTimer.Percentage >= 1)
                {
                    _currentMovementState = EnemyMovementState.Searching;
                    OnMovementStateChanged?.Invoke(this, previousMovementState, _currentMovementState);
                }

                break;

            case EnemyMovementState.Searching:
                // Update the player detection timer
                if (_playerDetected)
                    searchDetectionTimer.Update(Time.deltaTime);
                else
                    searchDetectionTimer.Update(-Time.deltaTime);

                // If the player is detected for long enough,
                // change the movement state to pursuit
                if (searchDetectionTimer.Percentage >= 1)
                {
                    _currentMovementState = EnemyMovementState.Pursuit;
                    OnMovementStateChanged?.Invoke(this, previousMovementState, _currentMovementState);
                }

                // If the player is no longer detected,
                // change the movement state to patrol
                if (searchDetectionTimer.Percentage <= 0)
                {
                    _currentMovementState = EnemyMovementState.Pursuit;
                    OnMovementStateChanged?.Invoke(this, previousMovementState, _currentMovementState);
                }

                break;

            case EnemyMovementState.Pursuit:
                // Update the player detection timer
                if (_playerDetected)
                    pursuitDetectionTimer.ForcePercent(1);

                else
                    pursuitDetectionTimer.Update(-Time.deltaTime);

                // If the player is no longer detected,
                // change the movement state to patrol
                if (pursuitDetectionTimer.Percentage <= 0)
                {
                    _currentMovementState = EnemyMovementState.Searching;

                    // Force the searching timer to 100%
                    searchDetectionTimer.ForcePercent(1);

                    OnMovementStateChanged?.Invoke(this, previousMovementState, _currentMovementState);
                }

                break;

            default:
                Debug.LogError($"Case not handled: {_currentMovementState}");
                break;
        }
    }

    private bool PlayerDetected()
    {
        // Get the player instance
        var player = Player.Instance;

        // Return false if the player instance is null
        if (player == null)
            return false;

        // Get the line between the enemy and the player
        var line = player.transform.position - transform.position;

        // Return false if the player is not within the vision distance
        if (line.magnitude > visionDistance)
            return false;

        var angle = Vector3.Angle(transform.forward, line);

        // Return false if the player is not within the enemy's field of view
        if (angle > visionAngle)
            return false;

        // Check a raycast to the player
        if (!Physics.Raycast(transform.position, line, out var hit, visionDistance))
            return false;

        // Return false if the raycast does not hit the player
        if (hit.collider.gameObject != player.gameObject)
            return false;

        // Debug.Log($"Player detected! Distance: {line.magnitude}, Angle: {Vector3.Angle(transform.forward, line)}");

        return true;
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

            Gizmos.color = Color.red;
            Gizmos.DrawLine(patrolCheckpoints[i].position, patrolCheckpoints[nextIndex].position);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(patrolCheckpoints[i].position, 0.1f);
        }

        // Draw a line from the enemy to the player
        var undetectedColor = Color.green;
        var detectedColor = Color.red;

        if (Player.Instance != null)
        {
            // Get the line between the enemy and the player
            var line = Player.Instance.transform.position - transform.position;

            var endPosition = transform.position + line.normalized * visionDistance;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, endPosition);
        }

        // Draw the vision angle
        var halfAngle = visionAngle / 2;
        var forward = transform.forward;

        var left = Quaternion.Euler(0, -halfAngle, 0) * forward;
        var right = Quaternion.Euler(0, halfAngle, 0) * forward;

        Gizmos.color = _playerDetected ? detectedColor : undetectedColor;
        Gizmos.DrawLine(transform.position, transform.position + left * visionDistance);
        Gizmos.DrawLine(transform.position, transform.position + right * visionDistance);
    }

    public string GetDebugText()
    {
        return $"Enemy Movement State: {_currentMovementState}\n" +
               $"Patrol Timer: {patrolDetectionTimer.Percentage:0.00}\n" +
               $"Patrol Timer: {searchDetectionTimer.Percentage:0.00}\n" +
               $"Patrol Timer: {pursuitDetectionTimer.Percentage:0.00}\n";
    }

    #endregion

    public enum EnemyMovementState
    {
        /// <summary>
        /// The player is nowhere in sight.
        /// The enemy is going along their route
        /// </summary>
        Patrol,

        /// <summary>
        /// The player is in sight, but the enemy hasn't had sight of them for long.
        /// </summary>
        Searching,

        /// <summary>
        /// The player is in sight and the enemy is actively pursuing them.
        /// </summary>
        Pursuit,
    }
}