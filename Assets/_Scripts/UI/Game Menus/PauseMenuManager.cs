using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PauseMenuManager : GameMenu
{
    private const string PAUSE_SCENE_NAME = "PauseUIScene";

    public static PauseMenuManager Instance { get; private set; }

    #region Serialized Fields

    [SerializeField] private string mainMenuScene = "MainMenuRevamp";

    [SerializeField] private GameObject pauseMenuParent;

    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject journalPanel;
    [SerializeField] private GameObject powersMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject tutorialsPanel;

    // Colors for normal, hover, and click states
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color clickColor = Color.red;

    [Header("Navigation Control"), SerializeField]
    private GameObject firstSelectedButton;

    [SerializeField] private GameObject settingsFirstSelected;
    [SerializeField] private GameObject journalFirstSelected;

    [Header("Tutorials"), SerializeField] private TutorialMenuButton[] tutorialButtons;
    [SerializeField] private TutorialButton tutorialButtonPrefab;
    [SerializeField] private GameObject tutorialBack;
    [SerializeField] private Button generalTutorialHeader;
    [SerializeField] private Button powerTutorialHeader;

    [SerializeField] private TutorialArrayVariable allTutorials;
    [SerializeField] private TutorialArrayVariable completedTutorials;
    [SerializeField] private TutorialArrayVariable generalTutorials;
    [SerializeField] private TutorialArrayVariable powerTutorials;

    [SerializeField] private PowerInfoScreen powerInfoScreen;

    #endregion

    #region Private Fields

    private readonly Stack<GameObject> _menuStack = new();

    private bool _inputtedThisFrame;

    private TutorialMenuType _tutorialMenuType = TutorialMenuType.General;

    #endregion

    #region Getters

    public HashSet<InputData> InputActions { get; } = new();

    public bool IsPaused { get; private set; }

    public GameObject SettingsPanel => settingsPanel;

    public GameObject SettingsFirstSelected => settingsFirstSelected;

    #endregion

    /// <summary>
    /// Changes the text color of the TextMeshPro to the hover color.
    /// Attach this to the PointerEnter event.
    /// </summary>
    protected override void CustomAwake()
    {
        // Set the instance
        Instance = this;
    }

    protected override void CustomStart()
    {
    }

    protected override void CustomActivate()
    {
        // Set the event system's selected object to the first button
        SetSelectedButton(firstSelectedButton);
    }

    protected override void CustomDeactivate()
    {
    }

    protected override void CustomDestroy()
    {
        // // Unregister the input user
        // InputManager.Instance.Unregister(this);

        // Deactivate the menu
        Deactivate();

        // Unset this as the instance
        Instance = null;
    }

    public void OnPausePerformed(InputAction.CallbackContext context)
    {
        // Return if inputted this frame
        if (_inputtedThisFrame)
            return;

        _inputtedThisFrame = true;

        TogglePause();
    }


    private void TogglePause()
    {
        // If there are any other active menus, don't toggle the pause menu
        if (MenuManager.Instance.ActiveMenus.Any(n => n != this))
            return;

        if (!IsPaused)
            Pause();

        // Resume the game
        else
            Resume(pauseMenuPanel.transform.GetChild(0).gameObject);
    }


    public void OnPointerEnter(GameObject textObject)
    {
        return;

        var tmpText = textObject.GetComponent<TMP_Text>();

        if (tmpText != null)
            tmpText.color = hoverColor;
    }

    /// <summary>
    /// Changes the text color of the TextMeshPro back to the normal color.
    /// Attach this to the PointerExit event.
    /// </summary>
    public void OnPointerExit(GameObject textObject)
    {
        return;

        var tmpText = textObject.GetComponent<TMP_Text>();

        if (tmpText != null)
            tmpText.color = normalColor;
    }

    protected override void CustomUpdate()
    {
        // Reset the inputted this frame flag
        _inputtedThisFrame = false;

        if (eventSystem.currentSelectedGameObject == null)
            SetSelectedButton(firstSelectedButton);
    }

    /// <summary>
    /// Changes the color of the TextMeshPro text to the click color.
    /// </summary>
    /// <param name="textObject">The GameObject associated with the button.</param>
    private void ChangeClickColor(GameObject textObject)
    {
        return;

        var tmpText = textObject.GetComponent<TMP_Text>();

        if (tmpText != null)
            tmpText.color = clickColor;
    }

    public void IsolateMenu(GameObject obj)
    {
        // Hide all the menus
        pauseMenuPanel.SetActive(false);
        powersMenuPanel.SetActive(false);
        journalPanel.SetActive(false);
        settingsPanel.SetActive(false);
        tutorialsPanel.SetActive(false);

        // Show the selected menu
        obj?.SetActive(true);

        if (obj == null)
            return;

        // Get all the tmp_text objects in the selected menu
        var tmpTexts = obj.GetComponentsInChildren<TMP_Text>();

        // Reset the color of all the text objects in the selected menu
        foreach (var tmpText in tmpTexts)
            tmpText.color = normalColor;
    }

    /// <summary>
    /// Called when the Resume button is clicked.
    /// </summary>
    public void Resume(GameObject textObject)
    {
        ChangeClickColor(textObject);

        // Hide menu
        // pauseMenuParent.SetActive(false);
        Deactivate();

        // Set pause to false
        IsPaused = false;

        // Clear the menu stack
        _menuStack.Clear();
    }

    public void Pause()
    {
        // Un-hide the pause menu
        // pauseMenuParent.SetActive(true);
        Activate();

        // Set pause to true
        IsPaused = true;

        // Clear the menu stack
        _menuStack.Clear();

        // Isolate the pause menu
        _menuStack.Push(pauseMenuPanel);

        // Isolate the pause menu
        IsolateMenu(_menuStack.Peek());
    }

    public void PowersMenu(GameObject textObject)
    {
        // Push the journal panel onto the stack
        _menuStack.Push(powersMenuPanel);

        // Isolate the journal panel
        IsolateMenu(_menuStack.Peek());

        // Set the event system's selected object to the first button
        SetSelectedButton(powerInfoScreen.FirstSelectedButton);
    }

    /// <summary>
    /// Called when the Journal button is clicked.
    /// </summary>
    public void Journal(GameObject textObject)
    {
        ChangeClickColor(textObject);

        // Push the journal panel onto the stack
        _menuStack.Push(journalPanel);

        // Isolate the journal panel
        IsolateMenu(_menuStack.Peek());

        // Set the event system's selected object to the first button
        SetSelectedButton(journalFirstSelected);
    }


    /// <summary>
    /// Called when the Settings button is clicked.
    /// </summary>
    public void Settings(GameObject textObject)
    {
        ChangeClickColor(textObject);

        // // Push the settings panel onto the stack
        // _menuStack.Push(settingsPanel);
        //
        // // Isolate the settings panel
        // IsolateMenu(_menuStack.Peek());
        //
        // // Set the event system's selected object to the first button
        // SetSelectedButton(settingsFirstSelected);

        // Activate the instance of the settings menu
        if (SettingsMenu.Instance != null)
            SettingsMenu.Instance.Activate();
    }

    public void Tutorials(GameObject textObject)
    {
        // Force set the tutorial menu type to general
        SetTutorialMenuType(TutorialMenuType.General);

        ChangeClickColor(textObject);

        // Push the tutorials panel onto the stack
        _menuStack.Push(tutorialsPanel);

        // Isolate the tutorials panel
        IsolateMenu(_menuStack.Peek());

        // Populate the tutorials panel with the tutorials
        var tutorialsFirstSelected = PopulateTutorialsPanel();

        // Set the event system's selected object to the first button
        SetSelectedButton(tutorialsFirstSelected);
    }

    private GameObject PopulateTutorialsPanel()
    {
        // Set the tutorial of the tutorial buttons
        foreach (var button in tutorialButtons)
            button.ResetTutorial();

        // Get the correct tutorial array depending on the tutorial menu type
        var currentTutorials = _tutorialMenuType switch
        {
            TutorialMenuType.General => generalTutorials.value,
            TutorialMenuType.Power => powerTutorials.value,
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
            TutorialMenuType.General => generalTutorialHeader.gameObject,
            TutorialMenuType.Power => powerTutorialHeader.gameObject,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void SetTutorialMenuType(TutorialMenuType type)
    {
        _tutorialMenuType = type;
    }

    public void SetTutorialMenuTypeAndPopulate(int type)
    {
        SetTutorialMenuType((TutorialMenuType)type);

        var firstSelected = PopulateTutorialsPanel();

        SetSelectedButton(firstSelected);
    }

    /// <summary>
    /// Called when the Exit button is clicked.
    /// </summary>
    public void Exit(GameObject textObject)
    {
        ChangeClickColor(textObject);

        // Go back to main menu
        SceneManager.LoadScene(mainMenuScene);
    }

    public void GoBack()
    {
        // If the menu stack is empty, deactivate and return
        if (_menuStack.Count == 0)
        {
            Deactivate();
            return;
        }

        // Pop the current menu off the stack
        _menuStack.Pop();

        // Isolate the new current menu
        IsolateMenu(_menuStack.Peek());

        // Set the event system's selected object to the first button
        if (_menuStack.Peek() == journalPanel)
            SetSelectedButton(journalFirstSelected);
        else if (_menuStack.Peek() == settingsPanel)
            SetSelectedButton(settingsFirstSelected);
        else
            SetSelectedButton(firstSelectedButton);
    }

    public void SetSelectedButton(GameObject button)
    {
        // eventSystem.SetSelectedGameObject(button);
        StartCoroutine(SetSelectedButtonCoroutine(button));
    }

    private IEnumerator SetSelectedButtonCoroutine(GameObject button)
    {
        yield return null;

        eventSystem.SetSelectedGameObject(button);
    }

    public override void OnBackPressed()
    {
        // Return if inputted this frame
        if (_inputtedThisFrame)
            return;

        // If the game is not paused, return
        if (!IsPaused)
            return;

        _inputtedThisFrame = true;

        // Check if the menu stack has more than one item
        // If it does, go back to the previous menu
        if (_menuStack.Count > 1)
            GoBack();

        // Resume the game
        else
            Resume(pauseMenuPanel.transform.GetChild(0).gameObject);
    }

    public static IEnumerator LoadPauseMenuManager()
    {
        // Load the pause menu scene
        SceneManager.LoadScene(PAUSE_SCENE_NAME, LoadSceneMode.Additive);

        // Wait while the instance is null
        yield return new WaitUntil(() => Instance != null);

        // // Call the OnPausePerformed method
        // Instance.OnPausePerformed(obj);
    }

    [Serializable]
    public enum TutorialMenuType : byte
    {
        General,
        Power
    }
}