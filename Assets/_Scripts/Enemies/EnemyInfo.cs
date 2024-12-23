﻿using System;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Enemy))]
public class EnemyInfo : MonoBehaviour, IActor
{
    #region Fields

    [SerializeField] private float maxHealth = 3f;
    [SerializeField] private float currentHealth;

    [SerializeField] [Min(0)] private int moneyReward;

    #endregion

    private bool _isDead;

    #region Getters

    public GameObject GameObject => gameObject;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    #endregion

    #region Events

    public event HealthChangedEventHandler OnDamaged;
    public event HealthChangedEventHandler OnHealed;
    public event HealthChangedEventHandler OnDeath;

    #endregion

    private void Start()
    {
        // OnHealed += LogOnHealed;
        // OnDamaged += LogOnDamaged;
        // OnDeath += LogOnDeath;
        return;

        void LogOnHealed(object _, HealthChangedEventArgs args)
        {
            Debug.Log(
                $"{gameObject.name} healed: {args.Amount:0.00} by {args.Changer.GameObject.name} ({args.DamagerObject.GameObject.name})");
        }

        void LogOnDamaged(object _, HealthChangedEventArgs args)
        {
            Debug.Log(
                $"{gameObject.name} damaged: {args.Amount:0.00} by {args.Changer.GameObject.name} ({args.DamagerObject.GameObject.name})");
        }

        void LogOnDeath(object _, HealthChangedEventArgs args)
        {
            Debug.Log($"{gameObject.name} died: {args.Changer.GameObject.name} ({args.DamagerObject.GameObject.name})");
        }
    }

    public void ChangeHealth(float amount, IActor changer, IDamager damager)
    {
        // Clamp the health value between 0 and the max health
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

        HealthChangedEventArgs args;

        // If the amount is less than 0, invoke the OnDamaged event
        if (amount < 0)
        {
            args = new HealthChangedEventArgs(this, changer, damager, -amount);
            OnDamaged?.Invoke(this, args);
        }

        // If the amount is greater than 0, invoke the OnHealed event
        else if (amount > 0)
        {
            args = new HealthChangedEventArgs(this, changer, damager, amount);
            OnHealed?.Invoke(this, args);
        }

        // If the amount is 0, do nothing
        else
            return;

        // If the enemy's health is less than or equal to 0, call the Die function
        if (currentHealth <= 0)
        {
            // Invoke the OnDeath event
            OnDeath?.Invoke(this, args);

            Die();
        }
    }

    private void Die()
    {
        // Return if the enemy is already dead
        if (_isDead)
            return;

        // Set the isDead flag to true
        _isDead = true;

        Player.Instance.PlayerInventory.AddItem(Player.Instance.PlayerInventory.MoneyObject, moneyReward);

        // TODO: Implement death logic
        Destroy(gameObject);
    }
}