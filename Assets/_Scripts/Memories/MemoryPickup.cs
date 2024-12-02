using System;
using System.Collections.Generic;
using UnityEngine;

public class MemoryPickup : MonoBehaviour, IInteractable
{
    #region Serialized Fields

    [SerializeField] private MemoryScriptableObject memory;

    #endregion

    #region Private Fields

    private bool _isMarkedForDestruction;

    #endregion

    #region Getters

    public GameObject GameObject => gameObject;

    public bool IsInteractable => true;

    public bool HasOutline { get; set; }
    public HashSet<Material> OutlineMaterials { get; } = new();

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
    }

    public void LookAtUpdate(PlayerInteraction playerInteraction)
    {
    }

    public string InteractText(PlayerInteraction playerInteraction)
    {
        return $"Pick Up {memory.MemoryName}";
    }
}