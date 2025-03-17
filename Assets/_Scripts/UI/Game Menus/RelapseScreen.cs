using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RelapseScreen : GameMenu
{
    private const string DEATH_SCENE_NAME = "DeathUIScene";

    public static RelapseScreen Instance { get; private set; }

    #region Serialized Fields

    [SerializeField] private SceneField mainMenuScene;
    [SerializeField] private SceneField playerDataScene;

    [SerializeField] private Button firstSelectedButton;

    [Header("Background Image"), SerializeField]
    private Image backgroundImage;

    [SerializeField] private Sprite deathImage;
    [SerializeField] private Sprite relapseImage;

    [Space, SerializeField] private Button respawnButton;

    [SerializeField] private Slider loadingBar;

    #endregion

    #region Private Fields

    private bool _respawnButtonClicked;

    #endregion

    #region Getters

    public Sprite DeathImage => deathImage;
    public Sprite RelapseImage => relapseImage;

    #endregion

    protected override void CustomAwake()
    {
        // Set the instance to this
        Instance = this;
    }

    protected override void CustomStart()
    {
    }

    protected override void CustomActivate()
    {
        // Set the event system's current selected game object to the first selected game object
        // EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
        eventSystem.SetSelectedGameObject(firstSelectedButton.gameObject);

        // Reset the flag
        _respawnButtonClicked = false;
    }

    protected override void CustomDeactivate()
    {
    }

    protected override void CustomDestroy()
    {
    }

    protected override void CustomUpdate()
    {
        // Set the loading bar's visibility based on whether the scene is loading
        loadingBar.gameObject.SetActive(_respawnButtonClicked);
    }

    private void OnDisable()
    {
        // Deactivate the menu
        Deactivate();
    }

    private void SetBackgroundImage(Sprite sprite)
    {
        // Set the background image sprite
        backgroundImage.sprite = sprite;
    }

    public void InitializeRelapse()
    {
        // // Set the background image of the Relapse Screen
        // SetBackgroundImage(relapseImage);

        // // Enable the respawn button
        // respawnButton.gameObject.SetActive(true);
    }

    public void InitializeDeath()
    {
        // // Set the background image of the Relapse Screen
        // SetBackgroundImage(deathImage);

        // // Enable the respawn button
        // respawnButton.gameObject.SetActive(true);
    }

    public void LoadScene(string sceneName)
    {
        // Load the scene
        SceneManager.LoadScene(sceneName);
    }

    public void LoadMainMenu()
    {
        // Load the main menu scene
        SceneManager.LoadScene(mainMenuScene);
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
            Deactivate();

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
            // Create scene loader information for just the one scene
            var localLoaderInfo = SceneLoaderInformation.Create(
                new[] { CheckpointManager.Instance.CurrentCheckpointInfo.levelSectionSceneInfo },
                new LevelSectionSceneInfo[] { }
            );

            Debug.Log(
                $"Respawn at checkpoint: {CheckpointManager.Instance.CurrentCheckpointInfo.levelSectionSceneInfo.SectionScene.SceneName}");

            // Load the scene asynchronously
            AsyncSceneManager.Instance.LoadMultipleScenesAsynchronously(
                localLoaderInfo, this, UpdateProgressBarPercent, RespawnOnCompletion
            );
        }
    }

    private void UpdateProgressBarPercent(float amount)
    {
        loadingBar.value = amount;
    }

    private void RespawnOnCompletion()
    {
        // Respawn at the latest checkpoint
        Player.Instance.PlayerDeathController.Respawn();

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
        Deactivate();
    }

    public override void OnBackPressed()
    {
        // Do nothing
    }

    public static IEnumerator LoadDeathScene()
    {
        // Load the death scene
        SceneManager.LoadScene(DEATH_SCENE_NAME, LoadSceneMode.Additive);

        // Wait while the instance is null
        yield return new WaitUntil(() => Instance != null);
    }
}