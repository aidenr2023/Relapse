using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Util.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour, IUsesInput, IDebugged, IGunHolder, IPlayerLoaderInfo
{
    private static readonly int ShootAnimationID = Animator.StringToHash("Shoot");

    #region Serialized Fields

    public EventVariable OnGameReset => Player.OnGameReset;

    [SerializeField] private GunInfoListVariable allGuns;

    [Tooltip("The position that the gun will fire from.")] [SerializeField]
    private Transform fireTransform;

    [SerializeField] private Transform gunHolder;
    [SerializeField] private Transform ShotgunHolder;
    [SerializeField] private Animator _shootingAnimator;


    /// <summary>
    /// A prefab that immediately equips the player with a gun.
    /// </summary>
    [SerializeField] private GameObject initialGunPrefab;

    #endregion

    #region Private Fields

    private Player _player;
    private PlayerInfo _playerInfo;
    private IGun _equippedGun;

    // private readonly SortedSet<DamageMultiplierToken> _damageMultiplierTokens = new();
    private TokenManager<float> _damageMultiplierTokens;

    private TokenManager<float> _fireRateMultiplierTokens;

    private bool _spawnedInitialGun = false;
    
    private Coroutine _shootCoroutine;

    #endregion

    #region Getters

    public HashSet<InputData> InputActions { get; } = new();

    public Player Player => _player;
    public PlayerInfo PlayerInfo => _playerInfo;
    public IGun EquippedGun => _equippedGun;

    public Transform FireTransform => fireTransform;

    public float CurrentDamageMultiplier
    {
        get
        {
            var multiplier = 1f;

            foreach (var token in _damageMultiplierTokens.Tokens)
                multiplier *= token.Value;

            return multiplier;
        }
    }

    public float CurrentFireRateMultiplier
    {
        get
        {
            var multiplier = 1f;

            foreach (var token in _fireRateMultiplierTokens.Tokens)
                multiplier *= token.Value;

            return multiplier;
        }
    }

    public TokenManager<float> DamageMultiplierTokens => _damageMultiplierTokens;

    public TokenManager<float> FireRateMultiplierTokens => _fireRateMultiplierTokens;

    public Transform GunHolder => gunHolder;

    #endregion

    public Action<WeaponManager, IGun> OnGunEquipped { get; set; }
    public Action<WeaponManager, IGun> OnGunRemoved { get; set; }

    #region Initialization Functions

    private void Awake()
    {
        // Initialize the components
        GetComponents();

        // Initialize the input actions
        InitializeInput();

        // Initialize the damage multiplier tokens
        _damageMultiplierTokens = new TokenManager<float>(
            true,
            (token1, token2) => token1.Value.CompareTo(token2.Value),
            1
        );

        // Initialize the fire rate multiplier tokens
        _fireRateMultiplierTokens = new TokenManager<float>(
            true,
            (token1, token2) => token1.Value.CompareTo(token2.Value),
            1
        );
    }

    private void Start()
    {
        DebugManager.Instance.AddDebuggedObject(this);

        // Spawn the initial gun
        if (initialGunPrefab != null && !_spawnedInitialGun)
        {
            var gun = Instantiate(initialGunPrefab).GetComponent<IGun>();

            // // Get the interactable materials of the gun
            // gun.GetOutlineMaterials(Player.PlayerInteraction.OutlineMaterial.shader);

            EquipGun(gun);
            _spawnedInitialGun = true;
        }
    }

    private void OnEnable()
    {
        // Register the input actions
        InputManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        // Unregister the input actions
        InputManager.Instance.Unregister(this);
    }

    private void OnDestroy()
    {
        // Remove this from the debug manager
        DebugManager.Instance.RemoveDebuggedObject(this);
    }

    private void GetComponents()
    {
        // Get the TestPlayer component
        _player = GetComponent<Player>();


        // Get the Player info component
        _playerInfo = _player.PlayerInfo;
    }

    #endregion

    # region Input Functions

    public void InitializeInput()
    {
        // Shoot input
        InputActions.Add(
            new InputData(InputManager.Instance.PControls.Player.Attack, InputType.Performed, OnShoot)
        );
        InputActions.Add(
            new InputData(InputManager.Instance.PControls.Player.Attack, InputType.Canceled, OnShootCanceled)
        );
        InputActions.Add(
            new InputData(InputManager.Instance.PControls.Player.Attack, InputType.Performed, ReloadOnShoot)
        );

        // Reload
        InputActions.Add(
            new InputData(InputManager.Instance.PControls.Player.Reload, InputType.Performed, OnReload)
        );
    }

    private void ReloadOnShoot(InputAction.CallbackContext obj)
    {
        // Return if the current gun is null
        if (_equippedGun == null)
            return;

        // Return if the current gun doesn't need to reload
        if (_equippedGun.CurrentAmmo > 0)
            return;

        // Reload the gun
        EquippedGun.Reload();
    }

    private void OnShoot(InputAction.CallbackContext obj)
    {
        // if the shoot coroutine is not null, return
        if (_shootCoroutine != null)
            return;
        
        _shootCoroutine = StartCoroutine(Shoot());
    }

    private void OnShootCanceled(InputAction.CallbackContext obj)
    {
        // If the current gun is null, return
        if (_equippedGun == null)
            return;

        // Fire the IGun
        EquippedGun.OnFireReleased();
    }

    private void OnReload(InputAction.CallbackContext obj)
    {
        Reload();
    }

    #endregion

    private void Update()
    {
        // Update the damage multipliers
        UpdateDamageMultipliers();

        // Update the fire rate multipliers
        UpdateFireRateMultipliers();
    }

    #region General Gun Functions

    public void EquipGun(IGun gun)
    {
        // If the current gun's reload animation is playing, return
        if (_equippedGun != null && _equippedGun.IsReloadAnimationPlaying)
            return;
        
        // Remove the current gun
        RemoveGun();

        // Set the current gun to the gun that was passed in
        _equippedGun = gun;

        // Set the gun's parent to the gun holder or shotgun holder
        if (gun.GunModelType == GunModelType.Mag7)
            gun.GameObject.transform.SetParent(ShotgunHolder, true);
        else
            gun.GameObject.transform.SetParent(gunHolder, true);

        // Set the local position of the gun to the gun holder's local position
        gun.GameObject.transform.localPosition = Vector3.zero;

        // Set the local rotation of the gun to the gun holder's local rotation
        gun.GameObject.transform.localRotation = Quaternion.identity;

        // Make the weapon kinematic (if it has a rigidbody)
        if (gun.GameObject.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;

            // // Also disable the collider
            // if (gun.Collider != null)
            //     gun.Collider.enabled = false;

            // Disable collision on the gun
            rb.detectCollisions = false;
        }

        // Call the OnEquip function
        gun.OnEquipToPlayer(this);

        // Play the equip sound
        SoundManager.Instance.PlaySfx(gun.GunInformation.PickupSound);

        // // TODO: DELETE THIS EVENTUALLY
        // // Get all the renderers in the gun
        // var renderers = gun.GameObject.GetComponentsInChildren<Renderer>();
        //
        // // Deactivate the renderers
        // foreach (var cRenderer in renderers)
        // {
        //     // Skip the cRenderer if the object has a particle system
        //     if (cRenderer.TryGetComponent(out ParticleSystem ps))
        //         continue;
        //
        //     cRenderer.enabled = false;
        // }

        // Get the gun's game object and child objects
        var gunTransforms = gun.GameObject.GetComponentsInChildren<Transform>().ToList();

        // Add the gun's game object to the child objects
        gunTransforms.Add(gun.GameObject.transform);

        // Set all the renderers to the hand holder layer
        foreach (var cTransform in gunTransforms)
            cTransform.gameObject.layer = LayerMask.NameToLayer("GunHandHolder");

        // Invoke the OnGunEquipped event
        OnGunEquipped?.Invoke(this, gun);
    }

    public void RemoveGun()
    {
        // Set the current gun to null
        if (_equippedGun == null)
            return;
        
        // If the current gun's reload animation is playing, return
        if (_equippedGun.IsReloadAnimationPlaying)
            return;

        // Release the fire button on the gun
        EquippedGun.OnFireReleased();

        // Set the equipped gun's parent to null
        _equippedGun.GameObject.transform.SetParent(null, true);

        // Make the weapon non-kinematic (if it has a rigidbody)
        if (_equippedGun.GameObject.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = false;

            // // Also enable the collider
            // _equippedGun.Collider.enabled = true;

            // Also, enable the collisions
            rb.detectCollisions = true;

            // Throw the gun
            ThrowRigidBody(rb);
        }

        // Call the OnRemoval function
        _equippedGun.OnRemovalFromPlayer(this);

        // Get the gun's game object and child objects
        var gunTransforms = _equippedGun.GameObject.GetComponentsInChildren<Transform>().ToList();

        // Add the gun's game object to the child objects
        gunTransforms.Add(_equippedGun.GameObject.transform);

        // Set all the renderers to the hand holder layer
        foreach (var cTransform in gunTransforms)
            cTransform.gameObject.layer = LayerMask.NameToLayer("Gun");

        // Invoke the OnGunRemoved event
        OnGunRemoved?.Invoke(this, _equippedGun);

        _equippedGun = null;
    }

    private IEnumerator Shoot()
    {
        // If the current gun is null, return
        if (_equippedGun == null)
            yield break;

        // Get the player movement component
        var playerMovement = Player.PlayerController as PlayerMovementV2;

        // If the player is currently sprinting, force them to stop and return
        if (playerMovement != null && playerMovement.IsSprinting)
        {
            playerMovement.ForceStopSprinting();
            
            // Wait until the sprint animation has stopped playing
            yield return new WaitUntil(() => !playerMovement.IsSprintAnimationPlaying);
        }

        // Fire the IGun
        EquippedGun.OnFire(this);

        //Play the shoot animation
        _shootingAnimator.SetTrigger(ShootAnimationID);
        
        // Set the shoot coroutine to null
        _shootCoroutine = null;
    }

    private void Reload()
    {
        // If the current gun is null, return
        if (_equippedGun == null)
            return;

        // // Get the player movement component
        // var playerMovement = Player.PlayerController as PlayerMovementV2;
        //
        // // If the player is currently sprinting, force them to stop
        // if (playerMovement != null && playerMovement.IsSprinting)
        //     playerMovement.ForceStopSprinting();

        // Reload the gun
        EquippedGun.Reload();
    }

    private void ThrowRigidBody(Rigidbody rb)
    {
        const float throwForce = 5;

        // Create a random force
        var forceX = UnityEngine.Random.Range(throwForce * .75f, throwForce);

        // Add a force to the gun
        rb.AddForce(Player.PlayerController.Orientation.forward * forceX, ForceMode.Impulse);

        // Create random torque force
        var torqueX = UnityEngine.Random.Range(-throwForce, throwForce);
        var torqueY = UnityEngine.Random.Range(-throwForce, throwForce);

        // Add torque to the gun
        rb.AddTorque(new Vector3(torqueX, torqueY, 0) / 10, ForceMode.Impulse);
    }

    #endregion

    private void UpdateDamageMultipliers()
    {
        _damageMultiplierTokens.Update(Time.deltaTime);
    }

    private void UpdateFireRateMultipliers()
    {
        _fireRateMultiplierTokens.Update(Time.deltaTime);
    }

    public void ResetPlayer()
    {
        // Completely discard the equipped weapon
        SetUpWeapon(null, 0);

        // Clear the damage multipliers
        _damageMultiplierTokens.Clear();
    }

    public void SetUpWeapon(GenericGun newGunPrefab, int currentAmmo)
    {
        // Remove the current gun (completely erase it, don't even drop it)
        if (_equippedGun != null)
        {
            // Set the equipped gun's parent to null
            _equippedGun.GameObject.transform.SetParent(null, true);

            // Destroy the equipped gun
            Destroy(_equippedGun.GameObject);

            // Set the equipped gun to null
            _equippedGun = null;
        }

        if (newGunPrefab == null)
            return;

        // Instantiate the new gun
        var gun = Instantiate(newGunPrefab).GetComponent<IGun>();

        // Equip the gun
        EquipGun(gun);

        gun = _equippedGun;

        if (gun == null)
            return;

        // Set the ammo count
        gun.CurrentAmmo = currentAmmo;
    }

    #region Debugging

    public string GetDebugText()
    {
        return
            $"Equipped Gun: {_equippedGun?.GunInformation.name}\n" +
            $"Damage Multiplier: {CurrentDamageMultiplier:0.00}x\n" +
            $"Fire Rate Multiplier: {CurrentFireRateMultiplier:0.00}x\n";
    }

    private void OnDrawGizmos()
    {
        if (fireTransform == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(fireTransform.position, fireTransform.forward * 10);
    }

    #endregion

    #region Saving and Loading

    public GameObject GameObject => gameObject;
    public string Id => "PlayerWeaponManager";

    private const string CURRENT_GUN_KEY = "_currentGun";
    private const string CURRENT_AMMO_KEY = "_currentAmmo";

    public void LoadData(PlayerLoader playerLoader, bool restore)
    {
        var hasGun = playerLoader.TryGetDataFromMemory(Id, CURRENT_GUN_KEY, out string gunId);
        var hasCurrentAmmo = playerLoader.TryGetDataFromMemory(Id, CURRENT_AMMO_KEY, out int currentAmmo);

        // Find the first gun with the unique id
        var gun = allGuns.value.FirstOrDefault(n => n.UniqueId == gunId);

        // If there is no gun, set the gun to null
        if (gun == null)
        {
            SetUpWeapon(null, 0);
            return;
        }

        // Set up the weapon's ammo
        var newGunAmmo = gun.GunPrefab.GunInformation.MagazineSize;

        if (hasCurrentAmmo)
            newGunAmmo = currentAmmo;

        SetUpWeapon(gun.GunPrefab, newGunAmmo);
    }

    public void SaveData(PlayerLoader playerLoader)
    {
        // Create data for the current gun
        // Save the data
        var gunId = _equippedGun?.GunInformation.UniqueId ?? "";
        var gunIdData = new DataInfo(CURRENT_GUN_KEY, gunId);
        playerLoader.AddDataToMemory(Id, gunIdData);

        // Create number data for the current ammo
        // Save the data
        var gunAmmoData = new DataInfo(CURRENT_AMMO_KEY, _equippedGun?.CurrentAmmo ?? 0);
        playerLoader.AddDataToMemory(Id, gunAmmoData);

        // Debug.Log($"Saved weapon data: [{gunId}, {_equippedGun?.CurrentAmmo}]");
    }

    #endregion
}