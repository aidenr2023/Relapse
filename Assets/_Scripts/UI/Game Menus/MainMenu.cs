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

    [SerializeField] private EventVariable gameOnStart;

    [SerializeField] private ResetableSOArrayVariable variablesToReset;

    [SerializeField] private Slider loadingBar;

    [SerializeField] private LevelStartupSceneInfo levelStartupSceneInfo;

    [SerializeField] private CanvasGroup blackOverlayGroup;
    [SerializeField, Min(0)] private float blackOverlayTransitionTime = .5f;

    [SerializeField] private Volume mainMenuVolume;

    [SerializeField] private Button resumeButton;

    [SerializeField] private SceneField openingCutscene;

    #endregion

    #region Private Fields

    private AsyncOperation _loadSceneOperation;

    private bool _startedLoading;

    private bool _clickedButton;

    private bool _showLoadingBar;

    #endregion

    protected override void CustomAwake()
    {
        // Reset the variables
        variablesToReset.Reset();
    }

    protected override void CustomStart()
    {
        // var recentSaveFile = SaveFile.GetMostRecentSaveFile();

        // Check if there are any save files present
        // If there are no save files, disable the resume button
        // If there are save files, enable the resume button
        // resumeButton.gameObject.SetActive(recentSaveFile != null);
        resumeButton.gameObject.SetActive(SceneSaveLoader.Instance.SceneResumeData != null);
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
        // Start the start game coroutine
        if (!_startedLoading)
        {
            // Set the flag to true
            _startedLoading = true;
            // StartCoroutine(StartGameCoroutine());

            // Deactivate the main menu
            Deactivate();

            // Start the cutscene by loading the scene singularly
            SceneManager.LoadScene(openingCutscene.SceneName, LoadSceneMode.Single);
        }

        // Set the flag to true
        _clickedButton = true;
    }

    public void ResumeButton()
    {
        // Reset the variables
        variablesToReset.Reset();

        // Load the scene asynchronously
        // Start the start game coroutine
        if (!_startedLoading)
        {
            // Set the flag to true
            _startedLoading = true;

            StartCoroutine(ResumeGameCoroutine());
        }

        // Set the flag to true
        _clickedButton = true;
    }

    private IEnumerator ResumeGameCoroutine()
    {
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

        // Create a new action to be called when the scene is loaded
        var onCompletion = new Action(() =>
        {
            // Deactivate the main menu
            Deactivate();

            var resumeData = SceneSaveLoader.Instance.SceneResumeData;

            // Move the player to the last checkpoint
            CheckpointManager.Instance.RespawnAt(Player.Instance, resumeData.PlayerPosition, resumeData.PlayerRotation);

            // Also, if there is a Player Loader Instance, load the data from disk
            if (PlayerLoader.Instance != null)
            {
                PlayerLoader.Instance.LoadDataDiskToMemory();
                PlayerLoader.Instance.LoadDataMemoryToScene(true);
            }
        });

        // StartCoroutine(LoadSceneAsync());
        AsyncSceneManager.Instance.LoadResumeScene(
            levelStartupSceneInfo, this, UpdateProgressBarPercent,
            onCompletion
        );

        yield return null;

        // Invoke the game on start event
        gameOnStart.Invoke();
    }

    private IEnumerator StartGameCoroutine()
    {
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

        // Invoke the game on start event
        gameOnStart.Invoke();
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
        // ForceChangeScene(sceneName);

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
        // var pauseMenuManager = PauseMenuManager.Instance;
        //
        // // Throw an exception if the pause menu instance is null
        // if (pauseMenuManager == null)
        //     return;
        //
        // // Activate the settings menu
        // pauseMenuManager.IsolateMenu(pauseMenuManager.SettingsPanel);
        // pauseMenuManager.Activate();
        //
        // // Update the event system
        // UpdateEventSystem();
        // pauseMenuManager.UpdateEventSystem();
        //
        // // Set the event system's selected object to the first button
        // pauseMenuManager.EventSystem.SetSelectedGameObject(pauseMenuManager.SettingsFirstSelected);

        var settingsMenu = NewSettingsMenu.Instance;

        if (settingsMenu == null)
            return;

        // Activate the settings menu
        settingsMenu.Activate();
    }
}