using UnityEngine;
using UnityEngine.Serialization;

public class DamageIncrease : MonoBehaviour, IPower
{
    private const string DAMAGE_INCREASE_KEY = "DAMAGE_INCREASE_POWER";

    public GameObject GameObject => gameObject;
    public PowerScriptableObject PowerScriptableObject { get; set; }

    [SerializeField] [Min(0)] private float multiplier = 1.5f;
    [SerializeField] private ParticleSystem damageIncreaseParticles;

    public string PassiveEffectDebugText(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
        var timeRemaining = pToken.PowerScriptableObject.PassiveEffectDuration - pToken.CurrentPassiveDuration;

        return $"Gun Damage Multiplier:\n" +
               $"\tDamage {multiplier:0.00}x.\n" +
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
        var particles = Instantiate(damageIncreaseParticles, powerManager.transform);

        // Set the regeneration particles to follow the player
        particles.transform.localPosition = Vector3.zero;

        // Play the regeneration particles
        particles.Play();

        // Create a new damage multiplier token
        var token = powerManager
            .Player
            .WeaponManager
            .AddDamageMultiplier(multiplier, PowerScriptableObject.PassiveEffectDuration);

        // Create a new data object to store the token and particles
        var data = new DamageIncreaseData(token, particles);

        // Add the particles to the power token's data dictionary
        pToken.AddData(DAMAGE_INCREASE_KEY, data);
    }

    public void UpdatePassiveEffect(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void EndPassiveEffect(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
        // Get the particles from the power token's data dictionary
        // Remove the particles from the power token's data dictionary
        var data = pToken.RemoveData<DamageIncreaseData>(DAMAGE_INCREASE_KEY);

        // Stop the particle system
        data.Particles.Stop();

        // Destroy the particles' game object after the duration of the particle system
        Destroy(data.Particles.gameObject, data.Particles.main.duration);
        
        // Remove the damage multiplier token
        powerManager
            .Player
            .WeaponManager
            .RemoveDamageMultiplier(data.Token);
    }

    private class DamageIncreaseData
    {
        private readonly WeaponManager.DamageMultiplierToken _token;
        private readonly ParticleSystem _particles;

        public WeaponManager.DamageMultiplierToken Token => _token;
        public ParticleSystem Particles => _particles;

        public DamageIncreaseData(WeaponManager.DamageMultiplierToken token, ParticleSystem particles)
        {
            _token = token;
            _particles = particles;
        }
    }
}