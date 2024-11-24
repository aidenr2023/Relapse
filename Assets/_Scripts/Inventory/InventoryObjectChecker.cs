using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class InventoryObjectChecker : MonoBehaviour, IInteractable
{
    #region Serialized Fields

    [SerializeField] private InventoryEntry[] requiredItems;

    [SerializeField] private bool requiresInteractionToActivate = true;

    [SerializeField] private bool destroyOnInteract;

    [SerializeField] private bool removeFromInventory;

    [SerializeField] private string interactText;

    [SerializeField] [TextArea(1, 8)] private string failedInteractText;

    [SerializeField] private UnityEvent onInventoryObjectFound;

    #endregion

    #region Private Fields

    private bool _hasInteracted;

    #endregion

    #region Getters

    public InventoryEntry[] RequiredItems => requiredItems;

    public GameObject GameObject => gameObject;

    public bool IsInteractable => requiresInteractionToActivate;

    #endregion

    public void Interact(PlayerInteraction playerInteraction)
    {
        // Return if the object has already been interacted with
        if (_hasInteracted)
            return;

        // Check if the player has all the required inventory objects
        var hasAllItems = true;
        foreach (var requiredItem in requiredItems)
        {
            // Return if the player does not have the required inventory object
            if (playerInteraction.Player.PlayerInventory.HasItem(requiredItem.InventoryObject, requiredItem.Quantity))
                continue;

            Debug.Log($"Player does not have {requiredItem.Quantity}x {requiredItem.InventoryObject.Name}!");

            hasAllItems = false;
        }

        // Return if the player does not have the required inventory object
        if (!hasAllItems)
        {
            // Create a tooltip if the player does not have the required inventory object
            if (!string.IsNullOrEmpty(failedInteractText))
                JournalTooltipManager.Instance.AddTooltip(failedInteractText);

            return;
        }

        // Set the object as interacted with
        _hasInteracted = true;

        // Invoke the event
        onInventoryObjectFound.Invoke();

        // Remove the inventory objects from the player's inventory
        if (removeFromInventory)
        {
            foreach (var requiredItem in requiredItems)
            {
                playerInteraction.Player.PlayerInventory.RemoveItem(
                    requiredItem.InventoryObject,
                    requiredItem.Quantity
                );
            }
        }

        // Destroy the game object if destroyOnInteract is true
        if (destroyOnInteract)
            Destroy(gameObject);
    }

    public void LookAtUpdate(PlayerInteraction playerInteraction)
    {
    }

    public string InteractText(PlayerInteraction playerInteraction)
    {
        return interactText;
    }

    private void Update()
    {
        // If the object does not require interaction to activate,
        // activate it if the conditions are met

        if (!requiresInteractionToActivate)
            ExternalActivate();
    }

    private void ExternalActivate()
    {
        // Return if the object has already been interacted with
        if (_hasInteracted)
            return;

        var hasAllItems = true;

        foreach (var requiredItem in requiredItems)
        {
            // Return if the player does not have the required inventory object
            if (Player.Instance.PlayerInventory.HasItem(requiredItem.InventoryObject, requiredItem.Quantity))
                continue;

            hasAllItems = false;
        }

        // Return if the player does not have the required inventory object
        if (!hasAllItems)
            return;

        // Set the object as interacted with
        _hasInteracted = true;

        // Invoke the event
        onInventoryObjectFound.Invoke();

        // Remove the inventory objects from the player's inventory
        if (removeFromInventory)
        {
            foreach (var requiredItem in requiredItems)
            {
                Player.Instance.PlayerInventory.RemoveItem(
                    requiredItem.InventoryObject,
                    requiredItem.Quantity
                );
            }
        }
    }
}