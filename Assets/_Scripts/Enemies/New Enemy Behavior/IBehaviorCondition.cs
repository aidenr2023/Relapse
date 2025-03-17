using System;
using UnityEngine;

public interface IBehaviorCondition
{
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
        DistanceFromTarget,
        DistanceFromDestination,
        Speed,
        HealthPercentage,
    }

    public bool TestCondition(NewEnemyBehaviorBrain brain)
    {
        var value = conditionTarget switch
        {
            ConditionTarget.DistanceFromTarget => brain.DistanceFromTarget,
            ConditionTarget.DistanceFromDestination => brain.DistanceFromDestination,
            ConditionTarget.Speed => brain.Speed,
            ConditionTarget.HealthPercentage => brain.HealthPercentage,
            _ => throw new ArgumentOutOfRangeException()
        };

        // XOR the result with isInverted to invert the result if necessary
        var returnValue = conditionType switch
        {
            ConditionType.LessThan => value < targetValue,
            ConditionType.LessThanOrEqualTo => value <= targetValue,
            ConditionType.GreaterThan => value > targetValue,
            ConditionType.GreaterThanOrEqualTo => value >= targetValue,
            ConditionType.EqualToExactly => Mathf.Approximately(value, targetValue),
            ConditionType.EqualToLoosely => value >= targetValue - EPSILON && value <= targetValue + EPSILON,
            _ => throw new ArgumentOutOfRangeException()
        } ^ isInverted;

        // Debug.Log($"{conditionTarget} {conditionType} {targetValue}: {returnValue}");

        return returnValue;
    }
}

[Serializable]
public struct BehaviorConditionInt : IBehaviorCondition
{
    public ConditionTarget conditionTarget;
    public ConditionType conditionType;
    public int targetValue;
    public bool isInverted;

    public bool IsInverted => isInverted;

    public enum ConditionType
    {
        EqualTo,
        NotEqualTo,
        LessThan,
        LessThanOrEqualTo,
        GreaterThan,
        GreaterThanOrEqualTo
    }

    public enum ConditionTarget
    {
        BehaviorMode
    }

    public bool TestCondition(NewEnemyBehaviorBrain brain)
    {
        var value = conditionTarget switch
        {
            ConditionTarget.BehaviorMode => brain.BehaviorMode,
            _ => throw new ArgumentOutOfRangeException()
        };

        // XOR the result with isInverted to invert the result if necessary
        var returnValue = conditionType switch
        {
            ConditionType.LessThan => value < targetValue,
            ConditionType.LessThanOrEqualTo => value <= targetValue,
            ConditionType.GreaterThan => value > targetValue,
            ConditionType.GreaterThanOrEqualTo => value >= targetValue,
            ConditionType.EqualTo => value == targetValue,
            ConditionType.NotEqualTo => value != targetValue,
            _ => throw new ArgumentOutOfRangeException()
        } ^ isInverted;

        // Debug.Log($"{conditionTarget} {conditionType} {targetValue}: {returnValue}");

        return returnValue;
    }
}

[Serializable]
public struct BehaviorConditionBool : IBehaviorCondition
{
    public ConditionTarget conditionTarget;
    public bool targetValue;

    public enum ConditionTarget
    {
        IsTargetDetected,
    }

    public bool TestCondition(NewEnemyBehaviorBrain brain)
    {
        var value = conditionTarget switch
        {
            ConditionTarget.IsTargetDetected => brain.IsTargetDetected,
            _ => throw new ArgumentOutOfRangeException()
        };

        // XOR the result with isInverted to invert the result if necessary
        var returnValue = value == targetValue;

        return returnValue;
    }
}