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

    public string PassiveEffectDebugText(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
        var timeRemaining = pToken.PowerScriptableObject.PassiveEffectDuration - pToken.CurrentPassiveDuration;

        var regenerationPerSecond = regenerationAmount / PowerScriptableObject.PassiveEffectDuration;
        
        return $"Regeneration:\n" +
               $"\tRestoring {regenerationPerSecond} health per second.\n" +
               $"\t{timeRemaining:0.00} seconds remaining.\n";
    }

    public void StartCharge(TestPlayerPowerManager powerManager, PowerToken pToken, bool startedChargingThisFrame)
    {
    }

    public void Charge(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void Release(TestPlayerPowerManager powerManager, PowerToken pToken, bool isCharged)
    {
    }

    public void Use(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
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
        // Instantiate the regeneration particles
        var particles = Instantiate(regenerationParticles, powerManager.transform);

        // Set the regeneration particles to follow the player
        particles.transform.localPosition = Vector3.zero;

        // Play the regeneration particles
        particles.Play();

        // Add the particles to the power token's data dictionary
        pToken.AddData(REGENERATION_KEY, particles);
    }

    public void UpdatePassiveEffect(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
        powerManager.Player.PlayerInfo.ChangeHealth(
            regenerationAmount / PowerScriptableObject.PassiveEffectDuration * Time.deltaTime
        );
    }

    public void EndPassiveEffect(TestPlayerPowerManager powerManager, PowerToken pToken)
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