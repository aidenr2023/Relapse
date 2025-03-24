using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossGunBehavior : BossPowerBehavior
{
    #region Serialized Fields

    [Header("Power"), SerializeField] private Transform firePoint;
    [SerializeField] private ShootingEnemyProjectile bulletPrefab;
    [SerializeField, Min(0)] private float fireRange = 20f;
    [SerializeField, Min(0)] private float projectileSpeed = 8f;
    [SerializeField, Min(0)] private float attackCooldown = 3f;
    [SerializeField, Min(0)] private float projectileLifetime = 5f;

    #endregion

    #region Getters

    public GameObject GameObject => gameObject;
    public Sound NormalHitSfx => null;
    public Sound CriticalHitSfx => null;

    #endregion

    protected override void CustomInitialize(BossEnemyAttack bossEnemyAttack)
    {
    }

    protected override IEnumerator CustomUsePower()
    {
        // random number from 5 to 10;
        var shotCount = Random.Range(5, 11);

        // Play the power ready sound
        SoundManager.Instance.PlaySfxAtPoint(powerReadySound, transform.position);
        
        for (var i = 0; i < shotCount; i++)
        {
            // Set the boss's behavior mode to chase, maintain distance
            BossEnemyAttack.ParentComponent.SetBossBehaviorMode(BossBehaviorMode.ChaseMaintainDistance);

            // Wait for the attack cooldown
            yield return new WaitForSeconds(attackCooldown);

            // Set the boss's behavior mode to strafe left, right, back
            BossEnemyAttack.ParentComponent.SetBossBehaviorMode(BossBehaviorMode.StrafeLeftRightBack);

            // Play the power release sound
            SoundManager.Instance.PlaySfxAtPoint(powerReleaseSound, transform.position);
            
            // Fire the projectile
            yield return StartCoroutine(FireProjectile());
        }
        
        // Set the boss's behavior mode to strafe left, right, back
        BossEnemyAttack.ParentComponent.SetBossBehaviorMode(BossBehaviorMode.StrafeLeftRightBack);
        
        // Wait for the attack cooldown
        yield return new WaitForSeconds(attackCooldown);

        Debug.Log($"Finished using {BossPower?.name ?? "GUN"}");
    }

    private IEnumerator FireProjectile()
    {
        // Shoot at the player
        CreateProjectile();

        yield return null;
    }

    private void CreateProjectile()
    {
        // Return if the spawn point is null
        if (firePoint == null)
            return;

        // Instantiate the bullet
        var bulletObj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // Calculate the direction of the bullet
        var direction = BossEnemyAttack.Enemy.DetectionBehavior.LastKnownTargetPosition - firePoint.position;

        // Call the shoot method on the bullet
        bulletObj.Shoot(BossEnemyAttack, direction, projectileSpeed, projectileLifetime);
    }
}