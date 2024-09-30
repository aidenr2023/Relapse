using UnityEngine;

public interface IActor
{
    public GameObject GameObject { get; }

    public float MaxHealth { get; }
    public float CurrentHealth { get; }

    public void ChangeHealth(float amount);
}