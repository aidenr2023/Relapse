using System;
using UnityEngine;

[Serializable]
public class CountdownTimer
{
    [SerializeField] [Min(0)] private float maxTime;

    public float MaxTime
    {
        get => maxTime;
        private set => maxTime = value;
    }

    public float TimeLeft { get; private set; }

    public bool IsActive { get; private set; }

    public bool IsNotComplete => TimeLeft > 0;

    public bool IsComplete => TimeLeft <= 0;

    public event Action OnTimerEnd;

    public float Percentage => 1 - (TimeLeft / MaxTime);

    public virtual float OutputValue => Percentage;

    public CountdownTimer(float maxTime, bool isActive = false, bool zeroByDefault = false)
    {
        MaxTime = Mathf.Max(0, maxTime);
        TimeLeft = zeroByDefault ? 0 : maxTime;
        IsActive = isActive;
    }

    public void Update(float deltaTime)
    {
        // Return if the timer is not active
        if (!IsActive)
            return;

        // Variable used to determine if the timer was ticking before the update
        var isTicking = TimeLeft > 0;

        TimeLeft = Mathf.Clamp(TimeLeft - deltaTime, 0, MaxTime);

        if (TimeLeft <= 0 && isTicking)
            OnTimerEnd?.Invoke();
    }

    public void Reset()
    {
        TimeLeft = MaxTime;
    }

    public void SetMaxTime(float time)
    {
        MaxTime = time;
    }

    public void SetMaxTimeAndReset(float time)
    {
        MaxTime = time;
        TimeLeft = MaxTime;
    }

    public void ForcePercent(float amount)
    {
        TimeLeft = Mathf.Clamp(TimeLeft - (MaxTime * amount), 0, MaxTime);

        if (TimeLeft <= 0)
            OnTimerEnd?.Invoke();
    }

    public void SetActive(bool active)
    {
        IsActive = active;
    }

    public void Start() => SetActive(true);

    public void Stop() => SetActive(false);
}