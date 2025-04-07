using System;
using UnityEngine;
using Object = UnityEngine.Object;

public static class CustomFunctions
{
    private const float DEFAULT_FRAME_AMOUNT = 1 / 60f;
    private const float FIXED_FRAME_AMOUNT = 1 / 50f;
    private const int MAX_COMPONENT_SEARCH_DEPTH = 20;

    public static TComponent GetComponentInParent<TComponent>(
        this Component mb,
        int maxDepth = MAX_COMPONENT_SEARCH_DEPTH
    )
    {
        var cItem = mb.transform;

        for (var i = 0; i < maxDepth && cItem != null; i++)
        {
            // Try to get the component
            var found = cItem.TryGetComponent(out TComponent component);

            // If there is a component, return it
            if (found)
                return component;

            // Move to the parent of the current item
            cItem = cItem.transform.parent;
        }

        // There is none of the component in any of the parents
        return default;
    }

    public static bool TryGetComponentInParent<TComponent>(
        this Component mb,
        out TComponent component,
        int maxDepth = MAX_COMPONENT_SEARCH_DEPTH
    )
    {
        var cItem = mb.transform;

        for (var i = 0; i < maxDepth && cItem != null; i++)
        {
            // Try to get the component
            var found = cItem.TryGetComponent(out component);

            // If there is a component, return it
            if (found)
                return true;

            // Move to the parent of the current item
            cItem = cItem.transform.parent;
        }

        // There is none of the component in any of the parents
        component = default;

        return false;
    }

    public static float FrameAmount(float lerpAmount, bool isFixed = false, bool isUnscaled = false)
    {
        var frameAmount = isFixed ? FIXED_FRAME_AMOUNT : DEFAULT_FRAME_AMOUNT;

        float deltaTime;

        if (isUnscaled)
            deltaTime = isFixed ? Time.fixedUnscaledDeltaTime : Time.unscaledDeltaTime;
        else
            deltaTime = isFixed ? Time.fixedDeltaTime : Time.deltaTime;

        return deltaTime / frameAmount * lerpAmount;
    }

    public static float FrameAmount(float lerpAmount, float deltaTime, bool isFixed)
    {
        var frameAmount = isFixed ? FIXED_FRAME_AMOUNT : DEFAULT_FRAME_AMOUNT;

        return deltaTime / frameAmount * lerpAmount;
    }

    public static void DrawArrow(
        Vector3 position, Vector3 forward,
        float arrowLength = 3, float arrowYOffset = 2, float arrowAngleSize = 30
    )
    {
        var arrowStart = position - forward * arrowLength / 2 + Vector3.up * arrowYOffset;
        var arrowEnd = position + forward * arrowLength + Vector3.up * arrowYOffset;

        // Draw the forward of the respawn point
        Gizmos.DrawLine(arrowStart, arrowEnd);

        // Draw the arrow head
        Gizmos.DrawLine(
            arrowEnd,
            arrowEnd + Quaternion.Euler(0, arrowAngleSize, 0) * -forward * arrowLength / 4
        );
        Gizmos.DrawLine(
            arrowEnd,
            arrowEnd + Quaternion.Euler(0, -arrowAngleSize, 0) * -forward * arrowLength / 4
        );
    }

    public static bool IsNotNull(this UnityEngine.Object obj) => obj != null;

    public static Result<T> NullCheckToResult<T>(this T obj, string valueName = "Value") where T : UnityEngine.Object
    {
        if (obj == null)
            return Result<T>.Error($"{valueName} is null!");

        return Result<T>.Ok(obj);
    }

    public static Result<T> BoolToResult<T>(this bool condition, T value)
    {
        return Result<T>.BoolToResult(value, v => condition);
    }
}