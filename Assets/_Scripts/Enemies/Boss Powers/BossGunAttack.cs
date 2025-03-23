using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossGunAttack : BossPowerBehavior
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

        for (var i = 0; i < shotCount; i++)
        {
            // Wait for the attack cooldown
            yield return new WaitForSeconds(attackCooldown);

            // Fire the projectile
            FireProjectile();
        }
        
        // Wait for the attack cooldown
        yield return new WaitForSeconds(attackCooldown);
        
        Debug.Log($"Finished using {BossPower?.name ?? "GUN"}");
    }

    private void FireProjectile()
    {
        // Shoot at the player
        CreateProjectile();
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