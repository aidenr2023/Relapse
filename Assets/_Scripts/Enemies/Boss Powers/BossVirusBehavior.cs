using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossVirusBehavior : BossPowerBehavior
{
    private readonly HashSet<IActor> _infectedActors = new();

    #region Serialized Fields

    [SerializeField] private BossVirusProjectile virusPrefab;
    [field: SerializeField] public BossVirusCloud VirusCloud { get; private set; }
    [SerializeField] private Transform firePoint;

    [SerializeField, Min(0)] private float attackStartupTime = 3f;

    [field: SerializeField] public float ProjectileDamage { get; private set; }
    [field: SerializeField] public float ProjectileVelocity { get; private set; }
    [field: SerializeField, Min(0)] public float TickDamage { get; private set; } = 10;
    [field: SerializeField, Min(0)] public float TickDelay { get; private set; } = 1;
    [field: SerializeField, Min(0)] public float TickDuration { get; private set; } = 5;

    [SerializeField] private float virusProjectileDuration;
    [field: SerializeField] public float VirusCloudDuration { get; private set; }

    #endregion

    #region Private Fields

    private BossVirusProjectile _virusObj;

    #endregion

    protected override void CustomInitialize(BossEnemyAttack bossEnemyAttack)
    {
    }

    protected override IEnumerator CustomUsePower()
    {
        // Set the movement mode to idle
        BossEnemyAttack.ParentComponent.SetBossBehaviorMode(BossBehaviorMode.Idle);
        
        // Create the projectile 
        yield return StartCoroutine(CreateProjectile());

        // Set the movement mode to chase maintain distance
        BossEnemyAttack.ParentComponent.SetBossBehaviorMode(BossBehaviorMode.ChaseMaintainDistance);
        
        // Play the power ready particles
        PlayPowerReadyParticles();
        
        // Wait for a second
        yield return new WaitForSeconds(1);
        
        // Shoot the projectile
        yield return StartCoroutine(ShootProjectile());

        // Wait for a second
        yield return new WaitForSeconds(1);
    }

    private IEnumerator CreateProjectile()
    {
        // Instantiate the virus projectile
        _virusObj = Instantiate(virusPrefab, firePoint);

        var targetTransform = BossEnemyAttack.Enemy.DetectionBehavior.Target.GameObject.transform;

        yield return StartCoroutine(_virusObj.CreateProjectile(this, attackStartupTime, targetTransform));

        // Destroy the projectile after a certain amount of time
        Destroy(_virusObj.gameObject, virusProjectileDuration);

        yield return null;
    }

    private IEnumerator ShootProjectile()
    {
        // Shoot the projectile
        yield return StartCoroutine(_virusObj.ShootProjectile());

        yield return null;
    }

    public void AddInfectedActor(IActor actor)
    {
        _infectedActors.Add(actor);
    }

    public void RemoveInfectedActor(IActor actor)
    {
        _infectedActors.Remove(actor);
    }

    public bool IsActorInfected(IActor actor)
    {
        return _infectedActors.Contains(actor);
    }
}