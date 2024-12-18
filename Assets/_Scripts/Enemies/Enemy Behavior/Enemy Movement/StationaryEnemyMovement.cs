using System;
using System.Collections.Generic;
using UnityEngine;

public class StationaryEnemyMovement : MonoBehaviour, IEnemyMovementBehavior
{
    public Enemy Enemy { get; private set; }
    public GameObject GameObject => gameObject;

    public HashSet<object> MovementDisableTokens { get; } = new();
    public TokenManager<float> MovementSpeedTokens { get; } = new(false, null, 1);

    private void Awake()
    {
        // Get the enemy component
        Enemy = GetComponent<Enemy>();
    }

    private void Update()
    {
        // Update the movement speed tokens
        MovementSpeedTokens.Update(Time.deltaTime);
    }
}