using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VirusProjectile : MonoBehaviour, IPowerProjectile
{
    #region Serialized Fields

    [SerializeField] private float damage = 100f;
    [SerializeField, Min(0)] private float explosionInfectionRadius = 10f;
    [SerializeField] private float tickDamage = 5f;
    [SerializeField] private float tickRate = 0.5f;
    [SerializeField] private float tickDuration = 5f;

    [SerializeField] private float yLaunchVelocity;
    [SerializeField] private float zLaunchVelocity;

    [SerializeField] private ParticleSystem explosionParticles;
    [SerializeField] [Range(0, 500)] private int explosionParticlesCount = 200;

    [SerializeField] private VisualEffect infectionVfxPrefab;

    [SerializeField] private float despawnTimer;

    #endregion

    #region Private Fields

    private Rigidbody _rigidbody;

    private Virus _virus;
    private PlayerPowerManager _powerManager;
    private PowerToken _pToken;

    private bool _isExploded;

    #endregion


    public void Shoot(IPower power, PlayerPowerManager powerManager, PowerToken pToken, Vector3 position,
        Vector3 forward)
    {
        // Move the projectile to the position parameter
        transform.position = position;

        // Set the forward of the game object to the forward parameter
        transform.forward = forward;

        _virus = (Virus)power;
        _powerManager = powerManager;
        _pToken = pToken;
        Destroy(gameObject, despawnTimer);

        var rb = GetComponent<Rigidbody>();
        rb.AddForce(transform.up * yLaunchVelocity, ForceMode.VelocityChange);
        rb.AddForce(transform.forward * zLaunchVelocity, ForceMode.VelocityChange);
    }

    private void InfectActor(IActor actor)
    {
        // If the actor is already infected, return
        if (Virus.IsActorInfected(actor))
            return;

        // Infect the actor
        Virus.AddInfectedActor(actor);

        // Start the virus ticks
        (actor as MonoBehaviour)!.StartCoroutine(VirusTicks(actor));
        
        // Add an event to the actor's death event to remove them from the infected actors list
        actor.OnDeath += RemoveFromInfectedActors;
    }

    private static void RemoveFromInfectedActors(object sender, HealthChangedEventArgs e)
    {
        // Remove the actor from the infected actors list
        Virus.RemoveInfectedActor(e.Actor);

        // Remove the event from the actor's death event
        e.Actor.OnDeath -= RemoveFromInfectedActors;
    }

    private IEnumerator VirusTicks(IActor actor)
    {
        // Instantiate a copy of the infection VFX prefab on the actor
        var infectionVfx = Instantiate(infectionVfxPrefab, actor.GameObject.transform);

        float totalDamage = 0;

        for (float elapsedTime = 0; elapsedTime < tickDuration; elapsedTime += tickRate)
        {
            // Actually do the damage
            actor.ChangeHealth(-tickDamage, _powerManager.Player.PlayerInfo, _virus,
                actor.GameObject.transform.position);

            totalDamage += tickDamage;

            // Do damage
            Debug.Log($"Did damage: {tickDamage} [{totalDamage}] ({elapsedTime} seconds have passed)");

            // Wait for tickRate
            yield return new WaitForSeconds(tickRate);
        }
        
        // Remove the actor from the infected actors list
        Virus.RemoveInfectedActor(actor);
        
        // Remove the infection VFX
        Destroy(infectionVfx.gameObject);
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
            actor.ChangeHealth(-damage, _powerManager.Player.PlayerInfo, _virus, transform.position);

        // Explode the projectile
        Explode();
    }

    private void Explode()
    {
        _isExploded = true;

        var enemiesInRange = GetAllEnemiesInRange();

        // Infect all enemies in range
        foreach (var enemy in enemiesInRange)
            InfectActor(enemy);

        // Create explosion particles
        CreateExplosionParticles();

        // Destroy the projectile
        Destroy(gameObject);
    }

    private List<EnemyInfo> GetAllEnemiesInRange()
    {
        // Create a new list to store the enemies in range
        var enemiesInRange = new List<EnemyInfo>();

        // Loop through all enemies in the scene
        foreach (var enemy in Enemy.Enemies)
        {
            // Continue to the next enemy if the enemy is null
            if (enemy == null)
                continue;

            // Calculate the distance between the enemy and the projectile
            var distance = Vector3.Distance(enemy.GameObject.transform.position, transform.position);

            // Continue if the enemy is not in range
            if (distance > explosionInfectionRadius)
                continue;

            // Add the enemy to the list of enemies in range
            enemiesInRange.Add(enemy.EnemyInfo);
        }

        return enemiesInRange;
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