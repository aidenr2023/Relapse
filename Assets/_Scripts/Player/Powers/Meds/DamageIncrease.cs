﻿using UnityEngine;
using UnityEngine.Serialization;

public class DamageIncrease : MonoBehaviour, IPower
{
    private const string DAMAGE_INCREASE_KEY = "DAMAGE_INCREASE_POWER";

    [SerializeField] [Min(0)] private float multiplier = 1.5f;
    [SerializeField] private ParticleSystem damageIncreaseParticles;

    public GameObject GameObject => gameObject;
    public PowerScriptableObject PowerScriptableObject { get; set; }


    public Sound NormalHitSfx => PowerScriptableObject.NormalHitSfx;
    public Sound CriticalHitSfx => PowerScriptableObject.CriticalHitSfx;


    public string PassiveEffectDebugText(PlayerPowerManager powerManager, PowerToken pToken)
    {
        var timeRemaining = pToken.PowerScriptableObject.PassiveEffectDuration - pToken.CurrentPassiveDuration;

        return $"Gun Damage Multiplier:\n" +
               $"\tDamage {multiplier:0.00}x.\n" +
               $"\t{timeRemaining:0.00} seconds remaining.\n";
    }

    public void StartCharge(PlayerPowerManager powerManager, PowerToken pToken, bool startedChargingThisFrame)
    {
    }

    public void Charge(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void Release(PlayerPowerManager powerManager, PowerToken pToken, bool isCharged)
    {
    }

    public void Use(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    private void CreateEffectToken(PlayerPowerManager powerManager, PowerToken pToken)
    {
        // Instantiate the regeneration particles
        var particles = Instantiate(damageIncreaseParticles);

        // Add a new follow transform component to the particles
        var followTransform = particles.gameObject.AddComponent<FollowTransform>();
        followTransform.SetTargetTransform(powerManager.transform);

        // Set the regeneration particles to follow the player
        particles.transform.localPosition = Vector3.zero;

        // Play the regeneration particles
        particles.Play();

        // Create a new damage multiplier token
        var token = powerManager
            .Player
            .WeaponManager
            .DamageMultiplierTokens
            .AddToken(multiplier, -1, true);

        // Create a new data object to store the token and particles
        var data = new DamageIncreaseData(token, particles);

        // Add the particles to the power token's data dictionary
        pToken.AddData(DAMAGE_INCREASE_KEY, data);
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
        CreateEffectToken(powerManager, pToken);
    }

    public void UpdatePassiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
        var hasData = pToken.TryGetData<DamageIncreaseData>(DAMAGE_INCREASE_KEY, out var data);

        // Ensure the player has the damage multiplier token
        if (!hasData || !powerManager.Player.WeaponManager.DamageMultiplierTokens.HasToken(data.Token))
            CreateEffectToken(powerManager, pToken);
    }

    public void EndPassiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
        // Get the particles from the power token's data dictionary
        // Remove the particles from the power token's data dictionary
        var hasData = pToken.RemoveData<DamageIncreaseData>(DAMAGE_INCREASE_KEY, out var data);

        if (!hasData)
            return;

        if (data.Particles != null)
        {
            // Stop the particle system
            data.Particles.Stop();

            // Destroy the particles' game object after the duration of the particle system
            Destroy(data.Particles.gameObject, data.Particles.main.duration);
        }

        // Remove the damage multiplier token
        powerManager
            .Player
            .WeaponManager
            .DamageMultiplierTokens
            .RemoveToken(data.Token);
    }

    private class DamageIncreaseData
    {
        public TokenManager<float>.ManagedToken Token { get; }
        public ParticleSystem Particles { get; }

        public DamageIncreaseData(TokenManager<float>.ManagedToken token, ParticleSystem particles)
        {
            Token = token;
            Particles = particles;
        }
    }
}