using System;
using UnityEngine;

public class StationaryEnemy : MonoBehaviour, IEnemyMovementBehavior
{
    public Enemy Enemy { get; private set; }
    public GameObject GameObject => gameObject;

    private void Awake()
    {
        // Get the enemy component
        Enemy = GetComponent<Enemy>();
    }
}