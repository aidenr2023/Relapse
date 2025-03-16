using System;
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
        {
            Debug.LogError($"Wave index out of bounds: {waveIndex}");
            return;
        }

        // Get the current wave spawn info
        var currentSpawnInfo = waves[waveIndex];

        // Loop through each enemy in the wave
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
        }
    }

    #region Subclasses

    [Serializable]
    private struct WaveEnemyInfo
    {
        [SerializeField] public Enemy enemyPrefab;
        [SerializeField] public Transform spawnPoint;
    }

    [Serializable]
    private struct WaveSpawnInfo
    {
        [SerializeField] public WaveEnemyInfo[] waveEnemyInfos;
    }

    #endregion
}