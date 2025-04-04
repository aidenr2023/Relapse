using UnityEngine;

public class WeightedContinuousEnemySpawner : ContinuousEnemySpawnerBase
{
    #region Serialized Fields

    [SerializeField] private WeightedEnemyInformation[] enemyPrefabs;

    #endregion

    protected override bool CanSpawnRandomEnemy => enemyPrefabs.Length > 0;

    private void Awake()
    {
        // Correct the weights of the enemies
        CorrectWeights();
    }

    protected override Enemy GetRandomEnemyPrefab()
    {
        // Get the total weight of all enemies
        var totalWeight = 0f;

        foreach (var weightedEnemyInformation in enemyPrefabs)
            totalWeight += weightedEnemyInformation.weight;

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

    private void CorrectWeights()
    {
        for (var i = 0; i < enemyPrefabs.Length; i++)
        {
            if (enemyPrefabs[i].weight <= 0)
                enemyPrefabs[i].weight = 1f;
        }
    }

    protected override string GetTooltipText()
    {
        return $"Remaining Enemies: {spawnerCompleteAmount - _enemyKilledCount}";
    }
}