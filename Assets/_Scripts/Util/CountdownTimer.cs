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

    public bool IsActive { get; private set; } = true;

    public bool IsTicking => TimeLeft > 0;

    public event Action OnTimerEnd;

    public float Percentage => 1 - (TimeLeft / MaxTime);

    public CountdownTimer(float maxTime, bool isActive = false, bool zeroByDefault = false)
    {
        MaxTime = maxTime;
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

    public void SetActive(bool active)
    {
        IsActive = active;
    }

    public void Start() => SetActive(true);

    public void Stop() => SetActive(false);
}