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
    public AnimationCurve OpacityCurve { get; private set; } = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [field: Header("Events"), SerializeField]
    public UnityEvent<NewGameMenu> OnActivate { get; private set; }

    [field: SerializeField] public UnityEvent<NewGameMenu> OnDeactivate { get; private set; }

    #endregion

    #region Private Fields / Auto Properties

    private readonly Stack<NewGameMenuPage> _pageStack = new();
    private Coroutine _fadeCoroutine;

    public bool IsActive { get; private set; }

    public bool IsCursorRequired => true;
        
    #endregion

    #region Unity Methods

    private void Awake()
    {
        // Forcibly set the menu to be deactivated
        ForceDeactivate();
        
        Debug.Assert(initialPage != null, this);
        
        // Activate the initial page
        PushMenuPage(initialPage);
    }

    protected void Start()
    {
        // If the menu is active on start, activate it
        if (isActiveOnStart)
            Activate();
    }

    #endregion

    #region Public Functions

    [ContextMenu("Activate Menu")]
    public void Activate()
    {
        // Return if the menu is already active
        if (IsActive)
            return;
        
        // Add this menu to the active menus of the menu manager
        MenuManager.Instance.AddActiveMenu(this);

        // Update the isActive state
        ChangeActivationState(true);

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
        
        // Remove this menu from the active menus of the menu manager
        MenuManager.Instance.RemoveActiveMenu(this);

        // Update the isActive state
        ChangeActivationState(false);

        // Start the fade out coroutine
        StartFadeCoroutine(false);

        // Perform the deactivation logic
        DeactivateLogic();
    }

    private void DeactivateLogic()
    {
        // Invoke the OnDeactivate event
        OnDeactivate?.Invoke(this);
    }

    private void ForceDeactivate()
    {
        DeactivateLogic();

        // Set the alpha to the minimum value
        canvasGroup.alpha = OpacityCurve.Evaluate(0);
    }

    private void ChangeActivationState(bool isActive)
    {
        // Return if isActive is the same as the current state
        if (isActive == IsActive)
            return;

        // Update the isActive state
        IsActive = isActive;

        // Set the canvas group to be interactable or not
        canvasGroup.interactable = isActive;
        canvasGroup.blocksRaycasts = isActive;

        // Set the event system's active state
        EventSystem.gameObject.SetActive(isActive);
    }

    public void OnBackPressed() => PreviousPage();
    
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
        // Pop
        PopMenuPage(true);
        
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
            canvasGroup.alpha = OpacityCurve.Evaluate(0);
    }
}