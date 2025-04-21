using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Processors;

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

    private ControlSchemeType _currentControlScheme;

    private readonly HashSet<object> _playerInputDisablers = new();

    #endregion

    #region Getters

    public PlayerControls PControls { get; }
    public PlayerControls OtherControls { get; }

    public UIControls UIControls { get; }

    public bool IsCursorActive =>
        MenuManager.Instance.IsCursorActiveInMenus && CurrentControlScheme == ControlSchemeType.Keyboard;

    public bool IsControlsDisabled => MenuManager.Instance.IsControlsDisabledInMenus || IsExternallyDisabled;

    public ControlSchemeType CurrentControlScheme => _currentControlScheme;

    public bool IsExternallyDisabled => _playerInputDisablers.Count > 0;

    #endregion

    #region Initialization Functions

    private InputManager()
    {
        // Set the instance to this
        _instance = this;

        // Create a new instance of the PlayerControls
        // Enable the PlayerControls
        PControls = new PlayerControls();
        PControls.Enable();

        // Create a new instance of the OtherControls
        // Enable the OtherControls
        OtherControls = new PlayerControls();
        OtherControls.Enable();

        // Create a new instance of the DefaultInputActions
        // Enable the DefaultInputActions
        UIControls = new UIControls();
        UIControls.Enable();

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

    public void SetCurrentControlScheme(ControlSchemeType controlScheme)
    {
        _currentControlScheme = controlScheme;
    }

    public void AddInputDisabler(object disabler)
    {
        // If the disabler is already in the set, return
        if (!_playerInputDisablers.Add(disabler))
            return;
    }

    public void RemoveInputDisabler(object disabler)
    {
        // If the disabler is not in the set, return
        if (!_playerInputDisablers.Remove(disabler))
            return;
    }

    #endregion

    public enum ControlSchemeType
    {
        Keyboard,
        Gamepad
    }
}