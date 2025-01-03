using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointInteractable : MonoBehaviour, IInteractable
{
    #region Serialized Fields

    [SerializeField] private Transform respawnPosition;

    [SerializeField] private SceneLoaderInformation sceneLoaderInformation;

    #endregion

    #region Getters

    public Transform RespawnPosition => respawnPosition;

    public bool IsInteractable => true;

    public GameObject GameObject => gameObject;

    public bool HasOutline { get; set; }

    public HashSet<Material> OutlineMaterials { get; } = new();

    public bool HasBeenCollected { get; private set; }
    public SceneLoaderInformation SceneLoaderInformation => sceneLoaderInformation;

    public InteractionIcon InteractionIcon => InteractionIcon.Action;

    #endregion

    public void Interact(PlayerInteraction playerInteraction)
    {
        CheckpointManager.Instance?.SaveCheckpoint(this);

        if (HasBeenCollected)
        {
            // Make a tooltip appear
            JournalTooltipManager.Instance?.AddTooltip("You already saved this checkpoint!");
            return;
        }

        // Make a tooltip appear
        JournalTooltipManager.Instance?.AddTooltip("Checkpoint saved!");

        // Set the has been collected flag to true
        HasBeenCollected = true;

        // Check if there is an instance of the level loader
        if (LevelLoader.Instance != null)
        {
            // Save all the scenes to memory
            LevelLoader.Instance.SaveDataSceneToMemory(null);

            // Save the data to the disk
            LevelLoader.Instance.SaveDataMemoryToDisk();

            Debug.Log("Checkpoint saved all the scenes to memory and disk.");
        }
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