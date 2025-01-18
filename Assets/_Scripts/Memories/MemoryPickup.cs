using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MemoryPickup : MonoBehaviour, IInteractable
{
    #region Serialized Fields

    [SerializeField] private MemoryScriptableObject memory;

    [SerializeField] private UnityEvent onInteraction;
        
    #endregion

    #region Private Fields

    private bool _isMarkedForDestruction;

    #endregion

    #region Getters

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

    public void LookAtUpdate(PlayerInteraction playerInteraction)
    {
    }

    public string InteractText(PlayerInteraction playerInteraction)
    {
        return $"Pick Up {memory.MemoryName}";
    }
}