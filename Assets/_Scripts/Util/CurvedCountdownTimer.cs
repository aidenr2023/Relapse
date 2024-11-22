using System;
using UnityEngine;

[Serializable]
public class CurvedCountdownTimer : CountdownTimer
{
    [SerializeField] private AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

    public override float OutputValue => curve.Evaluate(Percentage);

    public CurvedCountdownTimer(float maxTime, bool isActive = false, bool zeroByDefault = false)
        : base(maxTime, isActive, zeroByDefault)
    {
    }
}