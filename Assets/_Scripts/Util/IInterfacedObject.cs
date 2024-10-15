using UnityEngine;

public interface IInterfacedObject
{
    /// <summary>
    /// A reference to the game object that the script is attached to.
    /// </summary>

    public GameObject GameObject { get; }
}