using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NewEnemyBehaviorBrain : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField, Range(.001f, 60)] private float updatesPerSecond = 4f;
    [SerializeField] private EnemyMovementBehaviorState[] behaviorStates;

    [SerializeField] private BehaviorActionMove currentMoveAction;

    #endregion

    #region Private Fields

    private Coroutine _behaviorStateCoroutine;
    private EnemyMovementBehaviorState _currentBehaviorState;

    private readonly CountdownTimer _moveCooldown = new(10000, true, true);

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

    private void Awake()
    {
    }

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

        // If the new behavior state is the same as the current behavior state, return
        if (bestBehaviorState == _currentBehaviorState)
            return;

        // Reset the current action
        ResetCurrentAction();

        // Set the current behavior state to the best behavior state
        _currentBehaviorState = bestBehaviorState;
    }

    private void ResetCurrentAction()
    {
        // Force the move timer to be complete
        _moveCooldown.ForcePercent(1);
    }

    // TODO: Account for attacks as well
    private void DetermineMoveAction()
    {
        // Get the total weight of all the actions
        var totalWeight = _currentBehaviorState.moveActions.Sum(n => n.Weight);

        // Generate a random number between 0 and the total weight
        var randomWeight = UnityEngine.Random.Range(0, totalWeight);

        int index;

        // Keep subtracting the weight of the current action from the random weight until it's less than or equal to 0
        for (index = 0; index < _currentBehaviorState.moveActions.Length; index++)
        {
            randomWeight -= _currentBehaviorState.moveActions[index].Weight;

            if (randomWeight <= 0)
                break;
        }

        // Update the current move action   
        currentMoveAction = _currentBehaviorState.moveActions[index];

        // Start the current action
        currentMoveAction.Start(this, _currentBehaviorState);

        // Generate a random cooldown time between the min and max cooldown times
        var cooldownTime = UnityEngine.Random.Range(currentMoveAction.minCooldown, currentMoveAction.maxCooldown);

        // Update the move cooldown
        _moveCooldown.SetMaxTimeAndReset(cooldownTime);
    }


    private IEnumerator BehaviorStateCoroutine()
    {
        var lastUpdateTime = Time.time;

        while (true)
        {
            // Update the countdown
            _moveCooldown.Update(Time.time - lastUpdateTime);

            // Determine the behavior state
            DetermineBehaviorState();

            // Determine the move action
            if (_moveCooldown.IsComplete)
                DetermineMoveAction();

            // Log the current behavior state
            Debug.Log($"{gameObject.name} Behavior State: {_currentBehaviorState.name}");

            // Reset the last update time
            lastUpdateTime = Time.time;
            
            yield return new WaitForSeconds(1 / updatesPerSecond);
        }
    }
}