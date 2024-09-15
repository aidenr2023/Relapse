using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActions : MonoBehaviour
{
    public Gun gun;
    private MouseControls mc;

    private void Awake()
    {
        // Initialize the input actions
        mc = new MouseControls();
    }

    private void OnEnable()
    {
        // Enable the input actions
        mc.Mouse.Enable();

        // Bind the shoot action
        mc.Mouse.Shoot.performed += OnShoot;
    }

    private void OnDisable()
    {
        // Unbind the shoot action to avoid memory leaks
        mc.Mouse.Shoot.performed -= OnShoot;

        // Disable the input actions
        mc.Mouse.Disable();
    }

    private void OnShoot(InputAction.CallbackContext context)
    {
        // Call the gun's Shoot method when the shoot action is performed
        gun.Shoot();
    }
}
