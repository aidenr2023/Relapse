using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class NewEnemyBehaviorBrain : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField, Range(.001f, 60)] private float updatesPerSecond = 4f;
    [SerializeField] private EnemyMovementBehaviorState[] behaviorStates;

    [SerializeField] private BehaviorActionAttack currentAttackAction;
    [SerializeField] private BehaviorActionMove currentMoveAction;

    #endregion

    #region Private Fields

    private Coroutine _behaviorStateCoroutine;
    private EnemyMovementBehaviorState _currentBehaviorState;

    private readonly CountdownTimer _moveCooldown = new(10000, true, true);
    private readonly CountdownTimer _attackCooldown = new(10000, true, true);

    private bool _isAttacking;

    #endregion

    // TODO: Remove the inspector fields and replace them with properties

    #region Float Variables

    public float DistanceFromTarget { get; set; }

    public float DistanceFromDestination { get; set; }
    public float Speed { get; set; }
    public float HealthPercentage { get; set; }

    #endregion

    #region Getters

    public BehaviorActionMove CurrentMoveAction => currentMoveAction;
    public BehaviorActionAttack CurrentAttackAction => currentAttackAction;

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

    // TODO: Account for attacks as well
    private void DetermineAttackAction()
    {
        // If there are no attack actions, return
        if (_currentBehaviorState.attackActions.Length == 0)
        {
            _isAttacking = false;
            return;
        }

        var totalWeight = _currentBehaviorState.attackActions.Sum(n => n.Weight);

        // Generate a random number between 0 and the total weight
        var randomWeight = UnityEngine.Random.Range(0, totalWeight);

        int index;

        // Keep subtracting the weight of the current action from the random weight until it's less than or equal to 0
        for (index = 0; index < _currentBehaviorState.attackActions.Length; index++)
        {
            randomWeight -= _currentBehaviorState.attackActions[index].Weight;

            if (randomWeight <= 0)
                break;
        }

        // Update the current move action   
        currentAttackAction = _currentBehaviorState.attackActions[index];

        // Start the current action
        currentAttackAction.Start(this, _currentBehaviorState);

        // Generate a random cooldown time between the min and max cooldown times
        var cooldownTime = UnityEngine.Random.Range(currentAttackAction.minCooldown, currentAttackAction.maxCooldown);

        // Update the move cooldown
        _attackCooldown.SetMaxTimeAndReset(cooldownTime);

        // Set the attacking flag
        _isAttacking = true;

        // Add an action to reset the attacking flag
        currentAttackAction.OnEnd += (_, _, _) => _isAttacking = false;

        // TODO: Remove this
        Invoke(nameof(StopAttack_DEBUG), 1);
    }

    private void StopAttack_DEBUG()
    {
        _isAttacking = false;

        Debug.Log($"DONE ATTACKING");
    }

    private IEnumerator BehaviorStateCoroutine()
    {
        var lastUpdateTime = Time.time;

        while (true)
        {
            // Update the countdown
            var timeSinceLastUpdate = Time.time - lastUpdateTime;
            _moveCooldown.Update(timeSinceLastUpdate);
            _attackCooldown.Update(timeSinceLastUpdate);

            // Determine the behavior state
            DetermineBehaviorState();

            // Determine the move action
            if (_moveCooldown.IsComplete)
                DetermineMoveAction();

            // Determine the attack action
            if (_attackCooldown.IsComplete && !_isAttacking)
                DetermineAttackAction();

            // Log the current behavior state
            Debug.Log($"{gameObject.name} Behavior State: {_currentBehaviorState.name}");

            // Reset the last update time
            lastUpdateTime = Time.time;

            yield return new WaitForSeconds(1 / updatesPerSecond);
        }
    }
}