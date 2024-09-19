using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(TestPlayer))]
public class TestPlayerPowerManager : MonoBehaviour, IDebugManaged
{
    [SerializeField] private PowerScriptableObject[] powers;
    private Dictionary<PowerScriptableObject, PowerToken> _powerTokens;
    private HashSet<PowerScriptableObject> _drugsSet;
    private HashSet<PowerScriptableObject> _medsSet;

    [SerializeField] private PowerScriptableObject currentPower;

    private bool _isChargingPower;

    #region Initialization Functions

    private void Awake()
    {
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

    private void InitializeInput()
    {
        InputManager.Instance.PlayerControls.GamePlay.Brand.performed += OnPowerPerformed;
        InputManager.Instance.PlayerControls.GamePlay.Brand.canceled += OnPowerCanceled;
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

    private void OnPowerPerformed(InputAction.CallbackContext obj)
    {
        // Return if the current power is null
        if (currentPower == null)
            return;

        // Set the is charging power flag to true
        _isChargingPower = true;

        // Call the current power's start charge method
        var startedChargingThisFrame = _powerTokens[currentPower].ChargePercentage == 0;
        currentPower.PowerLogic.StartCharge(this, _powerTokens[currentPower], startedChargingThisFrame);
    }

    private void OnPowerCanceled(InputAction.CallbackContext obj)
    {
        // return if the current power is null
        if (currentPower == null)
            return;

        // Set the is charging power flag to false
        _isChargingPower = false;

        // Call the current power's release method
        var isChargeComplete = _powerTokens[currentPower].ChargePercentage >= 1;
        currentPower.PowerLogic.Release(this, _powerTokens[currentPower], isChargeComplete);

        // If the charge is complete, use the power
        if (isChargeComplete)
            currentPower.PowerLogic.Use(this, _powerTokens[currentPower]);
    }

    #endregion

    // Update is called once per frame
    void Update()
    {
        // Update the charge
        UpdateCharge();
    }

    private void UpdateCharge()
    {
        // Skip if the current power is null
        if (currentPower == null)
            return;

        // Skip if the power is not charging
        if (!_isChargingPower)
            return;

        // Call the current power's charge method
        currentPower.PowerLogic.Charge(this, _powerTokens[currentPower]);
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
        if (currentPower != null || powers.Length == 0)
            return;

        // Set the current power to the first power in the array
        currentPower = powers[0];
    }

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
        if (currentPower == null)
            return "No Power Selected!\n";

        StringBuilder debugString = new();

        debugString.Append($"Current Power: {currentPower.name}\n");
        debugString.Append($"\tIs Charging? {_powerTokens[currentPower].IsCharging}\n");

        if (_powerTokens[currentPower].IsCharging)
            debugString.Append($"\tCharge: {_powerTokens[currentPower].ChargePercentage * 100:0.00}\n");

        debugString.Append($"\tCooldown: {_powerTokens[currentPower].CooldownPercentage * 100:0.00}\n");

        return debugString.ToString();
    }
}