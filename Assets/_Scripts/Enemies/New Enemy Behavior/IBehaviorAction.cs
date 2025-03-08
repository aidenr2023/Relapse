using System;
using UnityEngine;

public interface IBehaviorAction
{
    public Action<IBehaviorAction, NewEnemyBehaviorBrain, EnemyMovementBehaviorState> OnEnd { get; set; }

    public float Weight { get; }

    public void Start(NewEnemyBehaviorBrain brain, EnemyMovementBehaviorState state);
    public void End(NewEnemyBehaviorBrain brain, EnemyMovementBehaviorState state);
}

[Serializable]
public struct BehaviorActionMove : IBehaviorAction
{
    public MoveAction moveAction;
    [SerializeField, Min(0)] private float weight;

    [Min(1 / 60f)] public float minCooldown;
    [Min(1 / 60f)] public float maxCooldown;

    public Action<IBehaviorAction, NewEnemyBehaviorBrain, EnemyMovementBehaviorState> OnEnd { get; set; }

    public float Weight => weight;

    public void Start(NewEnemyBehaviorBrain brain, EnemyMovementBehaviorState state)
    {
    }

    public void End(NewEnemyBehaviorBrain brain, EnemyMovementBehaviorState state)
    {
        OnEnd?.Invoke(this, brain, state);
    }

    public enum MoveAction
    {
        Idle,

        StrafeLeft,
        StrafeRight,
        StrafeForward,
        StrafeBackward,

        MoveTowardTarget,
        MoveAwayFromTarget,

        Wander,

        MovementScript,
    }
}

[Serializable]
public struct BehaviorActionAttack : IBehaviorAction
{
    public AttackAction attackAction;
    [SerializeField, Min(0.001f)] private float weight;

    [Min(1 / 60f)] public float minCooldown;
    [Min(1 / 60f)] public float maxCooldown;

    public Action<IBehaviorAction, NewEnemyBehaviorBrain, EnemyMovementBehaviorState> OnEnd { get; set; }
    public float Weight => weight;

    public void Start(NewEnemyBehaviorBrain brain, EnemyMovementBehaviorState state)
    {
    }

    public void End(NewEnemyBehaviorBrain brain, EnemyMovementBehaviorState state)
    {
        OnEnd?.Invoke(this, brain, state);
    }

    public enum AttackAction
    {
        None, 
        
        Attack1,
    }
}