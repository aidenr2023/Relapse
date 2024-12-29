using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

[RequireComponent(typeof(Enemy))]
public class EnemyInfo : MonoBehaviour, IActor
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

    [Space, SerializeField] private Sound enemyHitSound;
    [SerializeField] private Sound enemyDeathSound;

    #endregion

    #region Private Fields

    private bool _isDead;

    private float _damageThisFrame;
    private Vector3 _damagePosition;
    private bool _hasPlayedHitVFX;

    #endregion

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

        OnDamaged += AddDamageThisFrame;
        OnDamaged += SetDamagePositionOnDamaged;
        OnDamaged += PlaySoundOnDamaged;
        OnDeath += DetachVFXOnDeath;
        OnDeath += PlaySoundOnDeath;

        // Activate the animator's hit trigger
        OnDamaged += (_, args) => animator?.SetTrigger(HitAnimationID);

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

    private void SetDamagePositionOnDamaged(object sender, HealthChangedEventArgs args)
    {
        _damagePosition = args.Position;
    }

    private void PlaySoundOnDamaged(object sender, HealthChangedEventArgs e)
    {
        // Return if the sound is null
        if (enemyHitSound == null)
            return;

        // Return if the enemy's health is less than or equal to 0
        if (currentHealth <= 0)
            return;

        // Play the sound at the enemy's position
        SoundManager.Instance.PlaySfxAtPoint(enemyHitSound, transform.position);
    }

    private void PlaySoundOnDeath(object sender, HealthChangedEventArgs e)
    {
        // Return if the sound is null
        if (enemyDeathSound == null)
            return;

        // Play the sound at the enemy's position
        SoundManager.Instance.PlaySfxAtPoint(enemyDeathSound, transform.position);
    }

    private void AddDamageThisFrame(object sender, HealthChangedEventArgs args)
    {
        _damageThisFrame += args.Amount;
    }

    private void Update()
    {
        // If the enemy took damage this frame, play the visual effect
        PlayVFXAfterDamage();
    }

    private void LateUpdate()
    {
        // Reset the damage this frame if the visual effect has played
        if (_hasPlayedHitVFX)
        {
            _damageThisFrame = 0;
            _hasPlayedHitVFX = false;
        }
    }

    private void PlayVFXAfterDamage()
    {
        // Return if the visual effect is null
        if (enemyHitEffect == null)
            return;

        // If there was no damage this frame, return
        // If the damage this frame is less than the minimum damage for the visual effect, return
        if (_damageThisFrame <= 0 || _damageThisFrame < minVFXDamage)
            return;

        // If hit vfx has already played, return
        if (_hasPlayedHitVFX)
            return;

        // Set the visual effect's position to the position of the damage
        enemyHitEffect.SetVector3("StartPosition", _damagePosition);

        // Calculate the damage percentage based on the amount of damage the enemy took this frame
        var damagePercentage = Mathf.InverseLerp(minVFXRangeDamage, maxVFXRangeDamage, _damageThisFrame);

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

    public void ChangeHealth(float amount, IActor changer, IDamager damager, Vector3 position)
    {
        // Clamp the health value between 0 and the max health
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

        HealthChangedEventArgs args;

        // If the amount is less than 0, invoke the OnDamaged event
        if (amount < 0)
        {
            args = new HealthChangedEventArgs(this, changer, damager, -amount, position);
            OnDamaged?.Invoke(this, args);
        }

        // If the amount is greater than 0, invoke the OnHealed event
        else if (amount > 0)
        {
            args = new HealthChangedEventArgs(this, changer, damager, amount, position);
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