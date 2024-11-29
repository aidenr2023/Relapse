using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ShootingEnemy : MonoBehaviour, IEnemyAttackBehavior
{
    private EnemyInfo _enemyInfo;

    #region Serialized Fields

    [Header("Stats")]
    [Tooltip("How far away the player has to be for the enemy to detect them and begin to fire.")]
    [SerializeField]
    [Min(0)]
    private float detectionRadius = 5f;

    [Tooltip("How long it takes after the enemy detects the player for the enemy to shoot.")] [SerializeField] [Min(0)]
    private float fireDelay;

    [SerializeField] private GameObject enemyBullet;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float enemySpeed;
    [SerializeField] private float timer = 5;

    #endregion

    #region Private Fields

    private float _bulletTime;

    private GameObject _target;

    private bool _isExternallyEnabled = true;

    #endregion

    #region Getters

    public GameObject GameObject => gameObject;

    public Enemy Enemy { get; private set; }

    public bool IsAttackEnabled => _isExternallyEnabled;

    #endregion

    /// <summary>
    /// A boolean to determine if this enemy will blow up soon;
    /// Used to prevent the enemy from blowing up multiple times.
    /// </summary>
    private bool _isActivated;

    private void Awake()
    {
        // Get the enemy component
        Enemy = GetComponent<Enemy>();
    }

    // Update is called once per frame
    private void Update()
    {
        // Check if the player is in range
        CheckForPlayerInRange();
    }

    private void ShootAtPlayer()
    {
        // If the fire delay has not passed, return
        _bulletTime -= Time.deltaTime;

        if (_bulletTime > 0)
            return;

        // Reset the fire delay
        _bulletTime = timer;

        // Instantiate the bullet
        var bulletObj = Instantiate(enemyBullet, spawnPoint.transform.position, spawnPoint.transform.rotation);
        var bulletRig = bulletObj.GetComponent<Rigidbody>();

        // Fire the bullet at the player
        var direction = _target.transform.position - transform.position;
        bulletRig.AddForce(direction.normalized * enemySpeed, ForceMode.VelocityChange);

        // Destroy the bullet after 5 seconds
        Destroy(bulletObj, 5f);
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

        _target = players[0].gameObject;

        // Fire at player
        ShootAtPlayer();
    }

    private void OnDrawGizmos()
    {
        // Draw a circle around the enemy to represent the detection radius
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }


    public void SetAttackEnabled(bool on)
    {
        _isExternallyEnabled = on;
    }
}