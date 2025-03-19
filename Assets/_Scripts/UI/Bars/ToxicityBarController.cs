using System;
using UnityEngine;

public class ToxicityBarController : GenericTransparentBarController
{
    [SerializeField] private BoolReference isRelapsing;

    [SerializeField] private UIJitter jitter;
    [SerializeField, Range(0, 1)] private float maxJitterPercent = .75f;

    protected override void CustomUpdate()
    {
        base.CustomUpdate();
        
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
}