using System;
using UnityEngine;

[RequireComponent(typeof(EnemyInfo))]
public class Enemy : MonoBehaviour
{
    #region Fields

    private EnemyInfo _enemyInfo;
    private IEnemyAttackBehavior _enemyAttackBehavior;
    private IEnemyMovementBehavior _enemyMovementBehavior;

    #endregion

    #region Getters

    public EnemyInfo EnemyInfo => _enemyInfo;

    public IEnemyAttackBehavior EnemyAttackBehavior => _enemyAttackBehavior;

    public IEnemyMovementBehavior EnemyMovementBehavior => _enemyMovementBehavior;

    #endregion


    private void Awake()
    {
        // Get the components
        GetComponents();
    }

    private void Start()
    {
    }

    private void GetComponents()
    {
        // Get the EnemyInfo component
        _enemyInfo = GetComponent<EnemyInfo>();

        // Get the IEnemyAttackBehavior component
        _enemyAttackBehavior = GetComponent<IEnemyAttackBehavior>();

        // Get the IEnemyMovementBehavior component
        _enemyMovementBehavior = GetComponent<IEnemyMovementBehavior>();

        // Assert that the IEnemyAttackBehavior component is not null
        Debug.Assert(_enemyAttackBehavior != null, "The IEnemyAttackBehavior component is null.");

        // Assert that the IEnemyMovementBehavior component is not null
        Debug.Assert(_enemyMovementBehavior != null, "The IEnemyMovementBehavior component is null.");
    }
}