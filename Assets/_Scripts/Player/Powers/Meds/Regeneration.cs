using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Regeneration : MonoBehaviour, IPower
{
    private const string REGENERATION_KEY = "RegenerationPower";

    public GameObject GameObject => gameObject;
    public PowerScriptableObject PowerScriptableObject { get; set; }

    [SerializeField] private float regenerationAmount = 2f;
    [SerializeField] private ParticleSystem regenerationParticles;

    public string PassiveEffectDebugText(PlayerPowerManager powerManager, PowerToken pToken)
    {
        var timeRemaining = pToken.PowerScriptableObject.PassiveEffectDuration - pToken.CurrentPassiveDuration;

        var regenerationPerSecond = regenerationAmount / PowerScriptableObject.PassiveEffectDuration;

        return $"Regeneration:\n" +
               $"\tRestoring {regenerationPerSecond} health per second.\n" +
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
        // Instantiate the regeneration particles
        var particles = Instantiate(regenerationParticles, powerManager.transform);

        // Set the regeneration particles to follow the player
        particles.transform.localPosition = Vector3.zero;

        // Play the regeneration particles
        particles.Play();

        // Add the particles to the power token's data dictionary
        pToken.AddData(REGENERATION_KEY, particles);
    }

    public void UpdatePassiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
        // Return if the player's health is less than or equal to 0
        if (powerManager.Player.PlayerInfo.CurrentHealth <= 0)
            return;

        powerManager.Player.PlayerInfo.ChangeHealth(
            regenerationAmount / PowerScriptableObject.PassiveEffectDuration * Time.deltaTime,
            powerManager.Player.PlayerInfo,
            this,
            powerManager.Player.transform.position
        );
    }

    public void EndPassiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
        // Get the particles from the power token's data dictionary
        // Remove the particles from the power token's data dictionary
        var particles = pToken.RemoveData<ParticleSystem>(REGENERATION_KEY);

        // Stop the particle system
        particles.Stop();

        // Destroy the particles' game object after the duration of the particle system
        Destroy(particles.gameObject, particles.main.duration);
    }
}