using System;
using UnityEngine;

public static class CustomFunctions
{
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
}