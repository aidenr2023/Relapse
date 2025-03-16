using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(InteractableMaterialManager))]
public class CheckpointInteractable : MonoBehaviour, IInteractable
{
    private const float INTERACT_TOOLTIP_COOLDOWN = 3f;

    #region Serialized Fields

    [SerializeField] private Transform respawnPosition;

    [SerializeField] private UnityEvent onInteraction;

    [SerializeField] private bool isProximity = false;
    [SerializeField, Min(0)] private float proximityRange = 15;

    #endregion

    private bool _isInteractTooltipCooldown;

    #region Getters

    public InteractableMaterialManager InteractableMaterialManager { get; set; }

    public Transform RespawnPosition => respawnPosition;

    public bool IsInteractable => !isProximity;

    public GameObject GameObject => gameObject;

    public bool HasOutline { get; set; }

    public HashSet<Material> OutlineMaterials { get; } = new();

    public bool HasBeenCollected { get; private set; }

    public InteractionIcon InteractionIcon => InteractionIcon.Action;

    public UnityEvent OnInteraction => onInteraction;

    #endregion

    private void Update()
    {
        // Check if the player is nearby 
        if (isProximity)
        {
            // Check if the player is nearby
            if (Player.Instance == null)
                return;

            // Get the distance between the player and the checkpoint
            var distance = Vector3.Distance(Player.Instance.transform.position, transform.position);

            // Check if the player is within the proximity range
            // Force an interaction
            if (distance <= proximityRange && !HasBeenCollected)
                Interact(Player.Instance.PlayerInteraction);
        }
    }

    public void Interact(PlayerInteraction playerInteraction)
    {
        CheckpointManager.Instance?.SaveCheckpoint(this);

        if (HasBeenCollected)
        {
            // Make a tooltip appear
            if (!_isInteractTooltipCooldown)
            {
                JournalTooltipManager.Instance?.AddTooltip("You already saved this checkpoint!");

                // Start the cooldown
                StartCoroutine(InteractTooltipCooldown());
            }

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

        if (PlayerLoader.Instance != null)
        {
            // Save the player data to memory
            PlayerLoader.Instance.SaveDataSceneToMemory();

            // Save the player data to disk
            PlayerLoader.Instance.SaveDataMemoryToDisk();

            Debug.Log("Checkpoint saved the player data to memory and disk.");
        }

        // Invoke the event
        onInteraction.Invoke();
    }

    public string InteractText(PlayerInteraction playerInteraction)
    {
        if (HasBeenCollected)
            return "Checkpoint Saved!";

        return "Save checkpoint";
    }

    public void LookAtUpdate(PlayerInteraction playerInteraction)
    {
    }

    private IEnumerator InteractTooltipCooldown()
    {
        _isInteractTooltipCooldown = true;

        yield return new WaitForSecondsRealtime(INTERACT_TOOLTIP_COOLDOWN);

        _isInteractTooltipCooldown = false;
    }
}