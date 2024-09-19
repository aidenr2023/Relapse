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
    private float _currentChargeDuration;

    /// <summary>
    /// A flag to determine if the power is active.
    /// </summary>
    private bool _isActive;

    /// <summary>
    /// How far along the active duration is.
    /// </summary>
    private float _currentActiveDuration;

    /// <summary>
    /// Flag to determine if the power is cooling down.
    /// </summary>
    private bool _isCoolingDown;

    /// <summary>
    /// How far along the cooldown is.
    /// </summary>
    private float _currentCooldownDuration;

    #region Getters

    public PowerScriptableObject PowerScriptableObject => _powerScriptableObject;
    public int CurrentLevel => Mathf.Clamp(_currentLevel, 0, _powerScriptableObject.LevelCount - 1);

    public float ToleranceMeterImpact => _powerScriptableObject.BaseToleranceMeterImpact *
                                         _powerScriptableObject.ToleranceMeterLevelMultiplier[CurrentLevel] *
                                         PowerScriptableObject.PowerType.ToleranceMultiplier();

    public bool IsCharging => _isCharging;
    public float ChargePercentage => _currentChargeDuration / _powerScriptableObject.ChargeDuration;

    public float ChargeDuration => _currentChargeDuration;

    public bool IsActive => _isActive;

    public float ActivePercentage => _currentActiveDuration / _powerScriptableObject.ActiveDuration;

    public float ActiveDuration => _currentActiveDuration;

    public bool IsCoolingDown => _isCoolingDown;
    public float CooldownPercentage => _currentCooldownDuration / _powerScriptableObject.Cooldown;

    public float CooldownDuration => _currentCooldownDuration;

    #endregion

    public PowerToken(PowerScriptableObject powerScriptableObject)
    {
        _powerScriptableObject = powerScriptableObject;
    }

    public void SetChargingFlag(bool isCharging)
    {
        _isCharging = isCharging;
    }

    public void ChargePowerDuration()
    {
        // Update the charge duration
        IPowerExtensions.UpdateDuration(
            ref _currentChargeDuration,
            _powerScriptableObject.ChargeDuration,
            Time.deltaTime
        );
    }

    public void ResetChargeDuration()
    {
        _currentChargeDuration = 0;
    }

    public void SetActiveFlag(bool isActive)
    {
        _isActive = isActive;
    }

    public void ActivePowerDuration()
    {
        // Update the active duration
        IPowerExtensions.UpdateDuration(
            ref _currentActiveDuration,
            _powerScriptableObject.ActiveDuration,
            Time.deltaTime
        );
    }
    
    public void ResetActiveDuration()
    {
        _currentActiveDuration = 0;
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
}