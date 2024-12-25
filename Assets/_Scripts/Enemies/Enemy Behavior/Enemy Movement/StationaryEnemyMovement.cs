using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StationaryEnemyMovement : MonoBehaviour, IEnemyMovementBehavior
{
    #region Private Fields

    private NavMeshAgent _navMeshAgent;

    #endregion

    #region Getters

    public Enemy Enemy { get; private set; }
    public GameObject GameObject => gameObject;

    public HashSet<object> MovementDisableTokens { get; } = new();
    public TokenManager<float> MovementSpeedTokens { get; } = new(false, null, 1);

    #endregion

    private void Awake()
    {
        // Get the enemy component
        Enemy = GetComponent<Enemy>();

        // Get the nav mesh agent component
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        // Update the movement speed tokens
        MovementSpeedTokens.Update(Time.deltaTime);

        // Update the nav mesh agent
        UpdateNavMeshAgent();
    }

    private void UpdateNavMeshAgent()
    {
        // Return if the nav mesh agent is null
        if (_navMeshAgent == null)
            return;

        _navMeshAgent.updateRotation = true;
        _navMeshAgent.updatePosition = false;

        // Return if the detection is unaware
        if (Enemy.EnemyDetectionBehavior.CurrentDetectionState == EnemyDetectionState.Unaware)
            return;

        // Set the destination to the target's position
        _navMeshAgent.SetDestination(Enemy.EnemyDetectionBehavior.LastKnownTargetPosition);
    }
}