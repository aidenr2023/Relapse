using UnityEngine;

/// <summary>
/// An interface that defines an object that can be interacted with.
/// When I say "interacted with",
/// I mean that the player can press E when looking at it to do something.
/// </summary>
public interface IInteractable
{
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
    public void Interact(PlayerInteraction playerInteraction);

    /// <summary>
    /// An update function that is called when the player is looking at the object.
    /// </summary>
    /// <param name="playerInteraction"></param>
    public void LookAtUpdate(PlayerInteraction playerInteraction);
    
    /// <summary>
    /// Words that pop up when looking at the object.
    /// </summary>
    public string InteractText(PlayerInteraction playerInteraction);
}

public static class IInteractableExtensions
{
    /// <summary>
    /// Is the player currently looking at the object?
    /// </summary>
    public static bool IsCurrentlySelected(this IInteractable interactable, PlayerInteraction playerInteraction)
    {
        return playerInteraction.SelectedInteractable == interactable;
    }
}