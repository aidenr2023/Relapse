using System;
using System.Collections.Generic;
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

    public string PassiveEffectDebugText(PlayerPowerManager powerManager, PowerToken pToken) => string.Empty;

    #region IPower Methods

    public void StartCharge(PlayerPowerManager powerManager, PowerToken pToken, bool startedChargingThisFrame)
    {
        // Start playing fuse sound
    }

    public void Charge(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void Release(PlayerPowerManager powerManager, PowerToken pToken, bool isCharged)
    {
        // TODO: Play a defuse sound
    }

    public void Use(PlayerPowerManager powerManager, PowerToken pToken)
    {
        // Create a new list to avoid concurrent modification
        var enemies = new List<Enemy>(Enemy.Enemies);
        
        // Add enemies to the list if they are within the explosion radius
        foreach (var enemy in enemies)
        {
            // Continue if the enemy is null
            if (enemy == null)
                continue;

            // Get the distance between the player and the enemy
            var distance = Vector3.Distance(powerManager.Player.transform.position, enemy.transform.position);

            // Skip the enemy if it is out of range
            if (distance > explosionRadius)
                continue;
            
            // Deal damage to the enemy
            enemy.EnemyInfo.ChangeHealth(-explosionDamage, powerManager.Player.PlayerInfo, this, enemy.transform.position);
            
            // Add an explosion force to the enemy
            if (enemy.TryGetComponent(out Rigidbody rb))
                rb.AddExplosionForce(explosionForce, powerManager.Player.transform.position, explosionRadius);
        }

        // Create the explosion particles
        CreateExplosionParticles(powerManager, pToken);
    }

    public void StartActiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void UpdateActiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void EndActiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void StartPassiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void UpdatePassiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void EndPassiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    private void CreateExplosionParticles(PlayerPowerManager powerManager, PowerToken pToken)
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

    #endregion
}