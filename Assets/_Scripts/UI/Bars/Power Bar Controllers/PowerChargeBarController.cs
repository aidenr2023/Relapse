using UnityEngine;

public class PowerChargeBarController : TransparentBarController
{
    [SerializeField] private PowerArrayReference equippedPowers;
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
        
        if (equippedPowers.Value.Length > 0)
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

        CurrentValue = powerToken.ChargePercentage;
    }

    protected override void SetPreviousValue()
    {
        PreviousValue = CurrentValue;
    }

    protected override float CalculatePercentage()
    {
        PowerScriptableObject currentPower = null;
        
        if (equippedPowers.Value.Length > 0)
            currentPower = equippedPowers.Value[currentPowerIndex.Value];
        
        // If there is no power, set the current value to 0
        if (currentPower == null)
            return 0;

        var powerToken = powerTokens.GetPowerToken(currentPower);

        // If there is no power token, set the current value to 0
        if (powerToken == null)
            return 0;

        return powerToken.ChargePercentage;
    }
}