using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour, IActor
{
    public WinLose winLose; // Reference to the WinLose script

    [Header("Health Settings")] [SerializeField]
    private float maxHealth = 3f;

    [SerializeField] private float health;

    // TODO: Eventually, I might move this code to another script.
    // For now though, I'm keeping this here to make things easier
    // -Dimitri
    [Header("Tolerance Meter Settings")] [SerializeField]
    private float maxTolerance;

    [SerializeField] private float currentTolerance;
    [SerializeField] private TolereanceMeter tolereanceMeter;

    /// <summary>
    /// The number of times the player has relapsed during this level.
    /// </summary>
    private int _relapseCount;

    [Tooltip("The number of relapses the player can have before losing the level.")] [SerializeField]
    private int relapsesToLose = 3;

    [Tooltip("What percent is the tolerance meter set to when the player relapses?")] [Range(0, 1)] [SerializeField]
    private float toleranceRelapsePercent = .75f;

    [Header("Relapsing Settings")] [SerializeField] [Min(0)]
    private float relapseDuration = 3;

    private float _currentRelapseDuration;

    private bool _isRelapsing;

    /// <summary>
    /// An event that is called when the player relapses.
    /// Used mostly to connect to outside scripts.
    /// </summary>
    public Action<PlayerInfo> OnRelapseStart;

    /// <summary>
    /// An event that is called when the player's relapse ends.
    /// </summary>
    public Action<PlayerInfo> OnRelapseEnd;

    private InputUserHandler _inputUserHandler;

    #region Getters

    public GameObject GameObject => gameObject;

    public float MaxHealth => maxHealth;

    public float CurrentHealth => health;

    public float MaxTolerance => maxTolerance;

    public float CurrentTolerance => currentTolerance;

    public bool IsRelapsing => _isRelapsing;

    #endregion

    #region Initialization Functions

    void Start()
    {
        // Set the player's health to the max health
        // health = maxHealth;

        // Find the tolerance meter in the scene
        if (tolereanceMeter == null)
            tolereanceMeter = FindObjectOfType<TolereanceMeter>();

        // If the tolerance meter is still null, log an error
        if (tolereanceMeter == null)
            Debug.LogError("TolereanceMeter is not assigned and could not be found.");

        // Initialize the input handler
        InitializeInput();
    }

    private void InitializeInput()
    {
        // Create the input handler
        _inputUserHandler = new InputUserHandler(gameObject);
    }

    #endregion

    private void Update()
    {
        // Update the input users
        _inputUserHandler.UpdateInputUsers();

        // Update the relapse duration
        UpdateRelapseDuration();

        // Prevent the tolerance from going below 0 or above the max value
        ClampTolerance();

        if (maxTolerance > 0)
            tolereanceMeter.UpdateToleranceUI(currentTolerance / maxTolerance); // Scale to 0-1
    }

    private void UpdateRelapseDuration()
    {
        // Return if the player isn't relapsing
        if (!_isRelapsing)
            return;

        // Increment the relapse duration
        _currentRelapseDuration += Time.deltaTime;

        // Reset the tolerance meter after a relapse
        var targetTolerance = maxTolerance * toleranceRelapsePercent;
        var toleranceDifference = targetTolerance - maxTolerance;
        var toleranceDifferencePerSecond = toleranceDifference / relapseDuration;
        ChangeTolerance(toleranceDifferencePerSecond * Time.deltaTime);

        // If the relapse duration is greater than the relapse duration, end the relapse
        if (_currentRelapseDuration >= relapseDuration)
            EndRelapse();
    }

    private void OnDestroy()
    {
        // Remove all input
        _inputUserHandler?.RemoveAll();
    }

    public void ChangeHealth(float amount)
    {
        // If the amount is negative, the player is taking damage
        if (amount < 0)
            TakeDamage(-amount);

        // If the amount is positive, the player is gaining health
        else
            health = Mathf.Clamp(health + amount, 0, maxHealth);
    }

    private void TakeDamage(float damageAmount)
    {
        health = Mathf.Clamp(health - damageAmount, 0, maxHealth);

        if (health <= 0)
        {
            // Trigger the lose condition
            if (winLose != null)
                winLose.Lose("The Player Died!");
        }
    }

    private void ClampTolerance()
    {
        currentTolerance = Mathf.Clamp(currentTolerance, 0, maxTolerance);
    }

    public void ChangeTolerance(float amount)
    {
        currentTolerance = Mathf.Clamp(currentTolerance + amount, 0, maxTolerance);
        tolereanceMeter.UpdateToleranceUI(currentTolerance / maxTolerance); // Scale the dial

        // The player will relapse if the tolerance meter is too high
        if (currentTolerance >= maxTolerance)
            StartRelapse();
    }

    private void StartRelapse()
    {
        // Skip if the player is already relapsing
        if (_isRelapsing)
            return;

        // Set the isRelapsing flag to true
        _isRelapsing = true;

        // Increase the relapse count
        _relapseCount++;

        if (_relapseCount >= relapsesToLose)
        {
            // The player dies / restarts the level from relapsing too many times!
            DieFromRelapse();

            return;
        }

        // Reset the relapse duration
        _currentRelapseDuration = 0;

        // Invoke the relapse event
        OnRelapseStart?.Invoke(this);
    }

    private void EndRelapse()
    {
        // Set the isRelapsing flag to false
        _isRelapsing = false;

        // Reset the relapse duration
        _currentRelapseDuration = 0;

        // Invoke the end relapse event
        OnRelapseEnd?.Invoke(this);
    }

    private void DieFromRelapse()
    {
        // Trigger the lose condition
        if (winLose != null)
            winLose.Lose("Player relapsed too many times!");
    }
}