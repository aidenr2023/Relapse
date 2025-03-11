using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

[RequireComponent(typeof(Enemy))]
public class EnemyInfo : ComponentScript<Enemy>, IActor
{
    private static readonly int HitAnimationID = Animator.StringToHash("Hit");

    #region Serialized Fields

    [SerializeField] private float maxHealth = 3f;
    [SerializeField] private float currentHealth;

    [SerializeField] [Min(0)] private int moneyReward;

    [SerializeField] private Animator animator;

    [Space, SerializeField] private VisualEffect enemyHitEffect;
    [SerializeField, Min(0)] private float minVFXRangeDamage = 10f;
    [SerializeField, Min(0)] private float maxVFXRangeDamage = 50f;
    [SerializeField, Min(0)] private float minVFXDamage = 5f;

    [SerializeField] private VisualEffect enemyDeathVfxPrefab;

    [SerializeField] private Sound enemyDeathSound;

    [SerializeField] private ManagedAudioSource enemyMoanSource;
    [SerializeField] private Sound[] moanSounds;
    [SerializeField, Range(0, 1)] private float moanSoundChance = 1f;
    [SerializeField, Min(0)] private float moanSoundMinCooldown = 5f;
    [SerializeField, Min(0)] private float moanSoundMaxCooldown = 10f;

    #endregion

    #region Private Fields

    private bool _isDead;

    private float _damagedThisFrame;
    private Vector3 _damagePosition;
    private bool _hasPlayedHitVFX;

    private bool _hasPlayedHitSoundThisFrame;

    private CountdownTimer _moanSoundTimer;

    private CountdownTimer _comicImpactTimer;

    private float _remainingStunTime;

    #endregion

    #region Getters

    public GameObject GameObject => gameObject;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    public bool IsStunned => _remainingStunTime > 0;

    #endregion

    #region Events

    public event HealthChangedEventHandler OnDamaged;
    public event HealthChangedEventHandler OnHealed;
    public event HealthChangedEventHandler OnDeath;

    public event StunnedEventHandler OnStunStart;
    public event StunnedEventHandler OnStunEnd;

    #endregion

    protected override void CustomAwake()
    {
        // Set up the cooldown timer for the moan sound
        _moanSoundTimer = new CountdownTimer(UnityEngine.Random.Range(moanSoundMinCooldown, moanSoundMaxCooldown));
        _moanSoundTimer.OnTimerEnd += () =>
        {
            PlayMoanSound();
            _moanSoundTimer.SetMaxTimeAndReset(UnityEngine.Random.Range(moanSoundMinCooldown, moanSoundMaxCooldown));
        };
        _moanSoundTimer.Start();

        _comicImpactTimer = new(0.5f, true, true);
    }

    private void Start()
    {
        // OnHealed += LogOnHealed;
        // OnDamaged += LogOnDamaged;
        // OnDeath += LogOnDeath;

        OnDamaged += AddDamageThisFrame;
        OnDamaged += SetDamagePositionOnDamaged;
        OnDamaged += PlaySoundOnDamaged;
        OnDamaged += ComicImpactOnDamaged;

        OnDeath += DetachVFXOnDeath;
        OnDeath += PlaySoundOnDeath;
        OnDeath += CreateDeathVfx;

        // Activate the animator's hit trigger
        OnDamaged += (_, _) =>
        {
            if (animator != null)
                animator.SetTrigger(HitAnimationID);
        };

        // Add the movement and attack disable tokens on stun start
        OnStunStart += (_, _) =>
        {
            ParentComponent.NewMovement.AddMovementDisableToken(this);
            ParentComponent.AttackBehavior.AddAttackDisableToken(this);
        };
        OnStunEnd += (_, _) =>
        {
            ParentComponent.NewMovement.RemoveMovementDisableToken(this);
            ParentComponent.AttackBehavior.RemoveAttackDisableToken(this);
        };

        OnStunStart += (_, _) => Debug.Log($"END EEE");

        // Set the moan sound source to be permanent
        enemyMoanSource?.SetPermanent(true);
    }

    private void ComicImpactOnDamaged(object sender, HealthChangedEventArgs e)
    {
        // Return if the comic impact manager is null
        if (ComicImpactManager.Instance == null)
            return;

        // Return if the damager is not a power
        if (e.DamagerObject is not IPower && (e.DamagerObject is not Shotgun && !e.IsCriticalHit))
            return;

        // Return if the comic impact timer is not complete
        if (!_comicImpactTimer.IsComplete)
            return;

        // Reset the comic impact timer
        _comicImpactTimer.Reset();

        var ui = ComicImpactManager.Instance.SpawnImpact(e.Position);

        // // Make the UI a child of the enemy
        // ui.transform.SetParent(transform);
    }

    private void CreateDeathVfx(object sender, HealthChangedEventArgs e)
    {
        // Return if there is no death VFX prefab
        if (enemyDeathVfxPrefab == null)
            return;

        // Instantiate the death VFX prefab
        var deathVfx = Instantiate(enemyDeathVfxPrefab, transform.position, Quaternion.identity);
        deathVfx.Play();

        // Destroy the death VFX after 10 seconds
        Destroy(deathVfx.gameObject, 10f);
    }

    private void LogOnHealed(object _, HealthChangedEventArgs args)
    {
        Debug.Log(
            $"{gameObject.name} healed: {args.Amount:0.00} by {args.Changer.GameObject.name} ({args.DamagerObject.GameObject.name})");
    }

    private void LogOnDamaged(object _, HealthChangedEventArgs args)
    {
        Debug.Log(
            $"{gameObject.name} damaged: {args.Amount:0.00} by {args.Changer.GameObject.name} ({args.DamagerObject.GameObject.name})");
    }

    private void LogOnDeath(object _, HealthChangedEventArgs args)
    {
        Debug.Log($"{gameObject.name} died: {args.Changer.GameObject.name} ({args.DamagerObject.GameObject.name})");
    }

    private void PlayMoanSound()
    {
        // Random chance to play the moan sound
        if (UnityEngine.Random.value > moanSoundChance)
            return;

        // Return if the moan sounds array is null or empty
        if (moanSounds == null || moanSounds.Length == 0)
            return;

        // Play a random moan sound
        var randomSound = moanSounds[UnityEngine.Random.Range(0, moanSounds.Length)];
        enemyMoanSource?.Play(randomSound);
    }

    private void SetDamagePositionOnDamaged(object sender, HealthChangedEventArgs args)
    {
        _damagePosition = args.Position;
    }

    private void PlaySoundOnDamaged(object sender, HealthChangedEventArgs e)
    {
        // // Return if the sound is null
        // if (enemyHitSound == null)
        //     return;

        // TODO: Determine which sound should be played
        var sfx = 
            e.IsCriticalHit && e.DamagerObject.CriticalHitSfx != null 
            ? e.DamagerObject.CriticalHitSfx 
            : e.DamagerObject.NormalHitSfx;

        // Return if the sound is null
        if (sfx == null)
            return;

        // // Return if the enemy's health is less than or equal to 0
        // if (currentHealth <= 0)
        //     return;

        // Return if the hit sound has already played this frame
        if (_hasPlayedHitSoundThisFrame)
            return;

        // Play the sound at the enemy's position
        SoundManager.Instance.PlaySfxAtPoint(sfx, transform.position);

        // Set the hasPlayedHitSoundThisFrame flag to true
        _hasPlayedHitSoundThisFrame = true;
    }

    private void PlaySoundOnDeath(object sender, HealthChangedEventArgs e)
    {
        // Return if the sound is null
        if (enemyDeathSound == null)
            return;

        // Return if the hit sound has already played this frame
        if (_hasPlayedHitSoundThisFrame)
            return;

        // Play the sound at the enemy's position
        SoundManager.Instance.PlaySfxAtPoint(enemyDeathSound, transform.position);

        // Set the hasPlayedHitSoundThisFrame flag to true
        _hasPlayedHitSoundThisFrame = true;
    }

    private void AddDamageThisFrame(object sender, HealthChangedEventArgs args)
    {
        _damagedThisFrame += args.Amount;
    }

    private void Update()
    {
        // Update the moan sound timer
        _moanSoundTimer.Update(Time.deltaTime);

        // Update the comic pow timer
        _comicImpactTimer.Update(Time.deltaTime);

        // If the enemy took damage this frame, play the visual effect
        PlayVFXAfterDamage();
    }

    private void LateUpdate()
    {
        // Reset the damage this frame if the visual effect has played
        if (_hasPlayedHitVFX)
        {
            _damagedThisFrame = 0;
            _hasPlayedHitVFX = false;
        }

        // Reset the hasPlayedHitSoundThisFrame flag
        _hasPlayedHitSoundThisFrame = false;
    }

    private void PlayVFXAfterDamage()
    {
        // Return if the visual effect is null
        if (enemyHitEffect == null)
            return;

        // If there was no damage this frame, return
        // If the damage this frame is less than the minimum damage for the visual effect, return
        if (_damagedThisFrame <= 0 || _damagedThisFrame < minVFXDamage)
            return;

        // If hit vfx has already played, return
        if (_hasPlayedHitVFX)
            return;

        // Set the visual effect's position to the position of the damage
        enemyHitEffect.SetVector3("StartPosition", _damagePosition);

        // Calculate the damage percentage based on the amount of damage the enemy took this frame
        var damagePercentage = Mathf.InverseLerp(minVFXRangeDamage, maxVFXRangeDamage, _damagedThisFrame);

        // Set the damage amount float
        enemyHitEffect.SetFloat("DamageAmount", damagePercentage);

        // Play the visual effect
        enemyHitEffect.Play();

        // Set the hasPlayedHitVFX flag to true
        _hasPlayedHitVFX = true;
    }

    private void DetachVFXOnDeath(object sender, HealthChangedEventArgs args)
    {
        // Return if the visual effect is null
        if (enemyHitEffect == null)
            return;

        // Detach the visual effect
        enemyHitEffect.transform.SetParent(null);

        // Set the visual effect to die after 5 seconds
        Destroy(enemyHitEffect.gameObject, 5f);

        // Force the hasPlayedHitVFX flag to false
        _hasPlayedHitVFX = false;

        // Play the visual effect
        PlayVFXAfterDamage();
    }

    public void ChangeHealth(float amount, IActor changer, IDamager damager, Vector3 position, bool isCriticalHit = false)
    {
        // Clamp the health value between 0 and the max health
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

        HealthChangedEventArgs args;

        // If the amount is less than 0, invoke the OnDamaged event
        if (amount < 0)
        {
            args = new HealthChangedEventArgs(this, changer, damager, -amount, position, isCriticalHit);
            OnDamaged?.Invoke(this, args);
        }

        // If the amount is greater than 0, invoke the OnHealed event
        else if (amount > 0)
        {
            args = new HealthChangedEventArgs(this, changer, damager, amount, position, false);
            OnHealed?.Invoke(this, args);
        }

        // If the amount is 0, do nothing
        else
            return;

        // If the amount is less than 0 and the enemy is already dead, return
        if (amount < 0 && _isDead)
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

        // Implement death logic
        Destroy(gameObject);
    }

    public void Stun(HealthChangedEventArgs e, float duration)
    {
        // Return if the duration is less than or equal to 0
        if (duration <= 0)
            return;

        // Return if the enemy is already stunned
        if (_remainingStunTime > 0)
            return;

        var isStunned = _remainingStunTime > 0;

        _remainingStunTime = Mathf.Max(_remainingStunTime, duration);

        // TODO: Replace w/ coroutine
        if (!isStunned)
            OnStunStart?.Invoke(e, duration);

        // if (!isStunned)
        //     StartCoroutine(StunCoroutine(e, duration));
    }

    public void StopStun()
    {
        var isStunned = _remainingStunTime > 0;

        // Reset the remaining stun time
        _remainingStunTime = 0;

        if (isStunned)
            OnStunEnd?.Invoke(null, 0);
    }

    private IEnumerator StunCoroutine(HealthChangedEventArgs e, float duration)
    {
        // Invoke the OnStunned event
        OnStunStart?.Invoke(e, duration);

        // Wait for the duration of the stun
        var stunStartTime = Time.time;

        while (Time.time - stunStartTime < duration)
        {
            _remainingStunTime -= Time.deltaTime;

            yield return null;
        }

        // Invoke the OnStunEnd event
        OnStunEnd?.Invoke(e, duration);
    }
}