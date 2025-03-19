using UnityEngine;

public class GenericTransparentBarController : TransparentBarController
{
    [SerializeField] private FloatReference maxValue;
    [SerializeField] private FloatReference currentValue;
    
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
}