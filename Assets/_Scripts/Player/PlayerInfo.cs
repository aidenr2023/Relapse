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

    [Header("Health Settings")] [SerializeField]
    private float maxHealth = 3f;

    [SerializeField] private float health;

    [SerializeField] [Min(0)] private float invincibilityDuration = 1f;

    // TODO: Eventually, I might move this code to another script.
    // For now though, I'm keeping this here to make things easier
    [Header("Tolerance Meter Settings")] [SerializeField] [Min(.001f)]
    private float maxTolerance;

    [SerializeField] private float currentTolerance;

    [Header("Relapse Image Overlay")] 
    // [SerializeField] private Image relapseImage;

    [SerializeField] private AnimationCurve relapseOpacityCurve;
    [SerializeField] [Min(0.00001f)] private float relapseOpacityDuration = 1f;
    [SerializeField] private CountdownTimer relapseOpacityTimer = new(1);

    [Tooltip("The number of relapses the player can have before losing the level.")] [SerializeField]
    private int relapsesToLose = 3;

    [Tooltip("What percent is the tolerance meter set to when the player relapses?")] [Range(0, 1)] [SerializeField]
    private float toleranceRelapsePercent = .75f;

    [Header("Relapsing Settings")] [SerializeField] [Min(0)]
    private float relapseDuration = 3;

    [Header("Passive Regeneration")] [SerializeField, Min(0)]
    private float passiveRegenCap = 50;

    [SerializeField, Min(0)] private float passiveRegenRate = 5;
    [SerializeField, Min(0)] private float passiveRegenDelay = 10;

    // // TODO: Find a better way to do this
    // [SerializeField] private CinemachineVirtualCamera vCam;

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

    private bool _isRelapsing;

    private CountdownTimer _passiveRegenTimer;

    #endregion

    #region Getters

    public GameObject GameObject => gameObject;

    public float MaxHealth => maxHealth;

    public float CurrentHealth => health;

    public float MaxTolerance => maxTolerance;

    public float CurrentTolerance => currentTolerance;

    public float ToxicityPercentage => currentTolerance / maxTolerance;

    public bool IsRelapsing => _isRelapsing;

    public int RelapseCount => _relapseCount;

    public bool IsInvincible => _invincibilityTimer.IsActive;

    // public CinemachineVirtualCamera VirtualCamera => vCam;
    public CinemachineVirtualCamera VirtualCamera => CameraManager.Instance.VirtualCamera;

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
        // Set the player's health to the max health
        // health = maxHealth;

        // OnHealed += (sender, args) =>
        //     Debug.Log(
        //         $"{gameObject.name} healed: {args.Amount} by {args.Changer.GameObject.name} ({args.DamagerObject.GameObject.name})");
        // OnDamaged += (sender, args) =>
        //     Debug.Log(
        //         $"{gameObject.name} damaged: {args.Amount} by {args.Changer.GameObject.name} ({args.DamagerObject.GameObject.name})");
        // OnDeath += (sender, args) =>
        //     Debug.Log($"{gameObject.name} died: {args.Changer.GameObject.name} ({args.DamagerObject.GameObject.name})");
        //
        // OnDeath += (sender, args) =>
        //     Debug.Log($"{(args.DamagerObject == args.Actor ? "RELAPSE" : "DEATH")}!");

        // Disable the relapse image
        // relapseImage.enabled = false;
        relapseOpacityTimer.OnTimerEnd += () => { relapseOpacityTimer.Reset(); };

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
        // Subscribe to the OnRelapseStart event
        // Change the color of the relapse image
        onRelapseStart += StartRelapseImage;

        // Subscribe to the OnRelapseEnd event
        // Change the color of the relapse image
        onRelapseEnd += EndRelapseImage;

        // Subscribe to the OnDamaged event to play a sound
        OnDamaged += PlaySoundOnDamaged;
    }

    private void PlaySoundOnDamaged(object sender, HealthChangedEventArgs e)
    {
        var cSound = (health > 0) ? hitSound : deathSound;

        // Return if the sound is null
        if (cSound == null)
            return;

        // Play the sound
        SoundManager.Instance.PlaySfx(cSound);
    }

    private void StartRelapseImage(PlayerInfo obj)
    {
        // // Enable the relapse image
        // relapseImage.enabled = true;

        // Reset the relapse opacity timer
        relapseOpacityTimer.Reset();

        // Start the relapse opacity timer
        relapseOpacityTimer.Start();

        relapseOpacityTimer.SetActive(true);
    }

    private void EndRelapseImage(PlayerInfo obj)
    {
        // // Disable the relapse image
        // relapseImage.enabled = false;

        // Stop the relapse opacity timer
        relapseOpacityTimer.Stop();

        // Reset the relapse opacity timer
        relapseOpacityTimer.Reset();
    }

    private void OnDestroy()
    {
        // Set the relapse overlay UI's alpha to 0
        RelapseOverlayUI.Instance.CanvasGroup.alpha = 0;
    }

    #endregion

    #region Update Functions

    private void Update()
    {
        // Update the relapse duration
        UpdateRelapseDuration();

        // Update the relapse effects
        UpdateRelapseEffects();

        // Prevent the tolerance from going below 0 or above the max value
        ClampTolerance();

        // Update the Relapse Image update
        RelapseImageUpdate();

        // Update the invincibility timer
        _invincibilityTimer.SetMaxTime(invincibilityDuration);
        _invincibilityTimer.Update(Time.deltaTime);

        // Update the passive regeneration
        UpdatePassiveRegeneration();
    }

    private void UpdateRelapseDuration()
    {
        // Return if the player isn't relapsing
        if (!_isRelapsing)
            return;

        // Increment the relapse duration
        _currentRelapseDuration += Time.deltaTime;

        // Reset the tolerance meter after a relapse
        var targetTolerance = maxTolerance * toleranceRelapsePercent;
        var toleranceDifference = targetTolerance - maxTolerance;
        var toleranceDifferencePerSecond = toleranceDifference / relapseDuration;
        ChangeTolerance(toleranceDifferencePerSecond * Time.deltaTime);

        // // If the relapse duration is greater than the relapse duration, end the relapse
        // if (_currentRelapseDuration >= relapseDuration)
        //     EndRelapse();
        
        // If the current tolerance is less than or equal to 0, end the relapse
        if (currentTolerance <= 0)
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

    private void RelapseImageUpdate()
    {
        // Update the relapse opacity timer
        relapseOpacityTimer.Update(Time.deltaTime);

        // Update the relapse image's opacity
        var opacity = relapseOpacityCurve.Evaluate(relapseOpacityTimer.OutputValue);

        if (!_isRelapsing)
            opacity = Mathf.Lerp(RelapseOverlayUI.Instance.CanvasGroup.alpha, 0, CustomFunctions.FrameAmount(.1f));
        
        // Set the opacity of the relapse image
        RelapseOverlayUI.Instance.CanvasGroup.alpha = opacity;
    }

    private void UpdatePassiveRegeneration()
    {
        // Update the passive regeneration timer
        _passiveRegenTimer.SetMaxTime(passiveRegenRate);
        _passiveRegenTimer.Update(Time.deltaTime);
        _passiveRegenTimer.SetActive(true);

        // Return if the player is relapsing
        if (_isRelapsing)
            return;

        // Return if the player is at max health
        if (health >= maxHealth)
            return;

        // Return if health is at the passive regen cap
        if (health >= passiveRegenCap)
            return;
        
        // Return if the passive regen timer is not complete
        if (_passiveRegenTimer.IsNotComplete)
            return;

        // Increment the player's health
        var regenAmount = passiveRegenRate * Time.deltaTime;
        if (health + regenAmount >= passiveRegenCap)
            ChangeHealth(passiveRegenCap - health, this, this, transform.position);
        else
            ChangeHealth(regenAmount, this, this, transform.position);
    }

    #endregion

    public void ChangeMaxHealth(float newValue)
    {
        // If the player is gaining health, increase the max health & health
        if (newValue > maxHealth)
        {
            var healthDifference = newValue - maxHealth;
            maxHealth = newValue;
            health += healthDifference;
        }
        
        // If the player is losing health, decrease the max health & health
        else if (newValue < maxHealth)
            maxHealth = newValue;
        
        // Clamp the health
        health = Mathf.Clamp(health, 0, maxHealth);
    }

    public void ChangeMaxToxicity(float newValue)
    {
        // If the player is gaining toxicity, increase the max toxicity & toxicity
        if (newValue > maxTolerance)
            maxTolerance = newValue;
        
        // If the player is losing toxicity, decrease the max toxicity & toxicity
        else if (newValue < maxTolerance)
        {
            var toleranceDifference = newValue - maxTolerance;
            maxTolerance = newValue;
            currentTolerance += toleranceDifference;
        }
        
        // Clamp the toxicity
        currentTolerance = Mathf.Clamp(currentTolerance, 0, maxTolerance);
    }
    
    public void ChangeHealth(float amount, IActor changer, IDamager damager, Vector3 position)
    {
        // If the amount is negative, the player is taking damage
        if (amount < 0)
            TakeDamage(-amount, changer, damager, position);

        // If the amount is positive, the player is gaining health
        else if (amount > 0)
        {
            health = Mathf.Clamp(health + amount, 0, maxHealth);

            // Invoke the OnHealed event
            var args = new HealthChangedEventArgs(this, changer, damager, amount, position);
            OnHealed?.Invoke(this, args);
        }
    }

    private void TakeDamage(float damageAmount, IActor changer, IDamager damager, Vector3 position)
    {
        // Return if the player is already dead
        if (health <= 0)
            return;
        
        // Return if the player is invincible
        if (_invincibilityTimer.IsActive)
            return;

        health = Mathf.Clamp(health - damageAmount, 0, maxHealth);

        // Invoke the OnDamaged event
        var args = new HealthChangedEventArgs(this, changer, damager, damageAmount, position);
        OnDamaged?.Invoke(this, args);

        if (health <= 0)
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

    private void ClampTolerance()
    {
        currentTolerance = Mathf.Clamp(currentTolerance, 0, maxTolerance);
    }

    public void ChangeTolerance(float amount)
    {
        currentTolerance = Mathf.Clamp(currentTolerance + amount, 0, maxTolerance);

        // The player will relapse if the tolerance meter is too high
        if (currentTolerance >= maxTolerance)
            StartRelapse();
    }

    private void StartRelapse()
    {
        // Skip if the player is already relapsing
        if (_isRelapsing)
            return;

        // Set the isRelapsing flag to true
        _isRelapsing = true;

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
        _isRelapsing = false;

        // Reset the relapse duration
        _currentRelapseDuration = 0;

        // Invoke the end relapse event
        onRelapseEnd?.Invoke(this);
    }

    private void DieFromRelapse()
    {
        ChangeHealth(-maxHealth, this, this, transform.position);
    }

    public void ResetPlayer()
    {
        // Reset the health
        health = maxHealth;

        // Reset the tolerance
        currentTolerance = 0;

        // End the relapse
        EndRelapse();
    }

    public void SetUpHealth(float cHealth, float mHealth)
    {
        health = cHealth;
        maxHealth = mHealth;
    }

    public void SetUpToxicity(float cToxicity, float mToxicity, int relapseCount, bool isRelapsing)
    {
        var wasRelapsing = _isRelapsing;

        currentTolerance = cToxicity;
        maxTolerance = mToxicity;
        _relapseCount = relapseCount;
        _isRelapsing = isRelapsing;

        if (!isRelapsing && wasRelapsing)
            EndRelapse();
    }
}