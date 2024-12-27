using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Serialization;

public class ShootingEnemyAttack : MonoBehaviour, IEnemyAttackBehavior
{
    private static readonly int AnimatorAttackProperty = Animator.StringToHash("Attack");

    #region Serialized Fields

    [SerializeField] private Animator animator;

    [Header("Stats")]
    [Tooltip("How long it takes after the enemy detects the player for the enemy to shoot.")]
    [SerializeField, Min(0)]
    private float detectionFireDelay;

    [SerializeField, Min(0)] private float fireRange = 20f;
    [SerializeField, Min(0)] private float projectileSpeed = 8f;
    [SerializeField, Min(0)] private float attackCooldown = 3f;
    [SerializeField, Min(0)] private float projectileLifetime = 5f;

    [Space, SerializeField] private ShootingEnemyProjectile enemyBulletPrefab;
    [SerializeField] private Transform[] projectileSpawnPoints;

    #endregion

    #region Private Fields

    private CountdownTimer _attackCooldownTimer;

    private CountdownTimer _detectionFireDelayTimer;

    private bool _isExternallyEnabled = true;

    #endregion

    #region Getters

    public GameObject GameObject => gameObject;

    public Enemy Enemy { get; private set; }

    public bool IsAttackEnabled => _isExternallyEnabled;

    private bool IsTargetInRange =>
        Enemy.EnemyDetectionBehavior.Target != null &&
        Vector3.Distance(
            transform.position,
            Enemy.EnemyDetectionBehavior.Target.GameObject.transform.position
        ) <= fireRange;

    #endregion

    private void Awake()
    {
        // Get the enemy component
        Enemy = GetComponent<Enemy>();

        // Create the cooldown timer
        _attackCooldownTimer = new CountdownTimer(attackCooldown, true, true);

        // Create the detection fire delay timer
        _detectionFireDelayTimer = new CountdownTimer(detectionFireDelay, true);
    }

    // Update is called once per frame
    private void Update()
    {
        // Update the cooldown timer
        _attackCooldownTimer.SetMaxTime(attackCooldown);
        _attackCooldownTimer.Update(Time.deltaTime);

        // Update the detection fire delay timer
        _detectionFireDelayTimer.SetMaxTime(detectionFireDelay);

        // If the enemy is not aware of the player, recharge the fire delay timer
        _detectionFireDelayTimer.SetMaxTime(detectionFireDelay);
        var fireDelayDirection = (Enemy.EnemyDetectionBehavior.CurrentDetectionState == EnemyDetectionState.Aware)
            ? 1
            : -1;
        _detectionFireDelayTimer.Update(Time.deltaTime * fireDelayDirection);

        // Attack the player
        Attack();
    }

    private void CreateProjectile(int index)
    {
        // Return if the projectile spawn points are empty or null
        if (projectileSpawnPoints == null || projectileSpawnPoints.Length == 0)
            return;

        var spawnPoint = projectileSpawnPoints[index % projectileSpawnPoints.Length];

        // Return if the spawn point is null
        if (spawnPoint == null)
            return;

        // Instantiate the bullet
        var bulletObj = Instantiate(enemyBulletPrefab, spawnPoint.position, spawnPoint.rotation);

        // Calculate the direction of the bullet
        var direction = Enemy.EnemyDetectionBehavior.Target.GameObject.transform.position - spawnPoint.position;

        // Call the shoot method on the bullet
        bulletObj.Shoot(this, direction, projectileSpeed, projectileLifetime);
    }

    private void Attack()
    {
        // Return if the attack is not enabled
        // Check if the player is in range
        // Return if the fire delay timer is not complete
        // Return if the cooldown timer is not complete
        // Return if the enemy's line of sight with the target is broken
        if (!IsAttackEnabled || !IsTargetInRange || _detectionFireDelayTimer.IsNotComplete ||
            _attackCooldownTimer.IsNotComplete || !Enemy.EnemyDetectionBehavior.IsTargetDetected)
            return;

        // Reset the attack cooldown timer
        _attackCooldownTimer.SetMaxTimeAndReset(attackCooldown);
        _attackCooldownTimer.Start();

        if (animator == null)
            return;

        // Play the melee attack animation
        animator.SetTrigger(AnimatorAttackProperty);
    }

    public void SetAttackEnabled(bool on)
    {
        _isExternallyEnabled = on;
    }

    public void FireProjectile(int index)
    {
        // Shoot at the player
        CreateProjectile(index);
    }
}