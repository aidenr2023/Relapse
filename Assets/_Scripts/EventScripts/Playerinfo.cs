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

    [SerializeField] private float currentTolerance;

    #region Getters

    public float MaxHealth => maxHealth;

    public float CurrentHealth => health;

    public float MaxTolerance => maxTolerance;

    public float CurrentTolerance => currentTolerance;

    #endregion

    void Start()
    {
        health = maxHealth;
    }

    private void Update()
    {
        // Prevent the tolerance from going below 0 or above the max value
        ClampTolerance();
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
}