using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugManager : MonoBehaviour, IDebugged, IDamager
{
    public static DebugManager Instance { get; private set; }

    public bool IsDebugMode { get; private set; }

    private HashSet<IDebugged> _debuggedObjects;


    [Tooltip("A Canvas object to display debug text")] [SerializeField]
    private Canvas debugCanvas;

    [Tooltip("A TMP_Text object to display debug text")] [SerializeField]
    private TMP_Text debugText;

    private Player _player;

    private float _healthChange;
    private float _toleranceChange;

    [SerializeField] [Min(0)] private float healthMult = 1f;
    [SerializeField] [Min(0)] private float toleranceMult = 10f;

    public GameObject GameObject => gameObject;


    #region Initialization Functions

    private void Awake()
    {
        // Set the instance to this
        Instance = this;

        // Create the debug managed objects hash set
        _debuggedObjects = new HashSet<IDebugged>();

        // Add this to the debug managed objects
        AddDebuggedObject(this);
    }

    // Start is called before the first frame update
    private void Start()
    {
        // Initialize the input
        InitializeInput();

        // // Set debug mode to true by default
        // IsDebugMode = true;

        // Set the visibility of the debug text
        SetDebugVisibility(IsDebugMode);

        // Find the player in the scene
        _player = FindFirstObjectByType<Player>();
    }

    private void InitializeInput()
    {
        // Toggle debug mode
        InputManager.Instance.PlayerControls.Debug.ToggleDebug.performed += ToggleDebugMode;

        InputManager.Instance.PlayerControls.Debug.DebugHealth.performed += OnDebugHealthPerformed;
        InputManager.Instance.PlayerControls.Debug.DebugTolerance.performed += OnDebugTolerancePerformed;

        InputManager.Instance.PlayerControls.Debug.DebugHealth.canceled += OnDebugHealthCanceled;
        InputManager.Instance.PlayerControls.Debug.DebugTolerance.canceled += OnDebugToleranceCanceled;
    }

    #endregion

    // Update is called once per frame
    private void Update()
    {
        // Update the text
        UpdateText();

        // Update the tolerance and health
        UpdateToleranceAndHealth();
    }

    private void UpdateToleranceAndHealth()
    {
        _player.PlayerInfo.ChangeHealth(_healthChange * Time.deltaTime * healthMult, _player.PlayerInfo, this);
        _player.PlayerInfo.ChangeTolerance(_toleranceChange * Time.deltaTime * toleranceMult);
    }

    private void UpdateText()
    {
        // If the debug text is null, return
        if (debugText == null)
            return;

        // Create a new string from the debug managed objects
        StringBuilder textString = new();
        foreach (var obj in _debuggedObjects)
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


    private void OnDebugHealthPerformed(InputAction.CallbackContext context)
    {
        _healthChange = context.ReadValue<float>();
    }

    private void OnDebugHealthCanceled(InputAction.CallbackContext context)
    {
        _healthChange = 0;
    }

    private void OnDebugTolerancePerformed(InputAction.CallbackContext context)
    {
        _toleranceChange = context.ReadValue<float>();
    }

    private void OnDebugToleranceCanceled(InputAction.CallbackContext context)
    {
        _toleranceChange = 0;
    }

    private void SetDebugVisibility(bool isVisible)
    {
        // Set the debug canvas's visibility
        debugCanvas.enabled = isVisible;
    }

    public void AddDebuggedObject(IDebugged debugged)
    {
        // Add the debug managed object to the hash set
        _debuggedObjects.Add(debugged);
    }

    public void RemoveDebugManaged(IDebugged debugged)
    {
        // Remove the debug managed object from the hash set
        _debuggedObjects.Remove(debugged);
    }

    public string GetDebugText()
    {
        return "PRESS F1 TO TOGGLE DEBUG MODE\n";
    }
}