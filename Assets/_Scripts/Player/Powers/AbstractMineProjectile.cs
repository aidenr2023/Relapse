using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractMineProjectile : MonoBehaviour, IPowerProjectile
{
    protected const int MAX_ENEMIES = 64;

    #region Serialized Fields

    [SerializeField] protected LayerMask layersToIgnoreCollision;

    [SerializeField] protected LayerMask enemyLayers;

    [SerializeField, Min(0)] protected float shootForce = 10f;

    [SerializeField, Min(0)] protected float detectionRadius = 5f;
    [SerializeField, Min(0)] protected float explosionRadius = 5f;

    [SerializeField, Min(0)] protected float fuseTime = 2f;
    [SerializeField, Min(0)] protected float inactiveDestroyTime = 10f;

    [SerializeField] private bool destroyOnExplode = true;

    #endregion

    #region Private Fields

    protected IActor _shooter;
    protected IPower _power;

    protected Rigidbody _rigidbody;
    protected Collider _collider;

    protected bool _isAttached;
    protected bool _isFuseLit;
    protected bool _hasExploded;

    protected CountdownTimer _fuseTimer;
    protected CountdownTimer _inactiveDestroyTimer;

    #endregion

    protected void Awake()
    {
        // Get the rigidbody component
        _rigidbody = GetComponent<Rigidbody>();

        // Get the collider component
        _collider = GetComponent<Collider>();

        // Create the fuse timer
        _fuseTimer = new CountdownTimer(fuseTime);

        // Create the inactive destroy timer
        _inactiveDestroyTimer = new CountdownTimer(inactiveDestroyTime);
        _inactiveDestroyTimer.OnTimerEnd += () => Destroy(gameObject);

        // Custom awake function
        CustomAwake();
    }

    protected abstract void CustomAwake();

    protected void Update()
    {
        // Update the fuse timer
        _fuseTimer.SetActive(_isFuseLit);
        _fuseTimer.SetMaxTime(fuseTime);
        _fuseTimer.Update(Time.deltaTime);

        // Update the inactive destroy timer
        _inactiveDestroyTimer.SetActive(!_isFuseLit);
        _inactiveDestroyTimer.SetMaxTime(inactiveDestroyTime);
        _inactiveDestroyTimer.Update(Time.deltaTime);

        // Custom update function
        CustomUpdate();
    }

    protected abstract void CustomUpdate();

    private void DetectEnemies()
    {
        // Return if the projectile is not attached
        if (!_isAttached)
            return;

        // Return if the fuse is already lit
        if (_isFuseLit)
            return;

        var enemies = new Collider[MAX_ENEMIES];

        // Get the game objects in the enemyLayers
        var enemyCount = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, enemies, enemyLayers);

        var actorSet = new HashSet<IActor>();

        // For each enemy, get the actor component
        for (var i = 0; i < enemyCount; i++)
        {
            // Get the enemy
            var enemy = enemies[i];

            // Get the actor component from the enemy
            var actor = enemy.GetComponentInParent<IActor>();

            // Continue if the actor is null
            if (actor == null)
                continue;

            // If the actor is the shooter, continue
            if (actor == _shooter)
                continue;

            actorSet.Add(actor);
        }

        // Return if there are no enemies
        if (enemyCount == 0)
            return;

        // Loop through the enemies
        foreach (var actor in actorSet)
        {
            // Continue if the actor is null
            if (actor == null)
                continue;

            // If the actor is the shooter, continue
            if (actor == _shooter)
                continue;

            // Get the direction to the enemy
            var direction = actor.GameObject.transform.position - transform.position;

            // Get the distance to the enemy
            var distance = direction.magnitude;

            // Normalize the direction
            direction.Normalize();

            // Check if the enemy is within the distance
            // If they are, start the fuse
            if (distance <= detectionRadius)
                StartFuse();

            if (_isFuseLit)
                break;
        }
    }

    private void StartFuse()
    {
        // Set the isFuseLit to true
        _isFuseLit = true;

        // TODO: Start some vfx

        // OnStartFuse
        OnStartFuse();
    }

    protected abstract void OnStartFuse();

    protected void FixedUpdate()
    {
        // Check the distance
        DetectEnemies();

        // Explode when ready
        Explode();

        // Custom fixed update function
        CustomFixedUpdate();
    }

    protected abstract void CustomFixedUpdate();

    private void Explode()
    {
        // Return if the projectile is not attached
        if (!_isAttached)
            return;

        // Return if the fuse is not lit
        if (!_isFuseLit)
            return;

        // Return if the timer is not done
        if (_fuseTimer.IsNotComplete)
            return;

        // Return if the projectile has already exploded
        if (_hasExploded)
            return;

        // Set the hasExploded to true
        _hasExploded = true;

        var enemies = new Collider[MAX_ENEMIES];

        // Get the game objects in the enemyLayers
        var enemyCount = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, enemies, enemyLayers);

        var actorSet = new HashSet<IActor>();

        // For each enemy, get the actor component
        for (var i = 0; i < enemyCount; i++)
        {
            // Get the enemy
            var enemy = enemies[i];

            // Get the actor component from the enemy
            var actor = enemy.GetComponentInParent<IActor>();

            // Continue if the actor is null
            if (actor == null)
                continue;

            // If the actor is the shooter, continue
            if (actor == _shooter)
                continue;

            actorSet.Add(actor);
        }

        // Return if there are no enemies
        if (enemyCount == 0)
            return;

        // Loop through the enemies
        foreach (var actor in actorSet)
        {
            // Continue if the actor is null
            if (actor == null)
                continue;

            // If the actor is the shooter, continue
            if (actor == _shooter)
                continue;

            // Check if the enemy is in range
            // Deal damage to the enemy
            if (Vector3.Distance(transform.position, actor.GameObject.transform.position) <= explosionRadius)
                ApplyExplosion(actor);
        }

        // TODO: Play explosion vfx

        // Destroy the game object
        if (destroyOnExplode)
            Destroy(gameObject);
    }

    protected abstract void ApplyExplosion(IActor actor);

    private void OnCollisionEnter(Collision other)
    {
        // Return if the projectile is already attached
        if (_isAttached)
            return;

        // Return if the fuse is already lit
        if (_isFuseLit)
            return;

        // Ignore if the layer is in the layersToIgnore
        if (layersToIgnoreCollision == (layersToIgnoreCollision | (1 << other.gameObject.layer)))
            return;

        // Get the normal of the collision
        var normal = other.contacts[0].normal;

        // Attach to the surface
        AttachToSurface(other.contacts[0].point, normal);
    }

    private void AttachToSurface(Vector3 position, Vector3 normal)
    {
        // Set the rigidbody to kinematic
        _rigidbody.isKinematic = true;

        // Set the collider to trigger
        _collider.isTrigger = true;

        // Set the position of the game object to the position parameter
        transform.position = position;

        // Set the up of the game object to the normal parameter
        transform.up = normal;

        // Set the isAttached to true
        _isAttached = true;
    }

    public void Shoot(IPower power, PlayerPowerManager powerManager, PowerToken pToken, Vector3 position,
        Vector3 forward)
    {
        // Set the power
        _power = power;

        // Set the shooter
        _shooter = powerManager.Player.PlayerInfo;

        // Set the position of the game object to the position parameter
        transform.position = position;

        // Set the forward of the game object to the forward parameter
        transform.forward = forward;

        // Set the velocity of the rigidbody to the forward vector
        _rigidbody.velocity = forward * shootForce;

        // Custom shoot function
        CustomShoot(power, powerManager, pToken, position, forward);
    }

    protected abstract void CustomShoot(IPower power, PlayerPowerManager powerManager, PowerToken pToken,
        Vector3 position,
        Vector3 forward);

    private void OnDrawGizmos()
    {
        // Draw the detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Draw the explosion radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}