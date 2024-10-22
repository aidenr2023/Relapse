using UnityEngine;
using UnityEngine.InputSystem;

public abstract class PlayerMovementScript : ComponentScript<PlayerMovementV2>, IDebugged
{
    /// <summary>
    /// The corresponding input action map for this movement script.
    /// Will be disabled when this movement script is disabled & enabled when this movement script is enabled.
    /// </summary>
    public abstract InputActionMap InputActionMap { get; }

    public abstract void FixedMovementUpdate();

    public abstract string GetDebugText();

    #region Implemented Methods

    protected void FixedUpdate()
    {
    }

    protected void PushControls(PlayerMovementScript obj)
    {
        // Add the current object to the movement scripts
        ParentComponent.PushMovementScript(obj);
    }

    protected void RemoveControls(PlayerMovementScript obj)
    {
        // Remove the current object from the movement scripts
        ParentComponent.RemoveMovementScript(obj);
    }

    protected void ApplyLateralSpeedLimit(float speed)
    {
        // Create a vector2 with the x and z components of the velocity
        var velocity2D = new Vector2(
            ParentComponent.Rigidbody.velocity.x,
            ParentComponent.Rigidbody.velocity.z
        );

        // Get the magnitude of the velocity
        var velocityMagnitude = velocity2D.magnitude;

        // Return if the magnitude is less than the speed limit
        if (velocityMagnitude <= speed)
            return;

        // Get the velocity vector
        var velocityVector = velocity2D.normalized * speed;

        // Set the velocity of the rigid body
        ParentComponent.Rigidbody.velocity = new Vector3(
            velocityVector.x,
            ParentComponent.Rigidbody.velocity.y,
            velocityVector.y
        );

        // var postVelocity2D = new Vector2(
        //     ParentComponent.Rigidbody.velocity.x,
        //     ParentComponent.Rigidbody.velocity.z
        // );
    }

    protected void ApplyLateralSpeedLimit()
    {
        ApplyLateralSpeedLimit(ParentComponent.MovementSpeed);
    }

    #endregion
}