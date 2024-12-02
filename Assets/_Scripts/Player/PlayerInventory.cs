using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    #region Serialized Fields

    /// <summary>
    /// The object that represents the amount of money the player has
    /// </summary>
    [SerializeField] private InventoryObject moneyObject;

    [SerializeField] private List<InventoryEntry> inventoryEntries;

    #endregion

    #region Events

    public event Action<InventoryObject, int> OnItemAdded;

    public event Action<InventoryObject, int> OnItemRemoved;

    #endregion

    #region Getters

    public InventoryObject MoneyObject => moneyObject;

    public int MoneyCount => GetItemCount(moneyObject);

    public IReadOnlyCollection<InventoryEntry> InventoryEntries => inventoryEntries;

    #endregion

    private void Start()
    {
        // Subscribe to the OnItemAdded event
        OnItemAdded += MoneyTooltipOnPickup;

        // Subscribe to the OnItemAdded event
        OnItemAdded += ItemTooltipOnPickup;

        // Force the money object to be in the inventory
        ForceItem(moneyObject, 0);
    }

    private InventoryEntry GetInventoryEntry(InventoryObject inventoryObject)
    {
        return inventoryEntries.FirstOrDefault(inventoryEntry => inventoryEntry.InventoryObject == inventoryObject);
    }

    public int GetItemCount(InventoryObject inventoryObject)
    {
        var inventoryEntry = GetInventoryEntry(inventoryObject);

        return inventoryEntry?.Quantity ?? 0;
    }

    public void AddItem(InventoryObject inventoryObject, int quantity = 1)
    {
        var inventoryEntry = GetInventoryEntry(inventoryObject);

        // Create a new inventory entry and add it to the list
        if (inventoryEntry == null)
        {
            inventoryEntry = new InventoryEntry(inventoryObject, 0);
            inventoryEntries.Add(inventoryEntry);
        }

        inventoryEntry.AddQuantity(quantity);

        // Debug.Log($"Added {quantity} {inventoryObject.Name} to the inventory!");

        // Invoke the event
        OnItemAdded?.Invoke(inventoryObject, quantity);
    }

    public void AddItem(InventoryEntry entry)
    {
        AddItem(entry.InventoryObject, entry.Quantity);
    }

    private void ForceItem(InventoryObject obj, int quantity)
    {
        var entry = GetInventoryEntry(obj);

        if (entry == null)
        {
            entry = new InventoryEntry(obj, quantity);
            inventoryEntries.Add(entry);
        }
        else
            entry.AddQuantity(quantity);
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

        Debug.Log($"Removed {quantity} {inventoryObject.Name} from the inventory!");

        // If the quantity is 0, remove the inventory entry from the list
        if (inventoryEntry.Quantity <= 0)
            inventoryEntries.Remove(inventoryEntry);

        // Invoke the event
        OnItemRemoved?.Invoke(inventoryObject, quantity);
    }

    private void ItemTooltipOnPickup(InventoryObject inventoryObject, int quantity)
    {
        // Return if the item is money
        if (inventoryObject == moneyObject)
            return;

        string message;
        if (quantity > 1)
            message = $"Picked up {quantity}x {inventoryObject.Name}!";
        else
            message = $"Picked up a {inventoryObject.Name}!";

        // Show the tooltip
        JournalTooltipManager.Instance.AddTooltip(message);
    }

    private void MoneyTooltipOnPickup(InventoryObject inventoryObject, int quantity)
    {
        if (inventoryObject != moneyObject)
            return;

        // Show the tooltip
        JournalTooltipManager.Instance.AddTooltip(
            $"+${quantity}!\nTotal: {GetItemCount(moneyObject)}");
    }
}