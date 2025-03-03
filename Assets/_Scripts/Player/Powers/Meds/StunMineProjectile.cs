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
        if (actor is not EnemyInfo enemy)
            return;

        var movementBehavior = enemy.GetComponent<IEnemyMovementBehavior>();
        var attackBehavior = enemy.GetComponent<IEnemyAttackBehavior>();

        // Apply the stun effect
        // Call this from the power logic game object because this projectile is destroyed after the explosion
        ((MonoBehaviour)_power.PowerScriptableObject.PowerLogic)
            .StartCoroutine(ApplyStun(movementBehavior, attackBehavior));
        
        // Create the explosion particles
        CreateExplosionParticles(explosionParticles, transform.position, particleCount);
        
        // Create the explosion VFX
        CreateExplosionVfx(explosionVfxPrefab, transform.position);
    }

    private IEnumerator ApplyStun(IEnemyMovementBehavior movementBehavior, IEnemyAttackBehavior attackBehavior)
    {
        // Disable the movement & attack
        movementBehavior.AddMovementDisableToken(this);
        attackBehavior.AddAttackDisableToken(this);
        
        // Instantiate the stun VFX
        var stunVfx = Instantiate(stunVfxPrefab, movementBehavior.GameObject.transform);

        // Wait until the stun duration is over
        var timeRemaining = stunDuration;
        while (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            yield return null;
        }

        // Re-enable the movement & attack
        movementBehavior?.RemoveMovementDisableToken(this);
        attackBehavior?.RemoveAttackDisableToken(this);
        
        // Destroy the stun VFX
        Destroy(stunVfx.gameObject);
    }

    protected override void CustomShoot(IPower power, PlayerPowerManager powerManager, PowerToken pToken,
        Vector3 position, Vector3 forward)
    {
    }
}