using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Processors;
using UnityEngine.PlayerLoop;

[RequireComponent(typeof(PlayerInput))]
public class InputManagerHelper : MonoBehaviour
{
    public static InputManagerHelper Instance { get; private set; }

    private PlayerInput _playerInput;

    private void Awake()
    {
        // If the instance is not null and is not this, destroy this object
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Set the instance to this
        Instance = this;

        // Get the PlayerInput component
        _playerInput = GetComponent<PlayerInput>();
    }

    private void OnDestroy()
    {
        // Set the instance to null
        Instance = null;
    }

    private void Start()
    {
        // Start Function
        InputManager.Instance.Start();
    }

    private void Update()
    {
        // Enable / disable the cursor based on the cursor state
        SetCursorState(InputManager.Instance.IsCursorActive);

        // Enable / disable the player controls based on the controls state
        SetPlayerControlsState(!InputManager.Instance.IsControlsDisabled);

        // Set the current control scheme
        if (_playerInput.currentControlScheme == "Gamepad")
            InputManager.Instance.SetCurrentControlScheme(InputManager.ControlSchemeType.Gamepad);
        else
            InputManager.Instance.SetCurrentControlScheme(InputManager.ControlSchemeType.Keyboard);

    }

    /// <summary>
    /// Disable the player controls.
    /// Hide / show the cursor.
    ///
    /// True - Unlocks the cursor and shows it. Used for menus and pause states.
    /// False - Locks the cursor and hides it. Used for gameplay.
    /// </summary>
    /// <param name="state"></param>
    private void SetCursorState(bool state)
    {
        // Set the cursor state
        Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;

        // Show / hide the cursor
        Cursor.visible = state;
    }

    private void SetPlayerControlsState(bool state)
    {
        // Enable the player controls
        if (state)
            InputManager.Instance.PControls.Enable();

        // Disable the player controls
        else
            InputManager.Instance.PControls.Disable();
    }
}