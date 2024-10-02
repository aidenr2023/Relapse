using UnityEngine;

/// <summary>
/// This class is mainly used as a hub to connect all the player components together.
/// </summary>
[RequireComponent(typeof(PlayerInfo))]
[RequireComponent(typeof(TestPlayerPowerManager))]
public class TestPlayer : MonoBehaviour
{
    private PlayerInfo _playerInfo;
    private IPlayerController _playerController;
    private TestPlayerPowerManager _playerPowerManager;
    private WeaponManager _weaponManager;

    #region Getters

    public PlayerInfo PlayerInfo => _playerInfo;

    public IPlayerController PlayerController => _playerController;

    public TestPlayerPowerManager PlayerPowerManager => _playerPowerManager;

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

        // Get the TestPlayerPowerManager component
        _playerPowerManager = GetComponent<TestPlayerPowerManager>();

        // Get the WeaponManager component
        _weaponManager = GetComponent<WeaponManager>();
    }

    #endregion
}