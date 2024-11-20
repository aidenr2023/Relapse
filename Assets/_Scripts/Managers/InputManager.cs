using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public PlayerControls PlayerControls { get; private set; }

    private HashSet<IUsesInput> _registeredItems;

    #region Initialization Functions

    private void Awake()
    {
        // Set the instance to this
        Instance = this;

        // Create a new instance of the PlayerControls
        PlayerControls = new PlayerControls();

        // Initialize the registered items
        _registeredItems = new HashSet<IUsesInput>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Hide & Lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    #endregion

    private void OnEnable()
    {
        // Enable the PlayerControls
        PlayerControls.Enable();
    }

    private void OnDisable()
    {
        // Disable the PlayerControls
        PlayerControls.Disable();
    }

    public void Register(IUsesInput item)
    {
        // If the item is already in the registered items, return
        if (!_registeredItems.Add(item))
            return;

        // Add the item to the registered items
        item.InitializeInput();
    }

    public void Unregister(IUsesInput item)
    {
        // If the item is not in the registered items, return
        if (!_registeredItems.Remove(item))
            return;

        item.RemoveInput();
    }
}