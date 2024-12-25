using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingEnemyProjectile : MonoBehaviour
{
    [SerializeField] private float damage = 100f;

    private ShootingEnemyAttack _shootingEnemyAttack;

    private bool _isMarkedForDestruction;

    private Rigidbody _rigidbody;
    private float _velocity;
    private Vector3 _direction;

    private void Awake()
    {
        // Get the rigidbody component
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void LateUpdate()
    {
        if (_isMarkedForDestruction)
            Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        // Set the forward direction of the projectile to the direction of the projectile
        transform.forward = _direction.normalized;

        // Set the velocity of the rigidbody to the direction of the projectile
        _rigidbody.velocity = _direction.normalized * _velocity;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Return if the other collider is the shooter
        if (other.TryGetComponentInParent(out ShootingEnemyAttack shootingEnemyAttack) &&
            shootingEnemyAttack == _shootingEnemyAttack
           )
            return;

        // Return if this is marked for destruction
        if (_isMarkedForDestruction)
            return;

        // Mark this for destruction
        _isMarkedForDestruction = true;

        // Return if the other collider is not an actor
        if (!other.TryGetComponent(out IActor actor))
            return;

        // Damage the player
        actor.ChangeHealth(-damage, actor, _shootingEnemyAttack);
    }

    public void Shoot(ShootingEnemyAttack shootingEnemyAttack, Vector3 direction, float velocity, float lifetime)
    {
        _shootingEnemyAttack = shootingEnemyAttack;
        _direction = direction;
        _velocity = velocity;

        // Set the forward direction of the projectile to the direction of the projectile
        transform.forward = _direction.normalized;

        // Set the velocity of the rigidbody to the direction of the projectile
        _rigidbody.velocity = direction.normalized * velocity;

        // Set the lifetime of the projectile
        Destroy(gameObject, lifetime);
    }
}