using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager
{
    #region Singleton Pattern

    private static InputManager _instance;

    public static InputManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new InputManager();

            return _instance;
        }
    }

    #endregion

    #region Private Fields

    private readonly HashSet<IUsesInput> _registeredItems = new();

    private bool _hasStarted;

    #endregion

    #region Getters

    public PlayerControls PControls { get; }

    public DefaultInputActions DefaultInputActions { get; }

    public bool IsCursorActive => MenuManager.Instance.IsCursorActiveInMenus;

    public bool IsControlsDisabled => MenuManager.Instance.IsControlsDisabledInMenus;

    #endregion

    #region Initialization Functions

    private InputManager()
    {
        // Set the instance to this
        _instance = this;

        // Create a new instance of the PlayerControls
        PControls = new PlayerControls();

        // Enable the PlayerControls
        PControls.Enable();

        // Create a new instance of the DefaultInputActions
        DefaultInputActions = new DefaultInputActions();

        // Enable the DefaultInputActions
        DefaultInputActions.Enable();

        // Initialize the registered items
        _registeredItems.Clear();
    }

    // Start is called before the first frame update
    public void Start()
    {
        // Return if the InputManager has already started
        if (_hasStarted)
            return;

        // Hide & Lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Set the flag to true
        _hasStarted = true;
    }

    #endregion

    #region Public Methods

    public void Register(IUsesInput item)
    {
        // If the item is already in the registered items, return
        if (!_registeredItems.Add(item))
            return;

        // Add the item to the registered items
        // Loop through each input action and ADD the action to the callback
        foreach (var input in item.InputActions)
        {
            switch (input.inputType)
            {
                case InputType.Started:
                    input.inputAction.started += input.inputFunc;
                    break;

                case InputType.Performed:
                    input.inputAction.performed += input.inputFunc;
                    break;

                case InputType.Canceled:
                    input.inputAction.canceled += input.inputFunc;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // Debug.Log($"Registered {(item as MonoBehaviour)!} to the InputManager!");
    }

    public void Unregister(IUsesInput item)
    {
        // If the item is not in the registered items, return
        if (!_registeredItems.Remove(item))
            return;

        // Remove the item from the registered items
        // Loop through each input action and REMOVE the action from the callback
        foreach (var input in item.InputActions)
        {
            switch (input.inputType)
            {
                case InputType.Started:
                    input.inputAction.started -= input.inputFunc;
                    break;

                case InputType.Performed:
                    input.inputAction.performed -= input.inputFunc;
                    break;

                case InputType.Canceled:
                    input.inputAction.canceled -= input.inputFunc;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    #endregion
}