using System;
using System.Collections;
using UnityEngine;

public class NewEnemyBehaviorBrain : MonoBehaviour
{
    #region Serialized Fields
    
    [SerializeField, Min(.001f)] private float behaviorStateUpdateInterval = 1 / 15f;
    [SerializeField] private EnemyMovementBehaviorState[] behaviorStates;
    
    #endregion
    
    #region Private Fields

    private Coroutine _behaviorStateCoroutine;
    private EnemyMovementBehaviorState _currentBehaviorState;

    #endregion

    #region Float Variables

    [SerializeField] private float distance;
    [SerializeField] private float speed;
    [SerializeField] private float healthPercentage;

    public float Distance => distance;
    public float Speed => speed;
    public float HealthPercentage => healthPercentage;

    #endregion

    #region Initialization Methods

    private void OnEnable()
    {
        _behaviorStateCoroutine = StartCoroutine(BehaviorStateCoroutine());
    }

    private void OnDisable()
    {
        StopCoroutine(_behaviorStateCoroutine);
        _behaviorStateCoroutine = null;
    }

    #endregion

    private void DetermineBehaviorState()
    {
        // Automatically set the best behavior state to the last one
        var bestBehaviorState = behaviorStates[^1];

        // Determine which behavior state should be used
        for (var i = 0; i < behaviorStates.Length; i++)
        {
            // Test the conditions of the behavior state
            // if the conditions are met, set the best behavior state to the current one and break the loop
            if (!behaviorStates[i].TestConditions(this))
                continue;

            bestBehaviorState = behaviorStates[i];
            break;
        }

        // Set the current behavior state to the best behavior state
        _currentBehaviorState = bestBehaviorState;
    }

    private IEnumerator BehaviorStateCoroutine()
    {
        while (true)
        {
            DetermineBehaviorState();
            
            // Log the current behavior state
            Debug.Log($"{gameObject.name} Behavior State: {_currentBehaviorState.name}");
            
            yield return new WaitForSeconds(behaviorStateUpdateInterval);
        }
    }
}