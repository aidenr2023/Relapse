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
    /// How far along the charge is.
    /// </summary>
    private float _currentChargeDuration;

    /// <summary>
    /// How far along the cooldown is.
    /// </summary>
    private float _currentCooldown;

    /// <summary>
    /// Flag to determine if the power is charging.
    /// </summary>
    private bool _isCharging;


    #region Getters

    public PowerScriptableObject PowerScriptableObject => _powerScriptableObject;
    public int CurrentLevel => _currentLevel;
    public float ChargePercentage => _currentChargeDuration / _powerScriptableObject.ChargeDuration;
    public float CooldownPercentage => _currentCooldown / _powerScriptableObject.Cooldown;
    public bool IsCharging => _isCharging;

    #endregion

    public PowerToken(PowerScriptableObject powerScriptableObject)
    {
        _powerScriptableObject = powerScriptableObject;
    }

    public void SetChargingFlag(bool isCharging)
    {
        _isCharging = isCharging;
    }

    public void SetCooldownFlag(bool isCoolingDown)
    {
        _isCharging = isCoolingDown;
    }

    public void ChargePowerDuration()
    {
        _powerScriptableObject.PowerLogic.ChargePowerDuration(ref _currentChargeDuration);
    }
    
    public void ResetChargeDuration()
    {
        _currentChargeDuration = 0;
    }
}