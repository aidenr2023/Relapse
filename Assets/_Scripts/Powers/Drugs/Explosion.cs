using UnityEngine;

public class Explosion : MonoBehaviour, IPower
{
    private const string EXPLOSION_KEY = "Explosion Power Key";

    [SerializeField] private ParticleSystem explosionParticles;
    [SerializeField] [Min(0)] private int explosionParticlesCount = 200;

    [Header("Settings")] [SerializeField] [Min(0)]
    private float explosionDamage = 10f;

    [SerializeField] [Min(0)] private float explosionRadius = 5f;
    [SerializeField] [Min(0)] private float explosionForce = 1000f;


    public GameObject GameObject => gameObject;
    public PowerScriptableObject PowerScriptableObject { get; set; }

    public void StartCharge(TestPlayerPowerManager powerManager, PowerToken pToken, bool startedChargingThisFrame)
    {
        // Start playing fuse sound
    }

    public void Charge(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void Release(TestPlayerPowerManager powerManager, PowerToken pToken, bool isCharged)
    {
        // TODO: Play a defuse sound
    }

    public void Use(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
        // Get the caster's collider
        var casterCollider = powerManager.Player.GetComponent<Collider>();

        // Get the explosion position
        var explosionPosition = powerManager.Player.PlayerController.CameraPivot.transform.position;

        // Create a sphere cast that checks for all colliders within the explosion radius
        var colliders = Physics.OverlapSphere(explosionPosition, explosionRadius);

        // Loop through all colliders
        foreach (var cCollider in colliders)
        {
            // Skip the caster's collider
            if (cCollider == casterCollider)
                continue;

            // Add an explosion force to the collider
            if (cCollider.TryGetComponent(out Rigidbody rb))
                rb.AddExplosionForce(explosionForce, explosionPosition, explosionRadius);

            // Get the Actor component of the collider
            var actor = cCollider.GetComponent<IActor>();

            // If the collider has a health component
            // Deal damage to the health component
            // TODO: Increase damage & make it scale with distance
            if (actor != null)
                actor.ChangeHealth(-explosionDamage);
        }

        // Create the explosion particles
        CreateExplosionParticles(powerManager, pToken);
    }

    public void StartActiveEffect(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void UpdateActiveEffect(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void EndActiveEffect(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void StartPassiveEffect(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void UpdatePassiveEffect(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void EndPassiveEffect(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    private void CreateExplosionParticles(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
        // Instantiate the explosion particles
        var particles = Instantiate(explosionParticles, powerManager.Player.transform);

        // Set the explosion particles to follow the player
        particles.transform.localPosition = Vector3.zero;

        // Create the emission parameters
        var parameters = new ParticleSystem.EmitParams
        {
            position = powerManager.Player.transform.position,
            applyShapeToPosition = true,
        };

        // Play the explosion particles
        particles.Emit(parameters, explosionParticlesCount);

        // Destroy the particles after the duration of the particles
        Destroy(particles.gameObject, particles.main.duration);
    }
}