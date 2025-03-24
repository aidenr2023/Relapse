using System;
using System.Collections;
using UnityEngine;

public class BossFireballBehavior : BossPowerBehavior
{
    [Header("Power"), SerializeField] private Transform firePoint;
    [SerializeField] private BossFireballProjectile bulletPrefab;
    [SerializeField, Min(0)] private float attackStartupTime = 3f;
    [SerializeField, Min(0)] private float repeatCount = 3;
    [SerializeField, Min(0)] private float shootDistance = 25;
    
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
        for (var i = 0; i < repeatCount; i++)
        {
            Debug.Log($"Creating {BossPower?.name}");

            // Set the movement mode to strafe left, right, back
            BossEnemyAttack.ParentComponent.SetBossBehaviorMode(BossBehaviorMode.StrafeLeftRightBack);

            // Create the projectile
            yield return StartCoroutine(CreateProjectile());

            var startTime = Time.time;

            // Play the power ready particles
            PlayPowerReadyParticles();
            
            // Play the power ready sound
            SoundManager.Instance.PlaySfxAtPoint(powerReadySound, transform.position);
            
            // Wait a second before shooting the projectile
            while (Time.time - startTime < 1)
            {
                var forward = BossEnemyAttack.Enemy.DetectionBehavior.LastKnownTargetPosition - firePoint.position;

                // Set the forward of the projectile to the direction of the projectile
                _bulletObj.transform.forward = forward;
                yield return null;
            }
            
            // Set the movement mode to hard chase
            BossEnemyAttack.ParentComponent.SetBossBehaviorMode(BossBehaviorMode.HardChase);
            
            // var targetTransform = BossEnemyAttack.Enemy.DetectionBehavior.Target.GameObject.transform;
            //
            // // Yield while the target is not in range
            // yield return new WaitUntil(() => Vector3.Distance(transform.position, targetTransform.position) < shootDistance);
            
            Debug.Log($"Shooting {BossPower?.name}");

            // Set the movement mode to idle
            BossEnemyAttack.ParentComponent.SetBossBehaviorMode(BossBehaviorMode.Idle);
            
            // Play the power release sound
            SoundManager.Instance.PlaySfxAtPoint(powerReleaseSound, transform.position);

            // Then, shoot the projectile
            yield return StartCoroutine(ShootProjectile());
            
            // If this isn't the last fireball, wait for a second
            if (i < repeatCount - 1)
                yield return new WaitForSeconds(1);
        }

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