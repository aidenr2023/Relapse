using System;
using UnityEngine;

[Serializable]
public struct WaveEnemyInfo
{
    [SerializeField] public Enemy enemyPrefab;
    [SerializeField] public Transform spawnPoint;
}