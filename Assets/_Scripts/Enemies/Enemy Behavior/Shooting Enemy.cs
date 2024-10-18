using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ShootingEnemy : MonoBehaviour, IEnemyBehavior, IDamager
{
    private EnemyInfo _enemyInfo;

    [Header("Stats")]
    [Tooltip("How far away the player has to be for the enemy to detect them and begin to fire.")]
    [SerializeField]
    [Min(0)]
    private float detectionRadius = 5f;

    [Tooltip("How long it takes after the enemy detects the player for the enemy to shoot.")]
    [SerializeField]
    [Min(0)]
    private float fireDelay;
    public GameObject GameObject => gameObject;

    public GameObject enemyBullet;
    public Transform spawnPoint;
    public float enemySpeed;

    [SerializeField] private float timer = 5;
    private float bulletTime;

    private GameObject target;


    /// <summary>
    /// A boolean to determine if this enemy will blow up soon;
    /// Used to prevent the enemy from blowing up multiple times.
    /// </summary>
    private bool _isActivated;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the player is in range
        CheckForPlayerInRange();

    }
    void shootAtPlayer()
    {
        bulletTime -= Time.deltaTime;
        if (bulletTime > 0) return;
        bulletTime = timer;
        GameObject bulletObj = Instantiate(enemyBullet, spawnPoint.transform.position, spawnPoint.transform.rotation) as GameObject;
        Rigidbody bulletRig = bulletObj.GetComponent<Rigidbody>();

        var direction = target.transform.position - transform.position;
        bulletRig.AddForce(direction.normalized*enemySpeed,ForceMode.VelocityChange);
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
        target = players[0].gameObject;

        // Fire at player
        shootAtPlayer();
    }
    private void OnDrawGizmos()
    {
        // Draw a circle around the enemy to represent the detection radius
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
    }
