public interface IUsesInput
{
    /// <summary>
    /// A function used to initialize the input for the class.
    /// Is SUPPOSED to connect to the InputManager
    /// </summary>
    public void InitializeInput();

    /// <summary>
    /// A function to UNDO the input initialization.
    /// </summary>
    public void RemoveInput();
}