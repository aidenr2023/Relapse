using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewDeathMenu : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private EventVariable gameOnStart;
    [SerializeField] private HealthChangedEventVariable playerOnDeathEvent;

    [SerializeField] private SceneField mainMenuScene;
    [SerializeField] private SceneField playerDataScene;

    [SerializeField] private Button firstSelectedButton;

    [Header("Background Image"), SerializeField]
    private Image backgroundImage;

    [Space, SerializeField] private Button respawnButton;

    [SerializeField] private Slider loadingBar;

    [SerializeField] private UnityEvent onDeathEvent;
    [SerializeField] private UnityEvent deactivateFunction;

    #endregion

    #region Private Fields

    private bool _respawnButtonClicked;
    private CheckpointManager.CheckpointInformation _currentCheckpoint;

    #endregion

    private void Awake()
    {
        // Subscribe to the player's death event
        playerOnDeathEvent += InvokeEventOnDeath;

        // Subscribe to the game on start event
        gameOnStart += ForceEventSubscription;
    }

    private void ForceEventSubscription()
    {
        // Remove the event subscription (just in case)
        playerOnDeathEvent -= InvokeEventOnDeath;

        // Subscribe to the player's death event
        playerOnDeathEvent += InvokeEventOnDeath;
    }

    private void InvokeEventOnDeath(object sender, HealthChangedEventArgs arg1)
    {
        onDeathEvent?.Invoke();
    }

    public void LoadMainMenu()
    {
        // Load the main menu scene
        SceneManager.LoadScene(mainMenuScene);
    }

    public void LoadScene(string sceneName)
    {
        // Load the scene
        SceneManager.LoadScene(sceneName);
    }
    
    public void RespawnAtLatestCheckpoint()
    {
        // Check if there is a checkpoint manager
        if (CheckpointManager.Instance == null)
        {
            // If there is a level loader instance, load the data from disk
            if (LevelLoader.Instance != null)
                LevelLoader.Instance.LoadDataDiskToMemory();

            // Also, if there is a Player Loader Instance, load the data from disk
            if (PlayerLoader.Instance != null)
                PlayerLoader.Instance.LoadDataDiskToMemory();

            // Load the scene
            LoadScene(SceneManager.GetActiveScene().name);

            // Load the data from the memory to the scene
            LevelLoader.Instance.LoadDataMemoryToScene(null);

            // Also, load the player data from memory to the scene
            PlayerLoader.Instance.LoadDataMemoryToScene();

            // // Disable the game object
            // gameObject.SetActive(false);
            deactivateFunction.Invoke();

            return;
        }

        // Return if the button was already clicked
        if (_respawnButtonClicked)
            return;

        // If there is a level loader instance, load the data from disk
        if (LevelLoader.Instance != null)
            LevelLoader.Instance.LoadDataDiskToMemory();

        // Set the flag to true
        _respawnButtonClicked = true;

        // Get the currently managed scenes from the AsyncSceneManager
        var managedScenes = AsyncSceneManager.Instance.GetManagedScenes();

        // Get the scene the player is in
        var playerScene = playerDataScene?.SceneName ?? "";

        // Create a level section scene info array with all the managed scenes EXCEPT the player's scene
        var scenesToUnload = new List<string>();
        foreach (var scene in managedScenes)
        {
            // Continue if the scene is the player's scene
            if (scene == playerScene)
                continue;

            scenesToUnload.Add(scene);
            Debug.Log($"Unload: {scene}");
        }

        // Convert the scenes to unload to a LevelSectionSceneInfo array
        var scenesToUnloadInfo = scenesToUnload.Select(scene => LevelSectionSceneInfo.Create(null, scene)).ToArray();

        // Create a scene loader information object with the scenes to unload
        var loaderInfo = SceneLoaderInformation.Create(
            new LevelSectionSceneInfo[] { }, scenesToUnloadInfo
        );

        // Unload all the scenes to unload
        AsyncSceneManager.Instance.LoadMultipleScenesAsynchronously(
            loaderInfo, this, UpdateProgressBarPercent, RespawnSubFunction
        );

        return;

        void RespawnSubFunction()
        {
            _currentCheckpoint = CheckpointManager.Instance.CurrentCheckpointInfo;

            // Create scene loader information for just the one scene
            var localLoaderInfo = SceneLoaderInformation.Create(
                new[] { _currentCheckpoint.levelSectionSceneInfo },
                new LevelSectionSceneInfo[] { }
            );

            // Load the scene asynchronously
            AsyncSceneManager.Instance.LoadMultipleScenesAsynchronously(
                localLoaderInfo, this, UpdateProgressBarPercent, RespawnOnCompletion
            );
        }
    }

    #region Load Functions

    private void UpdateProgressBarPercent(float amount)
    {
        loadingBar.value = amount;
    }

    private void RespawnOnCompletion()
    {
        // Respawn at the latest checkpoint
        Player.Instance.PlayerDeathController.Respawn(_currentCheckpoint.position);

        // Also, if there is a Player Loader Instance, load the data from disk
        if (PlayerLoader.Instance != null)
        {
            PlayerLoader.Instance.LoadDataDiskToMemory();
            PlayerLoader.Instance.LoadDataMemoryToScene(true);
        }

        // Set the flag to false
        _respawnButtonClicked = false;

        // // Set the game object to inactive
        // gameObject.SetActive(false);
        deactivateFunction.Invoke();
    }

    #endregion
}