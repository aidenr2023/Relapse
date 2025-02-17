using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(UniqueId), typeof(InteractableMaterialManager))]
public class MemoryPickup : MonoBehaviour, IInteractable, ILevelLoaderInfo
{
    #region Serialized Fields

    [SerializeField] private MemoryScriptableObject memory;

    [SerializeField] private UnityEvent onInteraction;

    #endregion

    #region Private Fields

    private bool _isMarkedForDestruction;

    #endregion

    #region Getters

    public InteractableMaterialManager InteractableMaterialManager { get; set; }

    public GameObject GameObject => gameObject;

    public bool IsInteractable => true;

    public bool HasOutline { get; set; }
    public HashSet<Material> OutlineMaterials { get; } = new();

    public InteractionIcon InteractionIcon => InteractionIcon.Pickup;

    public UnityEvent OnInteraction => onInteraction;

    #endregion

    private void Update()
    {
        if (_isMarkedForDestruction)
        {
            Destroy(gameObject);
            return;
        }
    }

    public void Interact(PlayerInteraction playerInteraction)
    {
        MemoryManager.Instance.AddMemory(memory);

        _isMarkedForDestruction = true;

        // Invoke the onInteraction event
        onInteraction.Invoke();
    }

    private void OnDestroy()
    {
        // Save the data
        var isInteractedData = new DataInfo(IS_INTERACTED_KEY, true);
        LevelLoader.Instance.AddDataToMemory(UniqueId, isInteractedData);
    }

    public void LookAtUpdate(PlayerInteraction playerInteraction)
    {
    }

    public string InteractText(PlayerInteraction playerInteraction)
    {
        return $"Pick Up {memory.MemoryName}";
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
            _isMarkedForDestruction = isInteractedData;

            // Call the OnInteract method if the object has been interacted with
            if (_isMarkedForDestruction)
                Interact(Player.Instance.PlayerInteraction);
        }
    }

    public void SaveData(LevelLoader levelLoader)
    {
        // Save the data
        var isInteractedData = new DataInfo(IS_INTERACTED_KEY, false);
        LevelLoader.Instance.AddDataToMemory(UniqueId, isInteractedData);
    }

    #endregion
}