using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class WinLose : MonoBehaviour
{
    // Assign a UI panel or image for the lose screen
    [SerializeField] private GameObject loseScreen;

    // Time delay before restarting the game
    [SerializeField] private float restartDelay = 1f;

    // Reference to the lose text
    private TMP_Text _loseText;

    private float _loseTimeRemaining;

    private bool _isLoseScreenActive;

    private void Awake()
    {
        // Get the lose text component
        _loseText = loseScreen.GetComponentInChildren<TMP_Text>();
    }

    private void Start()
    {
        // Ensure the lose screen is initially hidden
        if (loseScreen != null)
            loseScreen.SetActive(false);
    }

    private void Update()
    {
        var needsToRestart = UpdateLoseTime();

        // Update the lose text
        UpdateLoseText();

        // Reload the current scene to restart the game
        if (needsToRestart)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ShowLoseScreen()
    {
        // Return if the lose screen is already active
        if (_isLoseScreenActive)
            return;

        // Set the lose screen active flag to true
        _isLoseScreenActive = true;

        // Show the lose screen
        if (loseScreen != null)
            loseScreen.SetActive(true);

        // Optional: Freeze the game
        Time.timeScale = 0f;

        // Set the remaining time to the restart delay
        _loseTimeRemaining = restartDelay;

        // // Automatically restart the game after a delay using unscaled time
        // StartCoroutine(RestartGameAfterDelay());
    }

    private bool UpdateLoseTime()
    {
        // Skip if the lose screen is not active
        if (!_isLoseScreenActive)
            return false;
        
        // Decrement the lose time remaining
        // Clamp the lose time remaining
        _loseTimeRemaining = Mathf.Clamp(_loseTimeRemaining - Time.unscaledDeltaTime, 0, restartDelay);

        // Check if the lose time is up
        return _loseTimeRemaining <= 0 && _isLoseScreenActive;
    }

    private void UpdateLoseText()
    {
        // Return if the lose text is null
        if (_loseText == null)
            return;

        _loseText.text = $"YOU LOSE\n" +
                         $"Restarting in {_loseTimeRemaining:0.00}s";
    }

    private IEnumerator RestartGameAfterDelay()
    {
        yield return new WaitForSecondsRealtime(restartDelay); // Wait for 3 real-time seconds

        // Reset the time scale
        Time.timeScale = 1f;

        // Reload the current scene to restart the game
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        // Quit the application
        Application.Quit();
    }
}