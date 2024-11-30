using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Slider loadingBar;

    private string _sceneName = "ApartmentBlockout";

    private AsyncOperation _loadSceneOperation;

    private bool _startedLoading;

    private bool _clickedButton;

    private void Awake()
    {
    }

    private void Start()
    {
    }

    private void Update()
    {
        // // Update the loading slider
        // UpdateLoadingSlider();
    }

    public void StartButton()
    {
        // // Load the scene asynchronously
        // StartCoroutine(LoadSceneAsync());

        // Load the scene asynchronously
        if (!_startedLoading)
            StartCoroutine(LoadSceneAsync());

        // Set the scene to active
        _loadSceneOperation.allowSceneActivation = true;

        // Set the flag to true
        _clickedButton = true;
    }

    public void ExitButton()
    {
        Debug.Log("Quit");
        Application.Quit();

        // If the player is in the editor, stop playing
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private IEnumerator LoadSceneAsync()
    {
        // Return if the scene is already loading
        if (_startedLoading)
            yield return null;

        // Asynchronously load the scene
        _loadSceneOperation = SceneManager.LoadSceneAsync(_sceneName, LoadSceneMode.Single);
        _loadSceneOperation.allowSceneActivation = false;

        while (!_loadSceneOperation.isDone)
        {
            // Update the loading bar value
            loadingBar.value = _loadSceneOperation.progress;

            // Wait for the next frame
            yield return null;
        }

        // Update the loading bar value
        loadingBar.value = _loadSceneOperation.progress;

        // Set the flag to true
        _startedLoading = true;

        // If the button was clicked, allow the scene to activate
        if (_clickedButton)
            _loadSceneOperation.allowSceneActivation = true;
    }
}