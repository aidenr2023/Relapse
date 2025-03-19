using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemyAttack : MonoBehaviour, IEnemyAttackBehavior
{
    #region Serialized Fields

    [SerializeField] private PowerListReference playerPowers;

    [SerializeField] private List<BossPowerScriptableObject> allBossPowers;
    [SerializeField] private List<BossPowerScriptableObject> bossPowers;

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

    private void Start()
    {
        // Initialize the boss powers
        InitializeBossPowers(playerPowers.Value.ToArray());

        StartCoroutine(Attack());
    }

    private void InitializeBossPowers(params PowerScriptableObject[] powers)
    {
        // Clear the boss powers
        bossPowers.Clear();

        var playerPowerCount = powers.Length;
        var bossPowerCount = 0;

        // Add the powers that player has
        foreach (var power in powers)
        {
            if (bossPowerCount >= playerPowerCount)
                break;

            // continue if the power's corresponding boss power is null
            if (power.BossPower == null)
                continue;

            // Add the power to the boss powers
            AddPower(power.BossPower);

            // Increment the power count
            bossPowerCount++;
        }
        
        Debug.Log($"Added {bossPowerCount} powers from player powers");

        // Add the remaining powers
        for (var i = 0; i < allBossPowers.Count && bossPowerCount < playerPowerCount; i++)
        {
            // Continue if the power is already in the boss powers
            if (bossPowers.Contains(allBossPowers[i]))
                continue;

            // Add the power to the boss powers
            AddPower(allBossPowers[i]);

            // Increment the power count
            bossPowerCount++;
        }
    }

    private void AddPower(BossPowerScriptableObject power)
    {
        // Continue if the power is already in the boss powers
        if (bossPowers.Contains(power))
            return;
        
        // Add the power to the boss powers
        bossPowers.Add(power);
        
        Debug.Log($"Added power: {power.name}");
    }

    private void Update()
    {
        var cPower = GetRandomPower();
    }

    private IEnumerator Attack()
    {
        while (Enemy.EnemyInfo.CurrentHealth > 0)
        {
            if (IsAttackEnabled)
            {
                var cPower = GetRandomPower();

                Debug.Log($"Using power: {cPower.name}");

                yield return new WaitForSeconds(3);
            }
            else
                yield return new WaitForSeconds(.25f);
        }
    }

    private BossPowerScriptableObject GetRandomPower()
    {
        return bossPowers[UnityEngine.Random.Range(0, bossPowers.Count)];
    }

    public void SetAttackEnabled(bool on)
    {
        _isExternallyEnabled = on;
    }
}