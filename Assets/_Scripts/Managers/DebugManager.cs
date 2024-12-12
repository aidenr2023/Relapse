using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugManager : MonoBehaviour, IDebugged, IDamager, IUsesInput
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

    [SerializeField] [Range(0, 1)] private float healthMult = 1f;
    [SerializeField] [Min(0)] private float toleranceMult = 10f;

    public GameObject GameObject => gameObject;

    public HashSet<InputData> InputActions { get; } = new();


    #region Initialization Functions

    private void Awake()
    {
        // Set the instance to this
        Instance = this;

        // Create the debug managed objects hash set
        _debuggedObjects = new HashSet<IDebugged>();

        // Add this to the debug managed objects
        AddDebuggedObject(this);

        // Initialize the input
        InitializeInput();
    }

    // Start is called before the first frame update
    private void Start()
    {
        // // Set debug mode to true by default
        // IsDebugMode = true;

        // Set the visibility of the debug text
        SetDebugVisibility(IsDebugMode);

        // Find the player in the scene
        _player = FindFirstObjectByType<Player>();
    }

    private void OnEnable()
    {
        // Register this to the input manager
        InputManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        // Unregister this from the input manager
        InputManager.Instance.Unregister(this);
    }

    public void InitializeInput()
    {
        InputActions.Add(
            new InputData(InputManager.Instance.PControls.Debug.ToggleDebug, InputType.Performed, ToggleDebugMode)
        );

        InputActions.Add(new InputData(
            InputManager.Instance.PControls.Debug.DebugHealth, InputType.Performed, OnDebugHealthPerformed)
        );
        InputActions.Add(new InputData(
            InputManager.Instance.PControls.Debug.DebugHealth, InputType.Canceled, OnDebugHealthCanceled)
        );

        InputActions.Add(new InputData(
            InputManager.Instance.PControls.Debug.DebugTolerance, InputType.Performed, OnDebugTolerancePerformed)
        );
        InputActions.Add(new InputData(
            InputManager.Instance.PControls.Debug.DebugTolerance, InputType.Canceled, OnDebugToleranceCanceled)
        );
    }

    #region Input Functions

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

    #endregion

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
        _player.PlayerInfo.ChangeHealth(
            _healthChange * Time.deltaTime *
            healthMult * _player.PlayerInfo.MaxHealth
            , _player.PlayerInfo, this);
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

    public void RemoveDebuggedObject(IDebugged debugged)
    {
        // Remove the debug managed object from the hash set
        _debuggedObjects.Remove(debugged);
    }

    public string GetDebugText()
    {
        return "PRESS F1 TO TOGGLE DEBUG MODE\n";
    }
}