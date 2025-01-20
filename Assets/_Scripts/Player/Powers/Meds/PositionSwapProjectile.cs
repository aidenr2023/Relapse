using UnityEngine;
using UnityEngine.VFX;

public class PositionSwapProjectile : MonoBehaviour, IPowerProjectile
{
    private Vector3 _forward;

    private Rigidbody _rigidbody;

    private PositionSwap _fireball;
    private PlayerPowerManager _powerManager;
    private PowerToken _pToken;

    private bool _isExploded;

    [SerializeField] private float speed = 10f;
    [SerializeField] private float damage = 100f;

    [SerializeField] [Min(0)] private float lifetime = 10;

    [SerializeField] private ParticleSystem explosionParticles;
    [SerializeField] [Range(0, 500)] private int explosionParticlesCount = 200;

    [SerializeField] private VisualEffect fireballVFXPrefab;
    [SerializeField] private VisualEffect explosionVFXPrefab;

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
        _fireball = (PositionSwap)power;
        _powerManager = powerManager;
        _pToken = pToken;

        // Set the position of the game object to the position parameter
        transform.position = position;

        // Set the forward of the game object to the forward parameter
        transform.forward = _forward = forward;

        // Set the projectile to explode in 10 seconds
        Invoke(nameof(Explode), lifetime);

        // // Instantiate the fireball VFX prefab
        // var fireballVFX = Instantiate(fireballVFXPrefab, transform.position, Quaternion.identity);
        //
        // // Set the parent of the fireball VFX to the game object
        // fireballVFX.transform.parent = transform;
        // fireballVFX.transform.localPosition = Vector3.zero;
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
        if (other.TryGetComponentInParent(out IActor actor))
        {
            // actor.ChangeHealth(-damage, _powerManager.Player.PlayerInfo, _fireball, transform.position);

            // Get the position of the shooter
            var shooterPosition = _powerManager.Player.PlayerInfo.transform.position;

            // Get the position of the actor
            var actorPosition = actor.GameObject.transform.position;

            // Swap the positions of the shooter and the actor
            if (actor is EnemyInfo enemyInfo)
            {
                // Get the enemy controller
                var enemyMovement = enemyInfo.ParentComponent.EnemyMovementBehavior;

                // Swap the positions of the shooter and the actor
                enemyMovement.SetPosition(shooterPosition);
            }

            // Set the position of the actor to the shooter's position
            _powerManager.Player.PlayerInfo.transform.position = actorPosition;
        }

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