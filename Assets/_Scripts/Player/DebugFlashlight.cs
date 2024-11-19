using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugFlashlight : MonoBehaviour
{
    [SerializeField] private Light debugFlashlight;

    [SerializeField] private bool canBeEnabled = true;

    public bool IsFlashlightOn => debugFlashlight.enabled;

    private void Awake()
    {
        // Ensure the flashlight is off when the game starts.
        SetFlashlight(false);
    }

    private void Start()
    {
        InputManager.Instance.PlayerControls.Debug.Flashlight.performed += ToggleFlashlightOnInput;
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