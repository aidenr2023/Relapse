using System.Collections;
using UnityEngine;

public class BossExplosionPower : BossPowerBehavior
{
    [SerializeField] private MultiDissolver explosionDissolver;
    [SerializeField] private ExplosionHelper explosionHelper;
    
    [SerializeField, Min(0)] private float chargeTime = 5f;
    [SerializeField, Range(0, 1)] private float explosionDissolveStrength;

    protected override void CustomInitialize(BossEnemyAttack bossEnemyAttack)
    {
        // Make the renderers fully dissolved
        explosionDissolver.SetDissolveStrength(1);
    }

    protected override IEnumerator CustomUsePower()
    {
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

        // Wait a sec
        yield return new WaitForSeconds(1);
        
        // Set the dissolve strength to 0
        explosionDissolver.SetDissolveStrength(1);
        
        // Explode
        Explode();
        
        yield return null;
    }

    protected void Explode()
    {
        // Explode
        explosionHelper.Explode(true);
    }
}