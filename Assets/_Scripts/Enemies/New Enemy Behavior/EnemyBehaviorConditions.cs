﻿using System;
using UnityEngine;

[Serializable]
public struct EnemyBehaviorConditions
{
    [SerializeField] public BehaviorConditionFloat[] floatConditions;
    [SerializeField] public BehaviorConditionInt[] intConditions;
    [SerializeField] public BehaviorConditionBool[] boolConditions;

    public bool TestConditions(NewEnemyBehaviorBrain brain)
    {
        // Test the Float Conditions
        if (floatConditions != null)
            foreach (var condition in floatConditions)
                if (!condition.TestCondition(brain))
                    return false;

        // Test the Int Conditions
        if (intConditions != null)
            foreach (var condition in intConditions)
                if (!condition.TestCondition(brain))
                    return false;

        // Test the Bool Conditions
        if (boolConditions != null)
            foreach (var condition in boolConditions)
                if (!condition.TestCondition(brain))
                    return false;

        return true;
    }
}