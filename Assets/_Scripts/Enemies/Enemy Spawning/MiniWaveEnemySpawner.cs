using System;
using UnityEngine;
using UnityEngine.Events;

public class MiniWaveEnemySpawner : EnemySpawner
{
    #region Serialized Fields

    [SerializeField, Min(0)] private float spawnInterval = 1f;

    [SerializeField] private bool isInfinite = true;
    [SerializeField, Min(1)] private int spawnerCompleteAmount = 3;

    [SerializeField] private WaveSpawnInfo[] waveSpawnInfos;

    [SerializeField] private UnityEvent onWaveComplete;

    #endregion

    #region Private Fields

    private int _wavesCompleted;
    private CountdownTimer _spawnTimer;
    private int _waveEnemiesRemaining;
    private bool _spawnedInitialWave;
    private int _totalEnemiesLeft;

    #endregion

    protected override void CustomStart()
    {
        // Count all the enemies in the waves
        foreach (var _ in waveSpawnInfos)
            _totalEnemiesLeft += spawnerCompleteAmount;
        
        onWaveComplete.AddListener(IncrementWavesCompleted);
        onWaveComplete.AddListener(SpawnEnemiesOnWaveComplete);

        // Initialize the spawn timer
        _spawnTimer = new CountdownTimer(spawnInterval);
        _spawnTimer.OnTimerEnd += SpawnAllEnemies;
        _spawnTimer.OnTimerEnd += _spawnTimer.Stop;

        // If this spawner is active on start, start spawning
        if (isActiveOnStart)
            StartSpawning();
    }

    private void SpawnEnemiesOnWaveComplete()
    {
        Debug.Log($"Spawning enemies: {_wavesCompleted} - {spawnerCompleteAmount}");

        // Return if the spawner is not infinite and the spawner has completed the required amount of waves
        if (!isInfinite && _wavesCompleted >= spawnerCompleteAmount)
        {
            // Invoke the spawner complete event
            if (_wavesCompleted == spawnerCompleteAmount)
                onSpawnerComplete.Invoke();

            return;
        }

        // Reset the timer
        _spawnTimer.SetMaxTimeAndReset(spawnInterval);

        // Start spawning the next wave
        _spawnTimer.Start();
    }

    private void IncrementWavesCompleted()
    {
        _wavesCompleted++;
    }

    protected override void CustomDestroy()
    {
    }

    private void Update()
    {
        _spawnTimer.SetMaxTime(spawnInterval);
        _spawnTimer.Update(Time.deltaTime);
    }

    protected override void CustomSpawnEnemy(Enemy actualEnemy, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        // Increment the wave enemies remaining
        _waveEnemiesRemaining++;

        actualEnemy.EnemyInfo.OnDeath += DecrementEnemiesRemaining;
        actualEnemy.EnemyInfo.OnDeath += OnEnemyDeath;
        actualEnemy.EnemyInfo.OnDeath += SpawnItemOnEnemyDeath;
    }

    private void SpawnItemOnEnemyDeath(object sender, HealthChangedEventArgs e)
    {
        // Return if the spawner is infinite
        if (isInfinite)
            return;

        // Return if the enemy killed count is not equal to the spawner complete amount
        if (_waveEnemiesRemaining != 0)
            return;

        // Return if there are waves remaining
        if (_wavesCompleted < spawnerCompleteAmount)
            return;

        SpawnItem(e);
    }

    private void DecrementEnemiesRemaining(object sender, HealthChangedEventArgs e)
    {
        _waveEnemiesRemaining--;
    }

    private void OnEnemyDeath(object sender, HealthChangedEventArgs e)
    {
        // Check if the wave is complete
        if (_waveEnemiesRemaining == 0)
            onWaveComplete.Invoke();
    }

    protected override void CustomStartSpawning()
    {
        // Start the initial wave
        if (!_spawnedInitialWave)
            SpawnAllEnemies();

        _spawnedInitialWave = true;

        // TODO: Fix bug where stopping the spawner between waves still spawns the next wave
    }

    protected override void CustomStopSpawning()
    {
    }

    private void SpawnAllEnemies()
    {
        foreach (var waveSpawnInfo in waveSpawnInfos)
            SpawnEnemy(waveSpawnInfo.EnemyPrefab, waveSpawnInfo.SpawnPoint.position, waveSpawnInfo.SpawnPoint.rotation);
    }
    
    protected override string GetTooltipText()
    {
        return $"Remaining Enemies: {_totalEnemiesLeft}";
    }

    protected override bool TooltipEndCondition()
    {
        return _totalEnemiesLeft <= 0;
    }

    private void OnDrawGizmos()
    {
        const float sphereSize = 0.25f;

        // Return if the spawn points is null
        if (waveSpawnInfos == null)
            return;

        foreach (var spawnPoint in waveSpawnInfos)
        {
            // continue if the spawn point is null
            if (spawnPoint == null)
                continue;

            // Draw a green sphere for each spawn point
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(spawnPoint.SpawnPoint.position, sphereSize);

            // Draw arrows for each spawn point
            Gizmos.color = Color.red;
            CustomFunctions.DrawArrow(spawnPoint.SpawnPoint.position, spawnPoint.SpawnPoint.forward);
        }
    }

    [Serializable]
    private class WaveSpawnInfo
    {
        [SerializeField] private Enemy enemyPrefab;
        [SerializeField] private Transform spawnPoint;

        public Enemy EnemyPrefab => enemyPrefab;
        public Transform SpawnPoint => spawnPoint;
    }
}