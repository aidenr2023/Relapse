using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActions : MonoBehaviour
{
    public Gun gun;

    private void OnEnable()
    {
        InputManager.Instance.PlayerControls.GamePlay.Shoot.performed += OnShoot;
    }

    private void OnDisable()
    {
        // Unbind the shoot action to avoid memory leaks
        InputManager.Instance.PlayerControls.GamePlay.Shoot.performed -= OnShoot;
    }

    private void OnShoot(InputAction.CallbackContext context)
    {
        // Call the gun's Shoot method when the shoot action is performed
        gun.Shoot();
    }
}
