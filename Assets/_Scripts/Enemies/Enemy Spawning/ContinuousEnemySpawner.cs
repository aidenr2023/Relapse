using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ContinuousEnemySpawner : ContinuousEnemySpawnerBase
{
    #region Serialized Fields

    [SerializeField] private Enemy[] enemyPrefabs;

    #endregion

    protected override bool CanSpawnRandomEnemy => enemyPrefabs.Length > 0;

    protected override Enemy GetRandomEnemyPrefab()
    {
        return enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Length)];
    }

    protected override string GetTooltipText()
    {
        return $"Remaining Enemies: {spawnerCompleteAmount - _enemyKilledCount}";
    }

    protected override bool TooltipEndCondition()
    {
        return spawnerCompleteAmount - _enemyKilledCount >= 0;
    }
}