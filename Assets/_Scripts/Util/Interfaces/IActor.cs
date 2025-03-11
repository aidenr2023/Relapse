using System;
using UnityEngine;

public delegate void HealthChangedEventHandler(object sender, HealthChangedEventArgs e);
public delegate void StunnedEventHandler(HealthChangedEventArgs e, float duration);

public interface IActor : IInterfacedObject
{
    public float MaxHealth { get; }
    public float CurrentHealth { get; }

    public event HealthChangedEventHandler OnDamaged;
    public event HealthChangedEventHandler OnHealed;
    public event HealthChangedEventHandler OnDeath;

    /// <summary>
    /// A function that changes the player's health by a certain amount.
    /// </summary>
    /// <param name="amount">The amount to change the actor's health by.</param>
    /// <param name="changer">The actor that is changing the actor's health.</param>
    /// <param name="damager"></param>
    /// <param name="position"></param>
    /// <param name="isCriticalHit"></param>
    public void ChangeHealth(float amount, IActor changer, IDamager damager, Vector3 position, bool isCriticalHit = false);
}

public sealed class HealthChangedEventArgs : EventArgs
{
    public IActor Actor { get; }

    public IActor Changer { get; }

    public IDamager DamagerObject { get; }

    public float Amount { get; }

    public Vector3 Position { get; }
    
    public bool IsCriticalHit { get; }

    public HealthChangedEventArgs(IActor actor, IActor changer, IDamager damager, float amount, Vector3 position, bool isCriticalHit = false)
    {
        Actor = actor;
        Changer = changer;
        DamagerObject = damager;
        Amount = amount;
        Position = position;
        IsCriticalHit = isCriticalHit;
    }
}
