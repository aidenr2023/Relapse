using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugFlashlight : MonoBehaviour, IUsesInput
{
    [SerializeField] private Light debugFlashlight;

    [SerializeField] private bool canBeEnabled = true;

    public HashSet<InputData> InputActions { get; } = new();

    public bool IsFlashlightOn => debugFlashlight.enabled;

    private void Awake()
    {
        // Ensure the flashlight is off when the game starts.
        SetFlashlight(false);

        InitializeInput();
    }

    private void OnEnable()
    {
        // Register the input user
        InputManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        // Unregister the input user
        InputManager.Instance.Unregister(this);
    }

    public void InitializeInput()
    {
        // Add the input action to the input actions hashset
        InputActions.Add(
            new InputData(InputManager.Instance.pControls.Debug.Flashlight, InputType.Performed,
                ToggleFlashlightOnInput)
        );
    }

    private void ToggleFlashlightOnInput(InputAction.CallbackContext obj)
    {
        if (!canBeEnabled)
        {
            SetFlashlight(false);
            return;
        }

        ToggleFlashlight();
    }


    public void ToggleFlashlight()
    {
        // return if there is no flashlight
        if (!debugFlashlight)
            return;

        SetFlashlight(!IsFlashlightOn);
    }

    public void SetFlashlight(bool value)
    {
        // return if there is no flashlight
        if (!debugFlashlight)
            return;

        // Set the flashlight to the given value.
        debugFlashlight.enabled = value;
    }
}