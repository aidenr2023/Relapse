using System;
using UnityEngine;

[RequireComponent(typeof(EnemyInfo))]
public class DrunkardEnemy : ComponentScript<EnemyInfo>
{
    [SerializeField] private DrunkardEnemyAttack drunkardEnemyAttack;
    [SerializeField, Range(0, 1)] private float phaseChangePercent = .5f;
    [SerializeField] private ParticleSystem drunkardParticles;

    private (short min, short max) _particleBurstCount = (0, 0);

    private void Start()
    {
        // Subscribe to the OnDamaged event
        ParentComponent.OnDamaged += ActivatePhaseChange;

        // Stop the particles
        drunkardParticles?.Stop();

        // Force run the activate phase change method to check if the phase change should be activated
        ActivatePhaseChange(null, null);

        // Get the burst count of the first burst
        var burst = drunkardParticles.emission.GetBurst(0);
        _particleBurstCount = (burst.minCount, burst.maxCount);
        burst.minCount = burst.maxCount = 0;

        // Set the burst
        drunkardParticles.emission.SetBurst(0, burst);

        // Start the particles by default
        drunkardParticles.Play();
    }

    private void ActivatePhaseChange(object sender, HealthChangedEventArgs e)
    {
        var healthPercent = ParentComponent.CurrentHealth / ParentComponent.MaxHealth;

        // Change the attack behavior to the drunkard attack
        if (healthPercent > phaseChangePercent)
            return;

        drunkardEnemyAttack.Activate();

        // Unsubscribe from the OnDamaged event
        ParentComponent.OnDamaged -= ActivatePhaseChange;

        // // Start the particles
        // drunkardParticles?.Play();
    }

    public void StartParticles()
    {
        drunkardParticles?.Play();

        // Set the burst count to the original value
        SetBurstCount(_particleBurstCount.min, _particleBurstCount.max);
    }

    public void SetBurstCount(short min, short max)
    {
        // Get the first burst
        var burst = drunkardParticles.emission.GetBurst(0);

        burst.minCount = min;
        burst.maxCount = max;

        // Set the burst
        drunkardParticles.emission.SetBurst(0, burst);
    }
}