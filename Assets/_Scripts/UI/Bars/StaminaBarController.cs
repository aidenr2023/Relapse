using System;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBarController : TransparentBarController
{
    [SerializeField] private FloatReference maxStamina;
    [SerializeField] private FloatReference currentStamina;

    protected override float CurrentValue { get; set; }
    protected override float PreviousValue { get; set; }

    protected override void CustomUpdate()
    {
    }

    protected override void SetCurrentValue()
    {
        CurrentValue = Mathf.Clamp01(currentStamina / maxStamina);
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