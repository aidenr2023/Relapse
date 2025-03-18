using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyInfo), typeof(UniqueId), typeof(NewEnemyBehaviorBrain))]
[RequireComponent(typeof(NewEnemyMovement))]
public class Enemy : MonoBehaviour, ILevelLoaderInfo
{
    private static readonly HashSet<Enemy> _enemies = new();

    public static IReadOnlyCollection<Enemy> Enemies => _enemies;

    [SerializeField] private Transform centerTransform;
        
    #region Getters

    public EnemyInfo EnemyInfo { get; private set; }

    public NewEnemyBehaviorBrain Brain { get; private set; }
    
    public NewEnemyMovement NewMovement { get; private set; }
    
    public IEnemyDetectionBehavior DetectionBehavior { get; private set; }

    public IEnemyAttackBehavior AttackBehavior { get; set; }

    public INewEnemyMovementBehavior MovementBehavior => Brain.MovementBehavior;

    public IEnemyAbilityBehavior AbilityBehavior { get; private set; }

    public Transform CenterTransform => centerTransform != null ? centerTransform : transform;
        
    #endregion

    private void Awake()
    {
        // Get the components
        InitializeComponents();
    }

    private void OnEnable()
    {
        // Add the enemy to the enemies hash set
        _enemies.Add(this);
    }

    private void OnDisable()
    {
        // Remove the enemy from the enemies hash set
        _enemies.Remove(this);
    }

    private void InitializeComponents()
    {
        // Get the EnemyInfo component
        EnemyInfo = GetComponent<EnemyInfo>();

        // Get the brain component
        Brain = GetComponent<NewEnemyBehaviorBrain>();
        
        NewMovement = GetComponent<NewEnemyMovement>();
        
        // Get the IEnemyDetectionBehavior component
        DetectionBehavior = GetComponent<IEnemyDetectionBehavior>();

        // // Get the IEnemyMovementBehavior component
        // MovementBehavior = GetComponent<IEnemyMovementBehavior>();

        // Get the IEnemyAttackBehavior component
        AttackBehavior = GetComponent<IEnemyAttackBehavior>();

        // Get the enemy ability behavior component
        AbilityBehavior = GetComponent<IEnemyAbilityBehavior>();

        // // Assert that the IEnemyDetectionBehavior component is not null
        // Debug.Assert(DetectionBehavior != null, "The IEnemyDetectionBehavior component is null.");
        //
        // // Assert that the IEnemyMovementBehavior component is not null
        // Debug.Assert(MovementBehavior != null, "The IEnemyMovementBehavior component is null.");
        //
        // // Assert that the IEnemyAttackBehavior component is not null
        // Debug.Assert(AttackBehavior != null, "The IEnemyAttackBehavior component is null.");

        // Assert that the brain component is not null
        Debug.Assert(Brain != null, "The brain component is null.");
        
        // Assert that the new movement component is not null
        Debug.Assert(NewMovement != null, "The new movement component is null.");
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
    private const string CURRENT_HEALTH_KEY = "_currentHealth";
    private const string POSITION_KEY = "_position";
    private const string ROTATION_KEY = "_rotation";

    public void LoadData(LevelLoader levelLoader)
    {
        // Load whether the enemy is alive or not
        if (levelLoader.TryGetDataFromMemory(UniqueId, IS_ALIVE_KEY, out bool isAlive) && !isAlive)
        {
            // // Drain all the enemy's health
            // _enemyInfo.ChangeHealth(-_enemyInfo.MaxHealth, _enemyInfo, _enemyAttackBehavior, transform.position);

            Destroy(gameObject);
        }

        // Load the current health
        else if (levelLoader.TryGetDataFromMemory(UniqueId, CURRENT_HEALTH_KEY, out float currentHealth))
            EnemyInfo.ChangeHealth(currentHealth - EnemyInfo.CurrentHealth, EnemyInfo, AttackBehavior,
                transform.position);

        // Load the position and rotation
        if (levelLoader.TryGetDataFromMemory(UniqueId, POSITION_KEY, out Vector3 position))
            NewMovement.SetPosition(position);

        if (levelLoader.TryGetDataFromMemory(UniqueId, ROTATION_KEY, out Vector3 rotation))
            transform.rotation = Quaternion.Euler(rotation);
    }

    public void SaveData(LevelLoader levelLoader)
    {
        // Create boolean data for whether the enemy is alive or not
        // Save the data
        var isAliveData = new DataInfo(IS_ALIVE_KEY, EnemyInfo.CurrentHealth > 0);
        levelLoader.AddDataToMemory(UniqueId, isAliveData);

        // Create number data for the current health
        // Save the data
        var currentHealthData = new DataInfo(CURRENT_HEALTH_KEY, EnemyInfo.CurrentHealth);
        levelLoader.AddDataToMemory(UniqueId, currentHealthData);

        // Create vector3 data for the position
        // Save the data
        var positionData = new DataInfo(POSITION_KEY, transform.position);
        levelLoader.AddDataToMemory(UniqueId, positionData);

        // Create vector3 data for the rotation
        // Save the data
        var rotationData = new DataInfo(ROTATION_KEY, transform.rotation.eulerAngles);
        levelLoader.AddDataToMemory(UniqueId, rotationData);
    }

    #endregion
}