using System.Collections;
using UnityEngine;

public class BossExplosionBehavior : BossPowerBehavior
{
    [Header("Power"), SerializeField] private MultiDissolver explosionDissolver;
    [SerializeField] private ExplosionHelper explosionHelper;
    
    [SerializeField, Min(0)] private float chargeTime = 5f;
    [SerializeField, Range(0, 1)] private float explosionDissolveStrength;

    [SerializeField] private ParticleSystem fuseParticlesPrefab;
    
    private ParticleSystem _fuseParticles;
    
    protected override void CustomInitialize(BossEnemyAttack bossEnemyAttack)
    {
        // Make the renderers fully dissolved
        explosionDissolver.SetDissolveStrength(1);
    }

    protected override IEnumerator CustomUsePower()
    {
        // Start the fuse particles
        StartFuseParticles();
        
        // Set the movement mode to hard chase
        BossEnemyAttack.ParentComponent.SetBossBehaviorMode(BossBehaviorMode.HardChase);
        
        var startTime = Time.time;

        var target = 1 - explosionDissolveStrength;
        
        while (Time.time - startTime < chargeTime)
        {
            var percentage = (Time.time - startTime) / chargeTime;
            
            // Set the dissolve strength to the charge percentage
            explosionDissolver.SetDissolveStrength(1 - (percentage * target));

            // Charge the explosion
            yield return null;
        }

        // Set the dissolve strength to 0
        explosionDissolver.SetDissolveStrength(explosionDissolveStrength);

        // Set the movement mode to idle 
        BossEnemyAttack.ParentComponent.SetBossBehaviorMode(BossBehaviorMode.Idle);

        // Stop the fuse particles
        StopFuseParticles();
        
        for (var i = 0; i < 3; i++)
        {
            // Play the power ready particles
            PlayPowerReadyParticles();
        
            // Wait a sec
            yield return new WaitForSeconds(1);
        }
        
        // Set the dissolve strength to 0
        explosionDissolver.SetDissolveStrength(1);
        
        // Explode
        Explode();
        
        // Wait for a sec
        yield return new WaitForSeconds(1);
    }

    protected void Explode()
    {
        // Explode
        explosionHelper.Explode(true);
    }

    private void StartFuseParticles()
    {
        // Destroy the old particles if they exist
        if (_fuseParticles != null)
            Destroy(_fuseParticles.gameObject);
        
        // Return if the prefab is null
        if (fuseParticlesPrefab == null)
            return;
        
        _fuseParticles = Instantiate(fuseParticlesPrefab, transform);
    }

    private void StopFuseParticles()
    {
        // Return if the particles are null
        if (_fuseParticles == null)
            return;
        
        // Stop the particles
        _fuseParticles.Stop();
        
        // Destroy the particles
        Destroy(_fuseParticles.gameObject, 5);
    }
}