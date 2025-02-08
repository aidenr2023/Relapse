using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

public abstract class EnemySpawner : MonoBehaviour, IDebugged
{
    #region Serialized Fields

    [SerializeField] protected bool isActiveOnStart = false;

    [SerializeField] protected UnityEvent onEnemyKilled;
    [SerializeField] protected UnityEvent onSpawnerComplete;

    #endregion

    #region Protected Fields

    protected readonly HashSet<EnemyInfo> spawnedEnemies = new();

    protected bool hasStartedSpawning;

    #endregion

    protected void Start()
    {
        // Add this to the debug manager
        DebugManager.Instance.AddDebuggedObject(this);

        CustomStart();
    }

    protected abstract void CustomStart();

    protected void SpawnEnemy(Enemy enemyPrefab, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        // Instantiate the enemy prefab at the spawn position and rotation
        var enemy = Instantiate(enemyPrefab, spawnPosition, spawnRotation);

        // Set the instantiated at runtime flag to true to avoid data for
        // this enemy being saved / loaded
        enemy.UniqueId.InstantiatedAtRuntime = true;
        
        // Add the enemy to the spawned enemies hash set
        spawnedEnemies.Add(enemy.EnemyInfo);

        // Attach the OnDeath event to the InvokeOnEnemyKilled method
        enemy.EnemyInfo.OnDeath += InvokeOnEnemyKilled;
        
        // Call the custom spawn enemy method
        CustomSpawnEnemy(enemy, spawnPosition, spawnRotation);
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