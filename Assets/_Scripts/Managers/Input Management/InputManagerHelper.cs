﻿using System;
using UnityEngine;

public class InputManagerHelper : MonoBehaviour
{
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