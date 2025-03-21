using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ExplosionHelper), typeof(DrunkardEnemy))]
public class DrunkardEnemyAttack : MonoBehaviour, IEnemyAttackBehavior
{
    #region Serialized Fields

    [SerializeField, Min(0)] private float chargeMovementSpeed = 12;

    [SerializeField, Min(0.0001f)] private float updatesPerSecond = 4f;

    [SerializeField, Min(0)] private float explosionRange = 5;
    [SerializeField, Min(0)] private float chargeUpTime = 3;
    [SerializeField, Min(0)] private float explosionTime = 3;

    [SerializeField, Min(0)] private float tooFarTimeMultiplier = 1f;

    [SerializeField] private Sound normalHitSfx;

    #endregion

    #region Private Fields

    private ExplosionHelper _explosionHelper;

    private bool _isExternallyEnabled = true;

    private Coroutine _updateCoroutine;

    private bool _isExploding;

    private float _currentChargeUpTime;

    private DrunkardEnemy _drunkardEnemy;

    #endregion

    #region Getters

    public GameObject GameObject => gameObject;
    public Enemy Enemy { get; private set; }
    public Sound NormalHitSfx => normalHitSfx;
    public Sound CriticalHitSfx => null;
    public HashSet<object> AttackDisableTokens { get; } = new();

    public bool IsAttackEnabled => _isExternallyEnabled && this.IsAttackEnabledTokens();

    #endregion

    private void Awake()
    {
        Enemy = GetComponent<Enemy>();
        _explosionHelper = GetComponent<ExplosionHelper>();
        _drunkardEnemy = GetComponent<DrunkardEnemy>();
    }

    private void OnEnable()
    {
        // Stop the existing update coroutine (if any)
        if (_updateCoroutine != null)
            StopCoroutine(_updateCoroutine);

        _updateCoroutine = StartCoroutine(UpdateCoroutine());
    }

    private void OnDisable()
    {
        if (_updateCoroutine != null)
        {
            StopCoroutine(_updateCoroutine);
            _updateCoroutine = null;
        }
    }

    private IEnumerator UpdateCoroutine()
    {
        // Wait for a frame
        yield return null;

        while (!_isExploding)
        {
            var frameTime = 1 / updatesPerSecond;

            // Continue if there is no target
            if (!Enemy.DetectionBehavior.IsTargetDetected)
            {
                yield return new WaitForSeconds(frameTime);
                continue;
            }

            // Continue if the attack is not enabled
            if (!IsAttackEnabled)
            {
                yield return new WaitForSeconds(frameTime);
                continue;
            }

            // Get the target
            var target = Enemy.DetectionBehavior.Target;

            // Get the distance from the target
            var distance = Vector3.Distance(target.GameObject.transform.position, transform.position);

            // Continue if the target is too far away
            if (distance > explosionRange)
            {
                // Decrement the charge time
                ChangeChargeUpTime(-frameTime * tooFarTimeMultiplier);

                yield return new WaitForSeconds(frameTime);
                continue;
            }

            // Increment the charge time
            ChangeChargeUpTime(frameTime);

            // If the charge up time is less than the charge up time, continue
            if (_currentChargeUpTime < chargeUpTime)
            {
                yield return new WaitForSeconds(frameTime);
                continue;
            }

            // Turn on the fuse
            TurnOnFuse();

            // Wait for the next update
            yield return new WaitForSeconds(frameTime);
        }
    }

    private void ChangeChargeUpTime(float time)
    {
        _currentChargeUpTime = Mathf.Clamp(_currentChargeUpTime + time, 0, chargeUpTime);
    }

    private IEnumerator ExplodeCoroutine()
    {
        // Start particles
        _drunkardEnemy.StartParticles();

        // Set the is exploding flag to true
        _isExploding = true;

        yield return new WaitForSeconds(explosionTime);

        Explode();
    }

    private void Explode()
    {
        // Stop any active coroutines
        if (_updateCoroutine != null)
        {
            StopCoroutine(_updateCoroutine);
            _updateCoroutine = null;
        }

        // Remove all health from the enemy
        Enemy.EnemyInfo.ChangeHealth(-Enemy.EnemyInfo.CurrentHealth, Enemy.EnemyInfo, this, transform.position, false);

        // Add explosion logic here
        _explosionHelper.Explode(true);
    }

    public void SetAttackEnabled(bool on)
    {
        _isExternallyEnabled = on;
    }

    public void Activate()
    {
    }

    private void TurnOnFuse()
    {
        // Change the movement speed of the movement
        Enemy.NewMovement.MovementSpeed = chargeMovementSpeed;

        // Start the explode coroutine
        StartCoroutine(ExplodeCoroutine());
    }
}