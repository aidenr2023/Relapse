using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

public class InventoryPickup : MonoBehaviour, IInteractable
{
    #region Serialized Fields

    [SerializeField] private InventoryEntry inventoryEntry;

    [SerializeField] private bool destroyOnPickup = true;

    [SerializeField] private UnityEvent onInteract;

    #endregion

    #region Getters

    public InventoryEntry InventoryEntry => inventoryEntry;

    public GameObject GameObject => gameObject;

    public bool IsInteractable => true;

    public bool HasOutline { get; set; }

    public HashSet<Material> OutlineMaterials { get; } = new();

    public InteractionIcon InteractionIcon => InteractionIcon.Pickup;

    #endregion


    private bool _isMarkedForDestruction;

    public void Interact(PlayerInteraction playerInteraction)
    {
        // Add the inventory entry to the player's inventory
        playerInteraction.Player.PlayerInventory.AddItem(inventoryEntry);

        // Subtract the quantity from the inventory entry
        inventoryEntry.RemoveQuantity(inventoryEntry.Quantity);

        // Destroy the game object if destroyOnPickup is true
        if (destroyOnPickup)
            _isMarkedForDestruction = true;

        // Invoke the onInteract event
        onInteract?.Invoke();
    }

    public void LookAtUpdate(PlayerInteraction playerInteraction)
    {
    }

    public string InteractText(PlayerInteraction playerInteraction)
    {
        StringBuilder sb = new();

        sb.Append("Pick up ");

        if (inventoryEntry.Quantity > 1)
            sb.Append($"{inventoryEntry.Quantity}x ");

        sb.Append(inventoryEntry.InventoryObject.ItemName);

        return sb.ToString();
    }

    private void Update()
    {
        // Destroy the game object if it is marked for destruction
        if (_isMarkedForDestruction)
        {
            Destroy(gameObject);
            return;
        }
    }
}