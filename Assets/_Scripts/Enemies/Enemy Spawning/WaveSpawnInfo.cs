using System;
using UnityEngine;

[Serializable]
public struct WaveSpawnInfo
{
    [SerializeField] public WaveEnemyInfo[] waveEnemyInfos;
}