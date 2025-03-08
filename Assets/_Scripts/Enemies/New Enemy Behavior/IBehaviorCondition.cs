using System;
using UnityEngine;

public interface IBehaviorCondition
{
    public bool IsInverted { get; }
    public bool TestCondition(NewEnemyBehaviorBrain brain);
}


[Serializable]
public struct BehaviorConditionFloat : IBehaviorCondition
{
    private const float EPSILON = 0.01f;

    public ConditionTarget conditionTarget;
    public ConditionType conditionType;
    public float targetValue;
    public bool isInverted;
    
    public bool IsInverted => isInverted;

    public enum ConditionType
    {
        LessThan,
        LessThanOrEqualTo,
        GreaterThan,
        GreaterThanOrEqualTo,
        EqualToExactly,
        EqualToLoosely
    }

    public enum ConditionTarget
    {
        Distance,
        Speed,
        HealthPercentage,
    }

    public bool TestCondition(NewEnemyBehaviorBrain brain)
    {
        var value = conditionTarget switch
        {
            ConditionTarget.Distance => brain.Distance,
            ConditionTarget.Speed => brain.Speed,
            ConditionTarget.HealthPercentage => brain.HealthPercentage,
            _ => throw new ArgumentOutOfRangeException()
        };

        // XOR the result with isInverted to invert the result if necessary
        return conditionType switch
        {
            ConditionType.LessThan => value < targetValue,
            ConditionType.LessThanOrEqualTo => value <= targetValue,
            ConditionType.GreaterThan => value > targetValue,
            ConditionType.GreaterThanOrEqualTo => value >= targetValue,
            ConditionType.EqualToExactly => Mathf.Approximately(value, targetValue),
            ConditionType.EqualToLoosely => value >= targetValue - EPSILON && value <= targetValue + EPSILON,
            _ => throw new ArgumentOutOfRangeException()
        } ^ isInverted;
    }
}