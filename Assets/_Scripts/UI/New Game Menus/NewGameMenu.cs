using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class NewGameMenu : MonoBehaviour, IGameMenu
{
    #region Serialized Fields

    [SerializeField] protected Canvas canvas;
    [SerializeField] protected CanvasGroup canvasGroup;
    [field: SerializeField] public EventSystem EventSystem { get; private set; }
    [SerializeField] protected NewGameMenuPage initialPage;

    [field: Header("Menu Settings"), SerializeField]
    public bool DisablePlayerControls { get; protected set; } = true;

    [field: SerializeField] public bool PausesGame { get; protected set; } = true;
    [field: SerializeField] public bool PausesGameMusic { get; protected set; } = true;


    [SerializeField] private bool isActiveOnStart = false;
    [SerializeField] protected bool usesFade = true;

    [field: Header("Misc."), SerializeField]
    public AnimationCurve OpacityCurve { get; private set; } = AnimationCurve.EaseInOut(0, 0, .125f, 1);

    [field: Header("Events"), SerializeField]
    public UnityEvent<NewGameMenu> OnActivate { get; private set; }

    [field: SerializeField] public UnityEvent<NewGameMenu> OnDeactivate { get; private set; }

    #endregion

    #region Private Fields / Auto Properties

    private readonly Stack<NewGameMenuPage> _pageStack = new();
    private Coroutine _fadeCoroutine;
    private NewGameMenuPage _reenablePage;

    private bool _activatedThisFrame;

    public bool IsActive { get; private set; }

    public bool IsCursorRequired => true;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        Debug.Assert(initialPage != null, this);

        // Activate the initial page
        PushMenuPage(initialPage);
    }

    protected void Start()
    {
        // Forcibly set the menu to be deactivated
        ForceDeactivate();

        // If the menu is active on start, activate it
        if (isActiveOnStart)
            Activate();
    }

    private void OnEnable()
    {
        // Connect to the onActiveMenuChanged event
        MenuManager.Instance.OnActiveMenuChanged += OnActiveMenuChanged;
    }

    private void OnDisable()
    {
        // Connect to the onActiveMenuChanged event
        MenuManager.Instance.OnActiveMenuChanged += OnActiveMenuChanged;

        // Deactivate
        Deactivate();
    }

    private void OnActiveMenuChanged()
    {
        // If the active menu is not this, deactivate the event system
        SetEventSystemActive(ReferenceEquals(MenuManager.Instance.ActiveMenu, this));
    }

    #endregion

    #region Public Functions

    [ContextMenu("Activate Menu")]
    public void Activate()
    {
        // Return if the menu is already active
        if (IsActive)
            return;

        // reactivate the page that needs to be reactivated
        if (_reenablePage != null)
        {
            _reenablePage.Activate();
            _reenablePage = null;
        }

        // If there is a menu page at the top of the stack, activate it just to make sure its on
        if (_pageStack.Count >= 1)
            _pageStack.Peek().Activate();

        // Add this menu to the active menus of the menu manager
        MenuManager.Instance.AddActiveMenu(this);

        // Update the isActive state
        ChangeActivationState(true);

        // Set the activate this frame flag
        _activatedThisFrame = true;

        // Start the coroutine to reset the activated this frame flag
        StartCoroutine(ResetActivatedThisFrame());

        // Start the fade in coroutine
        StartFadeCoroutine(true);

        // Invoke the OnActivate event
        OnActivate?.Invoke(this);
    }

    [ContextMenu("Deactivate Menu")]
    public void Deactivate()
    {
        // Return if the menu is already inactive
        if (!IsActive)
            return;

        // Debug.Log($"Deactivating {name}", this);

        // Start the fade out coroutine
        StartFadeCoroutine(false);

        // Perform the deactivation logic
        DeactivateLogic();
    }

    private void DeactivateLogic(bool forcePageOff = false)
    {
        // Remove this menu from the active menus of the menu manager
        MenuManager.Instance.RemoveActiveMenu(this);

        if (forcePageOff)
        {
            _reenablePage = _pageStack.Peek();

            // If fading out, deactivate the current page
            _reenablePage.Deactivate();
            // Debug.Log($"Deactivating {_reenablePage.name}", this);
        }

        // Update the isActive state
        ChangeActivationState(false);

        // Invoke the OnDeactivate event
        OnDeactivate?.Invoke(this);
    }

    private void ForceDeactivate()
    {
        DeactivateLogic(true);

        // Set the alpha to the minimum value
        canvasGroup.alpha = OpacityCurve.Evaluate(0);
    }

    private void ChangeActivationState(bool isActive)
    {
        // Update the isActive state
        IsActive = isActive;

        // Set the canvas group to be interactable or not
        canvasGroup.interactable = isActive;
        canvasGroup.blocksRaycasts = isActive;

        // Set the event system's active state
        SetEventSystemActive(isActive);
    }

    private void SetEventSystemActive(bool isActive)
    {
        // Return if the event system is null
        if (EventSystem == null)
            return;

        // Set the event system's active state
        EventSystem.gameObject.SetActive(isActive);
    }

    public void OnBackPressed()
    {
        // If this menu activated this frame, return
        if (_activatedThisFrame)
            return;

        PreviousPage();
    }

    #endregion

    #region Menu Stack

    public void PushMenuPage(NewGameMenuPage menuPage)
    {
        // Return if the new page is null
        if (menuPage == null)
            return;

        // Return if the page is already inside the stack
        if (_pageStack.Contains(menuPage))
            return;

        // If there is a menu page at the top of the stack, deactivate it
        if (_pageStack.Count >= 1)
            _pageStack.Peek().Deactivate();

        // Push the new menu page
        _pageStack.Push(menuPage);

        // Activate the new menu page
        menuPage.Activate();
    }

    private void PopMenuPage(bool reinitializeSelectedElement = true)
    {
        // If the stack is currently has 1 or fewer pages, return
        if (_pageStack.Count <= 1)
            return;

        // Pop menu at the top of the stack
        var prevMenu = _pageStack.Pop();

        // Deactivate the previous menu
        prevMenu.Deactivate();

        // If there is a new menu at the top of the stack, activate it
        _pageStack.Peek()?.Activate(reinitializeSelectedElement);
    }

    public void PopThenPushMenuPage(NewGameMenuPage menuPage)
    {
        // Pop menu at the top of the stack
        // Deactivate the previous menu
        _pageStack.Pop().Deactivate();

        // Push
        PushMenuPage(menuPage);
    }

    public void PreviousPage()
    {
        // Pop the page if there is more than one
        if (_pageStack.Count > 1)
        {
            PopMenuPage(false);
            return;
        }

        // Deactivate this menu
        Deactivate();
    }

    #endregion

    private void StartFadeCoroutine(bool inOut)
    {
        // Stop the existing coroutine if it exists
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        // Start the fade coroutine
        if (!usesFade)
            canvasGroup.alpha = inOut ? 1 : 0;
        else
            _fadeCoroutine = StartCoroutine(FadeCoroutine(inOut));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="inOut">true - in. false - out</param>
    /// <returns></returns>
    private IEnumerator FadeCoroutine(bool inOut)
    {
        if (!inOut)
            _reenablePage = _pageStack.Peek();

        var finalKey = OpacityCurve.keys[OpacityCurve.length - 1];

        // Get the maximum duration of the fade
        var maxDuration = finalKey.time;

        // store the start time of the fade
        var startTime = Time.unscaledTime;

        while (Time.unscaledTime < startTime + maxDuration)
        {
            // Calculate the current time
            var elapsedTime = Time.unscaledTime - startTime;

            // If fading out, do 1 - elapsedTime
            if (!inOut)
                elapsedTime = maxDuration - elapsedTime;

            // Calculate the current opacity
            var opacity = OpacityCurve.Evaluate(elapsedTime);

            // Set the canvas group's alpha to the current opacity
            canvasGroup.alpha = opacity;

            // Wait for the next frame
            yield return null;
        }

        // Set the final opacity
        if (inOut)
            canvasGroup.alpha = OpacityCurve.Evaluate(finalKey.time);
        else
        {
            canvasGroup.alpha = OpacityCurve.Evaluate(0);

            // If fading out, deactivate the current page
            _reenablePage.Deactivate();
            // Debug.Log($"Deactivating {_reenablePage.name}", this);
        }
    }

    private IEnumerator ResetActivatedThisFrame()
    {
        // Wait for the next frame
        yield return null;

        // Reset the activated this frame flag
        _activatedThisFrame = false;
    }
}