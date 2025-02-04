using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameUIHelper : MonoBehaviour, IUsesInput
{
    #region Private Fields

    private bool _isInputRegistered;

    #endregion

    #region Getters

    public HashSet<InputData> InputActions { get; } = new();

    #endregion

    private void Awake()
    {
        // Initialize the input
        InitializeInput();
        
        StartCoroutine(PauseMenuManager.LoadPauseMenuManager());
    }

    public void InitializeInput()
    {
        InputActions.Add(
            new InputData(InputManager.Instance.PControls.Player.Pause, InputType.Performed, OnPausePerformed)
        );
    }

    private void OnPausePerformed(InputAction.CallbackContext obj)
    {
        // If the pause menu's instance is NOT null, pause the game
        if (PauseMenuManager.Instance != null)
            PauseMenuManager.Instance.OnPausePerformed(obj);
    }

    private void OnEnable()
    {
        if (!_isInputRegistered)
        {
            // Register the input user
            InputManager.Instance.Register(this);

            // Set the input registered flag to true
            _isInputRegistered = true;
        }
    }

    private void OnDestroy()
    {
        // Unregister the input user
        InputManager.Instance.Unregister(this);
    }
}