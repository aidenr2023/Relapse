using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public PlayerControls PlayerControls { get; private set; }

    #region Initialization Functions

    private void Awake()
    {
        // Set the instance to this
        Instance = this;

        // Create a new instance of the PlayerControls
        PlayerControls = new PlayerControls();
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

    // Update is called once per frame
    void Update()
    {
    }
}