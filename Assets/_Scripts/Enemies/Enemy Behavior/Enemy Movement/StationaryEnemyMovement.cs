using System;
using System.Collections.Generic;
using UnityEngine;

public class StationaryEnemyMovement : MonoBehaviour, IEnemyMovementBehavior
{
    public Enemy Enemy { get; private set; }
    public GameObject GameObject => gameObject;

    public HashSet<object> MovementDisableTokens { get; } = new();


    private void Awake()
    {
        // Get the enemy component
        Enemy = GetComponent<Enemy>();
    }

}