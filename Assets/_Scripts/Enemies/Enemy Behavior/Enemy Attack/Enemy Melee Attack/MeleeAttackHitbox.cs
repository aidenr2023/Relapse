using System;
using UnityEngine;

public class MeleeAttackHitbox : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private MeleeEnemyAttack meleeEnemyAttack;
    [SerializeField] private ManagedAudioSource attackSound;
    [SerializeField] private Sound attackSoundClip;
    [SerializeField] private TrailRenderer trailRenderer;

    #endregion

    #region Private Fields

    public bool IsEnabled { get; private set; }

    #endregion

    private void Awake()
    {
        // Assert that the meleeEnemyAttack is not null
        Debug.Assert(meleeEnemyAttack != null, "The meleeEnemyAttack is null.");

        // Set the attack sound to be permanent
        attackSound.SetPermanent(true);

        // Stop the trail renderer
        StopTrail();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsEnabled)
            return;

        // Return if the other collider is not an actor
        // Try to get the actor component in the other collider
        if (!other.TryGetComponentInParent(out IActor actor))
            return;

        // Return if the actor is an enemy
        if (actor is EnemyInfo)
            return;

        // Deal damage to the actor
        actor.ChangeHealth(-meleeEnemyAttack.Damage, meleeEnemyAttack.Enemy.EnemyInfo, meleeEnemyAttack,
            transform.position
        );

        // Disable the hit box
        SetEnabled(false);
    }

    public void SetEnabled(bool on)
    {
        IsEnabled = on;
    }

    private void OnDrawGizmos()
    {
        // Return if not enabled
        if (!IsEnabled)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 1f);
    }

    public void PlayAttackSound()
    {
        attackSound.SetPermanent(true);
        attackSound.Play(attackSoundClip);
    }

    public void PlayTrail()
    {
        // Return if the trail renderer is null
        if (trailRenderer == null)
            return;

        trailRenderer.emitting = true;
    }

    public void StopTrail()
    {
        // Return if the trail renderer is null
        if (trailRenderer == null)
            return;

        trailRenderer.emitting = false;
    }
}