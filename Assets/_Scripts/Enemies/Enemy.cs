using System;
using UnityEngine;

[RequireComponent(typeof(EnemyInfo))]
public class Enemy : MonoBehaviour
{
    private EnemyInfo _enemyInfo;
    private IEnemyBehavior _enemyBehavior;

    private void Awake()
    {
        // Get the components
        GetComponents();
    }

    private void Start()
    {
    }

    private void GetComponents()
    {
        // Get the EnemyInfo component
        _enemyInfo = GetComponent<EnemyInfo>();
    }
}