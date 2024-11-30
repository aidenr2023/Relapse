using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointInteractable : MonoBehaviour, IInteractable
{
    #region Serialized Fields

    [SerializeField] private Transform respawnPosition;

    #endregion

    #region Getters

    public Transform RespawnPosition => respawnPosition;

    public bool IsInteractable => true;

    public GameObject GameObject => gameObject;

    public bool HasOutline { get; set; }

    public HashSet<Material> OutlineMaterials { get; } = new();

    public bool HasBeenCollected { get; private set; }

    #endregion

    public void Interact(PlayerInteraction playerInteraction)
    {
        Checkpoint.Instance?.SaveCheckpoint(this);

        // Set the has been collected flag to true
        HasBeenCollected = true;
    }

    public string InteractText(PlayerInteraction playerInteraction)
    {
        return "Save checkpoint";
    }

    public void LookAtUpdate(PlayerInteraction playerInteraction)
    {
    }


    private void OnDrawGizmosSelected()
    {
    }
}