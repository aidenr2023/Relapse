﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemyAttack : MonoBehaviour, IEnemyAttackBehavior
{
    #region Serialized Fields

    [SerializeField] private BossPowerScriptableObject[] powers;

    [SerializeField] private Sound normalHitSfx;

    #endregion

    #region Getters

    public GameObject GameObject => gameObject;
    public Enemy Enemy { get; private set; }
    public HashSet<object> AttackDisableTokens { get; } = new();

    public bool IsAttackEnabled => _isExternallyEnabled && this.IsAttackEnabledTokens();


    public Sound NormalHitSfx => normalHitSfx;
    public Sound CriticalHitSfx => null;

    #endregion

    #region Private Fields

    private bool _isExternallyEnabled = true;

    #endregion

    private void Awake()
    {
        // Get the enemy component
        Enemy = GetComponent<Enemy>();
    }

    public void SetAttackEnabled(bool on)
    {
        _isExternallyEnabled = on;
    }
}