using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private string _sceneName = "ApartmentBlockout";

    private AsyncOperation _loadSceneOperation;

    private void Awake()
    {
        Debug.Log($"Bruh who am I?: {gameObject}");
    }

    private void Start()
    {
        // Asynchronously load the scene
        _loadSceneOperation = SceneManager.LoadSceneAsync(_sceneName, LoadSceneMode.Single);
        _loadSceneOperation.allowSceneActivation = false;
    }

    public void StartButton()
    {
        // Set the scene to active
        _loadSceneOperation.allowSceneActivation = true;
    }
    public void ExitButton()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}
