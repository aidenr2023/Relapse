using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[RequireComponent(typeof(UniqueId), typeof(InteractableMaterialManager))]
public class InventoryObjectChecker : MonoBehaviour, IInteractable, ILevelLoaderInfo
{
    #region Serialized Fields

    [SerializeField] private InventoryEntry[] requiredItems;

    // [SerializeField] private bool requiresInteractionToActivate = true;

    [SerializeField] private bool destroyOnInteract;

    [SerializeField] private bool removeFromInventory;

    [SerializeField] private string interactText;

    [SerializeField] [TextArea(1, 8)] private string failedInteractText;

    [SerializeField] private InteractionIcon interactionIcon = InteractionIcon.Action;

    [SerializeField] private UnityEvent onInventoryObjectFound;

    #endregion

    #region Private Fields

    private bool _hasInteracted;

    private bool _isMarkedForDestruction;

    #endregion

    #region Getters

    public InteractableMaterialManager InteractableMaterialManager { get; set; }

    public InventoryEntry[] RequiredItems => requiredItems;

    public GameObject GameObject => gameObject;

    public bool IsInteractable => true;

    public bool HasOutline { get; set; }

    public HashSet<Material> OutlineMaterials { get; } = new();

    public InteractionIcon InteractionIcon => interactionIcon;

    public UnityEvent OnInteraction => onInventoryObjectFound;

    #endregion

    private void OnInteract()
    {
        // Set the object as interacted with
        _hasInteracted = true;

        // Invoke the event
        onInventoryObjectFound.Invoke();

        // Destroy the game object if destroyOnInteract is true
        if (destroyOnInteract)
            _isMarkedForDestruction = true;
    }

    public void Interact(PlayerInteraction interaction)
    {
        // Return if the object has already been interacted with
        if (_hasInteracted)
            return;

        // Check if the player has all the required inventory objects
        var hasAllItems = true;
        foreach (var requiredItem in requiredItems)
        {
            // Return if the player does not have the required inventory object
            if (interaction.Player.PlayerInventory.InventoryVariable.HasItem(requiredItem.InventoryObject,
                    requiredItem.Quantity))
                continue;

            Debug.Log($"Player does not have {requiredItem.Quantity}x {requiredItem.InventoryObject.ItemName}!");

            hasAllItems = false;
        }

        // Return if the player does not have the required inventory object
        if (!hasAllItems)
        {
            // Create a tooltip if the player does not have the required inventory object
            if (!string.IsNullOrEmpty(failedInteractText))
                JournalTooltipManager.Instance.AddTooltip(failedInteractText);

            return;
        }

        // Remove the inventory objects from the player's inventory
        if (removeFromInventory)
        {
            foreach (var requiredItem in requiredItems)
                interaction.Player.PlayerInventory.InventoryVariable.RemoveItem(requiredItem.InventoryObject,
                    requiredItem.Quantity);
        }

        // Call the OnInteract method
        OnInteract();
    }

    public void LookAtUpdate(PlayerInteraction playerInteraction)
    {
    }

    public string InteractText(PlayerInteraction playerInteraction)
    {
        return interactText;
    }

    private void Update()
    {
        // Destroy the game object if it is marked for destruction
        if (_isMarkedForDestruction)
            Destroy(gameObject);
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

    private const string IS_INTERACTED_KEY = "_isInteracted";

    public void LoadData(LevelLoader levelLoader)
    {
        // Get the is interacted data
        if (levelLoader.TryGetDataFromMemory(UniqueId, IS_INTERACTED_KEY, out bool isInteractedData))
        {
            _hasInteracted = isInteractedData;

            // Call the OnInteract method if the object has been interacted with
            if (_hasInteracted)
                OnInteract();
        }
    }

    public void SaveData(LevelLoader levelLoader)
    {
        // Create the is interacted data
        var isInteractedData = new DataInfo(IS_INTERACTED_KEY, _hasInteracted);
        levelLoader.AddDataToMemory(UniqueId, isInteractedData);
    }

    #endregion
}