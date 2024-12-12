using System;
using System.Collections;
using System.Collections.Generic;
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

    private float _tempTimeScale;
    private float _fixedDeltaTime;

    private readonly Stack<GameObject> _menuStack = new();

    private bool _isInputRegistered;

    #endregion

    #region Getters

    public HashSet<InputData> InputActions { get; } = new();

    public bool IsPaused { get; private set; }

    #endregion

    /// <summary>
    /// Changes the text color of the TextMeshPro to the hover color.
    /// Attach this to the PointerEnter event.
    /// </summary>
    private void Awake()
    {
        // Initialize the input
        InitializeInput();
    }

    public void InitializeInput()
    {
        // Connect to input system
        InputActions.Add(
            new InputData(InputManager.Instance.DefaultInputActions.UI.Cancel, InputType.Performed, OnPausePerformed)
        );
    }

    private void Start()
    {
        // Hide the pause menu at the start
        pauseMenuParent.SetActive(false);
    }

    protected override void CustomOnEnable()
    {
        if (!_isInputRegistered)
        {
            // Register the input user
            InputManager.Instance.Register(this);

            // Set the input registered flag to true
            _isInputRegistered = true;
        }

        // Set the event system's selected object to the first button
        SetSelectedButton(firstSelectedButton);

        Debug.Log($"Adding this to the input manager: {this}");
    }

    protected override void CustomOnDisable()
    {
    }

    private void OnDestroy()
    {
        // Unregister the input user
        InputManager.Instance.Unregister(this);
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        TogglePause();

        Debug.Log("Pause");
    }

    private void TogglePause()
    {
        if (!IsPaused)
            Pause();

        else
        {
            // Check if the menu stack has more than one item
            // If it does, go back to the previous menu
            if (_menuStack.Count > 1)
                Back();

            // Resume the game
            else
                Resume(pauseMenuPanel.transform.GetChild(0).gameObject);
        }
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

        // TODO: Time manager
        Time.timeScale = _tempTimeScale;

        //Hide menu
        pauseMenuParent.SetActive(false);

        //Set pause to false
        IsPaused = false;
    }

    public void Pause()
    {
        _fixedDeltaTime = Time.fixedDeltaTime;

        // Un-hide the pause menu
        pauseMenuParent.SetActive(true);

        // Save the current timescale
        // TODO: Time manager
        _tempTimeScale = Time.timeScale;

        // Pause the game
        // TODO: Time manager
        Time.timeScale = 0;

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

        // Add your Journal logic here
        Debug.Log("Open Journal");

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

        // Add your Settings logic here
        Debug.Log("Open Settings");

        // Set the event system's selected object to the first button
        SetSelectedButton(settingsFirstSelected);
    }

    /// <summary>
    /// Called when the Exit button is clicked.
    /// </summary>
    public void Exit(GameObject textObject)
    {
        // Resume the game
        // TODO: Time manager
        Time.timeScale = _tempTimeScale;

        ChangeClickColor(textObject);

        // Go back to main menu
        SceneManager.LoadScene("MainMenu");
    }

    public void Back()
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
}