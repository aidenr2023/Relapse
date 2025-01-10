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

    protected virtual void FixedUpdate()
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

    #endregion
}