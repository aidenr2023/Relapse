using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

[RequireComponent(typeof(Enemy), typeof(EnemySound))]
public class EnemyInfo : ComponentScript<Enemy>, IActor
{
    private static readonly int HitAnimationID = Animator.StringToHash("Hit");

    #region Serialized Fields

    [Header("Difficulty"), SerializeField] private FloatReference difficultyHealthMultiplier;
    [SerializeField] private FloatReference difficultyDamageMultiplier;
    [SerializeField] private bool applyDifficultyMultiplier = true;

    [Header("Settings"), SerializeField] private float maxHealth = 3f;
    [SerializeField] private float currentHealth;
    [SerializeField] [Min(0)] private int moneyReward;

    [SerializeField] private Animator animator;

    #endregion

    #region Private Fields

    private bool _isDead;

    private float _damageThisFrame;
    private Vector3 _damagePosition;

    private float _remainingStunTime;

    #endregion

    #region Getters

    public GameObject GameObject => gameObject;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    public bool IsStunned => _remainingStunTime > 0;

    public float DamageThisFrame => _damageThisFrame;

    public Vector3 DamagePosition => _damagePosition;

    public float DifficultyDamageMultiplier => applyDifficultyMultiplier ? difficultyDamageMultiplier.Value : 1;

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
    }

    private void Start()
    {
        // Apply the difficulty multiplier to the max health
        if (applyDifficultyMultiplier)
        {
            var oldMaxHealth = maxHealth;
            maxHealth *= difficultyHealthMultiplier.Value;

            ForceCurrentHealth(currentHealth + (maxHealth - oldMaxHealth));
        }

        OnDamaged += AddDamageThisFrame;
        OnDamaged += SetDamagePositionOnDamaged;

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

        OnDeath += AddMoneyOnDeath;
    }

    private void AddMoneyOnDeath(object sender, HealthChangedEventArgs e)
    {
        var playerInventory = Player.Instance.PlayerInventory;
        playerInventory.InventoryVariable.AddItem(playerInventory.InventoryVariable.MoneyObject, moneyReward);
    }

    private void SetDamagePositionOnDamaged(object sender, HealthChangedEventArgs args)
    {
        _damagePosition = args.Position;
    }

    private void AddDamageThisFrame(object sender, HealthChangedEventArgs args)
    {
        _damageThisFrame += args.Amount;
    }

    private void LateUpdate()
    {
        _damageThisFrame = 0;
    }

    public void ChangeHealth(float amount, IActor changer, IDamager damager, Vector3 position,
        bool isCriticalHit = false)
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

    public void ForceCurrentHealth(float health)
    {
        currentHealth = health;
    }

    private void Die()
    {
        // Return if the enemy is already dead
        if (_isDead)
            return;

        // Set the isDead flag to true
        _isDead = true;

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