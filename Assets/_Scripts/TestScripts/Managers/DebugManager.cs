using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugManager : MonoBehaviour, IDebugManaged
{
    public static DebugManager Instance { get; private set; }

    public bool IsDebugMode { get; private set; }

    private HashSet<IDebugManaged> _debugManagedObjects;


    [Tooltip("A Canvas object to display debug text")] [SerializeField]
    private Canvas debugCanvas;

    [Tooltip("A TMP_Text object to display debug text")] [SerializeField]
    private TMP_Text debugText;


    #region Initialization Functions

    private void Awake()
    {
        // Set the instance to this
        Instance = this;

        // Create the debug managed objects hash set
        _debugManagedObjects = new HashSet<IDebugManaged>();

        // Add this to the debug managed objects
        AddDebugManaged(this);
    }

    // Start is called before the first frame update
    private void Start()
    {
        // Initialize the input
        InitializeInput();

        // Set the visibility of the debug text
        SetDebugVisibility(IsDebugMode);
    }

    private void InitializeInput()
    {
        // Toggle debug mode
        InputManager.Instance.PlayerControls.Debug.ToggleDebug.performed += ToggleDebugMode;
    }

    #endregion

    // Update is called once per frame
    private void Update()
    {
        // Update the text
        UpdateText();
    }

    private void UpdateText()
    {
        // If the debug text is null, return
        if (debugText == null)
            return;

        // Create a new string from the debug managed objects
        StringBuilder textString = new();
        foreach (var obj in _debugManagedObjects)
            textString.Append($"{obj.GetDebugText()}\n");

        // Set the text
        debugText.text = textString.ToString();
    }

    private void ToggleDebugMode(InputAction.CallbackContext ctx)
    {
        // Toggle the debug mode
        IsDebugMode = !IsDebugMode;

        // Set the debug text visibility
        SetDebugVisibility(IsDebugMode);
    }

    private void SetDebugVisibility(bool isVisible)
    {
        // Set the debug canvas's visibility
        debugCanvas.enabled = isVisible;
    }
    
    public void AddDebugManaged(IDebugManaged debugManaged)
    {
        // Add the debug managed object to the hash set
        _debugManagedObjects.Add(debugManaged);
    }
    
    public void RemoveDebugManaged(IDebugManaged debugManaged)
    {
        // Remove the debug managed object from the hash set
        _debugManagedObjects.Remove(debugManaged);
    }

    public string GetDebugText()
    {
        return "PRESS F1 TO TOGGLE DEBUG MODE\n";
    }
}