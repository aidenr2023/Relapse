﻿using System;
using System.Collections;
using UnityEngine;

public class EnforcerEnemy : ComponentScript<EnemyInfo>
{
    #region Serialized Fields

    [SerializeField] private ShootingEnemyAttack shootingEnemyAttack;

    [SerializeField, Min(0)] private int attackCountBeforeRelocate = 3;
    [SerializeField, Min(0)] private float minRelocateTime = 2f;
    [SerializeField, Min(0)] private float maxRelocateTime = 5f;
    [SerializeField, Min(0)] private float cooldownAfterAttackTime = 1f;
    [SerializeField, Min(0)] private float attackTimeout = 3f;

    #endregion

    #region Private Fields

    private EnforcerBehaviorMode _currentBehaviorMode;
    private int _currentAttackCount;

    private Coroutine _updateCoroutine;

    private float _currentAttackTimeoutTime;

    #endregion

    protected override void CustomAwake()
    {
        base.CustomAwake();

        // Subscribe to the attack event of the shooting script
        shootingEnemyAttack.onAttack += IncrementAttackCountOnAttack;
        shootingEnemyAttack.onAttack += ResetAttackTimeout;
    }

    private void ResetAttackTimeout(ShootingEnemyAttack obj)
    {
        _currentAttackTimeoutTime = 0;
    }

    private void IncrementAttackCountOnAttack(ShootingEnemyAttack obj)
    {
        _currentAttackCount++;
    }

    private void Start()
    {
        ChangeBehaviorMode(EnforcerBehaviorMode.Relocate);
    }

    private void OnEnable()
    {
        // If there is an update coroutine, stop it
        if (_updateCoroutine != null)
            StopCoroutine(_updateCoroutine);

        _updateCoroutine = StartCoroutine(UpdateCoroutine());
    }

    private void OnDisable()
    {
        // If there is an update coroutine, stop it
        if (_updateCoroutine != null)
        {
            StopCoroutine(_updateCoroutine);
            _updateCoroutine = null;
        }
    }

    private IEnumerator UpdateCoroutine()
    {
        yield return null;
        yield return null;

        while (gameObject != null)
        {
            var targetDetection = ParentComponent.ParentComponent.DetectionBehavior;
            var newMovement = ParentComponent.ParentComponent.NewMovement;
            var movementBehavior = ParentComponent.ParentComponent.MovementBehavior;

            switch (_currentBehaviorMode)
            {
                case EnforcerBehaviorMode.Relocate:

                    // TODO: Wait until fully relocated
                    var relocateTime = UnityEngine.Random.Range(minRelocateTime, maxRelocateTime);
                    var relocateEndTime = Time.time + relocateTime;

                    yield return new WaitForSeconds(relocateTime);

                    yield return new WaitUntil(() => targetDetection.IsTargetDetected);

                    // Change the behavior mode to attack
                    ChangeBehaviorMode(EnforcerBehaviorMode.Attack);

                    break;

                case EnforcerBehaviorMode.Attack:

                    // Stay in attack mode until the attack times out OR
                    // The attack count reaches the attack count before relocate
                    while (_currentAttackTimeoutTime < attackTimeout && _currentAttackCount < attackCountBeforeRelocate)
                    {
                        // Wait until the attack timeout is over
                        _currentAttackTimeoutTime += Time.deltaTime;
                        yield return null;
                    }
                    
                    // Disable the shooting attack
                    shootingEnemyAttack.SetAttackEnabled(false);
                    
                    // Wait for the cooldown after attack
                    yield return new WaitForSeconds(cooldownAfterAttackTime);

                    // Change the behavior mode to relocate
                    ChangeBehaviorMode(EnforcerBehaviorMode.Relocate);
                    
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Wait for the next frame
            yield return null;
        }
    }

    private void ChangeBehaviorMode(EnforcerBehaviorMode behaviorMode)
    {
        _currentBehaviorMode = behaviorMode;

        switch (behaviorMode)
        {
            case EnforcerBehaviorMode.Relocate:
                // Deactivate the shooting enemy attack
                shootingEnemyAttack.SetAttackEnabled(false);
                break;

            case EnforcerBehaviorMode.Attack:
                // Activate the shooting enemy attack
                shootingEnemyAttack.SetAttackEnabled(true);

                // Reset the attack count
                _currentAttackCount = 0;
                
                // Reset the attack timeout
                _currentAttackTimeoutTime = 0;

                // Force the movement update
                ParentComponent.ParentComponent.Brain.ForceMovementUpdate = true;

                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(behaviorMode), behaviorMode, null);
        }

        // Force the movement update
        ParentComponent.ParentComponent.Brain.ForceMovementUpdate = true;

        // In the brain, change the behavior mode
        ParentComponent.ParentComponent.Brain.BehaviorMode = (int)behaviorMode;

        // Debug.Log($"Enforcer Behavior Mode: {behaviorMode} {ParentComponent.ParentComponent.Brain.BehaviorMode}", this);
    }

    #region Extra Types

    private enum EnforcerBehaviorMode
    {
        Relocate,
        Attack
    }

    #endregion
}