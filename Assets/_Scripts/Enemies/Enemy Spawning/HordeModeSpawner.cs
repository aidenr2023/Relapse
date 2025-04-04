using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HordeModeSpawner : EnemySpawner
{
    #region Serialized Fields

    [SerializeField] private IntReference currentRoundNumber;

    [SerializeField] private Enemy[] enemyPrefabs;

    [SerializeField] private AnimationCurve roundEnemyCountsCurve;

    [SerializeField] private Transform[] spawnPoints;

    #endregion

    #region Private Fields

    private int _totalEnemiesLeft;
    private int _remainingWaveEnemies;

    private int _currentWaveIndex;

    #endregion
    
    private Action onWaveComplete;

    #region EnemySpawner Implementation

    protected override string GetTooltipText()
    {
        return "TOOLTIP TEXT";
    }

    protected override void CustomStart()
    {
    }

    protected override void CustomDestroy()
    {
    }

    protected override void CustomSpawnEnemy(Enemy actualEnemy, Vector3 spawnPosition, Quaternion spawnRotation)
    {
    }

    protected override void CustomStartSpawning()
    {
        SpawnWave(CreateWave(_currentWaveIndex));
    }

    protected override void CustomStopSpawning()
    {
    }

    #endregion

    private int DetermineRoundEnemyCount(int waveNumber)
    {
        return (int)roundEnemyCountsCurve.Evaluate(waveNumber);
    }

    private WaveSpawnInfo CreateWave(int waveNumber)
    {
        // Get the array of valid spawn points
        var validSpawnPoints = new Transform[spawnPoints.Length];
        Array.Copy(spawnPoints, validSpawnPoints, spawnPoints.Length);

        var enemyCount = DetermineRoundEnemyCount(waveNumber);

        // Create a new wave spawn info
        var waveSpawnInfo = new WaveSpawnInfo
        {
            waveEnemyInfos = new WaveEnemyInfo[enemyCount]
        };

        // Populate the wave enemy info array
        for (var i = 0; i < waveSpawnInfo.waveEnemyInfos.Length; i++)
        {
            // Choose a random enemy prefab
            var enemyPrefab = enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Length)];

            // Choose a random spawn point from the list of valid spawn points
            var spawnPointIndex = UnityEngine.Random.Range(0, validSpawnPoints.Length);

            // Create a new wave enemy info
            waveSpawnInfo.waveEnemyInfos[i] = new WaveEnemyInfo
            {
                enemyPrefab = enemyPrefab,
                spawnPoint = validSpawnPoints[spawnPointIndex]
            };
        }

        return waveSpawnInfo;
    }

    private void SpawnWave(WaveSpawnInfo waveSpawnInfo)
    {
        // Get the current wave spawn info
        StartCoroutine(StaggerSpawn(waveSpawnInfo));
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
}