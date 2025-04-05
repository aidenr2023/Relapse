using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

[RequireComponent(typeof(Rigidbody))]
public class ExplosiveMineProjectile : AbstractMineProjectile
{
    [Header("Unique Stats")] [SerializeField]
    private ExplosionHelper explosionHelper;

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

    protected override void DoExplosion()
    {
        explosionHelper.Explode(explosionHelper.Damage, _shooter, explosionHelper, false);
    }

    protected override void ApplyExplosion(IActor actor)
    {
        // explosionHelper.Explode(explosionHelper.Damage, _shooter, explosionHelper, false);
        
        // // Change the health of all those caught in the explosion radius
        // actor.ChangeHealth(-damage, _shooter, _power, actor.GameObject.transform.position);
        //
        // CreateExplosionParticles(explosionParticles, transform.position, particleCount);
        //
        // // Create the explosion VFX
        // CreateExplosionVfx(explosionVfxPrefab, transform.position);
    }

    protected override void CustomShoot(IPower power, PlayerPowerManager powerManager, PowerToken pToken,
        Vector3 position, Vector3 forward)
    {
    }
}