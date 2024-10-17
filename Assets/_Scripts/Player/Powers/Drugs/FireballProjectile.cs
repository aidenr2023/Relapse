using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FireballProjectile : MonoBehaviour, IPowerProjectile
{
    private Vector3 _forward;

    private Rigidbody _rigidbody;

    private Fireball _fireball;
    private PlayerPowerManager _powerManager;
    private PowerToken _pToken;

    private bool _isExploded;

    [SerializeField] private float speed = 10f;
    [SerializeField] private float damage = 100f;

    [SerializeField] private ParticleSystem explosionParticles;
    [SerializeField] [Range(0, 500)] private int explosionParticlesCount = 200;


    private void Awake()
    {
        // Get the rigidbody component
        _rigidbody = GetComponent<Rigidbody>();

        // Make the current collider a trigger
        var projectileCollider = GetComponent<Collider>();
        projectileCollider.isTrigger = true;
    }

    public void Shoot(
        IPower power, PlayerPowerManager powerManager, PowerToken pToken,
        Vector3 position, Vector3 forward
    )
    {
        _fireball = (Fireball)power;
        _powerManager = powerManager;
        _pToken = pToken;

        // Set the position of the game object to the position parameter
        transform.position = position;

        // Set the forward of the game object to the forward parameter
        transform.forward = _forward = forward;
        
        // Set the projectile to explode in 10 seconds
        Invoke(nameof(Explode), 10f);
    }

    private void FixedUpdate()
    {
        // Set the velocity of the rigidbody to the forward vector
        _rigidbody.velocity = _forward * speed;
    }


    private void OnTriggerEnter(Collider other)
    {
        // Return if the projectile hits sender of the projectile
        if (other.gameObject == _powerManager.gameObject)
            return;

        // Return if the other object is a trigger
        if (other.isTrigger)
            return;

        // Return if the projectile has already exploded
        if (_isExploded)
            return;

        // If the projectile hits something with an IActor component, deal damage
        if (other.transform.root.TryGetComponent(out IActor actor))
            actor.ChangeHealth(-damage, _powerManager.Player.PlayerInfo, _fireball);

        // Destroy the projectile when it hits something
        // Debug.Log($"BOOM! {gameObject.name} hit {other.name}");

        // Explode the projectile
        Explode();
    }

    private void CreateExplosionParticles()
    {
        // Instantiate the explosion particles at the projectile's position
        var explosion = Instantiate(explosionParticles, transform.position, Quaternion.identity);

        // Create emit parameters for the explosion particles
        var emitParams = new ParticleSystem.EmitParams
        {
            applyShapeToPosition = true,
            position = transform.position
        };

        // Emit the explosion particles
        explosion.Emit(emitParams, explosionParticlesCount);
    }

    private void Explode()
    {
        _isExploded = true;

        // Create explosion particles
        CreateExplosionParticles();

        // Destroy the projectile
        Destroy(gameObject);
    }
}