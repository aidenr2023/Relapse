using System;
using UnityEngine;

public class ToxicityBarController : TransparentBarController
{
    [SerializeField] private FloatReference maxToxicity;
    [SerializeField] private FloatReference currentToxicity;
    [SerializeField] private BoolReference isRelapsing;

    [SerializeField] private UIJitter jitter;
    [SerializeField, Range(0, 1)] private float maxJitterPercent = .75f;

    protected override float CurrentValue { get; set; }
    protected override float PreviousValue { get; set; }

    protected override void CustomUpdate()
    {
    }

    protected override void SetCurrentValue()
    {
        CurrentValue = currentToxicity.Value;

        if (jitter == null)
            return;

        // If the player is relapsing, set the jitter lerp amount to 1
        if (isRelapsing.Value)
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
        return Mathf.Clamp01(currentToxicity / maxToxicity);
    }
}