using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(NewGameMenu))]
public class NewPauseMenu : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private SceneField mainMenuScene;

    [Header("Tutorials"), SerializeField] private TutorialMenuButton[] tutorialButtons;
    [SerializeField] private Button generalTutorialHeader;
    [SerializeField] private Button powerTutorialHeader;

    [SerializeField] private TutorialArrayVariable allTutorials;
    [SerializeField] private TutorialArrayVariable completedTutorials;
    [SerializeField] private TutorialArrayVariable generalTutorials;
    [SerializeField] private TutorialArrayVariable powerTutorials;

    [SerializeField] private PowerInfoScreen powerInfoScreen;

    #endregion

    #region Private Fields

    private NewGameMenu _menu;
    
    private PauseMenuManager.TutorialMenuType _tutorialMenuType = PauseMenuManager.TutorialMenuType.General;

    #endregion

    private void Awake()
    {
        //Get the NewGameMenu component
        _menu = GetComponent<NewGameMenu>();
    }

    // Load the main menu scene
    public void LoadMainMenu()
    {
        // Check if the main menu scene is set
        // Load the main menu scene
        if (mainMenuScene != null)
            SceneManager.LoadScene(mainMenuScene.SceneName);
        else
            Debug.LogError("Main menu scene is not set in the inspector.");
    }

    public void SettingsButton()
    {
        // Activate the instance of the settings menu
        if (NewSettingsMenu.Instance != null)
            NewSettingsMenu.Instance.Activate();
    }

    public void PauseButtonPressed()
    {
        // If the new game menu is active, deactivate it
        if (_menu.IsActive && ReferenceEquals(MenuManager.Instance.ActiveMenu, _menu))
            _menu.Deactivate();
        
        else if (MenuManager.Instance.ActiveMenu == null)
            _menu.Activate();
    }
    
    #region Tutorials Screen

    private GameObject PopulateTutorialsPanel()
    {
        // Set the tutorial of the tutorial buttons
        foreach (var button in tutorialButtons)
            button.ResetTutorial();

        // Get the correct tutorial array depending on the tutorial menu type
        var currentTutorials = _tutorialMenuType switch
        {
            PauseMenuManager.TutorialMenuType.General => generalTutorials.value,
            PauseMenuManager.TutorialMenuType.Power => powerTutorials.value,
            _ => throw new ArgumentOutOfRangeException()
        };

        HashSet<Tutorial> addedTutorials = new();
        var tutorialButtonIndex = 0;

        // Go through the player's tutorials and set the tutorial buttons
        for (var i = 0; i < completedTutorials.value.Count; i++)
        {
            var tutorial = completedTutorials.value[i];

            // If the tutorial is not in the current tutorials, continue
            if (!currentTutorials.Contains(tutorial))
                continue;

            // Add this tutorial to the added tutorials
            addedTutorials.Add(tutorial);

            // Set the tutorial of the tutorial button
            tutorialButtons[tutorialButtonIndex].SetTutorial(tutorial, true);

            // Increment the tutorial button index
            tutorialButtonIndex++;
        }

        // Set the tutorial of the remaining tutorial buttons
        for (var i = 0; i < currentTutorials.Count; i++)
        {
            // If the tutorial button index is greater than the tutorial buttons length, break
            if (tutorialButtonIndex >= tutorialButtons.Length)
                break;

            var tutorial = currentTutorials[i];

            // Continue if the tutorial has already been added
            // Add this tutorial to the added tutorials
            if (!addedTutorials.Add(tutorial))
                continue;

            // Determine if the tutorial is available
            var isAvailable = completedTutorials.value.Contains(currentTutorials[i]);

            // Set the tutorial of the tutorial button
            tutorialButtons[tutorialButtonIndex].SetTutorial(tutorial, isAvailable);

            // Increment the tutorial button index
            tutorialButtonIndex++;
        }

        // Depending on the tutorial menu type, set the first selected button
        return _tutorialMenuType switch
        {
            PauseMenuManager.TutorialMenuType.General => generalTutorialHeader.gameObject,
            PauseMenuManager.TutorialMenuType.Power => powerTutorialHeader.gameObject,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void SetTutorialMenuType(PauseMenuManager.TutorialMenuType type)
    {
        _tutorialMenuType = type;
    }

    public void SetTutorialMenuTypeAndPopulate(int type)
    {
        SetTutorialMenuType((PauseMenuManager.TutorialMenuType)type);

        var firstSelected = PopulateTutorialsPanel();
        SetSelectedButton(firstSelected);
    }

    #endregion

    private void SetSelectedButton(GameObject button)
    {
        // eventSystem.SetSelectedGameObject(button);
        StartCoroutine(SetSelectedButtonCoroutine(button));
    }

    private IEnumerator SetSelectedButtonCoroutine(GameObject button)
    {
        yield return null;

        EventSystem.current.SetSelectedGameObject(button);
    }
}