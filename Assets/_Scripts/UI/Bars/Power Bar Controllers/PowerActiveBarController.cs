using UnityEngine;

public class PowerActiveBarController : TransparentBarController
{
    [SerializeField] private PowerListReference equippedPowers;
    [SerializeField] private IntReference currentPowerIndex;
    [SerializeField] private PowerTokenListReference powerTokens;

    protected override float CurrentValue { get; set; }
    protected override float PreviousValue { get; set; }

    protected override void CustomUpdate()
    {
    }

    protected override void SetCurrentValue()
    {
        PowerScriptableObject currentPower = null;
        
        if (equippedPowers.Value.Count > 0)
            currentPower = equippedPowers.Value[currentPowerIndex.Value];
        
        // If there is no power, set the current value to 0
        if (currentPower == null)
        {
            CurrentValue = 0;
            return;
        }

        var powerToken = powerTokens.GetPowerToken(currentPower);

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