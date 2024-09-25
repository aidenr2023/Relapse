using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playerinfo : MonoBehaviour, IActor
{
    public WinLose winLose; // Reference to the WinLose script

    [Header("Health Settings")] public float maxHealth = 3f;
    public float health;

    // TODO: Eventually, I might move this code to another script.
    // For now though, I'm keeping this here to make things easier
    // -Dimitri
    [Header("Tolerance Meter Settings")] [SerializeField]
    private float maxTolerance;

    public TolereanceMeter tolereanceMeter;

    [SerializeField] private float currentTolerance;
    
    private InputUserHandler _inputUserHandler;

    #region Getters

    public float MaxHealth => maxHealth;

    public float CurrentHealth => health;

    public float MaxTolerance => maxTolerance;

    public float CurrentTolerance => currentTolerance;

    #endregion
    
    #region Initialization Functions

    void Start()
    {
        // Set the player's health to the max health
        health = maxHealth;

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
        
        // Prevent the tolerance from going below 0 or above the max value
        ClampTolerance();

        if (maxTolerance > 0)
        {
            tolereanceMeter.UpdateToleranceUI(currentTolerance / maxTolerance); // Scale to 0-1
        }
    }

    private void OnDestroy()
    {
        // Remove all input
        _inputUserHandler.RemoveAll();
    }

    private void TakeDamage(float damageAmount)
    {
        health = Mathf.Clamp(health - damageAmount, 0, maxHealth);

        if (health <= 0)
        {
            // Trigger the lose condition
            if (winLose != null)
            {
                winLose.ShowLoseScreen();
                Debug.Log("Player died!");
            }
        }
    }

    private void ClampTolerance()
    {
        currentTolerance = Mathf.Clamp(currentTolerance, 0, maxTolerance);
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

    public void ChangeTolerance(float amount)
    {
        currentTolerance = Mathf.Clamp(currentTolerance + amount, 0, maxTolerance);
        tolereanceMeter.UpdateToleranceUI(currentTolerance / maxTolerance); // Scale the dial
    }

    
}