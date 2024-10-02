using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(EnemyInfo))]
public class ProximityEnemy : MonoBehaviour, IEnemyBehavior
{
    private EnemyInfo _enemyInfo;

    [Header("Stats")]
    [Tooltip("How far away the player has to be for the enemy to detect them and begin to blow up.")]
    [SerializeField]
    [Min(0)]
    private float detectionRadius = 5f;

    [Tooltip("How long it takes after the enemy detects the player for the enemy to explode.")]
    [SerializeField]
    [Min(0)]
    private float explosionDelay;

    [Tooltip("How far away the explosion will reach.")] [SerializeField] [Min(0)]
    private float explosionRange = 5f;

    [Tooltip("How much force the explosion will apply to the player.")] [SerializeField] [Min(0)]
    private float explosiveForce = 1500f;

    [SerializeField] [Min(0)] private float explosionDamage = 5f;

    [Header("Visuals")] [SerializeField] private ParticleSystem explosionParticles;
    [SerializeField] [Range(0, 500)] private int explosionParticlesCount = 30;

    [SerializeField] private ParticleSystem fuseParticles;

    /// <summary>
    /// A boolean to determine if this enemy will blow up soon;
    /// Used to prevent the enemy from blowing up multiple times.
    /// </summary>
    private bool _isActivated;

    /// <summary>
    /// A boolean to determine if this enemy has already blown up.
    /// Used to prevent the enemy from blowing up multiple times.
    /// </summary>
    private bool _hasExploded;

    /// <summary>
    /// How long the enemy has been activated for.
    /// </summary>
    private float _currentFuseTime;

    /// <summary>
    /// A reference to the fuse particles that are instantiated once the enemy is activated.
    /// </summary>
    private ParticleSystem _fuseParticlesInstance;

    public GameObject GameObject => gameObject;

    #region Initialization Functions

    private void Awake()
    {
        // Get the components
        GetComponents();
    }

    private void Start()
    {
    }

    private void GetComponents()
    {
        // Get the EnemyInfo component
        _enemyInfo = GetComponent<EnemyInfo>();
    }

    #endregion

    private void Update()
    {
        // Check if the player is in range
        CheckForPlayerInRange();

        // If the enemy is activated, increment the fuse time
        if (_isActivated)
            _currentFuseTime += Time.deltaTime;

        // If the fuse time is greater than the explosion delay, explode
        if (_currentFuseTime >= explosionDelay)
            Explode();
    }

    private void CheckForPlayerInRange()
    {
        // Get all the test players in the scene
        var players = FindObjectsOfType<TestPlayer>();

        // Sort them based on their distance from the enemy
        Array.Sort(players, (player1, player2) =>
        {
            var distance1 = Vector3.Distance(transform.position, player1.transform.position);
            var distance2 = Vector3.Distance(transform.position, player2.transform.position);

            return distance1.CompareTo(distance2);
        });

        // If the closest player is not within range, return
        if (Vector3.Distance(transform.position, players[0].transform.position) > detectionRadius)
            return;

        // Start the activation process
        StartActivation();
    }

    private void StartActivation()
    {
        // Return if the enemy is already activated
        if (_isActivated)
            return;

        // Activate the enemy
        _isActivated = true;

        // Reset the fuse time
        _currentFuseTime = 0;

        // Start the fuse particles
        StartFuseParticles();
    }

    private void StartFuseParticles()
    {
        // Return if the fuse particles are null
        if (fuseParticles == null)
            return;

        // Create a new instance of the fuse particles at the enemy's position
        _fuseParticlesInstance = Instantiate(fuseParticles, transform.position, Quaternion.identity);

        // Emit the fuse particles
        _fuseParticlesInstance.Play();
    }

    private void Explode()
    {
        // Return if the enemy has already exploded
        if (_hasExploded)
            return;

        // Create the explosion particles
        CreateExplosionParticles();

        // Create the explosive force
        CreateExplosiveForce();

        // Kill the enemy by changing the health to 0
        _enemyInfo.ChangeHealth(-_enemyInfo.MaxHealth);
    }

    private void CreateExplosiveForce()
    {
        // Get all the players in the scene
        var players = FindObjectsOfType<PlayerInfo>();

        // For each player
        foreach (var player in players)
        {
            // Skip the player if they are not within the explosion range
            if (Vector3.Distance(transform.position, player.transform.position) > explosionRange)
                continue;

            // Change the health of the player
            player.ChangeHealth(-explosionDamage);

            // Get the rigidbody component from the player
            if (!player.transform.root.TryGetComponent(out Rigidbody rb))
                continue;

            // Apply the explosive force to the player
            rb.AddExplosionForce(explosiveForce, transform.position, explosionRange);
        }
    }

    private void CreateExplosionParticles()
    {
        // Return if the explosion particles are null
        if (explosionParticles == null)
            return;

        // Create a new instance of the explosion particles at the enemy's position
        var explosion = Instantiate(explosionParticles, transform.position, Quaternion.identity);

        // Create emit parameters for the explosion particles
        var emitParams = new ParticleSystem.EmitParams
        {
            applyShapeToPosition = true,
            position = transform.position
        };

        // Emit the explosion particles
        explosion.Emit(emitParams, explosionParticlesCount);

        // Set the explosion particles to be destroyed after the duration
        Destroy(explosion.gameObject, explosion.main.duration);

        // Stop the fuse particles
        _fuseParticlesInstance.Stop();

        // Set the fuse particles to be destroyed after the duration
        Destroy(_fuseParticlesInstance.gameObject, explosion.main.duration);
    }

    private void OnDestroy()
    {
        // If the fuse particles instance is not null, destroy it
        if (_fuseParticlesInstance != null)
            Destroy(_fuseParticlesInstance.gameObject);
    }

    private void OnDrawGizmos()
    {
        // Draw a circle around the enemy to represent the detection radius
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Draw a circle around the enemy to represent the explosion radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }
}