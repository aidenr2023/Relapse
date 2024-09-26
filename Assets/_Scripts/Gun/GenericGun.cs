using System;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class GenericGun : MonoBehaviour, IGun, IDebugManaged
{
    [SerializeField] private GunInformation gunInformation;

    [Header("Muzzle Particles")] [SerializeField]
    private Transform muzzleLocation;

    [SerializeField] private ParticleSystem muzzleParticles;
    [SerializeField] [Range(0, 500)] private int muzzleParticlesCount = 200;

    [Header("Impact Particles")] [SerializeField]
    private ParticleSystem impactParticles;

    [SerializeField] [Range(0, 500)] private int impactParticlesCount = 200;


    /// <summary>
    /// A flag to determine if the fire input is being held down.
    /// </summary>
    private bool _isFiring;

    /// <summary>
    /// A flag to determine if the fire input was pressed this frame.
    /// Used to force the player to release the fire button before firing again w/ semi-automatic weapons.
    /// </summary>
    private bool _firedThisFrame;

    /// <summary>
    /// The time since the gun last fired.
    /// Used to control fire rate;
    /// </summary>
    private float _fireDelta;

    /// <summary>
    /// A reference to the weapon manager that is currently using this gun.
    /// </summary>
    private WeaponManager _weaponManager;

    #region Getters

    public GunInformation GunInformation => gunInformation;

    public Collider Collider { get; private set; }

    public GameObject GameObject => gameObject;

    private float TimeBetweenShots => 1 / gunInformation.FireRate;

    #endregion

    private void Awake()
    {
        // Get the collider component
        Collider = GetComponent<Collider>();
    }

    public void OnFire(WeaponManager weaponManager)
    {
        // Set the firing flag to true
        _isFiring = true;

        // Set the fired this frame flag to true
        _firedThisFrame = true;
    }

    public void OnFireReleased()
    {
        // Set the firing flag to false
        _isFiring = false;
    }

    private void Update()
    {
        // if the gun is currently firing, Update the time since the gun last fired
        _fireDelta = Mathf.Clamp(_fireDelta + Time.deltaTime, 0, TimeBetweenShots);

        if (_weaponManager != null)
        {
            // Fire the weapon if applicable
            Fire(_weaponManager, _weaponManager.FiringPoint.position, _weaponManager.FiringPoint.forward);
        }

        // Reset the fired this frame flag
        _firedThisFrame = false;
    }

    public void Fire(WeaponManager weaponManager, Vector3 startingPosition, Vector3 direction)
    {
        // If the gun is not firing, return
        if (!_isFiring)
            return;

        // Special logic to determine if the gun should fire this frame based on the gun's fire type
        switch (gunInformation.FireType)
        {
            case GunInformation.GunFireType.SemiAutomatic:
                if (!_firedThisFrame)
                    return;
                break;
        }

        // Determine if the gun can fire based on the fire rate
        if (_fireDelta < TimeBetweenShots)
            return;

        // Determine how many times the gun should fire this frame
        var timesToFire = (int)(_fireDelta / TimeBetweenShots);
        _fireDelta %= TimeBetweenShots;

        // Create a ray cast from the starting point in the direction with the specified range

        // Emit the fire particles
        PlayParticles(muzzleParticles, muzzleLocation.position, direction, 10, muzzleParticlesCount);

        // Fire the gun
        for (var i = 0; i < timesToFire; i++)
        {
            // Determine the bloom of the gun
            var xBloom = UnityEngine.Random.Range(-gunInformation.BloomAngle, gunInformation.BloomAngle);
            var yBloom = UnityEngine.Random.Range(-gunInformation.BloomAngle, gunInformation.BloomAngle);
            var spreadDirection = Quaternion.Euler(xBloom, yBloom, 0) * direction;

            // Perform the raycast
            var hit = Physics.Raycast(startingPosition, spreadDirection, out var hitInfo, gunInformation.Range);
            
            Debug.Log($"SHOT!: {hit}");
            
            // Continue if the raycast did not hit anything
            if (!hit)
                continue;

            // Emit the particles
            PlayParticles(impactParticles, hitInfo.point, hitInfo.normal, 2, impactParticlesCount);
        }
    }
    
    private static void PlayParticles(ParticleSystem system, Vector3 position, Vector3 direction, float velocity, int count)
    {
        if (system == null)
            return;

        // Create emit parameters that only override the position and velocity
        var emitParams = new ParticleSystem.EmitParams
        {
            position = position,
            applyShapeToPosition = true,
        };
        
        // Emit the particles
        system.Emit(emitParams, count);
    }

    public void OnEquip(WeaponManager weaponManager)
    {
        // Set the weapon manager
        _weaponManager = weaponManager;

        // Add to debug manager
        DebugManager.Instance.AddDebugManaged(this);
    }

    public void OnRemoval(WeaponManager weaponManager)
    {
        // Remove from debug manager
        DebugManager.Instance.RemoveDebugManaged(this);

        // Clear the weapon manager
        _weaponManager = null;
    }

    public string GetDebugText()
    {
        return $"Equipped Generic Gun: {gunInformation.GunName}\n" +
               $"\tFire Type: {gunInformation.FireType}\n" +
               $"\tDamage: {gunInformation.BaseDamage}\n" +
               $"\tFire Rate: {gunInformation.FireRate}\t ({TimeBetweenShots}) \tDelta: {_fireDelta}\n" +
               $"\tEquipped: {((_weaponManager != null) ? _weaponManager.gameObject.name : "NONE")}\n" +
               $"\tFiring?: {_isFiring} - {_firedThisFrame}" +
               $"\n";
    }
}