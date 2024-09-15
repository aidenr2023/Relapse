using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class WinLose : MonoBehaviour
{
    public GameObject loseScreen; // Assign a UI panel or image for the lose screen
    public float restartDelay = 3f; // Time delay before restarting the game

    private void Start()
    {
        // Ensure the lose screen is initially hidden
        if (loseScreen != null)
        {
            loseScreen.SetActive(false);
        }
    }

    public void ShowLoseScreen()
    {
        // Show the lose screen
        if (loseScreen != null)
        {
            loseScreen.SetActive(true);
        }

        // Optional: Freeze the game
        Time.timeScale = 0f;

        // Automatically restart the game after a delay using unscaled time
        StartCoroutine(RestartGameAfterDelay());
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