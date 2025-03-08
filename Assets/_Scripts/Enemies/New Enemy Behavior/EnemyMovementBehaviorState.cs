using System;
using UnityEngine;

[Serializable]
public struct EnemyMovementBehaviorState
{
    [SerializeField] public string name;
    [SerializeField] public BehaviorConditionFloat[] floatConditions;

    [SerializeField] public BehaviorActionMove[] moveActions;
    
    public bool TestConditions(NewEnemyBehaviorBrain brain)
    {
        // Test the Float Conditions
        foreach (BehaviorConditionFloat condition in floatConditions)
            if (!condition.TestCondition(brain))
                return false;

        return true;
    }
}