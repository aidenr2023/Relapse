using System;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(Rigidbody))]
public class FireballProjectile : MonoBehaviour, IPowerProjectile
{
    #region Serialized Fields

    [SerializeField] private float speed = 10f;
    [SerializeField] private float damage = 100f;
    [SerializeField, Min(0)] private float lifetime = 10;
    [SerializeField] private AnimationCurve damageOverTimeCurve;
    [SerializeField] private AnimationCurve sizeOverTimeCurve;

    [Space, SerializeField] private ParticleSystem explosionParticles;
    [SerializeField, Range(0, 500)] private int explosionParticlesCount = 200;

    [SerializeField] private VisualEffect fireballVFXPrefab;
    [SerializeField] private VisualEffect explosionVFXPrefab;

    #endregion

    #region Private Fields

    private Vector3 _forward;

    private Rigidbody _rigidbody;

    private Fireball _fireball;
    private PlayerPowerManager _powerManager;
    private PowerToken _pToken;

    private bool _isExploded;
    private bool _isFired;
    private float _remainingLifeTime;
    private Vector3 _initialScale;

    #endregion

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
        Invoke(nameof(Explode), lifetime);

        // Set the isFired flag to true
        _isFired = true;

        // Set the starting lifetime
        _remainingLifeTime = lifetime;

        // // Instantiate the fireball VFX prefab
        // var fireballVFX = Instantiate(fireballVFXPrefab, transform.position, Quaternion.identity);
        //
        // // Set the parent of the fireball VFX to the game object
        // fireballVFX.transform.parent = transform;
        // fireballVFX.transform.localPosition = Vector3.zero;

        // Set the initial scale of the fireball VFX
        _initialScale = transform.localScale;
    }

    private void Update()
    {
        if (_isFired)
            _remainingLifeTime -= Time.deltaTime;

        // Update the scale of the fireball VFX
        var elapsedTime = lifetime - _remainingLifeTime;
        var evaluatedSize = sizeOverTimeCurve.Evaluate(elapsedTime);
        transform.localScale = new Vector3(
            _initialScale.x + evaluatedSize,
            _initialScale.y + evaluatedSize,
            _initialScale.z + evaluatedSize
        );
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

        // Get the elapsed time that the projectile has been in the air
        var elapsedTime = lifetime - _remainingLifeTime;

        // Get the damage based on the elapsed time
        var elapsedTimeDamage = damageOverTimeCurve.Evaluate(elapsedTime);

        Debug.Log($"Added {elapsedTimeDamage} damage due to elapsed time", this);

        // If the projectile hits something with an IActor component, deal damage
        if (other.TryGetComponentInParent(out IActor actor))
            actor.ChangeHealth(-(damage + elapsedTimeDamage), _powerManager.Player.PlayerInfo, _fireball,
                transform.position);

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

        // Destroy the explosion particles after the duration of the particles
        Destroy(explosion.gameObject, explosion.main.duration);
    }

    private void CreateExplosionVFX()
    {
        // Instantiate the explosion VFX at the projectile's position
        var explosion = Instantiate(explosionVFXPrefab, transform.position, Quaternion.identity);

        // Destroy the explosion VFX after the duration of the VFX
        Destroy(explosion.gameObject, explosion.GetFloat("MaxLifetime"));
    }

    private void Explode()
    {
        _isExploded = true;

        // // Create explosion particles
        CreateExplosionParticles();

        // Create explosion VFX
        // CreateExplosionVFX();

        // Destroy the projectile
        Destroy(gameObject);
    }
}