using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class WinLose : MonoBehaviour
{
    private const string DEFAULT_LOSE_MESSAGE = "You Lose!";

    public static WinLose Instance { get; private set; }

    // Assign a UI panel or image for the lose screen
    [SerializeField] private GameObject loseScreen;

    // Time delay before restarting the game
    [SerializeField] private float restartDelay = 1f;

    // Reference to the lose text
    private TMP_Text _loseText;

    private float _loseTimeRemaining;

    private bool _isLoseScreenActive;

    private string _loseMessage = DEFAULT_LOSE_MESSAGE;

    private void Awake()
    {
        // Set this to the active instance of the WinLose script
        Instance = this;

        // Initialize the components
        InitializeComponents();
    }

    private void Start()
    {
        // Ensure the lose screen is initially hidden
        if (loseScreen != null)
            loseScreen.SetActive(false);
    }

    private void InitializeComponents()
    {
        // Get the lose text component
        _loseText = loseScreen.GetComponentInChildren<TMP_Text>();
    }

    private void Update()
    {
        var needsToRestart = UpdateLoseTime();

        // Update the lose text
        UpdateLoseText();

        // Reload the current scene to restart the game
        if (needsToRestart)
            Restart();
    }

    public void Lose(string message = DEFAULT_LOSE_MESSAGE)
    {
        // Return if the lose screen is already active
        if (_isLoseScreenActive)
            return;

        // Set the lose message
        _loseMessage = message;
        
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

        // Update the lose text
        var newText = "";
        if (_loseMessage != string.Empty)
            newText += $"{_loseMessage}\n";

        newText += $"Restarting in {_loseTimeRemaining:0.00}s";

        _loseText.text = newText;
    }

    private void Restart()
    {
        // Reset the time scale
        Time.timeScale = 1f;

        // Reload the current scene to restart the game
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator RestartGameAfterDelay()
    {
        yield return new WaitForSecondsRealtime(restartDelay); // Wait for 3 real-time seconds

        // Restart the game
        Restart();
    }

    public void QuitGame()
    {
        // Quit the application
        Application.Quit();
    }
}