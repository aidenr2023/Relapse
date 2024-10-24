using UnityEngine;

/// <summary>
/// This class is mainly used as a hub to connect all the player components together.
/// </summary>
[RequireComponent(typeof(PlayerInfo))]
[RequireComponent(typeof(PlayerPowerManager))]
public class Player : MonoBehaviour
{
    private PlayerInfo _playerInfo;

    private IPlayerController _playerController;
    private PlayerLook _playerLook;

    private PlayerPowerManager _playerPowerManager;
    private WeaponManager _weaponManager;

    #region Getters

    public PlayerInfo PlayerInfo => _playerInfo;

    public IPlayerController PlayerController => _playerController;

    public PlayerLook PlayerLook => _playerLook;

    public PlayerPowerManager PlayerPowerManager => _playerPowerManager;

    public WeaponManager WeaponManager => _weaponManager;

    #endregion

    #region Initialization Functions

    private void Awake()
    {
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
    }

    #endregion
}