using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugManagerHelper : MonoBehaviour, IDamager, IUsesInput, IDebugged
{
    #region Serialized Fields

    [Tooltip("A Canvas object to display debug text")] [SerializeField]
    private Canvas debugCanvas;

    [Tooltip("A TMP_Text object to display debug text")] [SerializeField]
    private TMP_Text debugText;

    [SerializeField] [Range(0, 1)] private float healthMult = 1f;
    [SerializeField] [Min(0)] private float toleranceMult = 10f;

    #endregion

    #region Private Fields

    private float _healthChange;
    private float _toleranceChange;

    #endregion

    #region Getters

    public GameObject GameObject => gameObject;

    public HashSet<InputData> InputActions { get; } = new();

    private Player Player => Player.Instance;

    #endregion

    #region Initialization Functions

    private void Awake()
    {
        // Initialize the input
        InitializeInput();

        // Add this to the debug manager
        DebugManager.Instance.AddDebuggedObject(this);
    }

    private void OnDestroy()
    {
        // Remove this from the debug manager
        DebugManager.Instance.RemoveDebuggedObject(this);
    }

    // Start is called before the first frame update
    private void Start()
    {
        // Set the visibility of the debug text
        SetDebugVisibility(DebugManager.Instance.IsDebugMode);
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
            new InputData(InputManager.Instance.OtherControls.Debug.ToggleDebug, InputType.Performed, ToggleDebugMode)
        );

        InputActions.Add(new InputData(
            InputManager.Instance.OtherControls.Debug.DebugHealth, InputType.Performed, OnDebugHealthPerformed)
        );
        InputActions.Add(new InputData(
            InputManager.Instance.OtherControls.Debug.DebugHealth, InputType.Canceled, OnDebugHealthCanceled)
        );

        InputActions.Add(new InputData(
            InputManager.Instance.OtherControls.Debug.DebugTolerance, InputType.Performed, OnDebugTolerancePerformed)
        );
        InputActions.Add(new InputData(
            InputManager.Instance.OtherControls.Debug.DebugTolerance, InputType.Canceled, OnDebugToleranceCanceled)
        );
    }

    #region Input Functions

    private void ToggleDebugMode(InputAction.CallbackContext ctx)
    {
        // Toggle the debug mode
        DebugManager.Instance.IsDebugMode = !DebugManager.Instance.IsDebugMode;

        // Set the debug text visibility
        SetDebugVisibility(DebugManager.Instance.IsDebugMode);
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

        if (Input.GetKeyDown(KeyCode.Comma))
            UserSettings.Instance.SetGamma(UserSettings.Instance.Gamma - 0.1f);
        if (Input.GetKeyDown(KeyCode.Period))
            UserSettings.Instance.SetGamma(UserSettings.Instance.Gamma + 0.1f);
    }

    private void UpdateToleranceAndHealth()
    {
        // If there is no player, return
        if (Player == null)
            return;

        Player.PlayerInfo.ChangeHealth(
            _healthChange * Time.unscaledDeltaTime *
            healthMult * Player.PlayerInfo.MaxHealth,
            Player.PlayerInfo, this, Player.transform.position
        );
        Player.PlayerInfo.ChangeTolerance(_toleranceChange * Time.unscaledDeltaTime * toleranceMult);
    }

    private void UpdateText()
    {
        // If the debug text is null, return
        if (debugText == null)
            return;

        // Create a new string from the debug managed objects
        StringBuilder textString = new();
        foreach (var obj in DebugManager.Instance.DebuggedObjects)
        {
            textString.Append(obj.GetDebugText());
            textString.Append('\n');
        }

        // Set the text
        debugText.text = textString.ToString();
    }

    private void SetDebugVisibility(bool isVisible)
    {
        // Set the debug canvas's visibility
        debugCanvas.enabled = isVisible;
    }

    public string GetDebugText()
    {
        var sb = new StringBuilder();

        // sb.Append($"Serialization Data Info:\n");
        //
        // foreach (var keyValue in LevelLoader.Instance.Data)
        // {
        //     var uniqueId = keyValue.Key;
        //
        //     sb.Append($"\tUniqueId: {uniqueId}\n");
        //
        //     foreach (var data in keyValue.Value)
        //         sb.Append($"\t\t{data.Key}: {data.Value}\n");
        // }

        return sb.ToString();
    }
}