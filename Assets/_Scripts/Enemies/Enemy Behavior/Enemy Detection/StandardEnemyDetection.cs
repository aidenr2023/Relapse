﻿using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StandardEnemyDetection : MonoBehaviour, IEnemyDetectionBehavior
{
    #region Serialized Fields

    [SerializeField] private TransformReference playerTransform;
    
    [Header("Detection")] [SerializeField] private Transform detectionOrigin;

    [SerializeField] [Min(0)] private float visionDistance = 10f;

    [SerializeField] [Min(0)] private float autoDetectionDistance = 5f;

    [SerializeField] [Min(0)] private float visionAngle = 45f;

    [SerializeField] private CountdownTimer patrolDetectionTimer;
    [SerializeField] private CountdownTimer searchDetectionTimer;
    [SerializeField] private CountdownTimer pursuitDetectionTimer;

    [SerializeField] private LayerMask layersToIgnore;

    [Header("Debugging")] [SerializeField] private Canvas debugCanvas;
    [SerializeField] private Slider pursuitDetectionSlider;
    [SerializeField] private TMP_Text pursuitDetectionText;
    [SerializeField] private Image pursuitDetectionColorImage;

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

    public bool IsDetectionEnabled { get; private set; } = true;

    public EnemyDetectionState CurrentDetectionState { get; private set; }

    public bool IsTargetDetected => Target != null && _isTargetInSight;

    public IActor Target { get; private set; }

    public Vector3 LastKnownTargetPosition { get; private set; }

    public Transform DetectionOrigin => detectionOrigin;

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

        // // Subscribe to the enemy's death event
        // OnDetectionStateChanged += (behavior, oldState, newState) =>
        //     Debug.Log($"{behavior.GameObject.name} changed detection state from {oldState} to {newState}");

        OnDetectionStateChanged += ResetTimersOnStateChange;

        Enemy.EnemyInfo.OnDamaged += DetectPlayerWhenDamaged;
    }

    private void DetectPlayerWhenDamaged(object sender, HealthChangedEventArgs e)
    {
        // If the enemy is already aware of the player, return
        if (CurrentDetectionState == EnemyDetectionState.Aware)
            return;

        // Set the detection state to aware
        CurrentDetectionState = EnemyDetectionState.Aware;

        Target = e.Changer;

        if (Target != null)
            LastKnownTargetPosition = e.Changer.GameObject.transform.position;
        
        // Invoke the detection state changed event
        OnDetectionStateChanged?.Invoke(this, EnemyDetectionState.Unaware, CurrentDetectionState);
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
        _isTargetInSight = CheckPlayerInSight(out var target);

        // If the target is in sight, set the target to the player
        if (_isTargetInSight)
            Target = target;

        // Update the detection state
        UpdateDetectionState();

        // Update the debug canvas
        UpdateDebug();
    }

    private void UpdateLastKnownPlayerPosition()
    {
        // Update the last known player position
        if (_isTargetInSight && Target != null)
            LastKnownTargetPosition = Target.GameObject.transform.position;
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
        // Return if the debug canvas is null
        if (debugCanvas == null)
            return;
        
        // If the game is not in debug mode, hide the debug canvas and return
        if (!DebugManager.Instance.IsDebugMode)
        {
            debugCanvas.gameObject.SetActive(false);
            debugCanvas.enabled = false;
            return;
        }

        // Show the debug canvas
        debugCanvas.gameObject.SetActive(true);
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

    private bool CheckPlayerInSight(out IActor target)
    {
        target = null;
        
        // Return false if the player instance is null
        if (playerTransform.Value == null)
            return false;
        
        // Find the IActor component in the parent
        target = playerTransform.Value.GetComponentInParent<IActor>();

        // Get the line between the enemy and the player
        var line = playerTransform.Value.position - detectionOrigin.position;

        // Return false if the player is not within the vision distance
        // and the current state is not aware
        if (line.magnitude > visionDistance && CurrentDetectionState != EnemyDetectionState.Aware)
            return false;

        // If the player is within the auto-detection distance, return true
        if (line.magnitude <= autoDetectionDistance)
            return true;

        var angle = Vector3.Angle(transform.forward, line);

        // Return false if the player is not within the enemy's field of view
        if (angle > visionAngle)
            return false;

        // Create a layerMask to ignore the NonPhysical layer
        var layerMask = ~layersToIgnore;

        var currentVisionDistance = visionDistance;

        // If the current state is aware, increase the vision distance
        if (CurrentDetectionState == EnemyDetectionState.Aware)
            currentVisionDistance *= 4;

        var raycastHit = Physics.Raycast(
            detectionOrigin.position,
            line,
            out var hit,
            currentVisionDistance,
            layerMask
        );

        // Check a raycast to the player
        if (!raycastHit)
            return false;

        // Return false if the raycast does not hit the player
        if (hit.collider.gameObject != playerTransform.Value.gameObject)
            return false;

        return true;
    }

    #region Debugging

    private void OnDrawGizmosSelected()
    {
        // Draw a line from the enemy to the player
        var undetectedColor = Color.green;
        var detectedColor = Color.red;

        // Draw the vision angle
        var halfAngle = visionAngle / 2;
        var forward = transform.forward;

        var left = Quaternion.Euler(0, -halfAngle, 0) * forward;
        var right = Quaternion.Euler(0, halfAngle, 0) * forward;

        Gizmos.color = _isTargetInSight ? detectedColor : undetectedColor;
        Gizmos.DrawLine(detectionOrigin.position, detectionOrigin.position + left * visionDistance);
        Gizmos.DrawLine(detectionOrigin.position, detectionOrigin.position + right * visionDistance);

        // Draw the auto-detection distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(detectionOrigin.position, autoDetectionDistance);
    }

    #endregion

    public void SetDetectionEnabled(bool on)
    {
        IsDetectionEnabled = on;
    }
}