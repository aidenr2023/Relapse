using System;
using UnityEngine;

[Serializable]
public struct WeightedEnemyInformation
{
    public Enemy enemy;
    [SerializeField, Min(0)] public float weight;
}