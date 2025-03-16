using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : GameMenu
{
    #region Serialized Fields

    [SerializeField] private ResetableSOArrayVariable variablesToReset;
    
    [SerializeField] private Slider loadingBar;

    [SerializeField] private LevelStartupSceneInfo levelStartupSceneInfo;

    [SerializeField] private CanvasGroup blackOverlayGroup;
    [SerializeField, Min(0)] private float blackOverlayTransitionTime = .5f;
    
    [SerializeField] private Volume mainMenuVolume;

    #endregion

    #region Private Fields

    private AsyncOperation _loadSceneOperation;

    private bool _startedLoading;

    private bool _clickedButton;
    
    private bool _showLoadingBar;

    #endregion

    protected override void CustomAwake()
    {
    }

    protected override void CustomStart()
    {
    }

    protected override void CustomActivate()
    {
    }

    protected override void CustomDeactivate()
    {
    }

    protected override void CustomDestroy()
    {
    }

    private void OnDisable()
    {
        // Deactivate the main menu
        Deactivate();
    }

    protected override void CustomUpdate()
    {
        // Set the loading bar's visibility based on whether the scene is loading
        // loadingBar.gameObject.SetActive(_clickedButton);
        loadingBar.gameObject.SetActive(_showLoadingBar);

        // If the opacity is 0, unload the scene
        if (canvasGroup.alpha == 0)
            UnloadSceneAfterDeactivate();
    }

    private void UnloadSceneAfterDeactivate()
    {
        // Get the scene that this object is in
        var scene = gameObject.scene;

        // Unload the scene
        SceneManager.UnloadSceneAsync(scene);
    }

    private void UpdateProgressBarPercent(float amount)
    {
        loadingBar.value = amount;
    }

    public void StartButton()
    {
        // Reset the variables
        variablesToReset.Reset();
        
        // Load the scene asynchronously
        if (!_startedLoading)
        {
            // Start the start game coroutine
            StartCoroutine(StartGameCoroutine());
        }

        // Set the flag to true
        _clickedButton = true;
    }

    private IEnumerator StartGameCoroutine()
    {
        // Set the flag to true
        _startedLoading = true;

        // Fade into the black overlay
        var startTime = Time.unscaledTime;

        while (Time.unscaledTime - startTime < blackOverlayTransitionTime)
        {
            var time = (Time.unscaledTime - startTime) / blackOverlayTransitionTime;
            blackOverlayGroup.alpha = time;

            yield return null;
        }

        blackOverlayGroup.alpha = 1;
        
        // Turn off the post processing volume
        if (mainMenuVolume != null)
            mainMenuVolume.weight = 0;
        
        // Set the show loading bar flag to true
        _showLoadingBar = true;

        // StartCoroutine(LoadSceneAsync());
        AsyncSceneManager.Instance.LoadStartupScene(
            levelStartupSceneInfo, this, UpdateProgressBarPercent,
            Deactivate
        );

        yield return null;
    }

    public void ExitButton()
    {
        Application.Quit();

        // If the player is in the editor, stop playing
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void DeactivateOnLoad()
    {
        // Deactivate the main menu
        Deactivate();
    }

    public override void OnBackPressed()
    {
        // Do nothing for now
        // TODO: Make the submenus recognize the back button
    }

    public void ForceChangeScene(string sceneName)
    {
        // // Load the scene singularly
        // SceneManager.LoadScene(sceneName);

        // Load the scene additively
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);

        // Deactivate the main menu
        Deactivate();

        // Set the loaded scene as the active scene
        StartCoroutine(SetActiveScene(sceneName));
    }

    private static IEnumerator SetActiveScene(string sceneName)
    {
        // Wait 1 frame
        yield return null;

        // Set the active scene
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
    }

    public void ActiveSettingsMenu()
    {
        var pauseMenuManager = PauseMenuManager.Instance;

        // Throw an exception if the pause menu instance is null
        if (pauseMenuManager == null)
            return;

        // Activate the settings menu
        pauseMenuManager.IsolateMenu(pauseMenuManager.SettingsPanel);
        pauseMenuManager.Activate();

        // Update the event system
        UpdateEventSystem();
        pauseMenuManager.UpdateEventSystem();

        // Set the event system's selected object to the first button
        pauseMenuManager.EventSystem.SetSelectedGameObject(pauseMenuManager.SettingsFirstSelected);
    }
}