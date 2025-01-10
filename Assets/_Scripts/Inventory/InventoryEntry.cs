using System;
using UnityEngine;

[Serializable]
public class InventoryEntry
{
    [SerializeField] private InventoryObject inventoryObject;
    [SerializeField] [Min(1)] private int quantity = 1;

    public InventoryObject InventoryObject => inventoryObject;
    public int Quantity => quantity;

    public event Action<InventoryEntry, int> OnQuantityChanged;
    public event Action<InventoryEntry, int> OnEmpty;

    public InventoryEntry()
    {
    }

    public InventoryEntry(InventoryObject inventoryObject, int quantity = 1)
    {
        this.inventoryObject = inventoryObject;
        this.quantity = quantity;
    }

    public void AddQuantity(int value)
    {
        quantity += value;

        OnQuantityChanged?.Invoke(this, value);
    }

    public void RemoveQuantity(int value)
    {
        quantity -= value;

        OnQuantityChanged?.Invoke(this, value);

        if (quantity <= 0)
            OnEmpty?.Invoke(this, value);
    }

    public void SetQuantity(int value)
    {
        // If the value is the same as the current quantity, return
        if (value == quantity)
            return;

        // If the value is less 0, set the value to 0
        if (value < 0)
            value = 0;

        if (value < quantity)
            RemoveQuantity(quantity - value);
        else
            AddQuantity(value - quantity);
    }
}