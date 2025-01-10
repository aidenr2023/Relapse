using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class GameMenu : MonoBehaviour
{
    private const float OPACITY_THRESHOLD = 0.001f;

    #region Serialized Fields

    [SerializeField] protected CanvasGroup canvasGroup;

    [SerializeField] private bool isActiveOnStart = false;
    [SerializeField, Min(0)] private float opacityLerpAmount = .25f;

    [Header("Menu Settings"), SerializeField]
    protected bool isCursorRequired = true;

    [SerializeField] protected bool disablePlayerControls = true;
    [SerializeField] protected bool pausesGame = true;

    #endregion

    #region Private Fields

    private float _desiredOpacity;

    private bool _isActive;

    #endregion

    #region Getters

    public bool IsCursorRequired => isCursorRequired;

    public bool DisablePlayerControls => disablePlayerControls;

    public bool PausesGame => pausesGame;

    public bool IsActive => _isActive;

    #endregion

    protected void Awake()
    {
        // Manage this menu
        MenuManager.Instance.ManageMenu(this);

        if (isActiveOnStart)
        {
            // Activate the menu
            Activate();

            // Set the canvas group's alpha to 1
            canvasGroup.alpha = 1;
        }
        else
        {
            // Set the canvas group's alpha to 0
            canvasGroup.alpha = 0;

            // Deactivate the menu
            Deactivate();
        }

        // Set the desired opacity to the canvas group's alpha
        _desiredOpacity = canvasGroup.alpha;

        // Custom Awake
        CustomAwake();
    }

    protected abstract void CustomAwake();

    public void Activate()
    {
        // Add this to the active menus
        MenuManager.Instance.AddActiveMenu(this);

        // Set the desired opacity to 1
        _desiredOpacity = 1;

        // Set the activate flag to true
        _isActive = true;

        CustomActivate();
    }

    public void Deactivate()
    {
        // Remove this from the active menus
        MenuManager.Instance.RemoveActiveMenu(this);

        // Set the desired opacity to 0
        _desiredOpacity = 0;

        // Set the activate flag to false
        _isActive = false;

        CustomDeactivate();
    }

    protected void OnDestroy()
    {
        // Deactivate the menu
        Deactivate();

        // Un manage this menu
        MenuManager.Instance.UnManageMenu(this);

        // Custom Destroy
        CustomDestroy();
    }

    protected abstract void CustomDestroy();

    protected abstract void CustomActivate();
    protected abstract void CustomDeactivate();

    protected void Update()
    {
        // Lerp the canvas group's alpha to the desired opacity
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, _desiredOpacity, opacityLerpAmount);

        // If the canvas group's alpha is less than the opacity threshold
        // Set the canvas group's alpha to the desired opacity
        if (Mathf.Abs(canvasGroup.alpha - _desiredOpacity) < OPACITY_THRESHOLD)
            canvasGroup.alpha = _desiredOpacity;

        // If the canvas group's alpha is 0, deactivate the menu
        if (canvasGroup.alpha == 0)
            Deactivate();

        // Set the canvas group's interactable to the canvas group's alpha is 1
        canvasGroup.interactable = _isActive;
        canvasGroup.blocksRaycasts = _isActive;

        // Custom Update
        CustomUpdate();
    }

    protected abstract void CustomUpdate();

    public abstract void OnBackPressed();
}