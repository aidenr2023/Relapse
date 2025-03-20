using System;
using UnityEngine;

[RequireComponent(typeof(EnemyInfo))]
public class DrunkardEnemy : ComponentScript<EnemyInfo>
{
    [SerializeField] private DrunkardEnemyAttack drunkardEnemyAttack;
    [SerializeField] private MeleeEnemyAttack meleeEnemyAttack;
    [SerializeField, Range(0, 1)] private float phaseChangePercent = .5f;
    [SerializeField] private ParticleSystem drunkardParticles;
    
    private IEnemyAttackBehavior _currentAttackBehavior;

    private void Start()
    {
        // Change the attack behavior to the melee attack
        ChangeAttackBehavior(meleeEnemyAttack);

        // Subscribe to the OnDamaged event
        ParentComponent.OnDamaged += ActivatePhaseChange;
        
        // Stop the particles
        drunkardParticles?.Stop();
    }

    private void ActivatePhaseChange(object sender, HealthChangedEventArgs e)
    {
        var healthPercent = ParentComponent.CurrentHealth / ParentComponent.MaxHealth;

        // Change the attack behavior to the drunkard attack
        if (healthPercent > phaseChangePercent)
            return;

        ChangeAttackBehavior(drunkardEnemyAttack);
        drunkardEnemyAttack.Activate();

        // Unsubscribe from the OnDamaged event
        ParentComponent.OnDamaged -= ActivatePhaseChange;

        // Start the particles
        drunkardParticles?.Play();
    }

    private void ChangeAttackBehavior(IEnemyAttackBehavior newBehavior)
    {
        // Create an array of all the attack behaviors
        var attackBehaviors = new IEnemyAttackBehavior[]
        {
            drunkardEnemyAttack, meleeEnemyAttack
        };

        // Iterate through all of them and disable the ones that aren't the current one
        foreach (var cBehavior in attackBehaviors)
        {
            // Skip the current behavior
            if (cBehavior == newBehavior)
                continue;

            // // Disable the attack behavior
            // (cBehavior as MonoBehaviour)!.enabled = false;
            
            // Add the attack disable token to the current behavior
            cBehavior.AddAttackDisableToken(this);
        }

        // // Enable the new behavior
        // (newBehavior as MonoBehaviour)!.enabled = true;
        
        // Remove the attack disable token from the new behavior
        newBehavior.RemoveAttackDisableToken(this);

        // Set the current attack behavior
        _currentAttackBehavior = newBehavior;
        
        // Set the current movement behavior to the new behavior's movement behavior
        ParentComponent.ParentComponent.Brain.BehaviorMode = newBehavior switch
        {
            MeleeEnemyAttack => (int)DrunkardBehavior.Melee,
            DrunkardEnemyAttack => (int)DrunkardBehavior.Drunkard,
            _ => ParentComponent.ParentComponent.Brain.BehaviorMode
        };

        Debug.Log($"Current attack behavior: {ParentComponent.ParentComponent.Brain.BehaviorMode}");
    }

    public enum DrunkardBehavior : byte
    {
        Melee,
        Drunkard,
    }
}