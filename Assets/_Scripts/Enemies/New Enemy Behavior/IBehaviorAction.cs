using System;
using UnityEngine;

public interface IBehaviorAction
{
    public float Weight { get; }
    
    public void Start(NewEnemyBehaviorBrain brain);
}

[Serializable]
public struct BehaviorActionMove : IBehaviorAction
{
    public MoveAction moveAction;
    public float weight;
    
    public float minCooldown;
    public float maxCooldown;
    
    public float Weight => weight;
    
    public void Start(NewEnemyBehaviorBrain brain)
    {
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
        
        MovementScript,
    }
}