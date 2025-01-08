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

public class PauseMenuManager : GameMenu, IUsesInput
{
    #region Serialized Fields

    [SerializeField] private GameObject pauseMenuParent;

    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject journalPanel;
    [SerializeField] private GameObject settingsPanel;

    // Colors for normal, hover, and click states
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color clickColor = Color.red;


    [Header("Navigation Control")] [SerializeField]
    private GameObject firstSelectedButton;

    [SerializeField] private GameObject settingsFirstSelected;
    [SerializeField] private GameObject journalFirstSelected;

    #endregion

    #region Private Fields

    private readonly Stack<GameObject> _menuStack = new();

    private bool _isInputRegistered;

    private bool _inputtedThisFrame;

    #endregion

    #region Getters

    public HashSet<InputData> InputActions { get; } = new();

    public bool IsPaused { get; private set; }

    #endregion

    /// <summary>
    /// Changes the text color of the TextMeshPro to the hover color.
    /// Attach this to the PointerEnter event.
    /// </summary>
    protected override void CustomAwake()
    {
        // Initialize the input
        InitializeInput();
    }

    public void InitializeInput()
    {
        // // Connect to input system
        // InputActions.Add(
        //     new InputData(InputManager.Instance.DefaultInputActions.UI.Cancel, InputType.Performed, OnBackPerformed)
        // );

        InputActions.Add(
            new InputData(InputManager.Instance.PControls.Player.Pause, InputType.Performed, OnPausePerformed)
        );
    }

    private void OnEnable()
    {
        if (!_isInputRegistered)
        {
            // Register the input user
            InputManager.Instance.Register(this);

            // Set the input registered flag to true
            _isInputRegistered = true;
        }
    }

    protected override void CustomActivate()
    {
        // Set the event system's selected object to the first button
        SetSelectedButton(firstSelectedButton);
    }

    protected override void CustomDeactivate()
    {
    }

    private void OnDestroy()
    {
        // Unregister the input user
        InputManager.Instance.Unregister(this);

        // Deactivate the menu
        Deactivate();
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        // Return if inputted this frame
        if (_inputtedThisFrame)
            return;

        _inputtedThisFrame = true;

        Debug.Log($"Pause Button Pressed!");

        TogglePause();
    }


    private void OnBackPerformed(InputAction.CallbackContext obj)
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

    private void IsolateMenu(GameObject obj)
    {
        // Hide all the menus
        pauseMenuPanel.SetActive(false);
        journalPanel.SetActive(false);
        settingsPanel.SetActive(false);

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

        // Add your Resume logic here
        Debug.Log("Resume game");

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
        Debug.Log($"Pause!");

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

        // Push the settings panel onto the stack
        _menuStack.Push(settingsPanel);

        // Isolate the settings panel
        IsolateMenu(_menuStack.Peek());

        // Set the event system's selected object to the first button
        SetSelectedButton(settingsFirstSelected);
    }

    /// <summary>
    /// Called when the Exit button is clicked.
    /// </summary>
    public void Exit(GameObject textObject)
    {
        ChangeClickColor(textObject);

        // Go back to main menu
        // TODO: Use a scene field
        SceneManager.LoadScene("MainMenu");
    }

    public void GoBack()
    {
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
        EventSystem.current.SetSelectedGameObject(button);
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
}