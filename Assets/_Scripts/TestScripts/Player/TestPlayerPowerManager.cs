using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(TestPlayer))]
public class TestPlayerPowerManager : MonoBehaviour, IDebugManaged
{
    private TestPlayer _player;

    [SerializeField] private PowerScriptableObject[] powers;
    private Dictionary<PowerScriptableObject, PowerToken> _powerTokens;
    private HashSet<PowerScriptableObject> _drugsSet;
    private HashSet<PowerScriptableObject> _medsSet;

    private int _currentPowerIndex;

    private bool _isChargingPower;

    #region Getters

    public TestPlayer Player => _player;

    private PowerScriptableObject CurrentPower => powers[_currentPowerIndex];

    #endregion

    #region Initialization Functions

    private void Awake()
    {
        InitializeComponents();

        // Initialize the power collections
        InitializePowerCollections();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the input
        InitializeInput();

        // Add this to the debug managed objects
        DebugManager.Instance.AddDebugManaged(this);
    }

    private void InitializeComponents()
    {
        // Get the TestPlayer component
        _player = GetComponent<TestPlayer>();
    }


    private void InitializeInput()
    {
        InputManager.Instance.PlayerControls.GamePlay.Power.performed += OnPowerPerformed;
        InputManager.Instance.PlayerControls.GamePlay.Power.canceled += OnPowerCanceled;

        InputManager.Instance.PlayerControls.GamePlay.ChangePower.performed += OnPowerChanged;
    }


    private void InitializePowerCollections()
    {
        _drugsSet = new HashSet<PowerScriptableObject>();
        _medsSet = new HashSet<PowerScriptableObject>();
        _powerTokens = new Dictionary<PowerScriptableObject, PowerToken>();
        UpdatePowerCollections(powers);
    }

    #endregion


    #region Input Functions

    private void OnPowerChanged(InputAction.CallbackContext obj)
    {
        // Return if the powers array is empty
        if (powers.Length == 0)
            return;

        // Don't change the power if the current power is active
        if (_powerTokens[CurrentPower].IsActiveEffectOn)
            return;

        // Get the float value of the change power input
        var changePowerValue = obj.ReadValue<float>();

        Debug.Log($"{changePowerValue}");

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

        // Return if the power is currently cooling down
        if (_powerTokens[CurrentPower].IsCoolingDown)
            return;

        // Return if the power is currently active
        if (_powerTokens[CurrentPower].IsActiveEffectOn)
            return;

        // Set the is charging power flag to true
        _isChargingPower = true;

        // Call the current power's start charge method
        var startedChargingThisFrame = _powerTokens[CurrentPower].ChargePercentage == 0;
        CurrentPower.PowerLogic.StartCharge(this, _powerTokens[CurrentPower], startedChargingThisFrame);

        // Set the charging flag to true
        _powerTokens[CurrentPower].SetChargingFlag(true);
    }

    private void OnPowerCanceled(InputAction.CallbackContext obj)
    {
        // return if the current power is null
        if (CurrentPower == null)
            return;

        // Set the is charging power flag to false
        _isChargingPower = false;

        // Call the current power's release method
        var isChargeComplete = _powerTokens[CurrentPower].ChargePercentage >= 1;
        CurrentPower.PowerLogic.Release(this, _powerTokens[CurrentPower], isChargeComplete);

        // Set the charging flag to false
        _powerTokens[CurrentPower].SetChargingFlag(false);

        // Reset the charge duration if the power is not charging
        _powerTokens[CurrentPower].ResetChargeDuration();

        // If the charge is complete
        if (isChargeComplete)
        {
            // use the power
            CurrentPower.PowerLogic.Use(this, _powerTokens[CurrentPower]);

            // Set the active flag to true
            _powerTokens[CurrentPower].SetActiveFlag(true);

            // Reset the active duration
            _powerTokens[CurrentPower].ResetActiveDuration();

            // Set the passive flag to true
            _powerTokens[CurrentPower].SetPassiveFlag(true);

            // Reset the passive duration
            _powerTokens[CurrentPower].ResetPassiveDuration();

            // Start the active effect
            CurrentPower.PowerLogic.StartActiveEffect(this, _powerTokens[CurrentPower]);

            // Start the passive effect
            CurrentPower.PowerLogic.StartPassiveEffect(this, _powerTokens[CurrentPower]);
            
            // Change the player's tolerance
            _player.PlayerInfo.ChangeTolerance(_powerTokens[CurrentPower].ToleranceMeterImpact);
        }
    }

    #endregion

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
    }

    private void UpdateCharge()
    {
        // Skip if the current power is null
        if (CurrentPower == null)
            return;

        // Skip if the power is not charging
        if (!_isChargingPower)
            return;

        // Update the charge duration
        _powerTokens[CurrentPower].ChargePowerDuration();

        // Call the current power's charge method
        CurrentPower.PowerLogic.Charge(this, _powerTokens[CurrentPower]);
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
                _powerTokens[CurrentPower].SetCooldownFlag(true);

                // Set the cooldown duration to 0
                _powerTokens[CurrentPower].SetCooldownDuration(0);
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
                cToken.SetCooldownFlag(false);
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

        // Skip if the current power is already set or if there are no powers
        if (CurrentPower != null || powers.Length == 0)
            return;

        // Set the current power to the first power in the array
        _currentPowerIndex = 0;
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
        
        debugString.Append($"Tolerance: {_player.PlayerInfo.CurrentTolerance:0.00} / {_player.PlayerInfo.MaxTolerance:0.00} ({tolerancePercentage:0.00}%)\n\n");

        debugString.Append($"Current Power: {CurrentPower.name}\n");
        debugString.Append($"\tPurity (Level): {_powerTokens[CurrentPower].CurrentLevel}\n");
        debugString.Append($"\tTolerance Impact: {_powerTokens[CurrentPower].ToleranceMeterImpact}\n");

        // Charging Logic
        debugString.Append($"\tIs Charging? {_powerTokens[CurrentPower].IsCharging}\n");

        if (_powerTokens[CurrentPower].IsCharging)
        {
            debugString.Append($"\t\tCharge: {_powerTokens[CurrentPower].ChargePercentage * 100:0.00}%\n");
            debugString.Append(
                $"\t\tDuration: {_powerTokens[CurrentPower].CurrentChargeDuration:0.00}s / {CurrentPower.ChargeDuration:0.00}s\n");
        }

        // Active Logic
        debugString.Append($"\tActive Effect? {_powerTokens[CurrentPower].IsActiveEffectOn}\n");

        if (_powerTokens[CurrentPower].IsActiveEffectOn)
        {
            debugString.Append($"\t\tOn: {_powerTokens[CurrentPower].ActivePercentage * 100:0.00}%\n");
            debugString.Append(
                $"\t\tDuration: {_powerTokens[CurrentPower].CurrentActiveDuration:0.00}s / {CurrentPower.ActiveEffectDuration:0.00}s\n");
        }

        // Passive Logic
        debugString.Append($"\tPassive Effect? {_powerTokens[CurrentPower].IsActiveEffectOn}\n");

        if (_powerTokens[CurrentPower].IsPassiveEffectOn)
        {
            debugString.Append($"\t\tOn: {_powerTokens[CurrentPower].PassivePercentage * 100:0.00}%\n");
            debugString.Append(
                $"\t\tDuration: {_powerTokens[CurrentPower].CurrentPassiveDuration:0.00}s / {CurrentPower.PassiveEffectDuration:0.00}s\n");
        }

        // Cooldown Logic
        debugString.Append($"\tIs Cooling Down? {_powerTokens[CurrentPower].IsCoolingDown}\n");

        if (_powerTokens[CurrentPower].IsCoolingDown)
        {
            debugString.Append($"\t\tCooldown: {_powerTokens[CurrentPower].CooldownPercentage * 100:0.00}%\n");
            debugString.Append(
                $"\t\tDuration: {_powerTokens[CurrentPower].CurrentCooldownDuration:0.00}s / {CurrentPower.Cooldown:0.00}s\n");
        }

        return debugString.ToString();
    }

    #endregion
}