using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RelapseScreen : GameMenu
{
    public static RelapseScreen Instance { get; private set; }

    #region Serialized Fields

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

    private void Start()
    {
    }

    protected override void CustomActivate()
    {
        // Set the event system's current selected game object to the first selected game object
        EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);

        // Reset the flag
        _respawnButtonClicked = false;
    }

    protected override void CustomDeactivate()
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
        // Set the background image of the Relapse Screen
        SetBackgroundImage(relapseImage);

        // Disable the respawn button
        respawnButton.gameObject.SetActive(false);
    }

    public void InitializeDeath()
    {
        // Set the background image of the Relapse Screen
        SetBackgroundImage(deathImage);

        // // If there is no checkpoint manager, disable the respawn button
        // var buttonActive = !(CheckpointManager.Instance == null);

        // Enable the respawn button
        respawnButton.gameObject.SetActive(true);
    }

    public void LoadScene(string sceneName)
    {
        // Load the scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
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
            LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

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

        // Load the scene asynchronously
        AsyncSceneManager.Instance.LoadMultipleScenesAsynchronously(
            CheckpointManager.Instance.CurrentRespawnPoint.SceneLoaderInformation,
            this, UpdateProgressBarPercent, RespawnOnCompletion
        );
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
}