using System;
using UnityEngine;

public class MeleeAttackHitbox : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private MeleeEnemyAttack meleeEnemyAttack;

    #endregion

    #region Private Fields

    private bool _isEnabled;

    #endregion

    private void Awake()
    {
        // Assert that the meleeEnemyAttack is not null
        Debug.Assert(meleeEnemyAttack != null, "The meleeEnemyAttack is null.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isEnabled)
            return;

        // Return if the other collider is not an actor
        // Try to get the actor component in the other collider
        if (!other.TryGetComponentInParent(out IActor actor))
            return;

        // Return if the actor is an enemy
        if (actor is EnemyInfo enemyInfo)
            return;

        Debug.Log($"Hit an actor! {actor.GameObject.name}");

        // Deal damage to the actor
        actor.ChangeHealth(-meleeEnemyAttack.Damage, meleeEnemyAttack.Enemy.EnemyInfo, meleeEnemyAttack);

        // Disable the hit box
        SetEnabled(false);
    }

    public void SetEnabled(bool on)
    {
        _isEnabled = on;
    }

    private void OnDrawGizmos()
    {
        // Return if not enabled
        if (!_isEnabled)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 1f);
    }
}