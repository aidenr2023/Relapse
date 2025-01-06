using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class ExplosiveMineProjectile : AbstractMineProjectile
{
    [Header("Unique Stats")]
    [SerializeField, Min(0)] protected float damage = 50f;

    protected override void OnStartFuse()
    {
    }

    protected override void ApplyExplosion(IActor actor)
    {
        // Change the health of all those caught in the explosion radius
        actor.ChangeHealth(-damage, _shooter, _power, actor.GameObject.transform.position);
    }

    protected override void CustomShoot(IPower power, PlayerPowerManager powerManager, PowerToken pToken,
        Vector3 position, Vector3 forward)
    {
    }
}