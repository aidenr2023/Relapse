using System;
using UnityEngine;

[RequireComponent(typeof(EnemyInfo)), RequireComponent(typeof(UniqueId))]
public class Enemy : MonoBehaviour, ILevelLoaderInfo
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

    private void OnDestroy()
    {
        // Save the fact that the enemy is dead
        SaveData(LevelLoader.Instance);
    }

    #region ILevelLoaderInfo

    public GameObject GameObject => gameObject;

    private UniqueId _uniqueId;

    public UniqueId UniqueId
    {
        get
        {
            if (_uniqueId == null)
                _uniqueId = GetComponent<UniqueId>();

            return _uniqueId;
        }
    }

    private const string IS_ALIVE_KEY = "_isAlive";

    public void LoadData(LevelLoader levelLoader)
    {
        // Load whether the enemy is alive or not
        if (levelLoader.GetData($"{UniqueId.UniqueIdValue}{IS_ALIVE_KEY}", out bool isAlive) && !isAlive)
        {
            // Drain all the enemy's health
            _enemyInfo.ChangeHealth(-_enemyInfo.MaxHealth, _enemyInfo, _enemyAttackBehavior, transform.position);

            Destroy(gameObject);
        }
    }

    public void SaveData(LevelLoader levelLoader)
    {
        var isAlive = _enemyInfo.CurrentHealth > 0;

        // Create boolean data for whether the enemy is alive or not
        var isAliveData =
            SerializationDataInfo.SetOrCreateBooleanData($"{UniqueId.UniqueIdValue}{IS_ALIVE_KEY}", isAlive);

        // Save the data
        levelLoader.AddData(isAliveData);
    }

    #endregion
}