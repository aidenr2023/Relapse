using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameUIHelper : MonoBehaviour, IUsesInput
{
    private const float DEFAULT_TRANSITION_TIME = 1f;
    
    public static GameUIHelper Instance { get; private set; }

    #region Serialized Fields

    [SerializeField] private bool showUI = true;

    [SerializeField] private EventVariable pauseEvent;
    
    [SerializeField] private FloatReference uiOpacity;
    [SerializeField] private CanvasGroupListVariable uiElements;

    #endregion

    #region Private Fields

    private bool _isInputRegistered;
    
    private Coroutine _fadeCoroutine;
    
    private readonly HashSet<object> _uiHiders = new();

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

        // // Journal input actions
        // InputActions.Add(
        //     new InputData(InputManager.Instance.PControls.Player.Journal, InputType.Performed, OnJournalPerformed)
        // );
    }

    private void OnJournalPerformed(InputAction.CallbackContext obj)
    {
        // If the journal menu's instance is NOT null, perform the journal action
        if (JournalMenu.Instance != null)
            JournalMenu.Instance.OnJournalPerformed(obj);
    }

    private void OnPausePerformed(InputAction.CallbackContext obj)
    {
        // // If the pause menu's instance is NOT null, pause the game
        // if (PauseMenuManager.Instance != null)
        //     PauseMenuManager.Instance.OnPausePerformed(obj);
        
        // If the pause event is not null, invoke it
        if (pauseEvent != null)
            pauseEvent.Invoke();
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

    private void Update()
    {
        // Update the UI opacity
        UpdateUIOpacity(uiElements.value, uiOpacity);
    }

    private static void UpdateUIOpacity(IEnumerable<CanvasGroup> uiElements, float opacity)
    {
        foreach (var uiElement in uiElements)
            uiElement.alpha = opacity;
    }

    private void SetUIEnabled(bool isEnabled)
    {
        foreach (var uiElement in uiElements.value)
            uiElement.gameObject.SetActive(isEnabled);
    }

    private IEnumerator FadeUIOpacityCoroutine(float target, float time)
    {
        // Get the start time
        var startTime = Time.unscaledTime;

        // Get the start opacity
        var startOpacity = uiOpacity.Value;

        // While the time is less than the target time
        while (Time.unscaledTime - startTime < time)
        {
            // Get the time
            var timePercent = (Time.unscaledTime - startTime) / time;

            // Set the opacity
            uiOpacity.Value = Mathf.Lerp(startOpacity, target, timePercent);

            yield return null;
        }

        // Set the opacity to the target
        uiOpacity.Value = target;
        
        // Set the fade coroutine to null
        _fadeCoroutine = null;
    }

    private void FadeUIOpacity(float target, float time)
    {
        // If the fade coroutine is not null, stop it
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);
        
        // Start the fade coroutine
        _fadeCoroutine = StartCoroutine(FadeUIOpacityCoroutine(target, time));
    }

    [ContextMenu("Fade UI Opacity In")]
    private void FadeUIOpacityIn(float time = DEFAULT_TRANSITION_TIME) => FadeUIOpacity(1, time);
    
    [ContextMenu("Fade UI Opacity Out")]
    private void FadeUIOpacityOut(float time = DEFAULT_TRANSITION_TIME) => FadeUIOpacity(0, time);
    
    public void AddUIHider(object obj, float time = DEFAULT_TRANSITION_TIME)
    {
        // Store the number of hiders before adding
        var previousHiderCount = _uiHiders.Count;

        // If the object is already in the set, return
        if (!_uiHiders.Add(obj))
            return;
        
        // If the previous hider count is 0, fade the UI out
        if (previousHiderCount <= 0 && _fadeCoroutine == null)
            FadeUIOpacityOut(time);
    }

    public void RemoveUIHider(object obj, float time = DEFAULT_TRANSITION_TIME)
    {
        // Store the number of hiders before adding
        var previousHiderCount = _uiHiders.Count;
        
        // If the object is not in the set, return
        if (!_uiHiders.Remove(obj))
            return;
        
        // If the previous hider count greater than 0,
        // And the current hider count is 0
        // fade the UI out
        if (previousHiderCount > 0 && _uiHiders.Count <= 0 && _fadeCoroutine == null)
            FadeUIOpacityIn(time);
    }
}