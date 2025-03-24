using System;
using System.Collections;
using UnityEngine;

public class BossVirusProjectile : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private ParticleSystem explosionParticles;
    [SerializeField] [Range(0, 500)] private int explosionParticlesCount = 200;

    [SerializeField, Min(0.001f)] private float startingScale = 1 / 16f;
    [SerializeField, Min(0.001f)] private float maxScale = 1;

    #endregion

    #region Private Fields

    private bool _isActive;

    private Rigidbody _rigidbody;
    private Vector3 _direction;

    private BossVirusBehavior _attackBehavior;
    private Transform _target;

    #endregion

    private void Awake()
    {
        // Get the rigidbody component
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!_isActive)
            return;

        // Set the forward direction of the projectile
        transform.forward = _direction;

        // Set the velocity of the projectile
        _rigidbody.velocity = _direction * _attackBehavior.ProjectileVelocity;
    }

    public IEnumerator CreateProjectile(BossVirusBehavior attackBehavior, float time, Transform target)
    {
        // Turn off the gravity for the projectile
        _rigidbody.useGravity = false;

        // Set the rigidbody to kinematic
        _rigidbody.isKinematic = true;

        // Set the enemy attack behavior
        _attackBehavior = attackBehavior;
        _target = target;

        // Set the scale of the projectile to the starting scale
        var scale = new Vector3(startingScale, startingScale, startingScale);
        transform.localScale = scale;

        // Wait while the projectile scales up
        yield return StartCoroutine(ScaleSize(maxScale, time, target));
    }

    private IEnumerator ScaleSize(float targetScale, float duration, Transform target)
    {
        var cScale = transform.localScale.x;

        // Assume the scale is uniform. it SHOULD be uniform really
        var startScale = new Vector3(cScale, cScale, cScale);

        var targetScaleVector = new Vector3(targetScale, targetScale, targetScale);

        var startTime = Time.time;

        while (Time.time - startTime < duration)
        {
            transform.localPosition = Vector3.zero;

            transform.localScale = Vector3.Lerp(startScale, targetScaleVector, (Time.time - startTime) / duration);

            // If the target is not null, set the forward direction of the projectile to the direction of the target
            if (target != null)
                transform.forward = target.position - transform.position;

            yield return null;
        }

        transform.localScale = targetScaleVector;
    }

    public IEnumerator ShootProjectile()
    {
        // Set the parent to null
        transform.SetParent(null);

        // Set the isActive flag to true
        _isActive = true;

        // Turn off gravity for the projectile
        _rigidbody.useGravity = false;

        // Set the rigidbody to non-kinematic
        _rigidbody.isKinematic = false;

        _direction = (_target.position - transform.position).normalized;

        // Set the forward direction of the projectile
        transform.forward = _direction;

        // Set the velocity of the projectile
        _rigidbody.velocity = transform.forward * _attackBehavior.ProjectileVelocity;

        yield return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isActive)
            return;

        // Return if the projectile hits sender of the projectile
        if (other.gameObject == _attackBehavior.gameObject)
            return;

        // Return if the other object is a trigger
        if (other.isTrigger)
            return;

        // If the projectile hits something with an IActor component, deal damage
        if (other.TryGetComponentInParent(out PlayerInfo actor))
        {
            var damage = _attackBehavior.ProjectileDamage *
                         _attackBehavior.BossEnemyAttack.Enemy.EnemyInfo.DifficultyDamageMultiplier;

            actor.ChangeHealth(
                -damage, _attackBehavior.BossEnemyAttack.ParentComponent.ParentComponent,
                _attackBehavior.BossEnemyAttack, transform.position
            );
        }

        // Explode the projectile
        Explode();
    }

    private void Explode()
    {
        // Instantiate the boss virus cloud
        var cloud = Instantiate(_attackBehavior.VirusCloud, transform.position, Quaternion.identity);
        cloud.Initialize(_attackBehavior);

        // Destroy the cloud after a certain amount of time
        Destroy(cloud.gameObject, _attackBehavior.VirusCloudDuration);

        // Create the explosion particles
        CreateExplosionParticles();

        // Destroy the game object
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
    }
}