using UnityEngine;

/// <summary>
/// This class is mainly used as a hub to connect all the player components together.
/// </summary>
[RequireComponent(typeof(PlayerInfo))]
[RequireComponent(typeof(PlayerPowerManager))]
public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    private PlayerInfo _playerInfo;

    private IPlayerController _playerController;
    private PlayerLook _playerLook;

    private PlayerPowerManager _playerPowerManager;
    private WeaponManager _weaponManager;
    private PlayerInventory _playerInventory;

    #region Getters

    public PlayerInfo PlayerInfo => _playerInfo;

    public IPlayerController PlayerController => _playerController;

    public PlayerLook PlayerLook => _playerLook;

    public PlayerPowerManager PlayerPowerManager => _playerPowerManager;

    public WeaponManager WeaponManager => _weaponManager;

    public PlayerInventory PlayerInventory => _playerInventory;

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
        _playerInfo = GetComponent<PlayerInfo>();

        // Get the TestPlayerController component
        _playerController = GetComponent<IPlayerController>();

        // Get the PlayerLook component
        _playerLook = GetComponent<PlayerLook>();

        // Get the TestPlayerPowerManager component
        _playerPowerManager = GetComponent<PlayerPowerManager>();

        // Get the WeaponManager component
        _weaponManager = GetComponent<WeaponManager>();

        // Get the PlayerInventory component
        _playerInventory = GetComponent<PlayerInventory>();
    }

    #endregion

    public void ResetPlayer()
    {
        Debug.Log("Resetting Player");

        // Reset the health
        _playerInfo.ResetPlayer();

        // Reset the powers
        _playerPowerManager.ResetPlayer();

        // Reset the weapon
        _weaponManager.ResetPlayer();
    }
}