using System.Collections;
using UnityEngine;

public class BossFireballProjectile : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField, Min(0.001f)] private float startingScale = 1 / 16f;
    [SerializeField, Min(0.001f)] private float maxScale = 1;
    [SerializeField] private float damage = 100f;

    #endregion

    #region Private Fields

    private IEnemyAttackBehavior _attackBehavior;
    private bool _isMarkedForDestruction;

    private Rigidbody _rigidbody;
    private float _velocity;
    private Vector3 _direction;

    private bool _isActive;

    #endregion

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
        // Return if not active
        if (!_isActive)
            return;

        // Return if this is marked for destruction
        if (_isMarkedForDestruction)
            return;

        // Return if the other collider is the shooter
        if (other.TryGetComponentInParent(out IEnemyAttackBehavior attackBehavior) && attackBehavior == _attackBehavior)
            return;

        var hasActor = other.TryGetComponentInParent(out IActor actor);

        // Return if the actor is the shooter
        if (actor as EnemyInfo == _attackBehavior.Enemy.EnemyInfo)
            return;

        // Return if the actor is an enemy
        if (actor is EnemyInfo)
            return;

        Destroy(gameObject);

        // Return if the other collider is not an actor
        if (!hasActor)
            return;

        // Damage the player
        actor.ChangeHealth(-damage, actor, _attackBehavior, transform.position);
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
            transform.localScale = Vector3.Lerp(startScale, targetScaleVector, (Time.time - startTime) / duration);

            // If the target is not null, set the forward direction of the projectile to the direction of the target
            if (target != null)
                transform.forward = target.position - transform.position;

            yield return null;
        }

        transform.localScale = targetScaleVector;
    }

    public IEnumerator CreateProjectile(IEnemyAttackBehavior attackBehavior, float time, Transform target)
    {
        // Set the enemy attack behavior
        _attackBehavior = attackBehavior;

        // Set the scale of the projectile to the starting scale
        var scale = new Vector3(startingScale, startingScale, startingScale);
        transform.localScale = scale;

        // Wait while the projectile scales up
        yield return StartCoroutine(ScaleSize(maxScale, time, target));
    }

    public void FireProjectile(Vector3 direction, float velocity, float lifetime)
    {
        // Set the parent of the projectile to null
        transform.SetParent(null);

        // Set the isActive flag to true
        _isActive = true;

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