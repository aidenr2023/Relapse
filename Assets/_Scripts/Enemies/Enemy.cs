using System;
using UnityEngine;

[RequireComponent(typeof(EnemyInfo))]
public class Enemy : MonoBehaviour
{
    #region Fields

    private EnemyInfo _enemyInfo;

    private IEnemyDetectionBehavior _enemyDetectionBehavior;
    private IEnemyMovementBehavior _enemyMovementBehavior;
    private IEnemyAttackBehavior _enemyAttackBehavior;
    private IEnemyAbilityBehavior _enemyAbilityBehavior;

    #endregion

    #region Getters

    public EnemyInfo EnemyInfo => _enemyInfo;

    public IEnemyDetectionBehavior EnemyDetectionBehavior => _enemyDetectionBehavior;

    public IEnemyAttackBehavior EnemyAttackBehavior => _enemyAttackBehavior;

    public IEnemyMovementBehavior EnemyMovementBehavior => _enemyMovementBehavior;

    #endregion


    private void Awake()
    {
        // Get the components
        InitializeComponents();
    }

    private void Start()
    {
    }

    private void InitializeComponents()
    {
        // Get the EnemyInfo component
        _enemyInfo = GetComponent<EnemyInfo>();

        // Get the IEnemyDetectionBehavior component
        _enemyDetectionBehavior = GetComponent<IEnemyDetectionBehavior>();

        // Get the IEnemyMovementBehavior component
        _enemyMovementBehavior = GetComponent<IEnemyMovementBehavior>();

        // Get the IEnemyAttackBehavior component
        _enemyAttackBehavior = GetComponent<IEnemyAttackBehavior>();

        // Get the enemy ability behavior component
        _enemyAbilityBehavior = GetComponent<IEnemyAbilityBehavior>();

        // Assert that the IEnemyDetectionBehavior component is not null
        Debug.Assert(_enemyDetectionBehavior != null, "The IEnemyDetectionBehavior component is null.");

        // Assert that the IEnemyMovementBehavior component is not null
        Debug.Assert(_enemyMovementBehavior != null, "The IEnemyMovementBehavior component is null.");

        // Assert that the IEnemyAttackBehavior component is not null
        Debug.Assert(_enemyAttackBehavior != null, "The IEnemyAttackBehavior component is null.");
    }
}