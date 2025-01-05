using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Inventory Object", menuName = "Inventory Object")]
public class InventoryObject : ScriptableObject
{
    private static readonly HashSet<InventoryObject> inventoryObjects = new();

    public static IReadOnlyCollection<InventoryObject> InventoryObjects => inventoryObjects;

    [FormerlySerializedAs("name")] [SerializeField]
    private string itemName;

    [SerializeField, UniqueIdentifier] private string uniqueId;

    public string ItemName => itemName;

    public string UniqueId => uniqueId;

    public InventoryObject()
    {
        // Add the inventory object to the hash set
        inventoryObjects.Add(this);
    }
}