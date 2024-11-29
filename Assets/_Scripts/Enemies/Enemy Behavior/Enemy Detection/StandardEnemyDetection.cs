using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StandardEnemyDetection : MonoBehaviour, IEnemyDetectionBehavior
{
    #region Serialized Fields

    [Header("Detection")] [SerializeField] [Min(0)]
    private float visionDistance = 10f;

    [SerializeField] [Min(0)] private float visionAngle = 45f;

    [SerializeField] private CountdownTimer patrolDetectionTimer;
    [SerializeField] private CountdownTimer searchDetectionTimer;
    [SerializeField] private CountdownTimer pursuitDetectionTimer;


    [Header("Debugging")] [SerializeField] private Canvas debugCanvas;
    [SerializeField] private Slider pursuitDetectionSlider;
    [SerializeField] private TMP_Text pursuitDetectionText;
    [SerializeField] private Image pursuitDetectionColorImage;

    [SerializeField] private Color patrolDetectionColor = Color.green;
    [SerializeField] private Color searchDetectionColor = Color.yellow;
    [SerializeField] private Color pursuitDetectionColor = Color.red;

    #endregion

    public event Action<IEnemyDetectionBehavior, EnemyDetectionState, EnemyDetectionState> OnDetectionStateChanged;

    #region Private Fields

    /// <summary>
    /// The player is within the vision cone of the enemy & within the vision distance.
    /// </summary>
    private bool _isTargetInSight;

    #endregion

    #region Getters

    public Enemy Enemy { get; private set; }

    public GameObject GameObject => gameObject;

    public bool IsDetectionEnabled { get; set; } = true;

    public EnemyDetectionState CurrentDetectionState { get; private set; }

    public bool IsTargetDetected => CurrentDetectionState == EnemyDetectionState.Aware && Target != null;

    public IActor Target { get; private set; }

    public Vector3 LastKnownTargetPosition { get; private set; }

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
    }

    private void Start()
    {
        // Set the current detection state to unaware
        CurrentDetectionState = EnemyDetectionState.Unaware;

        // Set the target to null
        Target = null;

        // Subscribe to the enemy's death event
        OnDetectionStateChanged += (behavior, oldState, newState) =>
            Debug.Log($"{behavior.GameObject.name} changed detection state from {oldState} to {newState}");

        OnDetectionStateChanged += ResetTimersOnStateChange;
    }

    private void ResetTimersOnStateChange(
        IEnemyDetectionBehavior enemy,
        EnemyDetectionState oldState,
        EnemyDetectionState newState
    )
    {
        // Reset the timers based on the new state
        switch (newState)
        {
            case EnemyDetectionState.Unaware:
                // patrolDetectionTimer.Reset();
                searchDetectionTimer.Reset();
                pursuitDetectionTimer.Reset();
                break;

            case EnemyDetectionState.Curious:
                patrolDetectionTimer.Reset();
                // searchDetectionTimer.Reset();
                pursuitDetectionTimer.Reset();
                break;

            case EnemyDetectionState.Aware:
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
        // If the detection is not enabled, return
        if (!IsDetectionEnabled)
            return;

        // Update the last known player position
        UpdateLastKnownPlayerPosition();

        // Detect the player
        _isTargetInSight = CheckPlayerInSight();

        // If the target is in sight, set the target to the player
        if (_isTargetInSight)
            Target = Player.Instance.PlayerInfo;

        // Update the detection state
        UpdateDetectionState();

        // Update the debug canvas
        UpdateDebug();
    }

    private void UpdateLastKnownPlayerPosition()
    {
        // Update the last known player position
        if (_isTargetInSight)
            LastKnownTargetPosition = Player.Instance.transform.position;
    }

    /// <summary>
    /// Based on the enemy's current movement state,
    /// determine if the enemy needs to change state
    /// </summary>
    private void UpdateDetectionState()
    {
        var previousDetectionState = CurrentDetectionState;

        switch (CurrentDetectionState)
        {
            case EnemyDetectionState.Unaware:

                // // if tagged as "stationary" then disable movement
                // if (gameObject.CompareTag("Stationary") && !npcMovement.canMove)
                //     npcMovement.DisableMovement();
                // else
                //     npcMovement.EnableMovement();

                // Update the player detection timer
                if (_isTargetInSight)
                    patrolDetectionTimer.Update(Time.deltaTime);
                else
                    patrolDetectionTimer.Update(-Time.deltaTime);

                // If the player is detected for long enough,
                // change the movement state to pursuit
                if (patrolDetectionTimer.Percentage >= 1)
                {
                    CurrentDetectionState = EnemyDetectionState.Curious;
                    OnDetectionStateChanged?.Invoke(this, previousDetectionState, CurrentDetectionState);
                }

                break;

            case EnemyDetectionState.Curious:
                // Update the player detection timer
                if (_isTargetInSight)
                    searchDetectionTimer.Update(Time.deltaTime);
                else
                    searchDetectionTimer.Update(-Time.deltaTime);

                // If the player is detected for long enough,
                // change the movement state to pursuit
                if (searchDetectionTimer.Percentage >= 1)
                {
                    CurrentDetectionState = EnemyDetectionState.Aware;
                    OnDetectionStateChanged?.Invoke(this, previousDetectionState, CurrentDetectionState);
                }

                // If the player is no longer detected,
                // change the movement state to patrol
                else if (searchDetectionTimer.Percentage <= 0)
                {
                    CurrentDetectionState = EnemyDetectionState.Unaware;

                    // Reset the patrol timer
                    patrolDetectionTimer.Reset();

                    OnDetectionStateChanged?.Invoke(this, previousDetectionState, CurrentDetectionState);
                }

                break;

            case EnemyDetectionState.Aware:
                // Update the player detection timer
                if (_isTargetInSight)
                    pursuitDetectionTimer.ForcePercent(1);

                else
                    pursuitDetectionTimer.Update(-Time.deltaTime);

                // If the player is no longer detected,
                // change the movement state to patrol
                if (pursuitDetectionTimer.Percentage <= 0)
                {
                    CurrentDetectionState = EnemyDetectionState.Curious;

                    // Force the searching timer to 100%
                    searchDetectionTimer.ForcePercent(1);

                    OnDetectionStateChanged?.Invoke(this, previousDetectionState, CurrentDetectionState);
                }

                break;

            default:
                Debug.LogError($"Case not handled: {CurrentDetectionState}");
                break;
        }
    }

    private void UpdateDebug()
    {
        // If the game is not in debug mode, hide the debug canvas and return
        if (!DebugManager.Instance.IsDebugMode)
        {
            debugCanvas.enabled = false;
            return;
        }

        debugCanvas.enabled = true;

        // Get the main camera
        var mainCamera = Camera.main;

        // Set the debug canvas to be facing the current camera,
        // but flipped so text is oriented correctly
        debugCanvas.transform.LookAt(mainCamera.transform);
        debugCanvas.transform.Rotate(0, 180, 0);

        var currentTimer = CurrentDetectionState switch
        {
            EnemyDetectionState.Unaware => patrolDetectionTimer,
            EnemyDetectionState.Curious => searchDetectionTimer,
            EnemyDetectionState.Aware => pursuitDetectionTimer,
            _ => null
        };

        // Set the pursuit detection slider value
        pursuitDetectionSlider.value = currentTimer.Percentage;

        // Set the pursuit detection text
        pursuitDetectionText.text = $"{CurrentDetectionState.ToString()} {currentTimer.Percentage:0.00}";
    }

    private bool CheckPlayerInSight()
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

    #region Debugging

    private void OnDrawGizmos()
    {
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

        Gizmos.color = _isTargetInSight ? detectedColor : undetectedColor;
        Gizmos.DrawLine(transform.position, transform.position + left * visionDistance);
        Gizmos.DrawLine(transform.position, transform.position + right * visionDistance);
    }

    #endregion
}