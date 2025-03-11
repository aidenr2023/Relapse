using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AndreEnemy : MonoBehaviour, IActor, IDamager
{
    private GameObject _player;

    [Header("Enemy Settings")] public float enemySpeed = 5f;
    public float explosionRange = 2f;
    public float enemyHealth = 4;
    private float _maxHealth;

    [Header("Explosion")] [SerializeField] private GameObject explosionEffect; // Assign a particle system prefab here
    [SerializeField] private AudioSource explosionSound;
    private bool _isExploding;

    public GameObject GameObject => gameObject;

    public float MaxHealth => _maxHealth;
    public float CurrentHealth => enemyHealth;
    
    public Sound NormalHitSfx => null;
    public Sound CriticalHitSfx => null;

    #region Events

    public event HealthChangedEventHandler OnDamaged;
    public event HealthChangedEventHandler OnHealed;
    public event HealthChangedEventHandler OnDeath;

    #endregion

    private void Awake()
    {
        // Set the enemy's max health to the initial health value
        _maxHealth = enemyHealth;

        // Find the player object in the scene
        _player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Start()
    {
        // Ensure the enemy has a SphereCollider set as a trigger
        var triggerCollider = gameObject.AddComponent<SphereCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = explosionRange;
    }

    private void Explode()
    {
        // Set the exploding flag to true
        _isExploding = true;

        // Get the IActor component from the player object
        var playerInfo = _player.GetComponent<IActor>();

        if (playerInfo != null)
            playerInfo.ChangeHealth(-1, this, this, transform.position);

        // Instantiate the explosion effect at the enemy's position
        if (explosionEffect != null)
        {
            var explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            var ps = explosion.GetComponent<ParticleSystem>();

            // Play the explosion sound
            if (ps != null)
            {
                ps.Play();
                explosionSound.Play();

                // Destroy the particle system after it finishes playing
                Destroy(explosion, ps.main.duration);
            }

            else
            {
                explosionSound.Play();

                // Destroy the explosion effect after 2 seconds
                Destroy(explosion, 2f); // Fallback in case there's no ParticleSystem component
            }
        }

        // Deactivate the enemy
        Destroy(gameObject);
    }

    private void TakeDamage(float damageAmount, IActor changer, IDamager damager, Vector3 position)
    {
        enemyHealth -= damageAmount;

        // Invoke the OnDamaged event
        var args = new HealthChangedEventArgs(this, changer, damager, damageAmount, position);
        OnDamaged?.Invoke(this, args);

        if (enemyHealth <= 0)
        {
            // Invoke the OnDeath event
            OnDeath?.Invoke(this, args);

            explosionSound.Play();
            Destroy(gameObject);
        }
    }


    private void FixedUpdate()
    {
        MoveTowardTarget();
    }

    private void MoveTowardTarget()
    {
        // Return if there is no player or the enemy is exploding
        if (_player == null || _isExploding)
            return;

        // Get the direction from the enemy to the player
        var direction = _player.transform.position - transform.position;

        // Ignore the y-axis to prevent the enemy from flying up or down
        direction.y = 0;

        // Move the enemy towards the player
        transform.position += direction.normalized * (enemySpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !_isExploding)
            Explode();
    }


    public void ChangeHealth(float amount, IActor changer, IDamager damager, Vector3 position, bool isCriticalHit)
    {
        if (amount < 0)
            TakeDamage(-amount, changer, damager, position);
        else
            enemyHealth = Mathf.Clamp(enemyHealth + amount, 0, _maxHealth);
    }
}