using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager
{
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

    #region Private Fields

    private readonly HashSet<IUsesInput> _registeredItems = new();

    #endregion

    #region Getters

    public PlayerControls pControls { get; }

    public bool CursorActive =>
        (PauseManager.Instance != null && PauseManager.Instance.IsPaused) ||
        (VendorMenu.Instance != null && VendorMenu.Instance.IsVendorActive) ||
        (PauseMenuManager.Instance != null && PauseMenuManager.Instance.IsPaused);

    #endregion

    #region Initialization Functions

    private InputManager()
    {
        // Set the instance to this
        _instance = this;

        // Create a new instance of the PlayerControls
        pControls = new PlayerControls();

        // Enable the PlayerControls
        pControls.Enable();

        // Initialize the registered items
        _registeredItems.Clear();

        // Start Function
        Start();
    }

    // Start is called before the first frame update
    private void Start()
    {
        // Hide & Lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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