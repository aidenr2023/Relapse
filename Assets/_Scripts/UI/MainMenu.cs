using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : GameMenu
{
    #region Serialized Fields

    [SerializeField] private Slider loadingBar;

    [SerializeField] private LevelStartupSceneInfo levelStartupSceneInfo;

    #endregion

    #region Private Fields

    private AsyncOperation _loadSceneOperation;

    private bool _startedLoading;

    private bool _clickedButton;

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
        loadingBar.gameObject.SetActive(_clickedButton);
        
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
        // Load the scene asynchronously
        if (!_startedLoading)
        {
            // StartCoroutine(LoadSceneAsync());
            AsyncSceneManager.Instance.LoadStartupScene(
                levelStartupSceneInfo, this, UpdateProgressBarPercent,
                Deactivate
            );

            // Set the flag to true
            _startedLoading = true;
        }

        // Set the flag to true
        _clickedButton = true;
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