using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(UniqueId), typeof(InteractableMaterialManager))]
public class InventoryPickup : MonoBehaviour, IInteractable, ILevelLoaderInfo
{
    #region Serialized Fields

    [SerializeField] private InventoryEntry inventoryEntry;

    [SerializeField] private bool destroyOnPickup = true;

    [SerializeField] private UnityEvent onInteract;

    #endregion

    #region Getters

    public InteractableMaterialManager InteractableMaterialManager { get; set; }

    public InventoryEntry InventoryEntry => inventoryEntry;

    public GameObject GameObject => gameObject;


    public bool IsInteractable => true;

    public bool HasOutline { get; set; }

    public HashSet<Material> OutlineMaterials { get; } = new();

    public InteractionIcon InteractionIcon => InteractionIcon.Pickup;
    
    public UnityEvent OnInteraction => onInteract;

    #endregion

    private bool _hasBeenInteracted;

    private bool _isMarkedForDestruction;

    public void Interact(PlayerInteraction playerInteraction)
    {
        // Add the inventory entry to the player's inventory
        playerInteraction.Player.PlayerInventory.AddItem(inventoryEntry);

        // Call the OnPickup method
        OnPickup();
    }

    private void OnPickup()
    {
        // Subtract the quantity from the inventory entry
        inventoryEntry.RemoveQuantity(inventoryEntry.Quantity);

        // Destroy the game object if destroyOnPickup is true
        if (destroyOnPickup)
            _isMarkedForDestruction = true;

        // Invoke the onInteract event
        onInteract?.Invoke();

        // Set the has been interacted flag to true
        _hasBeenInteracted = true;
    }

    public void LookAtUpdate(PlayerInteraction playerInteraction)
    {
    }

    public string InteractText(PlayerInteraction playerInteraction)
    {
        StringBuilder sb = new();

        sb.Append("Pick up ");

        // If the object is money, display the quantity w/ a $ sign
        if (Player.Instance != null && Player.Instance.PlayerInventory.MoneyObject == inventoryEntry.InventoryObject)
        {
            sb.Append($"${inventoryEntry.Quantity}");
            return sb.ToString();
        }

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

    private void OnDestroy()
    {
        // Save the data
        SaveData(LevelLoader.Instance);
    }

    public void SetInventoryEntry(InventoryEntry entry)
    {
        inventoryEntry = entry;
    }

    public void SetQuantity(int quantity)
    {
        inventoryEntry.SetQuantity(quantity);
    }

    #region ILevelLoaderInfo

    private UniqueId _uniqueId;

    public UniqueId UniqueId
    {
        get
        {
            if (_uniqueId == null)
                _uniqueId = GetComponent<UniqueId>();

            return _uniqueId;
        }
    }

    private const string INTERACTED_KEY = "_interacted";

    public void LoadData(LevelLoader levelLoader)
    {
        // Load the data
        if (levelLoader.TryGetDataFromMemory(UniqueId, INTERACTED_KEY, out bool interacted))
        {
            _hasBeenInteracted = interacted;

            // Call the OnPickup method if the object has been interacted with
            if (_hasBeenInteracted)
                OnPickup();
        }
    }

    public void SaveData(LevelLoader levelLoader)
    {
        // If the object has been interacted with, save the data
        var interactedData = new DataInfo(INTERACTED_KEY, _hasBeenInteracted);
        levelLoader.AddDataToMemory(UniqueId, interactedData);
    }

    #endregion
}