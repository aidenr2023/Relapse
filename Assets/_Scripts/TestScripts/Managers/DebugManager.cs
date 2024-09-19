using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugManager : MonoBehaviour
{
    public static DebugManager Instance { get; private set; }

    public bool IsDebugMode { get; private set; }

    #region Initialization Functions

    private void Awake()
    {
        // Set the instance to this
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        InitializeInput();
    }

    private void InitializeInput()
    {
        // Toggle debug mode
        InputManager.Instance.PlayerControls.Debug.ToggleDebug.performed += ToggleDebugMode;
    }

    #endregion

    // Update is called once per frame
    void Update()
    {
    }

    private void ToggleDebugMode(InputAction.CallbackContext ctx)
    {
        IsDebugMode = !IsDebugMode;
        
        Debug.Log($"Debug Mode: {IsDebugMode}");
    }
}