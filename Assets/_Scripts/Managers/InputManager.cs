using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    #region Private Fields

    private HashSet<IUsesInput> _registeredItems;

    #endregion

    #region Getters

    public PlayerControls PlayerControls { get; private set; }

    private bool CursorActive => PauseManager.Instance.IsPaused || VendorMenu.Instance.IsVendorActive;

    #endregion

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

    private void Update()
    {
        SetCursorState(CursorActive);
    }

    #region Public Methods

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

        // // Enable / disable the player controls
        // if (state)
        //     PlayerControls.Disable();
        // else
        //     PlayerControls.Enable();
    }

    #endregion
}