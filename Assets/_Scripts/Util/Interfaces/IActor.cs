using System;
using UnityEngine;

public delegate void HealthChangedEventHandler(object sender, HealthChangedEventArgs e);

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
    public void ChangeHealth(float amount, IActor changer, IDamager damager);
}

public sealed class HealthChangedEventArgs : EventArgs
{
    private readonly IActor _actor;
    private readonly IActor _changer;
    private readonly IDamager _damagerObject;
    private readonly float _amount;

    public IActor Actor => _actor;
    public IActor Changer => _changer;
    public IDamager DamagerObject => _damagerObject;
    public float Amount => _amount;

    public HealthChangedEventArgs(IActor actor, IActor changer, IDamager damager, float amount)
    {
        _actor = actor;
        _changer = changer;
        _damagerObject = damager;
        _amount = amount;
    }
}