using System;
using UnityEngine;

public class ContinuousEnemySpawner : EnemySpawner
{
    #region Serialized Fields

    [SerializeField, Min(1)] private float maxActiveEnemies = 10;
    [SerializeField, Min(0)] private float spawnInterval = 1f;

    [SerializeField] private bool isInfinite = true;
    [SerializeField, Min(1)] private int spawnerCompleteAmount = 30;

    [SerializeField] private Enemy[] enemyPrefabs;
    [SerializeField] private Transform[] spawnPoints;

    #endregion

    #region Private Fields

    private int _totalSpawnCount;
    private int _enemyKilledCount;

    private CountdownTimer _spawnTimer;

    #endregion

    #region Getters

    private bool IsComplete => _totalSpawnCount >= spawnerCompleteAmount && !isInfinite;

    #endregion


    protected override void CustomStart()
    {
        // Initialize the spawn timer
        _spawnTimer = new CountdownTimer(spawnInterval);
        _spawnTimer.OnTimerEnd += SpawnOnTimerEnd;
        _spawnTimer.OnTimerEnd += _spawnTimer.Reset;

        // If this spawner is active on start, start spawning
        if (isActiveOnStart)
            StartSpawning();
    }

    protected override void CustomDestroy()
    {
        
    }

    private void Update()
    {
        _spawnTimer.SetMaxTime(spawnInterval);
        _spawnTimer.Update(Time.deltaTime);
    }

    private void SpawnOnTimerEnd()
    {
        // Return if the spawner is complete
        if (IsComplete)
            return;

        // If the number of enemies is GREATER than the max enemies, return
        if (spawnedEnemies.Count >= maxActiveEnemies)
            return;

        // Return if there are no spawn points or enemy prefabs
        if (spawnPoints.Length == 0 || enemyPrefabs.Length == 0)
            return;

        // Spawn an enemy at a random spawn point
        var randomSpawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
        var randomEnemyPrefab = enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Length)];
        SpawnEnemy(randomEnemyPrefab, randomSpawnPoint.position, randomSpawnPoint.rotation);
    }

    protected override void CustomSpawnEnemy(Enemy actualEnemy, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        // Increment the total spawn count
        _totalSpawnCount++;

        // Attach the OnDeath event to the CheckForSpawnerCompleteOnEnemyDeath method
        actualEnemy.EnemyInfo.OnDeath += IncrementEnemyKilledCount;
        actualEnemy.EnemyInfo.OnDeath += CheckForSpawnerCompleteOnEnemyDeath;
    }

    private void IncrementEnemyKilledCount(object sender, HealthChangedEventArgs e)
    {
        // Increment the enemy killed count
        _enemyKilledCount++;
    }

    private void CheckForSpawnerCompleteOnEnemyDeath(object sender, HealthChangedEventArgs healthChangedEventArgs)
    {
        // Return if the spawner is infinite
        if (isInfinite)
            return;

        // Return if the enemy killed count is not equal to the spawner complete amount
        if (_enemyKilledCount != spawnerCompleteAmount)
            return;

        // Invoke the spawner complete event
        onSpawnerComplete?.Invoke();
    }

    protected override void CustomStartSpawning()
    {
        // Start the timer
        _spawnTimer.Start();

        // Force the timer to be complete
        _spawnTimer.ForcePercent(1);
    }

    protected override void CustomStopSpawning()
    {
        // Stop the timer
        _spawnTimer.Stop();
    }

    private void OnDrawGizmos()
    {
        const float sphereSize = 0.25f;

        // Return if the spawn points is null
        if (spawnPoints == null)
            return;
        
        foreach (var spawnPoint in spawnPoints)
        {
            // continue if the spawn point is null
            if (spawnPoint == null)
                continue;
            
            // Draw a green sphere for each spawn point
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(spawnPoint.position, sphereSize);

            // Draw arrows for each spawn point
            Gizmos.color = Color.red;
            CustomFunctions.DrawArrow(spawnPoint.position, spawnPoint.forward);
        }
    }
}