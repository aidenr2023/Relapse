using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameUIHelper : MonoBehaviour, IUsesInput
{
    public static GameUIHelper Instance { get; private set; }

    #region Serialized Fields

    [SerializeField] private bool showUI = true;

    [SerializeField] private GameObject[] uiElements;

    #endregion

    #region Private Fields

    private bool _isInputRegistered;

    #endregion

    #region Getters

    public HashSet<InputData> InputActions { get; } = new();

    #endregion

    private void Awake()
    {
        // If the instance is not null and not this, destroy this
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // Set the instance to this
        Instance = this;

        // Set this to not be destroyed when reloading the scene
        DontDestroyOnLoad(gameObject);

        // Initialize the input
        InitializeInput();

        // Initialize the game UI
        InitializeGameUI();

        // Connect to the scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // Set the UI enabled state
        SetUIEnabled(showUI);
    }

    private void InitializeGameUI()
    {
        StartCoroutine(PauseMenuManager.LoadPauseMenuManager());
        StartCoroutine(VendorMenu.LoadVendorMenu());
        StartCoroutine(SettingsMenu.LoadSettingsMenu());
        StartCoroutine(RelapseScreen.LoadDeathScene());
        // StartCoroutine(JournalMenu.LoadJournalMenu());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Return if the mode is not single
        if (mode != LoadSceneMode.Single)
            return;

        // If the scene is loaded singularly, then the menus get unloaded.
        // This function forcibly reloads the menus.

        // Initialize the game UI
        InitializeGameUI();
    }

    public void InitializeInput()
    {
        // Pause input actions
        InputActions.Add(
            new InputData(InputManager.Instance.PControls.Player.Pause, InputType.Performed, OnPausePerformed)
        );

        // Journal input actions
        InputActions.Add(
            new InputData(InputManager.Instance.PControls.Player.Journal, InputType.Performed, OnJournalPerformed)
        );
    }

    private void OnJournalPerformed(InputAction.CallbackContext obj)
    {
        // If the journal menu's instance is NOT null, perform the journal action
        if (JournalMenu.Instance != null)
            JournalMenu.Instance.OnJournalPerformed(obj);
    }

    private void OnPausePerformed(InputAction.CallbackContext obj)
    {
        // If the pause menu's instance is NOT null, pause the game
        if (PauseMenuManager.Instance != null)
            PauseMenuManager.Instance.OnPausePerformed(obj);
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

    private void OnDestroy()
    {
        // Unregister the input user
        InputManager.Instance.Unregister(this);

        // Unset this as the instance
        if (Instance == this)
            Instance = null;
    }

    public void ShowUI()
    {
        // Return if the UI is already shown
        if (showUI)
            return;

        showUI = true;
        SetUIEnabled(showUI);
    }

    public void HideUI()
    {
        // Return if the UI is already hidden
        if (!showUI)
            return;

        showUI = false;
        SetUIEnabled(showUI);
    }

    private void SetUIEnabled(bool isEnabled)
    {
        foreach (var uiElement in uiElements)
            uiElement.SetActive(isEnabled);
    }
}