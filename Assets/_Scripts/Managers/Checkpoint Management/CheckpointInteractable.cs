using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[ExecuteAlways]
[RequireComponent(typeof(InteractableMaterialManager))]
public class CheckpointInteractable : MonoBehaviour, IInteractable
{
    private const float INTERACT_TOOLTIP_COOLDOWN = 3f;

    #region Serialized Fields

    [SerializeField] private Transform respawnPosition;

    [SerializeField] private bool isProximity = false;
    [SerializeField, Min(0)] private float proximityRange = 15;

    [SerializeField] private Sound checkpointCollectedSound;

    [SerializeField] private UnityEvent onInteraction;

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

#if UNITY_EDITOR
    private static HashSet<GameObject> duplicateHolder = new HashSet<GameObject>();

#endif

    private void Awake()
    {
#if UNITY_EDITOR
        if (duplicateHolder.Add(gameObject))
        {
            // Get the number of Checkpoint interactable scripts on the object
            var checkpointInteractableCount = GetComponents<CheckpointInteractable>().Length;

            // Assert that the respawn position is not null
            Debug.Assert(respawnPosition != null, "DESIGNER ISSUE: Respawn position is null!", this);
            Debug.Assert(
                checkpointInteractableCount == 1,
                "DESIGNER ISSUE: Multiple CheckpointInteractable scripts on this object! Remove the one that ISN'T part of the prefab",
                gameObject
            );
        }

#endif
    }

    private void OnEnable()
    {
        return;

        // TODO: If I uncomment this,
        // the invisible checkpoints will be added to the pickup icon manager

        if (!isProximity)
            PickupIconManager.Add(this);
    }

    private void OnDisable()
    {
        return;

        PickupIconManager.Remove(this);
    }

    private void Update()
    {
        // Check if the player is nearby 
        if (!isProximity)
            return;

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

    public void Interact(PlayerInteraction _)
    {
        var result = CheckpointManager.Instance?.SaveCheckpoint(this)
                     ?? Result<int>.Error("Checkpoint manager instance is null!").ReadError(Debug.LogError);

        // If the result was a failure, return
        if (result.IsFailure)
            return;

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

        // Play the sound
        if (checkpointCollectedSound != null)
            SoundManager.Instance.PlaySfx(checkpointCollectedSound);

        // Set the has been collected flag to true
        HasBeenCollected = true;

        // Get the current checkpoint information from the checkpoint manager
        var currentCheckpointInfo = CheckpointManager.Instance.CurrentCheckpointInfo;
        
        // Save the information
        SaveInformation(currentCheckpointInfo.position, currentCheckpointInfo.rotation);

        // Invoke the event
        onInteraction.Invoke();
    }

    public static void SaveInformation(Vector3 position, Quaternion rotation)
    {
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

        // Save the scene data to memory
        if (SceneSaveLoader.Instance != null)
        {
            SceneSaveLoader.Instance.SaveSettingsToDisk(position, rotation);

            Debug.Log("Checkpoint saved the scene data to memory and disk.");
        }
    }

    public string InteractText(PlayerInteraction _)
    {
        if (HasBeenCollected)
            return "Checkpoint Saved!";

        return "Save checkpoint";
    }

    public void LookAtUpdate(PlayerInteraction _)
    {
    }

    private IEnumerator InteractTooltipCooldown()
    {
        _isInteractTooltipCooldown = true;

        yield return new WaitForSecondsRealtime(INTERACT_TOOLTIP_COOLDOWN);

        _isInteractTooltipCooldown = false;
    }
}