using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour, IUsesInput, IDebugManaged
{
    #region Fields

    private TestPlayer _player;
    private Playerinfo _playerInfo;
    private IGun _equippedGun;

    /// <summary>
    /// A variable that stores the gun that the player is currently looking at.
    /// Used w/ the equip gun function.
    /// </summary>
    private IGun _currentLookedAtGun;

    [Tooltip("The position that the gun will fire from.")]
    [SerializeField] private Transform fireTransform;

    [SerializeField] private float gunInteractDistance;

    [SerializeField] private Transform gunHolder;

    /// <summary>
    /// A prefab that immediately equips the player with a gun.
    /// </summary>
    [SerializeField] private GameObject initialGunPrefab;

    #endregion

    #region Getters

    public TestPlayer Player => _player;
    public Playerinfo PlayerInfo => _playerInfo;
    public IGun EquippedGun => _equippedGun;
    
    public Transform FiringPoint => fireTransform;

    #endregion

    #region Initialization Functions

    private void Awake()
    {
        // Initialize the components
        GetComponents();
    }

    private void Start()
    {
        // Spawn the initial gun
        if (initialGunPrefab != null)
        {
            var gun = Instantiate(initialGunPrefab).GetComponent<IGun>();
            EquipGun(gun);
        }

        DebugManager.Instance.AddDebugManaged(this);
    }

    private void GetComponents()
    {
        // Get the TestPlayer component
        _player = GetComponent<TestPlayer>();

        // Get the Player info component
        _playerInfo = _player.PlayerInfo;
    }

    #endregion


    # region Input Functions

    public void InitializeInput()
    {
        // Initialize the input

        // Shoot input
        InputManager.Instance.PlayerControls.GamePlay.Shoot.performed += OnShoot;
        InputManager.Instance.PlayerControls.GamePlay.Shoot.canceled += OnShootCanceled;

        // Pick up weapon input
        InputManager.Instance.PlayerControls.GamePlay.Interact.performed += OnPickUpWeapon;
    }

    public void RemoveInput()
    {
        // Remove the input
        InputManager.Instance.PlayerControls.GamePlay.Shoot.performed -= OnShoot;
        InputManager.Instance.PlayerControls.GamePlay.Shoot.canceled -= OnShootCanceled;

        InputManager.Instance.PlayerControls.GamePlay.Interact.performed -= OnPickUpWeapon;
    }


    private void OnPickUpWeapon(InputAction.CallbackContext obj)
    {
        // Return if the player is not currently looking at a gun
        if (_currentLookedAtGun == null)
            return;

        // Equip the gun that the player is currently looking at
        EquipGun(_currentLookedAtGun);
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

    #endregion

    private void Update()
    {
        UpdateLookedAtGun();
    }

    private void UpdateLookedAtGun()
    {
        var cameraPivot = _player.PlayerController.CameraPivot.transform;

        // Is there a ray cast hit within the gun interact distance?
        var hit = Physics.Raycast(
            cameraPivot.position,
            cameraPivot.forward,
            out var hitInfo,
            gunInteractDistance
        );

        // Perform a raycast to see if the player is looking at a gun
        if (hit)
        {
            // If the player is looking at a gun, set the current looked at gun to the gun that the player is looking at
            if (hitInfo.collider.TryGetComponent(out IGun gun))
            {
                // Skip if the gun is the equipped gun
                if (_equippedGun == gun)
                    _currentLookedAtGun = null;

                else
                    _currentLookedAtGun = gun;
            }

            // If there is no gun, set the current looked at gun to null
            else
                _currentLookedAtGun = null;
        }
        // If the player is not looking at a gun, set the current looked at gun to null
        else
            _currentLookedAtGun = null;
    }

    public void EquipGun(IGun gun)
    {
        // Remove the current gun
        RemoveGun();

        // Set the current gun to the gun that was passed in
        _equippedGun = gun;

        // Set the gun's parent to the gun holder
        gun.GameObject.transform.SetParent(gunHolder);

        // Set the local position of the gun to the gun holder's local position
        gun.GameObject.transform.localPosition = Vector3.zero;

        // Set the local rotation of the gun to the gun holder's local rotation
        gun.GameObject.transform.localRotation = Quaternion.identity;

        // Make the weapon kinematic (if it has a rigidbody)
        if (gun.GameObject.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;

            // Also disable the collider
            gun.Collider.enabled = false;
        }
        
        // Call the OnEquip function
        gun.OnEquip(this);
    }

    public void RemoveGun()
    {
        // Set the current gun to null
        if (_equippedGun == null)
            return;

        // Set the equipped gun's parent to null
        _equippedGun.GameObject.transform.SetParent(null);

        // Make the weapon non-kinematic (if it has a rigidbody)
        if (_equippedGun.GameObject.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = false;

            // Also enable the collider
            _equippedGun.Collider.enabled = true;

            // Throw the gun
            ThrowRigidBody(rb);
        }
        
        // Call the OnRemoval function
        _equippedGun.OnRemoval(this);

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
        var torqueZ = UnityEngine.Random.Range(-throwForce / 10, throwForce / 10);

        // Add torque to the gun
        rb.AddTorque(new Vector3(torqueX, torqueY, 0) / 10, ForceMode.Impulse);
    }

    public string GetDebugText()
    {
        return
            $"Equipped Gun: {_equippedGun?.GunInformation.name}\n" +
            $"Looking at gun: {_currentLookedAtGun?.GunInformation.name} ({_currentLookedAtGun?.GameObject.name})\n";
    }
}