using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossChainLightningProjectile : MonoBehaviour
{
    private BossChainLightningBehavior _chainLightningBehavior;
    private Transform _target;

    public bool IsDoneBeingCreated { get; private set; }

    private bool _isActive;


    public IEnumerator CreateProjectile(BossChainLightningBehavior chainLightningBehavior, float time, Transform target)
    {
        // Set the chain lightning behavior
        _chainLightningBehavior = chainLightningBehavior;

        // Set the target
        _target = target;

        yield return new WaitForSeconds(time);

        // Set the is done being created flag to true
        IsDoneBeingCreated = true;
    }

    public IEnumerator ShootProjectile()
    {
        // Set the isActive flag to true
        _isActive = true;

        // Set the parent of the projectile to null
        transform.SetParent(null);

        // Start the update coroutine
        StartCoroutine(UpdateCoroutine());

        yield return null;
    }

    private IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            // Move forward
            transform.position += transform.forward *
                                  (_chainLightningBehavior.MaxMoveDistance * _chainLightningBehavior.MoveDelay);

            // Then change directions

            // First, set the forward to the direction of the target
            transform.forward = _target.position - transform.position;

            // Then create a random euler angle
            var euler = new Vector3(
                UnityEngine.Random.Range(-_chainLightningBehavior.MaxMoveAngle, _chainLightningBehavior.MaxMoveAngle),
                UnityEngine.Random.Range(-_chainLightningBehavior.MaxMoveAngle, _chainLightningBehavior.MaxMoveAngle),
                0
            );

            // Apply the euler angles to the forward direction
            transform.forward = Quaternion.Euler(euler) * transform.forward;

            // Then wait
            yield return new WaitForSeconds(_chainLightningBehavior.MoveDelay);
        }

        yield return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Return if not active
        if (!_isActive)
            return;

        // Return if the other collider is the shooter
        if (other.TryGetComponentInParent(out IEnemyAttackBehavior attackBehavior) &&
            (BossEnemyAttack)attackBehavior == _chainLightningBehavior.BossEnemyAttack)
            return;

        var hasActor = other.TryGetComponentInParent(out IActor actor);

        // Return if the actor is the shooter
        if (actor as EnemyInfo == _chainLightningBehavior.BossEnemyAttack.Enemy.EnemyInfo)
            return;

        // Return if the actor is an enemy
        if (actor is EnemyInfo)
            return;

        // Return if the other collider is not an actor
        if (!hasActor)
            return;

        Destroy(gameObject);

        var damage = _chainLightningBehavior.Damage * _chainLightningBehavior.BossEnemyAttack.ParentComponent
            .ParentComponent.DifficultyDamageMultiplier;

        // Damage the player
        actor.ChangeHealth(
            -damage, actor,
            _chainLightningBehavior.BossEnemyAttack, transform.position
        );
    }
}