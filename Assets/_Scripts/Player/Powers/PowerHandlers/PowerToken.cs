using System.Collections.Generic;
using UnityEngine;

public class PowerToken
{
    /// <summary>
    /// A reference to the power scriptable object that this token is associated with.
    /// </summary>
    private readonly PowerScriptableObject _powerScriptableObject;

    /// <summary>
    /// The current level of the power.
    /// </summary>
    private int _currentLevel;

    /// <summary>
    /// Flag to determine if the power is charging.
    /// </summary>
    private bool _isCharging;

    /// <summary>
    /// How far along the charge is.
    /// </summary>
    private float _currentCurrentChargeDuration;

    /// <summary>
    /// A flag to determine if the power is active.
    /// </summary>
    private bool _isActiveEffectOn;

    /// <summary>
    /// How far along the active duration is.
    /// </summary>
    private float _currentCurrentActiveDuration;

    /// <summary>
    /// A flag to determine if the power is passively affecting the player.
    /// </summary>
    private bool _isPassiveEffectOn;

    /// <summary>
    /// How far along the passive duration is.
    /// </summary>
    private float _currentPassiveDuration;

    /// <summary>
    /// Flag to determine if the power is cooling down.
    /// </summary>
    private bool _isCoolingDown;

    /// <summary>
    /// How far along the cooldown is.
    /// </summary>
    private float _currentCooldownDuration;

    private Dictionary<string, object> _dataDictionary;

    #region Getters

    public PowerScriptableObject PowerScriptableObject => _powerScriptableObject;
    public int CurrentLevel => Mathf.Clamp(_currentLevel, 0, _powerScriptableObject.MaxLevel);

    public float ToleranceMeterImpact => _powerScriptableObject.BaseToleranceMeterImpact *
                                         _powerScriptableObject.ToleranceMeterLevelMultiplier[CurrentLevel] *
                                         PowerScriptableObject.PowerType.ToleranceMultiplier();

    public bool IsCharging => _isCharging;
    public float ChargePercentage => _currentCurrentChargeDuration / _powerScriptableObject.ChargeDuration;

    public float CurrentChargeDuration => _currentCurrentChargeDuration;

    public bool IsActiveEffectOn => _isActiveEffectOn;

    public float ActivePercentage => _currentCurrentActiveDuration / _powerScriptableObject.ActiveEffectDuration;

    public float CurrentActiveDuration => _currentCurrentActiveDuration;

    public bool IsPassiveEffectOn => _isPassiveEffectOn;

    public float PassivePercentage => _currentPassiveDuration / _powerScriptableObject.PassiveEffectDuration;

    public float CurrentPassiveDuration => _currentPassiveDuration;

    public bool IsCoolingDown => _isCoolingDown;
    public float CooldownPercentage => _currentCooldownDuration / _powerScriptableObject.Cooldown;

    public float CurrentCooldownDuration => _currentCooldownDuration;

    #endregion

    public PowerToken(PowerScriptableObject powerScriptableObject)
    {
        _powerScriptableObject = powerScriptableObject;

        // Initialize the data dictionary
        _dataDictionary = new Dictionary<string, object>();

        // Set the current level to the max
        _currentCooldownDuration = powerScriptableObject.Cooldown;
    }

    #region Token Control

    public void SetChargingFlag(bool isCharging)
    {
        _isCharging = isCharging;
    }

    public void ChargePowerDuration()
    {
        // Update the charge duration
        IPowerExtensions.UpdateDuration(
            ref _currentCurrentChargeDuration,
            _powerScriptableObject.ChargeDuration,
            Time.deltaTime
        );
    }

    public void ResetChargeDuration()
    {
        _currentCurrentChargeDuration = 0;
    }

    public void SetActiveFlag(bool isActive)
    {
        _isActiveEffectOn = isActive;
    }

    public void ActivePowerDuration()
    {
        // Update the active duration
        IPowerExtensions.UpdateDuration(
            ref _currentCurrentActiveDuration,
            _powerScriptableObject.ActiveEffectDuration,
            Time.deltaTime
        );
    }

    public void ResetActiveDuration()
    {
        _currentCurrentActiveDuration = 0;
    }

    public void SetPassiveFlag(bool isPassive)
    {
        _isPassiveEffectOn = isPassive;
    }

    public void PassivePowerDuration()
    {
        // Update the passive duration
        IPowerExtensions.UpdateDuration(
            ref _currentPassiveDuration,
            _powerScriptableObject.PassiveEffectDuration,
            Time.deltaTime
        );
    }

    public void ResetPassiveDuration()
    {
        _currentPassiveDuration = 0;
    }

    public void SetCooldownFlag(bool isCoolingDown)
    {
        _isCoolingDown = isCoolingDown;
    }

    public void CooldownPowerDuration()
    {
        // Update the cooldown duration
        IPowerExtensions.UpdateDuration(
            ref _currentCooldownDuration,
            _powerScriptableObject.Cooldown,
            Time.deltaTime
        );
    }

    public void SetCooldownDuration(float amount)
    {
        _currentCooldownDuration = amount;
    }

    #endregion

    public void Reset()
    {
        // Reset the charge duration
        ResetChargeDuration();

        // Reset the active duration
        ResetActiveDuration();

        // Reset the passive duration
        ResetPassiveDuration();

        // Reset the cooldown duration
        SetCooldownDuration(_powerScriptableObject.Cooldown);
    }

    public void AddData(string key, object value)
    {
        if (_dataDictionary == null)
            _dataDictionary = new Dictionary<string, object>();

        // If the key already exists, throw an exception for the user to handle
        if (!_dataDictionary.TryAdd(key, value))
            throw new System.Exception($"Key {key} already exists in the data dictionary.");
    }

    public bool TryGetData<T>(string key, out T rValue)
    {
        if (_dataDictionary.TryGetValue(key, out var value))
        {
            rValue = (T)value;
            return true;
        }

        rValue = default;
        return false;
    }

    public bool RemoveData<T>(string key, out T rValue)
    {
        if (_dataDictionary.Remove(key, out var value))
        {
            rValue = (T)value;
            return true;
        }

        rValue = default;
        return false;
    }

    public void SetPowerLevel(int level)
    {
        // Clamp the level to the max level of the power
        level = Mathf.Clamp(level, 0, _powerScriptableObject.MaxLevel);

        // Set the current level value
        _currentLevel = level;
    }

    public static PowerToken CreatePowerToken(
        PowerScriptableObject powerScriptableObject,
        int powerLevel, float activeDuration, float passiveDuration, float cooldownDuration
    )
    {
        // Create a new power token
        var powerToken = new PowerToken(powerScriptableObject);

        // Set the power level
        powerToken.SetPowerLevel(powerLevel);

        // Set the active duration
        powerToken._currentCurrentActiveDuration = activeDuration;
        powerToken._isActiveEffectOn = activeDuration > 0;

        // Set the passive duration
        powerToken._currentPassiveDuration = passiveDuration;
        powerToken._isPassiveEffectOn = passiveDuration > 0;

        // Set the cooldown duration
        powerToken.SetCooldownDuration(cooldownDuration);
        powerToken._isCoolingDown = cooldownDuration > 0;

        return powerToken;
    }
}