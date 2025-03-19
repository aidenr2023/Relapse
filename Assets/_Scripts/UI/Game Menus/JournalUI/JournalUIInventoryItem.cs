using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JournalUIInventoryItem : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private InventoryEntry inventoryEntry;

    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private EventTrigger eventTrigger;

    [SerializeField] private Button button;

    #endregion

    #region Getters

    public InventoryEntry InventoryEntry => inventoryEntry;

    public EventTrigger EventTrigger => eventTrigger;

    #endregion

    public Button Button => button;

    private void Update()
    {
        if (inventoryEntry == null)
            throw new Exception("Inventory entry not set");

        // Update the inventory item data
        UpdateInventoryItemData();
    }

    private void UpdateInventoryItemData()
    {
        var countText = string.Empty;

        if (inventoryEntry.Quantity > 1)
            countText = $" x{inventoryEntry.Quantity}";

        itemNameText.text = $"{inventoryEntry.InventoryObject.ItemName}{countText}";
    }


    public void SetInventoryEntry(InventoryEntry entry)
    {
        inventoryEntry = entry;
    }
}