using System;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    #region Serialized Fields

    [SerializeField] private GameObject pauseMenu;

    #endregion

    #region Private Fields

    private bool _isPaused;

    private TokenManager<float>.ManagedToken _pauseToken;

    #endregion

    #region Getters

    public bool IsPaused => _isPaused;

    #endregion

    private void Awake()
    {
        Instance = this;
    }

    public void Pause()
    {
        // Set the pause state
        _isPaused = true;

        // Pause the game
        _pauseToken = TimeScaleManager.Instance.TimeScaleTokenManager.AddToken(0, -1, true);

        // Show the pause menu
        pauseMenu?.SetActive(true);
    }

    public void Resume()
    {
        // Set the pause state
        _isPaused = false;

        // Resume the game
        TimeScaleManager.Instance.TimeScaleTokenManager.RemoveToken(_pauseToken);

        // Hide the cursor & enable the player controls
        // InputManager.Instance.SetCursorState(false);

        // Hide the pause menu
        pauseMenu?.SetActive(false);
    }

    private void TogglePause()
    {
        // If the game is paused, resume it
        if (_isPaused)
            Resume();
        else
            Pause();
    }
}