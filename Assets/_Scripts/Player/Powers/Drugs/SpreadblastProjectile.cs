using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpreadblastProjectile : MonoBehaviour, IPowerProjectile
{
    #region Serialized Fields

    [SerializeField, Min(0)] private int projectileCount = 5;

    [SerializeField] private float damage = 100f;

    [SerializeField] private float yLaunchVelocity;
    [SerializeField] private float zLaunchVelocity;

    [SerializeField] private ParticleSystem explosionParticles;
    [SerializeField] [Range(0, 500)] private int explosionParticlesCount = 200;

    [SerializeField] private float despawnTimer;

    [SerializeField, Min(0)] private float randomSpreadX = 30;
    [SerializeField, Min(0)] private float randomSpreadY = 30;

    #endregion

    #region Private Fields

    private Rigidbody _rigidbody;

    private Spreadblast _spreadBlast;
    private PlayerPowerManager _powerManager;
    private PowerToken _pToken;

    private bool _isExploded;

    #endregion

    public int ProjectileCount => projectileCount;

    public void Shoot(IPower power, PlayerPowerManager powerManager, PowerToken pToken, Vector3 position,
        Vector3 forward)
    {
        // Move the projectile to the position parameter
        transform.position = position;

        // Randomize the forward parameter by rotating it by a random amount
        forward = Quaternion.Euler(
            Random.Range(-randomSpreadY, randomSpreadY),
            Random.Range(-randomSpreadX, randomSpreadX),
            0
        ) * forward;

        // Set the forward of the game object to the forward parameter
        transform.forward = forward;

        _spreadBlast = (Spreadblast)power;
        _powerManager = powerManager;
        _pToken = pToken;

        Destroy(gameObject, despawnTimer);

        var rb = GetComponent<Rigidbody>();
        rb.AddForce(transform.up * yLaunchVelocity, ForceMode.VelocityChange);
        rb.AddForce(transform.forward * zLaunchVelocity, ForceMode.VelocityChange);
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
        if (other.TryGetComponentInParent(out IActor actor))
            actor.ChangeHealth(-damage, _powerManager.Player.PlayerInfo, _spreadBlast, transform.position);

        // Explode the projectile
        Explode();
    }

    private void Explode()
    {
        _isExploded = true;

        // Create explosion particles
        CreateExplosionParticles();

        // Destroy the projectile
        Destroy(gameObject);
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
        
        // Destroy the explosion particles after the duration of the main explosion particle system
        Destroy(explosion.gameObject, explosion.main.duration);
    }
}