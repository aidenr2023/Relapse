using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Canvas))]
public class NewGameMenuPage : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private NewGameMenu parentMenu;

    [field: Header("Events"), SerializeField]
    public UnityEvent<NewGameMenuPage> OnActivate { get; private set; }

    [field: SerializeField] public UnityEvent<NewGameMenuPage> OnDeactivate { get; private set; }

    #endregion

    #region Private Fields / Auto Properties

    public Canvas Canvas { get; private set; }

    public bool IsActive { get; private set; }

    #endregion

    private void Awake()
    {
        // Get the canvas group
        Canvas = GetComponent<Canvas>();

        // Make sure the parent menu is not null
        Debug.Assert(parentMenu != null, this);
        
        // Update the isActive state
        IsActive = false;
        gameObject.SetActive(false);
        
    }

    #region Public Functions

    public void Activate()
    {
        // Return if the menu is already active
        if (IsActive)
            return;

        // Update the isActive state
        ChangeActivationState(true);

        // Invoke the OnActivate event
        OnActivate?.Invoke(this);
    }

    public void Deactivate()
    {
        // Return if the menu is already inactive
        if (!IsActive)
            return;

        // Update the isActive state
        ChangeActivationState(false);

        // Invoke the OnDeactivate event
        OnDeactivate?.Invoke(this);
    }

    #endregion

    private void ChangeActivationState(bool isActive)
    {
        // Return if isActive is the same as the current state
        if (isActive == IsActive)
            return;

        // Update the isActive state
        IsActive = isActive;

        // Enable / disable this game object
        gameObject.SetActive(isActive);
    }
}