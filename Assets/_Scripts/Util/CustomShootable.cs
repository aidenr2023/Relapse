using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class CustomShootable : MonoBehaviour, IActor
{
    #region Serialized Fields

    [SerializeField, Min(0)] private int hitsToActivate = 1;
    [SerializeField] private bool destroyOnActivation;
    [SerializeField] private UnityEvent onActivation;

    #endregion

    #region Private Fields

    [SerializeField, Readonly] private bool hasBeenActivated;

    #endregion

    #region Getters

    public GameObject GameObject => gameObject;

    public float MaxHealth { get; } = 900;
    public float CurrentHealth { get; private set; }

    public HealthChangedEventReference OnDamaged { get; set; }
    public HealthChangedEventReference OnHealed { get; set; }
    public HealthChangedEventReference OnDeath { get; set; }

    #endregion

    private void Awake()
    {
        // Set the current health to the max health
        CurrentHealth = MaxHealth;

        // Initialize the events
        InitializeEvents();
    }

    private void InitializeEvents()
    {
        // Force the events to be set to constant
        OnDamaged.ForceUseConstant();
        OnHealed.ForceUseConstant();
        OnDeath.ForceUseConstant();
        
        // Subscribe to the OnDeath event
        OnDeath += ActivateEventOnDeath;

        OnDamaged += (_, e) => { Debug.Log($"Damaged by {e.DamagerObject}"); };
    }

    private void ActivateEventOnDeath(object sender, HealthChangedEventArgs e)
    {
        // If the actor has already been activated, return
        if (hasBeenActivated)
            return;

        // If the actor has been activated, set the has been activated flag to true
        hasBeenActivated = true;

        // Invoke the onActivation event
        onActivation.Invoke();

        // If the destroyOnActivation flag is true, destroy the game object
        if (destroyOnActivation)
            Destroy(gameObject);
    }

    public void ChangeHealth(float amount, IActor changer, IDamager damager, Vector3 position, bool isCriticalHit = false)
    {
        // If the amount is negative, the actor is taking damage
        if (amount < 0)
        {
            amount = MaxHealth / hitsToActivate;

            // Decrease the current health by the amount
            CurrentHealth -= amount;

            // Create a new HealthChangedEventArgs object
            var eventArgs = new HealthChangedEventArgs(this, changer, damager, amount, position, isCriticalHit);

            // If the current health is less than or equal to 0
            // Invoke the OnDeath event
            if (CurrentHealth <= 0)
                OnDeath?.Value.Invoke(this, eventArgs);
            // Invoke the OnDamaged event
            else
                OnDamaged?.Value.Invoke(this, eventArgs);
        }
        // If the amount is positive, the actor is being healed
        else if (amount > 0)
        {
            // Increase the current health by the amount
            CurrentHealth += amount;

            // If the current health is greater than the max health
            // Set the current health to the max health
            if (CurrentHealth > MaxHealth)
                CurrentHealth = MaxHealth;

            // Invoke the OnHealed event
            // Create a new HealthChangedEventArgs object
            var eventArgs = new HealthChangedEventArgs(this, changer, damager, amount, position);
            OnHealed?.Value.Invoke(this, eventArgs);
        }
    }
}