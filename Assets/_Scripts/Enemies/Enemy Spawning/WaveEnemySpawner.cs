﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveEnemySpawner : EnemySpawner
{
    #region Serialized Fields

    [SerializeField, Min(0)] private float waveDelayInterval;
    [SerializeField] private WaveSpawnInfo[] waves;

    #endregion

    private int _currentWaveIndex;
    private int _remainingWaveEnemies;

    private int _totalEnemiesLeft;

    #region EnemySpawner Implementation

    protected override void CustomStart()
    {
        // Count all the enemies in the waves
        foreach (var cWave in waves)
        {
            foreach (var _ in cWave.waveEnemyInfos)
                _totalEnemiesLeft++;
        }
    }

    protected override void CustomDestroy()
    {
    }

    protected override void CustomSpawnEnemy(Enemy actualEnemy, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        actualEnemy.EnemyInfo.OnDeath += DecrementEnemiesRemaining;
        actualEnemy.EnemyInfo.OnDeath += IncrementWave;
        actualEnemy.EnemyInfo.OnDeath += CheckForSpawnerComplete;
        actualEnemy.EnemyInfo.OnDeath += SpawnItemOnEnemyDeath;
    }

    private void CheckForSpawnerComplete(object sender, HealthChangedEventArgs e)
    {
        if (_currentWaveIndex < waves.Length)
            return;

        // If the remaining wave enemies is greater than 0, return
        if (_remainingWaveEnemies > 0)
            return;

        // Invoke the spawner complete event
        onSpawnerComplete.Invoke();
    }
    
    private void SpawnItemOnEnemyDeath(object sender, HealthChangedEventArgs e)
    {
        // Return if the enemy killed count is not equal to the spawner complete amount
        if (_totalEnemiesLeft > 0)
            return;

        SpawnItem(e);
    }

    protected override void CustomStartSpawning()
    {
        SpawnWave(_currentWaveIndex);
    }

    protected override void CustomStopSpawning()
    {
    }

    protected override string GetTooltipText()
    {
        return $"Remaining Enemies: {_remainingWaveEnemies}";
    }

    #endregion

    private void DecrementEnemiesRemaining(object sender, HealthChangedEventArgs e)
    {
        _remainingWaveEnemies--;

        _totalEnemiesLeft--;
    }

    private void IncrementWave(object sender, HealthChangedEventArgs e)
    {
        if (_remainingWaveEnemies > 0)
            return;

        _currentWaveIndex++;

        // Spawn the next wave
        StartCoroutine(SpawnNextWave(_currentWaveIndex, waveDelayInterval));
    }

    private IEnumerator SpawnNextWave(int currentSpawnIndex, float spawnDelay)
    {
        yield return new WaitForSeconds(spawnDelay);

        SpawnWave(currentSpawnIndex);
    }

    private void SpawnWave(int waveIndex)
    {
        // Return if the wave index is out of bounds
        if (waveIndex < 0 || waveIndex >= waves.Length)
            return;

        // Get the current wave spawn info
        var currentSpawnInfo = waves[waveIndex];

        StartCoroutine(StaggerSpawn(currentSpawnInfo));
    }

    private IEnumerator StaggerSpawn(WaveSpawnInfo currentSpawnInfo)
    {
        // Clone the spawn info to a new array
        var randomizedSpawns = new List<WaveEnemyInfo>(currentSpawnInfo.waveEnemyInfos);

        // Shuffle the spawn info array
        for (var i = 0; i < randomizedSpawns.Count * 2; i++)
        {
            var randomIndexA = UnityEngine.Random.Range(0, randomizedSpawns.Count);
            var randomIndexB = UnityEngine.Random.Range(0, randomizedSpawns.Count);

            // Swap the two random indexes
            (randomizedSpawns[randomIndexA], randomizedSpawns[randomIndexB]) =
                (randomizedSpawns[randomIndexB], randomizedSpawns[randomIndexA]);
        }

        foreach (var enemySpawnInfo in currentSpawnInfo.waveEnemyInfos)
        {
            // Spawn the enemy
            var enemy = SpawnEnemy(
                enemySpawnInfo.enemyPrefab,
                enemySpawnInfo.spawnPoint.position,
                enemySpawnInfo.spawnPoint.rotation
            );

            // Increment the remaining wave enemies
            _remainingWaveEnemies++;

            yield return new WaitForSeconds(0.125f);
        }
    }

    private void OnDrawGizmos()
    {
        // Create a hash set to store the spawn points
        var spawnPoints = new HashSet<Transform>();
        
        // Go through all waves and their spawn points
        foreach (var wave in waves)
        {
            foreach (var enemyInfo in wave.waveEnemyInfos)
            {
                if (enemyInfo.spawnPoint == null)
                    continue;
                
                spawnPoints.Add(enemyInfo.spawnPoint);
            }
        }
        
        const float arrowLength = 0.5f;
        // Draw the spawn points

        foreach (var point in spawnPoints)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(point.position, 0.1f);
            
            // Draw the arrow
            Gizmos.color = Color.red;
            CustomFunctions.DrawArrow(
                point.position,
                point.forward,
                arrowLength, 0.5f, 30
            );
        }
    }
}