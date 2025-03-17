using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerInfo : ComponentScript<Player>, IActor, IDamager
{
    #region Serialized Fields

    [Header("Vars"), SerializeField] private FloatReference maxHealthSo;
    [SerializeField] private FloatReference currentHealthSo;
    [Space, SerializeField] private FloatReference maxToxicitySo;
    [SerializeField] private FloatReference currentToxicitySo;
    [SerializeField] private BoolReference isRelapsingSo;

    [Header("Health Settings")] [SerializeField] [Min(0)]
    private float invincibilityDuration = 1f;

    // [SerializeField] [Min(.001f)] private float maxTolerance;
    // [SerializeField] private float currentTolerance;

    [Tooltip("What percent is the toxicity meter set to when the player relapses?")] [Range(0, 1)] [SerializeField]
    private float toleranceRelapsePercent = .75f;

    [Header("Relapsing Settings")] [SerializeField] [Min(0)]
    private float relapseDuration = 3;

    [Header("Passive Regeneration")] [SerializeField, Range(0, 1)]
    private float passiveRegenCap = 1;

    [SerializeField, Min(0)] private float passiveRegenRate = 5;
    [SerializeField, Min(0)] private float passiveRegenDelay = 10;

    // // TODO: Find a better way to do this
    // [SerializeField] private Cinemachine VirtualCamera vCam;

    [Header("Audio"), SerializeField] private Sound hitSound;
    [SerializeField] private Sound deathSound;

    #endregion

    #region Private Fields

    private CountdownTimer _invincibilityTimer;

    /// <summary>
    /// The number of times the player has relapsed during this level.
    /// </summary>
    private int _relapseCount;

    private float _currentRelapseDuration;

    private CountdownTimer _passiveRegenTimer;

    #endregion

    #region Getters

    public GameObject GameObject => gameObject;

    public float MaxHealth => maxHealthSo;

    public float CurrentHealth => currentHealthSo;

    public float MaxToxicity => maxToxicitySo;

    public float CurrentToxicity => currentToxicitySo;

    public float ToxicityPercentage => currentToxicitySo / maxToxicitySo;

    public bool IsRelapsing => isRelapsingSo.Value;

    public int RelapseCount => _relapseCount;

    public bool IsInvincible => _invincibilityTimer.IsActive;

    public bool IsInvincibleBecauseDamaged => _invincibilityTimer.IsActive && _invincibilityTimer.Percentage < 1;

    public Sound NormalHitSfx => null;
    public Sound CriticalHitSfx => null;

    #endregion

    #region Events

    /// <summary>
    /// An event that is called when the player relapses.
    /// Used mostly to connect to outside scripts.
    /// </summary>
    public Action<PlayerInfo> onRelapseStart;

    /// <summary>
    /// An event that is called when the player's relapse ends.
    /// </summary>
    public Action<PlayerInfo> onRelapseEnd;

    public event HealthChangedEventHandler OnDamaged;
    public event HealthChangedEventHandler OnHealed;
    public event HealthChangedEventHandler OnDeath;

    #endregion

    #region Initialization Functions

    private void Start()
    {
        // Initialize the events
        InitializeEvents();

        // Initialize the invincibility timer
        _invincibilityTimer = new CountdownTimer(invincibilityDuration, false, false);
        _invincibilityTimer.OnTimerEnd += () => _invincibilityTimer.Stop();

        // Initialize the passive regen timer
        _passiveRegenTimer = new CountdownTimer(passiveRegenDelay, true, false);
        _passiveRegenTimer.Start();
    }

    private void InitializeEvents()
    {
        // Subscribe to the OnDamaged event to play a sound
        OnDamaged += PlaySoundOnDamaged;
    }

    private void PlaySoundOnDamaged(object sender, HealthChangedEventArgs e)
    {
        var cSound = (currentHealthSo > 0) ? hitSound : deathSound;

        // Return if the sound is null
        if (cSound == null)
            return;

        // Play the sound
        SoundManager.Instance.PlaySfx(cSound);
    }

    #endregion

    #region Update Functions

    private void Update()
    {
        // Update the relapse duration
        UpdateRelapseDuration();

        // Update the relapse effects
        UpdateRelapseEffects();

        // Prevent the toxicity from going below 0 or above the max value
        ClampToxicity();

        // Update the invincibility timer
        _invincibilityTimer.SetMaxTime(invincibilityDuration);
        _invincibilityTimer.Update(Time.deltaTime);

        // Update the passive regeneration
        UpdatePassiveRegeneration();
    }

    private void UpdateRelapseDuration()
    {
        // Return if the player isn't relapsing
        if (!isRelapsingSo.Value)
            return;

        // Increment the relapse duration
        _currentRelapseDuration += Time.deltaTime;

        // Reset the toxicity meter after a relapse
        var targetTolerance = maxToxicitySo * toleranceRelapsePercent;
        var toxicityDifference = targetTolerance - maxToxicitySo;
        var toxicityDifferencePerSecond = toxicityDifference / relapseDuration;
        ChangeToxicity(toxicityDifferencePerSecond * Time.deltaTime);

        // // If the relapse duration is greater than the relapse duration, end the relapse
        // if (_currentRelapseDuration >= relapseDuration)
        //     EndRelapse();

        // If the current toxicity is less than or equal to 0, end the relapse
        if (currentToxicitySo <= 0)
            EndRelapse();
    }

    private void UpdateRelapseEffects()
    {
        // var vCamDampening = _relapseCount switch
        // {
        //     0 => 0.0f,
        //     1 => 0.2f,
        //     2 => 0.4f,
        //     _ => 0.5f
        // };
        //
        // // Apply the dampening to the virtual camera's aim
        // vCam.GetCinemachineComponent<CinemachineSameAsFollowTarget>().m_Damping = vCamDampening;
    }

    private void UpdatePassiveRegeneration()
    {
        // Update the passive regeneration timer
        _passiveRegenTimer.SetMaxTime(passiveRegenDelay);
        _passiveRegenTimer.Update(Time.deltaTime);
        _passiveRegenTimer.SetActive(true);

        var actualPassiveRegenCap = passiveRegenCap * maxHealthSo;

        // Return if the player is relapsing
        if (isRelapsingSo.Value)
            return;

        // Return if the player is at max health
        if (currentHealthSo >= maxHealthSo)
            return;

        // Return if health is at the passive regen cap
        if (currentHealthSo >= actualPassiveRegenCap)
            return;

        // Return if the passive regen timer is not complete
        if (_passiveRegenTimer.IsNotComplete)
            return;

        // Increment the player's health
        var regenAmount = passiveRegenRate * Time.deltaTime;
        if (currentHealthSo + regenAmount >= actualPassiveRegenCap)
            ChangeHealth(actualPassiveRegenCap - currentHealthSo, this, this, transform.position);
        else
            ChangeHealth(regenAmount, this, this, transform.position);
    }

    #endregion

    public void ChangeHealth(float amount, IActor changer, IDamager damager, Vector3 position,
        bool isCriticalHit = false)
    {
        // If the amount is negative, the player is taking damage
        if (amount < 0)
            TakeDamage(-amount, changer, damager, position, isCriticalHit);

        // If the amount is positive, the player is gaining health
        else if (amount > 0)
        {
            currentHealthSo.Value = Mathf.Clamp(currentHealthSo + amount, 0, maxHealthSo);

            // Invoke the OnHealed event
            var args = new HealthChangedEventArgs(this, changer, damager, amount, position);
            OnHealed?.Invoke(this, args);
        }
    }

    private void TakeDamage(float damageAmount, IActor changer, IDamager damager, Vector3 position, bool isCriticalHit)
    {
        // Return if the player is already dead
        if (currentHealthSo <= 0)
            return;

        // Return if the player is invincible
        if (_invincibilityTimer.IsActive)
            return;

        currentHealthSo.Value = Mathf.Clamp(currentHealthSo - damageAmount, 0, maxHealthSo);

        // Invoke the OnDamaged event
        var args = new HealthChangedEventArgs(this, changer, damager, damageAmount, position, isCriticalHit);
        OnDamaged?.Invoke(this, args);

        if (currentHealthSo <= 0)
        {
            // Invoke the OnDeath event
            OnDeath?.Invoke(this, args);
        }

        // Start the invincibility timer
        _invincibilityTimer.SetMaxTimeAndReset(invincibilityDuration);
        _invincibilityTimer.Start();

        // Reset the passive regen timer
        _passiveRegenTimer.SetMaxTimeAndReset(passiveRegenDelay);
        _passiveRegenTimer.Start();
    }

    private void ClampToxicity()
    {
        currentToxicitySo.Value = Mathf.Clamp(currentToxicitySo, 0, maxToxicitySo);
    }

    public void ChangeToxicity(float amount)
    {
        currentToxicitySo.Value = Mathf.Clamp(currentToxicitySo + amount, 0, maxToxicitySo);

        // The player will relapse if the toxicity meter is too high
        if (currentToxicitySo >= maxToxicitySo)
            StartRelapse();
    }

    private void StartRelapse()
    {
        // Skip if the player is already relapsing
        if (isRelapsingSo.Value)
            return;

        // Set the isRelapsing flag to true
        isRelapsingSo.Value = true;

        // Increase the relapse count
        _relapseCount++;

        // Reset the relapse duration
        _currentRelapseDuration = 0;

        // Invoke the relapse event
        onRelapseStart?.Invoke(this);
    }

    private void EndRelapse()
    {
        // Set the isRelapsing flag to false
        isRelapsingSo.Value = false;

        // Reset the relapse duration
        _currentRelapseDuration = 0;

        // Invoke the end relapse event
        onRelapseEnd?.Invoke(this);
    }

    private void DieFromRelapse()
    {
        ChangeHealth(-maxHealthSo, this, this, transform.position);
    }

    public void ResetPlayer()
    {
        // Reset the health
        currentHealthSo.Value = maxHealthSo;

        // Reset the toxicity
        currentToxicitySo.Value = 0;

        // End the relapse
        EndRelapse();
    }

    public void SetUpHealth(float cHealth, float mHealth)
    {
        currentHealthSo.Value = cHealth;
        maxHealthSo.Value = mHealth;
    }

    public void SetUpToxicity(float cToxicity, float mToxicity, int relapseCount, bool isRelapsing)
    {
        var wasRelapsing = isRelapsingSo.Value;

        currentToxicitySo.Value = cToxicity;
        maxToxicitySo.Value = mToxicity;
        _relapseCount = relapseCount;
        isRelapsingSo.Value = isRelapsing;

        if (!isRelapsing && wasRelapsing)
            EndRelapse();
    }
}