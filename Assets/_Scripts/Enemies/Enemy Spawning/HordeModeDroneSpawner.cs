using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class HordeModeDroneSpawner : MonoBehaviour
{
    [SerializeField] private Enemy[] dronePrefabs;

    [SerializeField, Min(0)] private int droneCount = 5;
    [SerializeField] private Transform[] spawnPoints;

    public void SpawnDrones()
    {
        var currentDroneCount = Mathf.Min(droneCount, spawnPoints.Length);

        for (var i = 0; i < currentDroneCount; i++)
        {
            // Get a random drone prefab
            var randomDrone = dronePrefabs[Random.Range(0, dronePrefabs.Length)];

            // Get a random spawn point
            var randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // Instantiate the drone prefab at the spawn point
            var spawnedDrone = Instantiate(randomDrone, randomSpawnPoint.position, randomSpawnPoint.rotation);
        }
    }

    private void OnDrawGizmosSelected()
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