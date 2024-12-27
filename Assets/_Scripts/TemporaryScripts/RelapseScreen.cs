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

    #endregion

    #region Private Fields

    private TokenManager<float>.ManagedToken _pauseToken;

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
        // Set the timescale to 0
        _pauseToken = TimeScaleManager.Instance.TimeScaleTokenManager.AddToken(0, -1, true);

        // Enable the game object
        gameObject.SetActive(true);
    }


    public void LoadScene(string sceneName)
    {
        // Set the timescale to 1
        TimeScaleManager.Instance.TimeScaleTokenManager.RemoveToken(_pauseToken);

        // Load the scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void RespawnAtLatestCheckpoint()
    {
        // Set the timescale to 1
        TimeScaleManager.Instance.TimeScaleTokenManager.RemoveToken(_pauseToken);

        // Disable the game object
        gameObject.SetActive(false);

        // Check if there is a checkpoint manager
        if (CheckpointManager.Instance == null)
        {
            // Load the scene
            LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            return;
        }

        // Respawn at the latest checkpoint
        Player.Instance.PlayerDeathController.Respawn();
    }
}