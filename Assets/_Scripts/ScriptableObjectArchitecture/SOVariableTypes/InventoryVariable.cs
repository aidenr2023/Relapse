using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Inventory Variable",
    menuName = SOAHelper.ASSET_MENU_PATH + SOAHelper.PLAYER + "Inventory Variable"
)]
public sealed class InventoryVariable : GenericListVariable<InventoryEntry>
{
    /// <summary>
    /// The object that represents the amount of money the player has
    /// </summary>
    [SerializeField] private InventoryObject moneyObject;

    public event Action<InventoryObject, int> OnItemAdded;
    public event Action<InventoryObject, int> OnItemRemoved;

    #region Getters

    public InventoryObject MoneyObject => moneyObject;

    public int MoneyCount => GetItemCount(MoneyObject);

    #endregion

    protected override void CustomReset()
    {
        // Clear the list
        value.Clear();

        // Create copies
        foreach (var entry in defaultValue)
            value.Add(new InventoryEntry(entry));
        
        // Clear the events
        OnItemAdded = null;
        OnItemRemoved = null;

        // Add the money object to the inventory
        AddItem(MoneyObject, 0);
    }

    private InventoryEntry GetInventoryEntry(InventoryObject inventoryObject)
    {
        return value.FirstOrDefault(inventoryEntry => inventoryEntry.InventoryObject == inventoryObject);
    }

    public int GetItemCount(InventoryObject inventoryObject)
    {
        var inventoryEntry = GetInventoryEntry(inventoryObject);

        return inventoryEntry?.Quantity ?? 0;
    }

    public void AddItem(InventoryObject inventoryObject, int quantity)
    {
        var inventoryEntry = GetInventoryEntry(inventoryObject);

        // Create a new inventory entry and add it to the list
        if (inventoryEntry == null)
        {
            inventoryEntry = new InventoryEntry(inventoryObject, 0);
            value.Add(inventoryEntry);
        }

        inventoryEntry.AddQuantity(quantity);

        // Debug.Log($"Added {quantity} {inventoryObject.Name} to the inventory!");

        // Invoke the event
        if (quantity > 0)
            OnItemAdded?.Invoke(inventoryObject, quantity);
    }

    public void AddItem(InventoryEntry entry)
    {
        AddItem(entry.InventoryObject, entry.Quantity);
    }

    public bool HasItem(InventoryObject inventoryObject, int quantity = 1)
    {
        var inventoryEntry = GetInventoryEntry(inventoryObject);

        return inventoryEntry != null && inventoryEntry.Quantity >= quantity;
    }

    public void RemoveItem(InventoryObject inventoryObject, int quantity = 1)
    {
        var inventoryEntry = GetInventoryEntry(inventoryObject);

        if (inventoryEntry == null)
            return;

        inventoryEntry.RemoveQuantity(quantity);

        // Debug.Log($"Removed {quantity} {inventoryObject.ItemName} from the inventory!");

        // If the quantity is 0, remove the inventory entry from the list
        if (inventoryEntry.Quantity <= 0)
            value.Remove(inventoryEntry);

        // Invoke the event
        OnItemRemoved?.Invoke(inventoryObject, quantity);
    }
}