using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.VFX;

[RequireComponent(typeof(Player))]
public class PlayerPowerManager : MonoBehaviour, IDebugged, IUsesInput
{
    public static PlayerPowerManager Instance { get; private set; }

    #region Serialized Fields

    [SerializeField] private PowerScriptableObject[] powers;


    [Header("Power Charged Vignette"), SerializeField, Range(0, 1)]
    private float chargedVignetteStrength = .25f;

    [SerializeField, Range(0, 1)] private float chargedVignetteLerpAmount = .25f;
    [SerializeField, Min(0)] private float chargedVignetteFlashesPerSecond = 1f;

    [Header("Visual Effects")] [SerializeField]
    private VisualEffect gauntletChargeVfx;

    #endregion

    public event Action<PlayerPowerManager, PowerToken> OnPowerUsed;

    #region Private Fields

    private Player _player;

    private Dictionary<PowerScriptableObject, PowerToken> _powerTokens;
    private HashSet<PowerScriptableObject> _drugsSet;
    private HashSet<PowerScriptableObject> _medsSet;

    private int _currentPowerIndex;
    private bool _isChargingPower;

    private TokenManager<float>.ManagedToken _powerChargeVignetteToken;

    #endregion

    #region Getters

    public HashSet<InputData> InputActions { get; } = new();

    public Player Player => _player;

    public PowerScriptableObject CurrentPower => powers.Length > 0 ? powers[_currentPowerIndex] : null;

    public PowerToken CurrentPowerToken =>
        CurrentPower != null
            ? _powerTokens.GetValueOrDefault(CurrentPower)
            : null;

    public IReadOnlyCollection<PowerScriptableObject> Powers => powers;

    public int CurrentPowerIndex => _currentPowerIndex;

    #endregion

    #region Initialization Functions

    private void Awake()
    {
        // Set the instance to this
        Instance = this;

        InitializeComponents();

        // Initialize the power collections
        InitializePowerCollections();

        // Initialize the input
        InitializeInput();
    }

    // Start is called before the first frame update
    private void Start()
    {
        // Initialize the events
        InitializeEvents();

        // // Add this to the debug managed objects
        // DebugManager.Instance.AddDebuggedObject(this);

        // Initialize the vignette token
        _powerChargeVignetteToken = PostProcessingVolumeController.Instance.VignetteModule.Tokens.AddToken(0, -1, true);

        // Add this to the debug manager
        DebugManager.Instance.AddDebuggedObject(this);
    }

    private void OnEnable()
    {
        // Register this with the input manager
        InputManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        // Unregister this with the input manager
        InputManager.Instance.Unregister(this);

        // Remove this from the debug manager
        DebugManager.Instance.RemoveDebuggedObject(this);
    }

    private void InitializeComponents()
    {
        // Get the TestPlayer component
        _player = GetComponent<Player>();
    }

    private void InitializeEvents()
    {
        _player.PlayerInfo.onRelapseStart += OnRelapseStart;

        // Add the event for the power used
        OnPowerUsed += PlaySoundOnUse;
        OnPowerUsed += DisplayTooltipOnUse;
    }

    public void InitializeInput()
    {
        InputActions.Add(
            new InputData(InputManager.Instance.PControls.Player.Power, InputType.Performed, OnPowerPerformed)
        );
        InputActions.Add(
            new InputData(InputManager.Instance.PControls.Player.Power, InputType.Canceled, OnPowerCanceled)
        );

        InputActions.Add(
            new InputData(InputManager.Instance.PControls.Player.ChangePower, InputType.Performed, OnPowerChanged)
        );
    }


    private void InitializePowerCollections()
    {
        _drugsSet = new HashSet<PowerScriptableObject>();
        _medsSet = new HashSet<PowerScriptableObject>();
        _powerTokens = new Dictionary<PowerScriptableObject, PowerToken>();
        UpdatePowerCollections(powers);
    }


    private void OnRelapseStart(PlayerInfo obj)
    {
        // Force the power to stop charging
        StopCharge();
    }

    #endregion

    #region Input Functions

    private void OnPowerChanged(InputAction.CallbackContext obj)
    {
        // Return if the powers array is empty
        if (powers.Length == 0)
            return;

        // Don't change the power if the current power is active
        if (CurrentPowerToken.IsActiveEffectOn)
            return;

        // Reset the charge if the power is charging
        if (_isChargingPower)
            StopCharge();

        // Get the float value of the change power input
        var changePowerValue = obj.ReadValue<float>();

        // Scroll up or down based on the input
        var direction = changePowerValue > 0 ? 1 : -1;

        // Set the current power index to the next power
        _currentPowerIndex = (_currentPowerIndex + direction) % powers.Length;
        if (_currentPowerIndex < 0)
            _currentPowerIndex += powers.Length;
    }

    private void OnPowerPerformed(InputAction.CallbackContext obj)
    {
        // Return if the current power is null
        if (CurrentPower == null)
            return;

        // Return if the current power token is null
        if (CurrentPowerToken == null)
            return;

        // Return if the power is currently cooling down
        if (CurrentPowerToken.IsCoolingDown)
            return;

        // Return if the power is currently active
        if (CurrentPowerToken.IsActiveEffectOn)
            return;

        // Skip if the player is currently relapsing
        if (_player.PlayerInfo.IsRelapsing)
            return;

        // Set the is charging power flag to true
        _isChargingPower = true;

        // Call the current power's start charge method
        var startedChargingThisFrame = CurrentPowerToken.ChargePercentage == 0;
        CurrentPower.PowerLogic.StartCharge(this, CurrentPowerToken, startedChargingThisFrame);

        // Set the charging flag to true
        CurrentPowerToken.SetChargingFlag(true);

        // Play the charge start sound
        SoundManager.Instance.PlaySfx(CurrentPower.ChargeStartSound);
    }

    private void OnPowerCanceled(InputAction.CallbackContext obj)
    {
        // return if the current power is null
        if (CurrentPower == null)
            return;

        var isChargeComplete = StopCharge();

        // If the charge is complete
        if (isChargeComplete)
            UsePower();
    }

    #endregion

    #region Update Methods

    // Update is called once per frame
    private void Update()
    {
        // Update the charge
        UpdateCharge();

        // Update the active powers
        UpdateActivePowers();

        // Update the passive powers
        UpdatePassivePowers();

        // Update the cooldowns
        UpdateCooldowns();

        // Update the vignette token
        UpdateVignetteToken();

        // Update the gauntlet charge VFX
        UpdateGauntletChargeVFX();
    }

    private void UpdateCharge()
    {
        // Skip if the current power is null
        if (CurrentPower == null)
            return;

        // Return if the current power token is null
        if (CurrentPowerToken == null)
            return;

        // Skip if the power is not charging
        if (!_isChargingPower)
            return;

        // Update the charge duration
        CurrentPowerToken.ChargePowerDuration();

        // Call the current power's charge method
        CurrentPower.PowerLogic.Charge(this, CurrentPowerToken);
    }

    private void UpdateActivePowers()
    {
        foreach (var power in _powerTokens.Keys)
        {
            // Get the current power token
            var cToken = _powerTokens[power];

            // Skip if the power is not active
            if (!cToken.IsActiveEffectOn)
                continue;

            // Update the active duration
            cToken.ActivePowerDuration();

            // Call the current power's active method
            power.PowerLogic.UpdateActiveEffect(this, cToken);

            // If the active percentage is 1, set the active flag to false
            if (cToken.ActivePercentage >= 1)
            {
                cToken.SetActiveFlag(false);

                // Call the current power's end active method
                power.PowerLogic.EndActiveEffect(this, cToken);

                // Set the cooldown flag to true
                CurrentPowerToken.SetCooldownFlag(true);

                // Set the cooldown duration to 0
                CurrentPowerToken.SetCooldownDuration(0);
            }
        }
    }

    private void UpdatePassivePowers()
    {
        foreach (var power in _powerTokens.Keys)
        {
            // Get the current power token
            var cToken = _powerTokens[power];

            // Skip if the power's passive effect is not active
            if (!cToken.IsPassiveEffectOn)
                continue;

            // Update the passive duration
            cToken.PassivePowerDuration();

            // Call the current power's passive method
            power.PowerLogic.UpdatePassiveEffect(this, cToken);

            // If the passive percentage is 1, 
            if (cToken.PassivePercentage >= 1)
            {
                // set the passive flag to false
                cToken.SetPassiveFlag(false);

                // Call the current power's end passive method
                power.PowerLogic.EndPassiveEffect(this, cToken);
            }
        }
    }

    private void UpdateCooldowns()
    {
        foreach (var power in _powerTokens.Keys)
        {
            // Get the current power token
            var cToken = _powerTokens[power];

            // Skip if the power is not cooling down
            if (!cToken.IsCoolingDown)
                continue;

            // Skip if the power is active (Redundant, but here to be safe)
            if (cToken.IsActiveEffectOn)
                continue;

            // Update the cooldown
            cToken.CooldownPowerDuration();

            // If the cooldown percentage is 1, set the cooldown flag to false
            // TODO: Create an event or something for when the cooldown stops
            if (cToken.CooldownPercentage >= 1)
            {
                cToken.SetCooldownFlag(false);

                // Play the power ready sound
                SoundManager.Instance.PlaySfx(power.PowerReadySound);
            }
        }
    }

    /// <summary>
    /// A method to update the power collections properly
    /// </summary>
    private void UpdatePowerCollections(params PowerScriptableObject[] newPowers)
    {
        // Condense the powers array by trimming the null values at the end
        int removeAmount;
        for (removeAmount = 0; removeAmount < powers.Length; removeAmount++)
        {
            if (powers[powers.Length - 1 - removeAmount] != null)
                break;
        }

        // Remove the null values from the end of the array if removeAmount is greater than 0
        if (removeAmount > 0)
            Array.Resize(ref powers, powers.Length - removeAmount);

        // Loop through all the new powers
        foreach (var power in newPowers)
        {
            if (power == null)
                continue;

            // Determine which collection to add the power to
            var addSet = power.PowerType switch
            {
                PowerType.Drug => _drugsSet,
                PowerType.Medicine => _medsSet,
                _ => throw new ArgumentOutOfRangeException()
            };

            // Add the power to the correct collection
            addSet.Add(power);

            // Add the power to the power usage tokens
            _powerTokens.Add(power, new PowerToken(power));
        }

        // clamp the current power index to the new powers array
        _currentPowerIndex = Mathf.Clamp(_currentPowerIndex, 0, powers.Length - 1);

        // Skip if the current power is already set or if there are no powers
        if (CurrentPower != null || powers.Length == 0)
            return;

        // Set the current power to the first power in the array
        _currentPowerIndex = 0;
    }

    private void UpdateVignetteToken()
    {
        var targetValue = 0f;

        // If the player's power is currently fully charged, set the target value to the charged vignette strength
        if (CurrentPowerToken != null && CurrentPowerToken.ChargePercentage >= 1)
            targetValue = chargedVignetteStrength;

        const float defaultFrameTime = 1 / 60f;
        var frameAmount = Time.deltaTime / defaultFrameTime;

        // Sine wave with a y from 0 to 1
        var sineWave = Mathf.Sin(chargedVignetteFlashesPerSecond * Time.time * Mathf.PI * 2 + Mathf.PI / 2) / 2 + 0.5f;

        // Lerp the vignette value to the target value
        _powerChargeVignetteToken.Value = Mathf.Lerp(
            _powerChargeVignetteToken.Value * sineWave,
            targetValue,
            chargedVignetteLerpAmount * frameAmount
        );
    }

    private void UpdateGauntletChargeVFX()
    {
        uint chargeState = 0;

        if (_isChargingPower && CurrentPower != null && CurrentPowerToken != null)
        {
            // If the player is currently charging power, but it is not fully complete, set the charge state to 1
            if (CurrentPowerToken.ChargePercentage < 1)
                chargeState = 1;

            // If the player's power is currently fully charged, set the charge state to 2
            else
                chargeState = 2;
        }

        // Set the "ChargeState" uint property of the VFX graph
        gauntletChargeVfx.SetUInt("ChargeState", chargeState);
    }

    #endregion

    private bool StopCharge()
    {
        // Set the is charging power flag to false
        _isChargingPower = false;

        // Return if the current power token is null
        if (CurrentPowerToken == null)
            return false;

        // Call the current power's release method
        var isChargeComplete = CurrentPowerToken.ChargePercentage >= 1;
        CurrentPower.PowerLogic.Release(this, CurrentPowerToken, isChargeComplete);

        // Set the charging flag to false
        CurrentPowerToken.SetChargingFlag(false);

        // Reset the charge duration if the power is not charging
        CurrentPowerToken.ResetChargeDuration();

        return isChargeComplete;
    }

    private void UsePower()
    {
        // Skip if the player is currently relapsing
        if (_player.PlayerInfo.IsRelapsing)
            return;

        // use the power
        CurrentPower.PowerLogic.Use(this, CurrentPowerToken);

        // Set the active flag to true
        CurrentPowerToken.SetActiveFlag(true);

        // Reset the active duration
        CurrentPowerToken.ResetActiveDuration();

        // Set the passive flag to true
        CurrentPowerToken.SetPassiveFlag(true);

        // Reset the passive duration
        CurrentPowerToken.ResetPassiveDuration();

        // After using the power, reset the charge duration
        CurrentPowerToken.ResetChargeDuration();

        // Start the active effect
        CurrentPower.PowerLogic.StartActiveEffect(this, CurrentPowerToken);

        // Start the passive effect
        CurrentPower.PowerLogic.StartPassiveEffect(this, CurrentPowerToken);

        // Change the player's tolerance
        _player.PlayerInfo.ChangeTolerance(CurrentPowerToken.ToleranceMeterImpact);
        // StartCoroutine(
        //     AddToxicityOverTime(
        //         CurrentPowerToken.ToleranceMeterImpact,
        //         .5f)
        // );

        // Invoke the event for the power used
        OnPowerUsed?.Invoke(this, CurrentPowerToken);
    }

    public void ResetPlayer()
    {
        // Reset each power token
        foreach (var powerToken in _powerTokens.Values)
            powerToken.Reset();

        // Set the is charging power flag to false
        _isChargingPower = false;
    }

    public void SetUpPowers(int powerIndex, PowerScriptableObject[] newPowers)
    {
        // Clone the powers array
        powers = new PowerScriptableObject[newPowers.Length];
        Array.Copy(newPowers, powers, newPowers.Length);

        // Initialize the power collections
        InitializePowerCollections();

        // Set the current power index
        _currentPowerIndex = powerIndex;
    }

    #region Event Functions

    private static void PlaySoundOnUse(PlayerPowerManager powerManager, PowerToken powerToken)
    {
        // Play the power use sound
        SoundManager.Instance.PlaySfx(powerToken.PowerScriptableObject.PowerUseSound);
    }

    private static void DisplayTooltipOnUse(PlayerPowerManager powerManager, PowerToken powerToken)
    {
        var powerName = powerToken.PowerScriptableObject.PowerName;

        // Display a tooltip for the active effect (if applicable)
        if (powerToken.PowerScriptableObject.ActiveEffectDuration > 0)
        {
            // The function to display the tooltip
            string TextFunction()
            {
                var activeDurationRemaining =
                    powerToken.PowerScriptableObject.ActiveEffectDuration - powerToken.CurrentActiveDuration;

                return $"{powerName}'s Active Effect: {activeDurationRemaining:0.00}s Remaining!";
            }

            bool CompletionFunction() => powerToken.ActivePercentage >= 1;

            // Calculate how long the tooltip should be displayed
            var duration =
                powerToken.PowerScriptableObject.ActiveEffectDuration
                - JournalTooltipManager.Instance.IntroDuration
                - JournalTooltipManager.Instance.OutroDuration;

            // Add the tooltip
            JournalTooltipManager.Instance.AddTooltip(TextFunction, duration, true, CompletionFunction);
        }

        // Display a tooltip for the passive effect (if applicable)
        if (powerToken.PowerScriptableObject.PassiveEffectDuration > 0)
        {
            // The function to display the tooltip
            string TextFunction()
            {
                var passiveDurationRemaining =
                    powerToken.PowerScriptableObject.PassiveEffectDuration - powerToken.CurrentPassiveDuration;

                return $"{powerName}'s Passive Effect: {passiveDurationRemaining:0.00}s Remaining!";
            }

            bool CompletionFunction() => powerToken.PassivePercentage >= 1;

            // Calculate how long the tooltip should be displayed
            var duration =
                powerToken.PowerScriptableObject.PassiveEffectDuration
                - JournalTooltipManager.Instance.IntroDuration
                - JournalTooltipManager.Instance.OutroDuration;

            // Add the tooltip
            JournalTooltipManager.Instance.AddTooltip(TextFunction, duration, true, CompletionFunction);
        }
    }

    #endregion

    private IEnumerator AddToxicityOverTime(float amount, float duration)
    {
        // Calculate the amount to add each frame
        var amountPerSecond = amount / duration;

        var totalAdded = 0f;

        // Loop through the duration
        for (var i = 0f; i < duration; i += Time.deltaTime)
        {
            // Add the toxicity
            _player.PlayerInfo.ChangeTolerance(amountPerSecond * Time.deltaTime);

            totalAdded += amountPerSecond * Time.deltaTime;

            // Wait for the next frame
            yield return null;
        }

        var amountRemaining = amount - totalAdded;

        // Add the remaining toxicity
        _player.PlayerInfo.ChangeTolerance(amountRemaining);
    }

    #region Public Methods

    public void AddPower(PowerScriptableObject powerScriptableObject)
    {
        // Check if the power is already in one of the hash sets
        var powerSet = powerScriptableObject.PowerType switch
        {
            PowerType.Drug => _drugsSet,
            PowerType.Medicine => _medsSet,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (!powerSet.Add(powerScriptableObject))
            return;

        // Add the power to the end of the powers array
        Array.Resize(ref powers, powers.Length + 1);
        powers[^1] = powerScriptableObject;

        // Update the power collections
        UpdatePowerCollections(powerScriptableObject);

        // Display a tooltip for the power
        JournalTooltipManager.Instance.AddTooltip(
            $"New {powerScriptableObject.PowerType}: {powerScriptableObject.PowerName}"
        );
    }

    public void RemovePower(PowerScriptableObject powerScriptableObject)
    {
        // Check if the power is already in one of the hash sets
        var powerSet = powerScriptableObject.PowerType switch
        {
            PowerType.Drug => _drugsSet,
            PowerType.Medicine => _medsSet,
            _ => throw new ArgumentOutOfRangeException()
        };

        // Return if the player does not have the power
        if (!powerSet.Remove(powerScriptableObject))
            return;

        // Remove the power from the powers array
        for (int i = 0; i < powers.Length; i++)
        {
            // Look for the power in the array
            if (powers[i] != powerScriptableObject)
                continue;

            // Remove the power from the array by shifting all the elements to the left
            for (int j = i; j < powers.Length - 1; j++)
                powers[j] = powers[j + 1];

            // Resize the array
            Array.Resize(ref powers, powers.Length - 1);

            break;
        }

        // Remove the associated power token
        _powerTokens.Remove(powerScriptableObject);

        // Update the power collections
        UpdatePowerCollections();
    }

    public bool HasPower(PowerScriptableObject powerScriptableObject)
    {
        // Check if the power is already in one of the hash sets
        var powerSet = powerScriptableObject.PowerType switch
        {
            PowerType.Drug => _drugsSet,
            PowerType.Medicine => _medsSet,
            _ => throw new ArgumentOutOfRangeException()
        };

        return powerSet.Contains(powerScriptableObject);
    }

    public PowerToken GetPowerToken(PowerScriptableObject powerScriptableObject)
    {
        if (powerScriptableObject == null)
            return null;

        return _powerTokens.GetValueOrDefault(powerScriptableObject);
    }

    public void SetPowerLevel(PowerScriptableObject powerScriptableObject, int level)
    {
        // Get the power token
        var powerToken = GetPowerToken(powerScriptableObject);

        // Return if the power token is null
        if (powerToken == null)
            return;

        // Set the power level
        powerToken.SetPowerLevel(level);
    }

    public string GetDebugText()
    {
        if (CurrentPower == null)
            return "No Power Selected!\n";

        StringBuilder debugString = new();

        float tolerancePercentage;
        if (_player.PlayerInfo.MaxTolerance == 0)
            tolerancePercentage = 0;
        else
            tolerancePercentage = _player.PlayerInfo.CurrentTolerance / _player.PlayerInfo.MaxTolerance * 100;

        debugString.Append(
            $"Tolerance: {_player.PlayerInfo.CurrentTolerance:0.00} / {_player.PlayerInfo.MaxTolerance:0.00} ({tolerancePercentage:0.00}%)\n\n");

        debugString.Append($"Current Power: {CurrentPower.name}\n");
        // debugString.Append($"\tPurity (Level): {CurrentPowerToken.CurrentLevel}\n");
        debugString.Append($"\tTolerance Impact: {CurrentPowerToken.ToleranceMeterImpact}\n");

        // Charging Logic
        debugString.Append($"\tIs Charging? {CurrentPowerToken.IsCharging}\n");

        if (CurrentPowerToken.IsCharging)
        {
            debugString.Append($"\t\tCharge: {CurrentPowerToken.ChargePercentage * 100:0.00}%\n");
            debugString.Append(
                $"\t\tDuration: {CurrentPowerToken.CurrentChargeDuration:0.00}s / {CurrentPower.ChargeDuration:0.00}s\n");
        }

        // Active Logic
        debugString.Append($"\tActive Effect? {CurrentPowerToken.IsActiveEffectOn}\n");

        if (CurrentPowerToken.IsActiveEffectOn)
        {
            debugString.Append($"\t\tOn: {CurrentPowerToken.ActivePercentage * 100:0.00}%\n");
            debugString.Append(
                $"\t\tDuration: {CurrentPowerToken.CurrentActiveDuration:0.00}s / {CurrentPower.ActiveEffectDuration:0.00}s\n");
        }

        // Passive Logic
        debugString.Append($"\tPassive Effect? {CurrentPowerToken.IsActiveEffectOn}\n");

        if (CurrentPowerToken.IsPassiveEffectOn)
        {
            debugString.Append($"\t\tOn: {CurrentPowerToken.PassivePercentage * 100:0.00}%\n");
            debugString.Append(
                $"\t\tDuration: {CurrentPowerToken.CurrentPassiveDuration:0.00}s / {CurrentPower.PassiveEffectDuration:0.00}s\n");
        }

        // Cooldown Logic
        debugString.Append($"\tIs Cooling Down? {CurrentPowerToken.IsCoolingDown}\n");

        if (CurrentPowerToken.IsCoolingDown)
        {
            debugString.Append($"\t\tCooldown: {CurrentPowerToken.CooldownPercentage * 100:0.00}%\n");
            debugString.Append(
                $"\t\tDuration: {CurrentPowerToken.CurrentCooldownDuration:0.00}s / {CurrentPower.Cooldown:0.00}s\n");
        }

        return debugString.ToString();
    }

    #endregion
}