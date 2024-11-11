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
}