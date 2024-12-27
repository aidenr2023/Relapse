using System;
using UnityEngine;

public class ToxicityBar : TransparentBar
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
        CurrentValue = _player.PlayerInfo.CurrentTolerance;
    }

    protected override void SetPreviousValue()
    {
        PreviousValue = CurrentValue;
    }

    protected override float CalculatePercentage()
    {
        return Mathf.Clamp01(_player.PlayerInfo.CurrentTolerance / _player.PlayerInfo.MaxTolerance);
    }
}