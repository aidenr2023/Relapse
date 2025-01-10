using System;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBarController : TransparentBarController
{
    #region Private Fields

    private Player _player;

    private PlayerMovementV2 _movementV2;

    #endregion

    private void Start()
    {
        _player = Player.Instance;
    }

    protected override float CurrentValue { get; set; }
    protected override float PreviousValue { get; set; }

    protected override void CustomUpdate()
    {
        // if there is no player, try to get the player
        if (_player == null)
            _player = Player.Instance;

        // if the player is still null, return
        if (_player == null)
            return;

        // Get the player movement v2
        _movementV2 = _player.PlayerController as PlayerMovementV2;
    }

    protected override void SetCurrentValue()
    {
        CurrentValue = Mathf.Clamp01(_movementV2.CurrentStamina / _movementV2.MaxStamina);
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