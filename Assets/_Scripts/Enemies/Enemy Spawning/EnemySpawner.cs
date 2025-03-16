using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

public abstract class EnemySpawner : MonoBehaviour, IDebugged
{
    #region Serialized Fields

    [SerializeField] protected bool isActiveOnStart = false;

    [SerializeField] protected bool showPersistentTooltip = true;
    
    [SerializeField] protected GameObject itemToDropWhenComplete;

    [SerializeField] protected UnityEvent onEnemyKilled;
    [SerializeField] protected UnityEvent onSpawnerStart;
    [SerializeField] protected UnityEvent onSpawnerComplete;

    #endregion

    #region Protected Fields

    protected readonly HashSet<EnemyInfo> spawnedEnemies = new();

    protected bool hasStartedSpawning;
    
    protected bool isComplete;
    
    #endregion

    protected void Start()
    {
        // Add this to the debug manager
        DebugManager.Instance.AddDebuggedObject(this);

        onSpawnerComplete.AddListener(ChangeFlagOnSpawnerComplete);
        onSpawnerStart.AddListener(ShowTooltipOnStart);
        
        CustomStart();
    }

    private void ChangeFlagOnSpawnerComplete()
    {
        isComplete = true;
    }

    private void ShowTooltipOnStart()
    {
        JournalTooltipManager.Instance.AddTooltip(
            GetTooltipText, 3, true, TooltipEndCondition
        );
    }
    
    protected abstract string GetTooltipText();

    protected virtual bool TooltipEndCondition()
    {
        return isComplete;
    }

    protected abstract void CustomStart();

    private void OnDestroy()
    {
        // Set the is complete flag to true
        isComplete = true;
        
        // Remove this from the debug manager
        DebugManager.Instance.RemoveDebuggedObject(this);

        // Call the custom destroy method
        CustomDestroy();
    }

    protected abstract void CustomDestroy();

    protected Enemy SpawnEnemy(Enemy enemyPrefab, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        // Instantiate the enemy prefab at the spawn position and rotation
        var enemy = Instantiate(enemyPrefab, transform);

        // Set the transform's parent to null
        enemy.transform.SetParent(null);

        // Set the position and rotation of the enemy to the spawn position and rotation
        enemy.transform.position = spawnPosition;
        enemy.transform.rotation = spawnRotation;

        // Set the instantiated at runtime flag to true to avoid data for
        // this enemy being saved / loaded
        enemy.UniqueId.InstantiatedAtRuntime = true;

        // Add the enemy to the spawned enemies hash set
        spawnedEnemies.Add(enemy.EnemyInfo);

        // Attach the OnDeath event to the InvokeOnEnemyKilled method
        enemy.EnemyInfo.OnDeath += InvokeOnEnemyKilled;

        // Call the custom spawn enemy method
        CustomSpawnEnemy(enemy, spawnPosition, spawnRotation);

        return enemy;
    }

    protected abstract void CustomSpawnEnemy(Enemy actualEnemy, Vector3 spawnPosition, Quaternion spawnRotation);

    private void InvokeOnEnemyKilled(object sender, HealthChangedEventArgs e)
    {
        onEnemyKilled.Invoke();

        // Also, remove the enemy from the spawned enemies hash set
        spawnedEnemies.Remove((EnemyInfo)e.Actor);
    }

    public void StartSpawning()
    {
        // Return if the flag is already set to true
        if (hasStartedSpawning)
            return;

        // Set the flag to true
        hasStartedSpawning = true;

        // Invoke the on spawner start event
        onSpawnerStart.Invoke();

        // Call the custom start spawning method
        CustomStartSpawning();
    }

    protected abstract void CustomStartSpawning();

    public void StopSpawning()
    {
        // Return if the flag is already set to false
        if (!hasStartedSpawning)
            return;

        // Set the flag to false
        hasStartedSpawning = false;

        // Call the custom stop spawning method
        CustomStopSpawning();
    }

    protected abstract void CustomStopSpawning();

    protected void SpawnItem(HealthChangedEventArgs args)
    {
        // Drop the item to spawn
        if (itemToDropWhenComplete == null)
            return;

        var spawnedItem = Instantiate(
            itemToDropWhenComplete,
            args.Actor.GameObject.transform.position,
            Quaternion.identity
        );

        // Set the instantiated at runtime flag to true (if it has one)
        if (spawnedItem.TryGetComponent(out UniqueId uniqueId))
            uniqueId.InstantiatedAtRuntime = true;
    }

    public void KillAllCurrentEnemies()
    {
        // Copy the spawned enemies hash set to an array
        var enemies = new EnemyInfo[spawnedEnemies.Count];
        spawnedEnemies.CopyTo(enemies);

        // Loop through the enemies array and kill each enemy
        foreach (var enemy in enemies)
            enemy.ChangeHealth(-enemy.MaxHealth, enemy, enemy.ParentComponent.AttackBehavior, enemy.transform.position);
    }

    public virtual string GetDebugText()
    {
        var sb = new StringBuilder();

        sb.Append($"Spawner ({name})\n");
        sb.Append($"\tStarted Spawning: {hasStartedSpawning}\n");
        sb.Append($"\tSpawned Enemies: {spawnedEnemies.Count}\n");

        return sb.ToString();
    }
}