using UnityEngine;
using UnityEngine.Serialization;

public class FireRateIncrease : MonoBehaviour, IPower
{
    private const string FIRE_RATE_INCREASE_KEY = "FIRE_RATE_INCREASE_POWER";

    public GameObject GameObject => gameObject;
    public PowerScriptableObject PowerScriptableObject { get; set; }

    [SerializeField, Min(0)] private float multiplier = 2f;
    [SerializeField] private ParticleSystem fireRateIncreaseParticles;

    public string PassiveEffectDebugText(PlayerPowerManager powerManager, PowerToken pToken)
    {
        var timeRemaining = pToken.PowerScriptableObject.PassiveEffectDuration - pToken.CurrentPassiveDuration;

        return $"Gun Fire Rate Multiplier:\n" +
               $"\tRate {multiplier:0.00}x.\n" +
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
        var particles = Instantiate(fireRateIncreaseParticles, powerManager.transform);

        // Set the regeneration particles to follow the player
        particles.transform.localPosition = Vector3.zero;

        // Play the regeneration particles
        particles.Play();

        // Create a new multiplier token
        var token = powerManager
            .Player
            .WeaponManager
            .FireRateMultiplierTokens
            .AddToken(multiplier, -1, true);

        // Create a new data object to store the token and particles
        var data = new FireRateIncreaseData(token, particles);

        // Add the particles to the power token's data dictionary
        pToken.AddData(FIRE_RATE_INCREASE_KEY, data);
    }

    public void StartActiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
        CreateEffectToken(powerManager, pToken);
    }

    public void UpdateActiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
        var hasData = pToken.TryGetData<FireRateIncreaseData>(FIRE_RATE_INCREASE_KEY, out var data);

        // Ensure the player has the multiplier token
        if (!hasData && !powerManager.Player.WeaponManager.FireRateMultiplierTokens.HasToken(data.Token))
            CreateEffectToken(powerManager, pToken);
    }

    public void EndActiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
        // Get the particles from the power token's data dictionary
        // Remove the particles from the power token's data dictionary
        var hasData = pToken.RemoveData<FireRateIncreaseData>(FIRE_RATE_INCREASE_KEY, out var data);

        if (!hasData)
            return;

        // Stop the particle system
        data.Particles.Stop();

        // Destroy the particles' game object after the duration of the particle system
        Destroy(data.Particles.gameObject, data.Particles.main.duration);

        // Remove the multiplier token
        powerManager
            .Player
            .WeaponManager
            .FireRateMultiplierTokens
            .RemoveToken(data.Token);
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

    private class FireRateIncreaseData
    {
        public TokenManager<float>.ManagedToken Token { get; }
        public ParticleSystem Particles { get; }

        public FireRateIncreaseData(TokenManager<float>.ManagedToken token, ParticleSystem particles)
        {
            Token = token;
            Particles = particles;
        }
    }
}