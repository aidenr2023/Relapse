using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// This class is mainly used as a hub to connect all the player components together.
/// </summary>
[RequireComponent(typeof(PlayerInfo))]
[RequireComponent(typeof(PlayerPowerManager))]
public class Player : MonoBehaviour, IPlayerLoaderInfo
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

    public PlayerTutorialManager PlayerTutorialManager { get; private set; }

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

        // Get the PlayerTutorialManager component
        PlayerTutorialManager = GetComponent<PlayerTutorialManager>();
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

    #region Saving and Loading

    public GameObject GameObject => gameObject;
    public string Id => "Player";

    private const string MAX_HEALTH_KEY = "_maxHealth";
    private const string CURRENT_HEALTH_KEY = "_currentHealth";
    private const string MAX_TOXICITY_KEY = "_maxToxicity";
    private const string CURRENT_TOXICITY_KEY = "_currentToxicity";
    private const string RELAPSE_COUNT_KEY = "_relapseCount";
    private const string MAX_STAMINA_KEY = "_maxStamina";
    private const string CURRENT_STAMINA_KEY = "_currentStamina";

    public void LoadData(PlayerLoader playerLoader, bool restore)
    {
        var hasMaxHealth = playerLoader.TryGetDataFromMemory(Id, MAX_HEALTH_KEY, out float maxHealth);
        var hasCurrentHealth = playerLoader.TryGetDataFromMemory(Id, CURRENT_HEALTH_KEY, out float currentHealth);

        // Load the current & max Health
        if (hasMaxHealth && hasCurrentHealth && !restore)
            PlayerInfo.SetUpHealth(currentHealth, maxHealth);
        else if (hasMaxHealth && hasCurrentHealth)
            PlayerInfo.SetUpHealth(maxHealth, maxHealth);

        var hasMaxToxicity = playerLoader.TryGetDataFromMemory(Id, MAX_TOXICITY_KEY, out float maxToxicity);
        var hasCurrentToxicity = playerLoader.TryGetDataFromMemory(Id, CURRENT_TOXICITY_KEY, out float currentToxicity);
        var hasRelapseCount = playerLoader.TryGetDataFromMemory(Id, RELAPSE_COUNT_KEY, out int relapseCount);

        // Load the current & max Toxicity
        if (hasMaxToxicity && hasCurrentToxicity && hasRelapseCount)
            PlayerInfo.SetUpToxicity(currentToxicity, maxToxicity, relapseCount, false);

        var pmv2 = PlayerController as PlayerMovementV2;

        if (pmv2 == null)
        {
            Debug.LogError("PlayerController is not of type PlayerMovementV2! Could not load stamina data.");
            return;
        }

        var hasMaxStamina = playerLoader.TryGetDataFromMemory(Id, MAX_STAMINA_KEY, out float maxStamina);
        var hasCurrentStamina = playerLoader.TryGetDataFromMemory(Id, CURRENT_STAMINA_KEY, out float currentStamina);

        // Load the current & max Stamina
        if (hasMaxStamina && hasCurrentStamina)
            pmv2.SetUpStamina(currentStamina, maxStamina);
    }

    public void SaveData(PlayerLoader playerLoader)
    {
        // Create number data for the max health
        // Save the data
        var maxHealthData = new DataInfo(MAX_HEALTH_KEY, PlayerInfo.MaxHealth);
        playerLoader.AddDataToMemory(Id, maxHealthData);

        // Create number data for the current health
        // Save the data
        var currentHealthData = new DataInfo(CURRENT_HEALTH_KEY, PlayerInfo.CurrentHealth);
        playerLoader.AddDataToMemory(Id, currentHealthData);

        // Create number data for the max toxicity
        // Save the data
        var maxToxicityData = new DataInfo(MAX_TOXICITY_KEY, PlayerInfo.MaxTolerance);
        playerLoader.AddDataToMemory(Id, maxToxicityData);

        // Create number data for the current toxicity
        // Save the data
        var currentToxicityData = new DataInfo(CURRENT_TOXICITY_KEY, PlayerInfo.CurrentTolerance);
        playerLoader.AddDataToMemory(Id, currentToxicityData);

        // Create number data for the relapse count
        // Save the data
        var relapseCountData = new DataInfo(RELAPSE_COUNT_KEY, PlayerInfo.RelapseCount);
        playerLoader.AddDataToMemory(Id, relapseCountData);

        var pmv2 = PlayerController as PlayerMovementV2;

        if (pmv2 == null)
        {
            Debug.LogError("PlayerController is not of type PlayerMovementV2! Could not save stamina data.");
            return;
        }

        // Create number data for the max stamina
        // Save the data
        var maxStaminaData = new DataInfo(MAX_STAMINA_KEY, pmv2.MaxStamina);
        playerLoader.AddDataToMemory(Id, maxStaminaData);

        // Create number data for the current stamina
        // Save the data
        var currentStaminaData = new DataInfo(CURRENT_STAMINA_KEY, pmv2.CurrentStamina);
        playerLoader.AddDataToMemory(Id, currentStaminaData);
    }

    #endregion
}