using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using UnityEngine.VFX;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class GenericGun : MonoBehaviour, IGun, IDebugged
{
    private static readonly int ShootingAnimationID = Animator.StringToHash("Shooting");
    private static readonly int ReloadAnimationID = Animator.StringToHash("Reload");

    #region Serialized Fields

    [SerializeField] protected GunInformation gunInformation;
    [SerializeField] protected Animator animator;

    [Header("Muzzle Particles")] [SerializeField]
    protected Transform muzzleLocation;

    [SerializeField] protected VisualEffect muzzleFlash;

    [Header("Impact Particles")] [SerializeField]
    protected ParticleSystem impactParticles;

    [SerializeField] [Range(0, 500)] protected int impactParticlesCount = 200;

    [SerializeField] protected LayerMask layersToIgnore;

    [SerializeField] protected TrailRenderer bulletTrailRenderer;
    [SerializeField] protected DecalProjector bulletHoleDecal;

    #endregion

    #region Private Fields

    /// <summary>
    /// A flag to determine if the fire input is being held down.
    /// </summary>
    protected bool isFiring;

    /// <summary>
    /// A flag to determine if the fire input was pressed this frame.
    /// Used to force the player to release the fire button before firing again w/ semi-automatic weapons.
    /// </summary>
    protected bool hasFiredThisFrame;

    /// <summary>
    /// The time since the gun last fired.
    /// Used to control fire rate;
    /// </summary>
    protected float fireDelta;

    /// <summary>
    /// How many bullets are remaining in the magazine.
    /// </summary>
    protected int currentMagazineSize;

    /// <summary>
    /// How long the player has spent reloading the gun.
    /// </summary>
    protected float currentReloadTime;

    protected bool isReloading;

    /// <summary>
    /// A reference to the weapon manager that is currently using this gun.
    /// </summary>
    private WeaponManager _weaponManager;

    #endregion

    #region Getters

    public GunInformation GunInformation => gunInformation;

    public Collider Collider { get; private set; }

    public GameObject GameObject => gameObject;

    protected float TimeBetweenShots => 1 / gunInformation.FireRate;

    public bool IsMagazineEmpty => currentMagazineSize <= 0;

    public bool IsReloading => isReloading;

    public float ReloadingPercentage => (gunInformation.ReloadTime - currentReloadTime) / gunInformation.ReloadTime;

    public bool HasOutline { get; set; }

    public HashSet<Material> OutlineMaterials { get; } = new();

    public InteractionIcon InteractionIcon => InteractionIcon.Pickup;

    public int CurrentAmmo
    {
        get => currentMagazineSize;
        set => currentMagazineSize = value;
    }

    #region IInteractable

    public bool IsInteractable => true;

    #endregion

    #endregion

    protected virtual void Awake()
    {
        // Get the collider component
        Collider = GetComponent<Collider>();

        // Get the animator component from pistol
        //animator = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        // Set the current magazine size to the max magazine size
        currentMagazineSize = gunInformation.MagazineSize;

        // Set the fire delta to the time between shots
        fireDelta = TimeBetweenShots;
    }

    #region Update Functions

    protected virtual void Update()
    {
        UpdateFireDelta();

        // Fire the weapon if applicable
        if (_weaponManager != null)
            Fire(_weaponManager, _weaponManager.FireTransform.position, _weaponManager.FireTransform.forward);

        // Reset the fired this frame flag
        hasFiredThisFrame = false;

        // Update the reload time if the gun is currently reloading
        UpdateReload();

        // Update the outline
        UpdateOutline(_weaponManager);
    }

    private void UpdateReload()
    {
        // Return if the gun is not currently reloading
        if (!IsReloading)
            return;

        // Update the reload time
        currentReloadTime = Mathf.Clamp(currentReloadTime - Time.deltaTime, 0, gunInformation.ReloadTime);

        if (currentReloadTime > 0)
            return;

        // Set the reloading flag to false
        isReloading = false;

        // If the player has finished reloading, reset the magazine size
        currentMagazineSize = gunInformation.MagazineSize;
    }

    private void UpdateFireDelta()
    {
        // If the player is reloading, force the fire delta to the TimeBetweenShots
        if (IsReloading)
        {
            fireDelta = TimeBetweenShots;
            return;
        }

        // if the gun is currently firing, Update the time since the gun last fired
        fireDelta = Mathf.Clamp(fireDelta + Time.deltaTime, 0, TimeBetweenShots);
    }

    #endregion

    public virtual void OnFire(WeaponManager weaponManager)
    {
        // Return if the gun is currently reloading
        if (IsReloading)
            return;


        // Set the firing flag to true
        isFiring = true;
        //
        // Set the fired this frame flag to true
        hasFiredThisFrame = true;
    }

    public virtual void OnFireReleased()
    {
        // Set the firing flag to false
        isFiring = false;
    }

    public virtual void Fire(WeaponManager weaponManager, Vector3 startingPosition, Vector3 direction)
    {
        // If the gun should not fire, return
        if (!ShouldFire())
            return;

        // Determine how many times the gun should fire this frame
        var timesToFire = (int)(fireDelta / TimeBetweenShots);
        fireDelta %= TimeBetweenShots;

        // Fire the gun
        ShootProjectiles(weaponManager, timesToFire, startingPosition, direction);
    }

    protected virtual bool ShouldFire()
    {
        // If the gun is not firing, return
        if (!isFiring)
            return false;

        // Return if the gun is currently reloading
        if (IsReloading)
            return false;

        // return if the magazine is empty
        if (IsMagazineEmpty)
            return false;

        // Special logic to determine if the gun should fire this frame based on the gun's fire type
        switch (gunInformation.FireType)
        {
            case GunInformation.GunFireType.SemiAutomatic:
                if (!hasFiredThisFrame)
                    return false;
                break;
        }

        // Determine if the gun can fire based on the fire rate
        if (fireDelta < TimeBetweenShots)
            return false;

        return true;
    }

    protected void ShootProjectiles(
        WeaponManager weaponManager, int pelletsToFire, Vector3 startingPosition, Vector3 direction
    )
    {
        // Play shooting animation
        animator.SetTrigger(ShootingAnimationID);

        // Play the muzzle flash VFX
        PlayMuzzleFlash();

        // Recoil the gun
        Recoil();

        for (var i = 0; i < pelletsToFire; i++)
        {
            // break if the magazine is empty
            if (IsMagazineEmpty)
                break;

            // Determine the bloom of the gun
            var xBloom = UnityEngine.Random.Range(
                -gunInformation.BloomAngle / 2 * 100,
                gunInformation.BloomAngle / 2 * 100
            ) / 100;
            var yBloom = UnityEngine.Random.Range(
                -gunInformation.BloomAngle / 2 * 100,
                gunInformation.BloomAngle / 2 * 100
            ) / 100;
            // var spreadDirection = Quaternion.Euler(yBloom, xBloom, 0) * direction;
            var spreadDirection =
                Quaternion.AngleAxis(xBloom, weaponManager.Player.PlayerController.CameraPivot.transform.up) *
                Quaternion.AngleAxis(yBloom, weaponManager.Player.PlayerController.CameraPivot.transform.right) *
                direction;

            // Create a layerMask to ignore the NonPhysical layer
            var layerMask = ~layersToIgnore;

            // Perform the raycast
            var hit = Physics.Raycast(
                startingPosition,
                spreadDirection,
                out var hitInfo,
                gunInformation.Range,
                layerMask
            );

            // Decrease the magazine size
            currentMagazineSize--;

            // Spawn a bullet trail
            StartCoroutine(
                SpawnBulletTrail(bulletTrailRenderer, new Ray(muzzleLocation.position, spreadDirection), hitInfo)
            );

            // Continue if the raycast did not hit anything
            if (!hit)
                continue;

            // // Emit the particles
            // PlayParticles(impactParticles, hitInfo.point, impactParticlesCount);

            // Test if the cast hit an IActor (test the root object)
            if (!hitInfo.collider.TryGetComponentInParent(out IActor actor, 20))
                continue;

            // continue if the actor is the player
            if (actor is PlayerInfo info && info == weaponManager.PlayerInfo)
                continue;

            // Calculate the damage falloff
            var distance = Vector3.Distance(startingPosition, hitInfo.point);
            var damage = gunInformation.EvaluateBaseDamage(distance) * weaponManager.CurrentDamageMultiplier;

            // Debug.Log($"DAMAGE: {damage} - DISTANCE: {distance} / {gunInformation.Range}");

            // Deal damage to the actor
            actor.ChangeHealth(-damage, weaponManager.Player.PlayerInfo, this, hitInfo.point);
        }

        // Play the fire sound
        var fireSound = gunInformation.FireSounds.GetRandomSound();
        SoundManager.Instance.PlaySfx(fireSound);
    }

    private void Recoil()
    {
        var playerRecoil = _weaponManager.Player.GetComponent<PlayerRecoil>();

        if (playerRecoil == null)
            return;

        playerRecoil.AddRecoil(GunInformation);
    }

    private IEnumerator SpawnBulletTrail(TrailRenderer trailPrefab, Ray ray, RaycastHit hit)
    {
        var startPosition = ray.origin;

        // Determine the end position of the bullet trail
        var endPosition = hit.collider != null
            ? hit.point
            : ray.GetPoint(gunInformation.Range);

        // Instantiate the prefab
        var trail = Instantiate(trailPrefab, startPosition, Quaternion.identity);

        const float trailSpeed = 250f;
        const float variance = 0.25f;

        var cSpeed = Random.Range(
            trailSpeed - (variance * trailSpeed),
            trailSpeed + (variance * trailSpeed)
        );

        while (Vector3.Distance(trail.transform.position, endPosition) > 0.1f)
        {
            trail.transform.position = Vector3.MoveTowards(
                trail.transform.position,
                endPosition,
                Time.deltaTime * cSpeed
            );

            yield return null;
        }

        // Destroy the trail
        trail.autodestruct = true;

        // Instantiate the bullet hole decal
        if (hit.collider != null)
        {
            var decal = Instantiate(bulletHoleDecal, hit.point, Quaternion.LookRotation(hit.normal));

            // Set the parent of the decal to the object that was hit
            decal.transform.SetParent(hit.collider.transform);

            // Fix decal clipping
            decal.transform.position += hit.normal * -0.01f;

            // Set the decal to destroy itself after 10 seconds
            Destroy(decal.gameObject, 10);
        }
    }

    public virtual void Reload()
    {
        // Return if the player is reloading
        if (IsReloading)
            return;

        // Return if the gun's magazine is full
        if (currentMagazineSize == gunInformation.MagazineSize)
            return;

        // Force the firing flag to false
        isFiring = false;
        hasFiredThisFrame = false;

        // Set the reload time to the reload time of the gun
        currentReloadTime = gunInformation.ReloadTime;


        // Set the reloading flag to true
        isReloading = true;

        // set animation param trigger to reload
        animator.SetTrigger(ReloadAnimationID);

        // Play the reload sound
        // Debug.Log($"Sound Settings: {gunInformation.ReloadSound.Clip.name}");
        SoundManager.Instance.PlaySfx(gunInformation.ReloadSound);
    }

    public void OnEquipToPlayer(WeaponManager weaponManager)
    {
        // Set the weapon manager
        this._weaponManager = weaponManager;

        // Add to debug manager
        DebugManager.Instance.AddDebuggedObject(this);

        // Get the interactable materials of the gun
        this.GetOutlineMaterials(weaponManager.Player.PlayerInteraction.OutlineMaterial.shader);
    }

    public void OnRemovalFromPlayer(WeaponManager weaponManager)
    {
        // Remove from debug manager
        DebugManager.Instance.RemoveDebuggedObject(this);

        // Clear the weapon manager
        this._weaponManager = null;

        // Add the outline to the weapon manager
        foreach (var mat in OutlineMaterials)
            weaponManager.Player.PlayerInteraction.SetOutlineMaterial(mat, false, true);
    }

    protected static void PlayParticles(ParticleSystem system, Vector3 position, int count)
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

    protected void PlayMuzzleFlash()
    {
        // Return if the muzzle flash is null
        if (muzzleFlash == null)
            return;

        // // Instantiate the muzzle flash VFX
        // var muzzleFlashInstance = Instantiate(muzzleFlash, muzzleLocation.position, muzzleLocation.rotation);

        // Set the layer of the muzzle flash to the same layer as the gun
        muzzleFlash.gameObject.layer = gameObject.layer;

        // Set the layer of all the children of the muzzle flash to the same layer as the gun
        foreach (Transform child in muzzleFlash.transform)
            child.gameObject.layer = gameObject.layer;

        // // Get the max lifetime of the muzzle flash
        // var maxLifetime = 5;

        // // Destroy the muzzle flash after the max lifetime
        // Destroy(muzzleFlash.gameObject, maxLifetime);

        // Disable and re-enable the muzzle flash to reset the particles
        muzzleFlash.gameObject.SetActive(false);
        muzzleFlash.gameObject.SetActive(true);

        // Play the muzzle flash
        muzzleFlash.Play();

        // // Set the particles to be a child of the gun
        // muzzleFlash.transform.SetParent(muzzleLocation);

        // Debug.Log($"MUZZLE FLASH: {muzzleFlash.culled}, {muzzleFlash.gameObject.activeInHierarchy}, {muzzleFlash.playRate}, {muzzleFlash.HasAnySystemAwake()}");;
    }

    public void Interact(PlayerInteraction playerInteraction)
    {
        // Equip the gun
        playerInteraction.Player.WeaponManager.EquipGun(this);
    }

    public void LookAtUpdate(PlayerInteraction playerInteraction)
    {
    }

    public string InteractText(PlayerInteraction playerInteraction)
    {
        return $"Pick up {gunInformation.GunName}";
    }

    public void UpdateOutline(WeaponManager weaponManager)
    {
        if (_weaponManager == null)
            return;

        // Remove the outline from the previous weapon manager
        foreach (var mat in OutlineMaterials)
            _weaponManager.Player.PlayerInteraction.SetOutlineMaterial(mat, false, false);
    }

    public string GetDebugText()
    {
        return $"Equipped Generic Gun: {gunInformation.GunName}\n" +
               $"\tFire Type: {gunInformation.FireType}\n" +
               $"\tDamage: {gunInformation.BaseDamage}\n" +
               $"\tFire Rate: {gunInformation.FireRate}\t ({TimeBetweenShots}) \tDelta: {fireDelta}\n" +
               $"\tEquipped: {(_weaponManager != null ? _weaponManager.gameObject.name : "NONE")}\n" +
               $"\tFiring?: {isFiring} - {hasFiredThisFrame}\n" +
               $"\tMagazine: {currentMagazineSize} / {gunInformation.MagazineSize}\n" +
               $"\tReloading: {currentReloadTime} / {gunInformation.ReloadTime}" +
               $"\n";
    }
}