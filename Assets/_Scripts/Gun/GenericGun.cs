using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

[RequireComponent(typeof(Rigidbody))]
public class GenericGun : MonoBehaviour, IGun, IDebugged
{
    #region Serialized Fields

    [SerializeField] private GunInformation gunInformation;
    [SerializeField] private Animator animator;

    [Header("Muzzle Particles")] [SerializeField]
    private Transform muzzleLocation;

    [SerializeField] private ParticleSystem muzzleParticles;
    [SerializeField] [Range(0, 500)] private int muzzleParticlesCount = 200;

    [SerializeField] private VisualEffect muzzleFlashPrefab;

    [Header("Impact Particles")] [SerializeField]
    private ParticleSystem impactParticles;

    [SerializeField] [Range(0, 500)] private int impactParticlesCount = 200;

    #endregion

    #region Private Fields

    /// <summary>
    /// A flag to determine if the fire input is being held down.
    /// </summary>
    private bool _isFiring;

    /// <summary>
    /// A flag to determine if the fire input was pressed this frame.
    /// Used to force the player to release the fire button before firing again w/ semi-automatic weapons.
    /// </summary>
    private bool _hasFiredThisFrame;

    /// <summary>
    /// The time since the gun last fired.
    /// Used to control fire rate;
    /// </summary>
    private float _fireDelta;

    /// <summary>
    /// How many bullets are remaining in the magazine.
    /// </summary>
    private int _currentMagazineSize;

    /// <summary>
    /// How long the player has spent reloading the gun.
    /// </summary>
    private float _currentReloadTime;

    private bool _isReloading;

    /// <summary>
    /// A reference to the weapon manager that is currently using this gun.
    /// </summary>
    private WeaponManager _weaponManager;

    #endregion

    #region Getters

    public GunInformation GunInformation => gunInformation;

    public Collider Collider { get; private set; }

    public GameObject GameObject => gameObject;

    private float TimeBetweenShots => 1 / gunInformation.FireRate;

    public bool IsMagazineEmpty => _currentMagazineSize <= 0;

    public bool IsReloading => _isReloading;

    public float ReloadingPercentage => (gunInformation.ReloadTime - _currentReloadTime) / gunInformation.ReloadTime;

    #region IInteractable

    public bool IsInteractable => true;

    #endregion

    #endregion

    private void Awake()
    {
        // Get the collider component
        Collider = GetComponent<Collider>();

        // Get the animator component from pistol4
        //animator = GetComponent<Animator>();
    }

    private void Start()
    {
        // Set the current magazine size to the max magazine size
        _currentMagazineSize = gunInformation.MagazineSize;

        // Set the fire delta to the time between shots
        _fireDelta = TimeBetweenShots;
    }

    #region Update Functions

    private void Update()
    {
        UpdateFireDelta();

        // Fire the weapon if applicable
        if (_weaponManager != null)
            Fire(_weaponManager, _weaponManager.FireTransform.position, _weaponManager.FireTransform.forward);

        // Reset the fired this frame flag
        _hasFiredThisFrame = false;

        // Update the reload time if the gun is currently reloading
        UpdateReload();
    }

    private void UpdateReload()
    {
        // Return if the gun is not currently reloading
        if (!IsReloading)
            return;

        // Update the reload time
        _currentReloadTime = Mathf.Clamp(_currentReloadTime - Time.deltaTime, 0, gunInformation.ReloadTime);

        if (_currentReloadTime > 0)
            return;

        // Set the reloading flag to false
        _isReloading = false;

        // If the player has finished reloading, reset the magazine size
        _currentMagazineSize = gunInformation.MagazineSize;
    }

    private void UpdateFireDelta()
    {
        // If the player is reloading, force the fire delta to the TimeBetweenShots
        if (IsReloading)
        {
            _fireDelta = TimeBetweenShots;
            return;
        }

        // if the gun is currently firing, Update the time since the gun last fired
        _fireDelta = Mathf.Clamp(_fireDelta + Time.deltaTime, 0, TimeBetweenShots);
    }

    #endregion

    public void OnFire(WeaponManager weaponManager)
    {
        // Return if the gun is currently reloading
        if (IsReloading)
            return;


        // Set the firing flag to true
        _isFiring = true;
        //
        // Set the fired this frame flag to true
        _hasFiredThisFrame = true;
    }

    public void OnFireReleased()
    {
        // Set the firing flag to false
        _isFiring = false;
    }

    public void Fire(WeaponManager weaponManager, Vector3 startingPosition, Vector3 direction)
    {
        // If the gun is not firing, return
        if (!_isFiring)
            return;

        // Return if the gun is currently reloading
        if (IsReloading)
            return;

        // return if the magazine is empty
        if (IsMagazineEmpty)
            return;

        // Special logic to determine if the gun should fire this frame based on the gun's fire type
        switch (gunInformation.FireType)
        {
            case GunInformation.GunFireType.SemiAutomatic:
                if (!_hasFiredThisFrame)
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

        // // Emit the fire particles
        // PlayParticles(muzzleParticles, muzzleLocation.position, muzzleParticlesCount);

        // Play the muzzle flash VFX
        PlayMuzzleFlash();

        // Fire the gun
        for (var i = 0; i < timesToFire; i++)
        {
            // break if the magazine is empty
            if (IsMagazineEmpty)
                break;

            // Determine the bloom of the gun
            var xBloom = UnityEngine.Random.Range(-gunInformation.BloomAngle / 2, gunInformation.BloomAngle / 2);
            var yBloom = UnityEngine.Random.Range(-gunInformation.BloomAngle / 2, gunInformation.BloomAngle / 2);
            var spreadDirection = Quaternion.Euler(xBloom, yBloom, 0) * direction;

            // Perform the raycast
            var hit = Physics.Raycast(startingPosition, spreadDirection, out var hitInfo, gunInformation.Range);

            // Decrease the magazine size
            _currentMagazineSize--;


            //Play shooting animation
            animator.SetTrigger("Shooting");

            // Play the fire sound
            var fireSound = gunInformation.FireSounds.GetRandomSound();
            SoundManager.Instance.PlaySfx(fireSound);

            // Continue if the raycast did not hit anything
            if (!hit)
                continue;

            // Emit the particles
            PlayParticles(impactParticles, hitInfo.point, impactParticlesCount);

            // Test if the cast hit an IActor (test the root object)
            if (hitInfo.collider.transform.root.TryGetComponent(out IActor actor))
            {
                // continue if the actor is the player
                if (actor is PlayerInfo info && info == weaponManager.PlayerInfo)
                    continue;

                // Calculate the damage falloff
                var distance = Vector3.Distance(startingPosition, hitInfo.point);
                var damage = gunInformation.EvaluateBaseDamage(distance) * weaponManager.CurrentDamageMultiplier;

                // Debug.Log($"DAMAGE: {damage} - DISTANCE: {distance} / {gunInformation.Range}");

                // Deal damage to the actor
                actor.ChangeHealth(-damage, weaponManager.Player.PlayerInfo, this);
            }
        }
    }

    public void Reload()
    {
        // Return if the player is reloading
        if (IsReloading)
            return;

        // Return if the gun's magazine is full
        if (_currentMagazineSize == gunInformation.MagazineSize)
            return;

        // Force the firing flag to false
        _isFiring = false;
        _hasFiredThisFrame = false;

        // Set the reload time to the reload time of the gun
        _currentReloadTime = gunInformation.ReloadTime;


        // Set the reloading flag to true
        _isReloading = true;

        // set animation param trigger to reload
        animator.SetTrigger("Reload");

        // Play the reload sound
        Debug.Log($"Sound Settings: {gunInformation.ReloadSound.Clip.name}");
        SoundManager.Instance.PlaySfx(gunInformation.ReloadSound);
    }

    public void OnEquip(WeaponManager weaponManager)
    {
        // Set the weapon manager
        _weaponManager = weaponManager;

        // Add to debug manager
        DebugManager.Instance.AddDebuggedObject(this);
    }

    public void OnRemoval(WeaponManager weaponManager)
    {
        // Remove from debug manager
        DebugManager.Instance.RemoveDebuggedObject(this);

        // Clear the weapon manager
        _weaponManager = null;
    }

    public string GetDebugText()
    {
        return $"Equipped Generic Gun: {gunInformation.GunName}\n" +
               $"\tFire Type: {gunInformation.FireType}\n" +
               $"\tDamage: {gunInformation.BaseDamage}\n" +
               $"\tFire Rate: {gunInformation.FireRate}\t ({TimeBetweenShots}) \tDelta: {_fireDelta}\n" +
               $"\tEquipped: {(_weaponManager != null ? _weaponManager.gameObject.name : "NONE")}\n" +
               $"\tFiring?: {_isFiring} - {_hasFiredThisFrame}\n" +
               $"\tMagazine: {_currentMagazineSize} / {gunInformation.MagazineSize}\n" +
               $"\tReloading: {_currentReloadTime} / {gunInformation.ReloadTime}" +
               $"\n";
    }

    private static void PlayParticles(ParticleSystem system, Vector3 position, int count)
    {
        if (system == null)
            return;

        // Create emit parameters that only override the position and velocity
        var emitParams = new ParticleSystem.EmitParams
        {
            position = position,
            applyShapeToPosition = true
        };

        // Emit the particles
        system.Emit(emitParams, count);
    }

    private void PlayMuzzleFlash()
    {
        // Instantiate the muzzle flash VFX
        var muzzleFlashInstance = Instantiate(muzzleFlashPrefab, muzzleLocation.position, muzzleLocation.rotation);

        // Get the max lifetime of the muzzle flash
        var maxLifetime = muzzleFlashInstance.GetFloat("MaxLifetime");

        // Destroy the muzzle flash after the max lifetime
        Destroy(muzzleFlashInstance.gameObject, maxLifetime);

        // Play the muzzle flash
        muzzleFlashInstance.Play();
    }

    public void Interact(PlayerInteraction playerInteraction)
    {
        // Equip the gun
        playerInteraction.Player.WeaponManager.EquipGun(this);
    }

    public void LookAtUpdate(PlayerInteraction playerInteraction)
    {
        Debug.Log($"Looking at {gunInformation.GunName}");
    }

    public string InteractText(PlayerInteraction playerInteraction)
    {
        return $"Pick up {gunInformation.GunName}";
    }
}