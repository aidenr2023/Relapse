using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInventory : MonoBehaviour, IPlayerLoaderInfo
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
        // // Subscribe to the OnItemAdded event
        // OnItemAdded += MoneyTooltipOnPickup;

        // Subscribe to the OnItemAdded event
        OnItemAdded += ItemTooltipOnPickup;

        // Force the money object to be in the inventory
        ForceItem(moneyObject, 0);

        // Get all instance of the inventory objects
        foreach (var obj in InventoryObject.InventoryObjects)
            Debug.Log($"Found {obj.ItemName} in the scene!");
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

        // Debug.Log($"Removed {quantity} {inventoryObject.ItemName} from the inventory!");

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
            message = $"Picked up {quantity}x {inventoryObject.ItemName}!";
        else
            message = $"Picked up a {inventoryObject.ItemName}!";

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

    public void SetUpInventory(InventoryEntry[] entries)
    {
        // Clear the inventory
        inventoryEntries.Clear();

        // Add the entries (without calling the add item method)
        foreach (var entry in entries)
        {
            var inventoryEntry = GetInventoryEntry(entry.InventoryObject);

            // Create a new inventory entry and add it to the list
            if (inventoryEntry == null)
                inventoryEntries.Add(entry);
            else
                inventoryEntry.AddQuantity(entry.Quantity);
        }
    }

    #region Saving and Loading

    public GameObject GameObject => gameObject;
    public string Id => "PlayerInventory";

    public void LoadData(PlayerLoader playerLoader, bool restore)
    {
        // Clear the inventory
        inventoryEntries.Clear();

        // For each possible inventory object, check if it exists in the save data
        foreach (var inventoryObject in InventoryObject.InventoryObjects)
        {
            // If the inventory object is not in the save data, skip it
            if (!playerLoader.TryGetDataFromMemory(Id, inventoryObject.UniqueId, out int quantity))
                continue;

            // Add the item to the inventory
            AddItem(inventoryObject, quantity);

            Debug.Log($"Reloaded & added {quantity} {inventoryObject.ItemName} to the inventory!");
        }
    }

    public void SaveData(PlayerLoader playerLoader)
    {
        // For each inventory entry, save the data
        foreach (var entry in inventoryEntries)
        {
            var itemData = new DataInfo(entry.InventoryObject.UniqueId, entry.Quantity);
            playerLoader.AddDataToMemory(Id, itemData);
        }
    }

    #endregion
}