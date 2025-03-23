using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemyAttack : MonoBehaviour, IEnemyAttackBehavior
{
    #region Serialized Fields

    [SerializeField] private PowerListReference playerPowers;

    [SerializeField] private BossGunAttack bossGunAttack;

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

    /// <summary>
    /// All of the boss power behaviors EXCEPT the gun power.
    /// </summary>
    private readonly Dictionary<BossPowerScriptableObject, BossPowerBehavior> _bossPowerBehaviors = new();

    private Coroutine _updateCoroutine;

    #endregion

    private void Awake()
    {
        // Get the enemy component
        Enemy = GetComponent<Enemy>();

        // Get the boss power behavior components
        var bossPowerBehaviors = GetComponents<BossPowerBehavior>();

        foreach (var behavior in bossPowerBehaviors)
        {
            // Skip the gun attack
            if (behavior is BossGunAttack)
                continue;

            // Initialize the behavior
            behavior.Initialize(this);

            // Add the behavior to the dictionary
            _bossPowerBehaviors.Add(behavior.BossPower, behavior);
        }
        
        // Initialize the gun attack
        bossGunAttack.Initialize(this);
    }

    private void Start()
    {
        // Initialize the boss powers
        InitializeBossPowers(playerPowers.Value.ToArray());
    }

    private void OnEnable()
    {
        // If there is an update coroutine, stop it
        if (_updateCoroutine != null)
        {
            StopCoroutine(_updateCoroutine);
            _updateCoroutine = null;
        }

        _updateCoroutine = StartCoroutine(UpdateCoroutine());
    }

    private void OnDisable()
    {
        // If there is an update coroutine, stop it
        if (_updateCoroutine != null)
        {
            StopCoroutine(_updateCoroutine);
            _updateCoroutine = null;
        }
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

        // Go through each of the boss power behaviors.
        // Activate the ones that are in the boss powers.
        foreach (var power in bossPowers)
        {
            if (_bossPowerBehaviors.TryGetValue(power, out var behavior))
            {
                behavior.IsActive = true;

                Debug.Log($"Activated: {power.name}");
            }
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

    private IEnumerator UpdateCoroutine()
    {
        // Start the attack update coroutine with the gun attack
        var cBehavior = ChangePowerBehavior(bossGunAttack);

        while (isActiveAndEnabled)
        {
            yield return StartCoroutine(Attack(cBehavior));

            // If the current power is not a gun attack,
            // set it to the gun attack
            if (cBehavior != bossGunAttack)
                cBehavior = ChangePowerBehavior(bossGunAttack);
            else
                cBehavior = ChangePowerBehavior(_bossPowerBehaviors[GetRandomPower()]);
        }
    }

    private IEnumerator Attack(BossPowerBehavior cBehavior)
    {
        while (true)
        {
            if (IsAttackEnabled)
            {
                Debug.Log($"Started using power: {cBehavior.BossPower?.name ?? "GUN"}");

                // Start the power
                yield return StartCoroutine(cBehavior.UsePower());

                Debug.Log($"Finished using power: {cBehavior.BossPower?.name ?? "GUN"}");

                // Wait for a bit
                yield return new WaitForSeconds(1);
            }
            else
                yield return new WaitForSeconds(.25f);
        }
    }

    private BossPowerBehavior ChangePowerBehavior(BossPowerBehavior behavior)
    {
        // Get a copy of the boss power behaviors
        var bossPowerBehaviors = _bossPowerBehaviors.Values;

        // For each power behavior in the boss power behaviors
        foreach (var b in bossPowerBehaviors)
        {
            // Skip the power behavior
            if (b == behavior)
                continue;

            // Disable the power behavior
            b.IsActive = false;
        }

        // Consider the gun attack as a special case.
        // Disable it if it's not the current behavior
        if (behavior != bossGunAttack)
            bossGunAttack.IsActive = false;

        // Enable the power behavior
        behavior.IsActive = true;

        return behavior;
    }

    private BossPowerScriptableObject GetRandomPower()
    {
        // Return a random power from the keys of the dictionary
        var keys = new List<BossPowerScriptableObject>(_bossPowerBehaviors.Keys);

        return keys[UnityEngine.Random.Range(0, keys.Count)];
    }

    public void SetAttackEnabled(bool on)
    {
        _isExternallyEnabled = on;
    }
}