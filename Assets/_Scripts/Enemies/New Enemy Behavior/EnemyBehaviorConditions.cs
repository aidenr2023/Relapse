using System;
using UnityEngine;

[Serializable]
public struct EnemyBehaviorConditions
{
    [SerializeField] public BehaviorConditionFloat[] floatConditions;
    [SerializeField] public BehaviorConditionBool[] boolConditions;
    
    public bool TestConditions(NewEnemyBehaviorBrain brain)
    {
        // Test the Float Conditions
        foreach (var condition in floatConditions)
            if (!condition.TestCondition(brain))
                return false;
        
        // Test the Bool Conditions
        foreach (var condition in boolConditions)
            if (!condition.TestCondition(brain))
                return false;

        return true;
    }
}