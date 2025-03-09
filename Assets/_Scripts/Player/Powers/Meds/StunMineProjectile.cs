using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class StunMineProjectile : AbstractMineProjectile
{
    [Header("Unique Stats")] 
    
    [SerializeField, Min(0)] protected float stunDuration = 5f;
    [SerializeField] private VisualEffect stunVfxPrefab;

    [SerializeField] private ParticleSystem explosionParticles;
    [SerializeField, Min(0)] private int particleCount = 50;
    
    [SerializeField] private VisualEffect explosionVfxPrefab;
    
    protected override void CustomAwake()
    {
    }

    protected override void CustomUpdate()
    {
    }

    protected override void CustomFixedUpdate()
    {
    }

    protected override void OnStartFuse()
    {
    }

    protected override void ApplyExplosion(IActor actor)
    {
        if (actor is not EnemyInfo enemyInfo)
            return;

        var movement = enemyInfo.ParentComponent.NewMovement;
        var attackBehavior = enemyInfo.ParentComponent.AttackBehavior;

        // Apply the stun effect
        // Call this from the power logic game object because this projectile is destroyed after the explosion
        ((MonoBehaviour)_power.PowerScriptableObject.PowerLogic)
            .StartCoroutine(ApplyStun(movement, attackBehavior));
        
        // Create the explosion particles
        CreateExplosionParticles(explosionParticles, transform.position, particleCount);
        
        // Create the explosion VFX
        CreateExplosionVfx(explosionVfxPrefab, transform.position);
    }

    private IEnumerator ApplyStun(NewEnemyMovement movement, IEnemyAttackBehavior attackBehavior)
    {
        // // Add a movement disabled token to the enemy for the duration of the chain stop time
        // enemy.MovementBehavior.AddMovementDisableToken(this);
        // enemy.AttackBehavior.AddAttackDisableToken(this);

        var powerManager = (_shooter as PlayerInfo)!.ParentComponent.PlayerPowerManager;
        
        var enemy = movement.ParentComponent;
        
        // Create event args
        var args = new HealthChangedEventArgs(
            enemy.EnemyInfo, powerManager.Player.PlayerInfo, _power,
            0, enemy.transform.position
        );

        enemy.EnemyInfo.Stun(args, stunDuration);
        
        // Instantiate the stun VFX
        var stunVfx = Instantiate(stunVfxPrefab, movement.transform);

        // Wait until the stun duration is over
        var timeRemaining = stunDuration;
        while (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            yield return null;
        }

        enemy.EnemyInfo.StopStun();
        
        // // Re-enable the movement & attack
        // movementBehavior?.RemoveMovementDisableToken(this);
        // attackBehavior?.RemoveAttackDisableToken(this);
        
        // Destroy the stun VFX
        Destroy(stunVfx.gameObject);
    }

    protected override void CustomShoot(IPower power, PlayerPowerManager powerManager, PowerToken pToken,
        Vector3 position, Vector3 forward)
    {
    }
}