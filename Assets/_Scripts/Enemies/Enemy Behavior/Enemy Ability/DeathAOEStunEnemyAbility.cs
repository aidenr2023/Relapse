using System;
using System.Collections.Generic;
using UnityEngine;

public class DeathAOEStunEnemyAbility : ComponentScript<Enemy>, IEnemyAbilityBehavior, IDamager
{
    #region Serialized Fields

    [SerializeField, Min(0)] private float explosionRadius = 10;
    [SerializeField, Min(0)] private float explosionDamage = 40;
    [SerializeField, Min(0)] private float explosionForce = 100;

    [SerializeField] private AnimationCurve damageFalloff = AnimationCurve.Linear(0, 1, 1, 0);

    [SerializeField, Min(0)] private float stunDuration = 2f;

    #endregion

    #region Private Fields

    #endregion

    #region Getters

    public Enemy Enemy => ParentComponent;
    public GameObject GameObject => gameObject;
    
    public Sound NormalHitSfx => null;
    public Sound CriticalHitSfx => null;

    #endregion

    protected override void CustomAwake()
    {
        base.CustomAwake();
    }

    private void Start()
    {
        // Bind to the onDeath event of the enemy
        Enemy.EnemyInfo.OnDeath += ExplodeOnDeath;
    }

    private void ExplodeOnDeath(object sender, HealthChangedEventArgs e)
    {
        // Call the explosion method
        Explode();
    }

    private void Explode()
    {
        // Get all colliders in the explosion radius
        var colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        var actors = new HashSet<IActor>();

        // Loop through all colliders
        foreach (var cCollider in colliders)
        {
            // Continue if the collider does not have an IActor component somewhere in the parent
            if (!cCollider.TryGetComponentInParent(out IActor actor))
                continue;

            // Continue if the actor is already dead
            if (actor.CurrentHealth <= 0)
                continue;

            // Continue if the actor is the enemy itself
            if (actor as EnemyInfo == Enemy.EnemyInfo)
                continue;

            // Add the actor to the list of actors
            actors.Add(actor);
        }

        // For each actor in the list
        foreach (var actor in actors)
        {
            // Get the distance between the source of the explosion and the actor
            var distance = Vector3.Distance(transform.position, actor.GameObject.transform.position);

            // Evaluate the damage falloff curve
            var distancePercentage = Mathf.Clamp01(distance / explosionRadius);
            var damageFalloffValue = damageFalloff.Evaluate(distancePercentage);

            // Have the damage fall off with distance
            var damage = Mathf.Ceil(explosionDamage * damageFalloffValue);

            // Damage the actor
            actor.ChangeHealth(-damage, Enemy.EnemyInfo, this, actor.GameObject.transform.position);

            // Get the movement behavior of the actor
            if (!actor.GameObject.TryGetComponent(out IEnemyMovementBehavior movementBehavior))
                continue;

            movementBehavior.AddMovementSpeedToken(0, stunDuration);
        }

        Debug.Log($"{gameObject.name} exploded! {actors.Count} actors affected");
    }
}