using UnityEngine;

public class PowerActiveBarController : TransparentBarController
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

        // If the max active duration is 0, return 0
        if (currentPower.ActiveEffectDuration == 0)
        {
            CurrentValue = 0;
            return;
        }

        if (!powerToken.IsActiveEffectOn)
        {
            CurrentValue = 0;
            return;
        }

        CurrentValue = 1 - powerToken.ActivePercentage;
    }

    protected override void SetPreviousValue()
    {
        PreviousValue = CurrentValue;
    }

    protected override float CalculatePercentage()
    {
        return CurrentValue;
    }
}