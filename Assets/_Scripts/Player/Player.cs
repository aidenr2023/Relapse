using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// This class is mainly used as a hub to connect all the player components together.
/// </summary>
[RequireComponent(typeof(PlayerInfo))]
[RequireComponent(typeof(PlayerPowerManager))]
public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    #region Getters

    public PlayerInfo PlayerInfo { get; private set; }

    public IPlayerController PlayerController { get; private set; }

    public PlayerLook PlayerLook { get; private set; }

    public PlayerInteraction PlayerInteraction { get; private set; }

    public PlayerPowerManager PlayerPowerManager { get; private set; }

    public WeaponManager WeaponManager { get; private set; }

    public PlayerInventory PlayerInventory { get; private set; }

    public PlayerVirtualCameraController PlayerVirtualCameraController { get; private set; }

    public PlayerDeathController PlayerDeathController { get; private set; }

    #endregion

    #region Initialization Functions

    private void Awake()
    {
        // Set the instance to this
        Instance = this;

        // Initialize the components
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // Get the Player Info component
        PlayerInfo = GetComponent<PlayerInfo>();

        // Get the TestPlayerController component
        PlayerController = GetComponent<IPlayerController>();

        // Get the PlayerLook component
        PlayerLook = GetComponent<PlayerLook>();

        // Get the PlayerInteraction component
        PlayerInteraction = GetComponent<PlayerInteraction>();

        // Get the TestPlayerPowerManager component
        PlayerPowerManager = GetComponent<PlayerPowerManager>();

        // Get the WeaponManager component
        WeaponManager = GetComponent<WeaponManager>();

        // Get the PlayerInventory component
        PlayerInventory = GetComponent<PlayerInventory>();

        // Get the PlayerVirtualCameraController component
        PlayerVirtualCameraController = GetComponent<PlayerVirtualCameraController>();

        // Get the PlayerDeathController component
        PlayerDeathController = GetComponent<PlayerDeathController>();
    }

    #endregion

    public void ResetPlayer()
    {
        Debug.Log("Resetting Player");

        // Reset the health
        PlayerInfo.ResetPlayer();

        // Reset the powers
        PlayerPowerManager.ResetPlayer();

        // Reset the weapon
        WeaponManager.ResetPlayer();
    }

    public PlayerSerializationData SerializePlayer()
    {
        var movementV2 = PlayerController as PlayerMovementV2;

        if (movementV2 == null)
        {
            Debug.LogError("PlayerController is not of type PlayerMovementV2");
            return default;
        }

        // Create a new PlayerSerializationData object
        var playerSerializationData = new PlayerSerializationData
        {
            position = transform.position,
            rotation = PlayerController.Orientation.rotation,
            velocity = movementV2.Rigidbody.velocity,

            currentHealth = PlayerInfo.CurrentHealth,
            maxHealth = PlayerInfo.MaxHealth,

            currentToxicity = PlayerInfo.CurrentTolerance,
            maxToxicity = PlayerInfo.MaxTolerance,
            relapseCount = PlayerInfo.RelapseCount,
            isRelapsing = PlayerInfo.IsRelapsing,

            currentPowerIndex = PlayerPowerManager.CurrentPowerIndex,
            equippedPowers = PlayerPowerManager.Powers.ToArray(),

            equippedGun = WeaponManager.EquippedGun.GameObject,
            currentAmmo = WeaponManager.EquippedGun.CurrentAmmo,

            inventoryEntries = PlayerInventory.InventoryEntries.ToArray(),

            currentStamina = movementV2.CurrentStamina,
            maxStamina = movementV2.MaxStamina,
            canSprint = movementV2.BasicPlayerMovement.CanSprint,
            canJump = movementV2.BasicPlayerMovement.CanJump,
            canWallRun = movementV2.WallRunning.CanWallRun,
            canSlide = movementV2.PlayerSlide.CanSlide
        };

        return playerSerializationData;
    }

    public void ApplyPlayerData(PlayerSerializationData data)
    {
        var movementV2 = PlayerController as PlayerMovementV2;

        if (movementV2 == null)
            Debug.LogError("PlayerController is not of type PlayerMovementV2");

        // Set the position and rotation
        movementV2.Rigidbody.position = data.position;
        PlayerController.Orientation.rotation = data.rotation;
        movementV2.Rigidbody.velocity = data.velocity;

        // Set the health & toxicity
        PlayerInfo.SetUpHealth(data.currentHealth, data.maxHealth);
        PlayerInfo.SetUpToxicity(data.currentToxicity, data.maxToxicity, data.relapseCount, data.isRelapsing);

        // Set the powers
        PlayerPowerManager.SetUpPowers(data.currentPowerIndex, data.equippedPowers);

        // Set the weapon
        WeaponManager.SetUpWeapon(data.equippedGun, data.currentAmmo);

        // Set up the inventory
        PlayerInventory.SetUpInventory(data.inventoryEntries);

        // Set the movement values
        movementV2.SetUpStamina(data.currentStamina, data.maxStamina);
        movementV2.BasicPlayerMovement.CanSprint = data.canSprint;
        movementV2.BasicPlayerMovement.CanJump = data.canJump;
        movementV2.WallRunning.CanWallRun = data.canWallRun;
        movementV2.PlayerSlide.CanSlide = data.canSlide;
    }

    [SerializeField] private PlayerSerializationData playerData;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            playerData = SerializePlayer();
            Debug.Log("Player data saved!");
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            ApplyPlayerData(playerData);
            Debug.Log("Player data loaded!");
        }
    }
}