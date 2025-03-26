using UnityEngine;

public class StaminaBarController : TransparentBarController
{
    [SerializeField] private FloatReference maxValue;
    [SerializeField] private FloatReference currentValue;
    [SerializeField] private BoolReference isSprintRecovery;

    [SerializeField] private CanvasGroup staminaFlashGroup;
    [SerializeField] private float staminaFlashDuration = 1;
    [SerializeField] private AnimationCurve staminaFlashCurve;

    private bool _wasPreviouslySprintRecovery;
    private float _recoveryStartTime;

    protected override float CurrentValue { get; set; }
    protected override float PreviousValue { get; set; }

    protected override void SetCurrentValue()
    {
        CurrentValue = currentValue;
    }

    protected override void SetPreviousValue()
    {
        PreviousValue = CurrentValue;
    }

    protected override float CalculatePercentage()
    {
        return Mathf.Clamp01(currentValue / maxValue);
    }

    protected override void CustomUpdate()
    {
        base.CustomUpdate();

        // If the player is in sprint recovery, flash the stamina bar
        if (isSprintRecovery.Value)
        {
            // The player JUST started sprint recovery
            if (!_wasPreviouslySprintRecovery)
            {
                _recoveryStartTime = Time.time;
                _wasPreviouslySprintRecovery = true;
            }

            var currentTime = (Time.time - _recoveryStartTime) % staminaFlashDuration;

            staminaFlashGroup.alpha = staminaFlashCurve.Evaluate(currentTime);
        }
        
        // If the player is not in sprint recovery, set the alpha to 1
        else
            staminaFlashGroup.alpha = 1;
        
        // Update the previous sprint recovery state
        _wasPreviouslySprintRecovery = isSprintRecovery.Value;
    }
}