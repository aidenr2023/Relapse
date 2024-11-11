using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private List<InventoryEntry> inventoryEntries;

    private InventoryEntry GetInventoryEntry(InventoryObject inventoryObject)
    {
        return inventoryEntries.FirstOrDefault(inventoryEntry => inventoryEntry.InventoryObject == inventoryObject);
    }

    public void AddItem(InventoryObject inventoryObject, int quantity = 1)
    {
        var inventoryEntry = GetInventoryEntry(inventoryObject);

        // Create a new inventory entry and add it to the list
        if (inventoryEntry == null)
            inventoryEntry = new InventoryEntry(inventoryObject, 0);

        inventoryEntry.AddQuantity(quantity);

        Debug.Log($"Added {quantity} {inventoryObject.Name} to the inventory!");
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

        Debug.Log($"Removed {quantity} {inventoryObject.Name} from the inventory!");

        // If the quantity is 0, remove the inventory entry from the list
        if (inventoryEntry.Quantity <= 0)
            inventoryEntries.Remove(inventoryEntry);
    }
}