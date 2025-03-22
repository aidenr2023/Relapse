using System;
using System.Collections.Generic;

public static class PickupIconManager
{
    private static readonly HashSet<IInteractable> _interactables = new();

    public static Action<IInteractable> onInteractableAdded;
    public static Action<IInteractable> onInteractableRemoved;

    public static void Add(IInteractable interactable)
    {
        // If the interactable is already in the set, return
        if (!_interactables.Add(interactable))
            return;
        
        // Invoke the event
        onInteractableAdded?.Invoke(interactable);
    }
    
    public static void Remove(IInteractable interactable)
    {
        // If the interactable is not in the set, return
        if (!_interactables.Remove(interactable))
            return;
        
        // Invoke the event
        onInteractableRemoved?.Invoke(interactable);
    }
}