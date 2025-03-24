using System;
using System.Collections;
using UnityEngine;

public abstract class BossPowerBehavior : MonoBehaviour
{
    [field: SerializeField] public BossPowerScriptableObject BossPower { get; private set; }
    [field: SerializeField, Readonly] public bool IsActive { get; set; } = false;

    public BossEnemyAttack BossEnemyAttack { get; private set; }

    [field: SerializeField] public BossAttackMode AttackMode { get; private set; }

    [SerializeField] protected Transform powerReadyParticlesTransform;
    [SerializeField] protected ParticleSystem powerReadyParticlesPrefab;
    [SerializeField, Min(0)] protected int powerReadyParticlesCount = 50;

    private void Awake()
    {
        BossEnemyAttack = GetComponentInParent<BossEnemyAttack>();
    }

    public void Initialize(BossEnemyAttack bossEnemyAttack)
    {
        // Set the boss enemy attack
        BossEnemyAttack = bossEnemyAttack;

        // Custom initialization
        CustomInitialize(bossEnemyAttack);
    }

    protected abstract void CustomInitialize(BossEnemyAttack bossEnemyAttack);

    /// <summary>
    /// A coroutine that is called when the power is used.
    /// Returns when the power is done being used.
    /// </summary>
    /// <returns></returns>
    public IEnumerator UsePower()
    {
        yield return StartCoroutine(CustomUsePower());
    }

    protected abstract IEnumerator CustomUsePower();

    protected void PlayPowerReadyParticles()
    {
        // Return if the prefab is null
        if (powerReadyParticlesPrefab == null)
            return;

        // Instantiate a new particle system
        var particles = Instantiate(
            powerReadyParticlesPrefab,
            powerReadyParticlesTransform
        );

        // // Play the particles
        // particles.Play();

        // Emit the particles
        particles.Emit(powerReadyParticlesCount);
        
        // Destroy the particles after 10 seconds
        Destroy(particles.gameObject, 10);
    }
}