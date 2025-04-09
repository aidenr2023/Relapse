using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HordeModeSpawner : EnemySpawner
{
    #region Serialized Fields

    [SerializeField] private IntReference currentRoundNumber;
    [SerializeField] private AnimationCurve roundEnemyCountsCurve;
    [SerializeField] private AnimationCurve roundEnemyModifierCurve;
    [SerializeField, Min(0)] private float waveDelayInterval = 1;

    [SerializeField] private WeightedEnemyInformation[] enemyPrefabs;
    [SerializeField] private Transform[] spawnPoints;

    #endregion

    #region Private Fields

    private int _totalEnemiesLeft;
    private int _remainingWaveEnemies;
    private bool _isStillSpawning;

    #endregion

    private Action onRoundComplete;

    #region EnemySpawner Implementation

    protected override string GetTooltipText()
    {
        return $"Current Wave: {currentRoundNumber.Value}\n" +
               $"Remaining Enemies: {_remainingWaveEnemies}";
    }

    protected override void CustomStart()
    {
    }

    protected override void CustomDestroy()
    {
    }

    protected override void CustomSpawnEnemy(Enemy actualEnemy, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        actualEnemy.EnemyInfo.OnDeath += DecrementEnemiesRemaining;
        actualEnemy.EnemyInfo.OnDeath += CheckForSpawnerComplete;
        actualEnemy.EnemyInfo.OnDeath += IncrementWave;
    }

    private void DecrementEnemiesRemaining(object sender, HealthChangedEventArgs e)
    {
        _remainingWaveEnemies--;

        _totalEnemiesLeft--;
    }

    private void IncrementWave(object sender, HealthChangedEventArgs e)
    {
        if (_remainingWaveEnemies > 0)
            return;

        currentRoundNumber.Value++;

        // Spawn the next wave
        var waveCount = Mathf.Max((int)roundEnemyModifierCurve.keys[^1].time, 1);

        // Keep spawning if the current round number is greater than 0
        // and the current round number is not a multiple of the wave count
        if ((currentRoundNumber.Value - 1) % waveCount > 0)
            StartCoroutine(SpawnNextWave(currentRoundNumber.Value, waveDelayInterval));
    }

    private void CheckForSpawnerComplete(object sender, HealthChangedEventArgs e)
    {
        var waveCount = Mathf.Max((int)roundEnemyModifierCurve.keys[^1].time, 1);

        // Return if this is not the end of the round
        if (currentRoundNumber.Value <= 0 || currentRoundNumber.Value % waveCount != 0 || _isStillSpawning)
            return;

        // If the remaining wave enemies is greater than 0, return
        if (_remainingWaveEnemies > 0)
            return;

        // Invoke the spawner complete event
        onSpawnerComplete.Invoke();
    }

    private IEnumerator SpawnNextWave(int currentSpawnIndex, float spawnDelay)
    {
        yield return new WaitForSeconds(spawnDelay);

        SpawnWave(CreateWave(currentSpawnIndex));
    }

    protected override void CustomStartSpawning()
    {
        SpawnWave(CreateWave(currentRoundNumber.Value));
    }

    protected override void CustomStopSpawning()
    {
    }

    #endregion

    private int DetermineRoundEnemyCount(int waveNumber)
    {
        var roundCycleCount = Mathf.Max((int)roundEnemyModifierCurve.keys[^1].time, 1);

        var standardAmount = Mathf.Max(roundEnemyCountsCurve.Evaluate(waveNumber), 1);

        var valueToUse = waveNumber % roundCycleCount == 0 && waveNumber > 0
            ? roundCycleCount
            : waveNumber % roundCycleCount;

        var modifier = Mathf.Max(roundEnemyModifierCurve.Evaluate(valueToUse), 0);

        return (int)(standardAmount + modifier);
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
            // Choose a random spawn point from the list of valid spawn points
            var spawnPointIndex = UnityEngine.Random.Range(0, validSpawnPoints.Length);

            // Create a new wave enemy info
            waveSpawnInfo.waveEnemyInfos[i] = new WaveEnemyInfo
            {
                enemyPrefab = GetRandomEnemyPrefab(enemyPrefabs),
                spawnPoint = validSpawnPoints[spawnPointIndex]
            };
        }

        return waveSpawnInfo;
    }

    private static Enemy GetRandomEnemyPrefab(WeightedEnemyInformation[] enemyPrefabs)
    {
        // Get the total weight of all enemies
        var totalWeight = enemyPrefabs.Sum(weightedEnemyInformation => weightedEnemyInformation.weight);

        // Get a random number between 0 and the total weight
        var randomWeight = UnityEngine.Random.Range(0, totalWeight);

        // Get the enemy with the random weight
        while (randomWeight > 0)
        {
            foreach (var weightedEnemyInformation in enemyPrefabs)
            {
                randomWeight -= weightedEnemyInformation.weight;

                if (randomWeight <= 0)
                    return weightedEnemyInformation.enemy;
            }
        }

        // If something goes wrong, return the first enemy
        return enemyPrefabs[0].enemy;
    }

    private void SpawnWave(WaveSpawnInfo waveSpawnInfo)
    {
        // Get the current wave spawn info
        StartCoroutine(StaggerSpawn(waveSpawnInfo));
    }

    private IEnumerator StaggerSpawn(WaveSpawnInfo currentSpawnInfo)
    {
        // Set the is still spawning flag to true
        _isStillSpawning = true;

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
            yield return new WaitForSeconds(0.125f);

            // Spawn the enemy
            var enemy = SpawnEnemy(
                enemySpawnInfo.enemyPrefab,
                enemySpawnInfo.spawnPoint.position,
                enemySpawnInfo.spawnPoint.rotation
            );

            // Increment the remaining wave enemies
            _remainingWaveEnemies++;
        }

        // Set the is still spawning flag to false
        _isStillSpawning = false;
    }

    public void ResetSpawner()
    {
        hasStartedSpawning = false;
        isComplete = false;
    }

    private void OnDrawGizmos()
    {
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