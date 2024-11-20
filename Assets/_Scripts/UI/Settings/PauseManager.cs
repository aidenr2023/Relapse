using System;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    private bool _isPaused;

    public bool IsPaused => _isPaused;

    public GameObject pauseMenu;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InputManager.Instance.PlayerControls.Player.Pause.performed += _ => TogglePause();
    }

    public void Pause()
    {
        // Set the pause state
        _isPaused = true;

        // Pause the game
        Time.timeScale = 0;

        // Show the cursor & disable the player controls
        // InputManager.Instance.SetCursorState(true);

        // Show the pause menu
        pauseMenu?.SetActive(true);
    }

    public void Resume()
    {
        // Set the pause state
        _isPaused = false;

        // Resume the game
        Time.timeScale = 1;

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