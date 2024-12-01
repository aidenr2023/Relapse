using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance { get; private set; }

    #region Serialized Fields

    [SerializeField] private GameObject pauseMenuParent;

    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject journalPanel;
    [SerializeField] private GameObject settingsPanel;

    [SerializeField] private GameObject firstSelectedButton;

    // Colors for normal, hover, and click states
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color clickColor = Color.red;



    #endregion

    #region Private Fields

    private float _tempTimeScale;
    private float _fixedDeltaTime;

    private readonly Stack<GameObject> _menuStack = new();

    #endregion

    #region Getters

    public bool IsPaused { get; private set; }

    #endregion

    /// <summary>
    /// Changes the text color of the TextMeshPro to the hover color.
    /// Attach this to the PointerEnter event.
    /// </summary>
    private void Awake()
    {
        // Set the instance to this object
        Instance = this;
    }

    private void Start()
    {
        // Connect to input system
        InputManager.Instance.PlayerControls.Player.Pause.performed += _ => TogglePause();

        // Hide the pause menu at the start
        pauseMenuParent.SetActive(false);
    }

    private void OnEnable()
    {
        // Set the event system's selected object to the first button
        SetSelectedButton(firstSelectedButton);
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


    private void FixPhysics()
    {
        return;

        Time.fixedDeltaTime = _fixedDeltaTime * Time.timeScale;
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

        Time.timeScale = _tempTimeScale;
        FixPhysics();

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
        _tempTimeScale = Time.timeScale;

        //Pause the game
        Time.timeScale = 0;
        FixPhysics();

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
    }

    /// <summary>
    /// Called when the Exit button is clicked.
    /// </summary>
    public void Exit(GameObject textObject)
    {
        // Resume the game
        Time.timeScale = _tempTimeScale;
        FixPhysics();

        ChangeClickColor(textObject);

        // Go back to main menu
        SceneManager.LoadScene("MainMenu");

        Debug.Log($"RANNNNNNN: {textObject.name}");
    }

    public void Back()
    {
        // Pop the current menu off the stack
        _menuStack.Pop();

        // Isolate the new current menu
        IsolateMenu(_menuStack.Peek());
    }

    public void SetSelectedButton(GameObject button)
    {
        EventSystem.current.SetSelectedGameObject(button);
    }
}