using System;
using UnityEngine;

public class ToxicityBarController : TransparentBarController
{
    [SerializeField] private UIJitter jitter;
    [SerializeField, Range(0, 1)] private float maxJitterPercent = .75f;
    
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
        CurrentValue = _player.PlayerInfo.CurrentToxicity;

        if (jitter == null) 
            return;

        // If the player is relapsing, set the jitter lerp amount to 1
        if (_player?.PlayerInfo.IsRelapsing ?? false)
        {
            jitter.SetLerpAmount(1);
            return;
        }
        
        var lerpAmount = Mathf.InverseLerp(0, maxJitterPercent, CalculatePercentage());
        jitter.SetLerpAmount(lerpAmount);
    }

    protected override void SetPreviousValue()
    {
        PreviousValue = CurrentValue;
    }

    protected override float CalculatePercentage()
    {
        return Mathf.Clamp01(_player.PlayerInfo.CurrentToxicity / _player.PlayerInfo.MaxToxicity);
    }
}