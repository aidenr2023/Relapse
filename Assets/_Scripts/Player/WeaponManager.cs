using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour, IUsesInput, IDebugged, IGunHolder
{
    #region Fields

    [Tooltip("The position that the gun will fire from.")] [SerializeField]
    private Transform fireTransform;

    [SerializeField] private Transform gunHolder;
    [SerializeField] private Animator animator;

    /// <summary>
    /// A prefab that immediately equips the player with a gun.
    /// </summary>
    [SerializeField] private GameObject initialGunPrefab;

    [SerializeField] private TMP_Text reloadText;

    #endregion

    #region Private Fields

    private Player _player;
    private PlayerInfo _playerInfo;
    private IGun _equippedGun;

    private readonly SortedSet<DamageMultiplierToken> _damageMultiplierTokens = new();

    private bool _spawnedInitialGun = false;

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
            float multiplier = 1;

            foreach (var token in _damageMultiplierTokens)
                multiplier *= token.Multiplier;

            return multiplier;
        }
    }

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

        // Reload
        InputActions.Add(
            new InputData(InputManager.Instance.PControls.Player.Reload, InputType.Performed, OnReload)
        );
    }

    private void OnShoot(InputAction.CallbackContext obj)
    {
        // If the current gun is null, return
        if (_equippedGun == null)
            return;

        // Fire the IGun
        EquippedGun.OnFire(this);
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
        // If the current gun is null, return
        if (_equippedGun == null)
            return;

        // Reload the gun
        EquippedGun.Reload();
    }

    #endregion

    private void Update()
    {
        // TODO: Remove this and replace it with a bar or something
        UpdateReloadText();

        // Update the damage multipliers
        UpdateDamageMultipliers();
    }

    private void UpdateReloadText()
    {
        reloadText.text = "";

        if (_equippedGun == null)
            return;

        if (_equippedGun.IsReloading)
            reloadText.text = $"Reloading: {_equippedGun.ReloadingPercentage * 100:0}%";

        // Tell the player to reload
        else if (_equippedGun.IsMagazineEmpty)
            reloadText.text = "You need to reload!";
    }

    public void EquipGun(IGun gun)
    {
        // Remove the current gun
        RemoveGun();

        // Set the current gun to the gun that was passed in
        _equippedGun = gun;

        // Set the gun's parent to the gun holder
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

        //get animator set isArmed to true
        animator.SetBool("isArmed", true);

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

    private void ThrowRigidBody(Rigidbody rb)
    {
        const float throwForce = 5;

        // Create a random force
        var forceX = UnityEngine.Random.Range(throwForce * .75f, throwForce);

        // Add a force to the gun
        rb.AddForce(transform.forward * forceX, ForceMode.Impulse);

        // Create random torque force
        var torqueX = UnityEngine.Random.Range(-throwForce, throwForce);
        var torqueY = UnityEngine.Random.Range(-throwForce, throwForce);

        // Add torque to the gun
        rb.AddTorque(new Vector3(torqueX, torqueY, 0) / 10, ForceMode.Impulse);
    }

    public string GetDebugText()
    {
        return $"Equipped Gun: {_equippedGun?.GunInformation.name}\n";
    }

    private void OnDrawGizmos()
    {
        if (fireTransform == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(fireTransform.position, fireTransform.forward * 10);
    }

    public DamageMultiplierToken AddDamageMultiplier(float multiplier, float duration, bool isPermanent = false)
    {
        // Create the new token
        var token = new DamageMultiplierToken(multiplier, duration, isPermanent);

        // Add the token to the list
        _damageMultiplierTokens.Add(token);

        // Return the token
        return token;
    }

    private void UpdateDamageMultipliers()
    {
        // Create a list of tokens to remove
        var tokensToRemove = new List<DamageMultiplierToken>();

        // Loop through the tokens
        foreach (var token in _damageMultiplierTokens)
        {
            // If the token is permanent, continue
            if (token.IsPermanent)
                continue;

            // Update the duration of the token
            token.UpdateDuration(-Time.deltaTime);

            // If the duration is less than or equal to 0, add it to the list of tokens to remove
            if (token.Duration <= 0)
                tokensToRemove.Add(token);
        }

        // Remove the tokens
        foreach (var token in tokensToRemove)
            _damageMultiplierTokens.Remove(token);
    }

    public void RemoveDamageMultiplier(DamageMultiplierToken token)
    {
        // Remove the token from the list
        _damageMultiplierTokens.Remove(token);
    }

    public void ResetPlayer()
    {
        // Clear the damage multipliers
        _damageMultiplierTokens.Clear();
    }

    public void SetUpWeapon(GameObject newGun, int currentAmmo)
    {
        var spawnNewGun = false;

        // Remove the current gun (completely erase it, don't even drop it)
        if (_equippedGun != null && _equippedGun.GameObject != newGun)
        {
            // Set the equipped gun's parent to null
            _equippedGun.GameObject.transform.SetParent(null, true);

            // Destroy the equipped gun
            Destroy(_equippedGun.GameObject);

            // Set the equipped gun to null
            _equippedGun = null;

            // Set the spawn new gun flag to true
            spawnNewGun = true;
        }

        IGun gun;

        if (spawnNewGun)
        {
            // Equip the new gun
            gun = Instantiate(newGun).GetComponent<IGun>();

            // Get the interactable materials of the gun
            gun.GetOutlineMaterials(Player.PlayerInteraction.OutlineMaterial.shader);

            // Equip the gun
            EquipGun(gun);
        }

        gun = _equippedGun;

        if (gun == null)
            return;

        // Set the ammo count
        gun.CurrentAmmo = currentAmmo;
    }

    public class DamageMultiplierToken : IComparable<DamageMultiplierToken>
    {
        private readonly float _multiplier;
        private float _duration;
        private readonly bool _isPermanent;

        public float Multiplier => _multiplier;
        public float Duration => _duration;
        public bool IsPermanent => _isPermanent;

        public DamageMultiplierToken(float multiplier, float duration, bool isPermanent = false)
        {
            _multiplier = multiplier;
            _duration = duration;
            _isPermanent = isPermanent;
        }

        public void UpdateDuration(float changeAmount)
        {
            _duration += changeAmount;
        }

        public int CompareTo(DamageMultiplierToken other)
        {
            if (ReferenceEquals(this, other))
                return 0;

            if (other is null)
                return 1;

            var multiplierComparison = _multiplier.CompareTo(other._multiplier);

            // Return the comparison if the multipliers are not equal
            if (multiplierComparison != 0)
                return multiplierComparison;

            return _duration.CompareTo(other._duration);
        }
    }
}