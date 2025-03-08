using UnityEngine;

public class EnemyBehaviorStateBase : ScriptableObject
{
    [SerializeField] public string stateName;
    [SerializeField] public EnemyBehaviorConditions[] conditions;

    public bool TestConditions(NewEnemyBehaviorBrain brain)
    {
        // Test each condition
        foreach (var condition in conditions)
            if (!condition.TestConditions(brain))
                return false;

        return true;
    }
}