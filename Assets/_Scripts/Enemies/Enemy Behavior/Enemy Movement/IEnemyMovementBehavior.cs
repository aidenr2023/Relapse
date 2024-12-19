using System;
using System.Collections.Generic;

public interface IEnemyMovementBehavior : IEnemyBehavior
{
    public HashSet<object> MovementDisableTokens { get; }

    public TokenManager<float> MovementSpeedTokens { get; }
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

    public static void AddMovementSpeedToken(this IEnemyMovementBehavior movementBehavior, float speed, float duration)
    {
        movementBehavior.MovementSpeedTokens.AddToken(speed, duration);
    }

    public static void RemoveMovementSpeedToken(
        this IEnemyMovementBehavior movementBehavior,
        TokenManager<float>.ManagedToken token
    )
    {
        movementBehavior.MovementSpeedTokens.RemoveToken(token);
    }

    public static float GetMovementSpeedTokenMultiplier(this IEnemyMovementBehavior movementBehavior)
    {
        var multiplier = 1f;

        foreach (var speedToken in movementBehavior.MovementSpeedTokens.Tokens)
            multiplier *= speedToken.Value;

        return multiplier;
    }
}