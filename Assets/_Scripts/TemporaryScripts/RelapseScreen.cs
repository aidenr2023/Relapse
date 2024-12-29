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
    public Sprite RelapseImage => RelapseImage;

    #endregion

    protected override void CustomAwake()
    {
        // Set the instance to this
        Instance = this;
    }

    private void Start()
    {
        // Disable the game object
        gameObject.SetActive(false);
    }

    protected override void CustomOnEnable()
    {
        // Set the event system's current selected game object to the first selected game object
        EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
    }

    protected override void CustomOnDisable()
    {
    }

    private void Update()
    {
        // Set the loading bar's visibility based on whether the scene is loading
        loadingBar.gameObject.SetActive(_respawnButtonClicked);
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

    public void Activate()
    {
        // Enable the game object
        gameObject.SetActive(true);

        // Reset the flag
        _respawnButtonClicked = false;
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
            // Load the scene
            LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

            // Disable the game object
            gameObject.SetActive(false);

            return;
        }

        // Return if the button was already clicked
        if (_respawnButtonClicked)
            return;

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

        // Set the flag to false
        _respawnButtonClicked = false;

        // Set the game object to inactive
        gameObject.SetActive(false);
    }
}