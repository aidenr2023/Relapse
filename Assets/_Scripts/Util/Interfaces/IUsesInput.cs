using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public interface IUsesInput
{
    public HashSet<InputData> InputActions { get; }

    /// <summary>
    /// A function used to initialize the input for the class.
    /// This function should ONLY be called in the Start() function BEFORE being added to the InputManager.
    /// </summary>
    public void InitializeInput();
}

public enum InputType
{
    Started,
    Performed,
    Canceled,
}

public struct InputData : IEquatable<InputData>
{
    public readonly InputAction inputAction;
    public readonly InputType inputType;
    public readonly Action<InputAction.CallbackContext> inputFunc;

    public InputData(
        InputAction inputAction,
        InputType inputType,
        Action<InputAction.CallbackContext> inputFunc
    )
    {
        this.inputAction = inputAction;
        this.inputType = inputType;
        this.inputFunc = inputFunc;
    }

    public bool Equals(InputData other)
    {
        return Equals(inputAction, other.inputAction) && inputType == other.inputType && Equals(inputFunc, other.inputFunc);
    }

    public override bool Equals(object obj)
    {
        return obj is InputData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(inputAction, (int)inputType, inputFunc);
    }
}