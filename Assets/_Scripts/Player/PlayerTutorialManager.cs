using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerTutorialManager : ComponentScript<Player>, IPlayerLoaderInfo
{
    #region Serialized Fields

    [SerializeField, Readonly] private Tutorial[] completedTutorials = Array.Empty<Tutorial>();

    [Header("Tutorials")] [SerializeField] private Tutorial gunPickupTutorial;
    [SerializeField] private Tutorial interactWithCheckpointTutorial;
    [SerializeField] private Tutorial respawnTutorial;

    #endregion

    #region Private Fields

    private readonly HashSet<Tutorial> _completedTutorials = new();

    private bool _hasPickedUpGun;
    private const string HAS_PICKED_UP_GUN_FLAG = "HasPickedUpGun";

    private bool _hasInteractedWithCheckpoint;
    private const string HAS_INTERACTED_WITH_CHECKPOINT_FLAG = "HasInteractedWithCheckpoint";

    private bool _hasRespawned;
    private const string HAS_RESPAWNED_FLAG = "HasRespawned";

    #endregion

    #region Getters

    public GameObject GameObject => gameObject;

    #endregion

    protected void Start()
    {
        // Initialize the tutorial events
        InitializeTutorialEvents();
    }

    #region Tutorial Events

    private void InitializeTutorialEvents()
    {
        ParentComponent.WeaponManager.OnGunEquipped += OnGunEquipped;

        ParentComponent.PlayerDeathController.onRespawn += OnRespawn;
    }

    private void OnRespawn(PlayerDeathController obj)
    {
        // Return if the player has already respawned
        if (_hasRespawned)
            return;

        // Set the has respawned flag to true
        _hasRespawned = true;

        // Complete the tutorial
        TutorialScreen.Instance.PlayTutorial(respawnTutorial);
    }

    private void OnGunEquipped(WeaponManager manager, IGun gun)
    {
        // Return if the gun has already been picked up
        if (_hasPickedUpGun)
            return;

        // Set the has picked up gun flag to true
        _hasPickedUpGun = true;

        // Complete the tutorial
        TutorialScreen.Instance.PlayTutorial(gunPickupTutorial);
    }

    #endregion

    private void Update()
    {
        // Refresh the completed tutorials
        completedTutorials = _completedTutorials.ToArray();
    }

    public void CompleteTutorial(Tutorial tutorial)
    {
        // Add the tutorial to the completed tutorials
        _completedTutorials.Add(tutorial);
    }

    #region Saving and Loading

    public string Id => "PlayerTutorialManager";


    public void LoadData(PlayerLoader playerLoader, bool restore)
    {
        // Clear the completed tutorials
        _completedTutorials.Clear();

        // For each tutorial in the save data, add it to the completed tutorials
        foreach (var tutorial in Tutorial.Tutorials)
        {
            // If the tutorial is not in the save data, skip it
            if (!playerLoader.TryGetDataFromMemory(Id, tutorial.UniqueId, out bool isComplete))
                continue;

            // Add the tutorial to the completed tutorials
            if (isComplete)
                _completedTutorials.Add(tutorial);

            Debug.Log($"Reloaded & added {tutorial.TutorialName} to the completed tutorials!");
        }

        // Load the tutorial flags
        if (playerLoader.TryGetDataFromMemory(Id, HAS_PICKED_UP_GUN_FLAG, out bool hasPickedUpGun))
            _hasPickedUpGun = hasPickedUpGun;

        if (playerLoader.TryGetDataFromMemory(Id, HAS_INTERACTED_WITH_CHECKPOINT_FLAG,
                out bool hasInteractedWithCheckpoint))
            _hasInteractedWithCheckpoint = hasInteractedWithCheckpoint;

        if (playerLoader.TryGetDataFromMemory(Id, HAS_RESPAWNED_FLAG, out bool hasRespawned))
            _hasRespawned = hasRespawned;
    }

    public void SaveData(PlayerLoader playerLoader)
    {
        // For each completed tutorial, save the data
        foreach (var tutorial in _completedTutorials)
        {
            var itemData = new DataInfo(tutorial.UniqueId, _completedTutorials.Contains(tutorial));
            playerLoader.AddDataToMemory(Id, itemData);
        }

        // Save the tutorial flags
        playerLoader.AddDataToMemory(Id, new DataInfo(HAS_PICKED_UP_GUN_FLAG, _hasPickedUpGun));
        playerLoader.AddDataToMemory(Id,
            new DataInfo(HAS_INTERACTED_WITH_CHECKPOINT_FLAG, _hasInteractedWithCheckpoint));
        playerLoader.AddDataToMemory(Id, new DataInfo(HAS_RESPAWNED_FLAG, _hasRespawned));
    }

    #endregion
}