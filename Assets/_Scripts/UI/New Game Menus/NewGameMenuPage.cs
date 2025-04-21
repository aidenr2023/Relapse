using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Canvas))]
public class NewGameMenuPage : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private NewGameMenu parentMenu;
    [SerializeField] private GameObject firstSelectedElement;

    [field: Header("Events"), SerializeField]
    public UnityEvent<NewGameMenuPage> OnActivate { get; private set; }

    [field: SerializeField] public UnityEvent<NewGameMenuPage> OnDeactivate { get; private set; }

    #endregion

    #region Private Fields / Auto Properties

    public Canvas Canvas { get; private set; }

    public bool IsActive { get; private set; }

    private GameObject _previousSelectedElement;

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

        // Ensure the selected element is set on activation
        OnActivate.AddListener(_ => EnsureSelectedElement());
    }

    #region Public Functions

    public bool Activate(bool reinitializeSelectedElement = true)
    {
        // Return if the menu is already active
        if (IsActive)
            return false;

        // Update the isActive state
        ChangeActivationState(true);

        // Invoke the OnActivate event
        OnActivate?.Invoke(this);

        // Set the selected game object
        if (reinitializeSelectedElement)
            InitializeSelectedElement(firstSelectedElement);
        else
        {
            InitializeSelectedElement(_previousSelectedElement);

            // If the selected game object is STILL null, default to the first selected element
            if (parentMenu.EventSystem.currentSelectedGameObject == null)
                parentMenu.EventSystem.SetSelectedGameObject(firstSelectedElement);
        }

        return true;
    }

    public bool Deactivate()
    {
        // Return if the menu is already inactive
        if (!IsActive)
            return false;

        // Store the previous selected element
        _previousSelectedElement = parentMenu.EventSystem.currentSelectedGameObject;

        // Update the isActive state
        ChangeActivationState(false);

        // Invoke the OnDeactivate event
        OnDeactivate?.Invoke(this);

        return true;
    }

    private void InitializeSelectedElement(GameObject obj)
    {
        // Return if there is no object
        if (obj == null)
            return;

        // Select the initial game object
        parentMenu.EventSystem.SetSelectedGameObject(obj);
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

    private void EnsureSelectedElement()
    {
        // Return if the selected element is already set
        if (parentMenu.EventSystem.currentSelectedGameObject != null)
            return;

        // Set the selected game object to the first selected element
        parentMenu.EventSystem.SetSelectedGameObject(firstSelectedElement);
    }
}