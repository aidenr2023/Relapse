using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeProjectile : MonoBehaviour, IPowerProjectile
{
    private Vector3 _forward;

    private Rigidbody _rigidbody;

    private Grenade _grenade;
    private PlayerPowerManager _powerManager;
    private PowerToken _pToken;

    private bool _isExploded;

    [SerializeField] private float damage = 100f;

    [SerializeField] private float yLaunchVelocity;
    [SerializeField] private float zLaunchVelocity;

    [SerializeField] private ParticleSystem explosionParticles;
    [SerializeField][Range(0, 500)] private int explosionParticlesCount = 200;

    [SerializeField]
    float despawnTimer;
    public void Shoot(IPower power, PlayerPowerManager powerManager, PowerToken pToken, Vector3 position, Vector3 forward)
    {
        // Move the projectile to the position parameter
        transform.position = position;

        // Set the forward of the game object to the forward parameter
        transform.forward = _forward = forward;

        _grenade = (Grenade)power;
        _powerManager = powerManager;
        _pToken = pToken;
        Destroy(gameObject, despawnTimer);

        GetComponent<Rigidbody>()
            .AddRelativeForce(new Vector3(0, yLaunchVelocity, zLaunchVelocity), ForceMode.VelocityChange);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
            actor.ChangeHealth(-damage, _powerManager.Player.PlayerInfo, _grenade, transform.position);

        // Destroy the projectile when it hits something
        // Debug.Log($"BOOM! {gameObject.name} hit {other.name}");

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
    }
}
