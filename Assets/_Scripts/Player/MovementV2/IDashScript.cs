using System;

public interface IDashScript
{
    public event Action<IDashScript> OnDash;
}