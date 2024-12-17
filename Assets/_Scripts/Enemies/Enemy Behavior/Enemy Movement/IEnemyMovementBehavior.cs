using System;
using System.Collections.Generic;

public interface IEnemyMovementBehavior : IEnemyBehavior
{
    public HashSet<object> MovementDisableTokens { get; }
}

public static class EnemyMovementBehaviorExtensions
{
    public static bool IsMovementEnabled(this IEnemyMovementBehavior movementBehavior)
    {
        return movementBehavior.MovementDisableTokens.Count == 0;
    }

    public static void AddMovementDisableToken(this IEnemyMovementBehavior movementBehavior, Object token)
    {
        movementBehavior.MovementDisableTokens.Add(token);
    }

    public static void RemoveMovementDisableToken(this IEnemyMovementBehavior movementBehavior, Object token)
    {
        movementBehavior.MovementDisableTokens.Remove(token);
    }
}