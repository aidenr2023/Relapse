using System.Collections.Generic;

public interface IEnemyAttackBehavior : IEnemyBehavior, IDamager
{
    public HashSet<object> AttackDisableTokens { get; }

    public bool IsAttackEnabled { get; }

    public void SetAttackEnabled(bool on);
}

public static class EnemyAttackBehaviorExtensions
{
    public static bool IsAttackEnabledTokens(this IEnemyAttackBehavior attackBehavior)
    {
        return attackBehavior.AttackDisableTokens.Count == 0;
    }

    public static void AddAttackDisableToken(this IEnemyAttackBehavior attackBehavior, object token)
    {
        attackBehavior.AttackDisableTokens.Add(token);
    }

    public static void RemoveAttackDisableToken(this IEnemyAttackBehavior attackBehavior, object token)
    {
        attackBehavior.AttackDisableTokens.Remove(token);
    }
}