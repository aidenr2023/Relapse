using System;
using UnityEngine;
using Object = UnityEngine.Object;

public static class CustomFunctions
{
    private const int MAX_COMPONENT_SEARCH_DEPTH = 5;

    /// <summary>
    /// The standard AddForce waits until the next physics update to apply the force.
    /// This function applies the force immediately (as opposed to waiting for the next physics update).
    /// </summary>
    /// <param name="rb"></param>
    /// <param name="force"></param>
    /// <param name="forceMode"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static void CustomAddForce(this Rigidbody rb, Vector3 force, ForceMode forceMode = default)
    {
        Vector3 acceleration = default;
        var squaredDeltaTime = Time.fixedDeltaTime * Time.fixedDeltaTime;

        // acceleration = forceMode switch
        // {
        //     ForceMode.Force => force * squaredDeltaTime / rb.mass,
        //     ForceMode.Acceleration => force * squaredDeltaTime,
        //     ForceMode.Impulse => force * Time.fixedDeltaTime / rb.mass,
        //     ForceMode.VelocityChange => force * Time.fixedDeltaTime,
        //     _ => throw new ArgumentOutOfRangeException(nameof(forceMode), forceMode, null)
        // };

        acceleration = forceMode switch
        {
            ForceMode.Force => force * squaredDeltaTime / rb.mass,
            ForceMode.Acceleration => force * squaredDeltaTime,
            ForceMode.Impulse => force * Time.fixedDeltaTime / rb.mass,
            ForceMode.VelocityChange => force,
            _ => throw new ArgumentOutOfRangeException(nameof(forceMode), forceMode, null)
        };

        // rb.linearVelocity += acceleration;
        rb.velocity += acceleration;
    }

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
}