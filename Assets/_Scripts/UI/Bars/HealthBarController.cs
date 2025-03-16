using System;
using UnityEngine;

public class HealthBarController : TransparentBarController
{
    [SerializeField] private FloatReference maxHealth;
    [SerializeField] private FloatReference currentHealth;
    
    protected override float CurrentValue { get; set; }
    protected override float PreviousValue { get; set; }

    protected override void CustomUpdate()
    {
    }

    protected override void SetCurrentValue()
    {
        CurrentValue = currentHealth;
    }

    protected override void SetPreviousValue()
    {
        PreviousValue = CurrentValue;
    }

    protected override float CalculatePercentage()
    {
        return Mathf.Clamp01(currentHealth / maxHealth);
    }
}