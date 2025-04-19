using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public abstract class NewGameMenu : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] protected Canvas canvas;
    [SerializeField] protected CanvasGroup canvasGroup;
    [SerializeField] protected EventSystem eventSystem;

    [field: Header("Menu Settings"), SerializeField]
    public bool DisablePlayerControls { get; protected set; } = true;

    [field: SerializeField] public bool PausesGame { get; protected set; } = true;
    [field: SerializeField] public bool PausesGameMusic { get; protected set; } = true;
    [SerializeField] private bool isActiveOnStart = false;
    [SerializeField] protected bool usesFade = true;

    [Header("Misc."), SerializeField] private AnimationCurve opacityCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [field: Header("Events"), SerializeField]
    public UnityEvent<NewGameMenu> OnActivate { get; private set; }

    [field: SerializeField] public UnityEvent<NewGameMenu> OnDeactivate { get; private set; }

    #endregion

    #region Private Fields / Auto Properties

    private Coroutine _fadeCoroutine;

    public bool IsActive { get; private set; }

    #endregion

    #region Unity Methods

    private void Awake()
    {
        // Forcibly set the menu to be deactivated
        ForceDeactivate();
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
        canvasGroup.alpha = opacityCurve.Evaluate(0);
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
        
        Debug.Log($"Changed Activation State: {isActive}");
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
        var finalKey = opacityCurve.keys[opacityCurve.length - 1];

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
            var opacity = opacityCurve.Evaluate(elapsedTime);

            // Set the canvas group's alpha to the current opacity
            canvasGroup.alpha = opacity;

            // Wait for the next frame
            yield return null;
        }

        // Set the final opacity
        if (inOut)
            canvasGroup.alpha = opacityCurve.Evaluate(finalKey.time);
        else
            canvasGroup.alpha = opacityCurve.Evaluate(0);
    }
}