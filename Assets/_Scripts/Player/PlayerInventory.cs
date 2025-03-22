using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInventory : MonoBehaviour, IPlayerLoaderInfo
{
    #region Serialized Fields

    [SerializeField] private InventoryVariable inventoryVariable;
    [SerializeField] private IntVariable moneyCount;

    #endregion
    
    public InventoryVariable InventoryVariable => inventoryVariable;

    private void Start()
    {
        // Subscribe to the OnItemAdded event
        inventoryVariable.OnItemAdded += ItemTooltipOnPickup;
        
        inventoryVariable.OnItemAdded += UpdateMoneyCount;
        inventoryVariable.OnItemRemoved += UpdateMoneyCount;
    }

    private void UpdateMoneyCount(InventoryObject arg1, int _)
    {
        // Return if the money count is null
        if (moneyCount == null)
            return;
        
        moneyCount.value = inventoryVariable.MoneyCount;
    }

    private void ItemTooltipOnPickup(InventoryObject inventoryObject, int quantity)
    {
        // Return if the item is money
        if (inventoryObject == inventoryVariable.MoneyObject)
            return;

        string message;
        if (quantity > 1)
            message = $"Picked up {quantity}x {inventoryObject.ItemName}!";
        else
            message = $"Picked up a {inventoryObject.ItemName}!";

        // Show the tooltip
        JournalTooltipManager.Instance.AddTooltip(message);
    }

    #region Saving and Loading

    public GameObject GameObject => gameObject;
    public string Id => "PlayerInventory";

    public void LoadData(PlayerLoader playerLoader, bool restore)
    {
        // Clear the inventory
        inventoryVariable.value.Clear();

        // For each possible inventory object, check if it exists in the save data
        foreach (var inventoryObject in InventoryObject.InventoryObjects)
        {
            // If the inventory object is not in the save data, skip it
            if (!playerLoader.TryGetDataFromMemory(Id, inventoryObject.UniqueId, out int quantity))
                continue;

            // Add the item to the inventory
            inventoryVariable.AddItem(inventoryObject, quantity);

            Debug.Log($"Reloaded & added {quantity} {inventoryObject.ItemName} to the inventory!");
        }
    }

    public void SaveData(PlayerLoader playerLoader)
    {
        // For each inventory entry, save the data
        foreach (var entry in inventoryVariable.value)
        {
            var itemData = new DataInfo(entry.InventoryObject.UniqueId, entry.Quantity);
            playerLoader.AddDataToMemory(Id, itemData);
        }
    }

    #endregion
}