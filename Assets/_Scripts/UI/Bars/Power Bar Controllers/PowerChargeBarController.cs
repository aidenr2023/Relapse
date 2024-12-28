public class PowerChargeBarController : TransparentBarController
{
    #region Private Fields

    private Player _player;

    #endregion

    protected override float CurrentValue { get; set; }
    protected override float PreviousValue { get; set; }

    private void Start()
    {
        // Set the player
        _player = Player.Instance;
    }

    protected override void CustomUpdate()
    {
        // if there is no player, try to get the player
        if (_player == null)
            _player = Player.Instance;
    }

    protected override void SetCurrentValue()
    {
        var currentPower = _player.PlayerPowerManager.CurrentPower;

        // If there is no power, set the current value to 0
        if (currentPower == null)
        {
            CurrentValue = 0;
            return;
        }

        var powerToken = _player.PlayerPowerManager.CurrentPowerToken;

        // If there is no power token, set the current value to 0
        if (powerToken == null)
        {
            CurrentValue = 0;
            return;
        }

        CurrentValue = powerToken.ChargePercentage;
    }

    protected override void SetPreviousValue()
    {
        PreviousValue = CurrentValue;
    }

    protected override float CalculatePercentage()
    {
        var currentPower = _player.PlayerPowerManager.CurrentPower;

        // If there is no power, set the current value to 0
        if (currentPower == null)
            return 0;

        var powerToken = _player.PlayerPowerManager.CurrentPowerToken;

        // If there is no power token, set the current value to 0
        if (powerToken == null)
            return 0;

        return powerToken.ChargePercentage;
    }
}