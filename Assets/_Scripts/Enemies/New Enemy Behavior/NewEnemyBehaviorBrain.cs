using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

public class NewEnemyBehaviorBrain : MonoBehaviour, IDebugged
{
    #region Serialized Fields

    [SerializeField, Range(.001f, 60)] private float updatesPerSecond = 4f;
    [SerializeField] private EnemyBehaviorStateBase[] behaviorStates;

    #endregion

    #region Private Fields

    private BehaviorActionAttack _currentAttackAction;
    private BehaviorActionMove _currentMoveAction;
    private INewEnemyMovementBehavior _movementBehavior;

    private Coroutine _behaviorStateCoroutine;
    private EnemyBehaviorState _currentBehaviorState;

    private readonly CountdownTimer _moveCooldown = new(10000, true, true);
    private readonly CountdownTimer _attackCooldown = new(10000, true, true);

    private bool _isAttacking;

    #endregion
    
    public Action<BehaviorActionMove.MoveAction, NewEnemyBehaviorBrain> onPlayerMovementStateChange;

    #region Float Variables

    public float DistanceFromTarget { get; set; }

    public float DistanceFromDestination { get; set; }
    public float Speed { get; set; }
    public float HealthPercentage { get; set; }

    #endregion

    #region Bool Variables

    public bool IsTargetDetected { get; set; }

    #endregion

    #region Getters

    public BehaviorActionMove CurrentMoveAction => _currentMoveAction;
    public BehaviorActionAttack CurrentAttackAction => _currentAttackAction;
    
    public INewEnemyMovementBehavior MovementBehavior => _movementBehavior;
    
    public BehaviorActionMove.MoveAction CurrentMoveActionType => _currentMoveAction.moveAction;

    #endregion

    #region Initialization Methods

    private void Awake()
    {
        // Get the movement behavior
        _movementBehavior = GetComponent<INewEnemyMovementBehavior>();
        
        // Assert that the movement behavior is not null
        Debug.Assert(_movementBehavior != null, "The movement behavior is null!");
    }

    private void OnEnable()
    {
        _behaviorStateCoroutine = StartCoroutine(BehaviorStateCoroutine());

        DebugManager.Instance.AddDebuggedObject(this);
    }

    private void OnDisable()
    {
        StopCoroutine(_behaviorStateCoroutine);
        _behaviorStateCoroutine = null;

        DebugManager.Instance.RemoveDebuggedObject(this);
    }

    #endregion

    private void DetermineBehaviorState()
    {
        // Automatically set the best behavior state to the last one
        var bestBehaviorState = GetBehaviorStateRecursive(behaviorStates);

        var invokeEvent = false;

        // Store the previous movement state
        var previousMovementAction = _currentMoveAction;
        
        if (_currentBehaviorState != bestBehaviorState)
        {
            // Reset the current action
            ResetCurrentAction();
            
            // Set the flag to invoke the event
            invokeEvent = true;
        }

        // Set the current behavior state to the best behavior state
        _currentBehaviorState = bestBehaviorState;
        
        onPlayerMovementStateChange?.Invoke(previousMovementAction.moveAction, this);
    }

    private EnemyBehaviorState GetBehaviorStateRecursive(EnemyBehaviorStateBase[] currentStates)
    {
        // Automatically set the best behavior state to the last one
        var bestBehaviorState = currentStates[^1];

        for (var i = 0; i < currentStates.Length; i++)
        {
            // Test the conditions of the behavior state
            if (!currentStates[i].TestConditions(this))
                continue;

            // Set the best behavior state to the current one
            bestBehaviorState = currentStates[i];

            // If the current state is a SubStateHolder, recursively call this method
            if (bestBehaviorState is EnemyBehaviorSubStateHolder subStateHolder)
                return GetBehaviorStateRecursive(subStateHolder.subStates);

            // Otherwise, return the current state
            if (bestBehaviorState is EnemyBehaviorState behaviorState)
                return behaviorState;
        }

        // This might be a SubStateHolder
        return bestBehaviorState as EnemyBehaviorState;
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

        // Keep subtracting the weight of the current action from the random weight until it's less than or equal to 0
        for (var index = 0; index < _currentBehaviorState.moveActions.Length; index++)
        {
            randomWeight -= _currentBehaviorState.moveActions[index].Weight;

            if (randomWeight > 0)
                continue;
            
            // Update the current move action   
            _currentMoveAction = _currentBehaviorState.moveActions[index];
            break;
        }

        // Start the current action
        _currentMoveAction.Start(this, _currentBehaviorState);

        // Generate a random cooldown time between the min and max cooldown times
        var cooldownTime = UnityEngine.Random.Range(_currentMoveAction.minCooldown, _currentMoveAction.maxCooldown);

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
        _currentAttackAction = _currentBehaviorState.attackActions[index];

        // Start the current action
        _currentAttackAction.Start(this, _currentBehaviorState);

        // Generate a random cooldown time between the min and max cooldown times
        var cooldownTime = UnityEngine.Random.Range(_currentAttackAction.minCooldown, _currentAttackAction.maxCooldown);

        // Update the move cooldown
        _attackCooldown.SetMaxTimeAndReset(cooldownTime);

        // Set the attacking flag
        _isAttacking = true;

        // Add an action to reset the attacking flag
        _currentAttackAction.OnEnd += (_, _, _) => _isAttacking = false;

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
        // Delay the first update by a frame to allow the other scripts to initialize
        yield return null;
        
        while (true)
        {
            // Determine the behavior state
            DetermineBehaviorState();

            // Determine the move action
            if (_moveCooldown.IsComplete)
                DetermineMoveAction();

            // Determine the attack action
            if (_attackCooldown.IsComplete && !_isAttacking)
                DetermineAttackAction();

            // Log the current behavior state
            Debug.Log($"{gameObject.name} Behavior State: {_currentBehaviorState.stateName}");


            // Reset the last update time
            var updateDelay = 1 / updatesPerSecond;

            // Update the countdowns
            _moveCooldown.Update(updateDelay);
            _attackCooldown.Update(updateDelay);

            yield return new WaitForSeconds(updateDelay);
        }
    }

    public string GetDebugText()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"{gameObject.name}");
        sb.AppendLine($"\tBehavior State: {_currentBehaviorState.stateName}");
        sb.AppendLine($"\tMove Cooldown: {_moveCooldown.TimeLeft:0.00} ({_moveCooldown.IsComplete})");
        sb.AppendLine($"\t{_currentMoveAction.moveAction}");
        sb.AppendLine($"\tTarget: {DistanceFromTarget:0.00}");
        sb.AppendLine($"\tDestination: {DistanceFromDestination:0.00}");

        return sb.ToString();
    }
}