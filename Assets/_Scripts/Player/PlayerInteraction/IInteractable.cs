using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// An interface that defines an object that can be interacted with.
/// When I say "interacted with",
/// I mean that the player can press E when looking at it to do something.
/// </summary>
public interface IInteractable : IInterfacedObject
{
    /// <summary>
    /// Can the player interact with the object right now?
    /// </summary>
    public bool IsInteractable { get; }

    public bool HasOutline { get; }

    public HashSet<Material> OutlineMaterials { get; }

    /// <summary>
    /// The icon the reticle changes to when looking at the object.
    /// </summary>
    public InteractionIcon InteractionIcon { get; }
    
    public UnityEvent OnInteraction { get; }

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

    public static void GetOutlineMaterials(this IInteractable interactable, Shader shader)
    {
        // If the hash set for the outline materials is null, throw an exception
        if (interactable.OutlineMaterials == null)
            throw new System.NullReferenceException("IInteractable: OutlineMaterials hash set is null.");

        // Get the renderers of the interactable
        var renderers = interactable.GameObject.GetComponentsInChildren<Renderer>();

        // Get the materials of the renderer, including the materials in the children
        var materials = new List<Material>();

        foreach (var renderer in renderers)
            materials.AddRange(renderer.materials);

        // Loop through the materials
        foreach (var material in materials)
        {
            // Continue if the material's shader is not the shader
            if (material.shader != shader)
                continue;

            // Add the material to the hash set
            interactable.OutlineMaterials.Add(material);
        }
    }
}