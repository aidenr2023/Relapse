using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

[RequireComponent(typeof(EnemyInfo))]
public class ProximityEnemy : MonoBehaviour, IEnemyAttackBehavior
{
    #region Serialized Fields

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

    [Header("Visuals")]

    [SerializeField] private ParticleSystem fuseParticles;

    [SerializeField] private VisualEffect explosionVFX;

    #endregion

    #region Private Fields

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

    #endregion

    #region Getters

    public GameObject GameObject => gameObject;

    public Enemy Enemy { get; private set; }

    #endregion

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
        // Get the Enemy component
        Enemy = GetComponent<Enemy>();
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
        var players = FindObjectsOfType<Player>();

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

        _fuseParticlesInstance.transform.parent = transform;

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
        Enemy.EnemyInfo.ChangeHealth(-Enemy.EnemyInfo.MaxHealth, Enemy.EnemyInfo, this);
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
            player.ChangeHealth(-explosionDamage, Enemy.EnemyInfo, this);

            // Get the rigidbody component from the player
            if (!player.TryGetComponentInParent(out Rigidbody rb))
                continue;

            // Apply the explosive force to the player
            rb.AddExplosionForce(explosiveForce, transform.position, explosionRange);
        }
    }

    private void CreateExplosionParticles()
    {
        // Return if the explosion particles are null
        if (explosionVFX == null)
            return;

        // Create the explosion VFX
        var fx = Instantiate(explosionVFX, transform.position, Quaternion.identity);

        var duration = fx.GetFloat("Duration");

        // Set the explosion VFX to be destroyed after the duration
        Destroy(fx.gameObject, duration);

        Debug.Log($"FX: {duration} - {fx}");
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