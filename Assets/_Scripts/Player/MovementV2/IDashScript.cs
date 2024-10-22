using System;

public interface IDashScript
{
    public float DashDuration { get; }

    public event Action<IDashScript> OnDashStart;
    public event Action<IDashScript> OnDashEnd;
}