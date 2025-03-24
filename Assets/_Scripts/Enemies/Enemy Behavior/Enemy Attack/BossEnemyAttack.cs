using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BossEnemyAttack : ComponentScript<BossEnemy>, IEnemyAttackBehavior
{
    #region Serialized Fields

    [SerializeField] private PowerListReference playerPowers;

    [FormerlySerializedAs("bossGunAttack")] [SerializeField]
    private BossGunBehavior bossGunBehavior;

    [SerializeField] private List<BossPowerScriptableObject> allBossPowers;
    [SerializeField] private List<BossPowerScriptableObject> bossPowers;

    [SerializeField] private Sound normalHitSfx;

    [SerializeField] private Transform bossYellUiParent;
    [SerializeField] private GameObject[] bossPowerYellUi;

    #endregion

    #region Getters

    public GameObject GameObject => gameObject;
    public Enemy Enemy => ParentComponent.ParentComponent.ParentComponent;
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

    protected override void CustomAwake()
    {
        // Get the boss power behavior components
        var bossPowerBehaviors = GetComponents<BossPowerBehavior>();

        foreach (var behavior in bossPowerBehaviors)
        {
            // Skip the gun attack
            if (behavior is BossGunBehavior)
                continue;

            // Initialize the behavior
            behavior.Initialize(this);

            // Add the behavior to the dictionary
            _bossPowerBehaviors.Add(behavior.BossPower, behavior);
        }

        // Initialize the gun attack
        bossGunBehavior.Initialize(this);
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
        // Wait a frame before updating
        yield return null;

        // Start the attack update coroutine with the gun attack
        var cBehavior = ChangePowerBehavior(bossGunBehavior);

        while (isActiveAndEnabled)
        {
            // Yield the attack coroutine
            yield return StartCoroutine(Attack(cBehavior));

            // If the current power is not a gun attack,
            // set it to the gun attack
            if (cBehavior != bossGunBehavior)
                cBehavior = ChangePowerBehavior(bossGunBehavior);
            else
            {
                cBehavior = ChangePowerBehavior(_bossPowerBehaviors[GetRandomPower()]);

                // Start the coroutine to do the boss yell, but don't wait for it to finish
                StartCoroutine(BossYell(GetRandomBossPowerYellUi()));
            }
        }
    }

    private IEnumerator Attack(BossPowerBehavior cBehavior)
    {
        Debug.Log($"Started using power: {cBehavior.BossPower?.name ?? "GUN"}");

        // Start the power
        yield return StartCoroutine(cBehavior.UsePower());

        Debug.Log($"Finished using power: {cBehavior.BossPower?.name ?? "GUN"}");
    }

    private GameObject GetRandomBossPowerYellUi()
    {
        return bossPowerYellUi[UnityEngine.Random.Range(0, bossPowerYellUi.Length)];
    }

    private IEnumerator BossYell(GameObject yellUiPrefab)
    {
        // Instantiate the yell UI
        var yellUi = Instantiate(yellUiPrefab, bossYellUiParent);

        // Set the local position to zero
        yellUi.transform.localPosition = Vector3.zero;

        const float maxTime = 3;

        var startTime = Time.time;

        while (Time.time - startTime < maxTime)
        {
            // make the yell UI hover
            yellUi.transform.localPosition = new Vector3(
                0,
                Mathf.Sin(Time.time * Mathf.PI * 2) * 0.5f + .5f,
                0
            );

            yield return null;
        }

        // Destroy the yell UI
        Destroy(yellUi);

        yield return null;
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
        if (behavior != bossGunBehavior)
            bossGunBehavior.IsActive = false;

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

    public void OnBadEndingStarted()
    {
        // Stop the update coroutine
        if (_updateCoroutine != null)
        {
            StopCoroutine(_updateCoroutine);
            _updateCoroutine = null;
        }

        // Disable all of the boss power behaviors
        foreach (var behavior in _bossPowerBehaviors.Values)
            behavior.IsActive = false;

        // Disable the gun attack
        bossGunBehavior.IsActive = false;
    }
}