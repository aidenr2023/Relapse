using System;
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
                levelStartupSceneInfo, this, UpdateProgressBarPercent
            );

            // Set the flag to true
            _startedLoading = true;
        }

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
        // Load the scene singularly
        SceneManager.LoadScene(sceneName);
    }
}