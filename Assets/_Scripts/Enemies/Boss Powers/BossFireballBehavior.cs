using System;
using System.Collections;
using UnityEngine;

public class BossFireballBehavior : BossPowerBehavior
{
    [Header("Power"), SerializeField] private Transform firePoint;
    [SerializeField] private BossFireballProjectile bulletPrefab;
    [SerializeField, Min(0)] private float attackStartupTime = 3f;

    [SerializeField] private float projectileVelocity = 32f;
    [SerializeField] private float projectileLifetime = 10f;

    #region Private Fields

    private BossFireballProjectile _bulletObj;

    #endregion

    // Fireball projectile prefab
    protected override void CustomInitialize(BossEnemyAttack bossEnemyAttack)
    {
    }

    protected override IEnumerator CustomUsePower()
    {
        Debug.Log($"Creating {BossPower?.name}");

        // Create the projectile
        yield return StartCoroutine(CreateProjectile());

        var startTime = Time.time;

        // Wait a second before shooting the projectile
        while (Time.time - startTime < 1)
        {
            var forward = BossEnemyAttack.Enemy.DetectionBehavior.LastKnownTargetPosition - firePoint.position;
            
            // Set the forward of the projectile to the direction of the projectile
            _bulletObj.transform.forward = forward;
            yield return null;
        }
        
        Debug.Log($"Shooting {BossPower?.name}");

        // Then, shoot the projectile
        yield return StartCoroutine(ShootProjectile());

        // Attack cooldown
        yield return new WaitForSeconds(3);

        Debug.Log($"Finished using {BossPower?.name ?? "FIREBALL"}");
    }

    private IEnumerator CreateProjectile()
    {
        // Return if the spawn point is null
        if (firePoint == null)
            yield break;

        // Instantiate the bullet
        _bulletObj = Instantiate(bulletPrefab, firePoint);

        var target = BossEnemyAttack.Enemy.DetectionBehavior.Target.GameObject.transform;

        // Return the bullet object
        yield return StartCoroutine(_bulletObj.CreateProjectile(BossEnemyAttack, attackStartupTime, target));
    }

    private IEnumerator ShootProjectile()
    {
        // Set the parent of the bullet to null
        transform.SetParent(null);
        
        // Calculate the direction of the bullet
        var direction = BossEnemyAttack.Enemy.DetectionBehavior.LastKnownTargetPosition - firePoint.position;

        _bulletObj.FireProjectile(direction, projectileVelocity, projectileLifetime);

        yield return null;
    }

    private void OnDrawGizmos()
    {
        // Return if the fire point is null or if the boss enemy attack is null
        if (firePoint == null || BossEnemyAttack == null)
            return;
        
        // Draw the direction of the bullet
        Gizmos.DrawLine(firePoint.position, BossEnemyAttack.Enemy.DetectionBehavior.LastKnownTargetPosition);
    }
}