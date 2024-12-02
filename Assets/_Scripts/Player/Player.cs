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
}