using UnityEngine;

/// <summary>
/// An interface that defines an object that can be interacted with.
/// When I say "interacted with",
/// I mean that the player can press E when looking at it to do something.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Words that pop up when looking at the object.
    /// </summary>
    public string InteractText { get; }

    /// <summary>
    /// Is the player currently looking at the object?
    /// </summary>
    public bool IsCurrentlyLookedAt { get; set; }

    /// <summary>
    /// Can the player interact with the object right now?
    /// </summary>
    public bool IsInteractable { get; }

    /// <summary>
    /// A reference to the game object that the script is attached to.
    /// </summary>
    public GameObject GameObject { get; }

    /// <summary>
    /// The function that is called when the player interacts with the object.
    /// </summary>
    void Interact(PlayerInteraction playerInteraction);
}