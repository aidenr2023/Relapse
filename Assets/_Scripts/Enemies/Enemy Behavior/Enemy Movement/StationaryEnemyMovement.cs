using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StationaryEnemyMovement : MonoBehaviour, IEnemyMovementBehavior
{
    #region Serialized Fields

    [SerializeField, Range(0, 1)] private float rotationLerpAmount = 0.15f;

    #endregion

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

        // Return if the target is null
        if (Enemy.EnemyDetectionBehavior.Target == null)
            return;
        
        // If there are any movement disable tokens, return
        if (MovementDisableTokens.Count > 0)
            return;

        var oldRotation = transform.rotation;

        var diff = Enemy.EnemyDetectionBehavior.LastKnownTargetPosition - transform.position;

        const float defaultFrameTime = 1 / 60f;
        var frameTime = Time.deltaTime / defaultFrameTime;

        // Set the y rotation to the desired rotation's y rotation
        var desiredRotation = Quaternion.LookRotation(diff.normalized, Vector3.up);

        // Lerp the rotation
        transform.rotation = Quaternion.Lerp(oldRotation, desiredRotation, rotationLerpAmount * frameTime);

        // Update the nav mesh agent
        UpdateNavMeshAgent();
    }

    private void UpdateNavMeshAgent()
    {
        // Return if the nav mesh agent is null
        if (_navMeshAgent == null)
            return;

        _navMeshAgent.enabled = false;

        // _navMeshAgent.updateRotation = false;
        // _navMeshAgent.updatePosition = false;

        // Set the nav mesh agent's speed to 0
        // _navMeshAgent.speed = 0;

        // Return if the detection is unaware
        if (Enemy.EnemyDetectionBehavior.CurrentDetectionState == EnemyDetectionState.Unaware)
            return;

        // Set the destination to the target's position
        _navMeshAgent.SetDestination(Enemy.EnemyDetectionBehavior.LastKnownTargetPosition);
    }

    public void SetPosition(Vector3 pos)
    {
        // If there is a nav mesh agent, set the position
        if (_navMeshAgent != null)
        {
            _navMeshAgent.Warp(pos);
            return;
        }

        // If there is a rigidbody, set the position
        if (TryGetComponent(out Rigidbody rb))
        {
            rb.MovePosition(pos);
            return;
        }

        // Set the position
        transform.position = pos;
    }
}